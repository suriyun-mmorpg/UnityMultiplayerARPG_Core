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

        public SourceLocation sourceLocation { get; private set; }
        // Non Equip / Equip items data
        public UICharacterItem uiCharacterItem { get; private set; }
        // Hotkey data
        public UICharacterHotkey uiCharacterHotkey { get; private set; }

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

        public override bool CanDrag
        {
            get
            {
                switch (sourceLocation)
                {
                    case SourceLocation.NonEquipItems:
                    case SourceLocation.EquipItems:
                        return uiCharacterItem.IndexOfData >= 0;
                    case SourceLocation.Hotkey:
                        return true;
                }
                return false;
            }
        }

        private void Start()
        {
            rootTransform = CacheUI.CacheRoot.transform;
        }

        public void SetupForEquipItems(UICharacterItem uiCharacterItem)
        {
            sourceLocation = SourceLocation.EquipItems;
            this.uiCharacterItem = uiCharacterItem;
        }

        public void SetupForNonEquipItems(UICharacterItem uiCharacterItem)
        {
            sourceLocation = SourceLocation.NonEquipItems;
            this.uiCharacterItem = uiCharacterItem;
        }

        public void SetupForHotkey(UICharacterHotkey uiCharacterHotkey)
        {
            sourceLocation = SourceLocation.Hotkey;
            this.uiCharacterHotkey = uiCharacterHotkey;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            if (isDropped || !CanDrag)
                return;
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
                return;
            if (sourceLocation == SourceLocation.NonEquipItems)
                uiCharacterItem.OnClickDrop();
            if (sourceLocation == SourceLocation.EquipItems)
                uiCharacterItem.OnClickUnEquip();
            if (sourceLocation == SourceLocation.Hotkey)
                owningCharacter.RequestAssignHotkey(uiCharacterHotkey.hotkeyId, HotkeyType.None, 0);
        }
    }
}
