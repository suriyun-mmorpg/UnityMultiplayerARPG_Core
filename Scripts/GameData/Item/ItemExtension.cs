using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class ItemExtension
    {
        #region Item Type Extension

        public static bool IsDefendEquipment<T>(this T item)
            where T : IItem
        {
            return item.IsArmor() || item.IsShield();
        }

        public static bool IsEquipment<T>(this T item)
            where T : IItem
        {
            return item.IsDefendEquipment() || item.IsWeapon();
        }

        public static bool IsUsable<T>(this T item)
            where T : IItem
        {
            return item.IsPotion() || item.IsBuilding() || item.IsPet() || item.IsMount() || item.IsSkill() ;
        }

        public static bool IsJunk<T>(this T item)
            where T : IItem
        {
            return item.ItemType == ItemType.Junk;
        }

        public static bool IsArmor<T>(this T item)
            where T : IItem
        {
            return item.ItemType == ItemType.Armor;
        }

        public static bool IsShield<T>(this T item)
            where T : IItem
        {
            return item.ItemType == ItemType.Shield;
        }

        public static bool IsWeapon<T>(this T item)
            where T : IItem
        {
            return item.ItemType == ItemType.Weapon;
        }

        public static bool IsPotion<T>(this T item)
            where T : IItem
        {
            return item.ItemType == ItemType.Potion;
        }

        public static bool IsAmmo<T>(this T item)
            where T : IItem
        {
            return item.ItemType == ItemType.Ammo;
        }

        public static bool IsBuilding<T>(this T item)
            where T : IItem
        {
            return item.ItemType == ItemType.Building;
        }

        public static bool IsPet<T>(this T item)
            where T : IItem
        {
            return item.ItemType == ItemType.Pet;
        }

        public static bool IsSocketEnhancer<T>(this T item)
            where T : IItem
        {
            return item.ItemType == ItemType.SocketEnhancer;
        }

        public static bool IsMount<T>(this T item)
            where T : IItem
        {
            return item.ItemType == ItemType.Mount;
        }

        public static bool IsSkill<T>(this T item)
            where T : IItem
        {
            return item.ItemType == ItemType.Skill;
        }
        #endregion

        #region Ammo Extension
        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this IAmmoItem ammoItem, short level)
        {
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            if (ammoItem != null && ammoItem.IsAmmo())
                result = GameDataHelpers.CombineDamages(ammoItem.IncreaseDamages, result, level, 1f);
            return result;
        }
        #endregion

        #region Equipment Extension
        public static CharacterStats GetIncreaseStats<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            if (equipmentItem == null || !equipmentItem.IsEquipment())
                return new CharacterStats();
            return equipmentItem.IncreaseStats.GetCharacterStats(level);
        }

        public static CharacterStats GetIncreaseStatsRate<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            if (equipmentItem == null || !equipmentItem.IsEquipment())
                return new CharacterStats();
            return equipmentItem.IncreaseStatsRate.GetCharacterStats(level);
        }

        public static Dictionary<Attribute, float> GetIncreaseAttributes<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineAttributes(equipmentItem.IncreaseAttributes, result, level, 1f);
            return result;
        }

        public static Dictionary<Attribute, float> GetIncreaseAttributesRate<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineAttributes(equipmentItem.IncreaseAttributesRate, result, level, 1f);
            return result;
        }

        public static Dictionary<DamageElement, float> GetIncreaseResistances<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineResistances(equipmentItem.IncreaseResistances, result, level, 1f);
            return result;
        }

        public static Dictionary<DamageElement, float> GetIncreaseArmors<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineArmors(equipmentItem.IncreaseArmors, result, level, 1f);
            return result;
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages<T>(this T equipmentItem, short level)
            where T : IEquipmentItem
        {
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineDamages(equipmentItem.IncreaseDamages, result, level, 1f);
            return result;
        }

        public static Dictionary<BaseSkill, short> GetIncreaseSkills<T>(this T equipmentItem)
            where T : IEquipmentItem
        {
            Dictionary<BaseSkill, short> result = new Dictionary<BaseSkill, short>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
                result = GameDataHelpers.CombineSkills(equipmentItem.IncreaseSkillLevels, result);
            return result;
        }
        #endregion

        #region Armor/Shield Extension
        public static KeyValuePair<DamageElement, float> GetArmorAmount<T>(this T defendItem, short level, float rate)
            where T : IDefendEquipmentItem
        {
            if (defendItem == null || !defendItem.IsDefendEquipment())
                return new KeyValuePair<DamageElement, float>();
            return GameDataHelpers.MakeArmor(defendItem.ArmorAmount, level, rate);
        }
        #endregion

        #region Weapon Extension
        public static KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount<T>(this T weaponItem, short itemLevel, float statsRate, ICharacterData character)
            where T : IWeaponItem
        {
            if (weaponItem == null || !weaponItem.IsWeapon())
                return new KeyValuePair<DamageElement, MinMaxFloat>();
            return GameDataHelpers.MakeDamage(weaponItem.DamageAmount, itemLevel, statsRate, weaponItem.GetEffectivenessDamage(character));
        }

        public static float GetEffectivenessDamage<T>(this T weaponItem, ICharacterData character)
            where T : IWeaponItem
        {
            if (weaponItem == null || !weaponItem.IsWeapon())
                return 0f;
            return GameDataHelpers.GetEffectivenessDamage(weaponItem.WeaponType.CacheEffectivenessAttributes, character);
        }

        public static bool TryGetWeaponItemEquipType<T>(this T weaponItem, out WeaponItemEquipType equipType)
            where T : IWeaponItem
        {
            equipType = WeaponItemEquipType.OneHand;
            if (weaponItem == null || !weaponItem.IsWeapon())
                return false;
            equipType = weaponItem.EquipType;
            return true;
        }
        #endregion

        public static bool CanEquip<T>(this T item, BaseCharacterEntity character, short level, out GameMessage.Type gameMessageType)
             where T : IEquipmentItem
        {
            gameMessageType = GameMessage.Type.None;
            if (!item.IsEquipment() || character == null)
                return false;

            // Check is it pass attribute requirement or not
            Dictionary<Attribute, float> attributeAmountsDict = character.GetAttributes(true, false, null);
            Dictionary<Attribute, float> requireAttributeAmounts = item.RequireAttributeAmounts;
            foreach (KeyValuePair<Attribute, float> requireAttributeAmount in requireAttributeAmounts)
            {
                if (!attributeAmountsDict.ContainsKey(requireAttributeAmount.Key) ||
                    attributeAmountsDict[requireAttributeAmount.Key] < requireAttributeAmount.Value)
                {
                    gameMessageType = GameMessage.Type.NotEnoughAttributeAmounts;
                    return false;
                }
            }

            // Check another requirements
            if (item.Requirement.character != null && item.Requirement.character != character.GetDatabase())
            {
                gameMessageType = GameMessage.Type.NotMatchCharacterClass;
                return false;
            }

            if (character.Level < item.Requirement.level)
            {
                gameMessageType = GameMessage.Type.NotEnoughLevel;
                return false;
            }

            return true;
        }

        public static bool CanAttack<T>(this T item, BaseCharacterEntity character)
             where T : IWeaponItem
        {
            if (!item.IsWeapon() || character == null)
                return false;

            AmmoType requireAmmoType = item.WeaponType.RequireAmmoType;
            return requireAmmoType == null || character.IndexOfAmmoItem(requireAmmoType) >= 0;
        }
    }
}
