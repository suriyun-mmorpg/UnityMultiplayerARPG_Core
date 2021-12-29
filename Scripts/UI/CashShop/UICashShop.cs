using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICashShop : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Cash Amount}")]
        public UILocaleKeySetting formatKeyCash = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CASH);

        [Header("Filter")]
        public List<string> filterCategories = new List<string>();

        [Header("UI Elements")]
        public GameObject listEmptyObject;
        [FormerlySerializedAs("uiCashShopItemDialog")]
        public UICashShopItem uiDialog;
        [FormerlySerializedAs("uiCashShopItemPrefab")]
        public UICashShopItem uiPrefab;
        [FormerlySerializedAs("uiCashShopItemContainer")]
        public Transform uiContainer;
        public TextWrapper uiTextCash;
        public UIRewarding uiRewarding;

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

        private UICashShopSelectionManager cacheSelectionManager;
        public UICashShopSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UICashShopSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
            }
        }

        public List<CashShopItem> LoadedList { get; private set; } = new List<CashShopItem>();

        public void Refresh()
        {
            // Load cash shop item list
            GameInstance.ClientCashShopHandlers.RequestCashShopInfo(ResponseInfo);
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

        protected virtual void OnSelect(UICashShopItem ui)
        {
            if (uiDialog != null && ui.Data != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.uiCashShop = this;
                uiDialog.Data = ui.Data;
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICashShopItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public void Buy(int dataId, CashShopItemCurrencyType currencyType, int amount)
        {
            GameInstance.ClientCashShopHandlers.RequestCashShopBuy(new RequestCashShopBuyMessage()
            {
                dataId = dataId,
                currencyType = currencyType,
                amount = amount,
            }, ResponseBuy);
        }

        protected virtual void ResponseInfo(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCashShopInfoMessage response)
        {
            ClientCashShopActions.ResponseCashShopInfo(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;

            if (uiTextCash != null)
            {
                uiTextCash.text = string.Format(
                    LanguageManager.GetText(formatKeyCash),
                    response.cash.ToString("N0"));
            }

            LoadedList.Clear();
            foreach (int cashShopItemId in response.cashShopItemIds)
            {
                CashShopItem cashShopItem;
                if (GameInstance.CashShopItems.TryGetValue(cashShopItemId, out cashShopItem))
                    LoadedList.Add(cashShopItem);
            }

            GenerateList();
        }

        protected virtual void ResponseBuy(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCashShopBuyMessage response)
        {
            ClientCashShopActions.ResponseCashShopBuy(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            if (uiRewarding != null)
            {
                uiRewarding.Data = new UIRewardingData()
                {
                    rewardGold = response.rewardGold,
                    rewardItems = response.rewardItems.ToArray(),
                    rewardSkins = response.rewardItemSkins.ToArray(),
                    rewardCostumes = response.rewardCostumes.ToArray(),
                    rewardCharacters = response.rewardCharacters.ToArray(),
                    rewardCompanions = response.rewardCompanions.ToArray(),
                };
            }
            else
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_SUCCESS.ToString()), LanguageManager.GetText(UITextKeys.UI_CASH_SHOP_ITEM_BOUGHT.ToString()));
            }
            Refresh();
        }

        public virtual void GenerateList()
        {
            int selectedIdx = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.IndexOf(CacheSelectionManager.SelectedUI) : -1;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();
            ConvertFilterCategoriesToTrimedLowerChar();

            int showingCount = 0;
            UICashShopItem tempUI;
            CacheList.Generate(LoadedList, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UICashShopItem>();
                if (data != null &&
                    (filterCategories.Count == 0 || (!string.IsNullOrEmpty(data.Category) &&
                    filterCategories.Contains(data.Category.Trim().ToLower()))))
                {
                    tempUI.uiCashShop = this;
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

        protected void ConvertFilterCategoriesToTrimedLowerChar()
        {
            for (int i = 0; i < filterCategories.Count; ++i)
            {
                filterCategories[i] = filterCategories[i].Trim().ToLower();
            }
        }
    }
}
