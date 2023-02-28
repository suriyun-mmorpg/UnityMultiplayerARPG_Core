namespace MultiplayerARPG
{
    public static class CharacterRelatesDataExtensions
    {
        public static bool IsEmpty(this CharacterStats data)
        {
            return data.Equals(CharacterStats.Empty);
        }

        public static bool IsEmpty(this CharacterAttribute data)
        {
            return data == null || data.Equals(CharacterAttribute.Empty);
        }

        public static bool IsEmpty(this CharacterBuff data)
        {
            return data == null || data.Equals(CharacterBuff.Empty);
        }

        public static bool IsEmpty(this CharacterHotkey data)
        {
            return data == null || data.Equals(CharacterHotkey.Empty);
        }

        public static bool IsEmpty(this CharacterItem data)
        {
            return data == null || data.Equals(CharacterItem.Empty);
        }

        public static bool NotEmptySlot(this CharacterItem data)
        {
            return !data.IsEmpty() && data.GetItem() != null && data.amount > 0;
        }

        public static bool IsEmptySlot(this CharacterItem data)
        {
            return !data.NotEmptySlot();
        }

        public static bool IsEmpty(this CharacterQuest data)
        {
            return data == null || data.Equals(CharacterQuest.Empty);
        }

        public static bool IsEmpty(this CharacterSkill data)
        {
            return data == null || data.Equals(CharacterSkill.Empty);
        }

        public static bool IsEmpty(this CharacterSkillUsage data)
        {
            return data == null || data.Equals(CharacterSkillUsage.Empty);
        }

        public static bool IsEmpty(this CharacterSummon data)
        {
            return data == null || data.Equals(CharacterSummon.Empty);
        }

        public static bool IsDiffer(this CharacterItem data, CharacterItem anotherData)
        {
            if (data == null && anotherData == null)
                return false;
            if ((data != null && anotherData == null) || (data == null && anotherData != null))
                return true;
            return data.id != anotherData.id;
        }

        public static bool IsDiffer(this EquipWeapons data, EquipWeapons anotherData, out bool rightIsDiffer, out bool leftIsDiffer)
        {
            rightIsDiffer = false;
            leftIsDiffer = false;
            if (data == null && anotherData == null)
                return false;
            if ((data != null && anotherData == null) || (data == null && anotherData != null))
            {
                rightIsDiffer = true;
                leftIsDiffer = true;
                return true;
            }
            rightIsDiffer = data.rightHand.IsDiffer(anotherData.rightHand);
            leftIsDiffer = data.leftHand.IsDiffer(anotherData.leftHand);
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

        public static bool NotEmptyRightHandSlot(this EquipWeapons equipWeapons)
        {
            return equipWeapons != null && equipWeapons.rightHand.NotEmptySlot();
        }

        public static bool NotEmptyLeftHandSlot(this EquipWeapons equipWeapons)
        {
            return equipWeapons != null && equipWeapons.leftHand.NotEmptySlot();
        }

        public static bool IsEmptyRightHandSlot(this EquipWeapons equipWeapons)
        {
            return !equipWeapons.NotEmptyRightHandSlot();
        }

        public static bool IsEmptyLeftHandSlot(this EquipWeapons equipWeapons)
        {
            return !equipWeapons.NotEmptyLeftHandSlot();
        }
    }
}
