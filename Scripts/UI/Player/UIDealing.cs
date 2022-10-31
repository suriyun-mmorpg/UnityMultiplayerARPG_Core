using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;
using UnityEngine.Events;

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
                    cacheItemSelectionManager = gameObject.GetOrAddComponent<UICharacterItemSelectionManager>();
                cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheItemSelectionManager;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            CacheItemSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
            CacheItemSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
            if (uiItemDialog != null)
                uiItemDialog.onHide.AddListener(OnItemDialogHide);
            UpdateData();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.Dealing.onUpdateDealingState += UpdateDealingState;
            GameInstance.PlayingCharacterEntity.Dealing.onUpdateDealingGold += UpdateDealingGold;
            GameInstance.PlayingCharacterEntity.Dealing.onUpdateDealingItems += UpdateDealingItems;
            GameInstance.PlayingCharacterEntity.Dealing.onUpdateAnotherDealingState += UpdateAnotherDealingState;
            GameInstance.PlayingCharacterEntity.Dealing.onUpdateAnotherDealingGold += UpdateAnotherDealingGold;
            GameInstance.PlayingCharacterEntity.Dealing.onUpdateAnotherDealingItems += UpdateAnotherDealingItems;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (uiItemDialog != null)
                uiItemDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.Dealing.onUpdateDealingState -= UpdateDealingState;
            GameInstance.PlayingCharacterEntity.Dealing.onUpdateDealingGold -= UpdateDealingGold;
            GameInstance.PlayingCharacterEntity.Dealing.onUpdateDealingItems -= UpdateDealingItems;
            GameInstance.PlayingCharacterEntity.Dealing.onUpdateAnotherDealingState -= UpdateAnotherDealingState;
            GameInstance.PlayingCharacterEntity.Dealing.onUpdateAnotherDealingGold -= UpdateAnotherDealingGold;
            GameInstance.PlayingCharacterEntity.Dealing.onUpdateAnotherDealingItems -= UpdateAnotherDealingItems;
            GameInstance.PlayingCharacterEntity.Dealing.CallServerCancelDealing();
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
                uiItemDialog.Setup(ui.Data, GameInstance.PlayingCharacterEntity, -1);
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
            // In case that another character is exit or move so far hide the dialog
            if (Data == null)
            {
                Hide();
                return;
            }
        }

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity anotherCharacter = Data;

            if (uiAnotherCharacter != null)
            {
                uiAnotherCharacter.NotForOwningCharacter = true;
                uiAnotherCharacter.Data = anotherCharacter;
            }

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
                uiTextDealingGold.text = ZString.Format(
                    LanguageManager.GetText(formatKeyDealingGold),
                    gold.ToString("N0"));
            }
            dealingGold = gold;
        }

        public void UpdateAnotherDealingGold(int gold)
        {
            if (uiTextAnotherDealingGold != null)
            {
                uiTextAnotherDealingGold.text = ZString.Format(
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
                if (dealingItem.NotEmptySlot())
                {
                    tempUiCharacterItem.Setup(new UICharacterItemData(dealingItem, InventoryType.NonEquipItems), GameInstance.PlayingCharacterEntity, -1);
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
                GameInstance.PlayingCharacterEntity.Gold, // Max amount is number of gold
                GameInstance.PlayingCharacterEntity.Dealing.DealingGold);
        }

        private void OnDealingGoldConfirmed(int amount)
        {
            GameInstance.PlayingCharacterEntity.Dealing.CallServerSetDealingGold(amount);
        }

        public void OnClickLock()
        {
            GameInstance.PlayingCharacterEntity.Dealing.CallServerLockDealing();
        }

        public void OnClickConfirm()
        {
            GameInstance.PlayingCharacterEntity.Dealing.CallServerConfirmDealing();
        }

        public void OnClickCancel()
        {
            Hide();
        }
    }
}
