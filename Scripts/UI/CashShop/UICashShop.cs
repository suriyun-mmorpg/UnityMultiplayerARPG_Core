using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG
{
    public partial class UICashShop : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Cash Amount}")]
        public UILocaleKeySetting formatKeyCash = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CASH);

        [Header("UI Elements")]
        public UICashShopItem uiCashShopItemDialog;
        public UICashShopItem uiCashShopItemPrefab;
        public Transform uiCashShopItemContainer;
        public TextWrapper uiTextCash;

        private UIList cacheCashShopList;
        public UIList CacheCashShopList
        {
            get
            {
                if (cacheCashShopList == null)
                {
                    cacheCashShopList = gameObject.AddComponent<UIList>();
                    cacheCashShopList.uiPrefab = uiCashShopItemPrefab.gameObject;
                    cacheCashShopList.uiContainer = uiCashShopItemContainer;
                }
                return cacheCashShopList;
            }
        }

        private UICashShopSelectionManager cacheCashShopSelectionManager;
        public UICashShopSelectionManager CacheCashShopSelectionManager
        {
            get
            {
                if (cacheCashShopSelectionManager == null)
                    cacheCashShopSelectionManager = gameObject.GetOrAddComponent<UICashShopSelectionManager>();
                cacheCashShopSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheCashShopSelectionManager;
            }
        }

        public void RefreshCashShopInfo()
        {
            // Load cash shop item list
            GameInstance.ClientCashShopHandlers.RequestCashShopInfo(ResponseCashShopInfo);
        }

        protected virtual void OnEnable()
        {
            CacheCashShopSelectionManager.eventOnSelect.RemoveListener(OnSelectCashShopItem);
            CacheCashShopSelectionManager.eventOnSelect.AddListener(OnSelectCashShopItem);
            CacheCashShopSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCashShopItem);
            CacheCashShopSelectionManager.eventOnDeselect.AddListener(OnDeselectCashShopItem);
            if (uiCashShopItemDialog != null)
                uiCashShopItemDialog.onHide.AddListener(OnCashShopItemDialogHide);
            RefreshCashShopInfo();
        }

        protected virtual void OnDisable()
        {
            if (uiCashShopItemDialog != null)
                uiCashShopItemDialog.onHide.RemoveListener(OnCashShopItemDialogHide);
            CacheCashShopSelectionManager.DeselectSelectedUI();
        }

        protected void OnCashShopItemDialogHide()
        {
            CacheCashShopSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCashShopItem(UICashShopItem ui)
        {
            if (uiCashShopItemDialog != null && ui.Data != null)
            {
                uiCashShopItemDialog.selectionManager = CacheCashShopSelectionManager;
                uiCashShopItemDialog.Data = ui.Data;
                uiCashShopItemDialog.Show();
            }
        }

        protected void OnDeselectCashShopItem(UICashShopItem ui)
        {
            if (uiCashShopItemDialog != null)
            {
                uiCashShopItemDialog.onHide.RemoveListener(OnCashShopItemDialogHide);
                uiCashShopItemDialog.Hide();
                uiCashShopItemDialog.onHide.AddListener(OnCashShopItemDialogHide);
            }
        }

        public void Buy(int dataId)
        {
            GameInstance.ClientCashShopHandlers.RequestCashShopBuy(new RequestCashShopBuyMessage()
            {
                dataId = dataId,
            }, ResponseCashShopBuy);
        }

        private async UniTaskVoid ResponseCashShopInfo(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCashShopInfoMessage response)
        {
            await UniTask.Yield();
            if (responseCode.ShowUnhandledResponseMessageDialog(response.error)) return;

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

            int selectedIdx = CacheCashShopSelectionManager.SelectedUI != null ? CacheCashShopSelectionManager.IndexOf(CacheCashShopSelectionManager.SelectedUI) : -1;
            CacheCashShopSelectionManager.DeselectSelectedUI();
            CacheCashShopSelectionManager.Clear();

            CacheCashShopList.Generate(cashShopItems, (index, cashShopItem, ui) =>
            {
                UICashShopItem uiCashShopItem = ui.GetComponent<UICashShopItem>();
                uiCashShopItem.uiCashShop = this;
                uiCashShopItem.Data = cashShopItem;
                uiCashShopItem.Show();
                CacheCashShopSelectionManager.Add(uiCashShopItem);
                if (selectedIdx == index)
                    uiCashShopItem.OnClickSelect();
            });
        }

        private async UniTaskVoid ResponseCashShopBuy(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCashShopBuyMessage response)
        {
            await UniTask.Yield();
            if (responseCode.ShowUnhandledResponseMessageDialog(response.error)) return;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_SUCCESS.ToString()), LanguageManager.GetText(UITextKeys.UI_SUCCESS_CASH_SHOP_BUY.ToString()));
            RefreshCashShopInfo();
        }
    }
}
