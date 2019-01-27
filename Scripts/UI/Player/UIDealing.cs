using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
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
        public TextWrapper uiTextDealingGold;
        public Transform uiDealingItemsContainer;
        [Header("Another Character Elements")]
        public UICharacter uiAnotherCharacter;
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

        private UICharacterItemSelectionManager cacheItemSelectionManager;
        public UICharacterItemSelectionManager CacheItemSelectionManager
        {
            get
            {
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = GetComponent<UICharacterItemSelectionManager>();
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = gameObject.AddComponent<UICharacterItemSelectionManager>();
                cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheItemSelectionManager;
            }
        }

        public override void Show()
        {
            CacheItemSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
            CacheItemSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
            base.Show();
        }

        public override void Hide()
        {
            CacheItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            if (uiItemDialog != null && ui.Data.characterItem.IsValid())
            {
                uiItemDialog.selectionManager = CacheItemSelectionManager;
                uiItemDialog.Setup(ui.Data, null, -1);
                uiItemDialog.Show();
            }
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
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            BasePlayerCharacterEntity anotherCharacter = Data;

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
            CacheItemSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.Clear();
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
                    case DealingState.LockDealing:
                        if (onStateChangeToLock != null)
                            onStateChangeToLock.Invoke();
                        break;
                    case DealingState.ConfirmDealing:
                        if (onStateChangeToConfirm != null)
                            onStateChangeToConfirm.Invoke();
                        break;
                }
                if (dealingState == DealingState.LockDealing && anotherDealingState == DealingState.LockDealing)
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
                    case DealingState.LockDealing:
                        if (onAnotherStateChangeToLock != null)
                            onAnotherStateChangeToLock.Invoke();
                        break;
                    case DealingState.ConfirmDealing:
                        if (onAnotherStateChangeToConfirm != null)
                            onAnotherStateChangeToConfirm.Invoke();
                        break;
                }
                if (dealingState == DealingState.LockDealing && anotherDealingState == DealingState.LockDealing)
                {
                    if (onBothStateChangeToLock != null)
                        onBothStateChangeToLock.Invoke();
                }
            }
        }

        public void UpdateDealingGold(int gold)
        {
            if (uiTextDealingGold != null)
                uiTextDealingGold.text = string.Format(dealingGoldFormat, gold.ToString("N0"));
            dealingGold = gold;
        }

        public void UpdateAnotherDealingGold(int gold)
        {
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
            CacheItemSelectionManager.DeselectSelectedUI();
            List<CharacterItem> filterItems = new List<CharacterItem>();
            foreach (DealingCharacterItem dealingItem in dealingItems)
            {
                CharacterItem characterItem = new CharacterItem();
                characterItem.dataId = dealingItem.dataId;
                characterItem.level = dealingItem.level;
                characterItem.amount = dealingItem.amount;
                characterItem.durability = dealingItem.durability;
                filterItems.Add(characterItem);
            }
            uiList.Clear();
            list.Generate(filterItems, (index, characterItem, ui) =>
            {
                UICharacterItem uiCharacterItem = ui.GetComponent<UICharacterItem>();
                uiCharacterItem.Setup(new CharacterItemTuple(characterItem, characterItem.level, InventoryType.NonEquipItems), null, -1);
                uiCharacterItem.Show();
                uiList.Add(uiCharacterItem);
            });
            CacheItemSelectionManager.Clear();
            foreach (UICharacterItem tempDealingItemUI in tempDealingItemUIs)
            {
                CacheItemSelectionManager.Add(tempDealingItemUI);
            }
            foreach (UICharacterItem tempAnotherDealingItemUI in tempAnotherDealingItemUIs)
            {
                CacheItemSelectionManager.Add(tempAnotherDealingItemUI);
            }
        }

        public void OnClickSetDealingGold()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            UISceneGlobal.Singleton.ShowInputDialog(dealingGoldInputTitle, dealingGoldInputDescription, OnDealingGoldConfirmed, 0, owningCharacter.Gold, owningCharacter.DealingGold);
        }

        private void OnDealingGoldConfirmed(int amount)
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestSetDealingGold(amount);
        }

        public void OnClickLock()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestLockDealing();
        }

        public void OnClickConfirm()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestConfirmDealing();
        }

        public void OnClickCancel()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestCancelDealing();
        }
    }
}
