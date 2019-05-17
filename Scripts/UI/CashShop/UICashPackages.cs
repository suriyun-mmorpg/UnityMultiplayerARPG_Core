using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class UICashPackages : UIBase
    {
        [Header("Generic Info Format")]
        [Tooltip("Owning Cash Format => {0} = {Cash amount}, {1} = {Cash Label}")]
        public string cashFormat = "{1}: {0}";
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
            if (uiCashPackageDialog != null)
                uiCashPackageDialog.onHide.AddListener(OnCashPackageDialogHide);
            RefreshCashPackageInfo();
        }

        public override void Hide()
        {
            if (uiCashPackageDialog != null)
                uiCashPackageDialog.onHide.RemoveListener(OnCashPackageDialogHide);
            CacheCashPackageSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnCashPackageDialogHide()
        {
            CacheCashPackageSelectionManager.DeselectSelectedUI();
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
            {
                uiCashPackageDialog.onHide.RemoveListener(OnCashPackageDialogHide);
                uiCashPackageDialog.Hide();
                uiCashPackageDialog.onHide.AddListener(OnCashPackageDialogHide);
            }
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
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_LABEL_ERROR.ToString()), errorMessage);
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
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UILocaleKeys.UI_CANNOT_GET_CASH_PACKAGE_INFO.ToString()));
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UILocaleKeys.UI_CONNECTION_TIMEOUT.ToString()));
                    break;
                default:
                    if (uiTextCash != null)
                        uiTextCash.text = string.Format(cashFormat, castedMessage.cash.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_LABEL_CASH.ToString()));
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
