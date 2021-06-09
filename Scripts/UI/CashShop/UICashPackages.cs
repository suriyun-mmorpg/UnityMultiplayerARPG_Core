using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICashPackages : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Cash Amount}")]
        public UILocaleKeySetting formatKeyCash = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CASH);

        [Header("Filter")]
        public List<string> filterCategories;

        [Header("UI Elements")]
        public GameObject listEmptyObject;
        [FormerlySerializedAs("uiCashPackageDialog")]
        public UICashPackage uiDialog;
        [FormerlySerializedAs("uiCashPackagePrefab")]
        public UICashPackage uiPrefab;
        [FormerlySerializedAs("uiCashPackageContainer")]
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
            CacheSelectionManager.eventOnSelect.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelect.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselect.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselect.AddListener(OnDeselect);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnDialogHide);
            RefreshCashPackageInfo();
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

        protected virtual void OnSelect(UICashPackage ui)
        {
            if (uiDialog != null && ui.Data != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.Data = ui.Data;
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICashPackage ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
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

        protected virtual void ResponseCashPackageInfo(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCashPackageInfoMessage response)
        {
            ClientCashShopActions.ResponseCashPackageInfo(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;

            if (uiTextCash != null)
            {
                uiTextCash.text = string.Format(
                    LanguageManager.GetText(formatKeyCash),
                    response.cash.ToString("N0"));
            }

            List<CashPackage> list = new List<CashPackage>();
            foreach (int cashPackageId in response.cashPackageIds)
            {
                CashPackage cashPackage;
                if (GameInstance.CashPackages.TryGetValue(cashPackageId, out cashPackage))
                {
                    list.Add(cashPackage);
                }
            }

            int selectedIdx = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.IndexOf(CacheSelectionManager.SelectedUI) : -1;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            int showingCount = 0;
            UICashPackage tempUI;
            CacheList.Generate(list, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UICashPackage>();
                if (data == null ||
                    string.IsNullOrEmpty(data.category) ||
                    filterCategories == null || filterCategories.Count == 0 ||
                    filterCategories.Contains(data.category))
                {
                    tempUI.uiCashPackages = this;
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
    }
}
