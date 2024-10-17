using System.Collections.Generic;

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

        public static bool IsDiffer(this CharacterItem data, CharacterItem anotherData, bool checkLevel = false)
        {
            if (checkLevel && data.level != anotherData.level)
                return true;
            return !string.Equals(data.id, anotherData.id) || data.dataId != anotherData.dataId;
        }

        public static bool IsDifferSockets(this CharacterItem data, CharacterItem anotherData)
        {
            if (data.sockets == null && anotherData.sockets == null)
                return false;
            if (data.sockets.Count != anotherData.sockets.Count)
                return true;
            for (int i = 0; i < data.sockets.Count; ++i)
            {
                if (data.sockets[i] != anotherData.sockets[i])
                    return true;
            }
            return false;
        }

        public static bool IsDiffer(this EquipWeapons data, EquipWeapons anotherData, out bool rightIsDiffer, out bool leftIsDiffer, bool checkLevel = false)
        {
            rightIsDiffer = data.rightHand.IsDiffer(anotherData.rightHand, checkLevel);
            leftIsDiffer = data.leftHand.IsDiffer(anotherData.leftHand, checkLevel);
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

        public static bool TryGetMountType(this CharacterBuff data, out MountType mountType)
        {
            switch (data.type)
            {
                case BuffType.SkillBuff:
                    mountType = MountType.SkillBuff;
                    return true;
                case BuffType.SkillDebuff:
                    mountType = MountType.SkillDebuff;
                    return true;
                case BuffType.PotionBuff:
                    mountType = MountType.PotionBuff;
                    return true;
                case BuffType.GuildSkillBuff:
                    mountType = MountType.GuildSkillBuff;
                    return true;
                case BuffType.StatusEffect:
                    mountType = MountType.StatusEffect;
                    return true;
            }
            mountType = MountType.SkillBuff;
            return false;
        }

        public static bool TryGetBuffType(this CharacterMount data, out BuffType mountType)
        {
            switch (data.type)
            {
                case MountType.SkillBuff:
                    mountType = BuffType.SkillBuff;
                    return true;
                case MountType.SkillDebuff:
                    mountType = BuffType.SkillDebuff;
                    return true;
                case MountType.PotionBuff:
                    mountType = BuffType.PotionBuff;
                    return true;
                case MountType.GuildSkillBuff:
                    mountType = BuffType.GuildSkillBuff;
                    return true;
                case MountType.StatusEffect:
                    mountType = BuffType.StatusEffect;
                    return true;
            }
            mountType = BuffType.SkillBuff;
            return false;
        }
    }
}
