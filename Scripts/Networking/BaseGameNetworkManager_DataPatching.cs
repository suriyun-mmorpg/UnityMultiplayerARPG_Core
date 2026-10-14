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
        private readonly Queue<PatchReloadRequest> patchReloadRequests = new Queue<PatchReloadRequest>();
        private readonly HashSet<string> patchSeenRequests = new HashSet<string>();
        private readonly Queue<string> patchSeenRequestOrder = new Queue<string>();
        private int patchServerSession;
        private int patchClientSession;
        private bool patchReloading;
        private string patchClientNonce = "";
        private string patchClientPreparedId = "";
        private string patchClientPreparedHash = "";
        private bool PatchProtocolEnabled => GameInstance.Singleton.DataPatchProfile?.dataPatchSettings.enabled == true;

        private sealed class PatchReloadRequest
        {
            public string requestId;
            public string databaseId;
            public string releaseId;
            public string payloadHash;
            public bool broadcast;
        }

        private void RegisterPatchMessages()
        {
            RegisterClientMessage(GameNetworkingConsts.PatchStateMessage, message =>
            {
                byte mode = message.Reader.GetByte();
                string nonce = message.Reader.GetString();
                string id = message.Reader.GetString();
                string hash = message.Reader.GetString();
                string databaseId = message.Reader.GetString();
                ReceivePatchState(mode, nonce, id, hash, databaseId).Forget();
            });
            RegisterServerMessage(GameNetworkingConsts.PatchAckMessage, message =>
            {
                byte mode = message.Reader.GetByte();
                string nonce = message.Reader.GetString();
                string id = message.Reader.GetString();
                string hash = message.Reader.GetString();
                if (mode == 1 && DataPatchRuntime.IsReady && id == DataPatchRuntime.ActiveReleaseId && hash == DataPatchRuntime.ActiveHash)
                    patchPeerVersions[message.ConnectionId] = id + ":" + hash;
            });
        }

        private void SendPatchState(long connectionId, byte mode, string nonce, string id, string hash)
        {
            DataPatchProfile database = GameInstance.Singleton.DataPatchProfile;
            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.PatchStateMessage, writer =>
            {
                writer.Put(mode);
                writer.Put(nonce);
                writer.Put(id);
                writer.Put(hash);
                writer.Put(PatchProtocolEnabled ? database?.patchDatabaseId ?? "" : "");
            });
        }

        private async UniTaskVoid ReceivePatchState(byte mode, string nonce, string id, string hash, string databaseId)
        {
            int session = patchClientSession;
            try
            {
                if (mode == 3)
                {
                    if (nonce == patchClientNonce && !IsServer)
                    {
                        patchClientNonce = "";
                        if (!DataPatchRuntime.IsLoading)
                            DataPatchRuntime.Engine?.Discard();
                    }
                    return;
                }

                if (mode == 1)
                {
                    if (patchClientNonce != nonce || patchClientPreparedId != id || patchClientPreparedHash != hash)
                        throw new InvalidOperationException("Patch commit was not prepared.");
                    if (!IsServer)
                        DataPatchRuntime.Commit();
                    SendPatchAck(1, nonce, id, hash);
                    return;
                }

                if (mode != 0 && mode != 2)
                    throw new InvalidOperationException("Unknown patch protocol mode.");
                patchClientNonce = nonce;
                bool commitPrepared = false;
                if (!IsServer)
                {
                    await DataPatchRuntime.WaitForInitialization(requireReady: false);
                    if (this == null || session != patchClientSession || !IsClientConnected)
                        return;
                    while (DataPatchRuntime.IsLoading && patchClientNonce == nonce && IsClientConnected)
                        await UniTask.Yield();
                    if (this == null || session != patchClientSession || patchClientNonce != nonce || !IsClientConnected)
                        return;
                    if (!string.IsNullOrEmpty(databaseId) && databaseId != DataPatchRuntime.Profile?.patchDatabaseId)
                        throw new InvalidOperationException("Server/client patch scope mismatch.");
                    if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(hash))
                        throw new InvalidOperationException("Invalid server baseline patch hash.");
                    if (mode == 2 && DataPatchRuntime.IsReady && id == DataPatchRuntime.ActiveReleaseId && hash == DataPatchRuntime.ActiveHash)
                    {
                        if (!string.IsNullOrEmpty(id) && databaseId != DataPatchRuntime.Profile?.patchDatabaseId)
                            throw new InvalidOperationException("Server/client patch scope mismatch.");
                    }
                    else if (!string.IsNullOrEmpty(id))
                    {
                        if (DataPatchRuntime.Profile == null || databaseId != DataPatchRuntime.Profile.patchDatabaseId)
                            throw new InvalidOperationException("Server requires a patch scope unavailable on this client.");
                        await DataPatchRuntime.Prepare(id, expectedHash: hash);
                        commitPrepared = true;
                        if (DataPatchRuntime.Engine.PreparedRelease?.payloadHash != hash)
                            throw new InvalidOperationException("Server/client patch hash mismatch.");
                    }
                    else if (DataPatchRuntime.Engine != null)
                    {
                        await DataPatchRuntime.Prepare("", true);
                        commitPrepared = true;
                    }
                    if (this == null || session != patchClientSession || patchClientNonce != nonce || !IsClientConnected)
                    {
                        DataPatchRuntime.Engine?.Discard();
                        return;
                    }
                }

                patchClientPreparedId = id;
                patchClientPreparedHash = hash;
                if (mode == 2)
                {
                    if (!IsServer && commitPrepared)
                        DataPatchRuntime.Commit();
                    SendPatchAck(1, nonce, id, hash);
                }
                else
                    SendPatchAck(0, nonce, id, hash);
            }
            catch (Exception ex)
            {
                // Ignore work belonging to a previous connection.
                if (this == null || session != patchClientSession)
                    return;
                Debug.LogError("Game-data patch synchronization failed: " + ex.Message);
                if (!IsServer)
                {
                    // Keep gameplay running with the last successfully applied data.
                    if (!DataPatchRuntime.IsLoading)
                        DataPatchRuntime.Engine?.Discard();
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

        protected virtual string ValidateGameDataPatchReload()
        {
            if (!IsServer)
                return "Patch commands require a game server.";
            if (!PatchProtocolEnabled || DataPatchRuntime.Engine == null)
                return "Enable data patching on the GameInstance patch profile first.";
            return string.Empty;
        }

        public virtual UniTask<string> ReloadGameDataPatch(string id = "")
        {
            string error = ValidateGameDataPatchReload();
            if (!string.IsNullOrEmpty(error))
                return UniTask.FromResult(error);
            string requestId = Guid.NewGuid().ToString("N");
            if (!QueueGameDataPatchReload(requestId, DataPatchRuntime.Profile.patchDatabaseId, id, "", true))
                return UniTask.FromResult("Patch reload queue is full; try again after the pending reloads finish.");
            return UniTask.FromResult($"Queued data patch reload {requestId}; servers and clients apply it in the background.");
        }

        protected bool QueueGameDataPatchReload(string requestId, string databaseId, string releaseId, string payloadHash, bool broadcast = false)
        {
            if (!IsServer || !PatchProtocolEnabled || databaseId != DataPatchRuntime.Profile?.patchDatabaseId ||
                string.IsNullOrEmpty(requestId) || patchSeenRequests.Contains(requestId) || patchReloadRequests.Count >= 64)
                return false;
            patchSeenRequests.Add(requestId);
            patchSeenRequestOrder.Enqueue(requestId);
            while (patchSeenRequestOrder.Count > 256)
                patchSeenRequests.Remove(patchSeenRequestOrder.Dequeue());
            patchReloadRequests.Enqueue(new PatchReloadRequest
            {
                requestId = requestId,
                databaseId = databaseId,
                releaseId = releaseId,
                payloadHash = payloadHash,
                broadcast = broadcast,
            });
            if (!patchReloading)
            {
                patchReloading = true;
                ProcessGameDataPatchReloads(patchServerSession).Forget();
            }
            return true;
        }

        protected virtual UniTask BroadcastGameDataPatchReload(string requestId, string databaseId, string releaseId, string payloadHash)
        {
            // LAN games only have this server; MMO managers override the inter-server transport.
            return UniTask.CompletedTask;
        }

        private async UniTaskVoid ProcessGameDataPatchReloads(int session)
        {
            try
            {
                while (this != null && IsServer && session == patchServerSession && patchReloadRequests.Count > 0)
                {
                    PatchReloadRequest request = patchReloadRequests.Dequeue();
                    DataPatchEngine engine = DataPatchRuntime.Engine;
                    try
                    {
                        // Only the background worker waits; gameplay and new connections keep running.
                        await DataPatchRuntime.WaitForInitialization(requireReady: false);
                        while (this != null && IsServer && session == patchServerSession && DataPatchRuntime.IsLoading)
                            await UniTask.Yield();
                        if (this == null || !IsServer || session != patchServerSession || engine != DataPatchRuntime.Engine)
                            return;
                        if (request.databaseId != DataPatchRuntime.Profile?.patchDatabaseId)
                            throw new InvalidOperationException("Patch reload scope changed.");
                        await DataPatchRuntime.Prepare(request.releaseId, baseline: !request.broadcast && string.IsNullOrEmpty(request.releaseId), expectedHash: request.broadcast ? null : request.payloadHash);
                        if (this == null || !IsServer || session != patchServerSession || engine != DataPatchRuntime.Engine)
                        {
                            engine?.Discard();
                            return;
                        }
                        string releaseId = engine.PreparedRelease?.id ?? "";
                        string hash = engine.PreparedRelease?.payloadHash ?? "";
                        if (!request.broadcast && hash != request.payloadHash)
                            throw new InvalidOperationException("Broadcast patch hash does not match the downloaded release.");
                        DataPatchRuntime.Commit();
                        foreach (long peer in GetConnectionIds().ToArray())
                            SendPatchState(peer, 2, request.requestId, releaseId, hash);
                        if (request.broadcast)
                        {
                            bool sent = false;
                            for (int attempt = 0; attempt < 3 && !sent; ++attempt)
                            {
                                if (this == null || !IsServer || session != patchServerSession)
                                    return;
                                try
                                {
                                    await BroadcastGameDataPatchReload(request.requestId, request.databaseId, releaseId, hash);
                                    sent = true;
                                }
                                catch (Exception error)
                                {
                                    if (attempt == 2)
                                        Debug.LogError($"[Data Patch] Reload {request.requestId} activated locally, but broadcasting to other servers failed: {error.Message}");
                                    else
                                        await UniTask.Delay(1000, ignoreTimeScale: true);
                                }
                            }
                        }
                        Debug.Log($"[Data Patch] Reload {request.requestId} activated {releaseId} and notified connected clients.");
                    }
                    catch (Exception ex)
                    {
                        if (this == null || session != patchServerSession)
                            return;
                        if (engine == DataPatchRuntime.Engine && !DataPatchRuntime.IsLoading)
                            engine?.Discard();
                        Debug.LogError($"[Data Patch] Reload {request.requestId} failed: {ex.Message}. Gameplay continues with current data.");
                    }
                }
            }
            finally
            {
                if (this != null && session == patchServerSession)
                    patchReloading = false;
            }
        }

        private void ResetGameDataPatchReloads()
        {
            ++patchServerSession;
            patchReloading = false;
            patchReloadRequests.Clear();
            patchSeenRequests.Clear();
            patchSeenRequestOrder.Clear();
            patchPeerVersions.Clear();
        }

        private async UniTaskVoid SendInitialPatchState(long connectionId)
        {
            int session = patchServerSession;
            // This only schedules patch synchronization; connection/game entry never awaits it.
            try
            {
                if (PatchProtocolEnabled)
                    await DataPatchRuntime.WaitForInitialization(requireReady: false);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Startup patch unavailable; gameplay continues with current data: " + ex.Message);
            }

            while (this != null && IsServer && session == patchServerSession && patchReloading && GetConnectionIds().Contains(connectionId))
                await UniTask.Yield();
            if (this == null || !IsServer || session != patchServerSession || !GetConnectionIds().Contains(connectionId))
                return;
            SendPatchState(connectionId, 2, Guid.NewGuid().ToString("N"), DataPatchRuntime.ActiveReleaseId, DataPatchRuntime.ActiveHash);
        }
    }
}
