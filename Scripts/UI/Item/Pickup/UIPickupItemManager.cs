using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIPickupItemManager : UIBase
    {
        public UICharacterItem uiCharacterItemPrefab;
        public Transform uiCharacterItemContainer;
        public bool pickUpOnSelect;

        private UIList cacheItemList;
        public UIList CacheItemList
        {
            get
            {
                if (cacheItemList == null)
                {
                    cacheItemList = gameObject.AddComponent<UIList>();
                    cacheItemList.uiPrefab = uiCharacterItemPrefab.gameObject;
                    cacheItemList.uiContainer = uiCharacterItemContainer;
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

        public NearbyEntityDetector ItemDropEntityDetector { get; protected set; }

        protected override void Awake()
        {
            base.Awake();
            GameObject tempGameObject = new GameObject("_ItemDropEntityDetector");
            ItemDropEntityDetector = tempGameObject.AddComponent<NearbyEntityDetector>();
            ItemDropEntityDetector.detectingRadius = GameInstance.Singleton.pickUpItemDistance;
            ItemDropEntityDetector.findItemDrop = true;
            ItemDropEntityDetector.onUpdateList += OnUpdate;
        }

        private void OnDestroy()
        {
            if (ItemDropEntityDetector != null)
            {
                ItemDropEntityDetector.onUpdateList -= OnUpdate;
                Destroy(ItemDropEntityDetector.gameObject);
            }
        }

        public override void Show()
        {
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelectCharacterItem);
            base.Show();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            if (ui.Data.characterItem.IsEmptySlot())
            {
                CacheItemSelectionManager.DeselectSelectedUI();
                return;
            }
            if (pickUpOnSelect)
                OnClickPickUpSelectedItem();
        }

        private void OnUpdate()
        {
            string selectedId = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.CharacterItem.id : string.Empty;
            CacheItemSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.Clear();

            List<CharacterItem> droppedItems = new List<CharacterItem>();
            string tempEntryId;
            CharacterItem tempCharacterItem;
            foreach (ItemDropEntity entity in ItemDropEntityDetector.itemDrops)
            {
                if (entity.PlaceHolderItem == null)
                {
                    // Only place holder item drop entity will be shown in the list
                    continue;
                }
                tempEntryId = entity.ObjectId.ToString();
                tempCharacterItem = CharacterItem.Create(entity.PlaceHolderItem, entity.PlaceHolderLevel, entity.PlaceHolderAmount);
                tempCharacterItem.id = tempEntryId;
                droppedItems.Add(tempCharacterItem);
            }

            BaseItem tempItem;
            UICharacterItem tempUiCharacterItem;
            CacheItemList.Generate(droppedItems, (index, characterItem, ui) =>
            {
                tempUiCharacterItem = ui.GetComponent<UICharacterItem>();
                tempItem = characterItem.GetItem();
                CacheItemSelectionManager.Add(tempUiCharacterItem);
                if (!string.IsNullOrEmpty(selectedId) && selectedId.Equals(characterItem.id))
                    tempUiCharacterItem.OnClickSelect();
            });
        }

        public void OnClickPickUpSelectedItem()
        {
            string selectedId = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.CharacterItem.id : string.Empty;
            if (string.IsNullOrEmpty(selectedId))
                return;
            BasePlayerCharacterController.OwningCharacter.RequestPickupItem(uint.Parse(selectedId));
        }
    }
}
