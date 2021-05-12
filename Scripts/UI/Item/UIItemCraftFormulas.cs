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

        public ICraftingQueueSource Source { get; set; }

        private UIList cacheItemList;
        public UIList CacheItemList
        {
            get
            {
                if (cacheItemList == null)
                {
                    cacheItemList = gameObject.AddComponent<UIList>();
                    cacheItemList.uiPrefab = uiPrefab.gameObject;
                    cacheItemList.uiContainer = uiContainer;
                }
                return cacheItemList;
            }
        }

        private UIItemCraftFormulaSelectionManager cacheItemSelectionManager;
        public UIItemCraftFormulaSelectionManager CacheItemSelectionManager
        {
            get
            {
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = gameObject.GetOrAddComponent<UIItemCraftFormulaSelectionManager>();
                cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheItemSelectionManager;
            }
        }

        protected virtual void OnEnable()
        {
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelectItemCraftFormula);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelectItemCraftFormula);
            CacheItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselectItemCraftFormula);
            CacheItemSelectionManager.eventOnDeselected.AddListener(OnDeselectItemCraftFormula);
            if (uiDialog != null)
            {
                uiDialog.onHide.AddListener(OnItemDialogHide);
                uiDialog.Manager = this;
            }
            UpdateData();
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected void OnItemDialogHide()
        {
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectItemCraftFormula(UIItemCraftFormula ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheItemSelectionManager;
                uiDialog.Data = ui.Data;
                uiDialog.Show();
            }
        }

        protected void OnDeselectItemCraftFormula(UIItemCraftFormula ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnItemDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnItemDialogHide);
            }
        }

        protected void UpdateData()
        {
            int selectedIdx = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.IndexOf(CacheItemSelectionManager.SelectedUI) : -1;
            CacheItemSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.Clear();

            int sourceId = Source == null ? 0 : Source.SourceId;
            int showingCount = 0;
            UIItemCraftFormula tempUI;
            CacheItemList.Generate(GameInstance.ItemCraftFormulas.Values.Where(o => o.SourceId == sourceId), (index, formula, ui) =>
            {
                tempUI = ui.GetComponent<UIItemCraftFormula>();
                if (string.IsNullOrEmpty(formula.category) ||
                    filterCategories == null || filterCategories.Count == 0 ||
                    filterCategories.Contains(formula.category))
                {
                    tempUI.Manager = this;
                    tempUI.Data = formula;
                    tempUI.Show();
                    CacheItemSelectionManager.Add(tempUI);
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
