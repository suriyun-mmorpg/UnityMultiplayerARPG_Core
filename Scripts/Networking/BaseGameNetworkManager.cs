using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameNetworkManager : LiteNetLibGameManager
    {
        public class MsgTypes
        {
            public const ushort GameMessage = 99;
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

        public static BaseGameNetworkManager Singleton { get; protected set; }
        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }
        protected readonly Dictionary<long, BasePlayerCharacterEntity> playerCharacters = new Dictionary<long, BasePlayerCharacterEntity>();
        protected readonly Dictionary<string, BasePlayerCharacterEntity> playerCharactersById = new Dictionary<string, BasePlayerCharacterEntity>();
        protected readonly Dictionary<string, BuildingEntity> buildingEntities = new Dictionary<string, BuildingEntity>();
        protected readonly Dictionary<string, long> connectionIdsByCharacterName = new Dictionary<string, long>();
        protected readonly Dictionary<int, PartyData> parties = new Dictionary<int, PartyData>();
        protected readonly Dictionary<int, GuildData> guilds = new Dictionary<int, GuildData>();
        protected readonly Dictionary<long, PartyData> updatingPartyMembers = new Dictionary<long, PartyData>();
        protected readonly Dictionary<long, GuildData> updatingGuildMembers = new Dictionary<long, GuildData>();
        public PartyData ClientParty { get; protected set; }
        public GuildData ClientGuild { get; protected set; }
        public MapInfo CurrentMapInfo { get; protected set; }
        // Events
        public System.Action<ChatMessage> onClientReceiveChat;
        public System.Action<GameMessage> onClientReceiveGameMessage;
        public System.Action<PartyData> onClientUpdateParty;
        public System.Action<GuildData> onClientUpdateGuild;
        protected float lastUpdateOnlineCharacterTime;

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

        protected override void Awake()
        {
            Singleton = this;
            doNotDestroyOnSceneChanges = true;
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
            float tempUnscaledTime = Time.unscaledTime;
            if (tempUnscaledTime - lastUpdateOnlineCharacterTime > UPDATE_ONLINE_CHARACTER_DURATION)
            {
                // Update social members, every seconds
                // Update at server
                if (IsServer)
                    UpdateOnlineCharacters(tempUnscaledTime);
                lastUpdateOnlineCharacterTime = tempUnscaledTime;
            }
        }

        protected override void RegisterClientMessages()
        {
            this.InvokeInstanceDevExtMethods("RegisterClientMessages");
            base.RegisterClientMessages();
            RegisterClientMessage(MsgTypes.GameMessage, HandleGameMessageAtClient);
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
            this.InvokeInstanceDevExtMethods("RegisterServerMessages");
            base.RegisterServerMessages();
            RegisterServerMessage(MsgTypes.Chat, HandleChatAtServer);
            RegisterServerMessage(MsgTypes.CashShopInfo, HandleRequestCashShopInfo);
            RegisterServerMessage(MsgTypes.CashShopBuy, HandleRequestCashShopBuy);
            RegisterServerMessage(MsgTypes.CashPackageInfo, HandleRequestCashPackageInfo);
            RegisterServerMessage(MsgTypes.CashPackageBuyValidation, HandleRequestCashPackageBuyValidation);
        }

        protected virtual void Clean()
        {
            this.InvokeInstanceDevExtMethods("Clean");
            playerCharacters.Clear();
            playerCharactersById.Clear();
            buildingEntities.Clear();
            connectionIdsByCharacterName.Clear();
            parties.Clear();
            guilds.Clear();
            updatingPartyMembers.Clear();
            updatingGuildMembers.Clear();
            ClientParty = null;
            ClientGuild = null;
            CurrentMapInfo = null;
        }

        public override void OnStartServer()
        {
            this.InvokeInstanceDevExtMethods("OnStartServer");
            base.OnStartServer();
        }

        public override void OnStopServer()
        {
            Clean();
            base.OnStopServer();
        }

        public override void OnStopClient()
        {
            if (!IsServer)
                Clean();
            base.OnStopClient();
        }

        protected virtual void UpdateOnlineCharacter(long connectionId, BasePlayerCharacterEntity playerCharacterEntity, float time)
        {
            PartyData tempParty;
            GuildData tempGuild;

            if (playerCharacterEntity.PartyId > 0 && parties.TryGetValue(playerCharacterEntity.PartyId, out tempParty))
            {
                tempParty.UpdateMember(playerCharacterEntity);
                tempParty.NotifyOnlineMember(playerCharacterEntity.Id);
                if (!updatingPartyMembers.ContainsKey(connectionId))
                    updatingPartyMembers.Add(connectionId, tempParty);
            }

            if (playerCharacterEntity.GuildId > 0 && guilds.TryGetValue(playerCharacterEntity.GuildId, out tempGuild))
            {
                tempGuild.UpdateMember(playerCharacterEntity);
                tempGuild.NotifyOnlineMember(playerCharacterEntity.Id);
                if (!updatingGuildMembers.ContainsKey(connectionId))
                    updatingGuildMembers.Add(connectionId, tempGuild);
            }
        }

        protected virtual void UpdateOnlineCharacters(float time)
        {
            updatingPartyMembers.Clear();
            updatingGuildMembers.Clear();

            foreach (KeyValuePair<long, BasePlayerCharacterEntity> entry in playerCharacters)
            {
                UpdateOnlineCharacter(entry.Key, entry.Value, time);
            }

            foreach (KeyValuePair<long, PartyData> updatingPartyMember in updatingPartyMembers)
            {
                SendUpdatePartyMembersToClient(updatingPartyMember.Key, updatingPartyMember.Value);
            }

            foreach (KeyValuePair<long, GuildData> updatingGuildMember in updatingGuildMembers)
            {
                SendUpdateGuildMembersToClient(updatingGuildMember.Key, updatingGuildMember.Value);
            }
        }

        public virtual void SendServerGameMessage(long connectionId, GameMessage.Type type)
        {
            GameMessage message = new GameMessage();
            message.type = type;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.GameMessage, message);
        }

        public virtual uint RequestCashShopInfo(AckMessageCallback callback)
        {
            BaseAckMessage message = new BaseAckMessage();
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MsgTypes.CashShopInfo, message, callback);
        }

        public virtual uint RequestCashPackageInfo(AckMessageCallback callback)
        {
            BaseAckMessage message = new BaseAckMessage();
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MsgTypes.CashPackageInfo, message, callback);
        }

        public virtual uint RequestCashShopBuy(int dataId, AckMessageCallback callback)
        {
            RequestCashShopBuyMessage message = new RequestCashShopBuyMessage();
            message.dataId = dataId;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MsgTypes.CashShopBuy, message, callback);
        }

        public virtual uint RequestCashPackageBuyValidation(int dataId, string receipt, AckMessageCallback callback)
        {
            RequestCashPackageBuyValidationMessage message = new RequestCashPackageBuyValidationMessage();
            message.dataId = dataId;
            message.platform = Application.platform;
            message.receipt = receipt;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MsgTypes.CashPackageBuyValidation, message, callback);
        }

        protected virtual void HandleGameMessageAtClient(LiteNetLibMessageHandler messageHandler)
        {
            GameMessage message = messageHandler.ReadMessage<GameMessage>();
            if (onClientReceiveGameMessage != null)
                onClientReceiveGameMessage.Invoke(message);
        }

        protected virtual void HandleWarpAtClient(LiteNetLibMessageHandler messageHandler)
        {
            // TODO: May fade black when warping
        }

        protected virtual void HandleChatAtClient(LiteNetLibMessageHandler messageHandler)
        {
            ChatMessage message = messageHandler.ReadMessage<ChatMessage>();
            if (onClientReceiveChat != null)
                onClientReceiveChat.Invoke(message);
        }

        protected virtual void HandleResponseCashShopInfo(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseCashShopInfoMessage message = messageHandler.ReadMessage<ResponseCashShopInfoMessage>();
            transportHandler.TriggerAck(message.ackId, message.responseCode, message);
        }

        protected virtual void HandleResponseCashShopBuy(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseCashShopBuyMessage message = messageHandler.ReadMessage<ResponseCashShopBuyMessage>();
            transportHandler.TriggerAck(message.ackId, message.responseCode, message);
        }

        protected virtual void HandleResponseCashPackageInfo(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseCashPackageInfoMessage message = messageHandler.ReadMessage<ResponseCashPackageInfoMessage>();
            transportHandler.TriggerAck(message.ackId, message.responseCode, message);
        }

        protected virtual void HandleResponseCashPackageBuyValidation(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseCashPackageBuyValidationMessage message = messageHandler.ReadMessage<ResponseCashPackageBuyValidationMessage>();
            transportHandler.TriggerAck(message.ackId, message.responseCode, message);
        }

        protected virtual void HandleUpdatePartyMemberAtClient(LiteNetLibMessageHandler messageHandler)
        {
            UpdateSocialGroupMember(ClientParty, messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            if (onClientUpdateParty != null)
                onClientUpdateParty.Invoke(ClientParty);
        }

        protected virtual void HandleUpdatePartyAtClient(LiteNetLibMessageHandler messageHandler)
        {
            UpdatePartyMessage message = messageHandler.ReadMessage<UpdatePartyMessage>();
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
            UpdateGuildMessage message = messageHandler.ReadMessage<UpdateGuildMessage>();
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
                    case UpdateGuildMessage.UpdateType.SetSkillLevel:
                        ClientGuild.SetSkillLevel(message.dataId, message.level);
                        if (BasePlayerCharacterController.OwningCharacter != null)
                            BasePlayerCharacterController.OwningCharacter.ForceMakeCaches();
                        break;
                    case UpdateGuildMessage.UpdateType.LevelExpSkillPoint:
                        ClientGuild.level = message.level;
                        ClientGuild.exp = message.exp;
                        ClientGuild.skillPoint = message.skillPoint;
                        break;
                    case UpdateGuildMessage.UpdateType.Terminate:
                        ClientGuild = null;
                        if (BasePlayerCharacterController.OwningCharacter != null)
                            BasePlayerCharacterController.OwningCharacter.ForceMakeCaches();
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
                case ChatChannel.Local:
                    if (!string.IsNullOrEmpty(message.sender) &&
                        TryGetPlayerCharacterByName(message.sender, out playerCharacter))
                    {
                        // TODO: Don't use fixed user level
                        if (GMCommands.IsGMCommand(message.message) && playerCharacter.UserLevel > 0)
                        {
                            // If it's gm command and sender's user level > 0, handle gm commands
                            HandleGMCommand(message.sender, message.message);
                        }
                        else
                        {
                            // Send messages to nearby characters
                            List<BasePlayerCharacterEntity> receivers = playerCharacter.FindCharacters<BasePlayerCharacterEntity>(gameInstance.localChatDistance, false, true, true, true);
                            foreach (BasePlayerCharacterEntity receiver in receivers)
                            {
                                ServerSendPacket(receiver.ConnectionId, SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                            }
                            // Send messages to sender
                            ServerSendPacket(playerCharacter.ConnectionId, SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                        }
                    }
                    break;
                case ChatChannel.Global:
                    if (!string.IsNullOrEmpty(message.sender))
                    {
                        // Send message to all clients
                        ServerSendPacketToAllConnections(SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                    }
                    break;
                case ChatChannel.Whisper:
                    if (!string.IsNullOrEmpty(message.sender) &&
                        connectionIdsByCharacterName.TryGetValue(message.sender, out senderConnectionId))
                    {
                        // If found sender send whisper message to sender
                        ServerSendPacket(senderConnectionId, SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                    }
                    if (!string.IsNullOrEmpty(message.receiver) &&
                        connectionIdsByCharacterName.TryGetValue(message.receiver, out receiverConnectionId))
                    {
                        // If found receiver send whisper message to receiver
                        ServerSendPacket(receiverConnectionId, SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                    }
                    break;
                case ChatChannel.Party:
                    PartyData party;
                    if (parties.TryGetValue(message.channelId, out party))
                    {
                        foreach (string memberId in party.GetMemberIds())
                        {
                            if (TryGetPlayerCharacterById(memberId, out playerCharacter) &&
                                ContainsConnectionId(playerCharacter.ConnectionId))
                            {
                                // If party member is online, send party message to the member
                                ServerSendPacket(playerCharacter.ConnectionId, SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                            }
                        }
                    }
                    break;
                case ChatChannel.Guild:
                    GuildData guild;
                    if (guilds.TryGetValue(message.channelId, out guild))
                    {
                        foreach (string memberId in guild.GetMemberIds())
                        {
                            if (TryGetPlayerCharacterById(memberId, out playerCharacter) &&
                                ContainsConnectionId(playerCharacter.ConnectionId))
                            {
                                // If guild member is online, send guild message to the member
                                ServerSendPacket(playerCharacter.ConnectionId, SendOptions.ReliableOrdered, MsgTypes.Chat, message);
                            }
                        }
                    }
                    break;
            }
        }

        protected virtual void HandleRequestCashShopInfo(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            BaseAckMessage message = messageHandler.ReadMessage<BaseAckMessage>();
            ResponseCashShopInfoMessage responseMessage = new ResponseCashShopInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Error;
            responseMessage.error = ResponseCashShopInfoMessage.Error.NotAvailable;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.CashShopInfo, responseMessage);
        }

        protected virtual void HandleRequestCashShopBuy(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestCashShopBuyMessage message = messageHandler.ReadMessage<RequestCashShopBuyMessage>();
            ResponseCashShopBuyMessage responseMessage = new ResponseCashShopBuyMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Error;
            responseMessage.error = ResponseCashShopBuyMessage.Error.NotAvailable;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.CashShopBuy, responseMessage);
        }

        protected virtual void HandleRequestCashPackageInfo(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            BaseAckMessage message = messageHandler.ReadMessage<BaseAckMessage>();
            ResponseCashPackageInfoMessage responseMessage = new ResponseCashPackageInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Error;
            responseMessage.error = ResponseCashPackageInfoMessage.Error.NotAvailable;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.CashPackageInfo, responseMessage);
        }

        protected virtual void HandleRequestCashPackageBuyValidation(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestCashPackageBuyValidationMessage message = messageHandler.ReadMessage<RequestCashPackageBuyValidationMessage>();
            ResponseCashPackageBuyValidationMessage responseMessage = new ResponseCashPackageBuyValidationMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Error;
            responseMessage.error = ResponseCashPackageBuyValidationMessage.Error.NotAvailable;
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
            List<LiteNetLibIdentity> spawnablePrefabs = new List<LiteNetLibIdentity>(Assets.spawnablePrefabs);
            if (gameInstance.itemDropEntityPrefab != null)
                spawnablePrefabs.Add(gameInstance.itemDropEntityPrefab.Identity);
            if (gameInstance.warpPortalEntityPrefab != null)
                spawnablePrefabs.Add(gameInstance.warpPortalEntityPrefab.Identity);
            foreach (KeyValuePair<int, BasePlayerCharacterEntity> entry in GameInstance.PlayerCharacterEntities)
            {
                spawnablePrefabs.Add(entry.Value.Identity);
            }
            foreach (KeyValuePair<int, BaseMonsterCharacterEntity> entry in GameInstance.MonsterCharacterEntities)
            {
                spawnablePrefabs.Add(entry.Value.Identity);
            }
            foreach (KeyValuePair<int, WarpPortalEntity> entry in GameInstance.WarpPortalEntities)
            {
                spawnablePrefabs.Add(entry.Value.Identity);
            }
            foreach (KeyValuePair<int, NpcEntity> entry in GameInstance.NpcEntities)
            {
                spawnablePrefabs.Add(entry.Value.Identity);
            }
            foreach (KeyValuePair<int, BaseDamageEntity> entry in GameInstance.DamageEntities)
            {
                spawnablePrefabs.Add(entry.Value.Identity);
            }
            foreach (KeyValuePair<int, BuildingEntity> entry in GameInstance.BuildingEntities)
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
            ChatMessage chatMessage = new ChatMessage();
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
            MonsterSpawnArea[] monsterSpawnAreas = FindObjectsOfType<MonsterSpawnArea>();
            foreach (MonsterSpawnArea monsterSpawnArea in monsterSpawnAreas)
            {
                monsterSpawnArea.RegisterAssets();
            }

            HarvestableSpawnArea[] harvestableSpawnAreas = FindObjectsOfType<HarvestableSpawnArea>();
            foreach (HarvestableSpawnArea harvestableSpawnArea in harvestableSpawnAreas)
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
                    socialGroupData.AddMember(message.data);
                    break;
                case UpdateSocialMemberMessage.UpdateType.Update:
                    socialGroupData.UpdateMember(message.data);
                    if (message.isOnline)
                        socialGroupData.NotifyOnlineMember(message.CharacterId);
                    break;
                case UpdateSocialMemberMessage.UpdateType.Remove:
                    socialGroupData.RemoveMember(message.CharacterId);
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
            MonsterSpawnArea[] monsterSpawnAreas = FindObjectsOfType<MonsterSpawnArea>();
            foreach (MonsterSpawnArea monsterSpawnArea in monsterSpawnAreas)
            {
                monsterSpawnArea.SpawnAll();
            }
            // Spawn Warp Portals
            if (GameInstance.MapWarpPortals.Count > 0)
            {
                List<WarpPortal> mapWarpPortals;
                if (GameInstance.MapWarpPortals.TryGetValue(SceneManager.GetActiveScene().name, out mapWarpPortals))
                {
                    foreach (WarpPortal warpPortal in mapWarpPortals)
                    {
                        WarpPortalEntity warpPortalPrefab = warpPortal.entityPrefab != null ? warpPortal.entityPrefab : gameInstance.warpPortalEntityPrefab;
                        if (warpPortalPrefab != null)
                        {
                            LiteNetLibIdentity warpPortalIdentity = Assets.NetworkSpawn(warpPortalPrefab.Identity, warpPortal.position, Quaternion.identity);
                            WarpPortalEntity warpPortalEntity = warpPortalIdentity.GetComponent<WarpPortalEntity>();
                            warpPortalEntity.warpPortalType = warpPortal.warpPortalType;
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
                if (GameInstance.MapNpcs.TryGetValue(SceneManager.GetActiveScene().name, out mapNpcs))
                {
                    foreach (Npc npc in mapNpcs)
                    {
                        NpcEntity npcPrefab = npc.entityPrefab;
                        if (npcPrefab != null)
                        {
                            LiteNetLibIdentity npcIdentity = Assets.NetworkSpawn(npcPrefab.Identity, npc.position, Quaternion.Euler(npc.rotation));
                            NpcEntity npcEntity = npcIdentity.GetComponent<NpcEntity>();
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
                LiteNetLibIdentity buildingIdentity = Assets.NetworkSpawn(prefab.Identity, saveData.Position, saveData.Rotation);
                BuildingEntity buildingEntity = buildingIdentity.GetComponent<BuildingEntity>();
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

        public bool TryGetBuildingEntity(string id, out BuildingEntity entity)
        {
            return buildingEntities.TryGetValue(id, out entity);
        }
    }
}
