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
            // Validate drop position
            if (!RectTransformUtility.RectangleContainsScreenPoint(DropRect, Input.mousePosition))
                return;
            // Validate dragging UI
            UIDragHandler dragHandler = eventData.pointerDrag.GetComponent<UIDragHandler>();
            if (dragHandler == null || dragHandler.isDropped)
                return;
            // Set UI drop state
            dragHandler.isDropped = true;

            string swappingHotkeyId = string.Empty;
            HotkeyType swappingType = HotkeyType.None;
            string swappingDataId = string.Empty;
            // If dragged item UI
            UICharacterItemDragHandler draggedItemUI = dragHandler as UICharacterItemDragHandler;
            if (draggedItemUI != null)
            {
                if (draggedItemUI.sourceLocation == UICharacterItemDragHandler.SourceLocation.Hotkey)
                {
                    swappingHotkeyId = draggedItemUI.uiCharacterHotkey.Data.hotkeyId;
                    swappingType = uiCharacterHotkey.Data.type;
                    swappingDataId = uiCharacterHotkey.Data.relateId;
                }

                if (uiCharacterHotkey.CanAssignCharacterItem(draggedItemUI.CacheUI.Data.characterItem))
                {
                    // Assign item to hotkey
                    BasePlayerCharacterController.OwningCharacter.RequestAssignHotkey(uiCharacterHotkey.Data.hotkeyId, HotkeyType.Item, draggedItemUI.CacheUI.Data.characterItem.id);
                }

                if (draggedItemUI.sourceLocation == UICharacterItemDragHandler.SourceLocation.Hotkey)
                {
                    // Swap key
                    BasePlayerCharacterController.OwningCharacter.RequestAssignHotkey(swappingHotkeyId, swappingType, swappingDataId);
                }
            }
            // If dragged skill UI
            UICharacterSkillDragHandler draggedSkillUI = dragHandler as UICharacterSkillDragHandler;
            if (draggedSkillUI != null)
            {
                if (draggedSkillUI.sourceLocation == UICharacterSkillDragHandler.SourceLocation.Hotkey)
                {
                    swappingHotkeyId = draggedSkillUI.uiCharacterHotkey.Data.hotkeyId;
                    swappingType = uiCharacterHotkey.Data.type;
                    swappingDataId = uiCharacterHotkey.Data.relateId;
                }

                if (uiCharacterHotkey.CanAssignCharacterSkill(draggedSkillUI.CacheUI.Data.characterSkill))
                {
                    // Assign item to hotkey
                    BasePlayerCharacterController.OwningCharacter.RequestAssignHotkey(uiCharacterHotkey.Data.hotkeyId, HotkeyType.Skill, draggedSkillUI.CacheUI.Skill.Id);
                }

                if (draggedSkillUI.sourceLocation == UICharacterSkillDragHandler.SourceLocation.Hotkey)
                {
                    // Swap key
                    BasePlayerCharacterController.OwningCharacter.RequestAssignHotkey(swappingHotkeyId, swappingType, swappingDataId);
                }
            }
        }
    }
}
