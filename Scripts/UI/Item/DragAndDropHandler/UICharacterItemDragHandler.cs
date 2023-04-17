using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public partial class UICharacterItemDragHandler : UIDragHandler
    {
        public enum SourceLocation : byte
        {
            NonEquipItems,
            EquipItems,
            StorageItems,
            ItemsContainer,
            Hotkey,
            Unknow = 254,
        }

        [Tooltip("If this is `TRUE`, it have to be dropped on drop handler to proceed activities")]
        public bool requireDropArea = false;
        public bool enableDropItemAction = true;
        public bool enableUnequipItemAction = true;
        public bool enableMoveFromStorageAction = true;
        public bool enablePickupFromContainerAction = true;
        public bool enableUnassignHotkeyAction = true;

        public SourceLocation Location { get; protected set; }
        public UICharacterItem UIItem { get; protected set; }
        public UICharacterHotkey UIHotkey { get; protected set; }

        protected UICharacterItem _cacheUI;
        public UICharacterItem CacheUI
        {
            get
            {
                if (_cacheUI == null)
                    _cacheUI = GetComponent<UICharacterItem>();
                return _cacheUI;
            }
        }

        public override bool CanDrag
        {
            get
            {
                switch (Location)
                {
                    case SourceLocation.NonEquipItems:
                    case SourceLocation.EquipItems:
                    case SourceLocation.StorageItems:
                    case SourceLocation.ItemsContainer:
                        return UIItem != null && UIItem.IndexOfData >= 0 && UIItem.CharacterItem.NotEmptySlot();
                    case SourceLocation.Hotkey:
                        return UIHotkey != null;
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
            Location = SourceLocation.EquipItems;
            UIItem = uiCharacterItem;
        }

        public void SetupForNonEquipItems(UICharacterItem uiCharacterItem)
        {
            Location = SourceLocation.NonEquipItems;
            UIItem = uiCharacterItem;
        }

        public void SetupForStorageItems(UICharacterItem uiCharacterItem)
        {
            Location = SourceLocation.StorageItems;
            UIItem = uiCharacterItem;
        }

        public void SetupForItemsContainer(UICharacterItem uiCharacterItem)
        {
            Location = SourceLocation.ItemsContainer;
            UIItem = uiCharacterItem;
        }

        public void SetupForHotkey(UICharacterHotkey uiCharacterHotkey)
        {
            Location = SourceLocation.Hotkey;
            UIHotkey = uiCharacterHotkey;
        }

        public void SetupForUnknow(UICharacterItem uiCharacterItem)
        {
            Location = SourceLocation.Unknow;
            UIItem = uiCharacterItem;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (IsScrolling)
            {
                base.OnEndDrag(eventData);
                return;
            }
            base.OnEndDrag(eventData);
            if (IsDropped || !CanDrag)
                return;
            if (requireDropArea)
                return;
            if (enableDropItemAction && Location == SourceLocation.NonEquipItems && (!EventSystem.current.IsPointerOverGameObject() || EventSystem.current.currentSelectedGameObject.GetComponent<IMobileInputArea>() != null))
                UIItem.OnClickDrop();
            if (enableUnequipItemAction && Location == SourceLocation.EquipItems && EventSystem.current.IsPointerOverGameObject())
                UIItem.OnClickUnEquip();
            if (enableMoveFromStorageAction && Location == SourceLocation.StorageItems)
                UIItem.OnClickMoveFromStorage();
            if (enablePickupFromContainerAction && Location == SourceLocation.ItemsContainer)
                UIItem.OnClickPickUpFromContainer();
            if (enableUnassignHotkeyAction && Location == SourceLocation.Hotkey)
                GameInstance.PlayingCharacterEntity.UnAssignHotkey(UIHotkey.hotkeyId);
        }
    }
}
