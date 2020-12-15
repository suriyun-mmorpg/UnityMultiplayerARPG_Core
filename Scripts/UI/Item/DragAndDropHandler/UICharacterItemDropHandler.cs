using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public partial class UICharacterItemDropHandler : MonoBehaviour, IDropHandler
    {
        public UICharacterItem uiCharacterItem;

        protected RectTransform dropRect;
        public RectTransform DropRect
        {
            get
            {
                if (dropRect == null)
                    dropRect = transform as RectTransform;
                return dropRect;
            }
        }

        protected virtual void Start()
        {
            if (uiCharacterItem == null)
                uiCharacterItem = GetComponent<UICharacterItem>();
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            if (uiCharacterItem == null)
            {
                Debug.LogWarning("[UICharacterItemDropHandler] `uicharacterItem` is empty");
                return;
            }
            // Validate drop position
            if (!RectTransformUtility.RectangleContainsScreenPoint(DropRect, Input.mousePosition))
                return;
            // Validate dragging UI
            UIDragHandler dragHandler = eventData.pointerDrag.GetComponent<UIDragHandler>();
            if (dragHandler == null || dragHandler.isDropped)
                return;
            // Get dragged item UI, if dragging item UI is UI for character item.
            // try to equip the item
            UICharacterItemDragHandler draggedItemUI = dragHandler as UICharacterItemDragHandler;
            if (draggedItemUI != null && draggedItemUI.uiCharacterItem != uiCharacterItem)
            {
                switch (draggedItemUI.sourceLocation)
                {
                    case UICharacterItemDragHandler.SourceLocation.EquipItems:
                        OnDropEquipItem(draggedItemUI);
                        break;
                    case UICharacterItemDragHandler.SourceLocation.NonEquipItems:
                        OnDropNonEquipItem(draggedItemUI);
                        break;
                    case UICharacterItemDragHandler.SourceLocation.StorageItems:
                        OnDropStorageItem(draggedItemUI);
                        break;
                }
            }
        }

        protected virtual void OnDropEquipItem(UICharacterItemDragHandler draggedItemUI)
        {
            // Set UI drop state
            draggedItemUI.isDropped = true;
            string characterId = BasePlayerCharacterController.OwningCharacter.Id;
            switch (uiCharacterItem.InventoryType)
            {
                case InventoryType.NonEquipItems:
                    // Unequip item
                    GameInstance.ClientInventoryHandlers.RequestUnEquipItem(
                        characterId,
                        draggedItemUI.uiCharacterItem.InventoryType,
                        (short)draggedItemUI.uiCharacterItem.IndexOfData,
                        draggedItemUI.uiCharacterItem.EquipSlotIndex,
                        (short)uiCharacterItem.IndexOfData,
                        UIInventoryResponses.ResponseUnEquipArmor,
                        UIInventoryResponses.ResponseUnEquipWeapon);
                    break;
            }
        }

        protected virtual void OnDropNonEquipItem(UICharacterItemDragHandler draggedItemUI)
        {
            // Set UI drop state
            draggedItemUI.isDropped = true;
            string characterId = BasePlayerCharacterController.OwningCharacter.Id;
            StorageType storageType = BasePlayerCharacterController.OwningCurrentStorageType;
            string storageOwnerId = BasePlayerCharacterController.OwningCurrentStorageOwnerId;
            switch (uiCharacterItem.InventoryType)
            {
                case InventoryType.NonEquipItems:
                    // Drop non equip item to non equip item
                    GameInstance.ClientInventoryHandlers.RequestSwapOrMergeItem(new RequestSwapOrMergeItemMessage()
                    {
                        characterId = characterId,
                        fromIndex = (short)draggedItemUI.uiCharacterItem.IndexOfData,
                        toIndex = (short)uiCharacterItem.IndexOfData,
                    }, UIInventoryResponses.ResponseSwapOrMergeItem);
                    break;
                case InventoryType.EquipItems:
                case InventoryType.EquipWeaponRight:
                case InventoryType.EquipWeaponLeft:
                    // Drop non equip item to equip item
                    EquipItem(draggedItemUI);
                    break;
                case InventoryType.StorageItems:
                    // Drop non equip item to storage item
                    GameInstance.ClientStorageHandlers.RequestMoveItemToStorage(new RequestMoveItemToStorageMessage()
                    {
                        characterId = characterId,
                        storageType = storageType,
                        storageOwnerId = storageOwnerId,
                        inventoryIndex = (short)draggedItemUI.uiCharacterItem.IndexOfData,
                        amount = draggedItemUI.uiCharacterItem.CharacterItem.amount,
                        storageItemIndex = (short)uiCharacterItem.IndexOfData
                    }, ClientStorageActions.ResponseMoveItemToStorage);
                    break;
            }
        }

        protected void OnDropStorageItem(UICharacterItemDragHandler draggedItemUI)
        {
            // Set UI drop state
            draggedItemUI.isDropped = true;
            string characterId = BasePlayerCharacterController.OwningCharacter.Id;
            StorageType storageType = BasePlayerCharacterController.OwningCurrentStorageType;
            string storageOwnerId = BasePlayerCharacterController.OwningCurrentStorageOwnerId;
            switch (uiCharacterItem.InventoryType)
            {
                case InventoryType.NonEquipItems:
                    // Drop storage item to non equip item
                    GameInstance.ClientStorageHandlers.RequestMoveItemFromStorage(new RequestMoveItemFromStorageMessage()
                    {
                        characterId = characterId,
                        storageType = storageType,
                        storageOwnerId = storageOwnerId,
                        storageItemIndex = (short)draggedItemUI.uiCharacterItem.IndexOfData,
                        amount = draggedItemUI.uiCharacterItem.CharacterItem.amount,
                        inventoryIndex = (short)uiCharacterItem.IndexOfData
                    }, ClientStorageActions.ResponseMoveItemFromStorage);
                    break;
                case InventoryType.StorageItems:
                    // Drop storage item to storage item
                    GameInstance.ClientStorageHandlers.RequestSwapOrMergeStorageItem(new RequestSwapOrMergeStorageItemMessage()
                    {
                        characterId = characterId,
                        storageType = storageType,
                        storageOwnerId = storageOwnerId,
                        fromIndex = (short)draggedItemUI.uiCharacterItem.IndexOfData,
                        toIndex = (short)uiCharacterItem.IndexOfData
                    }, ClientStorageActions.ResponseSwapOrMergeStorageItem);
                    break;
            }
        }

        protected void EquipItem(UICharacterItemDragHandler draggedItemUI)
        {
            // Don't equip the item if drop area is not setup as equip slot UI
            if (!uiCharacterItem.IsSetupAsEquipSlot)
                return;

            // Get owing character
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
                return;

            // Detect type of equipping slot and validate
            IArmorItem armorItem = draggedItemUI.uiCharacterItem.CharacterItem.GetArmorItem();
            IWeaponItem weaponItem = draggedItemUI.uiCharacterItem.CharacterItem.GetWeaponItem();
            IShieldItem shieldItem = draggedItemUI.uiCharacterItem.CharacterItem.GetShieldItem();
            switch (uiCharacterItem.InventoryType)
            {
                case InventoryType.EquipItems:
                    if (armorItem == null ||
                        !armorItem.EquipPosition.Equals(uiCharacterItem.EquipPosition))
                    {
                        // Check if it's correct equip position or not
                        ClientGenericActions.ClientReceiveGameMessage(new GameMessage()
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
                        ClientGenericActions.ClientReceiveGameMessage(new GameMessage()
                        {
                            type = GameMessage.Type.CannotEquip
                        });
                        return;
                    }
                    break;
            }
            // Can equip the item
            // so tell the server that this client want to equip the item
            GameInstance.ClientInventoryHandlers.RequestEquipItem(
                owningCharacter.Id,
                (short)draggedItemUI.uiCharacterItem.IndexOfData,
                uiCharacterItem.InventoryType,
                uiCharacterItem.EquipSlotIndex,
                UIInventoryResponses.ResponseEquipArmor,
                UIInventoryResponses.ResponseEquipWeapon);
        }
    }
}
