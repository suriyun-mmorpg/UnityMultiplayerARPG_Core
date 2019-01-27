using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class UICashPackages : UIBase
    {
        [Header("Generic Info Format")]
        [Tooltip("Cash Format => {0} = {Cash amount}")]
        public string cashFormat = "{0}";
        public UICashPackage uiCashPackageDialog;
        public UICashPackage uiCashPackagePrefab;
        public Transform uiCashPackageContainer;
        public TextWrapper uiTextCash;

        private UIList cacheCashPackageList;
        public UIList CacheCashPackageList
        {
            get
            {
                if (cacheCashPackageList == null)
                {
                    cacheCashPackageList = gameObject.AddComponent<UIList>();
                    cacheCashPackageList.uiPrefab = uiCashPackagePrefab.gameObject;
                    cacheCashPackageList.uiContainer = uiCashPackageContainer;
                }
                return cacheCashPackageList;
            }
        }

        private UICashPackageSelectionManager cacheCashPackageSelectionManager;
        public UICashPackageSelectionManager CacheCashPackageSelectionManager
        {
            get
            {
                if (cacheCashPackageSelectionManager == null)
                    cacheCashPackageSelectionManager = GetComponent<UICashPackageSelectionManager>();
                if (cacheCashPackageSelectionManager == null)
                    cacheCashPackageSelectionManager = gameObject.AddComponent<UICashPackageSelectionManager>();
                cacheCashPackageSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheCashPackageSelectionManager;
            }
        }

        public void RefreshCashPackageInfo()
        {
            // Load cash shop item list
            BaseGameNetworkManager.Singleton.RequestCashPackageInfo(ResponseCashPackageInfo);
        }

        public override void Show()
        {
            base.Show();
            CacheCashPackageSelectionManager.eventOnSelect.RemoveListener(OnSelectCashPackage);
            CacheCashPackageSelectionManager.eventOnSelect.AddListener(OnSelectCashPackage);
            CacheCashPackageSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCashPackage);
            CacheCashPackageSelectionManager.eventOnDeselect.AddListener(OnDeselectCashPackage);
            RefreshCashPackageInfo();
        }

        public override void Hide()
        {
            CacheCashPackageSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCashPackage(UICashPackage ui)
        {
            if (uiCashPackageDialog != null && ui.Data != null)
            {
                uiCashPackageDialog.selectionManager = CacheCashPackageSelectionManager;
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
            ResponseCashPackageInfoMessage castedMessage = (ResponseCashPackageInfoMessage)message;
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
                    List<CashPackage> cashPackages = new List<CashPackage>();
                    foreach (int cashPackageId in castedMessage.cashPackageIds)
                    {
                        CashPackage cashPackage;
                        if (GameInstance.CashPackages.TryGetValue(cashPackageId, out cashPackage))
                        {
                            cashPackages.Add(cashPackage);
                        }
                    }

                    int selectedIdx = CacheCashPackageSelectionManager.SelectedUI != null ? CacheCashPackageSelectionManager.IndexOf(CacheCashPackageSelectionManager.SelectedUI) : -1;
                    CacheCashPackageSelectionManager.DeselectSelectedUI();
                    CacheCashPackageSelectionManager.Clear();

                    CacheCashPackageList.Generate(cashPackages, (index, cashShopItem, ui) =>
                    {
                        UICashPackage uiCashPackage = ui.GetComponent<UICashPackage>();
                        uiCashPackage.uiCashPackages = this;
                        uiCashPackage.Data = cashShopItem;
                        uiCashPackage.Show();
                        CacheCashPackageSelectionManager.Add(uiCashPackage);
                        if (selectedIdx == index)
                            uiCashPackage.OnClickSelect();
                    });
                    break;
            }
        }
    }
}
