using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

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
        [SerializeField]
        protected SyncFieldString respawnMapName = new SyncFieldString();
        [SerializeField]
        protected SyncFieldVector3 respawnPosition = new SyncFieldVector3();
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
        [SerializeField]
        protected SyncFieldBool isWarping = new SyncFieldBool();

        [Category("Sync Lists")]
        [SerializeField]
        protected SyncListCharacterHotkey hotkeys = new SyncListCharacterHotkey();
        [SerializeField]
        protected SyncListCharacterQuest quests = new SyncListCharacterQuest();
        [SerializeField]
        protected SyncListCharacterCurrency currencies = new SyncListCharacterCurrency();
        #endregion

        #region Fields/Interface/Getter/Setter implementation
        public override int DataId { get { return dataId.Value; } set { dataId.Value = value; } }
        public int FactionId { get { return factionId.Value; } set { factionId.Value = value; } }
        public float StatPoint { get { return statPoint.Value; } set { statPoint.Value = value; } }
        public float SkillPoint { get { return skillPoint.Value; } set { skillPoint.Value = value; } }
        public int Gold { get { return gold.Value; } set { gold.Value = value; } }
        public int UserGold { get { return userGold.Value; } set { userGold.Value = value; } }
        public int UserCash { get { return userCash.Value; } set { userCash.Value = value; } }
        public int PartyId { get { return partyId.Value; } set { partyId.Value = value; } }
        public int GuildId { get { return guildId.Value; } set { guildId.Value = value; } }
        public byte GuildRole { get; set; }
        public int SharedGuildExp { get; set; }
        public string UserId { get; set; }
        public byte UserLevel { get; set; }
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
        public int MountDataId
        {
            get
            {
                if (PassengingVehicleEntity != null &&
                    !PassengingVehicleEntity.Entity.IsSceneObject &&
                    PassengingVehicleEntity.IsDriver(PassengingVehicleSeatIndex))
                {
                    return PassengingVehicleEntity.Entity.Identity.HashAssetId;
                }
                return 0;
            }
            set { }
        }
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

        public IList<CharacterCurrency> Currencies
        {
            get { return currencies; }
            set
            {
                currencies.Clear();
                currencies.AddRange(value);
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
            respawnMapName.deliveryMethod = DeliveryMethod.ReliableOrdered;
            respawnMapName.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            respawnPosition.deliveryMethod = DeliveryMethod.ReliableOrdered;
            respawnPosition.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            iconDataId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            iconDataId.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            frameDataId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            frameDataId.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            titleDataId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            titleDataId.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            lastDeadTime.deliveryMethod = DeliveryMethod.Sequenced;
            lastDeadTime.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            isWarping.deliveryMethod = DeliveryMethod.ReliableOrdered;
            isWarping.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
            pitch.deliveryMethod = DeliveryMethod.Sequenced;
            pitch.syncMode = LiteNetLibSyncField.SyncMode.ClientMulticast;
            targetEntityId.clientDataChannel = STATE_DATA_CHANNEL;
            targetEntityId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            targetEntityId.syncMode = LiteNetLibSyncField.SyncMode.ClientMulticast;
            // Sync lists
            hotkeys.forOwnerOnly = true;
            quests.forOwnerOnly = true;
            currencies.forOwnerOnly = true;
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
            isWarping.onChange += OnIsWarpingChange;
            // On list changes events
            hotkeys.onOperation += OnHotkeysOperation;
            quests.onOperation += OnQuestsOperation;
            currencies.onOperation += OnCurrenciesOperation;
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
            isWarping.onChange -= OnIsWarpingChange;
            // On list changes events
            hotkeys.onOperation -= OnHotkeysOperation;
            quests.onOperation -= OnQuestsOperation;
            currencies.onOperation -= OnCurrenciesOperation;
            // Unsubscribe this entity
            if (GameInstance.ClientCharacterHandlers != null)
                GameInstance.ClientCharacterHandlers.UnsubscribePlayerCharacter(this);

            if (IsOwnerClient && BasePlayerCharacterController.Singleton != null)
                Destroy(BasePlayerCharacterController.Singleton.gameObject);
        }

        protected override void EntityOnSetOwnerClient()
        {
            base.EntityOnSetOwnerClient();

            // Setup relates elements
            if (IsOwnerClient)
            {
                BasePlayerCharacterController prefab;
                if (ControllerPrefab != null)
                {
                    prefab = ControllerPrefab;
                }
                else if (CurrentGameInstance.defaultControllerPrefab != null)
                {
                    prefab = CurrentGameInstance.defaultControllerPrefab;
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
            if (onDataIdChange != null)
                onDataIdChange.Invoke(dataId);
        }

        private void OnFactionIdChange(bool isInitial, int factionId)
        {
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
            if (onPartyIdChange != null)
                onPartyIdChange.Invoke(partyId);
        }

        private void OnGuildIdChange(bool isInitial, int guildId)
        {
            if (onGuildIdChange != null)
                onGuildIdChange.Invoke(guildId);
        }

        private void OnIconDataIdChange(bool isInitial, int guildId)
        {
            if (onIconDataIdChange != null)
                onIconDataIdChange.Invoke(guildId);
        }

        private void OnFrameDataIdChange(bool isInitial, int guildId)
        {
            if (onFrameDataIdChange != null)
                onFrameDataIdChange.Invoke(guildId);
        }

        private void OnTitleDataIdChange(bool isInitial, int guildId)
        {
            _isRecaching = true;
            if (onTitleDataIdChange != null)
                onTitleDataIdChange.Invoke(guildId);
        }

        private void OnIsWarpingChange(bool isInitial, bool isWarping)
        {
            if (onIsWarpingChange != null)
                onIsWarpingChange.Invoke(isWarping);
        }
        #endregion

        #region Net functions operation callback
        private void OnHotkeysOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (onHotkeysOperation != null)
                onHotkeysOperation.Invoke(operation, index);
        }

        private void OnQuestsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (onQuestsOperation != null)
                onQuestsOperation.Invoke(operation, index);
        }

        private void OnCurrenciesOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (onCurrenciesOperation != null)
                onCurrenciesOperation.Invoke(operation, index);
        }
        #endregion
    }
}
