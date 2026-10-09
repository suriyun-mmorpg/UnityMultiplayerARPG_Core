using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameNetworkManager
    {
        private readonly Dictionary<long, string> patchPeerVersions = new Dictionary<long, string>();
        private readonly HashSet<long> patchPreparedPeers = new HashSet<long>();
        private readonly HashSet<long> patchQueuedPeers = new HashSet<long>();
        private bool patchClientReady;
        private bool patchReloading;
        private string patchReloadNonce = "";
        private string patchRequiredId = "";
        private string patchRequiredHash = "";
        private string patchClientNonce = "";
        private string patchClientPreparedId = "";
        private bool PatchProtocolEnabled => GameInstance.Singleton.dataPatchProfile?.dataPatchSettings.enabled == true;

        private void RegisterPatchMessages()
        {
            RegisterClientMessage(GameNetworkingConsts.PatchStateMessage, message =>
            {
                byte mode = message.Reader.GetByte();
                string nonce = message.Reader.GetString();
                string id = message.Reader.GetString();
                string hash = message.Reader.GetString();
                string databaseId = message.Reader.GetString();
                string environment = message.Reader.GetString();
                ReceivePatchState(mode, nonce, id, hash, databaseId, environment).Forget();
            });
            RegisterServerMessage(GameNetworkingConsts.PatchAckMessage, message =>
            {
                byte mode = message.Reader.GetByte();
                string nonce = message.Reader.GetString();
                string id = message.Reader.GetString();
                string hash = message.Reader.GetString();
                if (mode == 0 && patchReloading && nonce == patchReloadNonce && id == patchRequiredId && hash == patchRequiredHash)
                    patchPreparedPeers.Add(message.ConnectionId);
                if (mode == 1 && id == DataPatchRuntime.ActiveReleaseId && hash == DataPatchRuntime.ActiveHash)
                    patchPeerVersions[message.ConnectionId] = id + ":" + hash;
            });
        }

        private void SendPatchState(long connectionId, byte mode, string nonce, string id, string hash)
        {
            DataPatchProfile database = GameInstance.Singleton.dataPatchProfile;
            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.PatchStateMessage, writer =>
            {
                writer.Put(mode);
                writer.Put(nonce);
                writer.Put(id);
                writer.Put(hash);
                writer.Put(database?.patchDatabaseId ?? "");
                writer.Put(database?.dataPatchSettings.environment ?? "");
            });
        }

        private async UniTaskVoid ReceivePatchState(byte mode, string nonce, string id, string hash, string databaseId, string environment)
        {
            try
            {
                if (mode == 3)
                {
                    if (nonce == patchClientNonce && !IsServer)
                        DataPatchRuntime.Engine?.Discard();
                    return;
                }

                if (mode == 1)
                {
                    if (patchClientNonce != nonce || patchClientPreparedId != id)
                        throw new InvalidOperationException("Patch commit was not prepared.");
                    if (!IsServer)
                        DataPatchRuntime.Commit();
                    patchClientReady = true;
                    SendPatchAck(1, nonce, id, hash);
                    return;
                }

                if (mode != 0 && mode != 2)
                    throw new InvalidOperationException("Unknown patch protocol mode.");
                if (mode == 2)
                    patchClientReady = false;
                patchClientNonce = nonce;
                if (!IsServer)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (DataPatchRuntime.Profile == null || databaseId != DataPatchRuntime.Profile.patchDatabaseId || environment != DataPatchRuntime.Profile.dataPatchSettings.environment)
                            throw new InvalidOperationException("Server requires a patch scope unavailable on this client.");
                        await DataPatchRuntime.Prepare(id);
                        if (DataPatchRuntime.Engine.PreparedRelease?.payloadHash != hash)
                            throw new InvalidOperationException("Server/client patch hash mismatch.");
                    }
                    else if (DataPatchRuntime.Engine != null)
                        await DataPatchRuntime.Prepare("", true);
                    if (patchClientNonce != nonce || !IsClientConnected)
                    {
                        DataPatchRuntime.Engine?.Discard();
                        return;
                    }
                }

                patchClientPreparedId = id;
                if (mode == 2)
                {
                    if (!IsServer && DataPatchRuntime.Engine != null)
                        DataPatchRuntime.Commit();
                    patchClientReady = true;
                    SendPatchAck(1, nonce, id, hash);
                }
                else
                    SendPatchAck(0, nonce, id, hash);
            }
            catch (Exception ex)
            {
                patchClientReady = false;
                Debug.LogError("Game-data patch synchronization failed: " + ex.Message);
                if (!IsServer)
                {
                    DataPatchRuntime.Engine?.Discard();
                    StopClient();
                }
            }
        }

        private void SendPatchAck(byte mode, string nonce, string id, string hash)
        {
            ClientSendPacket(0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.PatchAckMessage, writer =>
            {
                writer.Put(mode);
                writer.Put(nonce);
                writer.Put(id);
                writer.Put(hash);
            });
        }

        public override UniTask<bool> SetPlayerReady(uint requestId, long connectionId, LiteNetLib.Utils.NetDataReader reader)
        {
            if (PatchProtocolEnabled && (!DataPatchRuntime.IsReady || !patchPeerVersions.TryGetValue(connectionId, out string version) || version != DataPatchRuntime.ActiveReleaseId + ":" + DataPatchRuntime.ActiveHash))
                return UniTask.FromResult(false);
            return base.SetPlayerReady(requestId, connectionId, reader);
        }

        public async UniTask<string> ReloadGameDataPatch(string id = "")
        {
            if (!IsServer)
                return "Patch commands require a game server.";
            if (patchReloading || DataPatchRuntime.IsLoading)
                return "A patch load is already in progress.";
            patchReloading = true;
            bool activated = false;
            try
            {
                await DataPatchRuntime.Prepare(id);
                patchRequiredId = DataPatchRuntime.Engine.PreparedRelease?.id ?? "";
                patchRequiredHash = DataPatchRuntime.Engine.PreparedRelease?.payloadHash ?? "";
                patchReloadNonce = Guid.NewGuid().ToString("N");
                patchPreparedPeers.Clear();
                var connections = GetConnectionIds().ToArray();
                foreach (long peer in connections)
                {
                    patchQueuedPeers.Remove(peer);
                    SendPatchState(peer, 0, patchReloadNonce, patchRequiredId, patchRequiredHash);
                }

                float deadline = Time.realtimeSinceStartup + 30f;
                while (connections.Any(peer => GetConnectionIds().Contains(peer) && !patchPreparedPeers.Contains(peer)) && Time.realtimeSinceStartup < deadline)
                    await UniTask.Delay(100, ignoreTimeScale: true);
                // Disconnect non-participating clients before changing authoritative values.
                foreach (long peer in connections.Where(peer => GetConnectionIds().Contains(peer) && !patchPreparedPeers.Contains(peer)))
                    KickClient(peer, UITextKeys.UI_ERROR_SERVER_CLOSE);
                DataPatchRuntime.Commit();
                activated = true;
                patchPeerVersions.Clear();
                foreach (long peer in connections.Where(peer => GetConnectionIds().Contains(peer) && patchPreparedPeers.Contains(peer)))
                    SendPatchState(peer, 1, patchReloadNonce, patchRequiredId, patchRequiredHash);
                // New connections during preparation receive the committed release.
                SendQueuedPatchStates();
                deadline = Time.realtimeSinceStartup + 30f;
                string activeVersion = DataPatchRuntime.ActiveReleaseId + ":" + DataPatchRuntime.ActiveHash;
                while (connections.Any(peer => GetConnectionIds().Contains(peer) && patchPreparedPeers.Contains(peer) && (!patchPeerVersions.TryGetValue(peer, out string acknowledged) || acknowledged != activeVersion)) && Time.realtimeSinceStartup < deadline)
                    await UniTask.Delay(100, ignoreTimeScale: true);
                foreach (long peer in connections.Where(peer => GetConnectionIds().Contains(peer) && (!patchPeerVersions.TryGetValue(peer, out string acknowledged) || acknowledged != activeVersion)))
                    KickClient(peer, UITextKeys.UI_ERROR_SERVER_CLOSE);
                Debug.Log($"GM activated game-data release {patchRequiredId}.");
                return $"Activated data patch {patchRequiredId}.";
            }
            catch (Exception ex)
            {
                if (!activated)
                {
                    DataPatchRuntime.Engine?.Discard();
                    foreach (long peer in GetConnectionIds().ToArray())
                        SendPatchState(peer, 3, patchReloadNonce, "", "");
                }

                Debug.LogError("GM data patch reload failed: " + ex.Message);
                return "Patch reload failed: " + ex.Message;
            }
            finally
            {
                patchReloading = false;
                patchPreparedPeers.Clear();
                SendQueuedPatchStates();
            }
        }

        private void SendQueuedPatchStates()
        {
            foreach (long peer in patchQueuedPeers.ToArray())
                if (GetConnectionIds().Contains(peer))
                    SendPatchState(peer, 2, Guid.NewGuid().ToString("N"), DataPatchRuntime.ActiveReleaseId, DataPatchRuntime.ActiveHash);
            patchQueuedPeers.Clear();
        }
    }
}
