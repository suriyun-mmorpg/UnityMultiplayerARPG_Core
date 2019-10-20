using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

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
                    cacheCashShopSelectionManager = GetComponent<UICashShopSelectionManager>();
                if (cacheCashShopSelectionManager == null)
                    cacheCashShopSelectionManager = gameObject.AddComponent<UICashShopSelectionManager>();
                cacheCashShopSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheCashShopSelectionManager;
            }
        }

        public void RefreshCashShopInfo()
        {
            // Load cash shop item list
            BaseGameNetworkManager.Singleton.RequestCashShopInfo(ResponseCashShopInfo);
        }

        public override void Show()
        {
            base.Show();
            CacheCashShopSelectionManager.eventOnSelect.RemoveListener(OnSelectCashShopItem);
            CacheCashShopSelectionManager.eventOnSelect.AddListener(OnSelectCashShopItem);
            CacheCashShopSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCashShopItem);
            CacheCashShopSelectionManager.eventOnDeselect.AddListener(OnDeselectCashShopItem);
            if (uiCashShopItemDialog != null)
                uiCashShopItemDialog.onHide.AddListener(OnCashShopItemDialogHide);
            RefreshCashShopInfo();
        }

        public override void Hide()
        {
            if (uiCashShopItemDialog != null)
                uiCashShopItemDialog.onHide.RemoveListener(OnCashShopItemDialogHide);
            CacheCashShopSelectionManager.DeselectSelectedUI();
            base.Hide();
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
            BaseGameNetworkManager.Singleton.RequestCashShopBuy(dataId, ResponseCashShopBuy);
        }

        private void ResponseCashShopInfo(AckResponseCode responseCode, BaseAckMessage message)
        {
            ResponseCashShopInfoMessage castedMessage = (ResponseCashShopInfoMessage)message;
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CANNOT_GET_CASH_SHOP_INFO.ToString()));
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                    break;
                default:
                    if (uiTextCash != null)
                    {
                        uiTextCash.text = string.Format(
                            LanguageManager.GetText(formatKeyCash),
                            castedMessage.cash.ToString("N0"));
                    }

                    List<CashShopItem> cashShopItems = new List<CashShopItem>();
                    foreach (int cashShopItemId in castedMessage.cashShopItemIds)
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
                    break;
            }
        }

        private void ResponseCashShopBuy(AckResponseCode responseCode, BaseAckMessage message)
        {
            ResponseCashShopBuyMessage castedMessage = (ResponseCashShopBuyMessage)message;
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (castedMessage.error)
                    {
                        case ResponseCashShopBuyMessage.Error.UserNotFound:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_USER_NOT_FOUND.ToString());
                            break;
                        case ResponseCashShopBuyMessage.Error.ItemNotFound:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_ITEM_NOT_FOUND.ToString());
                            break;
                        case ResponseCashShopBuyMessage.Error.NotEnoughCash:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_NOT_ENOUGH_CASH.ToString());
                            break;
                        case ResponseCashShopBuyMessage.Error.CannotCarryAllRewards:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_CANNOT_CARRY_ALL_REWARDS.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), errorMessage);
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                    break;
                default:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_SUCCESS.ToString()), LanguageManager.GetText(UITextKeys.UI_SUCCESS_CASH_SHOP_BUY.ToString()));
                    RefreshCashShopInfo();
                    break;
            }
        }
    }
}
