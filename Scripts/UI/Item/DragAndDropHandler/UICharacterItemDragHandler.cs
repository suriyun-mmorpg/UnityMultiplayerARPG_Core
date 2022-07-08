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

        public SourceLocation sourceLocation { get; protected set; }
        // Non Equip / Equip items data
        public UICharacterItem uiCharacterItem { get; protected set; }
        // Hotkey data
        public UICharacterHotkey uiCharacterHotkey { get; protected set; }

        protected UICharacterItem cacheUI;
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
                    case SourceLocation.ItemsContainer:
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

        public void SetupForItemsContainer(UICharacterItem uiCharacterItem)
        {
            sourceLocation = SourceLocation.ItemsContainer;
            this.uiCharacterItem = uiCharacterItem;
        }

        public void SetupForHotkey(UICharacterHotkey uiCharacterHotkey)
        {
            sourceLocation = SourceLocation.Hotkey;
            this.uiCharacterHotkey = uiCharacterHotkey;
        }

        public void SetupForUnknow(UICharacterItem uiCharacterItem)
        {
            sourceLocation = SourceLocation.Unknow;
            this.uiCharacterItem = uiCharacterItem;
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
            if (enableDropItemAction && sourceLocation == SourceLocation.NonEquipItems && (!EventSystem.current.IsPointerOverGameObject() || EventSystem.current.currentSelectedGameObject.GetComponent<IMobileInputArea>() != null))
                uiCharacterItem.OnClickDrop();
            if (enableUnequipItemAction && sourceLocation == SourceLocation.EquipItems && EventSystem.current.IsPointerOverGameObject())
                uiCharacterItem.OnClickUnEquip();
            if (enableMoveFromStorageAction && sourceLocation == SourceLocation.StorageItems)
                uiCharacterItem.OnClickMoveFromStorage();
            if (enablePickupFromContainerAction && sourceLocation == SourceLocation.ItemsContainer)
                uiCharacterItem.OnClickPickUpFromContainer();
            if (enableUnassignHotkeyAction && sourceLocation == SourceLocation.Hotkey)
                GameInstance.PlayingCharacterEntity.UnAssignHotkey(uiCharacterHotkey.hotkeyId);
        }
    }
}
