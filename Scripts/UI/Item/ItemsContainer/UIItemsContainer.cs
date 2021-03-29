using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class UIItemsContainer : UICharacterItems
    {
        public bool pickUpOnSelect;
        private bool readyToPickUp;

        public ItemsContainerEntity TargetEntity { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (TargetEntity != null)
                TargetEntity.Items.onOperation += OnItemsOperation;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (TargetEntity != null)
                TargetEntity.Items.onOperation -= OnItemsOperation;
        }

        protected override void Update()
        {
            base.Update();
            if (TargetEntity == null || !GameInstance.PlayingCharacterEntity.IsGameEntityInDistance(TargetEntity, GameInstance.Singleton.pickUpItemDistance))
                Hide();
        }

        public void Show(ItemsContainerEntity targetEntity)
        {
            UpdateData(targetEntity);
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

        protected override void OnSelect(UICharacterItem ui)
        {
            base.OnSelect(ui);
            if (pickUpOnSelect && readyToPickUp)
                OnClickPickUpSelectedItem();
        }

        public void OnClickPickUpSelectedItem()
        {
            int selectedIndex = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.IndexOfData : -1;
            if (selectedIndex < 0)
                return;
            GameInstance.PlayingCharacterEntity.CallServerPickupItemFromContainer(TargetEntity.ObjectId, selectedIndex);
        }

        public void UpdateData(ItemsContainerEntity targetEntity)
        {
            if (targetEntity == null || !GameInstance.PlayingCharacterEntity.IsGameEntityInDistance(targetEntity, GameInstance.Singleton.pickUpItemDistance))
                return;
            TargetEntity = targetEntity;
            UpdateData(TargetEntity.Items);
        }

        public void UpdateData(IList<CharacterItem> characterItems)
        {
            readyToPickUp = false;
            string selectedId = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.Data.characterItem.id : string.Empty;
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
                if (!string.IsNullOrEmpty(selectedId) && selectedId.Equals(characterItem.id))
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
            readyToPickUp = true;
        }
    }
}