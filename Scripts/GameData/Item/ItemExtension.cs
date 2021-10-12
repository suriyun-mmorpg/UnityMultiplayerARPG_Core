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

        public static Dictionary<Attribute, float> GetIncreaseAttributes<T>(this T equipmentItem, short level, int randomSeed)
            where T : IEquipmentItem
        {
            Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
            {
                result = GameDataHelpers.CombineAttributes(equipmentItem.IncreaseAttributes, result, level, 1f);
                if (equipmentItem.RandomBonus.randomAttributeAmounts != null &&
                    equipmentItem.RandomBonus.randomAttributeAmounts.Length > 0)
                {
                    randomSeed = randomSeed.Increase(equipmentItem.RandomBonus.randomAttributeAmounts.Length * 8);
                    foreach (var randomBonus in equipmentItem.RandomBonus.randomAttributeAmounts)
                    {
                        if (!randomBonus.Apply(randomSeed)) continue;
                        randomSeed = randomSeed.Increase(16);
                        result = GameDataHelpers.CombineAttributes(result, randomBonus.GetRandomedAmount(randomSeed).ToKeyValuePair(1f));
                        randomSeed = randomSeed.Increase(16);
                    }
                }
            }
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

        public static Dictionary<DamageElement, float> GetIncreaseResistances<T>(this T equipmentItem, short level, int randomSeed)
            where T : IEquipmentItem
        {
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
            {
                result = GameDataHelpers.CombineResistances(equipmentItem.IncreaseResistances, result, level, 1f);
                if (equipmentItem.RandomBonus.randomResistanceAmounts != null &&
                    equipmentItem.RandomBonus.randomResistanceAmounts.Length > 0)
                {
                    randomSeed = randomSeed.Increase(equipmentItem.RandomBonus.randomResistanceAmounts.Length * 8);
                    foreach (var randomBonus in equipmentItem.RandomBonus.randomResistanceAmounts)
                    {
                        if (!randomBonus.Apply(randomSeed)) continue;
                        randomSeed = randomSeed.Increase(16);
                        result = GameDataHelpers.CombineResistances(result, randomBonus.GetRandomedAmount(randomSeed).ToKeyValuePair(1f));
                        randomSeed = randomSeed.Increase(16);
                    }
                }
            }
            return result;
        }

        public static Dictionary<DamageElement, float> GetIncreaseArmors<T>(this T equipmentItem, short level, int randomSeed)
            where T : IEquipmentItem
        {
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
            {
                result = GameDataHelpers.CombineArmors(equipmentItem.IncreaseArmors, result, level, 1f);
                if (equipmentItem.RandomBonus.randomArmorAmounts != null &&
                    equipmentItem.RandomBonus.randomArmorAmounts.Length > 0)
                {
                    randomSeed = randomSeed.Increase(equipmentItem.RandomBonus.randomArmorAmounts.Length * 8);
                    foreach (var randomBonus in equipmentItem.RandomBonus.randomArmorAmounts)
                    {
                        if (!randomBonus.Apply(randomSeed)) continue;
                        randomSeed = randomSeed.Increase(16);
                        result = GameDataHelpers.CombineArmors(result, randomBonus.GetRandomedAmount(randomSeed).ToKeyValuePair(1f));
                        randomSeed = randomSeed.Increase(16);
                    }
                }
            }
            return result;
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages<T>(this T equipmentItem, short level, int randomSeed)
            where T : IEquipmentItem
        {
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
            {
                result = GameDataHelpers.CombineDamages(equipmentItem.IncreaseDamages, result, level, 1f);
                if (equipmentItem.RandomBonus.randomDamageAmounts != null &&
                    equipmentItem.RandomBonus.randomDamageAmounts.Length > 0)
                {
                    randomSeed = randomSeed.Increase(equipmentItem.RandomBonus.randomDamageAmounts.Length * 8);
                    foreach (var randomBonus in equipmentItem.RandomBonus.randomDamageAmounts)
                    {
                        if (!randomBonus.Apply(randomSeed)) continue;
                        randomSeed = randomSeed.Increase(16);
                        result = GameDataHelpers.CombineDamages(result, randomBonus.GetRandomedAmount(randomSeed).ToKeyValuePair(1f, 1f));
                        randomSeed = randomSeed.Increase(16);
                    }
                }
            }
            return result;
        }

        public static Dictionary<BaseSkill, short> GetIncreaseSkills<T>(this T equipmentItem, int randomSeed)
            where T : IEquipmentItem
        {
            Dictionary<BaseSkill, short> result = new Dictionary<BaseSkill, short>();
            if (equipmentItem != null && equipmentItem.IsEquipment())
            {
                result = GameDataHelpers.CombineSkills(equipmentItem.IncreaseSkillLevels, result);
                if (equipmentItem.RandomBonus.randomSkillLevels != null &&
                    equipmentItem.RandomBonus.randomSkillLevels.Length > 0)
                {
                    randomSeed = randomSeed.Increase(equipmentItem.RandomBonus.randomSkillLevels.Length * 8);
                    foreach (var randomBonus in equipmentItem.RandomBonus.randomSkillLevels)
                    {
                        if (!randomBonus.Apply(randomSeed)) continue;
                        randomSeed = randomSeed.Increase(16);
                        result = GameDataHelpers.CombineSkills(result, randomBonus.GetRandomedAmount(randomSeed).ToKeyValuePair());
                        randomSeed = randomSeed.Increase(16);
                    }
                }
            }
            return result;
        }

        public static void ApplySelfStatusEffectsWhenAttacking<T>(this T equipmentItem, short level, EntityInfo applier, BaseCharacterEntity target)
            where T : IEquipmentItem
        {
            if (level <= 0 || target == null || equipmentItem == null || !equipmentItem.IsEquipment())
                return;
            equipmentItem.SelfStatusEffectsWhenAttacking.ApplyStatusEffect(level, applier, target);
        }

        public static void ApplyEnemyStatusEffectsWhenAttacking<T>(this T equipmentItem, short level, EntityInfo applier, BaseCharacterEntity target)
            where T : IEquipmentItem
        {
            if (level <= 0 || target == null || equipmentItem == null || !equipmentItem.IsEquipment())
                return;
            equipmentItem.EnemyStatusEffectsWhenAttacking.ApplyStatusEffect(level, applier, target);
        }

        public static void ApplySelfStatusEffectsWhenAttacked<T>(this T equipmentItem, short level, EntityInfo applier, BaseCharacterEntity target)
            where T : IEquipmentItem
        {
            if (level <= 0 || target == null || equipmentItem == null || !equipmentItem.IsEquipment())
                return;
            equipmentItem.SelfStatusEffectsWhenAttacked.ApplyStatusEffect(level, applier, target);
        }

        public static void ApplyEnemyStatusEffectsWhenAttacked<T>(this T equipmentItem, short level, EntityInfo applier, BaseCharacterEntity target)
            where T : IEquipmentItem
        {
            if (level <= 0 || target == null || equipmentItem == null || !equipmentItem.IsEquipment())
                return;
            equipmentItem.EnemyStatusEffectsWhenAttacked.ApplyStatusEffect(level, applier, target);
        }
        #endregion

        #region Armor/Shield Extension
        public static KeyValuePair<DamageElement, float> GetArmorAmount<T>(this T defendItem, short level, float rate)
            where T : IDefendEquipmentItem
        {
            if (defendItem == null || !defendItem.IsDefendEquipment())
                return new KeyValuePair<DamageElement, float>();
            return GameDataHelpers.ToKeyValuePair(defendItem.ArmorAmount, level, rate);
        }
        #endregion

        #region Weapon Extension
        public static WeaponItemEquipType GetEquipType<T>(this T weaponItem)
            where T : IWeaponItem
        {
            if (weaponItem == null || !weaponItem.IsWeapon() || !weaponItem.WeaponType)
                return WeaponItemEquipType.OneHand;
            return weaponItem.WeaponType.EquipType;
        }

        public static KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount<T>(this T weaponItem, short itemLevel, float statsRate, ICharacterData character)
            where T : IWeaponItem
        {
            if (weaponItem == null || !weaponItem.IsWeapon())
                return new KeyValuePair<DamageElement, MinMaxFloat>();
            return GameDataHelpers.ToKeyValuePair(weaponItem.DamageAmount, itemLevel, statsRate, weaponItem.GetEffectivenessDamage(character));
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
            equipType = weaponItem.GetEquipType();
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
        public static void ApplySelfStatusEffectsWhenAttacking<T>(this T socketEnhancerItem, EntityInfo applier, BaseCharacterEntity target)
            where T : ISocketEnhancerItem
        {
            if (target == null || socketEnhancerItem == null || !socketEnhancerItem.IsSocketEnhancer())
                return;
            socketEnhancerItem.SelfStatusEffectsWhenAttacking.ApplyStatusEffect(1, applier, target);
        }

        public static void ApplyEnemyStatusEffectsWhenAttacking<T>(this T socketEnhancerItem, EntityInfo applier, BaseCharacterEntity target)
            where T : ISocketEnhancerItem
        {
            if (target == null || socketEnhancerItem == null || !socketEnhancerItem.IsSocketEnhancer())
                return;
            socketEnhancerItem.EnemyStatusEffectsWhenAttacking.ApplyStatusEffect(1, applier, target);
        }

        public static void ApplySelfStatusEffectsWhenAttacked<T>(this T socketEnhancerItem, EntityInfo applier, BaseCharacterEntity target)
            where T : ISocketEnhancerItem
        {
            if (target == null || socketEnhancerItem == null || !socketEnhancerItem.IsSocketEnhancer())
                return;
            socketEnhancerItem.SelfStatusEffectsWhenAttacked.ApplyStatusEffect(1, applier, target);
        }

        public static void ApplyEnemyStatusEffectsWhenAttacked<T>(this T socketEnhancerItem, EntityInfo applier, BaseCharacterEntity target)
            where T : ISocketEnhancerItem
        {
            if (target == null || socketEnhancerItem == null || !socketEnhancerItem.IsSocketEnhancer())
                return;
            socketEnhancerItem.EnemyStatusEffectsWhenAttacked.ApplyStatusEffect(1, applier, target);
        }
        #endregion

        public static bool CanEquip<T>(this T item, ICharacterData character, short level, out UITextKeys gameMessage)
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
