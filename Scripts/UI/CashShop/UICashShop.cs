using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICashShopSelectionManager))]
    public partial class UICashShop : UIBase
    {
        [Header("Generic Info Format")]
        [Tooltip("Cash Format => {0} = {Cash amount}")]
        public string cashFormat = "{0}";
        public UICashShopItem uiCashShopItemDialog;
        public UICashShopItem uiCashShopItemPrefab;
        public Transform uiCashShopItemContainer;
        public TextWrapper uiTextCash;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiCashShopItemPrefab.gameObject;
                    cacheList.uiContainer = uiCashShopItemContainer;
                }
                return cacheList;
            }
        }

        private UICashShopSelectionManager selectionManager;
        public UICashShopSelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UICashShopSelectionManager>();
                selectionManager.selectionMode = UISelectionMode.SelectSingle;
                return selectionManager;
            }
        }

        private BaseGameNetworkManager cacheGameNetworkManager;
        public BaseGameNetworkManager CacheGameNetworkManager
        {
            get
            {
                if (cacheGameNetworkManager == null)
                    cacheGameNetworkManager = FindObjectOfType<BaseGameNetworkManager>();
                return cacheGameNetworkManager;
            }
        }

        public void RefreshCashShopInfo()
        {
            // Load cash shop item list
            CacheGameNetworkManager.RequestCashShopInfo(ResponseCashShopInfo);
        }

        public override void Show()
        {
            base.Show();
            SelectionManager.eventOnSelect.RemoveListener(OnSelectCashShopItem);
            SelectionManager.eventOnSelect.AddListener(OnSelectCashShopItem);
            SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCashShopItem);
            SelectionManager.eventOnDeselect.AddListener(OnDeselectCashShopItem);
            RefreshCashShopInfo();
        }

        public override void Hide()
        {
            SelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCashShopItem(UICashShopItem ui)
        {
            if (uiCashShopItemDialog != null && ui.Data != null)
            {
                uiCashShopItemDialog.selectionManager = SelectionManager;
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
            CacheGameNetworkManager.RequestCashShopBuy(dataId, ResponseCashShopBuy);
        }

        private void ResponseCashShopInfo(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponseCashShopInfoMessage)message;
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
                    var cashShopItems = new List<CashShopItem>();
                    foreach (var cashShopItemId in castedMessage.cashShopItemIds)
                    {
                        CashShopItem cashShopItem;
                        if (GameInstance.CashShopItems.TryGetValue(cashShopItemId, out cashShopItem))
                            cashShopItems.Add(cashShopItem);
                    }

                    var selectedIdx = SelectionManager.SelectedUI != null ? SelectionManager.IndexOf(SelectionManager.SelectedUI) : -1;
                    SelectionManager.DeselectSelectedUI();
                    SelectionManager.Clear();

                    CacheList.Generate(cashShopItems, (index, cashShopItem, ui) =>
                    {
                        var uiCashShopItem = ui.GetComponent<UICashShopItem>();
                        uiCashShopItem.uiCashShop = this;
                        uiCashShopItem.Data = cashShopItem;
                        uiCashShopItem.Show();
                        SelectionManager.Add(uiCashShopItem);
                        if (selectedIdx == index)
                            uiCashShopItem.OnClickSelect();
                    });
                    break;
            }
        }

        private void ResponseCashShopBuy(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponseCashShopBuyMessage)message;
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    var errorMessage = string.Empty;
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
