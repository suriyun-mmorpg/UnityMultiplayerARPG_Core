using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLib;
using LiteNetLibManager;

public class BaseGameNetworkManager : LiteNetLibGameManager
{
    public class MsgTypes
    {
        public const short ResponseWarp = 100;
    }

    protected override void RegisterClientMessages()
    {
        base.RegisterClientMessages();
        RegisterClientMessage(MsgTypes.ResponseWarp, HandleResponseWarp);
    }

    protected virtual void HandleResponseWarp(LiteNetLibMessageHandler messageHandler)
    {

    }

    public override bool StartServer()
    {
        Init();
        return base.StartServer();
    }

    public override LiteNetLibClient StartClient(string networkAddress, int networkPort, string connectKey)
    {
        Init();
        return base.StartClient(networkAddress, networkPort, connectKey);
    }

    public void Init()
    {
        doNotEnterGameOnConnect = false;
        var gameInstance = GameInstance.Singleton;
        Assets.offlineScene.SceneName = gameInstance.homeScene;
        Assets.playerPrefab = gameInstance.playerCharacterEntityPrefab.Identity;
        var spawnablePrefabs = new List<LiteNetLibIdentity>();
        spawnablePrefabs.Add(gameInstance.monsterCharacterEntityPrefab.Identity);
        spawnablePrefabs.Add(gameInstance.itemDropEntityPrefab.Identity);
        var damageEntities = GameInstance.DamageEntities.Values;
        foreach (var damageEntity in damageEntities)
        {
            spawnablePrefabs.Add(damageEntity.Identity);
        }
        Assets.spawnablePrefabs = spawnablePrefabs.ToArray();
    }

    public virtual void SendChatMessage(string message)
    {

    }

    public virtual void SendChatWhisperMessage(string targetCharacterName, string message)
    {

    }

    public virtual void SendChatPartyMessage(string message)
    {

    }

    public virtual void SendChatGuildMessage(string message)
    {

    }

    public virtual void WarpCharacter(PlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
    {
        if (playerCharacterEntity == null || !IsServer)
            return;
        // If warping to same map player does not have to reload new map data
        if (string.IsNullOrEmpty(mapName) || mapName.Equals(playerCharacterEntity.CurrentMapName))
        {
            playerCharacterEntity.CacheNetTransform.Teleport(position, Quaternion.identity);
            return;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public override void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        base.OnClientDisconnected(peer, disconnectInfo);
        var errorMessage = "Unknow";
        switch (disconnectInfo.Reason)
        {
            case DisconnectReason.DisconnectPeerCalled:
                errorMessage = "You have been kicked from server";
                break;
            case DisconnectReason.ConnectionFailed:
                errorMessage = "Cannot connect to the server";
                break;
            case DisconnectReason.RemoteConnectionClose:
                errorMessage = "Server has been closed";
                break;
            case DisconnectReason.SocketReceiveError:
                errorMessage = "Cannot receive data";
                break;
            case DisconnectReason.SocketSendError:
                errorMessage = "Cannot send data";
                break;
            case DisconnectReason.Timeout:
                errorMessage = "Connection timeout";
                break;
        }
        UISceneGlobal.Singleton.ShowMessageDialog("Disconnected", errorMessage, true, false, false, false);
    }

    public override void OnServerOnlineSceneLoaded()
    {
        base.OnServerOnlineSceneLoaded();
        var monsterSpawnAreas = FindObjectsOfType<MonsterSpawnArea>();
        foreach (var monsterSpawnArea in monsterSpawnAreas)
        {
            monsterSpawnArea.RandomSpawn(this);
        }
        if (IsServer && !IsClient && GameInstance.Singleton.serverCharacterPrefab != null)
            Instantiate(GameInstance.Singleton.serverCharacterPrefab);
    }
}
