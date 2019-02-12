using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public class UIRefineItemDropHandler : MonoBehaviour, IDropHandler
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
                            case UICharacterItemDragHandler.SourceLocation.NonEquipItems:
                            case UICharacterItemDragHandler.SourceLocation.EquipItems:
                                draggedItemUI.uiCharacterItem.OnClickSetRefineItem();
                                break;
                        }
                    }
                }
            }
        }
    }
}
