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
        protected SyncFieldInt partyId = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt guildId = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldByte partyMemberFlags = new SyncFieldByte();
        [SerializeField]
        protected SyncFieldByte guildMemberFlags = new SyncFieldByte();
        // List
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
                if (CoCharacter != null)
                    CoCharacter.RequestUpdateAnotherDealingState(value);
            }
        }

        public int DealingGold
        {
            get { return dealingGold; }
            set
            {
                dealingGold = value;
                RequestUpdateDealingGold(value);
                if (CoCharacter != null)
                    CoCharacter.RequestUpdateAnotherDealingGold(value);
            }
        }

        public DealingCharacterItems DealingItems
        {
            get { return dealingItems; }
            set
            {
                dealingItems = value;
                RequestUpdateDealingItems(value);
                if (CoCharacter != null)
                    CoCharacter.RequestUpdateAnotherDealingItems(value);
            }
        }
        #endregion

        #region Sync data actions
        public System.Action<short> onStatPointChange;
        public System.Action<short> onSkillPointChange;
        public System.Action<int> onGoldChange;
        public System.Action<int> onPartyIdChange;
        public System.Action<int> onGuildIdChange;
        // List
        public System.Action<LiteNetLibSyncList.Operation, int> onHotkeysOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onQuestsOperation;
        #endregion

        #region Fields/Interface/Getter/Setter implementation
        public short StatPoint { get { return statPoint.Value; } set { statPoint.Value = value; } }
        public short SkillPoint { get { return skillPoint.Value; } set { skillPoint.Value = value; } }
        public int Gold { get { return gold.Value; } set { gold.Value = value; } }
        public int PartyId { get { return partyId.Value; } set { partyId.Value = value; } }
        public int GuildId { get { return guildId.Value; } set { guildId.Value = value; } }
        public byte GuildRole { get; set; }
        public int SharedGuildExp { get; set; }
        public PartyMemberFlags PartyMemberFlags { get { return (PartyMemberFlags)partyMemberFlags.Value; } set { partyMemberFlags.Value = (byte)value; } }
        public GuildMemberFlags GuildMemberFlags { get { return (GuildMemberFlags)guildMemberFlags.Value; } set { guildMemberFlags.Value = (byte)value; } }
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
        #endregion
        
        public void ClearParty()
        {
            PartyMemberFlags = PartyMemberFlags.None;
            PartyId = 0;
        }

        public void ClearGuild()
        {
            GuildMemberFlags = GuildMemberFlags.None;
            GuildId = 0;
            GuildRole = 0;
            SharedGuildExp = 0;
        }

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
        #endregion
    }
}
