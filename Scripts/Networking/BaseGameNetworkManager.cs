using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

public abstract class BaseGameNetworkManager : LiteNetLibGameManager
{
    public class MsgTypes
    {
        public const short Warp = 100;
        public const short Chat = 101;
    }

    protected readonly Dictionary<long, PlayerCharacterEntity> playerCharacters = new Dictionary<long, PlayerCharacterEntity>();
    protected readonly Dictionary<string, NetPeer> peersByCharacterName = new Dictionary<string, NetPeer>();
    // Events
    public System.Action<ChatMessage> onReceiveChat;

    protected override void RegisterClientMessages()
    {
        base.RegisterClientMessages();
        RegisterClientMessage(MsgTypes.Warp, HandleWarpAtClient);
        RegisterClientMessage(MsgTypes.Chat, HandleChatAtClient);
    }

    protected override void RegisterServerMessages()
    {
        base.RegisterServerMessages();
        RegisterServerMessage(MsgTypes.Chat, HandleChatAtServer);
    }

    protected virtual void HandleWarpAtClient(LiteNetLibMessageHandler messageHandler)
    {
        // TODO: May fade black when warping
    }

    protected virtual void HandleChatAtServer(LiteNetLibMessageHandler messageHandler)
    {
        var peer = messageHandler.peer;
        var message = messageHandler.ReadMessage<ChatMessage>();
        HandleChatAtServer(message);
    }

    protected virtual void HandleChatAtServer(ChatMessage message)
    {
        switch (message.channel)
        {
            case ChatChannel.Global:
                // Send message to all peers (clients)
                SendPacketToAllPeers(SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                break;
            case ChatChannel.Whisper:
                NetPeer senderPeer;
                NetPeer receiverPeer;
                if (!string.IsNullOrEmpty(message.sender) &&
                    peersByCharacterName.TryGetValue(message.sender, out senderPeer))
                    LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, senderPeer, MsgTypes.Chat, message);
                if (!string.IsNullOrEmpty(message.receiver) &&
                    peersByCharacterName.TryGetValue(message.receiver, out receiverPeer))
                    LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, receiverPeer, MsgTypes.Chat, message);
                break;
            case ChatChannel.Party:
                // TODO: Implement this later when party system ready
                break;
            case ChatChannel.Guild:
                // TODO: Implement this later when guild system ready
                break;
        }
    }

    protected virtual void HandleChatAtClient(LiteNetLibMessageHandler messageHandler)
    {
        var message = messageHandler.ReadMessage<ChatMessage>();
        if (onReceiveChat != null)
            onReceiveChat.Invoke(message);
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
        spawnablePrefabs.Add(gameInstance.buildingEntityPrefab.Identity);
        var damageEntities = GameInstance.DamageEntities.Values;
        foreach (var damageEntity in damageEntities)
        {
            spawnablePrefabs.Add(damageEntity.Identity);
        }
        Assets.spawnablePrefabs = spawnablePrefabs.ToArray();
    }

    public virtual void EnterChat(ChatChannel channel, string message, string senderName, string receiverName)
    {
        if (!IsClientConnected)
            return;
        // Send chat message to server
        var chatMessage = new ChatMessage();
        chatMessage.channel = channel;
        chatMessage.message = message;
        chatMessage.sender = senderName;
        chatMessage.receiver = receiverName;
        LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, Client.Peer, MsgTypes.Chat, chatMessage);
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

    public virtual void RegisterPlayerCharacter(NetPeer peer, PlayerCharacterEntity playerCharacterEntity)
    {
        if (playerCharacterEntity == null || !Peers.ContainsKey(peer.ConnectId) || playerCharacters.ContainsKey(peer.ConnectId))
            return;
        playerCharacters[peer.ConnectId] = playerCharacterEntity;
        peersByCharacterName[playerCharacterEntity.CharacterName] = peer;
    }

    public virtual void UnregisterPlayerCharacter(NetPeer peer)
    {
        PlayerCharacterEntity playerCharacterEntity;
        if (!playerCharacters.TryGetValue(peer.ConnectId, out playerCharacterEntity))
            return;
        peersByCharacterName.Remove(playerCharacterEntity.CharacterName);
        playerCharacters.Remove(peer.ConnectId);
    }
}