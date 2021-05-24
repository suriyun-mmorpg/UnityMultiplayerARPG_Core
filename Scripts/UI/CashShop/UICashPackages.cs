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
        public GameObject listEmptyObject;
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

        private UICashPackageSelectionManager cacheSelectionManager;
        public UICashPackageSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UICashPackageSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
            }
        }

        public void RefreshCashPackageInfo()
        {
            // Load cash shop item list
            GameInstance.ClientCashShopHandlers.RequestCashPackageInfo(ResponseCashPackageInfo);
        }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelect.RemoveListener(OnSelectCashPackage);
            CacheSelectionManager.eventOnSelect.AddListener(OnSelectCashPackage);
            CacheSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCashPackage);
            CacheSelectionManager.eventOnDeselect.AddListener(OnDeselectCashPackage);
            if (uiCashPackageDialog != null)
                uiCashPackageDialog.onHide.AddListener(OnCashPackageDialogHide);
            RefreshCashPackageInfo();
        }

        protected virtual void OnDisable()
        {
            if (uiCashPackageDialog != null)
                uiCashPackageDialog.onHide.RemoveListener(OnCashPackageDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected void OnCashPackageDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCashPackage(UICashPackage ui)
        {
            if (uiCashPackageDialog != null && ui.Data != null)
            {
                uiCashPackageDialog.selectionManager = CacheSelectionManager;
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

        private void ResponseCashPackageInfo(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCashPackageInfoMessage response)
        {
            ClientCashShopActions.ResponseCashPackageInfo(requestHandler, responseCode, response);
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

            int selectedIdx = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.IndexOf(CacheSelectionManager.SelectedUI) : -1;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            UICashPackage tempUiCashPackage;
            CacheList.Generate(cashPackages, (index, cashShopItem, ui) =>
            {
                tempUiCashPackage = ui.GetComponent<UICashPackage>();
                tempUiCashPackage.uiCashPackages = this;
                tempUiCashPackage.Data = cashShopItem;
                tempUiCashPackage.Show();
                CacheSelectionManager.Add(tempUiCashPackage);
                if (selectedIdx == index)
                    tempUiCashPackage.OnClickSelect();
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(cashPackages.Count == 0);
        }
    }
}
