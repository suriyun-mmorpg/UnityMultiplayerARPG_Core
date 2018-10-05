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
            public const ushort UpdatePartyMember = 106;
            public const ushort UpdateParty = 107;
            public const ushort UpdateGuildMember = 108;
            public const ushort UpdateGuild = 109;
        }

        public const float UPDATE_ONLINE_CHARACTER_DURATION = 1f;

        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }
        protected readonly Dictionary<long, BasePlayerCharacterEntity> playerCharacters = new Dictionary<long, BasePlayerCharacterEntity>();
        protected readonly Dictionary<string, BasePlayerCharacterEntity> playerCharactersById = new Dictionary<string, BasePlayerCharacterEntity>();
        protected readonly Dictionary<string, BuildingEntity> buildingEntities = new Dictionary<string, BuildingEntity>();
        protected readonly Dictionary<string, long> connectionIdsByCharacterName = new Dictionary<string, long>();
        protected readonly Dictionary<int, PartyData> parties = new Dictionary<int, PartyData>();
        protected readonly Dictionary<int, GuildData> guilds = new Dictionary<int, GuildData>();
        public PartyData ClientParty { get; protected set; }
        public GuildData ClientGuild { get; protected set; }
        public MapInfo CurrentMapInfo { get; protected set; }
        // Events
        public System.Action<ChatMessage> onClientReceiveChat;
        public System.Action<PartyData> onClientUpdateParty;
        public System.Action<GuildData> onClientUpdateGuild;
        protected float lastUpdateOnlineCharacterTime;
        protected float tempUnscaledTime;
        protected PartyData[] tempPartyDataArray;
        protected GuildData[] tempGuildDataArray;
        protected string[] tempCharacterIdArray;

        public bool TryGetPlayerCharacter(long connectionId, out BasePlayerCharacterEntity result)
        {
            return playerCharacters.TryGetValue(connectionId, out result);
        }

        public bool TryGetPlayerCharacterById(string id, out BasePlayerCharacterEntity result)
        {
            return playerCharactersById.TryGetValue(id, out result);
        }

        public bool TryGetPlayerCharacterByName(string characterName, out BasePlayerCharacterEntity result)
        {
            result = null;
            long connectionId;
            return connectionIdsByCharacterName.TryGetValue(characterName, out connectionId) && playerCharacters.TryGetValue(connectionId, out result);
        }

        public bool TryGetParty(int id, out PartyData result)
        {
            return parties.TryGetValue(id, out result);
        }

        public bool TryGetGuild(int id, out GuildData result)
        {
            return guilds.TryGetValue(id, out result);
        }

        protected override void Update()
        {
            base.Update();
            if (IsServer)
            {
                tempUnscaledTime = Time.unscaledTime;
                if (tempUnscaledTime - lastUpdateOnlineCharacterTime > UPDATE_ONLINE_CHARACTER_DURATION)
                {
                    // Update online characters, every seconds
                    UpdateOnlineCharacters(tempUnscaledTime);
                    lastUpdateOnlineCharacterTime = tempUnscaledTime;
                }
            }
        }

        protected virtual void UpdateOnlineCharacters(float time)
        {
            BasePlayerCharacterEntity playerCharacter;
            // Update online party characters
            tempPartyDataArray = parties.Values.ToArray();
            foreach (var party in tempPartyDataArray)
            {
                tempCharacterIdArray = party.GetMemberIds().ToArray();
                foreach (var memberId in tempCharacterIdArray)
                {
                    if (playerCharactersById.TryGetValue(memberId, out playerCharacter))
                    {
                        party.UpdateMember(playerCharacter);
                        party.NotifyMemberOnline(memberId, time);
                    }
                    party.UpdateMemberOnline(memberId, time);
                }
            }
            // Update online guild characters
            tempGuildDataArray = guilds.Values.ToArray();
            foreach (var guild in tempGuildDataArray)
            {
                tempCharacterIdArray = guild.GetMemberIds().ToArray();
                foreach (var memberId in tempCharacterIdArray)
                {
                    if (playerCharactersById.TryGetValue(memberId, out playerCharacter))
                    {
                        guild.UpdateMember(playerCharacter);
                        guild.NotifyMemberOnline(memberId, time);
                    }
                    guild.UpdateMemberOnline(memberId, time);
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
            RegisterClientMessage(MsgTypes.UpdatePartyMember, HandleUpdatePartyMemberAtClient);
            RegisterClientMessage(MsgTypes.UpdateParty, HandleUpdatePartyAtClient);
            RegisterClientMessage(MsgTypes.UpdateGuildMember, HandleUpdateGuildMemberAtClient);
            RegisterClientMessage(MsgTypes.UpdateGuild, HandleUpdateGuildAtClient);
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
        }

        public uint RequestCashShopInfo(AckMessageCallback callback)
        {
            var message = new BaseAckMessage();
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MsgTypes.CashShopInfo, message, callback);
        }

        public uint RequestCashPackageInfo(AckMessageCallback callback)
        {
            var message = new BaseAckMessage();
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MsgTypes.CashPackageInfo, message, callback);
        }

        public uint RequestCashShopBuy(int dataId, AckMessageCallback callback)
        {
            var message = new RequestCashShopBuyMessage();
            message.dataId = dataId;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MsgTypes.CashShopBuy, message, callback);
        }

        public uint RequestCashPackageBuyValidation(int dataId, AckMessageCallback callback)
        {
            var message = new RequestCashPackageBuyValidationMessage();
            message.dataId = dataId;
            message.platform = Application.platform;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MsgTypes.CashPackageBuyValidation, message, callback);
        }

        protected virtual void HandleWarpAtClient(LiteNetLibMessageHandler messageHandler)
        {
            // TODO: May fade black when warping
        }

        protected virtual void HandleChatAtClient(LiteNetLibMessageHandler messageHandler)
        {
            var message = messageHandler.ReadMessage<ChatMessage>();
            if (onClientReceiveChat != null)
                onClientReceiveChat.Invoke(message);
        }

        protected virtual void HandleResponseCashShopInfo(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseCashShopInfoMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected virtual void HandleResponseCashShopBuy(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseCashShopBuyMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected virtual void HandleResponseCashPackageInfo(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseCashPackageInfoMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected virtual void HandleResponseCashPackageBuyValidation(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseCashPackageBuyValidationMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected virtual void HandleUpdatePartyMemberAtClient(LiteNetLibMessageHandler messageHandler)
        {
            UpdateSocialGroupMember(ClientParty, messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            if (onClientUpdateParty != null)
                onClientUpdateParty.Invoke(ClientParty);
        }

        protected virtual void HandleUpdatePartyAtClient(LiteNetLibMessageHandler messageHandler)
        {
            var message = messageHandler.ReadMessage<UpdatePartyMessage>();
            if (message.type == UpdatePartyMessage.UpdateType.Create)
            {
                ClientParty = new PartyData(message.id, message.shareExp, message.shareItem, message.characterId);
            }
            else if (ClientParty != null && ClientParty.id == message.id)
            {
                switch (message.type)
                {
                    case UpdatePartyMessage.UpdateType.ChangeLeader:
                        ClientParty.SetLeader(message.characterId);
                        break;
                    case UpdatePartyMessage.UpdateType.Setting:
                        ClientParty.Setting(message.shareExp, message.shareItem);
                        break;
                    case UpdatePartyMessage.UpdateType.Terminate:
                        ClientParty = null;
                        break;
                }
            }
            if (onClientUpdateParty != null)
                onClientUpdateParty.Invoke(ClientParty);
        }

        protected virtual void HandleUpdateGuildMemberAtClient(LiteNetLibMessageHandler messageHandler)
        {
            UpdateSocialGroupMember(ClientGuild, messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            if (onClientUpdateGuild != null)
                onClientUpdateGuild.Invoke(ClientGuild);
        }

        protected virtual void HandleUpdateGuildAtClient(LiteNetLibMessageHandler messageHandler)
        {
            var message = messageHandler.ReadMessage<UpdateGuildMessage>();
            if (message.type == UpdateGuildMessage.UpdateType.Create)
            {
                ClientGuild = new GuildData(message.id, message.guildName, message.characterId);
            }
            else if (ClientGuild != null && ClientGuild.id == message.id)
            {
                switch (message.type)
                {
                    case UpdateGuildMessage.UpdateType.ChangeLeader:
                        ClientGuild.SetLeader(message.characterId);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMessage:
                        ClientGuild.guildMessage = message.guildMessage;
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildRole:
                        ClientGuild.SetRole(message.guildRole, message.roleName, message.canInvite, message.canKick, message.shareExpPercentage);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMemberRole:
                        ClientGuild.SetMemberRole(message.characterId, message.guildRole);
                        break;
                    case UpdateGuildMessage.UpdateType.Terminate:
                        ClientGuild = null;
                        break;
                }
            }
            if (onClientUpdateGuild != null)
                onClientUpdateGuild.Invoke(ClientGuild);
        }

        protected virtual void HandleChatAtServer(LiteNetLibMessageHandler messageHandler)
        {
            ReadChatMessage(FillChatChannelId(messageHandler.ReadMessage<ChatMessage>()));
        }

        protected ChatMessage FillChatChannelId(ChatMessage message)
        {
            BasePlayerCharacterEntity playerCharacter;
            if (message.channel == ChatChannel.Party || message.channel == ChatChannel.Guild)
            {
                if (!string.IsNullOrEmpty(message.sender) &&
                    TryGetPlayerCharacterByName(message.sender, out playerCharacter))
                {
                    switch (message.channel)
                    {
                        case ChatChannel.Party:
                            message.channelId = playerCharacter.PartyId;
                            break;
                        case ChatChannel.Guild:
                            message.channelId = playerCharacter.GuildId;
                            break;
                    }
                }
            }
            return message;
        }

        protected virtual void ReadChatMessage(ChatMessage message)
        {
            long senderConnectionId;
            long receiverConnectionId;
            BasePlayerCharacterEntity playerCharacter;
            switch (message.channel)
            {
                case ChatChannel.Global:
                    // Send message to all clients
                    ServerSendPacketToAllConnections(SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                    break;
                case ChatChannel.Whisper:
                    if (!string.IsNullOrEmpty(message.sender) &&
                        connectionIdsByCharacterName.TryGetValue(message.sender, out senderConnectionId))
                        ServerSendPacket(senderConnectionId, SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                    if (!string.IsNullOrEmpty(message.receiver) &&
                        connectionIdsByCharacterName.TryGetValue(message.receiver, out receiverConnectionId))
                        ServerSendPacket(receiverConnectionId, SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                    break;
                case ChatChannel.Party:
                    PartyData party;
                    if (parties.TryGetValue(message.channelId, out party))
                    {
                        tempCharacterIdArray = party.GetMemberIds().ToArray();
                        foreach (var memberId in tempCharacterIdArray)
                        {
                            if (playerCharactersById.TryGetValue(memberId, out playerCharacter) &&
                                ContainsConnectionId(playerCharacter.ConnectId))
                                ServerSendPacket(playerCharacter.ConnectId, SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                        }
                    }
                    break;
                case ChatChannel.Guild:
                    GuildData guild;
                    if (guilds.TryGetValue(message.channelId, out guild))
                    {
                        tempCharacterIdArray = guild.GetMemberIds().ToArray();
                        foreach (var memberId in tempCharacterIdArray)
                        {
                            if (playerCharactersById.TryGetValue(memberId, out playerCharacter) &&
                                ContainsConnectionId(playerCharacter.ConnectId))
                                ServerSendPacket(playerCharacter.ConnectId, SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                        }
                    }
                    break;
            }
        }

        protected virtual void HandleRequestCashShopInfo(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var error = ResponseCashShopInfoMessage.Error.NotAvailable;
            var responseMessage = new ResponseCashShopInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashShopInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = 0;
            responseMessage.cashShopItemIds = new int[0];
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.CashShopInfo, responseMessage);
        }

        protected virtual void HandleRequestCashShopBuy(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestCashShopBuyMessage>();
            var error = ResponseCashShopBuyMessage.Error.NotAvailable;
            var responseMessage = new ResponseCashShopBuyMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashShopBuyMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = 0;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.CashShopBuy, responseMessage);
        }

        protected virtual void HandleRequestCashPackageInfo(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var error = ResponseCashPackageInfoMessage.Error.NotAvailable;
            var responseMessage = new ResponseCashPackageInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashPackageInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = 0;
            responseMessage.cashPackageIds = new int[0];
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.CashPackageInfo, responseMessage);
        }

        protected virtual void HandleRequestCashPackageBuyValidation(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestCashPackageBuyValidationMessage>();
            var error = ResponseCashPackageBuyValidationMessage.Error.NotAvailable;
            var responseMessage = new ResponseCashPackageBuyValidationMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashPackageBuyValidationMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.cash = 0;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.CashPackageBuyValidation, responseMessage);
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
            ClientSendPacket(SendOptions.ReliableOrdered, MsgTypes.Chat, chatMessage);
        }

        public void Quit()
        {
            Application.Quit();
        }

        public override void OnClientDisconnected(DisconnectInfo disconnectInfo)
        {
            base.OnClientDisconnected(disconnectInfo);
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

        protected bool UpdateSocialGroupMember(SocialGroupData socialGroupData, UpdateSocialMemberMessage message)
        {
            if (socialGroupData == null || socialGroupData.id != message.id)
                return false;

            switch (message.type)
            {
                case UpdateSocialMemberMessage.UpdateType.Add:
                    socialGroupData.AddMember(message.member);
                    break;
                case UpdateSocialMemberMessage.UpdateType.Update:
                    socialGroupData.UpdateMember(message.member);
                    break;
                case UpdateSocialMemberMessage.UpdateType.Remove:
                    socialGroupData.RemoveMember(message.characterId);
                    break;
            }
            return true;
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

        public virtual void RegisterPlayerCharacter(long connectionId, BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !ConnectionIds.Contains(connectionId) || playerCharacters.ContainsKey(connectionId))
                return;
            playerCharacters[connectionId] = playerCharacterEntity;
            playerCharactersById[playerCharacterEntity.Id] = playerCharacterEntity;
            connectionIdsByCharacterName[playerCharacterEntity.CharacterName] = connectionId;
        }

        public virtual void UnregisterPlayerCharacter(long connectionId)
        {
            BasePlayerCharacterEntity playerCharacter;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter))
                return;
            connectionIdsByCharacterName.Remove(playerCharacter.CharacterName);
            playerCharactersById.Remove(playerCharacter.Id);
            playerCharacters.Remove(connectionId);
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

        #region Activity validation functions
        public virtual bool CanWarpCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer)
                return false;
            return true;
        }

        public virtual bool CanCreateParty(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer)
                return false;
            if (playerCharacterEntity.PartyId > 0)
            {
                // TODO: May send warn message that player already in party
                return false;
            }
            return true;
        }

        public virtual bool CanChangePartyLeader(BasePlayerCharacterEntity playerCharacterEntity, string characterId, out int partyId, out PartyData party)
        {
            partyId = 0;
            party = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            partyId = playerCharacterEntity.PartyId;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                // TODO: May send warn message that player not in party
                return false;
            }
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return false;
            }
            if (!party.ContainsMemberId(characterId))
            {
                // TODO: May warn that target character is not in party
                return false;
            }
            return true;
        }

        public virtual bool CanPartySetting(BasePlayerCharacterEntity playerCharacterEntity, out int partyId, out PartyData party)
        {
            partyId = 0;
            party = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            partyId = playerCharacterEntity.PartyId;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                // TODO: May send warn message that player not in party
                return false;
            }
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return false;
            }
            return true;
        }

        public virtual bool CanSendPartyInvitation(BasePlayerCharacterEntity inviteCharacterEntity, uint objectId, out BasePlayerCharacterEntity targetCharacterEntity)
        {
            targetCharacterEntity = null;
            var partyId = inviteCharacterEntity.PartyId;
            PartyData party;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                // TODO: May send warn message that player not in party
                return false;
            }
            if (!PartyData.CanInvite(inviteCharacterEntity.PartyMemberFlags))
            {
                // TODO: May send warn message that player can not invite
                return false;
            }
            if (!inviteCharacterEntity.TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                // TODO: May send warn message that character is not found
                return false;
            }
            if (targetCharacterEntity.CoCharacter != null)
            {
                // TODO: May send warn message that character is not available
                return false;
            }
            if (targetCharacterEntity.PartyId > 0)
            {
                // TODO: May send warn message that player already in party
                return false;
            }
            return true;
        }

        public virtual bool CanAddPartyMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity, out int partyId, out PartyData party)
        {
            partyId = 0;
            party = null;
            if (inviteCharacterEntity == null || acceptCharacterEntity == null || !IsServer)
                return false;
            if (acceptCharacterEntity.PartyId > 0)
            {
                // TODO: May send warn message that player already in party
                return false;
            }
            partyId = inviteCharacterEntity.PartyId;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                // TODO: May send warn message that player not in party
                return false;
            }
            if (!party.IsLeader(inviteCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return false;
            }
            if (party.CountMember() == gameInstance.SocialSystemSetting.MaxPartyMember)
            {
                // TODO: May warn that it's exceeds limit max party member
                return false;
            }
            return true;
        }

        public virtual bool CanKickFromParty(BasePlayerCharacterEntity playerCharacterEntity, string characterId, out int partyId, out PartyData party)
        {
            partyId = 0;
            party = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            partyId = playerCharacterEntity.PartyId;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                // TODO: May send warn message that player is not in party
                return false;
            }
            if (!PartyData.CanKick(playerCharacterEntity.PartyMemberFlags))
            {
                // TODO: May send warn message that player can not kick
                return false;
            }
            if (playerCharacterEntity.Id.Equals(characterId))
            {
                // TODO: May warn that it's owning character so it's not able to kick
                return false;
            }
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return false;
            }
            if (!party.ContainsMemberId(characterId))
            {
                // TODO: May warn that target character is not in party
                return false;
            }
            return true;
        }

        public virtual bool CanLeaveParty(BasePlayerCharacterEntity playerCharacterEntity, out int partyId, out PartyData party)
        {
            partyId = 0;
            party = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            partyId = playerCharacterEntity.PartyId;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                // TODO: May send warn message that player is not in party
                return false;
            }
            return true;
        }

        public virtual bool CanCreateGuild(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer)
                return false;
            if (playerCharacterEntity.GuildId > 0)
            {
                // TODO: May send warn message that player already in guild
                return false;
            }
            return true;
        }

        public virtual bool CanChangeGuildLeader(BasePlayerCharacterEntity playerCharacterEntity, string characterId, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return false;
            }
            if (!guild.ContainsMemberId(characterId))
            {
                // TODO: May warn that target character is not in guild
                return false;
            }
            return true;
        }

        public virtual bool CanSetGuildMessage(BasePlayerCharacterEntity playerCharacterEntity, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return false;
            }
            return true;
        }

        public virtual bool CanSetGuildRole(BasePlayerCharacterEntity playerCharacterEntity, byte guildRole, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return false;
            }
            if (!guild.IsRoleAvailable(guildRole))
            {
                // TODO: May warn that guild role is not available
                return false;
            }
            return true;
        }

        public virtual bool CanSetGuildMemberRole(BasePlayerCharacterEntity playerCharacterEntity, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return false;
            }
            return true;
        }

        public virtual bool CanSendGuildInvitation(BasePlayerCharacterEntity inviteCharacterEntity, uint objectId, out BasePlayerCharacterEntity targetCharacterEntity)
        {
            targetCharacterEntity = null;
            var guildId = inviteCharacterEntity.GuildId;
            GuildData guild;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!GuildData.CanInvite(inviteCharacterEntity.GuildMemberFlags))
            {
                // TODO: May send warn message that player can not invite
                return false;
            }
            if (!inviteCharacterEntity.TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                // TODO: May send warn message that character is not found
                return false;
            }
            if (targetCharacterEntity.CoCharacter != null)
            {
                // TODO: May send warn message that character is not available
                return false;
            }
            if (targetCharacterEntity.GuildId > 0)
            {
                // TODO: May send warn message that player already in guild
                return false;
            }
            return true;
        }

        public virtual bool CanAddGuildMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (inviteCharacterEntity == null || acceptCharacterEntity == null || !IsServer)
                return false;
            if (acceptCharacterEntity.GuildId > 0)
            {
                // TODO: May send warn message that player already in guild
                return false;
            }
            guildId = inviteCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!guild.IsLeader(inviteCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return false;
            }
            if (guild.CountMember() == gameInstance.SocialSystemSetting.MaxGuildMember)
            {
                // TODO: May warn that it's exceeds limit max guild member
                return false;
            }
            return true;
        }

        public virtual bool CanKickFromGuild(BasePlayerCharacterEntity playerCharacterEntity, string characterId, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!GuildData.CanKick(playerCharacterEntity.GuildMemberFlags))
            {
                // TODO: May send warn message that player can not kick
                return false;
            }
            if (playerCharacterEntity.Id.Equals(characterId))
            {
                // TODO: May warn that it's owning character so it's not able to kick
                return false;
            }
            byte role;
            if (!guild.TryGetMemberRole(characterId, out role) && playerCharacterEntity.GuildRole < role)
            {
                // TODO: May warn that character rank is lower
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return false;
            }
            if (!guild.ContainsMemberId(characterId))
            {
                // TODO: May warn that target character is not in guild
                return false;
            }
            return true;
        }

        public virtual bool CanLeaveGuild(BasePlayerCharacterEntity playerCharacterEntity, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            return true;
        }
        #endregion

        #region Activity functions
        public void SendCreatePartyToClient(long connectionId, PartyData party)
        {
            Server.SendCreateParty(connectionId, MsgTypes.UpdateParty, party.id, party.shareExp, party.shareItem, party.leaderId);
        }

        public void SendChangePartyLeaderToClient(long connectionId, PartyData party)
        {
            Server.SendChangePartyLeader(connectionId, MsgTypes.UpdateParty, party.id, party.leaderId);
        }

        public void SendChangePartyLeaderToClients(PartyData party)
        {
            if (party == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in party.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendChangePartyLeaderToClient(playerCharacterEntity.ConnectId, party);
            }
        }

        public void SendPartySettingToClient(long connectionId, PartyData party)
        {
            Server.SendPartySetting(connectionId, MsgTypes.UpdateParty, party.id, party.shareExp, party.shareItem);
        }

        public void SendPartySettingToClients(PartyData party)
        {
            if (party == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in party.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendPartySettingToClient(playerCharacterEntity.ConnectId, party);
            }
        }

        public void SendPartyTerminateToClient(long connectionId, int id)
        {
            Server.SendPartyTerminate(connectionId, MsgTypes.UpdateParty, id);
        }

        public void SendAddPartyMemberToClient(long connectionId, int id, string characterId, string characterName, int dataId, int level)
        {
            Server.SendAddPartyMember(connectionId, MsgTypes.UpdatePartyMember, id, characterId, characterName, dataId, level);
        }

        public void SendAddPartyMembersToClient(long connectionId, PartyData party)
        {
            if (party == null)
                return;

            foreach (var member in party.GetMembers())
            {
                SendAddPartyMemberToClient(connectionId, party.id, member.id, member.characterName, member.dataId, member.level);
            }
        }

        public void SendAddPartyMemberToClients(PartyData party, string characterId, string characterName, int dataId, int level)
        {
            if (party == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in party.GetMembers())
            {
                if (!member.id.Equals(characterId) && TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendAddPartyMemberToClient(playerCharacterEntity.ConnectId, party.id, characterId, characterName, dataId, level);
            }
        }

        public void SendRemovePartyMemberToClient(long connectionId, int id, string characterId)
        {
            Server.SendRemovePartyMember(connectionId, MsgTypes.UpdatePartyMember, id, characterId);
        }

        public void SendRemovePartyMemberToClients(PartyData party, string characterId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in party.GetMembers())
            {
                if (!member.id.Equals(characterId) && TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendRemovePartyMemberToClient(playerCharacterEntity.ConnectId, party.id, characterId);
            }
        }

        public void SendCreateGuildToClient(long connectionId, GuildData guild)
        {
            Server.SendCreateGuild(connectionId, MsgTypes.UpdateGuild, guild.id, guild.guildName, guild.leaderId);
        }

        public void SendChangeGuildLeaderToClient(long connectionId, GuildData guild)
        {
            Server.SendChangeGuildLeader(connectionId, MsgTypes.UpdateGuild, guild.id, guild.leaderId);
        }

        public void SendChangeGuildLeaderToClients(GuildData guild)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in guild.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendChangeGuildLeaderToClient(playerCharacterEntity.ConnectId, guild);
            }
        }

        public void SendSetGuildMessageToClient(long connectionId, GuildData guild)
        {
            Server.SendSetGuildMessage(connectionId, MsgTypes.UpdateGuild, guild.id, guild.guildMessage);
        }

        public void SendSetGuildMessageToClients(GuildData guild)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in guild.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendSetGuildMessageToClient(playerCharacterEntity.ConnectId, guild);
            }
        }

        public void SendSetGuildRoleToClient(long connectionId, int id, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            Server.SendSetGuildRole(connectionId, MsgTypes.UpdateGuild, id, guildRole, roleName, canInvite, canKick, shareExpPercentage);
        }

        public void SendSetGuildRoleToClients(GuildData guild, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in guild.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendSetGuildRoleToClient(playerCharacterEntity.ConnectId, guild.id, guildRole, roleName, canInvite, canKick, shareExpPercentage);
            }
        }

        public void SendSetGuildMemberRoleToClient(long connectionId, int id, string characterId, byte guildRole)
        {
            Server.SendSetGuildMemberRole(connectionId, MsgTypes.UpdateGuild, id, characterId, guildRole);
        }

        public void SendSetGuildMemberRoleToClients(GuildData guild, string characterId, byte guildRole)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in guild.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendSetGuildMemberRoleToClient(playerCharacterEntity.ConnectId, guild.id, characterId, guildRole);
            }
        }

        public void SendGuildTerminateToClient(long connectionId, int id)
        {
            Server.SendGuildTerminate(connectionId, MsgTypes.UpdateGuild, id);
        }

        public void SendAddGuildMemberToClient(long connectionId, int id, string characterId, string characterName, int dataId, int level)
        {
            Server.SendAddGuildMember(connectionId, MsgTypes.UpdateGuildMember, id, characterId, characterName, dataId, level);
        }

        public void SendAddGuildMembersToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            foreach (var member in guild.GetMembers())
            {
                SendAddGuildMemberToClient(connectionId, guild.id, member.id, member.characterName, member.dataId, member.level);
            }
        }

        public void SendAddGuildMemberToClients(GuildData guild, string characterId, string characterName, int dataId, int level)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in guild.GetMembers())
            {
                if (!member.id.Equals(characterId) && TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendAddGuildMemberToClient(playerCharacterEntity.ConnectId, guild.id, characterId, characterName, dataId, level);
            }
        }

        public void SendRemoveGuildMemberToClient(long connectionId, int id, string characterId)
        {
            Server.SendRemoveGuildMember(connectionId, MsgTypes.UpdateGuildMember, id, characterId);
        }

        public void SendRemoveGuildMemberToClients(GuildData guild, string characterId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in guild.GetMembers())
            {
                if (!member.id.Equals(characterId) && TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendRemoveGuildMemberToClient(playerCharacterEntity.ConnectId, guild.id, characterId);
            }
        }

        public virtual void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            if (!CanWarpCharacter(playerCharacterEntity))
                return;

            // If warping to same map player does not have to reload new map data
            if (string.IsNullOrEmpty(mapName) || mapName.Equals(playerCharacterEntity.CurrentMapName))
                playerCharacterEntity.CacheNetTransform.Teleport(position, Quaternion.identity);
        }

        public virtual void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem, int partyId)
        {
            if (!CanCreateParty(playerCharacterEntity))
                return;

            var party = new PartyData(partyId, shareExp, shareItem, playerCharacterEntity);
            parties[partyId] = party;
            playerCharacterEntity.PartyId = partyId;
            playerCharacterEntity.PartyMemberFlags = party.GetPartyMemberFlags(playerCharacterEntity);
            SendCreatePartyToClient(playerCharacterEntity.ConnectId, party);
            SendAddPartyMembersToClient(playerCharacterEntity.ConnectId, party);
        }

        public virtual void ChangePartyLeader(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            int partyId;
            PartyData party;
            if (!CanChangePartyLeader(playerCharacterEntity, characterId, out partyId, out party))
                return;

            party.SetLeader(characterId);
            parties[partyId] = party;
            BasePlayerCharacterEntity targetCharacterEntity;
            if (TryGetPlayerCharacterById(characterId, out targetCharacterEntity))
                targetCharacterEntity.PartyMemberFlags = party.GetPartyMemberFlags(targetCharacterEntity);
            playerCharacterEntity.PartyMemberFlags = party.GetPartyMemberFlags(playerCharacterEntity);
            SendChangePartyLeaderToClients(party);
        }

        public virtual void PartySetting(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            int partyId;
            PartyData party;
            if (!CanPartySetting(playerCharacterEntity, out partyId, out party))
                return;

            party.Setting(shareExp, shareItem);
            parties[partyId] = party;
            SendPartySettingToClients(party);
        }

        public virtual void AddPartyMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            int partyId;
            PartyData party;
            if (!CanAddPartyMember(inviteCharacterEntity, acceptCharacterEntity, out partyId, out party))
                return;

            party.AddMember(acceptCharacterEntity);
            parties[partyId] = party;
            acceptCharacterEntity.PartyId = partyId;
            acceptCharacterEntity.PartyMemberFlags = party.GetPartyMemberFlags(acceptCharacterEntity);
            SendCreatePartyToClient(acceptCharacterEntity.ConnectId, party);
            SendAddPartyMembersToClient(acceptCharacterEntity.ConnectId, party);
            SendAddPartyMemberToClients(party, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
        }

        public virtual void KickFromParty(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            int partyId;
            PartyData party;
            if (!CanKickFromParty(playerCharacterEntity, characterId, out partyId, out party))
                return;

            BasePlayerCharacterEntity memberCharacterEntity;
            if (playerCharactersById.TryGetValue(characterId, out memberCharacterEntity))
            {
                memberCharacterEntity.ClearParty();
                SendPartyTerminateToClient(memberCharacterEntity.ConnectId, partyId);
            }
            party.RemoveMember(characterId);
            parties[partyId] = party;
            SendRemovePartyMemberToClients(party, characterId);
        }

        public virtual void LeaveParty(BasePlayerCharacterEntity playerCharacterEntity)
        {
            int partyId;
            PartyData party;
            if (!CanLeaveParty(playerCharacterEntity, out partyId, out party))
                return;

            if (party.IsLeader(playerCharacterEntity))
            {
                foreach (var memberId in party.GetMemberIds())
                {
                    BasePlayerCharacterEntity memberCharacterEntity;
                    if (playerCharactersById.TryGetValue(memberId, out memberCharacterEntity))
                    {
                        memberCharacterEntity.ClearParty();
                        SendPartyTerminateToClient(memberCharacterEntity.ConnectId, partyId);
                    }
                }
                parties.Remove(partyId);
            }
            else
            {
                playerCharacterEntity.ClearParty();
                SendPartyTerminateToClient(playerCharacterEntity.ConnectId, partyId);
                party.RemoveMember(playerCharacterEntity.Id);
                parties[partyId] = party;
                SendRemovePartyMemberToClients(party, playerCharacterEntity.Id);
            }
        }

        public virtual void CreateGuild(BasePlayerCharacterEntity playerCharacterEntity, string guildName, int guildId)
        {
            if (!CanCreateGuild(playerCharacterEntity))
                return;

            var guild = new GuildData(guildId, guildName, playerCharacterEntity);
            byte guildRole;
            guilds[guildId] = guild;
            playerCharacterEntity.GuildId = guildId;
            playerCharacterEntity.GuildMemberFlags = guild.GetGuildMemberFlagsAndRole(playerCharacterEntity, out guildRole);
            playerCharacterEntity.GuildRole = guildRole;
            playerCharacterEntity.SharedGuildExp = 0;
            SendCreateGuildToClient(playerCharacterEntity.ConnectId, guild);
            SendAddGuildMembersToClient(playerCharacterEntity.ConnectId, guild);
        }

        public virtual void ChangeGuildLeader(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            int guildId;
            GuildData guild;
            if (!CanChangeGuildLeader(playerCharacterEntity, characterId, out guildId, out guild))
                return;

            guild.SetLeader(characterId);
            guilds[guildId] = guild;
            byte role;
            BasePlayerCharacterEntity targetCharacterEntity;
            if (TryGetPlayerCharacterById(characterId, out targetCharacterEntity))
            {
                targetCharacterEntity.GuildMemberFlags = guild.GetGuildMemberFlagsAndRole(targetCharacterEntity, out role);
                targetCharacterEntity.GuildRole = role;
            }
            playerCharacterEntity.GuildMemberFlags = guild.GetGuildMemberFlagsAndRole(playerCharacterEntity, out role);
            playerCharacterEntity.GuildRole = role;
            SendChangeGuildLeaderToClients(guild);
        }

        public virtual void SetGuildMessage(BasePlayerCharacterEntity playerCharacterEntity, string guildMessage)
        {
            int guildId;
            GuildData guild;
            if (!CanSetGuildMessage(playerCharacterEntity, out guildId, out guild))
                return;

            guild.guildMessage = guildMessage;
            guilds[guildId] = guild;
            SendSetGuildMessageToClients(guild);
        }

        public virtual void SetGuildRole(BasePlayerCharacterEntity playerCharacterEntity, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            int guildId;
            GuildData guild;
            if (!CanSetGuildRole(playerCharacterEntity, guildRole, out guildId, out guild))
                return;

            guild.SetRole(guildRole, roleName, canInvite, canKick, shareExpPercentage);
            guilds[guildId] = guild;
            // Change characters guild role
            foreach (var memberId in guild.GetMemberIds())
            {
                BasePlayerCharacterEntity memberCharacterEntity;
                if (playerCharactersById.TryGetValue(memberId, out memberCharacterEntity))
                {
                    memberCharacterEntity.GuildMemberFlags = guild.GetGuildMemberFlagsAndRole(memberCharacterEntity, out guildRole);
                    memberCharacterEntity.GuildRole = guildRole;
                }
            }
            SendSetGuildRoleToClients(guild, guildRole, roleName, canInvite, canKick, shareExpPercentage);
        }

        public virtual void SetGuildMemberRole(BasePlayerCharacterEntity playerCharacterEntity, string characterId, byte guildRole)
        {
            int guildId;
            GuildData guild;
            if (!CanSetGuildMemberRole(playerCharacterEntity, out guildId, out guild))
                return;

            guild.SetMemberRole(characterId, guildRole);
            guilds[guildId] = guild;
            if (TryGetPlayerCharacterById(characterId, out playerCharacterEntity))
            {
                playerCharacterEntity.GuildMemberFlags = guild.GetGuildMemberFlagsAndRole(playerCharacterEntity, out guildRole);
                playerCharacterEntity.GuildRole = guildRole;
            }
            SendSetGuildMemberRoleToClients(guild, characterId, guildRole);
        }

        public virtual void AddGuildMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            int guildId;
            GuildData guild;
            if (!CanAddGuildMember(inviteCharacterEntity, acceptCharacterEntity, out guildId, out guild))
                return;

            guild.AddMember(acceptCharacterEntity);
            byte guildRole;
            guilds[guildId] = guild;
            acceptCharacterEntity.GuildId = guildId;
            acceptCharacterEntity.GuildMemberFlags = guild.GetGuildMemberFlagsAndRole(acceptCharacterEntity, out guildRole);
            acceptCharacterEntity.GuildRole = guildRole;
            acceptCharacterEntity.SharedGuildExp = 0;
            SendCreateGuildToClient(acceptCharacterEntity.ConnectId, guild);
            SendAddGuildMembersToClient(acceptCharacterEntity.ConnectId, guild);
            SendAddGuildMemberToClients(guild, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
        }

        public virtual void KickFromGuild(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            int guildId;
            GuildData guild;
            if (!CanKickFromGuild(playerCharacterEntity, characterId, out guildId, out guild))
                return;

            BasePlayerCharacterEntity memberCharacterEntity;
            if (playerCharactersById.TryGetValue(characterId, out memberCharacterEntity))
            {
                memberCharacterEntity.ClearGuild();
                SendGuildTerminateToClient(memberCharacterEntity.ConnectId, guildId);
            }
            guild.RemoveMember(characterId);
            guilds[guildId] = guild;
            SendRemoveGuildMemberToClients(guild, characterId);
        }

        public virtual void LeaveGuild(BasePlayerCharacterEntity playerCharacterEntity)
        {
            int guildId;
            GuildData guild;
            if (!CanLeaveGuild(playerCharacterEntity, out guildId, out guild))
                return;

            if (guild.IsLeader(playerCharacterEntity))
            {
                foreach (var memberId in guild.GetMemberIds())
                {
                    BasePlayerCharacterEntity memberCharacterEntity;
                    if (playerCharactersById.TryGetValue(memberId, out memberCharacterEntity))
                    {
                        memberCharacterEntity.ClearGuild();
                        SendGuildTerminateToClient(memberCharacterEntity.ConnectId, guildId);
                    }
                }
                guilds.Remove(guildId);
            }
            else
            {
                playerCharacterEntity.ClearGuild();
                SendGuildTerminateToClient(playerCharacterEntity.ConnectId, guildId);
                guild.RemoveMember(playerCharacterEntity.Id);
                guilds[guildId] = guild;
                SendRemoveGuildMemberToClients(guild, playerCharacterEntity.Id);
            }
        }
        #endregion

        public abstract void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem);
        public abstract void CreateGuild(BasePlayerCharacterEntity playerCharacterEntity, string guildName);
    }
}
