using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class ItemExtensions
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
            return item.IsPotion() || item.IsBuilding() || item.IsPet() || item.IsMount() || item.IsSkill();
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
        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this IAmmoItem ammoItem, int level)
        {
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            if (ammoItem != null && ammoItem.IsAmmo())
                result = GameDataHelpers.CombineDamages(ammoItem.IncreaseDamages, result, level, 1f);
            return result;
        }
        #endregion

        #region Equipment Extension
        public static CharacterStats GetIncreaseStats<T>(this T equipmentItem, int level, int randomSeed, bool withRandomBonus = true)
            where T : IEquipmentItem
        {
            if (equipmentItem == null || !equipmentItem.IsEquipment())
                return new CharacterStats();
            CharacterStats result = equipmentItem.IncreaseStats.GetCharacterStats(level);
            if (withRandomBonus)
                result = result + ItemRandomBonusCacheManager.GetCaches(equipmentItem, randomSeed).CharacterStats;
            return result;
        }

        public static CharacterStats GetIncreaseStatsRate<T>(this T equipmentItem, int level, int randomSeed, bool withRandomBonus = true)
            where T : IEquipmentItem
        {
            if (equipmentItem == null || !equipmentItem.IsEquipment())
                return new CharacterStats();
            CharacterStats result = equipmentItem.IncreaseStatsRate.GetCharacterStats(level);
            if (withRandomBonus)
                result = result + ItemRandomBonusCacheManager.GetCaches(equipmentItem, randomSeed).CharacterStatsRate;
            return result;
        }

        public static Dictionary<Attribute, float> GetIncreaseAttributes<T>(this T equipmentItem, int level, int randomSeed, Dictionary<Attribute, float> result = null, bool withRandomBonus = true)
            where T : IEquipmentItem
        {
            if (result == null)
                result = new Dictionary<Attribute, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
            {
                result = GameDataHelpers.CombineAttributes(equipmentItem.IncreaseAttributes, result, level, 1f);
                if (withRandomBonus && equipmentItem.RandomBonus.randomAttributeAmounts != null && equipmentItem.RandomBonus.randomAttributeAmounts.Length > 0)
                    result = GameDataHelpers.CombineAttributes(result, ItemRandomBonusCacheManager.GetCaches(equipmentItem, randomSeed).AttributeAmounts);
            }
            return result;
        }

        public static Dictionary<Attribute, float> GetIncreaseAttributesRate<T>(this T equipmentItem, int level, int randomSeed, Dictionary<Attribute, float> result = null, bool withRandomBonus = true)
            where T : IEquipmentItem
        {
            if (result == null)
                result = new Dictionary<Attribute, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
            {
                result = GameDataHelpers.CombineAttributes(equipmentItem.IncreaseAttributesRate, result, level, 1f);
                if (withRandomBonus && equipmentItem.RandomBonus.randomAttributeAmountRates != null && equipmentItem.RandomBonus.randomAttributeAmountRates.Length > 0)
                    result = GameDataHelpers.CombineAttributes(result, ItemRandomBonusCacheManager.GetCaches(equipmentItem, randomSeed).AttributeAmountRates);
            }
            return result;
        }

        public static Dictionary<DamageElement, float> GetIncreaseResistances<T>(this T equipmentItem, int level, int randomSeed, Dictionary<DamageElement, float> result = null, bool withRandomBonus = true)
            where T : IEquipmentItem
        {
            if (result == null)
                result = new Dictionary<DamageElement, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
            {
                result = GameDataHelpers.CombineResistances(equipmentItem.IncreaseResistances, result, level, 1f);
                if (withRandomBonus && equipmentItem.RandomBonus.randomResistanceAmounts != null && equipmentItem.RandomBonus.randomResistanceAmounts.Length > 0)
                    result = GameDataHelpers.CombineResistances(result, ItemRandomBonusCacheManager.GetCaches(equipmentItem, randomSeed).ResistanceAmounts);
            }
            return result;
        }

        public static Dictionary<DamageElement, float> GetIncreaseArmors<T>(this T equipmentItem, int level, int randomSeed, Dictionary<DamageElement, float> result = null, bool withRandomBonus = true)
            where T : IEquipmentItem
        {
            if (result == null)
                result = new Dictionary<DamageElement, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
            {
                result = GameDataHelpers.CombineArmors(equipmentItem.IncreaseArmors, result, level, 1f);
                if (withRandomBonus && equipmentItem.RandomBonus.randomArmorAmounts != null && equipmentItem.RandomBonus.randomArmorAmounts.Length > 0)
                    result = GameDataHelpers.CombineArmors(result, ItemRandomBonusCacheManager.GetCaches(equipmentItem, randomSeed).ArmorAmounts);
            }
            return result;
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages<T>(this T equipmentItem, int level, int randomSeed, Dictionary<DamageElement, MinMaxFloat> result = null, bool withRandomBonus = true)
            where T : IEquipmentItem
        {
            if (result == null)
                result = new Dictionary<DamageElement, MinMaxFloat>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
            {
                result = GameDataHelpers.CombineDamages(equipmentItem.IncreaseDamages, result, level, 1f);
                if (withRandomBonus && equipmentItem.RandomBonus.randomDamageAmounts != null && equipmentItem.RandomBonus.randomDamageAmounts.Length > 0)
                    result = GameDataHelpers.CombineDamages(result, ItemRandomBonusCacheManager.GetCaches(equipmentItem, randomSeed).DamageAmounts);
            }
            return result;
        }

        public static Dictionary<BaseSkill, int> GetIncreaseSkills<T>(this T equipmentItem, int level, int randomSeed, Dictionary<BaseSkill, int> result = null, bool withRandomBonus = true)
            where T : IEquipmentItem
        {
            if (result == null)
                result = new Dictionary<BaseSkill, int>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
            {
                result = GameDataHelpers.CombineSkills(equipmentItem.IncreaseSkills, result, level, 1f);
                if (withRandomBonus && equipmentItem.RandomBonus.randomSkillLevels != null && equipmentItem.RandomBonus.randomSkillLevels.Length > 0)
                    result = GameDataHelpers.CombineSkills(result, ItemRandomBonusCacheManager.GetCaches(equipmentItem, randomSeed).SkillLevels);
            }
            return result;
        }

        public static void ApplySelfStatusEffectsWhenAttacking<T>(this T equipmentItem, int level, EntityInfo applier, CharacterItem weapon, BaseCharacterEntity target)
            where T : IEquipmentItem
        {
            if (level <= 0 || target == null || equipmentItem == null || !equipmentItem.IsEquipment())
                return;
            equipmentItem.SelfStatusEffectsWhenAttacking.ApplyStatusEffect(level, applier, weapon, target);
        }

        public static void ApplyEnemyStatusEffectsWhenAttacking<T>(this T equipmentItem, int level, EntityInfo applier, CharacterItem weapon, BaseCharacterEntity target)
            where T : IEquipmentItem
        {
            if (level <= 0 || target == null || equipmentItem == null || !equipmentItem.IsEquipment())
                return;
            equipmentItem.EnemyStatusEffectsWhenAttacking.ApplyStatusEffect(level, applier, weapon, target);
        }

        public static void ApplySelfStatusEffectsWhenAttacked<T>(this T equipmentItem, int level, EntityInfo applier, BaseCharacterEntity target)
            where T : IEquipmentItem
        {
            if (level <= 0 || target == null || equipmentItem == null || !equipmentItem.IsEquipment())
                return;
            equipmentItem.SelfStatusEffectsWhenAttacked.ApplyStatusEffect(level, applier, null, target);
        }

        public static void ApplyEnemyStatusEffectsWhenAttacked<T>(this T equipmentItem, int level, EntityInfo applier, BaseCharacterEntity target)
            where T : IEquipmentItem
        {
            if (level <= 0 || target == null || equipmentItem == null || !equipmentItem.IsEquipment())
                return;
            equipmentItem.EnemyStatusEffectsWhenAttacked.ApplyStatusEffect(level, applier, null, target);
        }
        #endregion

        #region Armor/Shield Extension
        public static KeyValuePair<DamageElement, float> GetArmorAmount<T>(this T defendItem, int level, float rate)
            where T : IDefendEquipmentItem
        {
            if (defendItem == null || !defendItem.IsDefendEquipment())
                return new KeyValuePair<DamageElement, float>();
            return GameDataHelpers.ToKeyValuePair(defendItem.ArmorAmount, level, rate);
        }

        public static string GetEquipPosition<T>(this T armorItem)
            where T : IArmorItem
        {
            if (armorItem == null || armorItem.ArmorType == null)
                return string.Empty;
            return armorItem.ArmorType.EquipPosition;
        }
        #endregion

        #region Weapon Extension
        public static WeaponItemEquipType GetEquipType<T>(this T weaponItem)
            where T : IWeaponItem
        {
            if (weaponItem == null || !weaponItem.IsWeapon() || !weaponItem.WeaponType)
                return WeaponItemEquipType.MainHandOnly;
            return weaponItem.WeaponType.EquipType;
        }

        public static DualWieldRestriction GetDualWieldRestriction<T>(this T weaponItem)
            where T : IWeaponItem
        {
            if (weaponItem == null || !weaponItem.IsWeapon() || !weaponItem.WeaponType)
                return DualWieldRestriction.None;
            return weaponItem.WeaponType.DualWieldRestriction;
        }

        public static KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount<T>(this T weaponItem, int itemLevel, float statsRate, ICharacterData character)
            where T : IWeaponItem
        {
            return weaponItem.GetDamageAmount(itemLevel, statsRate, weaponItem.GetEffectivenessDamage(character));
        }

        public static KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount<T>(this T weaponItem, int itemLevel, float statsRate, float effectiveness)
            where T : IWeaponItem
        {
            if (weaponItem == null || !weaponItem.IsWeapon())
                return new KeyValuePair<DamageElement, MinMaxFloat>();
            return GameDataHelpers.ToKeyValuePair(weaponItem.DamageAmount, itemLevel, statsRate, effectiveness);
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
            equipType = WeaponItemEquipType.MainHandOnly;
            if (weaponItem == null || !weaponItem.IsWeapon())
                return false;
            equipType = weaponItem.GetEquipType();
            return true;
        }

        public static bool TryGetWeaponItemDualWieldRestriction<T>(this T weaponItem, out DualWieldRestriction dualWieldRestriction)
            where T : IWeaponItem
        {
            dualWieldRestriction = DualWieldRestriction.None;
            if (weaponItem == null || !weaponItem.IsWeapon())
                return false;
            dualWieldRestriction = weaponItem.GetDualWieldRestriction();
            return true;
        }

        public static WeaponType GetWeaponTypeOrDefault<T>(this T weaponItem)
            where T : IWeaponItem
        {
            if (weaponItem == null || !weaponItem.IsWeapon())
                return GameInstance.Singleton.DefaultWeaponType;
            return weaponItem.WeaponType;
        }
        #endregion

        #region Socket Enhancer Extension
        public static void ApplySelfStatusEffectsWhenAttacking<T>(this T socketEnhancerItem, EntityInfo applier, CharacterItem weapon, BaseCharacterEntity target)
            where T : ISocketEnhancerItem
        {
            if (target == null || socketEnhancerItem == null || !socketEnhancerItem.IsSocketEnhancer())
                return;
            socketEnhancerItem.SelfStatusEffectsWhenAttacking.ApplyStatusEffect(1, applier, weapon, target);
        }

        public static void ApplyEnemyStatusEffectsWhenAttacking<T>(this T socketEnhancerItem, EntityInfo applier, CharacterItem weapon, BaseCharacterEntity target)
            where T : ISocketEnhancerItem
        {
            if (target == null || socketEnhancerItem == null || !socketEnhancerItem.IsSocketEnhancer())
                return;
            socketEnhancerItem.EnemyStatusEffectsWhenAttacking.ApplyStatusEffect(1, applier, weapon, target);
        }

        public static void ApplySelfStatusEffectsWhenAttacked<T>(this T socketEnhancerItem, EntityInfo applier, BaseCharacterEntity target)
            where T : ISocketEnhancerItem
        {
            if (target == null || socketEnhancerItem == null || !socketEnhancerItem.IsSocketEnhancer())
                return;
            socketEnhancerItem.SelfStatusEffectsWhenAttacked.ApplyStatusEffect(1, applier, null, target);
        }

        public static void ApplyEnemyStatusEffectsWhenAttacked<T>(this T socketEnhancerItem, EntityInfo applier, BaseCharacterEntity target)
            where T : ISocketEnhancerItem
        {
            if (target == null || socketEnhancerItem == null || !socketEnhancerItem.IsSocketEnhancer())
                return;
            socketEnhancerItem.EnemyStatusEffectsWhenAttacked.ApplyStatusEffect(1, applier, null, target);
        }
        #endregion

        public static bool CanEquip<T>(this T item, ICharacterData character, int level, out UITextKeys gameMessage)
             where T : IEquipmentItem
        {
            gameMessage = UITextKeys.NONE;
            if (!item.IsEquipment() || character == null)
                return false;

            if (character.Level < item.Requirement.level)
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_LEVEL;
                return false;
            }

            if (!item.Requirement.ClassIsAvailable(character.GetDatabase() as PlayerCharacter))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_MATCH_CHARACTER_CLASS;
                return false;
            }

            // Check is it pass attribute requirement or not
            Dictionary<Attribute, float> currentAttributeAmounts = character.GetAttributes(true, false, character.GetSkills(true));
            Dictionary<Attribute, float> requireAttributeAmounts = item.RequireAttributeAmounts;
            foreach (KeyValuePair<Attribute, float> requireAttributeAmount in requireAttributeAmounts)
            {
                if (!currentAttributeAmounts.ContainsKey(requireAttributeAmount.Key) ||
                    currentAttributeAmounts[requireAttributeAmount.Key] < requireAttributeAmount.Value)
                {
                    gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ATTRIBUTE_AMOUNTS;
                    return false;
                }
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
