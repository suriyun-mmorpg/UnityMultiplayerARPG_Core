using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UICraftingQueueItems : UIBase
    {
        public UICraftingQueueItem uiDialog;
        public UICraftingQueueItem uiPrefab;
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

        private UICraftingQueueItemSelectionManager cacheItemSelectionManager;
        public UICraftingQueueItemSelectionManager CacheItemSelectionManager
        {
            get
            {
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = gameObject.GetOrAddComponent<UICraftingQueueItemSelectionManager>();
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

        protected void OnSelectCraftingItem(UICraftingQueueItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheItemSelectionManager;
                uiDialog.Data = ui.Data;
                uiDialog.Show();
            }
        }

        protected void OnDeselectCraftingItem(UICraftingQueueItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnItemDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnItemDialogHide);
            }
        }

        protected void UpdateData(IList<CraftingQueueItem> craftingItems)
        {
            int selectedIdx = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.IndexOf(CacheItemSelectionManager.SelectedUI) : -1;
            CacheItemSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.Clear();

            UICraftingQueueItem tempUiCraftingItem;
            CacheItemList.Generate(craftingItems, (index, craftingItem, ui) =>
            {
                tempUiCraftingItem = ui.GetComponent<UICraftingQueueItem>();
                tempUiCraftingItem.Data = craftingItem;
                tempUiCraftingItem.Show();
                CacheItemSelectionManager.Add(tempUiCraftingItem);
                if (selectedIdx == index)
                    tempUiCraftingItem.OnClickSelect();
            });
        }
    }
}
