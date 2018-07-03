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
        [SerializeField]
        protected SyncFieldShort statPoint = new SyncFieldShort();
        [SerializeField]
        protected SyncFieldShort skillPoint = new SyncFieldShort();
        [SerializeField]
        protected SyncFieldInt gold = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldByte dealingState = new SyncFieldByte();
        [SerializeField]
        protected SyncFieldInt dealingGold = new SyncFieldInt();
        // List
        [SerializeField]
        protected SyncListCharacterHotkey hotkeys = new SyncListCharacterHotkey();
        [SerializeField]
        protected SyncListCharacterQuest quests = new SyncListCharacterQuest();
        [SerializeField]
        protected SyncListCharacterItem dealingItems = new SyncListCharacterItem();
        #endregion

        #region Sync data actions
        public System.Action<short> onStatPointChange;
        public System.Action<short> onSkillPointChange;
        public System.Action<int> onGoldChange;
        public System.Action<DealingState> onDealingStateChange;
        public System.Action<int> onDealingGoldChange;
        // List
        public System.Action<LiteNetLibSyncList.Operation, int> onHotkeysOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onQuestsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onDealingItemsOperation;
        #endregion

        #region Fields/Interface implementation
        public short StatPoint { get { return statPoint.Value; } set { statPoint.Value = value; } }
        public short SkillPoint { get { return skillPoint.Value; } set { skillPoint.Value = value; } }
        public int Gold { get { return gold.Value; } set { gold.Value = value; } }
        public DealingState DealingState { get { return (DealingState)dealingState.Value; } set { dealingState.Value = (byte)value; } }
        public int DealingGold { get { return dealingGold.Value; } set { dealingGold.Value = value; } }
        public string CurrentMapName { get { return SceneManager.GetActiveScene().name; } set { } }
        public Vector3 CurrentPosition
        {
            get { return CacheTransform.position; }
            set
            {
                CacheNetTransform.Teleport(value, CacheTransform.rotation);
                CacheTransform.position = value;
            }
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
                foreach (var entry in value)
                    hotkeys.Add(entry);
            }
        }

        public IList<CharacterQuest> Quests
        {
            get { return quests; }
            set
            {
                quests.Clear();
                foreach (var entry in value)
                    quests.Add(entry);
            }
        }

        public IList<CharacterItem> DealingItems
        {
            get { return dealingItems; }
            set
            {
                dealingItems.Clear();
                foreach (var entry in value)
                    dealingItems.Add(entry);
            }
        }
        #endregion

        #region Sync data changes callback
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

        protected virtual void OnDealingStateChange(byte dealingState)
        {
            if (onDealingStateChange != null)
                onDealingStateChange.Invoke((DealingState)dealingState);
        }

        protected virtual void OnDealingGoldChange(int dealingGold)
        {
            if (onDealingGoldChange != null)
                onDealingGoldChange.Invoke(dealingGold);
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

        protected virtual void OnDealingItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (onDealingItemsOperation != null)
                onDealingItemsOperation.Invoke(operation, index);
        }
        #endregion
    }
}
