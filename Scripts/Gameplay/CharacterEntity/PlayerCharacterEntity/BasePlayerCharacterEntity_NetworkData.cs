using Insthync.AddressableAssetTools;
using LiteNetLib;
using LiteNetLibManager;
using NotifiableCollection;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        #region Sync data
        [Category("Sync Fields")]
        [SerializeField]
        protected SyncFieldInt dataId = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt factionId = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldFloat statPoint = new SyncFieldFloat();
        [SerializeField]
        protected SyncFieldFloat skillPoint = new SyncFieldFloat();
        [SerializeField]
        protected SyncFieldInt gold = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt userGold = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt userCash = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt partyId = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt guildId = new SyncFieldInt();
#if !DISABLE_DIFFER_MAP_RESPAWNING
        [SerializeField]
        protected SyncFieldString respawnMapName = new SyncFieldString();
        [SerializeField]
        protected SyncFieldVector3 respawnPosition = new SyncFieldVector3();
#endif
        [SerializeField]
        protected SyncFieldInt iconDataId = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt frameDataId = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt titleDataId = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldLong lastDeadTime = new SyncFieldLong();
        [SerializeField]
        protected SyncFieldLong unmuteTime = new SyncFieldLong();
#if !DISABLE_CLASSIC_PK
        [SerializeField]
        protected SyncFieldBool isPkOn = new SyncFieldBool();
        [SerializeField]
        protected SyncFieldInt pkPoint = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt consecutivePkKills = new SyncFieldInt();
#endif
        [SerializeField]
        protected SyncFieldInt reputation = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldBool isWarping = new SyncFieldBool();

        [Category("Sync Lists")]
        [SerializeField]
        protected SyncListCharacterHotkey hotkeys = new SyncListCharacterHotkey();
        [SerializeField]
        protected SyncListCharacterQuest quests = new SyncListCharacterQuest();
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
        [SerializeField]
        protected SyncListCharacterCurrency currencies = new SyncListCharacterCurrency();
#endif
#if !DISABLE_CUSTOM_CHARACTER_DATA
        [SerializeField]
        private NotifiableList<CharacterDataBoolean> serverBools = new NotifiableList<CharacterDataBoolean>();
        [SerializeField]
        private NotifiableList<CharacterDataInt32> serverInts = new NotifiableList<CharacterDataInt32>();
        [SerializeField]
        private NotifiableList<CharacterDataFloat32> serverFloats = new NotifiableList<CharacterDataFloat32>();
        [SerializeField]
        protected SyncListCharacterDataBoolean privateBools = new SyncListCharacterDataBoolean();
        [SerializeField]
        protected SyncListCharacterDataInt32 privateInts = new SyncListCharacterDataInt32();
        [SerializeField]
        protected SyncListCharacterDataFloat32 privateFloats = new SyncListCharacterDataFloat32();
        [SerializeField]
        protected SyncListCharacterDataBoolean publicBools = new SyncListCharacterDataBoolean();
        [SerializeField]
        protected SyncListCharacterDataInt32 publicInts = new SyncListCharacterDataInt32();
        [SerializeField]
        protected SyncListCharacterDataFloat32 publicFloats = new SyncListCharacterDataFloat32();
