using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LiteNetLibHighLevel;
using LiteNetLib;
using LiteNetLib.Utils;

[RequireComponent(typeof(LoadGameMaps))]
public abstract class BaseRpgNetworkManager : LiteNetLibGameManager
{
    public enum GameStartType
    {
        Client,
        Server,
        Host,
        SinglePlayer,
    }

    public class RpgGameMsgTypes
    {
        public const short ClientRequestCharacter = GameMsgTypes.Highest + 1;
        public const short ServerMapResult = GameMsgTypes.Highest + 2;
    }

    public static GameStartType StartType;
    public static string ConnectingNetworkAddress;
    public static int ConnectingNetworkPort;

    [Header("Rpg game UIs")]
    public GameObject mapLoadingObject;
    public Image imageMapLoadingGage;
    public Text textMapLoadingPercent;

    protected AsyncOperation clientMapLoadOp;
    protected Scene clientScene;
    protected GameObject clientScenePhysic;
    protected readonly Dictionary<long, CharacterData> pendingCharacters = new Dictionary<long, CharacterData>();

    private LoadGameMaps tempLoadGameMaps;
    public LoadGameMaps TempLoadGameMaps
    {
        get
        {
            if (tempLoadGameMaps == null)
                tempLoadGameMaps = GetComponent<LoadGameMaps>();
            return tempLoadGameMaps;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        var gameInstance = GameInstance.Singleton;
        Assets.RegisterPrefab(gameInstance.characterEntityPrefab.Identity);
        /*
        var damages = GameInstance.Damages.Values;
        foreach (var damage in damages)
        {
            Assets.RegisterPrefab(damage)
        }
        */
    }

    protected virtual void Start()
    {
        switch (StartType)
        {
            case GameStartType.Server:
            case GameStartType.Host:
            case GameStartType.SinglePlayer:
                TempLoadGameMaps.onLoadedMaps += OnLoadedMaps;
                TempLoadGameMaps.LoadMaps();
                StartCoroutine(LoadMapRoutine());
                break;
            case GameStartType.Client:
                networkAddress = ConnectingNetworkAddress;
                networkPort = ConnectingNetworkPort;
                StartClient();
                break;
        }
    }

    IEnumerator LoadMapRoutine()
    {
        yield return null;
        if (mapLoadingObject != null)
            mapLoadingObject.SetActive(true);
        yield return null;
        while (!TempLoadGameMaps.IsDone)
        {
            yield return null;
            var progress = TempLoadGameMaps.Progress;
            if (imageMapLoadingGage != null)
                imageMapLoadingGage.fillAmount = progress;
            if (textMapLoadingPercent != null)
                textMapLoadingPercent.text = (progress * 100).ToString("N0") + "% / 100%";
        }
        yield return null;
        if (mapLoadingObject != null)
            mapLoadingObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        switch (StartType)
        {
            case GameStartType.Server:
            case GameStartType.Host:
            case GameStartType.SinglePlayer:
                TempLoadGameMaps.onLoadedMaps -= OnLoadedMaps;
                break;
        }
        base.OnDestroy();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        clientReadyOnConnect = false;
        Assets.playerPrefab = null;
    }
#endif

    protected virtual void OnLoadedMaps()
    {
        switch (StartType)
        {
            case GameStartType.Server:
                StartServer();
                break;
            case GameStartType.Host:
                StartHost();
                break;
            case GameStartType.SinglePlayer:
                maxConnections = 1;
                StartHost();
                break;
        }
    }

    protected override void RegisterServerMessages()
    {
        base.RegisterServerMessages();
        RegisterServerMessage(RpgGameMsgTypes.ClientRequestCharacter, HandleClientRequestCharacter);
    }

    protected override void RegisterClientMessages()
    {
        base.RegisterClientMessages();
        RegisterClientMessage(RpgGameMsgTypes.ServerMapResult, HandleServerMapResult);
    }

    public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        base.OnPeerDisconnected(peer, disconnectInfo);
        if (pendingCharacters.ContainsKey(peer.ConnectId))
            pendingCharacters.Remove(peer.ConnectId);
    }

