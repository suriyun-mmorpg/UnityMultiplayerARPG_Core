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
        public bool selectFirstEntryByDefault;

        public ICraftingQueueSource Source { get; set; }

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

        private UICraftingQueueItemSelectionManager cacheSelectionManager;
        public UICraftingQueueItemSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UICraftingQueueItemSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
            }
        }

        public void Show(ICraftingQueueSource source)
        {
            if (IsVisible())
                UnregisterSourceEvents();
            Source = source;
            if (IsVisible())
            {
                if (uiFormulas != null)
                {
                    uiFormulas.CraftingQueueManager = this;
                    uiFormulas.UpdateData();
                }
                RegisterSourceEvents();
                UpdateData();
            }
            Show();
        }

        public void ShowPlayerCraftingQueue()
        {
            if (IsVisible())
                UnregisterSourceEvents();
            Source = null;
            if (IsVisible())
            {
                if (uiFormulas != null)
                {
                    uiFormulas.CraftingQueueManager = this;
                    uiFormulas.UpdateData();
                }
                RegisterSourceEvents();
                UpdateData();
            }
            Show();
        }

        public void RegisterSourceEvents()
        {
            if (Source == null && GameInstance.PlayingCharacterEntity)
                Source = GameInstance.PlayingCharacterEntity.Crafting;
            if (Source != null)
                Source.QueueItems.onOperation += OnCraftingQueueItemsOperation;
        }

        public void UnregisterSourceEvents()
        {
            if (Source != null)
                Source.QueueItems.onOperation -= OnCraftingQueueItemsOperation;
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
                uiDialog.CraftingQueueManager = this;
            }
            if (uiFormulas != null)
            {
                uiFormulas.CraftingQueueManager = this;
                uiFormulas.Show();
            }
            RegisterSourceEvents();
            UpdateData();
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
            UnregisterSourceEvents();
            Source = null;
        }

        protected virtual void OnDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelect(UICraftingQueueItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.Data = ui.Data;
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICraftingQueueItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        protected void OnCraftingQueueItemsOperation(LiteNetLibSyncList.Operation op, int itemIndex)
        {
            UpdateData();
        }

        public virtual void UpdateData()
        {
            int selectedDataId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.dataId : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            UICraftingQueueItem tempUI;
            CacheList.Generate(Source.QueueItems, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UICraftingQueueItem>();
                tempUI.CraftingQueueManager = this;
                tempUI.Setup(data, GameInstance.PlayingCharacterEntity, index);
                tempUI.Show();
                CacheSelectionManager.Add(tempUI);
                if ((selectFirstEntryByDefault && index == 0) || selectedDataId == data.dataId)
                    tempUI.OnClickSelect();
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(Source.QueueItems.Count == 0);
        }
    }
}
