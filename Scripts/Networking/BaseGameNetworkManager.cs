using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            public const ushort UpdateMapInfo = 110;
            public const ushort UpdateFoundCharacters = 111;
            public const ushort UpdateFriends = 112;
            public const ushort NotifyOnlineCharacter = 113;
        }

        public const float UPDATE_ONLINE_CHARACTER_DURATION = 1f;
        public const string INSTANTIATES_OBJECTS_DELAY_STATE_KEY = "INSTANTIATES_OBJECTS_DELAY";
        public const float INSTANTIATES_OBJECTS_DELAY = 0.5f;

        public static BaseGameNetworkManager Singleton { get; protected set; }
        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }
        protected static readonly Dictionary<long, BasePlayerCharacterEntity> playerCharacters = new Dictionary<long, BasePlayerCharacterEntity>();
        protected static readonly Dictionary<string, BasePlayerCharacterEntity> playerCharactersById = new Dictionary<string, BasePlayerCharacterEntity>();
        protected static readonly Dictionary<string, BuildingEntity> buildingEntities = new Dictionary<string, BuildingEntity>();
        protected static readonly Dictionary<string, long> connectionIdsByCharacterName = new Dictionary<string, long>();
        protected static readonly Dictionary<int, PartyData> parties = new Dictionary<int, PartyData>();
        protected static readonly Dictionary<int, GuildData> guilds = new Dictionary<int, GuildData>();
        protected static readonly Dictionary<long, PartyData> updatingPartyMembers = new Dictionary<long, PartyData>();
        protected static readonly Dictionary<long, GuildData> updatingGuildMembers = new Dictionary<long, GuildData>();
        protected static readonly Dictionary<string, NotifyOnlineCharacterTime> lastCharacterOnlineTimes = new Dictionary<string, NotifyOnlineCharacterTime>();
        /// <summary>
        /// This dictionary will be cleared in `OnServerOnlineSceneLoaded`
        /// </summary>
        protected static readonly Dictionary<string, bool> readyToInstantiateObjectsStates = new Dictionary<string, bool>();
        /// <summary>
        /// * This value will be `TRUE` when all values in `readyToInstantiateObjectsStates` are `TRUE`<para />
        /// * The manager will not validate values in `readyToInstantiateObjectsStates` after this value was `TRUE`<para />
        /// * This value will reset to `FALSE` in `OnServerOnlineSceneLoaded`<para />
        /// </summary>
        protected static bool isReadyToInstantiateObjects;
        public static PartyData ClientParty { get; protected set; }
        public static GuildData ClientGuild { get; protected set; }
        public static readonly SocialGroupData ClientFoundCharacters = new SocialGroupData(1);
        public static readonly SocialGroupData ClientFriends = new SocialGroupData(1);
        public static MapInfo CurrentMapInfo { get; protected set; }
        // Events
        public System.Action<ChatMessage> onClientReceiveChat;
        public System.Action<GameMessage> onClientReceiveGameMessage;
        public System.Action<PartyData> onClientUpdateParty;
        public System.Action<GuildData> onClientUpdateGuild;
        public System.Action<SocialGroupData> onClientUpdateFoundCharacters;
        public System.Action<SocialGroupData> onClientUpdateFriends;
        protected float lastUpdateOnlineCharacterTime;
        protected float serverSceneLoadedTime;

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
                    UpdateOnlineCharacters();
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
            RegisterClientMessage(MsgTypes.UpdateMapInfo, HandleUpdateMapInfoAtClient);
            RegisterClientMessage(MsgTypes.UpdateFoundCharacters, HandleUpdateFoundCharactersAtClient);
            RegisterClientMessage(MsgTypes.UpdateFriends, HandleUpdateFriendsAtClient);
            RegisterClientMessage(MsgTypes.NotifyOnlineCharacter, HandleNotifyOnlineCharacterAtClient);
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
            RegisterServerMessage(MsgTypes.NotifyOnlineCharacter, HandleRequestOnlineCharacter);
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
            lastCharacterOnlineTimes.Clear();
            ClientParty = null;
            ClientGuild = null;
            ClientFoundCharacters.ClearMembers();
            ClientFriends.ClearMembers();
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

        public override void OnStartClient(LiteNetLibClient client)
        {
            this.InvokeInstanceDevExtMethods("OnStartClient", client);
            base.OnStartClient(client);
        }

        public override void OnStopClient()
        {
            if (!IsServer)
                Clean();
            base.OnStopClient();
        }

        public override void OnPeerConnected(long connectionId)
        {
            base.OnPeerConnected(connectionId);
            SendMapInfo(connectionId);
        }

        public static void NotifyOnlineCharacter(string characterId)
        {
            NotifyOnlineCharacterTime notifyTime;
            if (!lastCharacterOnlineTimes.TryGetValue(characterId, out notifyTime))
            {
                lastCharacterOnlineTimes.Add(characterId, new NotifyOnlineCharacterTime()
                {
                    lastNotifyTime = Time.unscaledTime
                });
            }
            else
            {
                notifyTime.lastNotifyTime = Time.unscaledTime;
                lastCharacterOnlineTimes[characterId] = notifyTime;
            }
        }

        public static void RequestOnlineCharacter(string characterId)
        {
            if (Singleton == null || Singleton.IsServer || !Singleton.IsClientConnected)
                return;

            float unscaledTime = Time.unscaledTime;
            NotifyOnlineCharacterTime notifyTime;
            if (!lastCharacterOnlineTimes.TryGetValue(characterId, out notifyTime))
            {
                lastCharacterOnlineTimes.Add(characterId, new NotifyOnlineCharacterTime()
                {
                    lastRequestTime = unscaledTime
                });
            }
            else
            {
                if (unscaledTime - notifyTime.lastRequestTime < 1.5f)
                    return;

                notifyTime.lastRequestTime = unscaledTime;
                lastCharacterOnlineTimes[characterId] = notifyTime;
            }

            StringMessage msg = new StringMessage();
            msg.value = characterId;
            Singleton.ClientSendPacket(DeliveryMethod.ReliableOrdered, MsgTypes.NotifyOnlineCharacter, msg);
        }

        public static bool IsCharacterOnline(string characterId)
        {
            NotifyOnlineCharacterTime notifyTime;
            return lastCharacterOnlineTimes.TryGetValue(characterId, out notifyTime) &&
                Time.unscaledTime - notifyTime.lastNotifyTime <= 2f;
        }

        protected virtual void UpdateOnlineCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            NotifyOnlineCharacter(playerCharacterEntity.Id);
        }

        protected virtual void UpdateOnlineCharacters()
        {
            updatingPartyMembers.Clear();
            updatingGuildMembers.Clear();

            PartyData tempParty;
            GuildData tempGuild;
            foreach (BasePlayerCharacterEntity playerCharacter in playerCharacters.Values)
            {
                UpdateOnlineCharacter(playerCharacter);

                if (playerCharacter.PartyId > 0 && parties.TryGetValue(playerCharacter.PartyId, out tempParty))
                {
                    tempParty.UpdateMember(playerCharacter);
                    if (!updatingPartyMembers.ContainsKey(playerCharacter.ConnectionId))
                        updatingPartyMembers.Add(playerCharacter.ConnectionId, tempParty);
                }

                if (playerCharacter.GuildId > 0 && guilds.TryGetValue(playerCharacter.GuildId, out tempGuild))
                {
                    tempGuild.UpdateMember(playerCharacter);
                    if (!updatingGuildMembers.ContainsKey(playerCharacter.ConnectionId))
                        updatingGuildMembers.Add(playerCharacter.ConnectionId, tempGuild);
                }
            }

            foreach (long connectionId in updatingPartyMembers.Keys)
            {
                SendUpdatePartyMembersToClient(connectionId, updatingPartyMembers[connectionId]);
            }

            foreach (long connectionId in updatingGuildMembers.Keys)
            {
                SendUpdateGuildMembersToClient(connectionId, updatingGuildMembers[connectionId]);
            }
        }

        public virtual void SendServerGameMessage(long connectionId, GameMessage.Type type)
        {
            GameMessage message = new GameMessage();
            message.type = type;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.GameMessage, message);
        }

        public virtual uint RequestCashShopInfo(AckMessageCallback callback)
        {
            BaseAckMessage message = new BaseAckMessage();
            return Client.ClientSendAckPacket(DeliveryMethod.ReliableOrdered, MsgTypes.CashShopInfo, message, callback);
        }

        public virtual uint RequestCashPackageInfo(AckMessageCallback callback)
        {
            BaseAckMessage message = new BaseAckMessage();
            return Client.ClientSendAckPacket(DeliveryMethod.ReliableOrdered, MsgTypes.CashPackageInfo, message, callback);
        }

        public virtual uint RequestCashShopBuy(int dataId, AckMessageCallback callback)
        {
            RequestCashShopBuyMessage message = new RequestCashShopBuyMessage();
            message.dataId = dataId;
            return Client.ClientSendAckPacket(DeliveryMethod.ReliableOrdered, MsgTypes.CashShopBuy, message, callback);
        }

        public virtual uint RequestCashPackageBuyValidation(int dataId, string receipt, AckMessageCallback callback)
        {
            RequestCashPackageBuyValidationMessage message = new RequestCashPackageBuyValidationMessage();
            message.dataId = dataId;
            message.platform = Application.platform;
            message.receipt = receipt;
            return Client.ClientSendAckPacket(DeliveryMethod.ReliableOrdered, MsgTypes.CashPackageBuyValidation, message, callback);
        }

        protected virtual void HandleGameMessageAtClient(LiteNetLibMessageHandler messageHandler)
        {
            GameMessage message = messageHandler.ReadMessage<GameMessage>();
            ClientReceiveGameMessage(message);
        }

        public void ClientReceiveGameMessage(GameMessage message)
        {
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
                    case UpdateGuildMessage.UpdateType.SetGold:
                        ClientGuild.gold = message.gold;
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

        protected virtual void HandleUpdateMapInfoAtClient(LiteNetLibMessageHandler messageHandler)
        {
            // Don't set map info again at server
            if (IsServer)
                return;
            UpdateMapInfoMessage message = messageHandler.ReadMessage<UpdateMapInfoMessage>();
            SetMapInfo(message.mapId);
        }

        protected virtual void HandleUpdateFoundCharactersAtClient(LiteNetLibMessageHandler messageHandler)
        {
            UpdateSocialMembersMessage msg = messageHandler.ReadMessage<UpdateSocialMembersMessage>();
            ClientFoundCharacters.ClearMembers();
            foreach (SocialCharacterData member in msg.members)
            {
                ClientFoundCharacters.AddMember(member);
            }
            if (onClientUpdateFoundCharacters != null)
                onClientUpdateFoundCharacters.Invoke(ClientFoundCharacters);
        }

        protected virtual void HandleUpdateFriendsAtClient(LiteNetLibMessageHandler messageHandler)
        {
            UpdateSocialMembersMessage msg = messageHandler.ReadMessage<UpdateSocialMembersMessage>();
            ClientFriends.ClearMembers();
            foreach (SocialCharacterData member in msg.members)
            {
                ClientFriends.AddMember(member);
            }
            if (onClientUpdateFriends != null)
                onClientUpdateFriends.Invoke(ClientFriends);
        }

        protected virtual void HandleNotifyOnlineCharacterAtClient(LiteNetLibMessageHandler messageHandler)
        {
            StringMessage msg = messageHandler.ReadMessage<StringMessage>();
            NotifyOnlineCharacter(msg.value);
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
                                ServerSendPacket(receiver.ConnectionId, DeliveryMethod.ReliableOrdered, MsgTypes.Chat, message);
                            }
                            // Send messages to sender
                            ServerSendPacket(playerCharacter.ConnectionId, DeliveryMethod.ReliableOrdered, MsgTypes.Chat, message);
                        }
                    }
                    break;
                case ChatChannel.Global:
                    if (!string.IsNullOrEmpty(message.sender))
                    {
                        // Send message to all clients
                        ServerSendPacketToAllConnections(DeliveryMethod.ReliableOrdered, MsgTypes.Chat, message);
                    }
                    break;
                case ChatChannel.Whisper:
                    if (!string.IsNullOrEmpty(message.sender) &&
                        connectionIdsByCharacterName.TryGetValue(message.sender, out senderConnectionId))
                    {
                        // If found sender send whisper message to sender
                        ServerSendPacket(senderConnectionId, DeliveryMethod.ReliableOrdered, MsgTypes.Chat, message);
                    }
                    if (!string.IsNullOrEmpty(message.receiver) &&
                        connectionIdsByCharacterName.TryGetValue(message.receiver, out receiverConnectionId))
                    {
                        // If found receiver send whisper message to receiver
                        ServerSendPacket(receiverConnectionId, DeliveryMethod.ReliableOrdered, MsgTypes.Chat, message);
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
                                ServerSendPacket(playerCharacter.ConnectionId, DeliveryMethod.ReliableOrdered, MsgTypes.Chat, message);
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
                                ServerSendPacket(playerCharacter.ConnectionId, DeliveryMethod.ReliableOrdered, MsgTypes.Chat, message);
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
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.CashShopInfo, responseMessage);
        }

        protected virtual void HandleRequestCashShopBuy(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestCashShopBuyMessage message = messageHandler.ReadMessage<RequestCashShopBuyMessage>();
            ResponseCashShopBuyMessage responseMessage = new ResponseCashShopBuyMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Error;
            responseMessage.error = ResponseCashShopBuyMessage.Error.NotAvailable;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.CashShopBuy, responseMessage);
        }

        protected virtual void HandleRequestCashPackageInfo(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            BaseAckMessage message = messageHandler.ReadMessage<BaseAckMessage>();
            ResponseCashPackageInfoMessage responseMessage = new ResponseCashPackageInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Error;
            responseMessage.error = ResponseCashPackageInfoMessage.Error.NotAvailable;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.CashPackageInfo, responseMessage);
        }

        protected virtual void HandleRequestCashPackageBuyValidation(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestCashPackageBuyValidationMessage message = messageHandler.ReadMessage<RequestCashPackageBuyValidationMessage>();
            ResponseCashPackageBuyValidationMessage responseMessage = new ResponseCashPackageBuyValidationMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Error;
            responseMessage.error = ResponseCashPackageBuyValidationMessage.Error.NotAvailable;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.CashPackageBuyValidation, responseMessage);
        }

        protected virtual void HandleRequestOnlineCharacter(LiteNetLibMessageHandler messageHandler)
        {
            StringMessage msg = messageHandler.ReadMessage<StringMessage>();
            if (IsCharacterOnline(msg.value))
            {
                // Notify back online character
                ServerSendPacket(messageHandler.connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.NotifyOnlineCharacter, msg);
            }
        }

        public override bool StartServer()
        {
            Init();
            return base.StartServer();
        }

        public override LiteNetLibClient StartClient(string networkAddress, int networkPort)
        {
            Init();
            return base.StartClient(networkAddress, networkPort);
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
            foreach (BaseCharacterEntity entry in GameInstance.CharacterEntities.Values)
            {
                spawnablePrefabs.Add(entry.Identity);
            }
            foreach (MountEntity entry in GameInstance.MountEntities.Values)
            {
                spawnablePrefabs.Add(entry.Identity);
            }
            foreach (WarpPortalEntity entry in GameInstance.WarpPortalEntities.Values)
            {
                spawnablePrefabs.Add(entry.Identity);
            }
            foreach (NpcEntity entry in GameInstance.NpcEntities.Values)
            {
                spawnablePrefabs.Add(entry.Identity);
            }
            foreach (BuildingEntity entry in GameInstance.BuildingEntities.Values)
            {
                spawnablePrefabs.Add(entry.Identity);
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
            ClientSendPacket(DeliveryMethod.ReliableOrdered, MsgTypes.Chat, chatMessage);
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
                    break;
                case UpdateSocialMemberMessage.UpdateType.Remove:
                    socialGroupData.RemoveMember(message.data.id);
                    break;
                case UpdateSocialMemberMessage.UpdateType.Clear:
                    socialGroupData.ClearMembers();
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
                RegisterEntities();
        }

        public override void OnServerOnlineSceneLoaded()
        {
            base.OnServerOnlineSceneLoaded();
            readyToInstantiateObjectsStates.Clear();
            isReadyToInstantiateObjects = false;
            serverSceneLoadedTime = Time.unscaledTime;
            this.InvokeInstanceDevExtMethods("OnServerOnlineSceneLoaded");
            StartCoroutine(OnServerOnlineSceneLoaded_SpawnEntitiesRoutine());
        }

        private IEnumerator OnServerOnlineSceneLoaded_SpawnEntitiesRoutine()
        {
            while (!IsReadyToInstantiateObjects())
            {
                yield return null;
            }
            RegisterEntities();
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
                if (GameInstance.MapWarpPortals.TryGetValue(CurrentMapInfo.Id, out mapWarpPortals))
                {
                    foreach (WarpPortal warpPortal in mapWarpPortals)
                    {
                        WarpPortalEntity warpPortalPrefab = warpPortal.entityPrefab != null ? warpPortal.entityPrefab : gameInstance.warpPortalEntityPrefab;
                        if (warpPortalPrefab != null)
                        {
                            GameObject spawnObj = Instantiate(warpPortalPrefab.gameObject, warpPortal.position, Quaternion.identity);
                            WarpPortalEntity warpPortalEntity = spawnObj.GetComponent<WarpPortalEntity>();
                            warpPortalEntity.type = warpPortal.warpPortalType;
                            warpPortalEntity.mapInfo = warpPortal.warpToMapInfo;
                            warpPortalEntity.position = warpPortal.warpToPosition;
                            Assets.NetworkSpawn(spawnObj);
                        }
                    }
                }
            }
            // Spawn Npcs
            if (GameInstance.MapNpcs.Count > 0)
            {
                List<Npc> mapNpcs;
                if (GameInstance.MapNpcs.TryGetValue(CurrentMapInfo.Id, out mapNpcs))
                {
                    foreach (Npc npc in mapNpcs)
                    {
                        NpcEntity npcPrefab = npc.entityPrefab;
                        if (npcPrefab != null)
                        {
                            GameObject spawnObj = Instantiate(npcPrefab.gameObject, npc.position, Quaternion.Euler(npc.rotation));
                            NpcEntity npcEntity = spawnObj.GetComponent<NpcEntity>();
                            npcEntity.Title = npc.title;
                            npcEntity.StartDialog = npc.startDialog;
                            npcEntity.Graph = npc.graph;
                            Assets.NetworkSpawn(spawnObj);
                        }
                    }
                }
            }
            // If it's server (not host) spawn simple camera controller
            if (!IsClient && GameInstance.Singleton.serverCharacterPrefab != null)
                Instantiate(GameInstance.Singleton.serverCharacterPrefab);
        }

        public virtual void RegisterPlayerCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !ConnectionIds.Contains(playerCharacterEntity.ConnectionId) || playerCharacters.ContainsKey(playerCharacterEntity.ConnectionId))
                return;
            playerCharacters[playerCharacterEntity.ConnectionId] = playerCharacterEntity;
            playerCharactersById[playerCharacterEntity.Id] = playerCharacterEntity;
            connectionIdsByCharacterName[playerCharacterEntity.CharacterName] = playerCharacterEntity.ConnectionId;
        }

        public virtual void UnregisterPlayerCharacter(long connectionId)
        {
            BasePlayerCharacterEntity playerCharacter;
            if (!playerCharacters.TryGetValue(connectionId, out playerCharacter))
                return;
            CloseStorage(playerCharacter);
            connectionIdsByCharacterName.Remove(playerCharacter.CharacterName);
            playerCharactersById.Remove(playerCharacter.Id);
            playerCharacters.Remove(connectionId);
        }

        public virtual BuildingEntity CreateBuildingEntity(BuildingSaveData saveData, bool initialize)
        {
            BuildingEntity prefab;
            if (GameInstance.BuildingEntities.TryGetValue(saveData.DataId, out prefab))
            {
                GameObject spawnObj = Instantiate(prefab.gameObject, saveData.position, saveData.Rotation);
                BuildingEntity buildingEntity = spawnObj.GetComponent<BuildingEntity>();
                buildingEntity.Id = saveData.Id;
                buildingEntity.ParentId = saveData.ParentId;
                buildingEntity.CurrentHp = saveData.CurrentHp;
                buildingEntity.CreatorId = saveData.CreatorId;
                buildingEntity.CreatorName = saveData.CreatorName;
                Assets.NetworkSpawn(spawnObj);
                buildingEntities[buildingEntity.Id] = buildingEntity;
                return buildingEntity;
            }
            return null;
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

        public void SetMapInfo(string mapId)
        {
            MapInfo tempMapInfo;
            if (GameInstance.MapInfos.TryGetValue(mapId, out tempMapInfo))
                SetMapInfo(tempMapInfo);
        }

        public void SetMapInfo(MapInfo mapInfo)
        {
            if (mapInfo == null)
                return;
            CurrentMapInfo = mapInfo;
            SendMapInfo();
        }

        public void SendMapInfo()
        {
            if (!IsServer)
                return;
            foreach (long connectionId in ConnectionIds)
            {
                SendMapInfo(connectionId);
            }
        }

        public void SendMapInfo(long connectionId)
        {
            if (!IsServer || CurrentMapInfo == null)
                return;
            UpdateMapInfoMessage message = new UpdateMapInfoMessage();
            message.mapId = CurrentMapInfo.Id;
            ServerSendPacketToAllConnections(DeliveryMethod.ReliableOrdered, MsgTypes.UpdateMapInfo, message);
        }

        public bool IsReadyToInstantiateObjects()
        {
            if (!isReadyToInstantiateObjects)
            {
                readyToInstantiateObjectsStates[INSTANTIATES_OBJECTS_DELAY_STATE_KEY] = Time.unscaledTime - serverSceneLoadedTime >= INSTANTIATES_OBJECTS_DELAY;
                this.InvokeInstanceDevExtMethods("UpdateReadyToInstantiateObjectsStates");
                foreach (bool value in readyToInstantiateObjectsStates.Values)
                {
                    if (!value)
                        return false;
                }
                isReadyToInstantiateObjects = true;
            }
            return true;
        }
    }
}
