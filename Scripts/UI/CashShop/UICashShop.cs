using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class UICashShop : UIBase
    {
        [Header("Generic Info Format")]
        [Tooltip("Cash Format => {0} = {Cash amount}")]
        public string cashFormat = "{0}";
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
            RefreshCashShopInfo();
        }

        public override void Hide()
        {
            CacheCashShopSelectionManager.DeselectSelectedUI();
            base.Hide();
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
                uiCashShopItemDialog.Hide();
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
                    UISceneGlobal.Singleton.ShowMessageDialog("Error", "Cannot retrieve cash shop info");
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog("Error", "Connection timeout");
                    break;
                default:
                    if (uiTextCash != null)
                        uiTextCash.text = string.Format(cashFormat, castedMessage.cash.ToString("N0"));
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
                            errorMessage = "User not found";
                            break;
                        case ResponseCashShopBuyMessage.Error.ItemNotFound:
                            errorMessage = "Item not found";
                            break;
                        case ResponseCashShopBuyMessage.Error.NotEnoughCash:
                            errorMessage = "Not enough cash";
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog("Error", errorMessage);
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog("Error", "Connection timeout");
                    break;
                default:
                    UISceneGlobal.Singleton.ShowMessageDialog("Success", "Success, let's check your inventory");
                    RefreshCashShopInfo();
                    break;
            }
        }
    }
}