    public override void OnClientConnected(NetPeer peer)
    {
        SendPacket(SendOptions.ReliableUnordered, peer, RpgGameMsgTypes.ClientRequestCharacter, SerializeCharacterRequest);
    }

    protected override void HandleClientReady(LiteNetLibMessageHandler messageHandler)
    {
        base.HandleClientReady(messageHandler);
        var gameInstance = GameInstance.Singleton;
        var peer = messageHandler.peer;
        var reader = messageHandler.reader;
        if (pendingCharacters.ContainsKey(peer.ConnectId))
        {
            var characterData = pendingCharacters[peer.ConnectId];
            var playerIdentity = SpawnPlayer(peer, gameInstance.characterEntityPrefab.Identity);
            var characterEntity = playerIdentity.GetComponent<CharacterEntity>();
            characterData.CloneTo(characterEntity);
        }
    }

    protected void HandleClientRequestCharacter(LiteNetLibMessageHandler messageHandler)
    {
        var peer = messageHandler.peer;
        var reader = messageHandler.reader;
        var character = DeserializeCharacterRequest(peer, reader);
        pendingCharacters[peer.ConnectId] = character;
        SendPacket(SendOptions.ReliableUnordered, peer, RpgGameMsgTypes.ServerMapResult, (writer) =>
        {
            var map = TempLoadGameMaps.LoadedMaps[character.CurrentMapName];
            writer.Put(character.CurrentMapName);
            writer.Put(map.MapOffsets.x);
            writer.Put(map.MapOffsets.y);
            writer.Put(map.MapOffsets.z);
        });
    }

    protected void HandleServerMapResult(LiteNetLibMessageHandler messageHandler)
    {
        var reader = messageHandler.reader;
        var mapName = reader.GetString();
        var offset = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        StartCoroutine(LoadMap(mapName, offset));
    }

    IEnumerator LoadMap(string mapName, Vector3 offset)
    {
        yield return null;
        if (mapLoadingObject != null)
            mapLoadingObject.SetActive(true);
        yield return null;
        clientMapLoadOp = SceneManager.LoadSceneAsync(mapName, LoadSceneMode.Additive);
        while (!clientMapLoadOp.isDone)
        {
            yield return null;
            var progress = clientMapLoadOp.progress * 100f;
            if (imageMapLoadingGage != null)
                imageMapLoadingGage.fillAmount = progress / 100f;
            if (textMapLoadingPercent != null)
                textMapLoadingPercent.text = (progress * 100).ToString("N0") + "% / 100%";
        }
        clientScene = SceneManager.GetSceneByName(mapName);
        var rootObjects = clientScene.GetRootGameObjects();
        foreach (var rootObject in rootObjects)
        {
            var position = rootObject.transform.position;
            rootObject.transform.position = position + offset;
            // Remove all colliders/cameras/audio listeners
            rootObject.RemoveComponentsInChildren<Collider>(true);
            rootObject.RemoveComponentsInChildren<AudioListener>(true);
            rootObject.RemoveComponentsInChildren<FlareLayer>(true);
            rootObject.RemoveComponentsInChildren<Camera>(true);
        }
        // Load collider data for clients
        if (!IsServer)
        {
            var gameMap = GameInstance.GameMaps[mapName];
            clientScenePhysic = Instantiate(gameMap.physicPrefab);
            var position = clientScenePhysic.transform.position;
            clientScenePhysic.transform.position = position + offset;
            TempLoadGameMaps.TempNavMeshSurface.BuildNavMesh();
        }
        yield return null;
        if (mapLoadingObject != null)
            mapLoadingObject.SetActive(false);
        yield return null;
        SendClientReady();
    }

    protected abstract void SerializeCharacterRequest(NetDataWriter writer);
    protected abstract CharacterData DeserializeCharacterRequest(NetPeer peer, NetDataReader reader);
}
