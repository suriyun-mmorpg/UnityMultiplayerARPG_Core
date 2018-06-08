using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibManager;
using LiteNetLib;
using LiteNetLib.Utils;

[RequireComponent(typeof(RpgGameManager))]
public class LanRpgNetworkManager : LiteNetLibGameManager
{
    public static LanRpgNetworkManager Singleton { get; protected set; }
    public enum GameStartType
    {
        Client,
        Host,
        SinglePlayer,
    }

    public float autoSaveDuration = 2f;
    public GameStartType startType;
    public PlayerCharacterData selectedCharacter;
    protected float lastSaveTime;

    private RpgGameManager cacheGameManager;
    public RpgGameManager GameManager
    {
        get
        {
            if (cacheGameManager == null)
                cacheGameManager = GetComponent<RpgGameManager>();
            return cacheGameManager;
        }
    }

    protected override void Awake()
    {
        Singleton = this;
        doNotDestroyOnSceneChanges = true;
        base.Awake();
    }

    public void StartGame()
    {
        GameManager.Init(this);
        var gameInstance = GameInstance.Singleton;
        var gameServiceConnection = gameInstance.NetworkSetting;
        switch (startType)
        {
            case GameStartType.Host:
                networkPort = gameServiceConnection.networkPort;
                maxConnections = gameServiceConnection.maxConnections;
                StartHost(false);
                break;
            case GameStartType.SinglePlayer:
                StartHost(true);
                break;
            case GameStartType.Client:
                networkPort = gameServiceConnection.networkPort;
                StartClient();
                break;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (Time.unscaledTime - lastSaveTime > autoSaveDuration)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null && IsNetworkActive)
                owningCharacter.SavePersistentCharacterData();
            lastSaveTime = Time.unscaledTime;
        }
    }

    public override void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        base.OnClientDisconnected(peer, disconnectInfo);
        GameManager.OnClientDisconnected(peer, disconnectInfo);
    }

    public override void OnServerOnlineSceneLoaded()
    {
        base.OnServerOnlineSceneLoaded();
        GameManager.OnServerOnlineSceneLoaded();
    }

    public override void SerializeClientReadyExtra(NetDataWriter writer)
    {
        selectedCharacter.SerializeCharacterData(writer);
    }

    public override void DeserializeClientReadyExtra(LiteNetLibIdentity playerIdentity, NetDataReader reader)
    {
        if (playerIdentity == null)
            return;
        var playerCharacterEntity = playerIdentity.GetComponent<PlayerCharacterEntity>();
        playerCharacterEntity.DeserializeCharacterData(reader);
        // Notify clients that this character is spawn or dead
        if (playerCharacterEntity.CurrentHp > 0)
            playerCharacterEntity.RequestOnRespawn(true);
        else
            playerCharacterEntity.RequestOnDead(true);
    }
}
