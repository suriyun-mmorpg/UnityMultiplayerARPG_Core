using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [DefaultExecutionOrder(100)]
    public class UICraftingQueueItems : UIBase
    {
        [Header("UI Elements")]
        public GameObject listEmptyObject;
        public UICraftingQueueItem uiDialog;
        public UICraftingQueueItem uiPrefab;
        public Transform uiContainer;
        public UIItemCraftFormulas uiFormulas;

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
            {
                uiDialog.onHide.AddListener(OnItemDialogHide);
                uiDialog.Manager = this;
            }
            if (Source == null && GameInstance.PlayingCharacterEntity)
                Source = GameInstance.PlayingCharacterEntity.Crafting;
            if (Source != null)
            {
                Source.QueueItems.onOperation += OnCraftingQueueItemsOperation;
                uiFormulas.Source = Source;
                uiFormulas.Show();
            }
            UpdateData();
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
            if (Source != null)
            {
                Source.QueueItems.onOperation -= OnCraftingQueueItemsOperation;
                Source = null;
            }
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

        protected void OnCraftingQueueItemsOperation(LiteNetLibSyncList.Operation op, int itemIndex)
        {
            UpdateData();
        }

        protected void UpdateData()
        {
            int selectedIdx = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.IndexOf(CacheItemSelectionManager.SelectedUI) : -1;
            CacheItemSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.Clear();

            UICraftingQueueItem tempUI;
            CacheItemList.Generate(GameInstance.PlayingCharacterEntity.Crafting.QueueItems, (index, craftingItem, ui) =>
            {
                tempUI = ui.GetComponent<UICraftingQueueItem>();
                tempUI.Manager = this;
                tempUI.Setup(craftingItem, GameInstance.PlayingCharacterEntity, index);
                tempUI.Show();
                CacheItemSelectionManager.Add(tempUI);
                if (selectedIdx == index)
                    tempUI.OnClickSelect();
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(GameInstance.PlayingCharacterEntity.Crafting.QueueItems.Count == 0);
        }
    }
}
