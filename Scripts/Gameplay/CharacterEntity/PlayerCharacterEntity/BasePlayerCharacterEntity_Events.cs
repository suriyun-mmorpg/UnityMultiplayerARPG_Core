using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        // Note: You may use `Awake` dev extension to setup an events and `OnDestroy` to desetup an events
        // Npc dialog events
        public System.Action<int> onShowNpcDialog;
        public System.Action onShowNpcRefineItem;
        public System.Action onShowNpcDismantleItem;
        public System.Action onShowNpcRepairItem;
        // Dealing dialog events
        public System.Action<BasePlayerCharacterEntity> onShowDealingRequestDialog;
        public System.Action<BasePlayerCharacterEntity> onShowDealingDialog;
        public System.Action<DealingState> onUpdateDealingState;
        public System.Action<DealingState> onUpdateAnotherDealingState;
        public System.Action<int> onUpdateDealingGold;
        public System.Action<int> onUpdateAnotherDealingGold;
        public System.Action<DealingCharacterItems> onUpdateDealingItems;
        public System.Action<DealingCharacterItems> onUpdateAnotherDealingItems;
        // Storage dialog events
        public System.Action<StorageType, string, uint, short, short> onShowStorage;
        // Sync variables
        public System.Action<int> onDataIdChange;
        public System.Action<int> onFactionIdChange;
        public System.Action<short> onStatPointChange;
        public System.Action<short> onSkillPointChange;
        public System.Action<int> onGoldChange;
        public System.Action<int> onUserGoldChange;
        public System.Action<int> onUserCashChange;
        public System.Action<int> onPartyIdChange;
        public System.Action<int> onGuildIdChange;
        public System.Action<bool> onIsWarpingChange;
        // Sync lists
        public System.Action<LiteNetLibSyncList.Operation, int> onHotkeysOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onQuestsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onCurrenciesOperation;
        public System.Action<CharacterItem[]> onStorageItemsChange;
    }
}
