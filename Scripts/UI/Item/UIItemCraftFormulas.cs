using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIItemCraftFormulas : UIBase
    {
        public UIItemCraftFormula uiDialog;
        public UIItemCraftFormula uiPrefab;
        public Transform uiContainer;

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
                uiDialog.onHide.AddListener(OnItemDialogHide);
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

            UIItemCraftFormula tempUiItemCraftFormula;
            CacheItemList.Generate(GameInstance.ItemCraftFormulas.Values, (index, formula, ui) =>
            {
                tempUiItemCraftFormula = ui.GetComponent<UIItemCraftFormula>();
                tempUiItemCraftFormula.Data = formula;
                tempUiItemCraftFormula.Show();
                CacheItemSelectionManager.Add(tempUiItemCraftFormula);
                if (selectedIdx == index)
                    tempUiItemCraftFormula.OnClickSelect();
            });
        }
    }
}
