using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LiteNetLibHighLevel;
using LiteNetLib;
using LiteNetLib.Utils;

public abstract class BaseRpgNetworkManager : LiteNetLibGameManager
{
    protected bool isQuit;
    protected override void Awake()
    {
        var gameInstance = GameInstance.Singleton;
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
        base.Awake();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        clientReadyOnConnect = true;
    }
#endif

    protected override void RegisterServerMessages()
    {
        base.RegisterServerMessages();
    }

    protected override void RegisterClientMessages()
    {
        base.RegisterClientMessages();
    }

    public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        base.OnPeerDisconnected(peer, disconnectInfo);
    }

    public override void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        base.OnClientDisconnected(peer, disconnectInfo);
        var errorMessage = "Unknow";
        switch (disconnectInfo.Reason)
        {
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
        if (disconnectInfo.Reason != DisconnectReason.DisconnectPeerCalled)
        {
            UISceneGlobal.Singleton.ShowMessageDialog("Disconnected", errorMessage, true, false, false, false, () =>
            {
                UISceneLoading.Singleton.LoadScene(GameInstance.Singleton.homeScene);
            });
        }
    }

    protected override void OnApplicationQuit()
    {
        isQuit = true;
        base.OnApplicationQuit();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        var monsterSpawnAreas = FindObjectsOfType<MonsterSpawnArea>();
        foreach (var monsterSpawnArea in monsterSpawnAreas)
        {
            monsterSpawnArea.RandomSpawn(this);
        }
    }

    public override void OnStopHost()
    {
        if (!isQuit)
            UISceneLoading.Singleton.LoadScene(GameInstance.Singleton.homeScene);
        base.OnStopHost();
    }
}
