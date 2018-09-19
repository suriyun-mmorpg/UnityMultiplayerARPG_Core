using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICashPackageSelectionManager))]
    public class UICashPackages : UIBase
    {
        [Header("Generic Info Format")]
        [Tooltip("Cash Format => {0} = {Cash amount}")]
        public string cashFormat = "{0}";
        public UICashPackage uiCashPackageDialog;
        public UICashPackage uiCashPackagePrefab;
        public Transform uiCashPackageContainer;
        public TextWrapper uiTextCash;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiCashPackagePrefab.gameObject;
                    cacheList.uiContainer = uiCashPackageContainer;
                }
                return cacheList;
            }
        }

        private UICashPackageSelectionManager selectionManager;
        public UICashPackageSelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UICashPackageSelectionManager>();
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

        public void RefreshCashPackageInfo()
        {
            // Load cash shop item list
            CacheGameNetworkManager.RequestCashPackageInfo(ResponseCashPackageInfo);
        }

        public override void Show()
        {
            base.Show();
            SelectionManager.eventOnSelect.RemoveListener(OnSelectCashPackage);
            SelectionManager.eventOnSelect.AddListener(OnSelectCashPackage);
            SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCashPackage);
            SelectionManager.eventOnDeselect.AddListener(OnDeselectCashPackage);
            RefreshCashPackageInfo();
        }

        public override void Hide()
        {
            SelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCashPackage(UICashPackage ui)
        {
            if (uiCashPackageDialog != null && ui.Data != null)
            {
                uiCashPackageDialog.selectionManager = SelectionManager;
                uiCashPackageDialog.Data = ui.Data;
                uiCashPackageDialog.Show();
            }
        }

        protected void OnDeselectCashPackage(UICashPackage ui)
        {
            if (uiCashPackageDialog != null)
                uiCashPackageDialog.Hide();
        }

        public void Buy(string productId)
        {
            GameInstance.PurchaseCallback = ResponsePurchase;
            GameInstance.Singleton.Purchase(productId);
        }

        private void ResponsePurchase(bool success, string errorMessage)
        {
            if (!success)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Error", errorMessage);
                return;
            }
            RefreshCashPackageInfo();
        }

        private void ResponseCashPackageInfo(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponseCashPackageInfoMessage)message;
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    UISceneGlobal.Singleton.ShowMessageDialog("Error", "Cannot retrieve cash package info");
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog("Error", "Connection timeout");
                    break;
                default:
                    if (uiTextCash != null)
                        uiTextCash.text = string.Format(cashFormat, castedMessage.cash.ToString("N0"));
                    var cashPackages = new List<CashPackage>();
                    foreach (var cashPackageId in castedMessage.cashPackageIds)
                    {
                        CashPackage cashPackage;
                        if (GameInstance.CashPackages.TryGetValue(cashPackageId, out cashPackage))
                        {
                            cashPackages.Add(cashPackage);
                        }
                    }

                    var selectedIdx = SelectionManager.SelectedUI != null ? SelectionManager.IndexOf(SelectionManager.SelectedUI) : -1;
                    SelectionManager.DeselectSelectedUI();
                    SelectionManager.Clear();

                    CacheList.Generate(cashPackages, (index, cashShopItem, ui) =>
                    {
                        var uiCashPackage = ui.GetComponent<UICashPackage>();
                        uiCashPackage.uiCashPackages = this;
                        uiCashPackage.Data = cashShopItem;
                        uiCashPackage.Show();
                        SelectionManager.Add(uiCashPackage);
                        if (selectedIdx == index)
                            uiCashPackage.OnClickSelect();
                    });
                    break;
            }
        }
    }
}