#endif
        [SerializeField]
        protected SyncListCharacterSkill guildSkills = new SyncListCharacterSkill();
        #endregion

        #region Fields/Interface/Getter/Setter implementation
        public override int DataId { get { return dataId.Value; } set { dataId.Value = value; } }
        public override int FactionId { get { return factionId.Value; } set { factionId.Value = value; } }
        public float StatPoint { get { return statPoint.Value; } set { statPoint.Value = value; } }
        public float SkillPoint { get { return skillPoint.Value; } set { skillPoint.Value = value; } }
        public int Gold
        {
            get
            {
                if (CurrentGameInstance.goldStoreMode == GoldStoreMode.UserGoldOnly)
                    return UserGold;
                return gold.Value;
            }
            set
            {
                if (IsServer && CurrentGameInstance.goldStoreMode == GoldStoreMode.UserGoldOnly)
                {
                    GameInstance.ServerUserHandlers.ChangeUserGold(UserId, value - UserGold);
                    return;
                }
                gold.Value = value;
            }
        }
        public int UserGold { get { return userGold.Value; } set { userGold.Value = value; } }
        public int UserCash { get { return userCash.Value; } set { userCash.Value = value; } }
        public int PartyId { get { return partyId.Value; } set { partyId.Value = value; } }
        public int GuildId { get { return guildId.Value; } set { guildId.Value = value; } }
        public byte GuildRole { get; set; }
        public string UserId { get; set; }
        public byte UserLevel { get; set; }
        public string CurrentChannel { get { return CurrentGameManager.GetCurrentChannel(this); } set { } }
        public string CurrentMapName { get { return CurrentGameManager.GetCurrentMapId(this); } set { } }
        public Vec3 CurrentPosition
        {
            get { return CurrentGameManager.GetCurrentPosition(this); }
            set { CurrentGameManager.SetCurrentPosition(this, value); }
        }
        public Vec3 CurrentRotation
        {
            get
            {
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                    return EntityTransform.eulerAngles;
                return Quaternion.LookRotation(Direction2D).eulerAngles;
            }
            set
            {
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                {
                    EntityTransform.eulerAngles = value;
                    return;
                }
                Direction2D = Quaternion.Euler(value) * Vector3.forward;
            }
        }
        public string CurrentSafeArea { get { return CurrentGameManager.GetCurrentSafeArea(this); } set { } }
#if !DISABLE_DIFFER_MAP_RESPAWNING
        public string RespawnMapName
        {
            get { return respawnMapName.Value; }
            set { respawnMapName.Value = value; }
        }
        public Vec3 RespawnPosition
        {
            get { return respawnPosition.Value; }
            set { respawnPosition.Value = value; }
        }
#endif
        public override int IconDataId
        {
            get { return iconDataId.Value; }
            set { iconDataId.Value = value; }
        }
        public override int FrameDataId
        {
            get { return frameDataId.Value; }
            set { frameDataId.Value = value; }
        }
        public override int TitleDataId
        {
            get { return titleDataId.Value; }
            set { titleDataId.Value = value; }
        }
        public long LastDeadTime
        {
            get { return lastDeadTime.Value; }
            set { lastDeadTime.Value = value; }
        }
        public long UnmuteTime
        {
            get { return unmuteTime.Value; }
            set { unmuteTime.Value = value; }
        }
        public long LastUpdate { get; set; }
#if !DISABLE_CLASSIC_PK
        public bool IsPkOn
        {
            get { return isPkOn.Value; }
            set { isPkOn.Value = value; }
        }
        public long LastPkOnTime { get; set; }
        public int PkPoint
        {
            get { return pkPoint.Value; }
            set { pkPoint.Value = value; }
        }
        public int ConsecutivePkKills
        {
            get { return consecutivePkKills.Value; }
            set { consecutivePkKills.Value = value; }
        }
        public int HighestPkPoint { get; set; }
        public int HighestConsecutivePkKills { get; set; }
#endif

        public override int Reputation
        {
            get { return reputation.Value; }
            set { reputation.Value = value; }
        }

        public bool IsWarping
        {
            get { return isWarping.Value; }
            set { isWarping.Value = value; }
        }

        public IList<CharacterHotkey> Hotkeys
        {
            get { return hotkeys; }
            set
            {
                hotkeys.Clear();
                hotkeys.AddRange(value);
            }
        }

        public IList<CharacterQuest> Quests
        {
            get { return quests; }
            set
            {
                quests.Clear();
                quests.AddRange(value);
            }
        }

#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
        public IList<CharacterCurrency> Currencies
        {
            get { return currencies; }
            set
            {
                currencies.Clear();
                currencies.AddRange(value);
            }
        }
#endif

