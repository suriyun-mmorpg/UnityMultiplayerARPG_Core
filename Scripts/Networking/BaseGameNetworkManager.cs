using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;
using LiteNetLibManager.SuperGrid2D;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameNetworkManager : LiteNetLibGameManager
    {
        public class MsgTypes
        {
            public const ushort GameMessage = 100;
            public const ushort Warp = 101;
            public const ushort Chat = 102;
            public const ushort UpdatePartyMember = 106;
            public const ushort UpdateParty = 107;
            public const ushort UpdateGuildMember = 108;
            public const ushort UpdateGuild = 109;
            public const ushort UpdateMapInfo = 110;
            public const ushort UpdateFoundCharacters = 111;
            public const ushort UpdateFriends = 112;
            public const ushort NotifyOnlineCharacter = 113;
            public const ushort NotifyRewardExp = 114;
            public const ushort NotifyRewardGold = 115;
            public const ushort NotifyRewardItem = 116;
            public const ushort UpdateTimeOfDay = 117;
        }

        public class ReqTypes
        {
            public const ushort CashShopInfo = 100;
            public const ushort CashShopBuy = 101;
            public const ushort CashPackageInfo = 102;
            public const ushort CashPackageBuyValidation = 103;
            public const ushort MailList = 104;
            public const ushort ReadMail = 105;
            public const ushort ClaimMailItems = 106;
            public const ushort DeleteMail = 107;
            public const ushort SendMail = 108;
        }

        public const string CHAT_SYSTEM_ANNOUNCER_SENDER = "SYSTEM_ANNOUNCER";
        public const float UPDATE_ONLINE_CHARACTER_DURATION = 1f;
        public const float UPDATE_TIME_OF_DAY_DURATION = 5f;
        public const string INSTANTIATES_OBJECTS_DELAY_STATE_KEY = "INSTANTIATES_OBJECTS_DELAY";
        public const float INSTANTIATES_OBJECTS_DELAY = 0.5f;

        public static BaseGameNetworkManager Singleton { get; protected set; }
        protected GameInstance CurrentGameInstance { get { return GameInstance.Singleton; } }
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
        protected static bool isReadyToInstantiatePlayers;
        public static PartyData ClientParty { get; protected set; }
        public static GuildData ClientGuild { get; protected set; }
        public static readonly SocialGroupData ClientFoundCharacters = new SocialGroupData(1);
        public static readonly SocialGroupData ClientFriends = new SocialGroupData(1);
        public static BaseMapInfo CurrentMapInfo { get; protected set; }
        // Events
        public System.Action onClientWarp;
        public System.Action<ChatMessage> onClientReceiveChat;
        public System.Action<GameMessage> onClientReceiveGameMessage;
        public System.Action<PartyData> onClientUpdateParty;
        public System.Action<GuildData> onClientUpdateGuild;
        public System.Action<SocialGroupData> onClientUpdateFoundCharacters;
        public System.Action<SocialGroupData> onClientUpdateFriends;
        public System.Action<int> onNotifyRewardExp;
        public System.Action<int> onNotifyRewardGold;
        public System.Action<int, short> onNotifyRewardItem;
        protected float updateOnlineCharactersCountDown;
        protected float updateTimeOfDayCountDown;
        protected float serverSceneLoadedTime;
        // Spawn entities events
        public LiteNetLibLoadSceneEvent onSpawnEntitiesStart;
        public LiteNetLibLoadSceneEvent onSpawnEntitiesProgress;
        public LiteNetLibLoadSceneEvent onSpawnEntitiesFinish;

        public override uint PacketVersion()
        {
            return 4;
        }

        public Dictionary<long, BasePlayerCharacterEntity>.ValueCollection GetPlayerCharacters()
        {
            return playerCharacters.Values;
        }

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

        protected override void LateUpdate()
        {
            base.LateUpdate();
            float tempDeltaTime = Time.unscaledDeltaTime;
            if (IsServer)
            {
                updateOnlineCharactersCountDown -= tempDeltaTime;
                if (updateOnlineCharactersCountDown <= 0f)
                {
                    updateOnlineCharactersCountDown = UPDATE_ONLINE_CHARACTER_DURATION;
                    UpdateOnlineCharacters();
                }
                updateTimeOfDayCountDown -= tempDeltaTime;
                if (updateTimeOfDayCountDown <= 0f)
                {
                    updateTimeOfDayCountDown = UPDATE_TIME_OF_DAY_DURATION;
                    SendTimeOfDay();
                }
            }
            if (IsNetworkActive)
            {
                // Update day-night time on both client and server. It will sync from server some time to make sure that clients time of day won't very difference
                CurrentGameInstance.DayNightTimeUpdater.UpdateTimeOfDay(tempDeltaTime);
            }
        }

        protected override void RegisterClientMessages()
        {
            this.InvokeInstanceDevExtMethods("RegisterClientMessages");
            base.RegisterClientMessages();
            RegisterClientMessage(MsgTypes.GameMessage, HandleGameMessageAtClient);
            RegisterClientMessage(MsgTypes.Warp, HandleWarpAtClient);
            RegisterClientMessage(MsgTypes.Chat, HandleChatAtClient);
            RegisterClientMessage(MsgTypes.UpdatePartyMember, HandleUpdatePartyMemberAtClient);
            RegisterClientMessage(MsgTypes.UpdateParty, HandleUpdatePartyAtClient);
            RegisterClientMessage(MsgTypes.UpdateGuildMember, HandleUpdateGuildMemberAtClient);
            RegisterClientMessage(MsgTypes.UpdateGuild, HandleUpdateGuildAtClient);
            RegisterClientMessage(MsgTypes.UpdateMapInfo, HandleUpdateMapInfoAtClient);
            RegisterClientMessage(MsgTypes.UpdateFoundCharacters, HandleUpdateFoundCharactersAtClient);
            RegisterClientMessage(MsgTypes.UpdateFriends, HandleUpdateFriendsAtClient);
            RegisterClientMessage(MsgTypes.NotifyOnlineCharacter, HandleNotifyOnlineCharacterAtClient);
            RegisterClientMessage(MsgTypes.NotifyRewardExp, HandleNotifyRewardExpAtClient);
            RegisterClientMessage(MsgTypes.NotifyRewardGold, HandleNotifyRewardGoldAtClient);
            RegisterClientMessage(MsgTypes.NotifyRewardItem, HandleNotifyRewardItemAtClient);
            RegisterClientMessage(MsgTypes.UpdateTimeOfDay, HandleUpdateDayNightTimeAtClient);
            // Responses
            RegisterClientResponse<EmptyMessage, ResponseCashShopInfoMessage>(ReqTypes.CashShopInfo);
            RegisterClientResponse<EmptyMessage, ResponseCashPackageInfoMessage>(ReqTypes.CashPackageInfo);
            RegisterClientResponse<RequestCashShopBuyMessage, ResponseCashShopBuyMessage>(ReqTypes.CashShopBuy);
            RegisterClientResponse<RequestCashPackageBuyValidationMessage, ResponseCashPackageBuyValidationMessage>(ReqTypes.CashPackageBuyValidation);
            RegisterClientResponse<RequestMailListMessage, ResponseMailListMessage>(ReqTypes.MailList);
            RegisterClientResponse<RequestReadMailMessage, ResponseReadMailMessage>(ReqTypes.ReadMail);
            RegisterClientResponse<RequestClaimMailItemsMessage, ResponseClaimMailItemsMessage>(ReqTypes.ClaimMailItems);
            RegisterClientResponse<RequestDeleteMailMessage, ResponseDeleteMailMessage>(ReqTypes.DeleteMail);
            RegisterClientResponse<RequestSendMailMessage, ResponseSendMailMessage>(ReqTypes.SendMail);
        }

        protected override void RegisterServerMessages()
        {
            this.InvokeInstanceDevExtMethods("RegisterServerMessages");
            base.RegisterServerMessages();
            // Networking messages
            RegisterServerMessage(MsgTypes.Chat, HandleChatAtServer);
            RegisterServerMessage(MsgTypes.NotifyOnlineCharacter, HandleRequestOnlineCharacter);
            // Requests
            RegisterServerRequest<EmptyMessage, ResponseCashShopInfoMessage>(ReqTypes.CashShopInfo, HandleRequestCashShopInfo);
            RegisterServerRequest<EmptyMessage, ResponseCashPackageInfoMessage>(ReqTypes.CashPackageInfo, HandleRequestCashPackageInfo);
            RegisterServerRequest<RequestCashShopBuyMessage, ResponseCashShopBuyMessage>(ReqTypes.CashShopBuy, HandleRequestCashShopBuy);
            RegisterServerRequest<RequestCashPackageBuyValidationMessage, ResponseCashPackageBuyValidationMessage>(ReqTypes.CashPackageBuyValidation, HandleRequestCashPackageBuyValidation);
            RegisterServerRequest<RequestMailListMessage, ResponseMailListMessage>(ReqTypes.MailList, HandleRequestMailList);
            RegisterServerRequest<RequestReadMailMessage, ResponseReadMailMessage>(ReqTypes.ReadMail, HandleRequestReadMail);
            RegisterServerRequest<RequestClaimMailItemsMessage, ResponseClaimMailItemsMessage>(ReqTypes.ClaimMailItems, HandleRequestClaimMailItems);
            RegisterServerRequest<RequestDeleteMailMessage, ResponseDeleteMailMessage>(ReqTypes.DeleteMail, HandleRequestDeleteMail);
            RegisterServerRequest<RequestSendMailMessage, ResponseSendMailMessage>(ReqTypes.SendMail, HandleRequestSendMail);
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
            CurrentGameInstance.DayNightTimeUpdater.InitTimeOfDay(this);
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
            this.InvokeInstanceDevExtMethods("OnPeerConnected", connectionId);
            base.OnPeerConnected(connectionId);
            SendMapInfo(connectionId);
            SendTimeOfDay(connectionId);
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
            Singleton.ClientSendPacket(DeliveryMethod.ReliableOrdered, MsgTypes.NotifyOnlineCharacter, (writer) =>
            {
                writer.Put(characterId);
            });
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

        public virtual void SendNotifyRewardExp(long connectionId, int exp)
        {
            if (exp <= 0)
                return;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.NotifyRewardExp, (writer) =>
            {
                writer.Put(exp);
            });
        }

        public virtual void SendNotifyRewardGold(long connectionId, int gold)
        {
            if (gold <= 0)
                return;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.NotifyRewardGold, (writer) =>
            {
                writer.Put(gold);
            });
        }

        public virtual void SendNotifyRewardItem(long connectionId, int dataId, short amount)
        {
            if (amount <= 0)
                return;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.NotifyRewardItem, (writer) =>
            {
                writer.Put(dataId);
                writer.Put(amount);
            });
        }

        public virtual void SendServerGameMessage(long connectionId, GameMessage.Type type)
        {
            GameMessage message = new GameMessage();
            message.type = type;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.GameMessage, message);
        }

        public virtual bool RequestCashShopInfo(ResponseDelegate callback)
        {
            return ClientSendRequest(ReqTypes.CashShopInfo, new EmptyMessage(), responseDelegate: callback);
        }

        public virtual bool RequestCashPackageInfo(ResponseDelegate callback)
        {
            return ClientSendRequest(ReqTypes.CashPackageInfo, new EmptyMessage(), responseDelegate: callback);
        }

        public virtual bool RequestCashShopBuy(int dataId, ResponseDelegate callback)
        {
            return ClientSendRequest(ReqTypes.CashShopBuy, new RequestCashShopBuyMessage()
            {
                dataId = dataId,
            }, responseDelegate: callback);
        }

        public virtual bool RequestCashPackageBuyValidation(int dataId, string receipt, ResponseDelegate callback)
        {
            return ClientSendRequest(ReqTypes.CashPackageBuyValidation, new RequestCashPackageBuyValidationMessage()
            {
                dataId = dataId,
                platform = Application.platform,
                receipt = receipt,
            }, responseDelegate: callback);
        }

        protected virtual void HandleGameMessageAtClient(MessageHandlerData messageHandler)
        {
            GameMessage message = messageHandler.ReadMessage<GameMessage>();
            ClientReceiveGameMessage(message);
        }

        public void ClientReceiveGameMessage(GameMessage message)
        {
            if (onClientReceiveGameMessage != null)
                onClientReceiveGameMessage.Invoke(message);
        }

        protected virtual void HandleWarpAtClient(MessageHandlerData messageHandler)
        {
            if (onClientWarp != null)
                onClientWarp.Invoke();
        }

        protected virtual void HandleChatAtClient(MessageHandlerData messageHandler)
        {
            ChatMessage message = messageHandler.ReadMessage<ChatMessage>();
            if (onClientReceiveChat != null)
                onClientReceiveChat.Invoke(message);
        }

        protected virtual void HandleUpdatePartyMemberAtClient(MessageHandlerData messageHandler)
        {
            UpdateSocialGroupMember(ClientParty, messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            if (onClientUpdateParty != null)
                onClientUpdateParty.Invoke(ClientParty);
        }

        protected virtual void HandleUpdatePartyAtClient(MessageHandlerData messageHandler)
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

        protected virtual void HandleUpdateGuildMemberAtClient(MessageHandlerData messageHandler)
        {
            UpdateSocialGroupMember(ClientGuild, messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            if (onClientUpdateGuild != null)
                onClientUpdateGuild.Invoke(ClientGuild);
        }

        protected virtual void HandleUpdateGuildAtClient(MessageHandlerData messageHandler)
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

        protected virtual void HandleUpdateMapInfoAtClient(MessageHandlerData messageHandler)
        {
            // Don't set map info again at server
            if (IsServer)
                return;
            UpdateMapInfoMessage message = messageHandler.ReadMessage<UpdateMapInfoMessage>();
            SetMapInfo(message.mapId);
        }

        protected virtual void HandleUpdateDayNightTimeAtClient(MessageHandlerData messageHandler)
        {
            // Don't set time of day again at server
            if (IsServer)
                return;
            UpdateTimeOfDayMessage message = messageHandler.ReadMessage<UpdateTimeOfDayMessage>();
            CurrentGameInstance.DayNightTimeUpdater.SetTimeOfDay(message.timeOfDay);
        }

        protected virtual void HandleUpdateFoundCharactersAtClient(MessageHandlerData messageHandler)
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

        protected virtual void HandleUpdateFriendsAtClient(MessageHandlerData messageHandler)
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

        protected virtual void HandleNotifyOnlineCharacterAtClient(MessageHandlerData messageHandler)
        {
            NotifyOnlineCharacter(messageHandler.Reader.GetString());
        }

        protected virtual void HandleNotifyRewardExpAtClient(MessageHandlerData messageHandler)
        {
            if (onNotifyRewardExp != null)
                onNotifyRewardExp.Invoke(messageHandler.Reader.GetInt());
        }

        protected virtual void HandleNotifyRewardGoldAtClient(MessageHandlerData messageHandler)
        {
            if (onNotifyRewardGold != null)
                onNotifyRewardGold.Invoke(messageHandler.Reader.GetInt());
        }

        protected virtual void HandleNotifyRewardItemAtClient(MessageHandlerData messageHandler)
        {
            if (onNotifyRewardItem != null)
                onNotifyRewardItem.Invoke(messageHandler.Reader.GetInt(), messageHandler.Reader.GetShort());
        }

        protected virtual void HandleChatAtServer(MessageHandlerData messageHandler)
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
                        string gmCommand;
                        if (CurrentGameInstance.GMCommands.IsGMCommand(message.message, out gmCommand) &&
                            CurrentGameInstance.GMCommands.CanUseGMCommand(playerCharacter, gmCommand))
                        {
                            // If it's gm command and sender's user level > 0, handle gm commands
                            CurrentGameInstance.GMCommands.HandleGMCommand(this, message.sender, message.message);
                        }
                        else
                        {
                            // Send messages to nearby characters
                            List<BasePlayerCharacterEntity> receivers = playerCharacter.FindCharacters<BasePlayerCharacterEntity>(CurrentGameInstance.localChatDistance, false, true, true, true);
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
                case ChatChannel.System:
                    if (CanSendSystemAnnounce(message.sender))
                    {
                        // Send message to all clients
                        ServerSendPacketToAllConnections(DeliveryMethod.ReliableOrdered, MsgTypes.Chat, message);
                    }
                    break;
            }
        }

        protected virtual UniTaskVoid HandleRequestCashShopInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashShopInfoMessage> result)
        {
            result.Invoke(AckResponseCode.Error, new ResponseCashShopInfoMessage()
            {
                error = ResponseCashShopInfoMessage.Error.NotAvailable,
            });
            return default;
        }

        protected virtual UniTaskVoid HandleRequestCashPackageInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashPackageInfoMessage> result)
        {
            result.Invoke(AckResponseCode.Error, new ResponseCashPackageInfoMessage()
            {
                error = ResponseCashPackageInfoMessage.Error.NotAvailable,
            });
            return default;
        }

        protected virtual UniTaskVoid HandleRequestCashShopBuy(
            RequestHandlerData requestHandler, RequestCashShopBuyMessage request,
            RequestProceedResultDelegate<ResponseCashShopBuyMessage> result)
        {
            result.Invoke(AckResponseCode.Error, new ResponseCashShopBuyMessage()
            {
                error = ResponseCashShopBuyMessage.Error.NotAvailable,
            });
            return default;
        }

        protected virtual UniTaskVoid HandleRequestCashPackageBuyValidation(
            RequestHandlerData requestHandler, RequestCashPackageBuyValidationMessage request,
            RequestProceedResultDelegate<ResponseCashPackageBuyValidationMessage> result)
        {
            result.Invoke(AckResponseCode.Error, new ResponseCashPackageBuyValidationMessage()
            {
                error = ResponseCashPackageBuyValidationMessage.Error.NotAvailable,
            });
            return default;
        }

        protected virtual UniTaskVoid HandleRequestMailList(
            RequestHandlerData requestHandler, RequestMailListMessage request,
            RequestProceedResultDelegate<ResponseMailListMessage> result)
        {
            result.Invoke(AckResponseCode.Success, new ResponseMailListMessage());
            return default;
        }

        protected virtual UniTaskVoid HandleRequestReadMail(
            RequestHandlerData requestHandler, RequestReadMailMessage request,
            RequestProceedResultDelegate<ResponseReadMailMessage> result)
        {
            result.Invoke(AckResponseCode.Error, new ResponseReadMailMessage()
            {
                error = ResponseReadMailMessage.Error.NotAvailable,
            });
            return default;
        }

        protected virtual UniTaskVoid HandleRequestClaimMailItems(
            RequestHandlerData requestHandler, RequestClaimMailItemsMessage request,
            RequestProceedResultDelegate<ResponseClaimMailItemsMessage> result)
        {
            result.Invoke(AckResponseCode.Error, new ResponseClaimMailItemsMessage()
            {
                error = ResponseClaimMailItemsMessage.Error.NotAvailable,
            });
            return default;
        }

        protected virtual UniTaskVoid HandleRequestDeleteMail(
            RequestHandlerData requestHandler, RequestDeleteMailMessage request,
            RequestProceedResultDelegate<ResponseDeleteMailMessage> result)
        {
            result.Invoke(AckResponseCode.Error, new ResponseDeleteMailMessage()
            {
                error = ResponseDeleteMailMessage.Error.NotAvailable,
            });
            return default;
        }

        protected virtual UniTaskVoid HandleRequestSendMail(
            RequestHandlerData requestHandler, RequestSendMailMessage request,
            RequestProceedResultDelegate<ResponseSendMailMessage> result)
        {
            result.Invoke(AckResponseCode.Error, new ResponseSendMailMessage()
            {
                error = ResponseSendMailMessage.Error.NotAvailable,
            });
            return default;
        }

        protected virtual void HandleRequestOnlineCharacter(MessageHandlerData messageHandler)
        {
            string characterId = messageHandler.Reader.GetString();
            if (IsCharacterOnline(characterId))
            {
                // Notify back online character
                ServerSendPacket(messageHandler.ConnectionId, DeliveryMethod.ReliableOrdered, MsgTypes.NotifyOnlineCharacter, (writer) =>
                {
                    writer.Put(characterId);
                });
            }
        }

        public override bool StartServer()
        {
            Init();
            return base.StartServer();
        }

        public override bool StartClient(string networkAddress, int networkPort)
        {
            Init();
            return base.StartClient(networkAddress, networkPort);
        }

        public void Init()
        {
            doNotEnterGameOnConnect = false;
            Assets.offlineScene.SceneName = CurrentGameInstance.HomeSceneName;
            // Prepare networking prefabs
            Assets.playerPrefab = null;
            HashSet<LiteNetLibIdentity> spawnablePrefabs = new HashSet<LiteNetLibIdentity>(Assets.spawnablePrefabs);
            if (CurrentGameInstance.itemDropEntityPrefab != null)
                spawnablePrefabs.Add(CurrentGameInstance.itemDropEntityPrefab.Identity);
            if (CurrentGameInstance.warpPortalEntityPrefab != null)
                spawnablePrefabs.Add(CurrentGameInstance.warpPortalEntityPrefab.Identity);
            foreach (BaseCharacterEntity entry in GameInstance.CharacterEntities.Values)
            {
                spawnablePrefabs.Add(entry.Identity);
            }
            foreach (VehicleEntity entry in GameInstance.VehicleEntities.Values)
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
            Assets.spawnablePrefabs = new LiteNetLibIdentity[spawnablePrefabs.Count];
            spawnablePrefabs.CopyTo(Assets.spawnablePrefabs);
            // Make sure that grid manager -> axis mode set correctly for current dimension type
            GridManager gridManager = gameObject.GetOrAddComponent<GridManager>();
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                gridManager.axisMode = GridManager.EAxisMode.XZ;
            else
                gridManager.axisMode = GridManager.EAxisMode.XY;
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
                monsterSpawnArea.RegisterPrefabs();
            }

            HarvestableSpawnArea[] harvestableSpawnAreas = FindObjectsOfType<HarvestableSpawnArea>();
            foreach (HarvestableSpawnArea harvestableSpawnArea in harvestableSpawnAreas)
            {
                harvestableSpawnArea.RegisterPrefabs();
            }

            ItemDropSpawnArea[] itemDropSpawnAreas = FindObjectsOfType<ItemDropSpawnArea>();
            foreach (ItemDropSpawnArea itemDropSpawnArea in itemDropSpawnAreas)
            {
                itemDropSpawnArea.RegisterPrefabs();
            }

            // Register scene entities
            GameInstance.AddCharacterEntities(FindObjectsOfType<BaseMonsterCharacterEntity>());
            GameInstance.AddHarvestableEntities(FindObjectsOfType<HarvestableEntity>());
            GameInstance.AddItemDropEntities(FindObjectsOfType<ItemDropEntity>());

            PoolSystem.Clear();
            foreach (IPoolDescriptor poolingObject in GameInstance.PoolingObjectPrefabs)
            {
                if (!IsClient && (poolingObject is GameEffect || poolingObject is ProjectileEffect))
                    continue;
                PoolSystem.InitPool(poolingObject);
            }
            System.GC.Collect();
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
            isReadyToInstantiatePlayers = false;
            serverSceneLoadedTime = Time.unscaledTime;
            this.InvokeInstanceDevExtMethods("OnServerOnlineSceneLoaded");
            SpawnEntities().Forget();
        }

        protected virtual async UniTaskVoid SpawnEntities()
        {
            while (!IsReadyToInstantiateObjects())
            {
                await UniTask.Yield();
            }
            float progress = 0f;
            string sceneName = SceneManager.GetActiveScene().name;
            onSpawnEntitiesStart.Invoke(sceneName, true, progress);
            await PreSpawnEntities();
            RegisterEntities();
            await UniTask.SwitchToMainThread();
            int i;
            // Spawn Warp Portals
            if (LogInfo)
                Logging.Log("Spawning warp portals");
            if (GameInstance.MapWarpPortals.Count > 0)
            {
                List<WarpPortal> mapWarpPortals;
                if (GameInstance.MapWarpPortals.TryGetValue(CurrentMapInfo.Id, out mapWarpPortals))
                {
                    WarpPortal warpPortal;
                    WarpPortalEntity warpPortalPrefab;
                    WarpPortalEntity warpPortalEntity;
                    for (i = 0; i < mapWarpPortals.Count; ++i)
                    {
                        warpPortal = mapWarpPortals[i];
                        warpPortalPrefab = warpPortal.entityPrefab != null ? warpPortal.entityPrefab : CurrentGameInstance.warpPortalEntityPrefab;
                        if (warpPortalPrefab != null)
                        {
                            warpPortalEntity = Instantiate(warpPortalPrefab, warpPortal.position, Quaternion.identity);
                            warpPortalEntity.warpPortalType = warpPortal.warpPortalType;
                            warpPortalEntity.warpToMapInfo = warpPortal.warpToMapInfo;
                            warpPortalEntity.warpToPosition = warpPortal.warpToPosition;
                            warpPortalEntity.warpOverrideRotation = warpPortal.warpOverrideRotation;
                            warpPortalEntity.warpToRotation = warpPortal.warpToRotation;
                            Assets.NetworkSpawn(warpPortalEntity.gameObject);
                        }
                        await UniTask.Yield();
                        progress = 0f + ((float)i / (float)mapWarpPortals.Count * 0.25f);
                        onSpawnEntitiesProgress.Invoke(sceneName, true, progress);
                    }
                }
            }
            await UniTask.Yield();
            progress = 0.25f;
            onSpawnEntitiesProgress.Invoke(sceneName, true, progress);
            // Spawn Npcs
            if (LogInfo)
                Logging.Log("Spawning NPCs");
            if (GameInstance.MapNpcs.Count > 0)
            {
                List<Npc> mapNpcs;
                if (GameInstance.MapNpcs.TryGetValue(CurrentMapInfo.Id, out mapNpcs))
                {
                    Npc npc;
                    NpcEntity npcPrefab;
                    NpcEntity npcEntity;
                    for (i = 0; i < mapNpcs.Count; ++i)
                    {
                        npc = mapNpcs[i];
                        npcPrefab = npc.entityPrefab;
                        if (npcPrefab != null)
                        {
                            npcEntity = Instantiate(npcPrefab, npc.position, Quaternion.Euler(npc.rotation));
                            npcEntity.Title = npc.title;
                            npcEntity.StartDialog = npc.startDialog;
                            npcEntity.Graph = npc.graph;
                            Assets.NetworkSpawn(npcEntity.gameObject);
                        }
                        await UniTask.Yield();
                        progress = 0.25f + ((float)i / (float)mapNpcs.Count * 0.25f);
                        onSpawnEntitiesProgress.Invoke(sceneName, true, progress);
                    }
                }
            }
            await UniTask.Yield();
            progress = 0.5f;
            onSpawnEntitiesProgress.Invoke(sceneName, true, progress);
            // Spawn monsters
            if (LogInfo)
                Logging.Log("Spawning monsters");
            MonsterSpawnArea[] monsterSpawnAreas = FindObjectsOfType<MonsterSpawnArea>();
            for (i = 0; i < monsterSpawnAreas.Length; ++i)
            {
                monsterSpawnAreas[i].SpawnAll();
                await UniTask.Yield();
                progress = 0.5f + ((float)i / (float)monsterSpawnAreas.Length * 0.25f);
                onSpawnEntitiesProgress.Invoke(sceneName, true, progress);
            }
            await UniTask.Yield();
            progress = 0.75f;
            onSpawnEntitiesProgress.Invoke(sceneName, true, progress);
            // Spawn harvestables
            if (LogInfo)
                Logging.Log("Spawning harvestables");
            HarvestableSpawnArea[] harvestableSpawnAreas = FindObjectsOfType<HarvestableSpawnArea>();
            for (i = 0; i < harvestableSpawnAreas.Length; ++i)
            {
                harvestableSpawnAreas[i].SpawnAll();
                await UniTask.Yield();
                progress = 0.75f + ((float)i / (float)harvestableSpawnAreas.Length * 0.125f);
                onSpawnEntitiesProgress.Invoke(sceneName, true, progress);
            }
            await UniTask.Yield();
            progress = 0.875f;
            onSpawnEntitiesProgress.Invoke(sceneName, true, progress);
            // Spawn item drop entities
            if (LogInfo)
                Logging.Log("Spawning harvestables");
            ItemDropSpawnArea[] itemDropSpawnAreas = FindObjectsOfType<ItemDropSpawnArea>();
            for (i = 0; i < itemDropSpawnAreas.Length; ++i)
            {
                itemDropSpawnAreas[i].SpawnAll();
                await UniTask.Yield();
                progress = 0.875f + ((float)i / (float)itemDropSpawnAreas.Length * 0.125f);
                onSpawnEntitiesProgress.Invoke(sceneName, true, progress);
            }
            await UniTask.Yield();
            progress = 1f;
            onSpawnEntitiesProgress.Invoke(sceneName, true, progress);
            // If it's server (not host) spawn simple camera controller
            if (!IsClient && GameInstance.Singleton.serverCharacterPrefab != null &&
                SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
            {
                if (LogInfo)
                    Logging.Log("Spawning server character");
                Instantiate(GameInstance.Singleton.serverCharacterPrefab, CurrentMapInfo.StartPosition, Quaternion.identity);
            }
            await UniTask.Yield();
            progress = 1f;
            onSpawnEntitiesFinish.Invoke(sceneName, true, progress);
            await PostSpawnEntities();
            isReadyToInstantiatePlayers = true;
        }

        protected virtual async UniTask PreSpawnEntities()
        {
            await UniTask.Yield();
        }

        protected virtual async UniTask PostSpawnEntities()
        {
            await UniTask.Yield();
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
            if (GameInstance.BuildingEntities.ContainsKey(saveData.EntityId))
            {
                GameObject spawnObj = Instantiate(GameInstance.BuildingEntities[saveData.EntityId].gameObject, saveData.position, saveData.Rotation);
                BuildingEntity buildingEntity = spawnObj.GetComponent<BuildingEntity>();
                buildingEntity.Id = saveData.Id;
                buildingEntity.ParentId = saveData.ParentId;
                buildingEntity.CurrentHp = saveData.CurrentHp;
                buildingEntity.RemainsLifeTime = saveData.RemainsLifeTime;
                buildingEntity.IsLocked = saveData.IsLocked;
                buildingEntity.LockPassword = saveData.LockPassword;
                buildingEntity.CreatorId = saveData.CreatorId;
                buildingEntity.CreatorName = saveData.CreatorName;
                Assets.NetworkSpawn(spawnObj);
                buildingEntities[buildingEntity.Id] = buildingEntity;
                buildingEntity.CallAllOnBuildingConstruct();
                return buildingEntity;
            }
            return null;
        }

        public virtual void DestroyBuildingEntity(string id)
        {
            if (buildingEntities.ContainsKey(id))
            {
                buildingEntities[id].Destroy();
                buildingEntities.Remove(id);
            }
        }

        public bool TryGetBuildingEntity<T>(string id, out T entity) where T : BuildingEntity
        {
            entity = null;
            if (buildingEntities.ContainsKey(id))
            {
                entity = buildingEntities[id] as T;
                return entity;
            }
            return false;
        }

        public void SetMapInfo(string mapId)
        {
            if (GameInstance.MapInfos.ContainsKey(mapId))
                SetMapInfo(GameInstance.MapInfos[mapId]);
        }

        public void SetMapInfo(BaseMapInfo mapInfo)
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
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.UpdateMapInfo, message);
        }

        public void SendTimeOfDay()
        {
            if (!IsServer)
                return;
            foreach (long connectionId in ConnectionIds)
            {
                SendTimeOfDay(connectionId);
            }
        }

        public void SendTimeOfDay(long connectionId)
        {
            if (!IsServer)
                return;
            UpdateTimeOfDayMessage message = new UpdateTimeOfDayMessage();
            message.timeOfDay = CurrentGameInstance.DayNightTimeUpdater.TimeOfDay;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.UpdateTimeOfDay, message);
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

        public bool CanSendSystemAnnounce(string sender)
        {
            // TODO: Don't use fixed user level
            BasePlayerCharacterEntity playerCharacter;
            return (!string.IsNullOrEmpty(sender) &&
                    TryGetPlayerCharacterByName(sender, out playerCharacter) &&
                    playerCharacter.UserLevel > 0) ||
                    CHAT_SYSTEM_ANNOUNCER_SENDER.Equals(sender);
        }

        public void SendSystemAnnounce(string message)
        {
            if (!IsServer)
                return;
            NetDataWriter writer = new NetDataWriter();
            ChatMessage chatMessage = new ChatMessage()
            {
                channel = ChatChannel.System,
                sender = CHAT_SYSTEM_ANNOUNCER_SENDER,
                message = message,
            };
            chatMessage.Serialize(writer);
            HandleChatAtServer(new MessageHandlerData(MsgTypes.Chat, Server, -1, new NetDataReader(writer.CopyData())));
        }
    }
}
