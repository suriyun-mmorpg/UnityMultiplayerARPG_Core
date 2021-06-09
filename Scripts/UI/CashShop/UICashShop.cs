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
            CacheSelectionManager.eventOnSelect.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelect.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselect.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselect.AddListener(OnDeselect);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnDialogHide);
            RefreshCashShopInfo();
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelect(UICashShopItem ui)
        {
            if (uiDialog != null && ui.Data != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.uiCashShop = this;
                uiDialog.Data = ui.Data;
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICashShopItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
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

        protected virtual void ResponseCashShopInfo(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCashShopInfoMessage response)
        {
            ClientCashShopActions.ResponseCashShopInfo(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;

            if (uiTextCash != null)
            {
                uiTextCash.text = string.Format(
                    LanguageManager.GetText(formatKeyCash),
                    response.cash.ToString("N0"));
            }

            List<CashShopItem> list = new List<CashShopItem>();
            foreach (int cashShopItemId in response.cashShopItemIds)
            {
                CashShopItem cashShopItem;
                if (GameInstance.CashShopItems.TryGetValue(cashShopItemId, out cashShopItem))
                    list.Add(cashShopItem);
            }

            int selectedIdx = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.IndexOf(CacheSelectionManager.SelectedUI) : -1;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            int showingCount = 0;
            UICashShopItem tempUI;
            CacheList.Generate(list, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UICashShopItem>();
                if (data == null ||
                    string.IsNullOrEmpty(data.category) ||
                    filterCategories == null || filterCategories.Count == 0 ||
                    filterCategories.Contains(data.category))
                {
                    tempUI.uiCashShop = this;
                    tempUI.Data = data;
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
                listEmptyObject.SetActive(showingCount == 0);
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