#if !DISABLE_CUSTOM_CHARACTER_DATA
        public IList<CharacterDataBoolean> ServerBools
        {
            get { return serverBools; }
            set
            {
                serverBools.Clear();
                serverBools.AddRange(value);
            }
        }

        public IList<CharacterDataInt32> ServerInts
        {
            get { return serverInts; }
            set
            {
                serverInts.Clear();
                serverInts.AddRange(value);
            }
        }

        public IList<CharacterDataFloat32> ServerFloats
        {
            get { return serverFloats; }
            set
            {
                serverFloats.Clear();
                serverFloats.AddRange(value);
            }
        }

        public IList<CharacterDataBoolean> PrivateBools
        {
            get { return privateBools; }
            set
            {
                privateBools.Clear();
                privateBools.AddRange(value);
            }
        }

        public IList<CharacterDataInt32> PrivateInts
        {
            get { return privateInts; }
            set
            {
                privateInts.Clear();
                privateInts.AddRange(value);
            }
        }

        public IList<CharacterDataFloat32> PrivateFloats
        {
            get { return privateFloats; }
            set
            {
                privateFloats.Clear();
                privateFloats.AddRange(value);
            }
        }

        public IList<CharacterDataBoolean> PublicBools
        {
            get { return publicBools; }
            set
            {
                publicBools.Clear();
                publicBools.AddRange(value);
            }
        }

        public IList<CharacterDataInt32> PublicInts
        {
            get { return publicInts; }
            set
            {
                publicInts.Clear();
                publicInts.AddRange(value);
            }
        }

        public IList<CharacterDataFloat32> PublicFloats
        {
            get { return publicFloats; }
            set
            {
                publicFloats.Clear();
                publicFloats.AddRange(value);
            }
        }
#endif

        public IList<CharacterSkill> GuildSkills
        {
            get { return guildSkills; }
            set
            {
                guildSkills.Clear();
                guildSkills.AddRange(value);
            }
        }
        #endregion

        #region Network setup functions
        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            // Sync fields
            dataId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            dataId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;

            factionId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            factionId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;

            statPoint.deliveryMethod = DeliveryMethod.ReliableOrdered;
            statPoint.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;

            skillPoint.deliveryMethod = DeliveryMethod.ReliableOrdered;
            skillPoint.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;

            gold.deliveryMethod = DeliveryMethod.ReliableOrdered;
            gold.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;

            userGold.deliveryMethod = DeliveryMethod.ReliableOrdered;
            userGold.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;

            userCash.deliveryMethod = DeliveryMethod.ReliableOrdered;
            userCash.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;

            partyId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            partyId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;

            guildId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            guildId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
#if !DISABLE_DIFFER_MAP_RESPAWNING
            respawnMapName.deliveryMethod = DeliveryMethod.ReliableOrdered;
            respawnMapName.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;

            respawnPosition.deliveryMethod = DeliveryMethod.ReliableOrdered;
            respawnPosition.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
#endif
            iconDataId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            iconDataId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;

            frameDataId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            frameDataId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;

            titleDataId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            titleDataId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;

            lastDeadTime.deliveryMethod = DeliveryMethod.ReliableOrdered;
            lastDeadTime.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
#if !DISABLE_CLASSIC_PK
            isPkOn.deliveryMethod = DeliveryMethod.ReliableOrdered;
            isPkOn.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;

            pkPoint.deliveryMethod = DeliveryMethod.ReliableOrdered;
            pkPoint.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;

            consecutivePkKills.deliveryMethod = DeliveryMethod.ReliableOrdered;
            consecutivePkKills.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
#endif
            reputation.deliveryMethod = DeliveryMethod.ReliableOrdered;
            reputation.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;

            isWarping.deliveryMethod = DeliveryMethod.ReliableOrdered;
            isWarping.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;

            pitch.deliveryMethod = DeliveryMethod.Sequenced;
            pitch.clientDeliveryMethod = DeliveryMethod.Sequenced;
            pitch.syncMode = LiteNetLibSyncField.SyncMode.ClientMulticast;

            lookPosition.deliveryMethod = DeliveryMethod.Sequenced;
            lookPosition.clientDeliveryMethod = DeliveryMethod.Sequenced;
            lookPosition.syncMode = LiteNetLibSyncField.SyncMode.ClientMulticast;

            targetEntityId.dataChannel = ACTION_DATA_CHANNEL;
            targetEntityId.clientDataChannel = ACTION_DATA_CHANNEL;
            targetEntityId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            targetEntityId.clientDeliveryMethod = DeliveryMethod.ReliableOrdered;
            targetEntityId.syncMode = LiteNetLibSyncField.SyncMode.ClientMulticast;
            // Sync lists
            hotkeys.forOwnerOnly = true;
            quests.forOwnerOnly = true;
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
            currencies.forOwnerOnly = true;
