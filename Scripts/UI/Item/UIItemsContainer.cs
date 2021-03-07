using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public class UIItemsContainer : UIBase
    {
        [Header("UI Elements")]
        public GameObject listEmptyObject;
        [FormerlySerializedAs("uiItemDialog")]
        public UICharacterItem uiDialog;
        public UICharacterItem uiPrefab;
        public Transform uiContainer;

        [Header("Other Settings")]
        public bool pickUpOnSelect;

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

        private UICharacterItemSelectionManager cacheItemSelectionManager;
        public UICharacterItemSelectionManager CacheItemSelectionManager
        {
            get
            {
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = gameObject.GetOrAddComponent<UICharacterItemSelectionManager>();
                cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheItemSelectionManager;
            }
        }

        public ItemsContainerEntity TargetEntity { get; private set; }

        protected virtual void OnEnable()
        {
            if (TargetEntity != null)
                TargetEntity.Items.onOperation += OnItemsOperation;
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelect);
            CacheItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselect);
            CacheItemSelectionManager.eventOnDeselected.AddListener(OnDeselect);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnDialogHide);
        }

        protected virtual void OnDisable()
        {
            if (TargetEntity != null)
                TargetEntity.Items.onOperation -= OnItemsOperation;
            // Hide
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        private void Update()
        {
            if (TargetEntity == null || Vector3.Distance(GameInstance.PlayingCharacterEntity.CacheTransform.position, TargetEntity.CacheTransform.position) > GameInstance.Singleton.pickUpItemDistance)
                Hide();
        }

        public void Show(ItemsContainerEntity targetEntity)
        {
            if (targetEntity == null || Vector3.Distance(GameInstance.PlayingCharacterEntity.CacheTransform.position, targetEntity.CacheTransform.position) > GameInstance.Singleton.pickUpItemDistance)
                return;
            TargetEntity = targetEntity;
            Show();
        }

        protected void OnDialogHide()
        {
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData(TargetEntity.Items);
        }

        protected void OnSelect(UICharacterItem ui)
        {
            if (ui.Data.characterItem.IsEmptySlot())
            {
                CacheItemSelectionManager.DeselectSelectedUI();
                return;
            }
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheItemSelectionManager;
                uiDialog.Setup(ui.Data, GameInstance.PlayingCharacter, ui.IndexOfData);
                uiDialog.Show();
            }
            if (pickUpOnSelect)
                OnClickPickUpSelectedItem();
        }

        protected void OnDeselect(UICharacterItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public void OnClickPickUpSelectedItem()
        {
            int selectedIndex = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.IndexOfData : -1;
            if (selectedIndex < 0)
                return;
            GameInstance.PlayingCharacterEntity.CallServerPickupItemFromContainer(TargetEntity.ObjectId, selectedIndex);
        }

        public void UpdateData(IList<CharacterItem> characterItems)
        {
            int selectedIndex = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.IndexOfData : -1;
            CacheItemSelectionManager.Clear();

            if (characterItems == null || characterItems.Count == 0)
            {
                if (uiDialog != null)
                    uiDialog.Hide();
                CacheItemList.HideAll();
                if (listEmptyObject != null)
                    listEmptyObject.SetActive(true);
                return;
            }

            UICharacterItem selectedUI = null;
            UICharacterItem tempUI;
            CacheItemList.Generate(characterItems, (index, characterItem, ui) =>
            {
                tempUI = ui.GetComponent<UICharacterItem>();
                tempUI.Setup(new UICharacterItemData(characterItem, InventoryType.Unknow), GameInstance.PlayingCharacter, index);
                tempUI.Show();
                UICharacterItemDragHandler dragHandler = tempUI.GetComponentInChildren<UICharacterItemDragHandler>();
                if (dragHandler != null)
                    dragHandler.SetupForStorageItems(tempUI);
                CacheItemSelectionManager.Add(tempUI);
                if (selectedIndex == index)
                    selectedUI = tempUI;
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(false);
            if (selectedUI == null)
            {
                CacheItemSelectionManager.DeselectSelectedUI();
            }
            else
            {
                bool defaultDontShowComparingEquipments = uiDialog != null ? uiDialog.dontShowComparingEquipments : false;
                if (uiDialog != null)
                    uiDialog.dontShowComparingEquipments = true;
                selectedUI.OnClickSelect();
                if (uiDialog != null)
                    uiDialog.dontShowComparingEquipments = defaultDontShowComparingEquipments;
            }
        }
    }
}