using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    [DefaultExecutionOrder(101)]
    public class UIItemCraftFormulas : UIBase
    {
        [Header("Filter")]
        public List<string> filterCategories;

        [Header("UI Elements")]
        public GameObject listEmptyObject;
        public UIItemCraftFormula uiDialog;
        public UIItemCraftFormula uiPrefab;
        public Transform uiContainer;

        public UICraftingQueueItems CraftingQueueManager { get; set; }

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

        protected virtual void UpdateData()
        {
            int selectedIdx = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.IndexOf(CacheSelectionManager.SelectedUI) : -1;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            int sourceId = CraftingQueueManager != null && CraftingQueueManager.Source != null ? CraftingQueueManager.Source.SourceId : 0;
            int showingCount = 0;
            UIItemCraftFormula tempUI;
            CacheList.Generate(GameInstance.ItemCraftFormulas.Values.Where(o => o.SourceId == sourceId), (index, formula, ui) =>
            {
                tempUI = ui.GetComponent<UIItemCraftFormula>();
                if (string.IsNullOrEmpty(formula.category) ||
                    filterCategories == null || filterCategories.Count == 0 ||
                    filterCategories.Contains(formula.category))
                {
                    tempUI.CraftFormulaManager = this;
                    tempUI.Data = formula;
                    tempUI.Show();
                    CacheSelectionManager.Add(tempUI);
                    if (selectedIdx == index)
                        tempUI.OnClickSelect();
                    showingCount++;
                }
                else
                {
                    // Hide because formula's category not matches in the filter list
                    tempUI.Hide();
                }
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(showingCount == 0);
        }
    }
}