#endif
#if !DISABLE_CUSTOM_CHARACTER_DATA
            privateBools.forOwnerOnly = true;
            privateInts.forOwnerOnly = true;
            privateFloats.forOwnerOnly = true;
            publicBools.forOwnerOnly = false;
            publicInts.forOwnerOnly = false;
            publicFloats.forOwnerOnly = false;
#endif
            guildSkills.forOwnerOnly = false;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // On data changes events
            id.onChange += OnPlayerIdChange;
            syncTitle.onChange += OnPlayerCharacterNameChange;
            dataId.onChange += OnDataIdChange;
            factionId.onChange += OnFactionIdChange;
            statPoint.onChange += OnStatPointChange;
            skillPoint.onChange += OnSkillPointChange;
            gold.onChange += OnGoldChange;
            userGold.onChange += OnUserGoldChange;
            userCash.onChange += OnUserCashChange;
            partyId.onChange += OnPartyIdChange;
            guildId.onChange += OnGuildIdChange;
            iconDataId.onChange += OnIconDataIdChange;
            frameDataId.onChange += OnFrameDataIdChange;
            titleDataId.onChange += OnTitleDataIdChange;
#if !DISABLE_CLASSIC_PK
            isPkOn.onChange += OnIsPkOnChange;
            pkPoint.onChange += OnPkPointChange;
            consecutivePkKills.onChange += OnConsecutivePkKillsChange;
#endif
            reputation.onChange += OnReputationChange;
            isWarping.onChange += OnIsWarpingChange;
            // On list changes events
            hotkeys.onOperation += OnHotkeysOperation;
            quests.onOperation += OnQuestsOperation;
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
            currencies.onOperation += OnCurrenciesOperation;
#endif
#if !DISABLE_CUSTOM_CHARACTER_DATA
            serverBools.ListChanged += OnServerBoolsOperation;
            serverInts.ListChanged += OnServerIntsOperation;
            serverFloats.ListChanged += OnServerFloatsOperation;
            privateBools.onOperation += OnPrivateBoolsOperation;
            privateInts.onOperation += OnPrivateIntsOperation;
            privateFloats.onOperation += OnPrivateFloatsOperation;
            publicBools.onOperation += OnPublicBoolsOperation;
            publicInts.onOperation += OnPublicIntsOperation;
            publicFloats.onOperation += OnPublicFloatsOperation;
#endif
            guildSkills.onOperation += OnGuildSkillsOperation;
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            // On data changes events
            id.onChange -= OnPlayerIdChange;
            syncTitle.onChange -= OnPlayerCharacterNameChange;
            dataId.onChange -= OnDataIdChange;
            factionId.onChange -= OnFactionIdChange;
            statPoint.onChange -= OnStatPointChange;
            skillPoint.onChange -= OnSkillPointChange;
            gold.onChange -= OnGoldChange;
            userGold.onChange -= OnUserGoldChange;
            userCash.onChange -= OnUserCashChange;
            partyId.onChange -= OnPartyIdChange;
            guildId.onChange -= OnGuildIdChange;
            iconDataId.onChange -= OnIconDataIdChange;
            frameDataId.onChange -= OnFrameDataIdChange;
            titleDataId.onChange -= OnTitleDataIdChange;
#if !DISABLE_CLASSIC_PK
            isPkOn.onChange -= OnIsPkOnChange;
            pkPoint.onChange -= OnPkPointChange;
            consecutivePkKills.onChange -= OnConsecutivePkKillsChange;
#endif
            reputation.onChange -= OnReputationChange;
            isWarping.onChange -= OnIsWarpingChange;
            // On list changes events
            hotkeys.onOperation -= OnHotkeysOperation;
            quests.onOperation -= OnQuestsOperation;
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
            currencies.onOperation -= OnCurrenciesOperation;
