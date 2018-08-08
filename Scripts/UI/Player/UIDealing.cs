using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterItemSelectionManager))]
    public partial class UIDealing : UISelectionEntry<BasePlayerCharacterEntity>
    {
        [Header("Display Format")]
        [Tooltip("Gold Format => {0} = {Gold}")]
        public string dealingGoldFormat = "Gold: {0}";
        [Tooltip("Gold Format => {0} = {Gold}")]
        public string anotherDealingGoldFormat = "Gold: {0}";

        [Header("Input Dialog Settings")]
        public string dealingGoldInputTitle = "Offer Gold";
        public string dealingGoldInputDescription = "";

        [Header("UI Elements")]
        public UICharacterItem uiDealingItemPrefab;
        public UICharacterItem uiItemDialog;
        [Header("Owning Character Elements")]
        public Text textDealingGold;
        public TextWrapper uiTextDealingGold;
        public Transform uiDealingItemsContainer;
        [Header("Another Character Elements")]
        public UICharacter uiAnotherCharacter;
        public Text textAnotherDealingGold;
        public TextWrapper uiTextAnotherDealingGold;
        public Transform uiAnotherDealingItemsContainer;

        [Header("UI Events")]
        public UnityEvent onStateChangeToDealing;
        public UnityEvent onStateChangeToLock;
        public UnityEvent onStateChangeToConfirm;
        public UnityEvent onAnotherStateChangeToDealing;
        public UnityEvent onAnotherStateChangeToLock;
        public UnityEvent onAnotherStateChangeToConfirm;
        public UnityEvent onBothStateChangeToLock;

        public DealingState dealingState { get; private set; }
        public DealingState anotherDealingState { get; private set; }
        public int dealingGold { get; private set; }
        public int anotherDealingGold { get; private set; }
        private readonly List<UICharacterItem> tempDealingItemUIs = new List<UICharacterItem>();
        private readonly List<UICharacterItem> tempAnotherDealingItemUIs = new List<UICharacterItem>();

        private UIList cacheDealingItemsList;
        public UIList CacheDealingItemsList
        {
            get
            {
                if (cacheDealingItemsList == null)
                {
                    cacheDealingItemsList = gameObject.AddComponent<UIList>();
                    cacheDealingItemsList.uiPrefab = uiDealingItemPrefab.gameObject;
                    cacheDealingItemsList.uiContainer = uiDealingItemsContainer;
                }
                return cacheDealingItemsList;
            }
        }

        private UIList cacheAnotherDealingItemsList;
        public UIList CacheAnotherDealingItemsList
        {
            get
            {
                if (cacheAnotherDealingItemsList == null)
                {
                    cacheAnotherDealingItemsList = gameObject.AddComponent<UIList>();
                    cacheAnotherDealingItemsList.uiPrefab = uiDealingItemPrefab.gameObject;
                    cacheAnotherDealingItemsList.uiContainer = uiAnotherDealingItemsContainer;
                }
                return cacheAnotherDealingItemsList;
            }
        }

        private UICharacterItemSelectionManager itemSelectionManager;
        public UICharacterItemSelectionManager ItemSelectionManager
        {
            get
            {
                if (itemSelectionManager == null)
                    itemSelectionManager = GetComponent<UICharacterItemSelectionManager>();
                itemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return itemSelectionManager;
            }
        }

        public override void Show()
        {
            ItemSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
            ItemSelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
            ItemSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
            ItemSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
            base.Show();
        }

        public override void Hide()
        {
            ItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            if (uiItemDialog != null && ui.Data.characterItem.IsValid())
            {
                uiItemDialog.selectionManager = ItemSelectionManager;
                uiItemDialog.Setup(ui.Data, null, -1, string.Empty);
                uiItemDialog.Show();
            }
            else
                ItemSelectionManager.Deselect(ui);
        }

        protected void OnDeselectCharacterItem(UICharacterItem ui)
        {
            if (uiItemDialog != null)
                uiItemDialog.Hide();
        }

        protected override void UpdateUI()
        {
            Profiler.BeginSample("UIDealing - Update UI");
            // In case that another character is exit or move so far hide the dialog
            if (Data == null)
            {
                Hide();
                return;
            }
            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            var anotherCharacter = Data;

            if (uiAnotherCharacter != null)
                uiAnotherCharacter.Data = anotherCharacter;

            dealingState = DealingState.None;
            anotherDealingState = DealingState.None;
            UpdateDealingState(DealingState.Dealing);
            UpdateAnotherDealingState(DealingState.Dealing);
            UpdateDealingGold(0);
            UpdateAnotherDealingGold(0);
            CacheDealingItemsList.HideAll();
            CacheAnotherDealingItemsList.HideAll();
            ItemSelectionManager.DeselectSelectedUI();
            ItemSelectionManager.Clear();
        }

        public void UpdateDealingState(DealingState state)
        {
            if (dealingState != state)
            {
                dealingState = state;
                switch (dealingState)
                {
                    case DealingState.None:
                        Hide();
                        break;
                    case DealingState.Dealing:
                        if (onStateChangeToDealing != null)
                            onStateChangeToDealing.Invoke();
                        break;
                    case DealingState.Lock:
                        if (onStateChangeToLock != null)
                            onStateChangeToLock.Invoke();
                        break;
                    case DealingState.Confirm:
                        if (onStateChangeToConfirm != null)
                            onStateChangeToConfirm.Invoke();
                        break;
                }
                if (dealingState == DealingState.Lock && anotherDealingState == DealingState.Lock)
                {
                    if (onBothStateChangeToLock != null)
                        onBothStateChangeToLock.Invoke();
                }
            }
        }

        public void UpdateAnotherDealingState(DealingState state)
        {
            if (anotherDealingState != state)
            {
                anotherDealingState = state;
                switch (anotherDealingState)
                {
                    case DealingState.Dealing:
                        if (onAnotherStateChangeToDealing != null)
                            onAnotherStateChangeToDealing.Invoke();
                        break;
                    case DealingState.Lock:
                        if (onAnotherStateChangeToLock != null)
                            onAnotherStateChangeToLock.Invoke();
                        break;
                    case DealingState.Confirm:
                        if (onAnotherStateChangeToConfirm != null)
                            onAnotherStateChangeToConfirm.Invoke();
                        break;
                }
                if (dealingState == DealingState.Lock && anotherDealingState == DealingState.Lock)
                {
                    if (onBothStateChangeToLock != null)
                        onBothStateChangeToLock.Invoke();
                }
            }
        }

        public void UpdateDealingGold(int gold)
        {
            MigrateUIComponents();
            if (uiTextDealingGold != null)
                uiTextDealingGold.text = string.Format(dealingGoldFormat, gold.ToString("N0"));
            dealingGold = gold;
        }

        public void UpdateAnotherDealingGold(int gold)
        {
            MigrateUIComponents();
            if (uiTextAnotherDealingGold != null)
                uiTextAnotherDealingGold.text = string.Format(anotherDealingGoldFormat, gold.ToString("N0"));
            anotherDealingGold = gold;
        }

        public void UpdateDealingItems(DealingCharacterItems dealingItems)
        {
            SetupList(CacheDealingItemsList, dealingItems, tempDealingItemUIs);
        }

        public void UpdateAnotherDealingItems(DealingCharacterItems dealingItems)
        {
            SetupList(CacheAnotherDealingItemsList, dealingItems, tempAnotherDealingItemUIs);
        }

        private void SetupList(UIList list, DealingCharacterItems dealingItems, List<UICharacterItem> uiList)
        {
            ItemSelectionManager.DeselectSelectedUI();
            var filterItems = new List<CharacterItem>();
            foreach (var dealingItem in dealingItems)
            {
                var characterItem = new CharacterItem();
                characterItem.dataId = dealingItem.dataId;
                characterItem.level = dealingItem.level;
                characterItem.amount = dealingItem.amount;
                characterItem.durability = dealingItem.durability;
                filterItems.Add(characterItem);
            }
            uiList.Clear();
            list.Generate(filterItems, (index, characterItem, ui) =>
            {
                var uiCharacterItem = ui.GetComponent<UICharacterItem>();
                uiCharacterItem.Setup(new CharacterItemLevelTuple(characterItem, characterItem.level), null, -1, string.Empty);
                uiCharacterItem.Show();
                uiList.Add(uiCharacterItem);
            });
            ItemSelectionManager.Clear();
            foreach (var tempDealingItemUI in tempDealingItemUIs)
            {
                ItemSelectionManager.Add(tempDealingItemUI);
            }
            foreach (var tempAnotherDealingItemUI in tempAnotherDealingItemUIs)
            {
                ItemSelectionManager.Add(tempAnotherDealingItemUI);
            }
        }

        public void OnClickSetDealingGold()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            UISceneGlobal.Singleton.ShowInputDialog(dealingGoldInputTitle, dealingGoldInputDescription, OnDealingGoldConfirmed, 0, owningCharacter.Gold, owningCharacter.DealingGold);
        }

        private void OnDealingGoldConfirmed(int amount)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestSetDealingGold(amount);
        }

        public void OnClickLock()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestLockDealing();
        }

        public void OnClickConfirm()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestConfirmDealing();
        }

        public void OnClickCancel()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestCancelDealing();
        }

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextDealingGold = MigrateUIHelpers.SetWrapperToText(textDealingGold, uiTextDealingGold);
            uiTextAnotherDealingGold = MigrateUIHelpers.SetWrapperToText(textAnotherDealingGold, uiTextAnotherDealingGold);
        }
    }
}
