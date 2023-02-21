using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public partial class UIDealingItemDropHandler : MonoBehaviour, IDropHandler
    {
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

        public virtual void OnDrop(PointerEventData eventData)
        {
            // Validate drop position
            if (!RectTransformUtility.RectangleContainsScreenPoint(DropRect, InputManager.MousePosition()))
                return;
            // Validate dragging UI
            UIDragHandler dragHandler = eventData.pointerDrag.GetComponent<UIDragHandler>();
            if (dragHandler == null || !dragHandler.CanDrop)
                return;
            // Set UI drop state
            dragHandler.IsDropped = true;
            // Get dragged item UI
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
