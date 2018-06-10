using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;
using LiteNetLib.Utils;

public class LanRpgNetworkManager : BaseGameNetworkManager
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

    protected override void Awake()
    {
        Singleton = this;
        doNotDestroyOnSceneChanges = true;
        base.Awake();
    }

    public void StartGame()
    {
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

    public override void SerializeClientReadyExtra(NetDataWriter writer)
    {
        selectedCharacter.SerializeCharacterData(writer);
    }

    public override void DeserializeClientReadyExtra(LiteNetLibIdentity playerIdentity, NetPeer peer, NetDataReader reader)
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
