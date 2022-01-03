using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    [DefaultExecutionOrder(101)]
    public class UIItemCraftFormulas : UIBase
    {
        [Header("Filter")]
        public List<string> filterCategories = new List<string>();

        [Header("UI Elements")]
        public GameObject listEmptyObject;
        public UIItemCraftFormula uiDialog;
        public UIItemCraftFormula uiPrefab;
        public Transform uiContainer;
        public bool selectFirstEntryByDefault;

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

        private UIItemCraftFormulaSelectionManager cacheSelectionManager;
        public UIItemCraftFormulaSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UIItemCraftFormulaSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
            }
        }

        public UICraftingQueueItems CraftingQueueManager { get; set; }
        public List<ItemCraftFormula> LoadedList { get; private set; } = new List<ItemCraftFormula>();

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselected.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselected.AddListener(OnDeselect);
            if (uiDialog != null)
            {
                uiDialog.onHide.AddListener(OnDialogHide);
                uiDialog.CraftFormulaManager = this;
            }
            UpdateData();
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

        protected virtual void OnSelect(UIItemCraftFormula ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.Data = ui.Data;
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UIItemCraftFormula ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public virtual void UpdateData()
        {
            int sourceId = CraftingQueueManager != null && CraftingQueueManager.Source != null ? CraftingQueueManager.Source.SourceId : 0;
            LoadedList.Clear();
            LoadedList.AddRange(GameInstance.ItemCraftFormulas.Values.Where(o => o.SourceIds.Contains(sourceId)));
            GenerateList();
        }

        public virtual void GenerateList()
        {
            int selectedDataId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.DataId : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            List<ItemCraftFormula> filteredList = UIItemCraftFormulasUtils.GetFilteredList(LoadedList, filterCategories);
            if (filteredList.Count == 0)
            {
                if (uiDialog != null)
                    uiDialog.Hide();
                CacheList.HideAll();
                if (listEmptyObject != null)
                    listEmptyObject.SetActive(true);
                return;
            }

            if (listEmptyObject != null)
                listEmptyObject.SetActive(false);

            UIItemCraftFormula tempUI;
            CacheList.Generate(filteredList, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UIItemCraftFormula>();
                tempUI.CraftFormulaManager = this;
                tempUI.Data = data;
                tempUI.Show();
                CacheSelectionManager.Add(tempUI);
                if ((selectFirstEntryByDefault && index == 0) || selectedDataId == data.DataId)
                    tempUI.OnClickSelect();
            });
        }
    }
}
