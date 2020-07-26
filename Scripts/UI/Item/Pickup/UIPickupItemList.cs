using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	public class UIPickupItemList : UIBase
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

        public void UpdateData(List<CharacterItem> droppedItems)
        {
            string selectedId = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.CharacterItem.id : string.Empty;
            CacheItemSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.Clear();
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
