using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLib;
using LiteNetLibManager;

public class RpgGameManager : MonoBehaviour
{
    private LiteNetLibGameManager networkManager;

    public void Init(LiteNetLibGameManager networkManager)
    {
        this.networkManager = networkManager;
        networkManager.doNotEnterGameOnConnect = false;
        var gameInstance = GameInstance.Singleton;
        networkManager.Assets.offlineScene.SceneName = gameInstance.homeScene;
        networkManager.Assets.playerPrefab = gameInstance.playerCharacterEntityPrefab.Identity;
        var spawnablePrefabs = new List<LiteNetLibIdentity>();
        spawnablePrefabs.Add(gameInstance.monsterCharacterEntityPrefab.Identity);
        spawnablePrefabs.Add(gameInstance.itemDropEntityPrefab.Identity);
        var damageEntities = GameInstance.DamageEntities.Values;
        foreach (var damageEntity in damageEntities)
        {
            spawnablePrefabs.Add(damageEntity.Identity);
        }
        networkManager.Assets.spawnablePrefabs = spawnablePrefabs.ToArray();
    }

    public void Disconnect()
    {
        networkManager.StopHost();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
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

    public void OnServerOnlineSceneLoaded()
    {
        var monsterSpawnAreas = FindObjectsOfType<MonsterSpawnArea>();
        foreach (var monsterSpawnArea in monsterSpawnAreas)
        {
            monsterSpawnArea.RandomSpawn(networkManager);
        }
        if (networkManager.IsServer && !networkManager.IsClient && GameInstance.Singleton.serverCharacterPrefab != null)
            Instantiate(GameInstance.Singleton.serverCharacterPrefab);
    }
}
