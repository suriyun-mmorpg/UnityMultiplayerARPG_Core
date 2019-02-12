using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public class UICharacterHotkeyDropHandler : MonoBehaviour, IDropHandler
    {
        public UICharacterHotkey uiCharacterHotkey;

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
            if (uiCharacterHotkey == null)
                uiCharacterHotkey = GetComponent<UICharacterHotkey>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (uiCharacterHotkey == null)
            {
                Debug.LogWarning("[UICharacterHotkeyDropHandler] `uiCharacterHotkey` is empty");
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
                    // If dragged item UI
                    UICharacterItemDragHandler draggedItemUI = dragHandler as UICharacterItemDragHandler;
                    if (draggedItemUI != null)
                    {
                        if (uiCharacterHotkey.CanAssignCharacterItem(draggedItemUI.CacheUI.Data.characterItem))
                        {
                            // Assign item to hotkey
                            owningCharacter.RequestAssignHotkey(uiCharacterHotkey.Data.hotkeyId, HotkeyType.Item, draggedItemUI.CacheUI.Data.characterItem.dataId);
                        }
                    }
                    // If dragged skill UI
                    UICharacterSkillDragHandler draggedSkillUI = dragHandler as UICharacterSkillDragHandler;
                    if (draggedSkillUI != null)
                    {
                        if (uiCharacterHotkey.CanAssignCharacterSkill(draggedSkillUI.CacheUI.Data.characterSkill))
                        {
                            // Assign item to hotkey
                            owningCharacter.RequestAssignHotkey(uiCharacterHotkey.Data.hotkeyId, HotkeyType.Skill, draggedSkillUI.CacheUI.Data.characterSkill.dataId);
                        }
                    }
                }
            }
        }
    }
}
