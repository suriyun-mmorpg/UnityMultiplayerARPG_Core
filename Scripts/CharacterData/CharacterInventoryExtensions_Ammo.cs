using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static partial class CharacterInventoryExtensions
    {
        public static int GetAmmo(this IPlayerCharacterData character, InventoryType inventoryType, int index)
        {
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    return GetAmmoNonEquipItem(character, index);
                case InventoryType.EquipItems:
                    return GetAmmoEquipItem(character, index);
                case InventoryType.EquipWeaponRight:
                    return GetAmmoRightHandItem(character);
                case InventoryType.EquipWeaponLeft:
                    return GetAmmoLeftHandItem(character);
            }
            return 0;
        }

        public static bool PutAmmoToItem(this IPlayerCharacterData character, InventoryType inventoryType, int index, string ammoItemId, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    return PutAmmoToNonEquipItem(character, index, ammoItemId, out gameMessage);
                case InventoryType.EquipItems:
                    return PutAmmoToEquipItem(character, index, ammoItemId, out gameMessage);
                case InventoryType.EquipWeaponRight:
                    return PutAmmoToRightHandItem(character, ammoItemId, out gameMessage);
                case InventoryType.EquipWeaponLeft:
                    return PutAmmoToLeftHandItem(character, ammoItemId, out gameMessage);
            }
            gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
            return false;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }

        public static bool RemoveAmmoFromItem(this IPlayerCharacterData character, InventoryType inventoryType, int index, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    return RemoveAmmoFromNonEquipItem(character, index, true, out gameMessage);
                case InventoryType.EquipItems:
                    return RemoveAmmoFromEquipItem(character, index, true, out gameMessage);
                case InventoryType.EquipWeaponRight:
                    return RemoveAmmoFromRightHandItem(character, true, out gameMessage);
                case InventoryType.EquipWeaponLeft:
                    return RemoveAmmoFromLeftHandItem(character, true, out gameMessage);
            }
            gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
            return false;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }

        public static int GetAmmoRightHandItem(IPlayerCharacterData character)
        {
            return character.EquipWeapons.rightHand.ammo;
        }

        public static int GetAmmoLeftHandItem(IPlayerCharacterData character)
        {
            return character.EquipWeapons.leftHand.ammo;
        }

        public static int GetAmmoEquipItem(IPlayerCharacterData character, int index)
        {
            return character.EquipItems[index].ammo;
        }

        public static int GetAmmoNonEquipItem(IPlayerCharacterData character, int index)
        {
            return character.NonEquipItems[index].ammo;
        }

        public static bool PutAmmoToRightHandItem(IPlayerCharacterData character, string ammoItemId, out UITextKeys gameMessage)
        {
            return PutAmmoToItem(character, character.EquipWeapons.rightHand, ammoItemId, (weaponItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = weaponItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessage);
        }

        public static bool PutAmmoToLeftHandItem(IPlayerCharacterData character, string ammoItemId, out UITextKeys gameMessage)
        {
            return PutAmmoToItem(character, character.EquipWeapons.leftHand, ammoItemId, (weaponItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = weaponItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessage);
        }

        public static bool PutAmmoToEquipItem(IPlayerCharacterData character, int index, string ammoItemId, out UITextKeys gameMessage)
        {
            return PutAmmoToItemByList(character, character.EquipItems, index, ammoItemId, out gameMessage);
        }

        public static bool PutAmmoToNonEquipItem(IPlayerCharacterData character, int index, string ammoItemId, out UITextKeys gameMessage)
        {
            return PutAmmoToItemByList(character, character.NonEquipItems, index, ammoItemId, out gameMessage);
        }

        private static bool PutAmmoToItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, string ammoItemId, out UITextKeys gameMessage)
        {
            return PutAmmoToItem(character, list[index], ammoItemId, (weaponItem) =>
            {
                list[index] = weaponItem;
            }, out gameMessage);
        }

        private static bool PutAmmoToItem(IPlayerCharacterData character, CharacterItem weaponItem, string ammoItemId, System.Action<CharacterItem> onPutAmmo, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.NONE;
            if (weaponItem.IsEmptySlot())
            {
                // Cannot change ammo because character item is empty
                gameMessage = UITextKeys.UI_ERROR_ITEM_NOT_FOUND;
                return false;
            }
            if (weaponItem.ammo > 0)
            {
                // Have to remove ammo before proceed
                gameMessage = UITextKeys.UI_ERROR_NOT_ALLOWED;
                return false;
            }
            IWeaponItem weaponItemData = weaponItem.GetWeaponItem();
            if (weaponItemData == null)
            {
                // Cannot change ammo because it's not weapon item
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
                return false;
            }
            int indexOfAmmo = character.IndexOfNonEquipItem(ammoItemId);
            if (indexOfAmmo < 0)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                return false;
            }
            CharacterItem ammoItem = character.NonEquipItems[indexOfAmmo];
            int ammoItemDataId = ammoItem.dataId;
            if (!GameInstance.Items.TryGetValue(ammoItemDataId, out BaseItem ammoItemData) || !ammoItemData.IsAmmo() || !weaponItemData.AmmoItemIds.Contains(ammoItemDataId))
            {
                // Cannot change ammo because ammo item id is invalid
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
                return false;
            }

            int ammo;
            if (!weaponItemData.NoAmmoCapacityOverriding && ammoItemData.OverrideAmmoCapacity > 0)
                ammo = Mathf.Min(ammoItem.amount, ammoItemData.OverrideAmmoCapacity);
            else
                ammo = Mathf.Min(ammoItem.amount, weaponItemData.AmmoCapacity);

            // Update weapon item, before make changes to other items
            if (weaponItemData.NoAmmoDataIdChange)
                ammoItemDataId = 0;
            weaponItem.ammoDataId = ammoItemDataId;
            weaponItem.ammo = ammo;
            onPutAmmo.Invoke(weaponItem);

            // Make changes to ammo in inventroy
            character.DecreaseItemsByIndex(indexOfAmmo, ammo, true);
            // Fill empty slots
            character.FillEmptySlots();
            return true;
        }

        public static bool RemoveAmmoFromRightHandItem(IPlayerCharacterData character, bool returnAmmo, out UITextKeys gameMessage)
        {
            return RemoveAmmoFromItem(character, character.EquipWeapons.rightHand, returnAmmo, (weaponSocketItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = weaponSocketItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessage);
        }

        public static bool RemoveAmmoFromLeftHandItem(IPlayerCharacterData character, bool returnAmmo, out UITextKeys gameMessage)
        {
            return RemoveAmmoFromItem(character, character.EquipWeapons.leftHand, returnAmmo, (weaponSocketItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = weaponSocketItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessage);
        }

        public static bool RemoveAmmoFromEquipItem(IPlayerCharacterData character, int index, bool returnAmmo, out UITextKeys gameMessage)
        {
            return RemoveAmmoFromItemByList(character, character.EquipItems, index, returnAmmo, out gameMessage);
        }

        public static bool RemoveAmmoFromNonEquipItem(IPlayerCharacterData character, int index, bool returnAmmo, out UITextKeys gameMessage)
        {
            return RemoveAmmoFromItemByList(character, character.NonEquipItems, index, returnAmmo, out gameMessage);
        }

        private static bool RemoveAmmoFromItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, bool returnAmmo, out UITextKeys gameMessage)
        {
            return RemoveAmmoFromItem(character, list[index], returnAmmo, (weaponSocketItem) =>
            {
                list[index] = weaponSocketItem;
            }, out gameMessage);
        }

        private static bool RemoveAmmoFromItem(IPlayerCharacterData character, CharacterItem weaponItem, bool returnAmmo, System.Action<CharacterItem> onRemoveAmmo, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.NONE;
            if (weaponItem.IsEmptySlot())
            {
                gameMessage = UITextKeys.UI_ERROR_ITEM_NOT_FOUND;
                return false;
            }
            int ammoDataId = weaponItem.ammoDataId;
            int ammoAmount = weaponItem.ammo;
            if (ammoAmount <= 0)
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ALLOWED;
                return false;
            }
            bool inventoryChanged = false;
            if (returnAmmo)
            {
                if (character.IncreasingItemsWillOverwhelming(ammoDataId, ammoAmount))
                {
                    gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                    return false;
                }
                // Update weapon item, before make changes to other items
                weaponItem.ammoDataId = 0;
                weaponItem.ammo = 0;
                onRemoveAmmo.Invoke(weaponItem);
                character.IncreaseItems(CharacterItem.Create(ammoDataId, 1, ammoAmount));
                inventoryChanged = true;
            }
            else
            {
                // Update weapon item, before make changes to other items
                weaponItem.ammoDataId = 0;
                weaponItem.ammo = 0;
                onRemoveAmmo.Invoke(weaponItem);
            }
            // Fill empty slots
            if (inventoryChanged)
                character.FillEmptySlots();
            return true;
        }
    }
}