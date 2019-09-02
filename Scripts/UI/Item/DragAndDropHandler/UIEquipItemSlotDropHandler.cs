using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public class UIEquipItemSlotDropHandler : MonoBehaviour, IDropHandler
    {
        public UICharacterItem uiCharacterItem;

        private RectTransform dropRect;
        public RectTransform DropRect
        {
            get
            {
                if (dropRect == null)
                    dropRect = transform as RectTransform;
                return dropRect;
            }
        }

        private void Start()
        {
            if (uiCharacterItem == null)
                uiCharacterItem = GetComponent<UICharacterItem>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (uiCharacterItem == null)
            {
                Debug.LogWarning("[UIEquipItemSlotDropHandler] `uicharacterItem` is empty");
                return;
            }
            // Validate drop position
            if (!RectTransformUtility.RectangleContainsScreenPoint(DropRect, Input.mousePosition))
                return;
            // Validate dragging UI
            UIDragHandler dragHandler = eventData.pointerDrag.GetComponent<UIDragHandler>();
            if (dragHandler == null || dragHandler.isDropped)
                return;
            // Set UI drop state
            dragHandler.isDropped = true;
            // Get dragged item UI, if dragging item UI is UI for character item.
            // try to equip the item
            UICharacterItemDragHandler draggedItemUI = dragHandler as UICharacterItemDragHandler;
            if (draggedItemUI != null)
            {
                switch (draggedItemUI.sourceLocation)
                {
                    case UICharacterItemDragHandler.SourceLocation.EquipItems:
                        break;
                    case UICharacterItemDragHandler.SourceLocation.NonEquipItems:
                        // If dropped non equipped equipment item to equip slot, equip it
                        EquipItem(draggedItemUI);
                        break;
                }
            }
        }

        private void EquipItem(UICharacterItemDragHandler draggedItemUI)
        {
            // Don't equip the item if drop area is not setup as equip slot UI
            if (!uiCharacterItem.IsSetupAsEquipSlot)
                return;

            // Get owing character
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
                return;

            // Detect type of equipping slot and validate
            Item armorItem = draggedItemUI.uiCharacterItem.CharacterItem.GetArmorItem();
            Item weaponItem = draggedItemUI.uiCharacterItem.CharacterItem.GetWeaponItem();
            Item shieldItem = draggedItemUI.uiCharacterItem.CharacterItem.GetShieldItem();
            switch (uiCharacterItem.InventoryType)
            {
                case InventoryType.EquipItems:
                    if (armorItem == null ||
                        !armorItem.EquipPosition.Equals(uiCharacterItem.EquipPosition))
                    {
                        // Check if it's correct equip position or not
                        BaseGameNetworkManager.Singleton.ClientReceiveGameMessage(new GameMessage()
                        {
                            type = GameMessage.Type.CannotEquip
                        });
                        return;
                    }
                    break;
                case InventoryType.EquipWeaponRight:
                case InventoryType.EquipWeaponLeft:
                    if (weaponItem == null &&
                        shieldItem == null)
                    {
                        // Check if it's correct equip position or not
                        BaseGameNetworkManager.Singleton.ClientReceiveGameMessage(new GameMessage()
                        {
                            type = GameMessage.Type.CannotEquip
                        });
                        return;
                    }
                    break;
            }
            // Can equip the item
            // so tell the server that this client want to equip the item
            owningCharacter.RequestEquipItem(
                (short)draggedItemUI.uiCharacterItem.IndexOfData,
                uiCharacterItem.InventoryType,
                uiCharacterItem.EquipSlotIndex);
        }
    }
}
