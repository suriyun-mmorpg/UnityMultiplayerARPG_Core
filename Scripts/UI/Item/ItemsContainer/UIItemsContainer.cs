using LiteNetLibManager;
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

        public void OnClickPickUpAllItems()
        {
            GameInstance.PlayingCharacterEntity.CallServerPickupAllItemsFromContainer(TargetEntity.ObjectId);
        }

        public void OnClickPickUpSelectedItem()
        {
            int selectedIndex = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.IndexOfData : -1;
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

        public virtual void UpdateData(IList<CharacterItem> characterItems)
        {
            readyToPickUp = false;
            string selectedId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.characterItem.id : string.Empty;
            CacheSelectionManager.Clear();

            if (characterItems == null || characterItems.Count == 0)
            {
                if (uiDialog != null)
                    uiDialog.Hide();
                CacheList.HideAll();
                if (listEmptyObject != null)
                    listEmptyObject.SetActive(true);
                return;
            }

            UICharacterItem selectedUI = null;
            UICharacterItem tempUI;
            CacheList.Generate(characterItems, (index, characterItem, ui) =>
            {
                tempUI = ui.GetComponent<UICharacterItem>();
                tempUI.Setup(new UICharacterItemData(characterItem, InventoryType.Unknow), GameInstance.PlayingCharacter, index);
                tempUI.Show();
                UICharacterItemDragHandler dragHandler = tempUI.GetComponentInChildren<UICharacterItemDragHandler>();
                if (dragHandler != null)
                    dragHandler.SetupForStorageItems(tempUI);
                CacheSelectionManager.Add(tempUI);
                if (!string.IsNullOrEmpty(selectedId) && selectedId.Equals(characterItem.id))
                    selectedUI = tempUI;
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(false);
            if (selectedUI == null)
            {
                CacheSelectionManager.DeselectSelectedUI();
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