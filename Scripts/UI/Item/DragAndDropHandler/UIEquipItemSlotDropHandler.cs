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
            if (RectTransformUtility.RectangleContainsScreenPoint(DropRect, Input.mousePosition))
            {
                UIDragHandler dragHandler = eventData.pointerDrag.GetComponent<UIDragHandler>();
                if (dragHandler != null && !dragHandler.isDropped)
                {
                    dragHandler.isDropped = true;
                    // Get owing character
                    BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
                    if (owningCharacter == null)
                        return;
                    UICharacterItemDragHandler draggedItemUI = dragHandler as UICharacterItemDragHandler;
                    if (draggedItemUI != null)
                    {
                        switch (draggedItemUI.sourceLocation)
                        {
                            case UICharacterItemDragHandler.SourceLocation.EquipItems:
                                break;
                            case UICharacterItemDragHandler.SourceLocation.NonEquipItems:
                                // If dropped non equip item to equip slot, equip it
                                owningCharacter.RequestEquipItem((short)draggedItemUI.uiCharacterItem.IndexOfData, (byte)uiCharacterItem.InventoryType, (short)uiCharacterItem.IndexOfData);
                                break;
                        }
                    }
                }
            }
        }
    }
}