#endif
#if !DISABLE_CUSTOM_CHARACTER_DATA
            serverBools.ListChanged -= OnServerBoolsOperation;
            serverInts.ListChanged -= OnServerIntsOperation;
            serverFloats.ListChanged -= OnServerFloatsOperation;
            privateBools.onOperation -= OnPrivateBoolsOperation;
            privateInts.onOperation -= OnPrivateIntsOperation;
            privateFloats.onOperation -= OnPrivateFloatsOperation;
            publicBools.onOperation -= OnPublicBoolsOperation;
            publicInts.onOperation -= OnPublicIntsOperation;
            publicFloats.onOperation -= OnPublicFloatsOperation;
#endif
            guildSkills.onOperation -= OnGuildSkillsOperation;

            // Unsubscribe this entity
            if (GameInstance.ClientCharacterHandlers != null)
                GameInstance.ClientCharacterHandlers.UnsubscribePlayerCharacter(this);

            if (IsOwnerClient && BasePlayerCharacterController.Singleton != null)
                Destroy(BasePlayerCharacterController.Singleton.gameObject);
        }

        protected override async void EntityOnSetOwnerClient()
        {
            base.EntityOnSetOwnerClient();

            // Setup relates elements
            if (IsOwnerClient)
            {
                BasePlayerCharacterController prefab = null;
#if !EXCLUDE_PREFAB_REFS
                if (ControllerPrefab != null)
                {
                    prefab = ControllerPrefab;
                }
                else if (CurrentGameInstance.DefaultControllerPrefab != null)
                {
                    prefab = CurrentGameInstance.DefaultControllerPrefab;
                }
#endif
                if (prefab != null)
                {
                    // Do nothing, just have it to make it able to compile properly (it have compile condition above)
                }
                else if (AddressableControllerPrefab.IsDataValid())
                {
                    prefab = await AddressableControllerPrefab.GetOrLoadAssetAsync<BasePlayerCharacterController>();
                }
                else if (CurrentGameInstance.AddressableDefaultControllerPrefab.IsDataValid())
                {
                    prefab = await CurrentGameInstance.AddressableDefaultControllerPrefab.GetOrLoadAssetAsync<BasePlayerCharacterController>();
                }
                else if (BasePlayerCharacterController.Singleton != null)
                {
                    prefab = BasePlayerCharacterController.LastPrefab;
                }
                else
                {
                    Logging.LogWarning(ToString(), "`Controller Prefab` is empty so it cannot be instantiated");
                    prefab = null;
                }
                if (prefab != null)
                {
                    BasePlayerCharacterController.LastPrefab = prefab;
                    BasePlayerCharacterController controller = Instantiate(prefab);
                    controller.PlayingCharacterEntity = this;
                }
                // Instantiates owning character objects
                if (CurrentGameInstance.owningCharacterObjects != null && CurrentGameInstance.owningCharacterObjects.Length > 0)
                {
                    foreach (GameObject obj in CurrentGameInstance.owningCharacterObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, EntityTransform.position, EntityTransform.rotation, EntityTransform);
                    }
                }
                // Instantiates owning character minimap objects
                if (CurrentGameInstance.owningCharacterMiniMapObjects != null && CurrentGameInstance.owningCharacterMiniMapObjects.Length > 0)
                {
                    foreach (GameObject obj in CurrentGameInstance.owningCharacterMiniMapObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, MiniMapUiTransform.position, MiniMapUiTransform.rotation, MiniMapUiTransform);
                    }
                }
                // Instantiates owning character UI
                if (CurrentGameInstance.owningCharacterUI != null)
                {
                    InstantiateUI(CurrentGameInstance.owningCharacterUI);
                }
            }
            else if (IsClient)
            {
                // Instantiates non-owning character objects
                if (CurrentGameInstance.nonOwningCharacterObjects != null && CurrentGameInstance.nonOwningCharacterObjects.Length > 0)
                {
                    foreach (GameObject obj in CurrentGameInstance.nonOwningCharacterObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, EntityTransform.position, EntityTransform.rotation, EntityTransform);
                    }
                }
                // Instantiates non-owning character minimap objects
                if (CurrentGameInstance.nonOwningCharacterMiniMapObjects != null && CurrentGameInstance.nonOwningCharacterMiniMapObjects.Length > 0)
                {
                    foreach (GameObject obj in CurrentGameInstance.nonOwningCharacterMiniMapObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, MiniMapUiTransform.position, MiniMapUiTransform.rotation, MiniMapUiTransform);
                    }
                }
                // Instantiates non-owning character UI
                if (CurrentGameInstance.nonOwningCharacterUI != null)
                {
                    InstantiateUI(CurrentGameInstance.nonOwningCharacterUI);
                }
            }
        }
        #endregion

        #region Sync data changes callback
        private void OnPlayerIdChange(bool isInitial, string id)
        {
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(CharacterName) && GameInstance.ClientCharacterHandlers != null)
                GameInstance.ClientCharacterHandlers.SubscribePlayerCharacter(this);
        }

        private void OnPlayerCharacterNameChange(bool isInitial, string characterName)
        {
            if (!string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(characterName) && GameInstance.ClientCharacterHandlers != null)
                GameInstance.ClientCharacterHandlers.SubscribePlayerCharacter(this);
        }

        private void OnDataIdChange(bool isInitial, int dataId)
        {
            IsRecaching = true;
            if (onDataIdChange != null)
                onDataIdChange.Invoke(dataId);
        }

        private void OnFactionIdChange(bool isInitial, int factionId)
        {
            IsRecaching = true;
            if (onFactionIdChange != null)
                onFactionIdChange.Invoke(factionId);
        }

        private void OnStatPointChange(bool isInitial, float statPoint)
        {
            if (onStatPointChange != null)
                onStatPointChange.Invoke(statPoint);
        }

        private void OnSkillPointChange(bool isInitial, float skillPoint)
        {
            if (onSkillPointChange != null)
                onSkillPointChange.Invoke(skillPoint);
        }

        private void OnGoldChange(bool isInitial, int gold)
        {
            if (onGoldChange != null)
                onGoldChange.Invoke(gold);
        }

        private void OnUserGoldChange(bool isInitial, int gold)
        {
            if (onUserGoldChange != null)
                onUserGoldChange.Invoke(gold);
        }

        private void OnUserCashChange(bool isInitial, int gold)
        {
            if (onUserCashChange != null)
                onUserCashChange.Invoke(gold);
        }

        private void OnPartyIdChange(bool isInitial, int partyId)
        {
            IsRecaching = true;
            if (onPartyIdChange != null)
                onPartyIdChange.Invoke(partyId);
        }

        private void OnGuildIdChange(bool isInitial, int guildId)
        {
            IsRecaching = true;
            if (onGuildIdChange != null)
                onGuildIdChange.Invoke(guildId);
        }

        private void OnIconDataIdChange(bool isInitial, int iconDataId)
        {
            IsRecaching = true;
            if (onIconDataIdChange != null)
                onIconDataIdChange.Invoke(iconDataId);
        }

        private void OnFrameDataIdChange(bool isInitial, int frameDataId)
        {
            IsRecaching = true;
            if (onFrameDataIdChange != null)
                onFrameDataIdChange.Invoke(frameDataId);
        }

        private void OnTitleDataIdChange(bool isInitial, int titleDataId)
        {
            IsRecaching = true;
            if (onTitleDataIdChange != null)
                onTitleDataIdChange.Invoke(titleDataId);
        }

