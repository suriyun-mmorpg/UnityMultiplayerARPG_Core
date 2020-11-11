using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIEnhanceSocketItem : UIBaseOwningCharacterItem
    {
        public IEquipmentItem EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public byte MaxSocket { get { return GameInstance.Singleton.GameplayRule.GetItemMaxSocket(OwningCharacter, CharacterItem); } }
        public bool CanEnhance { get { return MaxSocket > 0 && CharacterItem.Sockets.Count < MaxSocket; } }
        public int SelectedEnhancerId
        {
            get
            {
                if (uiSocketEnhancerItems.CacheItemSelectionManager != null &&
                    uiSocketEnhancerItems.CacheItemSelectionManager.SelectedUI != null &&
                    uiSocketEnhancerItems.CacheItemSelectionManager.SelectedUI.SocketEnhancerItem != null)
                    return uiSocketEnhancerItems.CacheItemSelectionManager.SelectedUI.SocketEnhancerItem.DataId;
                return 0;
            }
        }

        public int SelectedSocketIndex
        {
            get
            {
                if (uiAppliedSocketEnhancerItems.CacheItemSelectionManager != null &&
                    uiAppliedSocketEnhancerItems.CacheItemSelectionManager.SelectedUI != null)
                    return uiAppliedSocketEnhancerItems.CacheItemSelectionManager.SelectedUI.IndexOfData;
                return -1;
            }
        }

        [Header("UI Elements for UI Enhance Socket Item")]
        public UINonEquipItems uiSocketEnhancerItems;
        public UICharacterItems uiAppliedSocketEnhancerItems;

        protected bool activated;
        protected string activeItemId;

        public override void OnUpdateCharacterItems()
        {
            if (!IsVisible())
                return;

            // Store data to variable so it won't lookup for data from property again
            CharacterItem characterItem = CharacterItem;

            if (activated && (characterItem.IsEmptySlot() || !characterItem.id.Equals(activeItemId)))
            {
                // Item's ID is difference to active item ID, so the item may be destroyed
                // So clear data
                Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
                return;
            }

            if (uiCharacterItem != null)
            {
                if (characterItem.IsEmptySlot())
                {
                    uiCharacterItem.Hide();
                }
                else
                {
                    uiCharacterItem.Setup(new UICharacterItemData(characterItem, InventoryType), OwningCharacter, IndexOfData);
                    uiCharacterItem.Show();
                }
            }

            if (uiSocketEnhancerItems != null)
            {
                uiSocketEnhancerItems.filterItemTypes = new List<ItemType>() { ItemType.SocketEnhancer };
                uiSocketEnhancerItems.filterCategories = new List<string>();
                uiSocketEnhancerItems.UpdateData(OwningCharacter);
            }

            if (uiAppliedSocketEnhancerItems != null)
            {
                uiAppliedSocketEnhancerItems.filterItemTypes = new List<ItemType>() { ItemType.SocketEnhancer };
                uiAppliedSocketEnhancerItems.filterCategories = new List<string>();
                List<CharacterItem> characterItems = new List<CharacterItem>();
                if (EquipmentItem != null)
                {
                    for (int i = 0; i < characterItem.Sockets.Count; ++i)
                    {
                        if (characterItem.Sockets[i] == 0)
                            characterItems.Add(CharacterItem.CreateEmptySlot());
                        else
                            characterItems.Add(CharacterItem.Create(characterItem.Sockets[i]));
                    }
                }
                uiAppliedSocketEnhancerItems.UpdateData(OwningCharacter, characterItems);
            }
        }

        public override void Show()
        {
            base.Show();
            activated = false;
            OnUpdateCharacterItems();
        }

        public override void Hide()
        {
            base.Hide();
            Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
        }

        public void OnClickEnhanceSocket()
        {
            if (CharacterItem.IsEmptySlot() || SelectedEnhancerId == 0)
                return;
            activated = true;
            activeItemId = CharacterItem.id;
            OwningCharacter.CallServerEnhanceSocketItem(InventoryType, (short)IndexOfData, SelectedEnhancerId, -1);
        }

        public void OnClickRemoveEnhancer()
        {
            if (CharacterItem.IsEmptySlot() || SelectedSocketIndex < 0)
                return;
            activated = true;
            activeItemId = CharacterItem.id;
            OwningCharacter.CallServerRemoveEnhancerFromItem(InventoryType, (short)IndexOfData, (short)SelectedSocketIndex);
        }
    }
}
