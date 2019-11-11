using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    public partial class UIDealing : UISelectionEntry<BasePlayerCharacterEntity>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Gold Amount}")]
        public UILocaleKeySetting formatKeyDealingGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD);
        [Tooltip("Format => {0} = {Gold Amount}")]
        public UILocaleKeySetting formatKeyAnotherDealingGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD);

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
            if (uiItemDialog != null)
                uiItemDialog.onHide.AddListener(OnItemDialogHide);
            base.Show();
        }

        public override void Hide()
        {
            if (uiItemDialog != null)
                uiItemDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnItemDialogHide()
        {
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            if (ui.Data.characterItem.IsEmptySlot())
            {
                CacheItemSelectionManager.DeselectSelectedUI();
                return;
            }
            if (uiItemDialog != null)
            {
                uiItemDialog.selectionManager = CacheItemSelectionManager;
                uiItemDialog.Setup(ui.Data, BasePlayerCharacterController.OwningCharacter, -1);
                uiItemDialog.Show();
            }
        }

        protected void OnDeselectCharacterItem(UICharacterItem ui)
        {
            if (uiItemDialog != null)
            {
                uiItemDialog.onHide.RemoveListener(OnItemDialogHide);
                uiItemDialog.Hide();
                uiItemDialog.onHide.AddListener(OnItemDialogHide);
            }
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
            {
                uiTextDealingGold.text = string.Format(
                    LanguageManager.GetText(formatKeyDealingGold),
                    gold.ToString("N0"));
            }
            dealingGold = gold;
        }

        public void UpdateAnotherDealingGold(int gold)
        {
            if (uiTextAnotherDealingGold != null)
            {
                uiTextAnotherDealingGold.text = string.Format(
                    LanguageManager.GetText(formatKeyAnotherDealingGold),
                    gold.ToString("N0"));
            }
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
            uiList.Clear();

            UICharacterItem tempUiCharacterItem;
            list.Generate(dealingItems, (index, dealingItem, ui) =>
            {
                tempUiCharacterItem = ui.GetComponent<UICharacterItem>();
                if (dealingItem.characterItem.NotEmptySlot())
                {
                    tempUiCharacterItem.Setup(new UICharacterItemData(dealingItem.characterItem, dealingItem.characterItem.level, InventoryType.NonEquipItems), BasePlayerCharacterController.OwningCharacter, -1);
                    tempUiCharacterItem.Show();
                    uiList.Add(tempUiCharacterItem);
                }
                else
                {
                    tempUiCharacterItem.Hide();
                }
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
            UISceneGlobal.Singleton.ShowInputDialog(
                LanguageManager.GetText(UITextKeys.UI_OFFER_GOLD.ToString()), 
                LanguageManager.GetText(UITextKeys.UI_OFFER_GOLD_DESCRIPTION.ToString()), 
                OnDealingGoldConfirmed, 
                0, // Min amount is 0
                BasePlayerCharacterController.OwningCharacter.Gold, // Max amount is number of gold
                BasePlayerCharacterController.OwningCharacter.DealingGold);
        }

        private void OnDealingGoldConfirmed(int amount)
        {
            BasePlayerCharacterController.OwningCharacter.RequestSetDealingGold(amount);
        }

        public void OnClickLock()
        {
            BasePlayerCharacterController.OwningCharacter.RequestLockDealing();
        }

        public void OnClickConfirm()
        {
            BasePlayerCharacterController.OwningCharacter.RequestConfirmDealing();
        }

        public void OnClickCancel()
        {
            BasePlayerCharacterController.OwningCharacter.RequestCancelDealing();
        }
    }
}
