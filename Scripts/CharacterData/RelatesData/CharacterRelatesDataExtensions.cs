using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static partial class CharacterRelatesDataExtensions
    {
        public static bool IsEmpty(this CharacterStats data)
        {
            return data.Equals(CharacterStats.Empty);
        }

        public static bool IsEmpty(this CharacterAttribute data)
        {
            return data.Equals(CharacterAttribute.Empty);
        }

        public static bool IsEmpty(this CharacterBuff data)
        {
            return data.Equals(CharacterBuff.Empty);
        }

        public static bool IsEmpty(this CharacterHotkey data)
        {
            return data.Equals(CharacterHotkey.Empty);
        }

        public static bool IsEmpty(this CharacterItem data)
        {
            return data.Equals(CharacterItem.Empty);
        }

        public static bool IsEmptySlot(this CharacterItem data)
        {
            return data.IsEmpty() || data.dataId == 0 || data.amount <= 0 || data.GetItem() == null;
        }

        public static bool NotEmptySlot(this CharacterItem data)
        {
            return !data.IsEmptySlot();
        }

        public static bool IsEmpty(this CharacterQuest data)
        {
            return data.Equals(CharacterQuest.Empty);
        }

        public static bool IsEmpty(this CharacterSkill data)
        {
            return data.Equals(CharacterSkill.Empty);
        }

        public static bool IsEmpty(this CharacterSkillUsage data)
        {
            return data.Equals(CharacterSkillUsage.Empty);
        }

        public static bool IsEmpty(this CharacterSummon data)
        {
            return data.Equals(CharacterSummon.Empty);
        }

        public static bool IsDiffer(this CharacterItem data, CharacterItem anotherData,
            bool checkLevel = false,
            bool checkSockets = false,
            bool checkRandomSeed = false,
            bool checkAmmoDataId = false,
            bool checkAmmoAmount = false,
            bool checkDurability = false)
        {
            if (checkLevel && data.level != anotherData.level)
                return true;
            if (checkSockets && IsDifferSockets(data, anotherData))
                return true;
            if (checkRandomSeed && data.randomSeed != anotherData.randomSeed)
                return true;
            if (checkAmmoDataId && data.ammoDataId != anotherData.ammoDataId)
                return true;
            if (checkAmmoAmount && data.ammo != anotherData.ammo)
                return true;
            if (checkDurability && !Mathf.Approximately(data.durability, anotherData.durability))
                return true;
            return !string.Equals(data.id, anotherData.id) || data.dataId != anotherData.dataId;
        }

        public static bool IsDifferSockets(this CharacterItem data, CharacterItem anotherData)
        {
            int len1 = 0;
            int len2 = 0;
            if (data.sockets != null)
                len1 = data.sockets.Count;
            if (anotherData.sockets != null)
                len2 = anotherData.sockets.Count;
            if (len1 != len2)
                return true;
            if (len1 == 0)
                return false;
            for (int i = 0; i < data.sockets.Count; ++i)
            {
                if (data.sockets[i] != anotherData.sockets[i])
                    return true;
            }
            return false;
        }

        public static bool IsDiffer(this EquipWeapons data, EquipWeapons anotherData,
            out bool rightIsDiffer, out bool leftIsDiffer,
            bool checkLevel = false,
            bool checkSockets = false,
            bool checkRandomSeed = false,
            bool checkAmmoDataId = false,
            bool checkAmmoAmount = false,
            bool checkDurability = false)
        {
            rightIsDiffer = data.rightHand.IsDiffer(anotherData.rightHand, checkLevel, checkSockets, checkRandomSeed, checkAmmoDataId, checkAmmoAmount, checkDurability);
            leftIsDiffer = data.leftHand.IsDiffer(anotherData.leftHand, checkLevel, checkSockets, checkRandomSeed, checkAmmoDataId, checkAmmoAmount, checkDurability);
            return rightIsDiffer || leftIsDiffer;
        }

        public static IWeaponItem GetRightHandWeaponItem(this EquipWeapons equipWeapons)
        {
            if (equipWeapons.IsEmptyRightHandSlot())
                return null;
            return equipWeapons.rightHand.GetWeaponItem();
        }

        public static IEquipmentItem GetRightHandEquipmentItem(this EquipWeapons equipWeapons)
        {
            if (equipWeapons.IsEmptyRightHandSlot())
                return null;
            return equipWeapons.rightHand.GetEquipmentItem();
        }

        public static BaseItem GetRightHandItem(this EquipWeapons equipWeapons)
        {
            if (equipWeapons.IsEmptyRightHandSlot())
                return null;
            return equipWeapons.rightHand.GetItem();
        }

        public static IWeaponItem GetLeftHandWeaponItem(this EquipWeapons equipWeapons)
        {
            if (equipWeapons.IsEmptyLeftHandSlot())
                return null;
            return equipWeapons.leftHand.GetWeaponItem();
        }

        public static IShieldItem GetLeftHandShieldItem(this EquipWeapons equipWeapons)
        {
            if (equipWeapons.IsEmptyLeftHandSlot())
                return null;
            return equipWeapons.leftHand.GetShieldItem();
        }

        public static IEquipmentItem GetLeftHandEquipmentItem(this EquipWeapons equipWeapons)
        {
            if (equipWeapons.IsEmptyLeftHandSlot())
                return null;
            return equipWeapons.leftHand.GetEquipmentItem();
        }

        public static BaseItem GetLeftHandItem(this EquipWeapons equipWeapons)
        {
            if (equipWeapons.IsEmptyLeftHandSlot())
                return null;
            return equipWeapons.leftHand.GetItem();
        }

        public static bool IsEmptyRightHandSlot(this EquipWeapons equipWeapons)
        {
            return equipWeapons.rightHand.IsEmptySlot();
        }

        public static bool IsEmptyLeftHandSlot(this EquipWeapons equipWeapons)
        {
            return equipWeapons.leftHand.IsEmptySlot();
        }

        public static bool NotEmptyRightHandSlot(this EquipWeapons equipWeapons)
        {
            return !equipWeapons.IsEmptyRightHandSlot();
        }

        public static bool NotEmptyLeftHandSlot(this EquipWeapons equipWeapons)
        {
            return !equipWeapons.IsEmptyLeftHandSlot();
        }

        public static int IndexOfEmptyItemSlot(this IList<CharacterItem> list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].IsEmptySlot())
                    return i;
            }
            return -1;
        }
    }
}
