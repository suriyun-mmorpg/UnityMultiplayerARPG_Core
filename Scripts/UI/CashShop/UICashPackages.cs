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
        public List<string> filterCategories = new List<string>();

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

        public List<CashPackage> LoadedList { get; private set; } = new List<CashPackage>();

        public void Refresh()
        {
            // Load cash shop item list
            GameInstance.ClientCashShopHandlers.RequestCashPackageInfo(ResponseInfo);
        }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelect.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelect.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselect.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselect.AddListener(OnDeselect);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnDialogHide);
            Refresh();
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
                uiDialog.uiCashPackages = this;
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
            Refresh();
        }

        protected virtual void ResponseInfo(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCashPackageInfoMessage response)
        {
            ClientCashShopActions.ResponseCashPackageInfo(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;

            if (uiTextCash != null)
            {
                uiTextCash.text = string.Format(
                    LanguageManager.GetText(formatKeyCash),
                    response.cash.ToString("N0"));
            }

            LoadedList.Clear();
            foreach (int cashPackageId in response.cashPackageIds)
            {
                CashPackage cashPackage;
                if (GameInstance.CashPackages.TryGetValue(cashPackageId, out cashPackage))
                {
                    LoadedList.Add(cashPackage);
                }
            }

            GenerateList();
        }

        public virtual void GenerateList()
        {
            int selectedDataId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.DataId : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();
            ConvertFilterCategoriesToTrimedLowerChar();

            int showingCount = 0;
            bool selectedOnce = false;
            UICashPackage tempUI;
            CacheList.Generate(LoadedList, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UICashPackage>();
                if (data != null &&
                    (filterCategories.Count == 0 || (!string.IsNullOrEmpty(data.Category) &&
                    filterCategories.Contains(data.Category.Trim().ToLower()))))
                {
                    tempUI.uiCashPackages = this;
                    tempUI.Data = data;
                    tempUI.Show();
                    CacheSelectionManager.Add(tempUI);
                    if (!selectedOnce || selectedDataId == data.DataId)
                    {
                        selectedOnce = true;
                        tempUI.OnClickSelect();
                    }
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

        protected void ConvertFilterCategoriesToTrimedLowerChar()
        {
            for (int i = 0; i < filterCategories.Count; ++i)
            {
                filterCategories[i] = filterCategories[i].Trim().ToLower();
            }
        }
    }
}
