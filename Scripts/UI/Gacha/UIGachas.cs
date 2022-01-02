using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class UIGachas : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Cash Amount}")]
        public UILocaleKeySetting formatKeyCash = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CASH);

        [Header("Filter")]
        public List<string> filterCategories = new List<string>();

        [Header("UI Elements")]
        public GameObject listEmptyObject;
        public UIGacha uiDialog;
        public UIGacha uiPrefab;
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

        private UIGachaSelectionManager cacheSelectionManager;
        public UIGachaSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UIGachaSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.Toggle;
                return cacheSelectionManager;
            }
        }

        public List<Gacha> LoadedList { get; private set; } = new List<Gacha>();

        public void Refresh()
        {
            // Load gacha list
            GameInstance.ClientGachaHandlers.RequestGachaInfo(ResponseInfo);
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

        protected virtual void OnSelect(UIGacha ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.uiGachas = this;
                uiDialog.Data = ui.Data;
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UIGacha ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public void Buy(int dataId, GachaOpenMode openMode)
        {
            GameInstance.ClientGachaHandlers.RequestOpenGacha(new RequestOpenGachaMessage()
            {
                dataId = dataId,
                openMode = openMode,
            }, ResponseBuy);
        }

        protected virtual void ResponseInfo(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseGachaInfoMessage response)
        {
            ClientGachaActions.ResponseGachaInfo(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;

            if (uiTextCash != null)
            {
                uiTextCash.text = string.Format(
                    LanguageManager.GetText(formatKeyCash),
                    response.cash.ToString("N0"));
            }

            LoadedList.Clear();
            foreach (int gachaId in response.gachaIds)
            {
                Gacha gacha;
                if (GameInstance.Gachas.TryGetValue(gachaId, out gacha))
                    LoadedList.Add(gacha);
            }

            GenerateList();
        }

        protected virtual void ResponseBuy(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseOpenGachaMessage response)
        {
            ClientGachaActions.ResponseOpenGacha(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            if (uiRewarding != null)
            {
                uiRewarding.Data = new UIRewardingData()
                {
                    rewardItems = response.rewardItems,
                };
                uiRewarding.Show();
            }
            else
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_SUCCESS.ToString()), LanguageManager.GetText(UITextKeys.UI_CASH_SHOP_ITEM_BOUGHT.ToString()));
            }
            Refresh();
        }

        public virtual void GenerateList()
        {
            int selectedDataId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.DataId : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();
            ConvertFilterCategoriesToTrimedLowerChar();

            int showingCount = 0;
            bool selectedOnce = false;
            UIGacha tempUI;
            CacheList.Generate(LoadedList, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UIGacha>();
                if (data != null &&
                    (filterCategories.Count == 0 || (!string.IsNullOrEmpty(data.Category) &&
                    filterCategories.Contains(data.Category.Trim().ToLower()))))
                {
                    tempUI.uiGachas = this;
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
