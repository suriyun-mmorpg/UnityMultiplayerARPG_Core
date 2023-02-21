using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterItemDropHandler : MonoBehaviour, IDropHandler
    {
        public UICharacterItem uiCharacterItem;
        public bool doNotUnEquipItem;
        public bool doNotSwapOrMergeItem;
        public bool doNotEquipItem;
        public bool doNotMoveToStorage;
        public bool doNotMoveFromStorage;
        public bool doNotSwapOrMergeStorageItem;
        [Tooltip("If this is `TRUE`, it will not swap or merge item which dragging from inventory to storage")]
        public bool doNotSwapOrMergeWithStorageItem;
        [Tooltip("If this is `TRUE`, it will not swap or merge item which dragging from storage to inventory")]
        [FormerlySerializedAs("doNotSwapOrMergeWithNonEquipItem")]
        public bool doNotSwapOrMergeWithInventoryItem;

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
            if (!RectTransformUtility.RectangleContainsScreenPoint(DropRect, InputManager.MousePosition()))
                return;
            // Validate dragging UI
            UIDragHandler dragHandler = eventData.pointerDrag.GetComponent<UIDragHandler>();
            if (dragHandler == null || !dragHandler.CanDrop)
                return;
            // Get dragged item UI. If dragging item UI is UI for character item, equip the item
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
            draggedItemUI.IsDropped = true;
            switch (uiCharacterItem.InventoryType)
            {
                case InventoryType.NonEquipItems:
                    if (doNotUnEquipItem)
                        return;
                    // Unequip item
                    GameInstance.ClientInventoryHandlers.RequestUnEquipItem(
                        draggedItemUI.uiCharacterItem.InventoryType,
                        draggedItemUI.uiCharacterItem.IndexOfData,
                        draggedItemUI.uiCharacterItem.EquipSlotIndex,
                        uiCharacterItem.IndexOfData,
                        ClientInventoryActions.ResponseUnEquipArmor,
                        ClientInventoryActions.ResponseUnEquipWeapon);
                    break;
                case InventoryType.StorageItems:
                    if (doNotMoveToStorage)
                        return;
                    // Drop non equip item to storage item
                    if (doNotSwapOrMergeWithStorageItem)
                    {
                        draggedItemUI.uiCharacterItem.OnClickMoveToStorage(-1);
                    }
                    else
                    {
                        draggedItemUI.uiCharacterItem.OnClickMoveToStorage(uiCharacterItem.IndexOfData);
                    }
                    break;
            }
        }

        protected virtual void OnDropNonEquipItem(UICharacterItemDragHandler draggedItemUI)
        {
            // Set UI drop state
            draggedItemUI.IsDropped = true;
            StorageType storageType = GameInstance.OpenedStorageType;
            string storageOwnerId = GameInstance.OpenedStorageOwnerId;
            switch (uiCharacterItem.InventoryType)
            {
                case InventoryType.NonEquipItems:
                    if (doNotSwapOrMergeItem)
                        return;
                    // Drop non equip item to non equip item
                    GameInstance.ClientInventoryHandlers.RequestSwapOrMergeItem(new RequestSwapOrMergeItemMessage()
                    {
                        fromIndex = draggedItemUI.uiCharacterItem.IndexOfData,
                        toIndex = uiCharacterItem.IndexOfData,
                    }, ClientInventoryActions.ResponseSwapOrMergeItem);
                    break;
                case InventoryType.EquipItems:
                case InventoryType.EquipWeaponRight:
                case InventoryType.EquipWeaponLeft:
                    if (doNotEquipItem)
                        return;
                    // Drop non equip item to equip item
                    EquipItem(draggedItemUI);
                    break;
                case InventoryType.StorageItems:
                    if (doNotMoveToStorage)
                        return;
                    // Drop non equip item to storage item
                    if (doNotSwapOrMergeWithStorageItem)
                    {
                        draggedItemUI.uiCharacterItem.OnClickMoveToStorage(-1);
                    }
                    else
                    {
                        draggedItemUI.uiCharacterItem.OnClickMoveToStorage(uiCharacterItem.IndexOfData);
                    }
                    break;
            }
        }

        protected void OnDropStorageItem(UICharacterItemDragHandler draggedItemUI)
        {
            // Set UI drop state
            draggedItemUI.IsDropped = true;
            StorageType storageType = GameInstance.OpenedStorageType;
            string storageOwnerId = GameInstance.OpenedStorageOwnerId;
            switch (uiCharacterItem.InventoryType)
            {
                case InventoryType.NonEquipItems:
                case InventoryType.EquipItems:
                case InventoryType.EquipWeaponRight:
                case InventoryType.EquipWeaponLeft:
                    if (doNotMoveFromStorage)
                        return;
                    // Drop storage item to non equip item
                    if (doNotSwapOrMergeWithInventoryItem)
                    {
                        draggedItemUI.uiCharacterItem.OnClickMoveFromStorage(uiCharacterItem.InventoryType, uiCharacterItem.EquipSlotIndex, -1);
                    }
                    else
                    {
                        draggedItemUI.uiCharacterItem.OnClickMoveFromStorage(uiCharacterItem.InventoryType, uiCharacterItem.EquipSlotIndex, uiCharacterItem.IndexOfData);
                    }
                    break;
                case InventoryType.StorageItems:
                    if (doNotSwapOrMergeStorageItem)
                        return;
                    // Drop storage item to storage item
                    GameInstance.ClientStorageHandlers.RequestSwapOrMergeStorageItem(new RequestSwapOrMergeStorageItemMessage()
                    {
                        storageType = storageType,
                        storageOwnerId = storageOwnerId,
                        fromIndex = draggedItemUI.uiCharacterItem.IndexOfData,
                        toIndex = uiCharacterItem.IndexOfData
                    }, ClientStorageActions.ResponseSwapOrMergeStorageItem);
                    break;
            }
        }

        protected void EquipItem(UICharacterItemDragHandler draggedItemUI)
        {
            // Don't equip the item if drop area is not setup as equip slot UI
            if (!uiCharacterItem.IsSetupAsEquipSlot)
                return;

            // Detect type of equipping slot and validate
            IArmorItem armorItem = draggedItemUI.uiCharacterItem.CharacterItem.GetArmorItem();
            IWeaponItem weaponItem = draggedItemUI.uiCharacterItem.CharacterItem.GetWeaponItem();
            IShieldItem shieldItem = draggedItemUI.uiCharacterItem.CharacterItem.GetShieldItem();
            switch (uiCharacterItem.InventoryType)
            {
                case InventoryType.EquipItems:
                    if (armorItem == null ||
                        !armorItem.GetEquipPosition().Equals(uiCharacterItem.EquipPosition))
                    {
                        // Check if it's correct equip position or not
                        ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CANNOT_EQUIP);
                        return;
                    }
                    break;
                case InventoryType.EquipWeaponRight:
                case InventoryType.EquipWeaponLeft:
                    if (weaponItem == null &&
                        shieldItem == null)
                    {
                        // Check if it's correct equip position or not
                        ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CANNOT_EQUIP);
                        return;
                    }
                    break;
            }
            // Can equip the item
            // so tell the server that this client want to equip the item
            GameInstance.ClientInventoryHandlers.RequestEquipItem(
                draggedItemUI.uiCharacterItem.IndexOfData,
                uiCharacterItem.InventoryType,
                uiCharacterItem.EquipSlotIndex,
                ClientInventoryActions.ResponseEquipArmor,
                ClientInventoryActions.ResponseEquipWeapon);
        }
    }
}
