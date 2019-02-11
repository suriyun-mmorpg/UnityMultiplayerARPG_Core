using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public class UICharacterItemDragHandler : UIDragHandler
    {
        public enum SourceLocation
        {
            NonEquipItems,
            EquipItems,
            Hotkey,
        }

        public SourceLocation sourceLocation;
        // Non Equip / Equip items data
        public UICharacterItem uiCharacterItem;
        // Hotkey data
        public UICharacterHotkey uiCharacterHotkey;

        private UICharacterItem cacheUI;
        public UICharacterItem CacheUI
        {
            get
            {
                if (cacheUI == null)
                    cacheUI = GetComponent<UICharacterItem>();
                return cacheUI;
            }
        }

        private void Start()
        {
            rootTransform = CacheUI.CacheRoot.transform;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
                return;
            if (sourceLocation == SourceLocation.EquipItems)
                owningCharacter.RequestUnEquipItem((byte)uiCharacterItem.InventoryType, (short)uiCharacterItem.indexOfData);
            if (sourceLocation == SourceLocation.Hotkey)
                owningCharacter.RequestAssignHotkey(uiCharacterHotkey.hotkeyId, HotkeyType.None, 0);
        }
    }
}
