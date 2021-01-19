using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG
{
    public partial class UICashPackages : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Cash Amount}")]
        public UILocaleKeySetting formatKeyCash = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CASH);

        [Header("UI Elements")]
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
                    cacheCashPackageSelectionManager = gameObject.GetOrAddComponent<UICashPackageSelectionManager>();
                cacheCashPackageSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheCashPackageSelectionManager;
            }
        }

        public void RefreshCashPackageInfo()
        {
            // Load cash shop item list
            GameInstance.ClientCashShopHandlers.RequestCashPackageInfo(ResponseCashPackageInfo);
        }

        protected virtual void OnEnable()
        {
            CacheCashPackageSelectionManager.eventOnSelect.RemoveListener(OnSelectCashPackage);
            CacheCashPackageSelectionManager.eventOnSelect.AddListener(OnSelectCashPackage);
            CacheCashPackageSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCashPackage);
            CacheCashPackageSelectionManager.eventOnDeselect.AddListener(OnDeselectCashPackage);
            if (uiCashPackageDialog != null)
                uiCashPackageDialog.onHide.AddListener(OnCashPackageDialogHide);
            RefreshCashPackageInfo();
        }

        protected virtual void OnDisable()
        {
            if (uiCashPackageDialog != null)
                uiCashPackageDialog.onHide.RemoveListener(OnCashPackageDialogHide);
            CacheCashPackageSelectionManager.DeselectSelectedUI();
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
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), errorMessage);
                return;
            }
            RefreshCashPackageInfo();
        }

        private async UniTaskVoid ResponseCashPackageInfo(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCashPackageInfoMessage response)
        {
            await UniTask.Yield();
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;

            if (uiTextCash != null)
            {
                uiTextCash.text = string.Format(
                    LanguageManager.GetText(formatKeyCash),
                    response.cash.ToString("N0"));
            }

            List<CashPackage> cashPackages = new List<CashPackage>();
            foreach (int cashPackageId in response.cashPackageIds)
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

            UICashPackage tempUiCashPackage;
            CacheCashPackageList.Generate(cashPackages, (index, cashShopItem, ui) =>
            {
                tempUiCashPackage = ui.GetComponent<UICashPackage>();
                tempUiCashPackage.uiCashPackages = this;
                tempUiCashPackage.Data = cashShopItem;
                tempUiCashPackage.Show();
                CacheCashPackageSelectionManager.Add(tempUiCashPackage);
                if (selectedIdx == index)
                    tempUiCashPackage.OnClickSelect();
            });
        }
    }
}
