using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public abstract class BaseGameNetworkManager : LiteNetLibGameManager
    {
        public class MsgTypes
        {
            public const ushort Warp = 100;
            public const ushort Chat = 101;
            public const ushort CashShopInfo = 102;
            public const ushort CashShopBuy = 103;
            public const ushort CashPackageInfo = 104;
            public const ushort CashPackageBuyValidation = 105;
            public const ushort PartyInfo = 106;
        }

        public const float UPDATE_PARTY_MEMBER_DURATION = 1f;
        
        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }
        protected readonly Dictionary<long, BasePlayerCharacterEntity> playerCharacters = new Dictionary<long, BasePlayerCharacterEntity>();
        protected readonly Dictionary<string, BasePlayerCharacterEntity> playerCharactersById = new Dictionary<string, BasePlayerCharacterEntity>();
        protected readonly Dictionary<string, BuildingEntity> buildingEntities = new Dictionary<string, BuildingEntity>();
        protected readonly Dictionary<string, NetPeer> peersByCharacterName = new Dictionary<string, NetPeer>();
        protected readonly Dictionary<int, PartyData> parties = new Dictionary<int, PartyData>();
        public MapInfo CurrentMapInfo { get; protected set; }
        // Events
        public System.Action<ChatMessage> onReceiveChat;
        private float lastUpdatePartyMemberTime;
        protected float tempUnscaledTime;

        protected override void Update()
        {
            base.Update();
            if (IsServer)
            {
                tempUnscaledTime = Time.unscaledTime;
                if (tempUnscaledTime - lastUpdatePartyMemberTime > UPDATE_PARTY_MEMBER_DURATION)
                {
                    // Update party member data, every seconds
                    UpdatePartyMembers();
                    lastUpdatePartyMemberTime = tempUnscaledTime;
                }
            }
        }

        protected virtual void UpdatePartyMembers()
        {
            foreach (var party in parties.Values)
            {
                foreach (var memberId in party.GetMemberIds().ToArray())
                {
                    BasePlayerCharacterEntity playerCharacterEntity;
                    if (playerCharactersById.TryGetValue(memberId, out playerCharacterEntity))
                        party.SetMember(playerCharacterEntity);
                    else
                        party.SetMemberInvisible(memberId);
                }
            }
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            this.InvokeInstanceDevExtMethods("RegisterClientMessages");
            RegisterClientMessage(MsgTypes.Warp, HandleWarpAtClient);
            RegisterClientMessage(MsgTypes.Chat, HandleChatAtClient);
            RegisterClientMessage(MsgTypes.CashShopInfo, HandleResponseCashShopInfo);
            RegisterClientMessage(MsgTypes.CashShopBuy, HandleResponseCashShopBuy);
            RegisterClientMessage(MsgTypes.CashPackageInfo, HandleResponseCashPackageInfo);
            RegisterClientMessage(MsgTypes.CashPackageBuyValidation, HandleResponseCashPackageBuyValidation);
            RegisterClientMessage(MsgTypes.PartyInfo, HandleResponsePartyInfo);
        }

        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
            this.InvokeInstanceDevExtMethods("RegisterServerMessages");
            RegisterServerMessage(MsgTypes.Chat, HandleChatAtServer);
            RegisterServerMessage(MsgTypes.CashShopInfo, HandleRequestCashShopInfo);
            RegisterServerMessage(MsgTypes.CashShopBuy, HandleRequestCashShopBuy);
            RegisterServerMessage(MsgTypes.CashPackageInfo, HandleRequestCashPackageInfo);
            RegisterServerMessage(MsgTypes.CashPackageBuyValidation, HandleRequestCashPackageBuyValidation);
            RegisterClientMessage(MsgTypes.PartyInfo, HandleRequestPartyInfo);
        }

        public uint RequestCashShopInfo(AckMessageCallback callback)
        {
            var message = new BaseAckMessage();
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MsgTypes.CashShopInfo, message, callback);
        }

        public uint RequestCashPackageInfo(AckMessageCallback callback)
        {
            var message = new BaseAckMessage();
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MsgTypes.CashPackageInfo, message, callback);
        }

        public uint RequestCashShopBuy(int dataId, AckMessageCallback callback)
        {
            var message = new RequestCashShopBuyMessage();
            message.dataId = dataId;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MsgTypes.CashShopBuy, message, callback);
        }

        public uint RequestCashPackageBuyValidation(int dataId, AckMessageCallback callback)
        {
            var message = new RequestCashPackageBuyValidationMessage();
            message.dataId = dataId;
            message.platform = Application.platform;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MsgTypes.CashPackageBuyValidation, message, callback);
        }

        public uint RequestPartyInfo(AckMessageCallback callback)
        {
            var message = new BaseAckMessage();
            return Client.SendAckPacket(SendOptions.Sequenced, Client.Peer, MsgTypes.PartyInfo, message, callback);
        }

        protected virtual void HandleWarpAtClient(LiteNetLibMessageHandler messageHandler)
        {
            // TODO: May fade black when warping
        }

        protected virtual void HandleChatAtClient(LiteNetLibMessageHandler messageHandler)
        {
            var message = messageHandler.ReadMessage<ChatMessage>();
            if (onReceiveChat != null)
                onReceiveChat.Invoke(message);
        }

        protected virtual void HandleResponseCashShopInfo(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseCashShopInfoMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected virtual void HandleResponseCashShopBuy(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseCashShopBuyMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected virtual void HandleResponseCashPackageInfo(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseCashPackageInfoMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected virtual void HandleResponseCashPackageBuyValidation(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseCashPackageBuyValidationMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected virtual void HandleResponsePartyInfo(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponsePartyInfoMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
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

        protected virtual void HandleRequestCashShopInfo(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var error = ResponseCashShopInfoMessage.Error.NotAvailable;
            var responseMessage = new ResponseCashShopInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashShopInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = 0;
            responseMessage.cashShopItemIds = new int[0];
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MsgTypes.CashShopInfo, responseMessage);
        }

        protected virtual void HandleRequestCashShopBuy(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestCashShopBuyMessage>();
            var error = ResponseCashShopBuyMessage.Error.NotAvailable;
            var responseMessage = new ResponseCashShopBuyMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashShopBuyMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = 0;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MsgTypes.CashShopBuy, responseMessage);
        }

        protected virtual void HandleRequestCashPackageInfo(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var error = ResponseCashPackageInfoMessage.Error.NotAvailable;
            var responseMessage = new ResponseCashPackageInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashPackageInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = 0;
            responseMessage.cashPackageIds = new int[0];
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MsgTypes.CashPackageInfo, responseMessage);
        }

        protected virtual void HandleRequestCashPackageBuyValidation(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestCashPackageBuyValidationMessage>();
            var error = ResponseCashPackageBuyValidationMessage.Error.NotAvailable;
            var responseMessage = new ResponseCashPackageBuyValidationMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashPackageBuyValidationMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = 0;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MsgTypes.CashPackageBuyValidation, responseMessage);
        }

        protected virtual void HandleRequestPartyInfo(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var responseMessage = new ResponsePartyInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Success;
            BasePlayerCharacterEntity playerCharacterEntity;
            PartyData partyData;
            if (playerCharacters.TryGetValue(peer.ConnectId, out playerCharacterEntity) && parties.TryGetValue(playerCharacterEntity.PartyId, out partyData))
                responseMessage.members = partyData.GetMembers().ToArray();
            LiteNetLibPacketSender.SendPacket(SendOptions.Sequenced, peer, MsgTypes.PartyInfo, responseMessage);
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
            Assets.offlineScene.SceneName = gameInstance.homeScene;
            Assets.playerPrefab = null;
            var spawnablePrefabs = new List<LiteNetLibIdentity>(Assets.spawnablePrefabs);
            if (gameInstance.itemDropEntityPrefab != null)
                spawnablePrefabs.Add(gameInstance.itemDropEntityPrefab.Identity);
            if (gameInstance.warpPortalEntityPrefab != null)
                spawnablePrefabs.Add(gameInstance.warpPortalEntityPrefab.Identity);
            foreach (var entry in GameInstance.PlayerCharacterEntities)
            {
                spawnablePrefabs.Add(entry.Value.Identity);
            }
            foreach (var entry in GameInstance.MonsterCharacterEntities)
            {
                spawnablePrefabs.Add(entry.Value.Identity);
            }
            foreach (var entry in GameInstance.WarpPortalEntities)
            {
                spawnablePrefabs.Add(entry.Value.Identity);
            }
            foreach (var entry in GameInstance.NpcEntities)
            {
                spawnablePrefabs.Add(entry.Value.Identity);
            }
            foreach (var entry in GameInstance.DamageEntities)
            {
                spawnablePrefabs.Add(entry.Value.Identity);
            }
            foreach (var entry in GameInstance.BuildingEntities)
            {
                spawnablePrefabs.Add(entry.Value.Identity);
            }
            Assets.spawnablePrefabs = spawnablePrefabs.ToArray();
            this.InvokeInstanceDevExtMethods("Init");
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

        public void Quit()
        {
            Application.Quit();
        }

        public override void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnClientDisconnected(peer, disconnectInfo);
            UISceneGlobal.Singleton.ShowDisconnectDialog(disconnectInfo);
        }

        private void RegisterEntities()
        {
            var harvestableSpawnAreas = FindObjectsOfType<HarvestableSpawnArea>();
            foreach (var harvestableSpawnArea in harvestableSpawnAreas)
            {
                harvestableSpawnArea.RegisterAssets();
            }
        }

        protected void SetupMapInfo()
        {
            MapInfo foundMapInfo;
            if (GameInstance.MapInfos.TryGetValue(SceneManager.GetActiveScene().name, out foundMapInfo))
                CurrentMapInfo = foundMapInfo;
            else
                CurrentMapInfo = ScriptableObject.CreateInstance<MapInfo>();
        }

        public override void OnClientOnlineSceneLoaded()
        {
            base.OnClientOnlineSceneLoaded();
            this.InvokeInstanceDevExtMethods("OnClientOnlineSceneLoaded");
            // Server will register entities later, so don't register entities now
            if (!IsServer)
            {
                RegisterEntities();
                SetupMapInfo();
            }
        }

        public override void OnServerOnlineSceneLoaded()
        {
            base.OnServerOnlineSceneLoaded();
            this.InvokeInstanceDevExtMethods("OnServerOnlineSceneLoaded");
            RegisterEntities();
            SetupMapInfo();
            // Spawn monsters
            var monsterSpawnAreas = FindObjectsOfType<MonsterSpawnArea>();
            foreach (var monsterSpawnArea in monsterSpawnAreas)
            {
                monsterSpawnArea.SpawnAll();
            }
            // Spawn Warp Portals
            if (GameInstance.MapWarpPortals.Count > 0)
            {
                List<WarpPortal> mapWarpPortals;
                if (GameInstance.MapWarpPortals.TryGetValue(Assets.onlineScene.SceneName, out mapWarpPortals))
                {
                    foreach (var warpPortal in mapWarpPortals)
                    {
                        var warpPortalPrefab = warpPortal.entityPrefab != null ? warpPortal.entityPrefab : gameInstance.warpPortalEntityPrefab;
                        if (warpPortalPrefab != null)
                        {
                            var warpPortalIdentity = Assets.NetworkSpawn(warpPortalPrefab.Identity, warpPortal.position, Quaternion.identity);
                            var warpPortalEntity = warpPortalIdentity.GetComponent<WarpPortalEntity>();
                            warpPortalEntity.mapScene.SceneName = warpPortal.warpToMap.SceneName;
                            warpPortalEntity.position = warpPortal.warpToPosition;
                        }
                    }
                }
            }
            // Spawn Npcs
            if (GameInstance.MapNpcs.Count > 0)
            {
                List<Npc> mapNpcs;
                if (GameInstance.MapNpcs.TryGetValue(Assets.onlineScene.SceneName, out mapNpcs))
                {
                    foreach (var npc in mapNpcs)
                    {
                        var npcPrefab = npc.entityPrefab;
                        if (npcPrefab != null)
                        {
                            var npcIdentity = Assets.NetworkSpawn(npcPrefab.Identity, npc.position, Quaternion.Euler(npc.rotation));
                            var npcEntity = npcIdentity.GetComponent<NpcEntity>();
                            npcEntity.StartDialog = npc.startDialog;
                            npcEntity.Title = npc.title;
                        }
                    }
                }
            }
            // If it's server (not host) spawn simple camera controller
            if (!IsClient && GameInstance.Singleton.serverCharacterPrefab != null)
                Instantiate(GameInstance.Singleton.serverCharacterPrefab);
        }

        public virtual void RegisterPlayerCharacter(NetPeer peer, BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !Peers.ContainsKey(peer.ConnectId) || playerCharacters.ContainsKey(peer.ConnectId))
                return;
            playerCharacters[peer.ConnectId] = playerCharacterEntity;
            playerCharactersById[playerCharacterEntity.Id] = playerCharacterEntity;
            peersByCharacterName[playerCharacterEntity.CharacterName] = peer;
        }

        public virtual void UnregisterPlayerCharacter(NetPeer peer)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!playerCharacters.TryGetValue(peer.ConnectId, out playerCharacterEntity))
                return;
            peersByCharacterName.Remove(playerCharacterEntity.CharacterName);
            playerCharactersById.Remove(playerCharacterEntity.Id);
            playerCharacters.Remove(peer.ConnectId);
        }

        public virtual void CreateBuildingEntity(BuildingSaveData saveData, bool initialize)
        {
            BuildingEntity prefab;
            if (GameInstance.BuildingEntities.TryGetValue(saveData.DataId, out prefab))
            {
                var buildingIdentity = Assets.NetworkSpawn(prefab.Identity, saveData.Position, saveData.Rotation);
                var buildingEntity = buildingIdentity.GetComponent<BuildingEntity>();
                buildingEntity.Id = saveData.Id;
                buildingEntity.ParentId = saveData.ParentId;
                buildingEntity.CurrentHp = saveData.CurrentHp;
                buildingEntity.CreatorId = saveData.CreatorId;
                buildingEntity.CreatorName = saveData.CreatorName;
                buildingEntities[buildingEntity.Id] = buildingEntity;
            }
        }

        public virtual void DestroyBuildingEntity(string id)
        {
            BuildingEntity entity;
            if (buildingEntities.TryGetValue(id, out entity))
            {
                entity.NetworkDestroy();
                buildingEntities.Remove(id);
            }
        }
        
        public abstract void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position);
        public abstract void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem);
        public abstract void PartySetting(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem);
        public abstract void AddPartyMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity);
        public abstract void KickFromParty(BasePlayerCharacterEntity playerCharacterEntity, string characterId);
        public abstract void LeaveParty(BasePlayerCharacterEntity playerCharacterEntity);
    }
}