#if !DISABLE_CLASSIC_PK
        private void OnIsPkOnChange(bool isInitial, bool isPkOn)
        {
            if (onIsPkOnChange != null)
                onIsPkOnChange.Invoke(isPkOn);
        }

        private void OnPkPointChange(bool isInitial, int pkPoint)
        {
            if (onPkPointChange != null)
                onPkPointChange.Invoke(pkPoint);
        }

        private void OnConsecutivePkKillsChange(bool isInitial, int consecutivePkKills)
        {
            if (onConsecutivePkKillsChange != null)
                onConsecutivePkKillsChange.Invoke(consecutivePkKills);
        }
#endif

        private void OnReputationChange(bool isInitial, int reputation)
        {
            if (onReputationChange != null)
                onReputationChange.Invoke(reputation);
        }

        private void OnIsWarpingChange(bool isInitial, bool isWarping)
        {
            if (onIsWarpingChange != null)
                onIsWarpingChange.Invoke(isWarping);
        }
        #endregion

        #region Net functions operation callback
        private void OnHotkeysOperation(LiteNetLibSyncListOp operation, int index, CharacterHotkey oldItem, CharacterHotkey newItem)
        {
            if (onHotkeysOperation != null)
                onHotkeysOperation.Invoke(operation, index, oldItem, newItem);
        }

        private void OnQuestsOperation(LiteNetLibSyncListOp operation, int index, CharacterQuest oldItem, CharacterQuest newItem)
        {
            if (onQuestsOperation != null)
                onQuestsOperation.Invoke(operation, index, oldItem, newItem);
        }

