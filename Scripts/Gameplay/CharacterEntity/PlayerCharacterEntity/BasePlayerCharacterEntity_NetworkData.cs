using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        #region Sync data
        [Header("Player Character - Sync Fields")]
        [SerializeField]
        protected SyncFieldInt dataId = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldShort statPoint = new SyncFieldShort();
        [SerializeField]
        protected SyncFieldShort skillPoint = new SyncFieldShort();
        [SerializeField]
        protected SyncFieldInt gold = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt partyId = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt guildId = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldBool isWarping = new SyncFieldBool();
        [Header("Player Character - Sync Lists")]
        [SerializeField]
        protected SyncListCharacterHotkey hotkeys = new SyncListCharacterHotkey();
        [SerializeField]
        protected SyncListCharacterQuest quests = new SyncListCharacterQuest();
        #endregion

        #region Dealing System
        protected DealingState dealingState = DealingState.None;
        protected int dealingGold = 0;
        protected DealingCharacterItems dealingItems = new DealingCharacterItems();
        public DealingState DealingState
        {
            get { return dealingState; }
            set
            {
                dealingState = value;
                RequestUpdateDealingState(value);
                if (DealingCharacter != null)
                    DealingCharacter.RequestUpdateAnotherDealingState(value);
            }
        }

        public int DealingGold
        {
            get { return dealingGold; }
            set
            {
                dealingGold = value;
                RequestUpdateDealingGold(value);
                if (DealingCharacter != null)
                    DealingCharacter.RequestUpdateAnotherDealingGold(value);
            }
        }

        public DealingCharacterItems DealingItems
        {
            get { return dealingItems; }
            set
            {
                dealingItems = value;
                RequestUpdateDealingItems(value);
                if (DealingCharacter != null)
                    DealingCharacter.RequestUpdateAnotherDealingItems(value);
            }
        }
        #endregion

        #region Storage System
        protected StorageId currentStorageId;
        protected SyncListCharacterItem storageItems = new SyncListCharacterItem();
        public IList<CharacterItem> StorageItems
        {
            get { return storageItems; }
            set
            {
                storageItems.Clear();
                foreach (CharacterItem entry in value)
                    storageItems.Add(entry);
            }
        }
        public System.Action<LiteNetLibSyncList.Operation, int> onStorageItemsOperation;
        #endregion

        #region Sync data actions
        public System.Action<int> onDataIdChange;
        public System.Action<short> onStatPointChange;
        public System.Action<short> onSkillPointChange;
        public System.Action<int> onGoldChange;
        public System.Action<int> onPartyIdChange;
        public System.Action<int> onGuildIdChange;
        public System.Action<bool> onIsWarpingChange;
        // List
        public System.Action<LiteNetLibSyncList.Operation, int> onHotkeysOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onQuestsOperation;
        #endregion

        #region Fields/Interface/Getter/Setter implementation
        public override int DataId { get { return dataId.Value; } set { dataId.Value = value; } }
        public short StatPoint { get { return statPoint.Value; } set { statPoint.Value = value; } }
        public short SkillPoint { get { return skillPoint.Value; } set { skillPoint.Value = value; } }
        public int Gold { get { return gold.Value; } set { gold.Value = value; } }
        public int PartyId { get { return partyId.Value; } set { partyId.Value = value; } }
        public int GuildId { get { return guildId.Value; } set { guildId.Value = value; } }
        public string GuildName { get { return syncTitle2.Value; } set { syncTitle2.Value = value; } }
        public bool IsWarping { get { return isWarping.Value; } set { isWarping.Value = value; } }
        public byte GuildRole { get; set; }
        public int SharedGuildExp { get; set; }
        public byte UserLevel { get; set; }
        public string CurrentMapName { get { return gameManager.GetCurrentMapId(this); } set { } }
        public Vector3 CurrentPosition
        {
            get { return gameManager.GetCurrentPosition(this); }
            set { gameManager.SetCurrentPosition(this, value); }
        }
        public string RespawnMapName { get; set; }
        public Vector3 RespawnPosition { get; set; }
        public int LastUpdate { get; set; }

        public IList<CharacterHotkey> Hotkeys
        {
            get { return hotkeys; }
            set
            {
                hotkeys.Clear();
                foreach (CharacterHotkey entry in value)
                    hotkeys.Add(entry);
            }
        }

        public IList<CharacterQuest> Quests
        {
            get { return quests; }
            set
            {
                quests.Clear();
                foreach (CharacterQuest entry in value)
                    quests.Add(entry);
            }
        }
        #endregion

        public void ClearParty()
        {
            PartyId = 0;
        }

        public void ClearGuild()
        {
            GuildId = 0;
            GuildName = string.Empty;
            GuildRole = 0;
            SharedGuildExp = 0;
        }

        #region Sync data changes callback
        protected virtual void OnDataIdChange(int dataId)
        {
            if (onDataIdChange != null)
                onDataIdChange.Invoke(dataId);
        }

        protected virtual void OnStatPointChange(short statPoint)
        {
            if (onStatPointChange != null)
                onStatPointChange.Invoke(statPoint);
        }

        protected virtual void OnSkillPointChange(short skillPoint)
        {
            if (onSkillPointChange != null)
                onSkillPointChange.Invoke(skillPoint);
        }

        protected virtual void OnGoldChange(int gold)
        {
            if (onGoldChange != null)
                onGoldChange.Invoke(gold);
        }

        protected virtual void OnPartyIdChange(int partyId)
        {
            if (onPartyIdChange != null)
                onPartyIdChange.Invoke(partyId);
        }

        protected virtual void OnGuildIdChange(int guildId)
        {
            if (onGuildIdChange != null)
                onGuildIdChange.Invoke(guildId);
        }

        protected virtual void OnIsWarpingChange(bool isWarping)
        {
            if (onIsWarpingChange != null)
                onIsWarpingChange.Invoke(isWarping);
        }
        #endregion

        #region Net functions operation callback
        protected virtual void OnHotkeysOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (onHotkeysOperation != null)
                onHotkeysOperation.Invoke(operation, index);
        }

        protected virtual void OnQuestsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (onQuestsOperation != null)
                onQuestsOperation.Invoke(operation, index);
        }

        protected virtual void OnStorageItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (onStorageItemsOperation != null)
                onStorageItemsOperation.Invoke(operation, index);
        }
        #endregion
    }
}
