using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICashShop : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Cash Amount}")]
        public UILocaleKeySetting formatKeyCash = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CASH);

        [Header("Filter")]
        public List<string> filterCategories;

        [Header("UI Elements")]
        public GameObject listEmptyObject;
        [FormerlySerializedAs("uiCashShopItemDialog")]
        public UICashShopItem uiDialog;
        [FormerlySerializedAs("uiCashShopItemPrefab")]
        public UICashShopItem uiPrefab;
        [FormerlySerializedAs("uiCashShopItemContainer")]
        public Transform uiContainer;
        public TextWrapper uiTextCash;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiPrefab.gameObject;
                    cacheList.uiContainer = uiContainer;
                }
                return cacheList;
            }
        }

        private UICashShopSelectionManager cacheSelectionManager;
        public UICashShopSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UICashShopSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
            }
        }

        public void RefreshCashShopInfo()
        {
            // Load cash shop item list
            GameInstance.ClientCashShopHandlers.RequestCashShopInfo(ResponseCashShopInfo);
        }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelect.RemoveListener(OnSelectCashShopItem);
            CacheSelectionManager.eventOnSelect.AddListener(OnSelectCashShopItem);
            CacheSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCashShopItem);
            CacheSelectionManager.eventOnDeselect.AddListener(OnDeselectCashShopItem);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnCashShopItemDialogHide);
            RefreshCashShopInfo();
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnCashShopItemDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected void OnCashShopItemDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCashShopItem(UICashShopItem ui)
        {
            if (uiDialog != null && ui.Data != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.uiCashShop = this;
                uiDialog.Data = ui.Data;
                uiDialog.Show();
            }
        }

        protected void OnDeselectCashShopItem(UICashShopItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnCashShopItemDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnCashShopItemDialogHide);
            }
        }

        public void Buy(int dataId, CashShopItemCurrencyType currencyType, int amount)
        {
            GameInstance.ClientCashShopHandlers.RequestCashShopBuy(new RequestCashShopBuyMessage()
            {
                dataId = dataId,
                currencyType = currencyType,
                amount = amount,
            }, ResponseCashShopBuy);
        }

        private void ResponseCashShopInfo(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCashShopInfoMessage response)
        {
            ClientCashShopActions.ResponseCashShopInfo(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;

            if (uiTextCash != null)
            {
                uiTextCash.text = string.Format(
                    LanguageManager.GetText(formatKeyCash),
                    response.cash.ToString("N0"));
            }

            List<CashShopItem> cashShopItems = new List<CashShopItem>();
            foreach (int cashShopItemId in response.cashShopItemIds)
            {
                CashShopItem cashShopItem;
                if (GameInstance.CashShopItems.TryGetValue(cashShopItemId, out cashShopItem))
                    cashShopItems.Add(cashShopItem);
            }

            int selectedIdx = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.IndexOf(CacheSelectionManager.SelectedUI) : -1;
            CacheSelectionManager.Clear();

            int showingCount = 0;
            UICashShopItem tempUI;
            CacheList.Generate(cashShopItems, (index, cashShopItem, ui) =>
            {
                tempUI = ui.GetComponent<UICashShopItem>();
                if (cashShopItem == null ||
                    string.IsNullOrEmpty(cashShopItem.category) ||
                    filterCategories == null || filterCategories.Count == 0 ||
                    filterCategories.Contains(cashShopItem.category))
                {
                    tempUI.uiCashShop = this;
                    tempUI.Data = cashShopItem;
                    tempUI.Show();
                    CacheSelectionManager.Add(tempUI);
                    if (selectedIdx == index)
                        tempUI.OnClickSelect();
                    showingCount++;
                }
                else
                {
                    // Hide because item's category not matches in the filter list
                    tempUI.Hide();
                }
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(cashShopItems.Count == 0);
        }

        private void ResponseCashShopBuy(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCashShopBuyMessage response)
        {
            ClientCashShopActions.ResponseCashShopBuy(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_SUCCESS.ToString()), LanguageManager.GetText(UITextKeys.UI_CASH_SHOP_ITEM_BOUGHT.ToString()));
            RefreshCashShopInfo();
        }
    }
}