#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
        private void OnCurrenciesOperation(LiteNetLibSyncListOp operation, int index, CharacterCurrency oldItem, CharacterCurrency newItem)
        {
            if (onCurrenciesOperation != null)
                onCurrenciesOperation.Invoke(operation, index, oldItem, newItem);
        }
#endif

#if !DISABLE_CUSTOM_CHARACTER_DATA

        private void OnServerBoolsOperation(NotifiableListAction action, int index, CharacterDataBoolean oldItem, CharacterDataBoolean newItem)
        {
            if (onServerBoolsOperation != null)
                onServerBoolsOperation.Invoke(action, index, oldItem, newItem);
        }

        private void OnServerIntsOperation(NotifiableListAction action, int index, CharacterDataInt32 oldItem, CharacterDataInt32 newItem)
        {
            if (onServerIntsOperation != null)
                onServerIntsOperation.Invoke(action, index, oldItem, newItem);
        }

        private void OnServerFloatsOperation(NotifiableListAction action, int index, CharacterDataFloat32 oldItem, CharacterDataFloat32 newItem)
        {
            if (onServerFloatsOperation != null)
                onServerFloatsOperation.Invoke(action, index, oldItem, newItem);
        }

        private void OnPrivateBoolsOperation(LiteNetLibSyncListOp operation, int index, CharacterDataBoolean oldItem, CharacterDataBoolean newItem)
        {
            if (onPrivateBoolsOperation != null)
                onPrivateBoolsOperation.Invoke(operation, index, oldItem, newItem);
        }

        private void OnPrivateIntsOperation(LiteNetLibSyncListOp operation, int index, CharacterDataInt32 oldItem, CharacterDataInt32 newItem)
        {
            if (onPrivateIntsOperation != null)
                onPrivateIntsOperation.Invoke(operation, index, oldItem, newItem);
        }

        private void OnPrivateFloatsOperation(LiteNetLibSyncListOp operation, int index, CharacterDataFloat32 oldItem, CharacterDataFloat32 newItem)
        {
            if (onPrivateFloatsOperation != null)
                onPrivateFloatsOperation.Invoke(operation, index, oldItem, newItem);
        }

        private void OnPublicBoolsOperation(LiteNetLibSyncListOp operation, int index, CharacterDataBoolean oldItem, CharacterDataBoolean newItem)
        {
            if (onPublicBoolsOperation != null)
                onPublicBoolsOperation.Invoke(operation, index, oldItem, newItem);
        }

        private void OnPublicIntsOperation(LiteNetLibSyncListOp operation, int index, CharacterDataInt32 oldItem, CharacterDataInt32 newItem)
        {
            if (onPublicIntsOperation != null)
                onPublicIntsOperation.Invoke(operation, index, oldItem, newItem);
        }

        private void OnPublicFloatsOperation(LiteNetLibSyncListOp operation, int index, CharacterDataFloat32 oldItem, CharacterDataFloat32 newItem)
        {
            if (onPublicFloatsOperation != null)
                onPublicFloatsOperation.Invoke(operation, index, oldItem, newItem);
        }
#endif

        private void OnGuildSkillsOperation(LiteNetLibSyncListOp operation, int index, CharacterSkill oldItem, CharacterSkill newItem)
        {
            switch (operation)
            {
                case LiteNetLibSyncListOp.Set:
                case LiteNetLibSyncListOp.Dirty:
                    if (oldItem.dataId != newItem.dataId ||
                        oldItem.level != newItem.level)
                        IsRecaching = true;
                    break;
                default:
                    IsRecaching = true;
                    break;
            }
            if (onGuildSkillsOperation != null)
                onGuildSkillsOperation.Invoke(operation, index, oldItem, newItem);
        }
        #endregion
    }
}
