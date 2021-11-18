using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        // Note: You may use `Awake` dev extension to setup an events and `OnDestroy` to desetup an events
        // Quest dialog events
        /// <summary>
        /// Action: int questDataId
        /// </summary>
        public event System.Action<int> onShowQuestRewardItemSelection;
        // Npc dialog events
        /// <summary>
        /// Action: int npcDialogDataId
        /// </summary>
        public event System.Action<int> onShowNpcDialog;
        public event System.Action onShowNpcRefineItem;
        public event System.Action onShowNpcDismantleItem;
        public event System.Action onShowNpcRepairItem;
        // Dealing dialog events
        /// <summary>
        /// Action: BasePlayerCharacterEntity anotherCharacter
        /// </summary>
        public event System.Action<BasePlayerCharacterEntity> onShowDealingRequestDialog;
        /// <summary>
        /// Action: BasePlayerCharacterEntity anotherCharacter
        /// </summary>
        public event System.Action<BasePlayerCharacterEntity> onShowDealingDialog;
        public event System.Action<DealingState> onUpdateDealingState;
        public event System.Action<DealingState> onUpdateAnotherDealingState;
        public event System.Action<int> onUpdateDealingGold;
        public event System.Action<int> onUpdateAnotherDealingGold;
        public event System.Action<DealingCharacterItems> onUpdateDealingItems;
        public event System.Action<DealingCharacterItems> onUpdateAnotherDealingItems;
        // Sync variables
        public event System.Action<int> onDataIdChange;
        public event System.Action<int> onFactionIdChange;
        public event System.Action<float> onStatPointChange;
        public event System.Action<float> onSkillPointChange;
        public event System.Action<int> onGoldChange;
        public event System.Action<int> onUserGoldChange;
        public event System.Action<int> onUserCashChange;
        public event System.Action<int> onPartyIdChange;
        public event System.Action<int> onGuildIdChange;
        public event System.Action<bool> onIsWarpingChange;
        // Sync lists
        public event System.Action<LiteNetLibSyncList.Operation, int> onHotkeysOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onQuestsOperation;
        public event System.Action<LiteNetLibSyncList.Operation, int> onCurrenciesOperation;
        public event System.Action<CharacterItem[]> onStorageItemsChange;
    }
}
