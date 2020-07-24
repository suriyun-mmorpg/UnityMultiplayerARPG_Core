using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public class DropItemDropHandler : MonoBehaviour, IDropHandler
    {
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

        public void OnDrop(PointerEventData eventData)
        {
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
            if (draggedItemUI != null)
            {
                switch (draggedItemUI.sourceLocation)
                {
                    case UICharacterItemDragHandler.SourceLocation.EquipItems:
                        break;
                    case UICharacterItemDragHandler.SourceLocation.NonEquipItems:
                        draggedItemUI.uiCharacterItem.OnClickDrop();
                        break;
                    case UICharacterItemDragHandler.SourceLocation.StorageItems:
                        break;
                }
            }
        }
    }
}
