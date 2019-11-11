using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public class UICharacterItemDragHandler : UIDragHandler
    {
        public enum SourceLocation
        {
            NonEquipItems,
            EquipItems,
            StorageItems,
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
                    case SourceLocation.StorageItems:
                        return uiCharacterItem != null && uiCharacterItem.IndexOfData >= 0 && uiCharacterItem.CharacterItem.NotEmptySlot();
                    case SourceLocation.Hotkey:
                        return uiCharacterHotkey != null;
                }
                return false;
            }
        }

        protected override void Start()
        {
            base.Start();
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

        public void SetupForStorageItems(UICharacterItem uiCharacterItem)
        {
            sourceLocation = SourceLocation.StorageItems;
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
            if (sourceLocation == SourceLocation.NonEquipItems && !EventSystem.current.IsPointerOverGameObject())
                uiCharacterItem.OnClickDrop();
            if (sourceLocation == SourceLocation.EquipItems && EventSystem.current.IsPointerOverGameObject())
                uiCharacterItem.OnClickUnEquip();
            if (sourceLocation == SourceLocation.StorageItems)
                uiCharacterItem.OnClickMoveFromStorage();
            if (sourceLocation == SourceLocation.Hotkey)
                BasePlayerCharacterController.OwningCharacter.RequestAssignHotkey(uiCharacterHotkey.hotkeyId, HotkeyType.None, string.Empty);
        }
    }
}
