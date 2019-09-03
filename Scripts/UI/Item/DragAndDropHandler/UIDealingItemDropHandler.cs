using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public class UIDealingItemDropHandler : MonoBehaviour, IDropHandler
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
            // Set UI drop state
            dragHandler.isDropped = true;
            // If dragged item UI
            UICharacterItemDragHandler draggedItemUI = dragHandler as UICharacterItemDragHandler;
            if (draggedItemUI != null)
            {
                switch (draggedItemUI.sourceLocation)
                {
                    case UICharacterItemDragHandler.SourceLocation.NonEquipItems:
                    case UICharacterItemDragHandler.SourceLocation.EquipItems:
                        draggedItemUI.uiCharacterItem.OnClickSetDealingItem();
                        break;
                }
            }
        }
    }
}
