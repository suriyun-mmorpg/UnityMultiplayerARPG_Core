using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public partial class UICharacterHotkeyDropHandler : MonoBehaviour, IDropHandler
    {
        public UICharacterHotkey uiCharacterHotkey;

        protected RectTransform _dropRect;
        public RectTransform DropRect
        {
            get
            {
                if (_dropRect == null)
                    _dropRect = transform as RectTransform;
                return _dropRect;
            }
        }

        protected virtual void Start()
        {
            if (uiCharacterHotkey == null)
                uiCharacterHotkey = GetComponent<UICharacterHotkey>();
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            if (uiCharacterHotkey == null)
            {
                Debug.LogWarning("[UICharacterHotkeyDropHandler] `uiCharacterHotkey` is empty");
                return;
            }
            // Validate drop position
            if (!RectTransformUtility.RectangleContainsScreenPoint(DropRect, eventData.position))
                return;
            // Validate dragging UI
            UIDragHandler dragHandler = eventData.pointerDrag.GetComponent<UIDragHandler>();
            if (dragHandler == null || !dragHandler.CanDrop)
                return;
            // Set UI drop state
            dragHandler.IsDropped = true;
            string swappingHotkeyId = string.Empty;
            HotkeyType swappingType = HotkeyType.None;
            string swappingDataId = string.Empty;
            // If dragged item UI
            UICharacterItemDragHandler draggedItemUI = dragHandler as UICharacterItemDragHandler;
            if (draggedItemUI != null)
            {
                if (draggedItemUI.Location == UICharacterItemDragHandler.SourceLocation.Hotkey)
                {
                    swappingHotkeyId = draggedItemUI.UIHotkey.Data.hotkeyId;
                    swappingType = uiCharacterHotkey.Data.type;
                    swappingDataId = uiCharacterHotkey.Data.relateId;
                }

                if (uiCharacterHotkey.CanAssignCharacterItem(draggedItemUI.CacheUI.Data.characterItem))
                {
                    // Assign item to hotkey
                    GameInstance.PlayingCharacterEntity.AssignItemHotkey(uiCharacterHotkey.Data.hotkeyId, draggedItemUI.CacheUI.CharacterItem);
                }

                if (draggedItemUI.Location == UICharacterItemDragHandler.SourceLocation.Hotkey)
                {
                    // Swap key
                    GameInstance.PlayingCharacterEntity.CallServerAssignHotkey(swappingHotkeyId, swappingType, swappingDataId);
                }
            }
            // If dragged skill UI
            UICharacterSkillDragHandler draggedSkillUI = dragHandler as UICharacterSkillDragHandler;
            if (draggedSkillUI != null)
            {
                if (draggedSkillUI.Location == UICharacterSkillDragHandler.SourceLocation.Hotkey)
                {
                    swappingHotkeyId = draggedSkillUI.UIHotkey.Data.hotkeyId;
                    swappingType = uiCharacterHotkey.Data.type;
                    swappingDataId = uiCharacterHotkey.Data.relateId;
                }

                if (uiCharacterHotkey.CanAssignCharacterSkill(draggedSkillUI.CacheUI.Data.characterSkill))
                {
                    // Assign item to hotkey
                    GameInstance.PlayingCharacterEntity.AssignSkillHotkey(uiCharacterHotkey.Data.hotkeyId, draggedSkillUI.CacheUI.CharacterSkill);
                }

                if (draggedSkillUI.Location == UICharacterSkillDragHandler.SourceLocation.Hotkey)
                {
                    // Swap key
                    GameInstance.PlayingCharacterEntity.CallServerAssignHotkey(swappingHotkeyId, swappingType, swappingDataId);
                }
            }
        }
    }
}
