using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UICraftingItems : UIBase
    {
        public UICraftingItem uiDialog;
        public UICraftingItem uiPrefab;
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

        private UICraftingItemSelectionManager cacheItemSelectionManager;
        public UICraftingItemSelectionManager CacheItemSelectionManager
        {
            get
            {
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = gameObject.GetOrAddComponent<UICraftingItemSelectionManager>();
                cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheItemSelectionManager;
            }
        }

        protected virtual void OnEnable()
        {
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelectCraftingItem);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelectCraftingItem);
            CacheItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselectCraftingItem);
            CacheItemSelectionManager.eventOnDeselected.AddListener(OnDeselectCraftingItem);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnItemDialogHide);
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnItemDialogHide()
        {
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCraftingItem(UICraftingItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheItemSelectionManager;
                uiDialog.Data = ui.Data;
                uiDialog.Show();
            }
        }

        protected void OnDeselectCraftingItem(UICraftingItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnItemDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnItemDialogHide);
            }
        }

        protected void UpdateData(IList<CraftingItem> craftingItems)
        {
            int selectedIdx = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.IndexOf(CacheItemSelectionManager.SelectedUI) : -1;
            CacheItemSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.Clear();

            UICraftingItem tempUiCraftingItem;
            CacheItemList.Generate(craftingItems, (index, craftingItem, ui) =>
            {
                tempUiCraftingItem = ui.GetComponent<UICraftingItem>();
                tempUiCraftingItem.Data = craftingItem;
                tempUiCraftingItem.Show();
                CacheItemSelectionManager.Add(tempUiCraftingItem);
                if (selectedIdx == index)
                    tempUiCraftingItem.OnClickSelect();
            });
        }
    }
}
