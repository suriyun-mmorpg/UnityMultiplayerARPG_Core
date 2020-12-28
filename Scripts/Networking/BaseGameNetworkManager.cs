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
        public const string CHAT_SYSTEM_ANNOUNCER_SENDER = "SYSTEM_ANNOUNCER";
        public const float UPDATE_ONLINE_CHARACTER_DURATION = 1f;
        public const float UPDATE_TIME_OF_DAY_DURATION = 5f;
        public const string INSTANTIATES_OBJECTS_DELAY_STATE_KEY = "INSTANTIATES_OBJECTS_DELAY";
        public const float INSTANTIATES_OBJECTS_DELAY = 0.5f;

        public static BaseGameNetworkManager Singleton { get; protected set; }
        protected GameInstance CurrentGameInstance { get { return GameInstance.Singleton; } }
        // Server Handlers
        protected IServerUserHandlers ServerUserHandlers { get; set; }
        protected IServerGameMessageHandlers ServerGameMessageHandlers { get; set; }
        protected IServerStorageHandlers ServerStorageHandlers { get; set; }
        protected IServerPartyHandlers ServerPartyHandlers { get; set; }
        protected IServerGuildHandlers ServerGuildHandlers { get; set; }
        // Server Message Handlers
        protected IServerCashShopMessageHandlers ServerCashShopMessageHandlers { get; set; }
        protected IServerMailMessageHandlers ServerMailMessageHandlers { get; set; }
        protected IServerStorageMessageHandlers ServerStorageMessageHandlers { get; set; }
        protected IServerInventoryMessageHandlers ServerInventoryMessageHandlers { get; set; }
        protected IServerPartyMessageHandlers ServerPartyMessageHandlers { get; set; }
        protected IServerGuildMessageHandlers ServerGuildMessageHandlers { get; set; }
        protected IServerFriendMessageHandlers ServerFriendMessageHandlers { get; set; }
        protected IServerBankMessageHandlers ServerBankMessageHandlers { get; set; }
        // Client handlers
        protected IClientCashShopHandlers ClientCashShopHandlers { get; set; }
        protected IClientMailHandlers ClientMailHandlers { get; set; }
        protected IClientStorageHandlers ClientStorageHandlers { get; set; }
        protected IClientInventoryHandlers ClientInventoryHandlers { get; set; }
        protected IClientPartyHandlers ClientPartyHandlers { get; set; }
        protected IClientGuildHandlers ClientGuildHandlers { get; set; }
        protected IClientFriendHandlers ClientFriendHandlers { get; set; }
        protected IClientBankHandlers ClientBankHandlers { get; set; }
        protected IClientUserHandlers ClientUserHandlers { get; set; }

        public static readonly Dictionary<string, BuildingEntity> BuildingEntities = new Dictionary<string, BuildingEntity>();
        public static readonly Dictionary<string, NotifyOnlineCharacterTime> LastCharacterOnlineTimes = new Dictionary<string, NotifyOnlineCharacterTime>();
        public static BaseMapInfo CurrentMapInfo { get; protected set; }

        // Events
        protected float updateOnlineCharactersCountDown;
        protected float updateTimeOfDayCountDown;
        protected float serverSceneLoadedTime;
        // Instantiate object allowing status
        protected Dictionary<string, bool> readyToInstantiateObjectsStates = new Dictionary<string, bool>();
        protected bool isReadyToInstantiateObjects;
        protected bool isReadyToInstantiatePlayers;
        // Spawn entities events
        public LiteNetLibLoadSceneEvent onSpawnEntitiesStart;
        public LiteNetLibLoadSceneEvent onSpawnEntitiesProgress;
        public LiteNetLibLoadSceneEvent onSpawnEntitiesFinish;

        public override uint PacketVersion()
        {
            return 5;
        }

        protected override void Awake()
        {
            Singleton = this;
            doNotEnterGameOnConnect = false;
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
            RegisterClientMessage(GameNetworkingConsts.GameMessage, HandleGameMessageAtClient);
            RegisterClientMessage(GameNetworkingConsts.Warp, HandleWarpAtClient);
            RegisterClientMessage(GameNetworkingConsts.Chat, HandleChatAtClient);
            RegisterClientMessage(GameNetworkingConsts.UpdatePartyMember, HandleUpdatePartyMemberAtClient);
            RegisterClientMessage(GameNetworkingConsts.UpdateParty, HandleUpdatePartyAtClient);
            RegisterClientMessage(GameNetworkingConsts.UpdateGuildMember, HandleUpdateGuildMemberAtClient);
            RegisterClientMessage(GameNetworkingConsts.UpdateGuild, HandleUpdateGuildAtClient);
            RegisterClientMessage(GameNetworkingConsts.UpdateFriends, HandleUpdateFriendsAtClient);
            RegisterClientMessage(GameNetworkingConsts.UpdateMapInfo, HandleUpdateMapInfoAtClient);
            RegisterClientMessage(GameNetworkingConsts.NotifyOnlineCharacter, HandleNotifyOnlineCharacterAtClient);
            RegisterClientMessage(GameNetworkingConsts.NotifyRewardExp, HandleNotifyRewardExpAtClient);
            RegisterClientMessage(GameNetworkingConsts.NotifyRewardGold, HandleNotifyRewardGoldAtClient);
            RegisterClientMessage(GameNetworkingConsts.NotifyRewardItem, HandleNotifyRewardItemAtClient);
            RegisterClientMessage(GameNetworkingConsts.UpdateTimeOfDay, HandleUpdateDayNightTimeAtClient);
            RegisterClientMessage(GameNetworkingConsts.NotifyStorageOpened, HandleNotifyStorageOpenedAtClient);
            RegisterClientMessage(GameNetworkingConsts.NotifyStorageClosed, HandleNotifyStorageClosedAtClient);
            RegisterClientMessage(GameNetworkingConsts.NotifyStorageItemsUpdated, HandleNotifyStorageItemsUpdatedAtClient);
            RegisterClientMessage(GameNetworkingConsts.NotifyPartyInvitation, HandleNotifyPartyInvitationAtClient);
            RegisterClientMessage(GameNetworkingConsts.NotifyGuildInvitation, HandleNotifyGuildInvitationAtClient);
            // Responses
            // Cash shop
            RegisterClientResponse<EmptyMessage, ResponseCashShopInfoMessage>(GameNetworkingConsts.CashShopInfo);
            RegisterClientResponse<EmptyMessage, ResponseCashPackageInfoMessage>(GameNetworkingConsts.CashPackageInfo);
            RegisterClientResponse<RequestCashShopBuyMessage, ResponseCashShopBuyMessage>(GameNetworkingConsts.CashShopBuy);
            RegisterClientResponse<RequestCashPackageBuyValidationMessage, ResponseCashPackageBuyValidationMessage>(GameNetworkingConsts.CashPackageBuyValidation);
            // Mail
            RegisterClientResponse<RequestMailListMessage, ResponseMailListMessage>(GameNetworkingConsts.MailList);
            RegisterClientResponse<RequestReadMailMessage, ResponseReadMailMessage>(GameNetworkingConsts.ReadMail);
            RegisterClientResponse<RequestClaimMailItemsMessage, ResponseClaimMailItemsMessage>(GameNetworkingConsts.ClaimMailItems);
            RegisterClientResponse<RequestDeleteMailMessage, ResponseDeleteMailMessage>(GameNetworkingConsts.DeleteMail);
            RegisterClientResponse<RequestSendMailMessage, ResponseSendMailMessage>(GameNetworkingConsts.SendMail);
            // Storage
            RegisterClientResponse<RequestMoveItemFromStorageMessage, ResponseMoveItemFromStorageMessage>(GameNetworkingConsts.MoveItemFromStorage);
            RegisterClientResponse<RequestMoveItemToStorageMessage, ResponseMoveItemToStorageMessage>(GameNetworkingConsts.MoveItemToStorage);
            RegisterClientResponse<RequestSwapOrMergeStorageItemMessage, ResponseSwapOrMergeStorageItemMessage>(GameNetworkingConsts.SwapOrMergeStorageItem);
            // Inventory
            RegisterClientResponse<RequestSwapOrMergeItemMessage, ResponseSwapOrMergeItemMessage>(GameNetworkingConsts.SwapOrMergeItem);
            RegisterClientResponse<RequestEquipWeaponMessage, ResponseEquipWeaponMessage>(GameNetworkingConsts.EquipWeapon);
            RegisterClientResponse<RequestEquipArmorMessage, ResponseEquipArmorMessage>(GameNetworkingConsts.EquipArmor);
            RegisterClientResponse<RequestUnEquipWeaponMessage, ResponseUnEquipWeaponMessage>(GameNetworkingConsts.UnEquipWeapon);
            RegisterClientResponse<RequestUnEquipArmorMessage, ResponseUnEquipArmorMessage>(GameNetworkingConsts.UnEquipArmor);
            // Party
            RegisterClientResponse<RequestCreatePartyMessage, ResponseCreatePartyMessage>(GameNetworkingConsts.CreateParty);
            RegisterClientResponse<RequestChangePartyLeaderMessage, ResponseChangePartyLeaderMessage>(GameNetworkingConsts.ChangePartyLeader);
            RegisterClientResponse<RequestChangePartySettingMessage, ResponseChangePartySettingMessage>(GameNetworkingConsts.ChangePartySetting);
            RegisterClientResponse<RequestSendPartyInvitationMessage, ResponseSendPartyInvitationMessage>(GameNetworkingConsts.SendPartyInvitation);
            RegisterClientResponse<RequestAcceptPartyInvitationMessage, ResponseAcceptPartyInvitationMessage>(GameNetworkingConsts.AcceptPartyInvitation);
            RegisterClientResponse<RequestDeclinePartyInvitationMessage, ResponseDeclinePartyInvitationMessage>(GameNetworkingConsts.DeclinePartyInvitation);
            RegisterClientResponse<RequestKickMemberFromPartyMessage, ResponseKickMemberFromPartyMessage>(GameNetworkingConsts.KickMemberFromParty);
            RegisterClientResponse<EmptyMessage, ResponseLeavePartyMessage>(GameNetworkingConsts.LeaveParty);
            // Guild
            RegisterClientResponse<RequestCreateGuildMessage, ResponseCreateGuildMessage>(GameNetworkingConsts.CreateGuild);
            RegisterClientResponse<RequestChangeGuildLeaderMessage, ResponseChangeGuildLeaderMessage>(GameNetworkingConsts.ChangeGuildLeader);
            RegisterClientResponse<RequestChangeGuildMessageMessage, ResponseChangeGuildMessageMessage>(GameNetworkingConsts.ChangeGuildMessage);
            RegisterClientResponse<RequestChangeGuildRoleMessage, ResponseChangeGuildRoleMessage>(GameNetworkingConsts.ChangeGuildRole);
            RegisterClientResponse<RequestChangeMemberGuildRoleMessage, ResponseChangeMemberGuildRoleMessage>(GameNetworkingConsts.ChangeMemberGuildRole);
            RegisterClientResponse<RequestSendGuildInvitationMessage, ResponseSendGuildInvitationMessage>(GameNetworkingConsts.SendGuildInvitation);
            RegisterClientResponse<RequestAcceptGuildInvitationMessage, ResponseAcceptGuildInvitationMessage>(GameNetworkingConsts.AcceptGuildInvitation);
            RegisterClientResponse<RequestDeclineGuildInvitationMessage, ResponseDeclineGuildInvitationMessage>(GameNetworkingConsts.DeclineGuildInvitation);
            RegisterClientResponse<RequestKickMemberFromGuildMessage, ResponseKickMemberFromGuildMessage>(GameNetworkingConsts.KickMemberFromGuild);
            RegisterClientResponse<EmptyMessage, ResponseLeaveGuildMessage>(GameNetworkingConsts.LeaveGuild);
            RegisterClientResponse<RequestIncreaseGuildSkillLevelMessage, ResponseIncreaseGuildSkillLevelMessage>(GameNetworkingConsts.IncreaseGuildSkillLevel);
            // Friend
            RegisterClientResponse<RequestFindCharactersMessage, ResponseFindCharactersMessage>(GameNetworkingConsts.FindCharacters);
            RegisterClientResponse<EmptyMessage, ResponseGetFriendsMessage>(GameNetworkingConsts.GetFriends);
            RegisterClientResponse<RequestAddFriendMessage, ResponseAddFriendMessage>(GameNetworkingConsts.AddFriend);
            RegisterClientResponse<RequestRemoveFriendMessage, ResponseRemoveFriendMessage>(GameNetworkingConsts.RemoveFriend);
            // Bank
            RegisterClientResponse<RequestDepositUserGoldMessage, ResponseDepositUserGoldMessage>(GameNetworkingConsts.DepositUserGold);
            RegisterClientResponse<RequestWithdrawUserGoldMessage, ResponseWithdrawUserGoldMessage>(GameNetworkingConsts.WithdrawUserGold);
            RegisterClientResponse<RequestDepositGuildGoldMessage, ResponseDepositGuildGoldMessage>(GameNetworkingConsts.DepositGuildGold);
            RegisterClientResponse<RequestWithdrawGuildGoldMessage, ResponseWithdrawGuildGoldMessage>(GameNetworkingConsts.WithdrawGuildGold);
        }

        protected override void RegisterServerMessages()
        {
            this.InvokeInstanceDevExtMethods("RegisterServerMessages");
            base.RegisterServerMessages();
            // Networking messages
            RegisterServerMessage(GameNetworkingConsts.Chat, HandleChatAtServer);
            RegisterServerMessage(GameNetworkingConsts.NotifyOnlineCharacter, HandleRequestOnlineCharacter);
            // Requests
            // Cash shop
            if (ServerCashShopMessageHandlers != null)
            {
                RegisterServerRequest<EmptyMessage, ResponseCashShopInfoMessage>(GameNetworkingConsts.CashShopInfo, ServerCashShopMessageHandlers.HandleRequestCashShopInfo);
                RegisterServerRequest<EmptyMessage, ResponseCashPackageInfoMessage>(GameNetworkingConsts.CashPackageInfo, ServerCashShopMessageHandlers.HandleRequestCashPackageInfo);
                RegisterServerRequest<RequestCashShopBuyMessage, ResponseCashShopBuyMessage>(GameNetworkingConsts.CashShopBuy, ServerCashShopMessageHandlers.HandleRequestCashShopBuy);
                RegisterServerRequest<RequestCashPackageBuyValidationMessage, ResponseCashPackageBuyValidationMessage>(GameNetworkingConsts.CashPackageBuyValidation, ServerCashShopMessageHandlers.HandleRequestCashPackageBuyValidation);
            }
            // Mail
            if (ServerMailMessageHandlers != null)
            {
                RegisterServerRequest<RequestMailListMessage, ResponseMailListMessage>(GameNetworkingConsts.MailList, ServerMailMessageHandlers.HandleRequestMailList);
                RegisterServerRequest<RequestReadMailMessage, ResponseReadMailMessage>(GameNetworkingConsts.ReadMail, ServerMailMessageHandlers.HandleRequestReadMail);
                RegisterServerRequest<RequestClaimMailItemsMessage, ResponseClaimMailItemsMessage>(GameNetworkingConsts.ClaimMailItems, ServerMailMessageHandlers.HandleRequestClaimMailItems);
                RegisterServerRequest<RequestDeleteMailMessage, ResponseDeleteMailMessage>(GameNetworkingConsts.DeleteMail, ServerMailMessageHandlers.HandleRequestDeleteMail);
                RegisterServerRequest<RequestSendMailMessage, ResponseSendMailMessage>(GameNetworkingConsts.SendMail, ServerMailMessageHandlers.HandleRequestSendMail);
            }
            // Storage
            if (ServerStorageMessageHandlers != null)
            {
                RegisterServerRequest<RequestMoveItemFromStorageMessage, ResponseMoveItemFromStorageMessage>(GameNetworkingConsts.MoveItemFromStorage, ServerStorageMessageHandlers.HandleRequestMoveItemFromStorage);
                RegisterServerRequest<RequestMoveItemToStorageMessage, ResponseMoveItemToStorageMessage>(GameNetworkingConsts.MoveItemToStorage, ServerStorageMessageHandlers.HandleRequestMoveItemToStorage);
                RegisterServerRequest<RequestSwapOrMergeStorageItemMessage, ResponseSwapOrMergeStorageItemMessage>(GameNetworkingConsts.SwapOrMergeStorageItem, ServerStorageMessageHandlers.HandleRequestSwapOrMergeStorageItem);
            }
            // Inventory
            if (ServerInventoryMessageHandlers != null)
            {
                RegisterServerRequest<RequestSwapOrMergeItemMessage, ResponseSwapOrMergeItemMessage>(GameNetworkingConsts.SwapOrMergeItem, ServerInventoryMessageHandlers.HandleRequestSwapOrMergeItem);
                RegisterServerRequest<RequestEquipWeaponMessage, ResponseEquipWeaponMessage>(GameNetworkingConsts.EquipWeapon, ServerInventoryMessageHandlers.HandleRequestEquipWeapon);
                RegisterServerRequest<RequestEquipArmorMessage, ResponseEquipArmorMessage>(GameNetworkingConsts.EquipArmor, ServerInventoryMessageHandlers.HandleRequestEquipArmor);
                RegisterServerRequest<RequestUnEquipWeaponMessage, ResponseUnEquipWeaponMessage>(GameNetworkingConsts.UnEquipWeapon, ServerInventoryMessageHandlers.HandleRequestUnEquipWeapon);
                RegisterServerRequest<RequestUnEquipArmorMessage, ResponseUnEquipArmorMessage>(GameNetworkingConsts.UnEquipArmor, ServerInventoryMessageHandlers.HandleRequestUnEquipArmor);
            }
            // Party
            if (ServerPartyMessageHandlers != null)
            {
                RegisterServerRequest<RequestCreatePartyMessage, ResponseCreatePartyMessage>(GameNetworkingConsts.CreateParty, ServerPartyMessageHandlers.HandleRequestCreateParty);
                RegisterServerRequest<RequestChangePartyLeaderMessage, ResponseChangePartyLeaderMessage>(GameNetworkingConsts.ChangePartyLeader, ServerPartyMessageHandlers.HandleRequestChangePartyLeader);
                RegisterServerRequest<RequestChangePartySettingMessage, ResponseChangePartySettingMessage>(GameNetworkingConsts.ChangePartySetting, ServerPartyMessageHandlers.HandleRequestChangePartySetting);
                RegisterServerRequest<RequestSendPartyInvitationMessage, ResponseSendPartyInvitationMessage>(GameNetworkingConsts.SendPartyInvitation, ServerPartyMessageHandlers.HandleRequestSendPartyInvitation);
                RegisterServerRequest<RequestAcceptPartyInvitationMessage, ResponseAcceptPartyInvitationMessage>(GameNetworkingConsts.AcceptPartyInvitation, ServerPartyMessageHandlers.HandleRequestAcceptPartyInvitation);
                RegisterServerRequest<RequestDeclinePartyInvitationMessage, ResponseDeclinePartyInvitationMessage>(GameNetworkingConsts.DeclinePartyInvitation, ServerPartyMessageHandlers.HandleRequestDeclinePartyInvitation);
                RegisterServerRequest<RequestKickMemberFromPartyMessage, ResponseKickMemberFromPartyMessage>(GameNetworkingConsts.KickMemberFromParty, ServerPartyMessageHandlers.HandleRequestKickMemberFromParty);
                RegisterServerRequest<EmptyMessage, ResponseLeavePartyMessage>(GameNetworkingConsts.LeaveParty, ServerPartyMessageHandlers.HandleRequestLeaveParty);
            }
            // Guild
            if (ServerGuildMessageHandlers != null)
            {
                RegisterServerRequest<RequestCreateGuildMessage, ResponseCreateGuildMessage>(GameNetworkingConsts.CreateGuild, ServerGuildMessageHandlers.HandleRequestCreateGuild);
                RegisterServerRequest<RequestChangeGuildLeaderMessage, ResponseChangeGuildLeaderMessage>(GameNetworkingConsts.ChangeGuildLeader, ServerGuildMessageHandlers.HandleRequestChangeGuildLeader);
                RegisterServerRequest<RequestChangeGuildMessageMessage, ResponseChangeGuildMessageMessage>(GameNetworkingConsts.ChangeGuildMessage, ServerGuildMessageHandlers.HandleRequestChangeGuildMessage);
                RegisterServerRequest<RequestChangeGuildRoleMessage, ResponseChangeGuildRoleMessage>(GameNetworkingConsts.ChangeGuildRole, ServerGuildMessageHandlers.HandleRequestChangeGuildRole);
                RegisterServerRequest<RequestChangeMemberGuildRoleMessage, ResponseChangeMemberGuildRoleMessage>(GameNetworkingConsts.ChangeMemberGuildRole, ServerGuildMessageHandlers.HandleRequestChangeMemberGuildRole);
                RegisterServerRequest<RequestSendGuildInvitationMessage, ResponseSendGuildInvitationMessage>(GameNetworkingConsts.SendGuildInvitation, ServerGuildMessageHandlers.HandleRequestSendGuildInvitation);
                RegisterServerRequest<RequestAcceptGuildInvitationMessage, ResponseAcceptGuildInvitationMessage>(GameNetworkingConsts.AcceptGuildInvitation, ServerGuildMessageHandlers.HandleRequestAcceptGuildInvitation);
                RegisterServerRequest<RequestDeclineGuildInvitationMessage, ResponseDeclineGuildInvitationMessage>(GameNetworkingConsts.DeclineGuildInvitation, ServerGuildMessageHandlers.HandleRequestDeclineGuildInvitation);
                RegisterServerRequest<RequestKickMemberFromGuildMessage, ResponseKickMemberFromGuildMessage>(GameNetworkingConsts.KickMemberFromGuild, ServerGuildMessageHandlers.HandleRequestKickMemberFromGuild);
                RegisterServerRequest<EmptyMessage, ResponseLeaveGuildMessage>(GameNetworkingConsts.LeaveGuild, ServerGuildMessageHandlers.HandleRequestLeaveGuild);
                RegisterServerRequest<RequestIncreaseGuildSkillLevelMessage, ResponseIncreaseGuildSkillLevelMessage>(GameNetworkingConsts.IncreaseGuildSkillLevel, ServerGuildMessageHandlers.HandleRequestIncreaseGuildSkillLevel);
            }
            // Friend
            if (ServerFriendMessageHandlers != null)
            {
                RegisterServerRequest<RequestFindCharactersMessage, ResponseFindCharactersMessage>(GameNetworkingConsts.FindCharacters, ServerFriendMessageHandlers.HandleRequestFindCharacters);
                RegisterServerRequest<EmptyMessage, ResponseGetFriendsMessage>(GameNetworkingConsts.GetFriends, ServerFriendMessageHandlers.HandleRequestGetFriends);
                RegisterServerRequest<RequestAddFriendMessage, ResponseAddFriendMessage>(GameNetworkingConsts.AddFriend, ServerFriendMessageHandlers.HandleRequestAddFriend);
                RegisterServerRequest<RequestRemoveFriendMessage, ResponseRemoveFriendMessage>(GameNetworkingConsts.RemoveFriend, ServerFriendMessageHandlers.HandleRequestRemoveFriend);
            }
            // Bank
            if (ServerBankMessageHandlers != null)
            {
                RegisterServerRequest<RequestDepositUserGoldMessage, ResponseDepositUserGoldMessage>(GameNetworkingConsts.DepositUserGold, ServerBankMessageHandlers.HandleRequestDepositUserGold);
                RegisterServerRequest<RequestWithdrawUserGoldMessage, ResponseWithdrawUserGoldMessage>(GameNetworkingConsts.WithdrawUserGold, ServerBankMessageHandlers.HandleRequestWithdrawUserGold);
                RegisterServerRequest<RequestDepositGuildGoldMessage, ResponseDepositGuildGoldMessage>(GameNetworkingConsts.DepositGuildGold, ServerBankMessageHandlers.HandleRequestDepositGuildGold);
                RegisterServerRequest<RequestWithdrawGuildGoldMessage, ResponseWithdrawGuildGoldMessage>(GameNetworkingConsts.WithdrawGuildGold, ServerBankMessageHandlers.HandleRequestWithdrawGuildGold);
            }
        }

        protected virtual void Clean()
        {
            this.InvokeInstanceDevExtMethods("Clean");
            if (ServerUserHandlers != null)
                ServerUserHandlers.ClearUsersAndPlayerCharacters();
            if (ServerStorageHandlers != null)
                ServerStorageHandlers.ClearStorage();
            if (ServerPartyHandlers != null)
                ServerPartyHandlers.ClearParty();
            if (ServerGuildHandlers != null)
                ServerGuildHandlers.ClearGuild();
            BuildingEntities.Clear();
            LastCharacterOnlineTimes.Clear();
            CurrentMapInfo = null;
        }

        public override bool StartServer()
        {
            InitPrefabs();
            return base.StartServer();
        }

        public override void OnStartServer()
        {
            this.InvokeInstanceDevExtMethods("OnStartServer");
            base.OnStartServer();
            GameInstance.ServerUserHandlers = ServerUserHandlers;
            GameInstance.ServerGameMessageHandlers = ServerGameMessageHandlers;
            GameInstance.ServerStorageHandlers = ServerStorageHandlers;
            GameInstance.ServerPartyHandlers = ServerPartyHandlers;
            GameInstance.ServerGuildHandlers = ServerGuildHandlers;
            CurrentGameInstance.DayNightTimeUpdater.InitTimeOfDay(this);
        }

        public override void OnStopServer()
        {
            Clean();
            base.OnStopServer();
        }

        public override bool StartClient(string networkAddress, int networkPort)
        {
            InitPrefabs();
            return base.StartClient(networkAddress, networkPort);
        }

        public override void OnStartClient(LiteNetLibClient client)
        {
            this.InvokeInstanceDevExtMethods("OnStartClient", client);
            base.OnStartClient(client);
            GameInstance.ClientCashShopHandlers = ClientCashShopHandlers;
            GameInstance.ClientMailHandlers = ClientMailHandlers;
            GameInstance.ClientInventoryHandlers = ClientInventoryHandlers;
            GameInstance.ClientStorageHandlers = ClientStorageHandlers;
            GameInstance.ClientPartyHandlers = ClientPartyHandlers;
            GameInstance.ClientGuildHandlers = ClientGuildHandlers;
            GameInstance.ClientFriendHandlers = ClientFriendHandlers;
            GameInstance.ClientBankHandlers = ClientBankHandlers;
            GameInstance.ClientUserHandlers = ClientUserHandlers;
        }

        public override void OnStopClient()
        {
            if (!IsServer)
                Clean();
            base.OnStopClient();
        }

        public override void OnClientConnected()
        {
            base.OnClientConnected();
            ClientGenericActions.ClientConnected();
        }

        public override void OnClientDisconnected(DisconnectInfo disconnectInfo)
        {
            base.OnClientDisconnected(disconnectInfo);
            ClientGenericActions.ClientDisconnected(disconnectInfo);
            UISceneGlobal.Singleton.ShowDisconnectDialog(disconnectInfo);
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
            if (!LastCharacterOnlineTimes.TryGetValue(characterId, out notifyTime))
            {
                LastCharacterOnlineTimes.Add(characterId, new NotifyOnlineCharacterTime()
                {
                    LastNotifyTime = Time.unscaledTime
                });
            }
            else
            {
                notifyTime.LastNotifyTime = Time.unscaledTime;
                LastCharacterOnlineTimes[characterId] = notifyTime;
            }
        }

        public static void RequestOnlineCharacter(string characterId)
        {
            if (Singleton == null || Singleton.IsServer || !Singleton.IsClientConnected)
                return;

            float unscaledTime = Time.unscaledTime;
            NotifyOnlineCharacterTime notifyTime;
            if (!LastCharacterOnlineTimes.TryGetValue(characterId, out notifyTime))
            {
                LastCharacterOnlineTimes.Add(characterId, new NotifyOnlineCharacterTime()
                {
                    LastRequestTime = unscaledTime
                });
            }
            else
            {
                if (unscaledTime - notifyTime.LastRequestTime < 1.5f)
                    return;

                notifyTime.LastRequestTime = unscaledTime;
                LastCharacterOnlineTimes[characterId] = notifyTime;
            }
            Singleton.ClientSendPacket(DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyOnlineCharacter, (writer) =>
            {
                writer.Put(characterId);
            });
        }

        public static bool IsCharacterOnline(string characterId)
        {
            NotifyOnlineCharacterTime notifyTime;
            return LastCharacterOnlineTimes.TryGetValue(characterId, out notifyTime) &&
                Time.unscaledTime - notifyTime.LastNotifyTime <= 2f;
        }

        protected virtual void UpdateOnlineCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            NotifyOnlineCharacter(playerCharacterEntity.Id);
        }

        protected virtual void UpdateOnlineCharacters()
        {
            Dictionary<long, PartyData> updatingPartyMembers = new Dictionary<long, PartyData>();
            Dictionary<long, GuildData> updatingGuildMembers = new Dictionary<long, GuildData>();

            PartyData tempParty;
            GuildData tempGuild;
            foreach (BasePlayerCharacterEntity playerCharacter in ServerUserHandlers.GetPlayerCharacters())
            {
                UpdateOnlineCharacter(playerCharacter);

                if (playerCharacter.PartyId > 0 && ServerPartyHandlers.TryGetParty(playerCharacter.PartyId, out tempParty))
                {
                    tempParty.UpdateMember(playerCharacter);
                    if (!updatingPartyMembers.ContainsKey(playerCharacter.ConnectionId))
                        updatingPartyMembers.Add(playerCharacter.ConnectionId, tempParty);
                }

                if (playerCharacter.GuildId > 0 && ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out tempGuild))
                {
                    tempGuild.UpdateMember(playerCharacter);
                    if (!updatingGuildMembers.ContainsKey(playerCharacter.ConnectionId))
                        updatingGuildMembers.Add(playerCharacter.ConnectionId, tempGuild);
                }
            }

            foreach (long connectionId in updatingPartyMembers.Keys)
            {
                ServerGameMessageHandlers.SendUpdatePartyMembersToOne(connectionId, updatingPartyMembers[connectionId]);
            }

            foreach (long connectionId in updatingGuildMembers.Keys)
            {
                ServerGameMessageHandlers.SendUpdateGuildMembersToOne(connectionId, updatingGuildMembers[connectionId]);
            }
        }

        protected virtual void HandleGameMessageAtClient(MessageHandlerData messageHandler)
        {
            ClientGenericActions.ClientReceiveGameMessage(messageHandler.ReadMessage<GameMessage>());
        }

        protected virtual void HandleWarpAtClient(MessageHandlerData messageHandler)
        {
            ClientGenericActions.ClientWarp();
        }

        protected virtual void HandleChatAtClient(MessageHandlerData messageHandler)
        {
            ClientGenericActions.ClientReceiveChatMessage(messageHandler.ReadMessage<ChatMessage>());
        }

        protected void HandleUpdatePartyMemberAtClient(MessageHandlerData messageHandler)
        {
            UpdateSocialGroupMember(ClientPartyHandlers.ClientParty, messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            ClientPartyActions.NotifyPartyUpdated(ClientPartyHandlers.ClientParty);
        }

        protected void HandleUpdatePartyAtClient(MessageHandlerData messageHandler)
        {
            UpdatePartyMessage message = messageHandler.ReadMessage<UpdatePartyMessage>();
            if (message.type == UpdatePartyMessage.UpdateType.Create)
            {
                ClientPartyHandlers.ClientParty = new PartyData(message.id, message.shareExp, message.shareItem, message.characterId);
            }
            else if (ClientPartyHandlers.ClientParty != null && ClientPartyHandlers.ClientParty.id == message.id)
            {
                switch (message.type)
                {
                    case UpdatePartyMessage.UpdateType.ChangeLeader:
                        ClientPartyHandlers.ClientParty.SetLeader(message.characterId);
                        break;
                    case UpdatePartyMessage.UpdateType.Setting:
                        ClientPartyHandlers.ClientParty.Setting(message.shareExp, message.shareItem);
                        break;
                    case UpdatePartyMessage.UpdateType.Terminate:
                        ClientPartyHandlers.ClientParty = null;
                        break;
                }
            }
            ClientPartyActions.NotifyPartyUpdated(ClientPartyHandlers.ClientParty);
        }

        protected void HandleUpdateGuildMemberAtClient(MessageHandlerData messageHandler)
        {
            UpdateSocialGroupMember(ClientGuildHandlers.ClientGuild, messageHandler.ReadMessage<UpdateSocialMemberMessage>());
            ClientGuildActions.NotifyGuildUpdated(ClientGuildHandlers.ClientGuild);
        }

        protected void HandleUpdateGuildAtClient(MessageHandlerData messageHandler)
        {
            UpdateGuildMessage message = messageHandler.ReadMessage<UpdateGuildMessage>();
            if (message.type == UpdateGuildMessage.UpdateType.Create)
            {
                ClientGuildHandlers.ClientGuild = new GuildData(message.id, message.guildName, message.characterId);
            }
            else if (ClientGuildHandlers.ClientGuild != null && ClientGuildHandlers.ClientGuild.id == message.id)
            {
                switch (message.type)
                {
                    case UpdateGuildMessage.UpdateType.ChangeLeader:
                        ClientGuildHandlers.ClientGuild.SetLeader(message.characterId);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMessage:
                        ClientGuildHandlers.ClientGuild.guildMessage = message.guildMessage;
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildRole:
                        ClientGuildHandlers.ClientGuild.SetRole(message.guildRole, message.roleName, message.canInvite, message.canKick, message.shareExpPercentage);
                        break;
                    case UpdateGuildMessage.UpdateType.SetGuildMemberRole:
                        ClientGuildHandlers.ClientGuild.SetMemberRole(message.characterId, message.guildRole);
                        break;
                    case UpdateGuildMessage.UpdateType.SetSkillLevel:
                        ClientGuildHandlers.ClientGuild.SetSkillLevel(message.dataId, message.level);
                        if (BasePlayerCharacterController.OwningCharacter != null)
                            BasePlayerCharacterController.OwningCharacter.ForceMakeCaches();
                        break;
                    case UpdateGuildMessage.UpdateType.SetGold:
                        ClientGuildHandlers.ClientGuild.gold = message.gold;
                        break;
                    case UpdateGuildMessage.UpdateType.LevelExpSkillPoint:
                        ClientGuildHandlers.ClientGuild.level = message.level;
                        ClientGuildHandlers.ClientGuild.exp = message.exp;
                        ClientGuildHandlers.ClientGuild.skillPoint = message.skillPoint;
                        break;
                    case UpdateGuildMessage.UpdateType.Terminate:
                        ClientGuildHandlers.ClientGuild = null;
                        if (BasePlayerCharacterController.OwningCharacter != null)
                            BasePlayerCharacterController.OwningCharacter.ForceMakeCaches();
                        break;
                }
            }
            ClientGuildActions.NotifyGuildUpdated(ClientGuildHandlers.ClientGuild);
        }

        protected void HandleUpdateFriendsAtClient(MessageHandlerData messageHandler)
        {
            ClientFriendActions.NotifyFriendsUpdated(messageHandler.Reader.GetArray<SocialCharacterData>());
        }

        protected void HandleUpdateMapInfoAtClient(MessageHandlerData messageHandler)
        {
            // Don't set map info again at server
            if (IsServer)
                return;
            UpdateMapInfoMessage message = messageHandler.ReadMessage<UpdateMapInfoMessage>();
            SetMapInfo(message.mapId);
        }

        protected void HandleUpdateDayNightTimeAtClient(MessageHandlerData messageHandler)
        {
            // Don't set time of day again at server
            if (IsServer)
                return;
            UpdateTimeOfDayMessage message = messageHandler.ReadMessage<UpdateTimeOfDayMessage>();
            CurrentGameInstance.DayNightTimeUpdater.SetTimeOfDay(message.timeOfDay);
        }

        protected void HandleNotifyOnlineCharacterAtClient(MessageHandlerData messageHandler)
        {
            NotifyOnlineCharacter(messageHandler.Reader.GetString());
        }

        protected void HandleNotifyRewardExpAtClient(MessageHandlerData messageHandler)
        {
            ClientGenericActions.NotifyRewardExp(messageHandler.Reader.GetPackedInt());
        }

        protected void HandleNotifyRewardGoldAtClient(MessageHandlerData messageHandler)
        {
            ClientGenericActions.NotifyRewardGold(messageHandler.Reader.GetPackedInt());
        }

        protected void HandleNotifyRewardItemAtClient(MessageHandlerData messageHandler)
        {
            ClientGenericActions.NotifyRewardItem(
                messageHandler.Reader.GetPackedInt(),
                messageHandler.Reader.GetPackedShort());
        }

        protected void HandleNotifyStorageOpenedAtClient(MessageHandlerData messageHandler)
        {
            ClientStorageActions.NotifyStorageOpened(
                (StorageType)messageHandler.Reader.GetByte(),
                messageHandler.Reader.GetString(),
                messageHandler.Reader.GetPackedUInt(),
                messageHandler.Reader.GetPackedShort(),
                messageHandler.Reader.GetPackedShort());
        }

        protected void HandleNotifyStorageClosedAtClient(MessageHandlerData messageHandler)
        {
            ClientStorageActions.NotifyStorageClosed();
        }

        protected void HandleNotifyStorageItemsUpdatedAtClient(MessageHandlerData messageHandler)
        {
            ClientStorageActions.NotifyStorageItemsUpdated(messageHandler.Reader.GetList<CharacterItem>());
        }

        protected void HandleNotifyPartyInvitationAtClient(MessageHandlerData messageHandler)
        {
            ClientPartyActions.NotifyPartyInvitation(messageHandler.ReadMessage<PartyInvitationData>());
        }

        protected void HandleNotifyGuildInvitationAtClient(MessageHandlerData messageHandler)
        {
            ClientGuildActions.NotifyGuildInvitation(messageHandler.ReadMessage<GuildInvitationData>());
        }

        protected virtual void HandleChatAtServer(MessageHandlerData messageHandler)
        {
            ReadChatMessage(FillChatChannelId(messageHandler.ReadMessage<ChatMessage>()));
        }

        protected ChatMessage FillChatChannelId(ChatMessage message)
        {
            IPlayerCharacterData playerCharacter;
            if (message.channel == ChatChannel.Party || message.channel == ChatChannel.Guild)
            {
                if (!string.IsNullOrEmpty(message.sender) &&
                    ServerUserHandlers.TryGetPlayerCharacterByName(message.sender, out playerCharacter))
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
            BasePlayerCharacterEntity playerCharacter;
            switch (message.channel)
            {
                case ChatChannel.Local:
                    if (!string.IsNullOrEmpty(message.sender) &&
                        ServerUserHandlers.TryGetPlayerCharacterByName(message.sender, out playerCharacter))
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
                                ServerSendPacket(receiver.ConnectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                            }
                            // Send messages to sender
                            ServerSendPacket(playerCharacter.ConnectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                        }
                    }
                    break;
                case ChatChannel.Global:
                    if (!string.IsNullOrEmpty(message.sender))
                    {
                        // Send message to all clients
                        ServerSendPacketToAllConnections(DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                    }
                    break;
                case ChatChannel.Whisper:
                    if (!string.IsNullOrEmpty(message.sender) &&
                        ServerUserHandlers.TryGetPlayerCharacterByName(message.sender, out playerCharacter))
                    {
                        // If found sender send whisper message to sender
                        ServerSendPacket(playerCharacter.ConnectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                    }
                    if (!string.IsNullOrEmpty(message.receiver) &&
                        ServerUserHandlers.TryGetPlayerCharacterByName(message.receiver, out playerCharacter))
                    {
                        // If found receiver send whisper message to receiver
                        ServerSendPacket(playerCharacter.ConnectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                    }
                    break;
                case ChatChannel.Party:
                    PartyData party;
                    if (GameInstance.ServerPartyHandlers.TryGetParty(message.channelId, out party))
                    {
                        foreach (string memberId in party.GetMemberIds())
                        {
                            if (ServerUserHandlers.TryGetPlayerCharacterById(memberId, out playerCharacter) &&
                                ContainsConnectionId(playerCharacter.ConnectionId))
                            {
                                // If party member is online, send party message to the member
                                ServerSendPacket(playerCharacter.ConnectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                            }
                        }
                    }
                    break;
                case ChatChannel.Guild:
                    GuildData guild;
                    if (GameInstance.ServerGuildHandlers.TryGetGuild(message.channelId, out guild))
                    {
                        foreach (string memberId in guild.GetMemberIds())
                        {
                            if (ServerUserHandlers.TryGetPlayerCharacterById(memberId, out playerCharacter) &&
                                ContainsConnectionId(playerCharacter.ConnectionId))
                            {
                                // If guild member is online, send guild message to the member
                                ServerSendPacket(playerCharacter.ConnectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                            }
                        }
                    }
                    break;
                case ChatChannel.System:
                    if (CanSendSystemAnnounce(message.sender))
                    {
                        // Send message to all clients
                        ServerSendPacketToAllConnections(DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
                    }
                    break;
            }
        }

        protected virtual void HandleRequestOnlineCharacter(MessageHandlerData messageHandler)
        {
            string characterId = messageHandler.Reader.GetString();
            if (IsCharacterOnline(characterId))
            {
                // Notify back online character
                ServerSendPacket(messageHandler.ConnectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyOnlineCharacter, (writer) =>
                {
                    writer.Put(characterId);
                });
            }
        }

        public virtual void InitPrefabs()
        {
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
            this.InvokeInstanceDevExtMethods("InitPrefabs");
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
            ClientSendPacket(DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, chatMessage);
        }

        public void Quit()
        {
            Application.Quit();
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

        public virtual void RegisterPlayerCharacter(long connectionId, BasePlayerCharacterEntity playerCharacter)
        {
            ServerUserHandlers.AddPlayerCharacter(connectionId, playerCharacter);
        }

        public virtual void UnregisterPlayerCharacter(long connectionId)
        {
            ServerUserHandlers.RemovePlayerCharacter(connectionId);
        }

        public virtual void RegisterUserId(long connectionId, string userId)
        {
            ServerUserHandlers.AddUserId(connectionId, userId);
        }

        public virtual void UnregisterUserId(long connectionId)
        {
            ServerUserHandlers.RemoveUserId(connectionId);
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
                BuildingEntities[buildingEntity.Id] = buildingEntity;
                buildingEntity.CallAllOnBuildingConstruct();
                return buildingEntity;
            }
            return null;
        }

        public virtual void DestroyBuildingEntity(string id)
        {
            if (BuildingEntities.ContainsKey(id))
            {
                BuildingEntities[id].Destroy();
                BuildingEntities.Remove(id);
            }
        }

        public bool TryGetBuildingEntity<T>(string id, out T entity) where T : BuildingEntity
        {
            entity = null;
            if (BuildingEntities.ContainsKey(id))
            {
                entity = BuildingEntities[id] as T;
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
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.UpdateMapInfo, message);
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
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.UpdateTimeOfDay, message);
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
                    ServerUserHandlers.TryGetPlayerCharacterByName(sender, out playerCharacter) &&
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
            HandleChatAtServer(new MessageHandlerData(GameNetworkingConsts.Chat, Server, -1, new NetDataReader(writer.CopyData())));
        }
    }
}
