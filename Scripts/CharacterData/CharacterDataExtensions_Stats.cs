using System.Collections.Generic;
using UnityEngine.Pool;

namespace MultiplayerARPG
{
    public static partial class CharacterDataExtensions
    {
        private static void GetCharacterAttributes(this ICharacterData data, Dictionary<Attribute, float> result)
        {
            result.Clear();
            BaseCharacter database = data.GetDatabase();
            // Attributes from character database
            if (database != null)
                database.GetCharacterAttributes(data.Level, result);
            // Added attributes
            for (int i = 0; i < data.Attributes.Count; ++i)
            {
                Attribute attribute = data.Attributes[i].GetAttribute();
                int amount = data.Attributes[i].amount;
                if (attribute == null)
                    continue;
                if (!result.ContainsKey(attribute))
                    result[attribute] = amount;
                else
                    result[attribute] += amount;
            }
        }

        private static void GetCharacterAttributes(this ICharacterData data, ref AttributeAmounts result)
        {
            result.Clear();
            using (CollectionPool<Dictionary<Attribute, float>, KeyValuePair<Attribute, float>>.Get(out Dictionary<Attribute, float> temporary))
            {
                data.GetCharacterAttributes(temporary);
                result.Combine(temporary);
            }
        }

        private static void GetCharacterSkills(this ICharacterData data, Dictionary<BaseSkill, int> result)
        {
            result.Clear();
            BaseCharacter database = data.GetDatabase();
            // Skills from character database
            if (database != null)
                database.GetSkillLevels(data.Level, result);
            // Combine with skills that character learnt
            for (int i = 0; i < data.Skills.Count; ++i)
            {
                BaseSkill skill = data.Skills[i].GetSkill();
                int level = data.Skills[i].level;
                if (skill == null)
                    continue;
                if (!result.ContainsKey(skill))
                    result[skill] = level;
                else
                    result[skill] += level;
            }
        }

        private static void GetCharacterResistances(this ICharacterData data, Dictionary<DamageElement, float> result)
        {
            result.Clear();
            BaseCharacter database = data.GetDatabase();
            if (database != null)
                database.GetCharacterResistances(data.Level, result);
        }

        private static void GetCharacterResistances(this ICharacterData data, ref DamageElementFloatAmounts result)
        {
            result.Clear();
            using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> temporary))
            {
                data.GetCharacterResistances(temporary);
                result.Combine(temporary);
            }
        }

        private static void GetCharacterArmors(this ICharacterData data, Dictionary<DamageElement, float> result)
        {
            result.Clear();
            BaseCharacter database = data.GetDatabase();
            if (database != null)
                database.GetCharacterArmors(data.Level, result);
        }

        private static void GetCharacterArmors(this ICharacterData data, ref DamageElementFloatAmounts result)
        {
            result.Clear();
            using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> temporary))
            {
                data.GetCharacterArmors(temporary);
                result.Combine(temporary);
            }
        }

        private static void GetCharacterStatusEffectResistances(this ICharacterData data, Dictionary<StatusEffect, float> result)
        {
            result.Clear();
            BaseCharacter database = data.GetDatabase();
            if (database != null)
                database.GetCharacterStatusEffectResistances(data.Level, result);
        }

        private static CharacterStats GetCharacterStats(this ICharacterData data)
        {
            if (data == null)
                return new CharacterStats();
            CharacterStats result = new CharacterStats();
            BaseCharacter database = data.GetDatabase();
            if (database != null)
                result += database.GetCharacterStats(data.Level);
            return result;
        }

        public static void GetBuffs(this ISocketEnhancerItem socketEnhancerItem,
            System.Action<CharacterStats> onIncreasingStats,
            System.Action<CharacterStats> onIncreasingStatsRate,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributes,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributesRate,
            System.Action<Dictionary<DamageElement, float>> onIncreasingResistances,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmors,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmorsRate,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamages,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate,
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkills,
            System.Action<Dictionary<StatusEffect, float>> onIncreasingStatusEffectResistances)
        {
            if (socketEnhancerItem == null)
                return;
            if (onIncreasingStats != null)
                onIncreasingStats.Invoke(socketEnhancerItem.SocketEnhanceEffect.Stats);
            if (onIncreasingStatsRate != null)
                onIncreasingStatsRate.Invoke(socketEnhancerItem.SocketEnhanceEffect.StatsRate);
            if (onIncreasingAttributes != null)
                onIncreasingAttributes.Invoke(socketEnhancerItem.SocketEnhanceEffect.Attributes);
            if (onIncreasingAttributesRate != null)
                onIncreasingAttributesRate.Invoke(socketEnhancerItem.SocketEnhanceEffect.AttributesRate);
            if (onIncreasingResistances != null)
                onIncreasingResistances.Invoke(socketEnhancerItem.SocketEnhanceEffect.Resistances);
            if (onIncreasingArmors != null)
                onIncreasingArmors.Invoke(socketEnhancerItem.SocketEnhanceEffect.Armors);
            if (onIncreasingArmorsRate != null)
                onIncreasingArmorsRate.Invoke(socketEnhancerItem.SocketEnhanceEffect.ArmorsRate);
            if (onIncreasingDamages != null)
                onIncreasingDamages.Invoke(socketEnhancerItem.SocketEnhanceEffect.Damages);
            if (onIncreasingDamagesRate != null)
                onIncreasingDamagesRate.Invoke(socketEnhancerItem.SocketEnhanceEffect.DamagesRate);
            if (onIncreasingSkills != null)
                onIncreasingSkills.Invoke(socketEnhancerItem.SocketEnhanceEffect.Skills);
            if (onIncreasingStatusEffectResistances != null)
                onIncreasingStatusEffectResistances.Invoke(socketEnhancerItem.SocketEnhanceEffect.StatusEffectResistances);
        }

        public static void GetBuffs(this CharacterItem item,
            System.Action<CharacterStats> onIncreasingStats,
            System.Action<CharacterStats> onIncreasingStatsRate,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributes,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributesRate,
            System.Action<Dictionary<DamageElement, float>> onIncreasingResistances,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmors,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmorsRate,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamages,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate,
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkills,
            System.Action<Dictionary<StatusEffect, float>> onIncreasingStatusEffectResistances)
        {
            if (item.IsEmptySlot())
                return;
            IEquipmentItem tempEquipmentItem = item.GetEquipmentItem();
            if (tempEquipmentItem == null)
                return;
            if (onIncreasingStats != null)
                onIncreasingStats.Invoke(item.GetBuff().GetIncreaseStats());
            if (onIncreasingStatsRate != null)
                onIncreasingStatsRate.Invoke(item.GetBuff().GetIncreaseStatsRate());
            if (onIncreasingAttributes != null)
                onIncreasingAttributes.Invoke(item.GetBuff().GetIncreaseAttributes());
            if (onIncreasingAttributesRate != null)
                onIncreasingAttributesRate.Invoke(item.GetBuff().GetIncreaseAttributesRate());
            if (onIncreasingResistances != null)
                onIncreasingResistances.Invoke(item.GetBuff().GetIncreaseResistances());
            if (onIncreasingArmors != null)
                onIncreasingArmors.Invoke(item.GetBuff().GetIncreaseArmors());
            if (onIncreasingArmorsRate != null)
                onIncreasingArmorsRate.Invoke(item.GetBuff().GetIncreaseArmorsRate());
            if (onIncreasingDamages != null)
                onIncreasingDamages.Invoke(item.GetBuff().GetIncreaseDamages());
            if (onIncreasingDamagesRate != null)
                onIncreasingDamagesRate.Invoke(item.GetBuff().GetIncreaseDamagesRate());
            if (onIncreasingSkills != null)
                onIncreasingSkills.Invoke(item.GetBuff().GetIncreaseSkills());
            if (onIncreasingStatusEffectResistances != null)
                onIncreasingStatusEffectResistances.Invoke(item.GetBuff().GetIncreaseStatusEffectResistances());
            BaseItem tempItem;
            int i;
            for (i = 0; i < item.sockets.Count; ++i)
            {
                if (!GameInstance.Items.TryGetValue(item.sockets[i], out tempItem) || !tempItem.IsSocketEnhancer())
                    continue;
                GetBuffs(tempItem as ISocketEnhancerItem,
                    onIncreasingStats,
                    onIncreasingStatsRate,
                    onIncreasingAttributes,
                    onIncreasingAttributesRate,
                    onIncreasingResistances,
                    onIncreasingArmors,
                    onIncreasingArmorsRate,
                    onIncreasingDamages,
                    onIncreasingDamagesRate,
                    onIncreasingSkills,
                    onIncreasingStatusEffectResistances);
            }
        }

        public static void GetBuffs(this EquipmentSet equipmentSet, int setAmount,
            System.Action<CharacterStats> onIncreasingStats,
            System.Action<CharacterStats> onIncreasingStatsRate,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributes,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributesRate,
            System.Action<Dictionary<DamageElement, float>> onIncreasingResistances,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmors,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmorsRate,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamages,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate,
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkills,
            System.Action<Dictionary<StatusEffect, float>> onIncreasingStatusEffectResistances)
        {
            if (equipmentSet == null)
                return;
            EquipmentBonus[] effects = equipmentSet.Effects;
            int i;
            for (i = 0; i < setAmount; ++i)
            {
                if (i < effects.Length)
                {
                    if (onIncreasingStats != null)
                        onIncreasingStats.Invoke(effects[i].Stats);
                    if (onIncreasingStatsRate != null)
                        onIncreasingStatsRate.Invoke(effects[i].StatsRate);
                    if (onIncreasingAttributes != null)
                        onIncreasingAttributes.Invoke(effects[i].Attributes);
                    if (onIncreasingAttributesRate != null)
                        onIncreasingAttributesRate.Invoke(effects[i].AttributesRate);
                    if (onIncreasingResistances != null)
                        onIncreasingResistances.Invoke(effects[i].Resistances);
                    if (onIncreasingArmors != null)
                        onIncreasingArmors.Invoke(effects[i].Armors);
                    if (onIncreasingArmorsRate != null)
                        onIncreasingArmorsRate.Invoke(effects[i].ArmorsRate);
                    if (onIncreasingDamages != null)
                        onIncreasingDamages.Invoke(effects[i].Damages);
                    if (onIncreasingDamagesRate != null)
                        onIncreasingDamagesRate.Invoke(effects[i].DamagesRate);
                    if (onIncreasingSkills != null)
                        onIncreasingSkills.Invoke(effects[i].Skills);
                    if (onIncreasingStatusEffectResistances != null)
                        onIncreasingStatusEffectResistances.Invoke(effects[i].StatusEffectResistances);
                }
                else
                    break;
            }
        }

        public static void GetBuffs(this CharacterBuff buff,
            System.Action<CharacterStats> onIncreasingStats,
            System.Action<CharacterStats> onIncreasingStatsRate,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributes,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributesRate,
            System.Action<Dictionary<DamageElement, float>> onIncreasingResistances,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmors,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmorsRate,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamages,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate,
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkills,
            System.Action<Dictionary<StatusEffect, float>> onIncreasingStatusEffectResistances)
        {
            if (buff.IsEmpty())
                return;
            if (onIncreasingStats != null)
                onIncreasingStats.Invoke(buff.GetBuff().GetIncreaseStats());
            if (onIncreasingStatsRate != null)
                onIncreasingStatsRate.Invoke(buff.GetBuff().GetIncreaseStatsRate());
            if (onIncreasingAttributes != null)
                onIncreasingAttributes.Invoke(buff.GetBuff().GetIncreaseAttributes());
            if (onIncreasingAttributesRate != null)
                onIncreasingAttributesRate.Invoke(buff.GetBuff().GetIncreaseAttributesRate());
            if (onIncreasingResistances != null)
                onIncreasingResistances.Invoke(buff.GetBuff().GetIncreaseResistances());
            if (onIncreasingArmors != null)
                onIncreasingArmors.Invoke(buff.GetBuff().GetIncreaseArmors());
            if (onIncreasingArmorsRate != null)
                onIncreasingArmorsRate.Invoke(buff.GetBuff().GetIncreaseArmorsRate());
            if (onIncreasingDamages != null)
                onIncreasingDamages.Invoke(buff.GetBuff().GetIncreaseDamages());
            if (onIncreasingDamagesRate != null)
                onIncreasingDamagesRate.Invoke(buff.GetBuff().GetIncreaseDamagesRate());
            if (onIncreasingSkills != null)
                onIncreasingSkills.Invoke(buff.GetBuff().GetIncreaseSkills());
            if (onIncreasingStatusEffectResistances != null)
                onIncreasingStatusEffectResistances.Invoke(buff.GetBuff().GetIncreaseStatusEffectResistances());
        }

        public static void GetBuffs(this CharacterSummon summon,
            System.Action<CharacterStats> onIncreasingStats,
            System.Action<CharacterStats> onIncreasingStatsRate,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributes,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributesRate,
            System.Action<Dictionary<DamageElement, float>> onIncreasingResistances,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmors,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmorsRate,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamages,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate,
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkills,
            System.Action<Dictionary<StatusEffect, float>> onIncreasingStatusEffectResistances)
        {
            if (summon.IsEmpty())
                return;
            if (onIncreasingStats != null)
                onIncreasingStats.Invoke(summon.GetBuff().GetIncreaseStats());
            if (onIncreasingStatsRate != null)
                onIncreasingStatsRate.Invoke(summon.GetBuff().GetIncreaseStatsRate());
            if (onIncreasingAttributes != null)
                onIncreasingAttributes.Invoke(summon.GetBuff().GetIncreaseAttributes());
            if (onIncreasingAttributesRate != null)
                onIncreasingAttributesRate.Invoke(summon.GetBuff().GetIncreaseAttributesRate());
            if (onIncreasingResistances != null)
                onIncreasingResistances.Invoke(summon.GetBuff().GetIncreaseResistances());
            if (onIncreasingArmors != null)
                onIncreasingArmors.Invoke(summon.GetBuff().GetIncreaseArmors());
            if (onIncreasingArmorsRate != null)
                onIncreasingArmorsRate.Invoke(summon.GetBuff().GetIncreaseArmorsRate());
            if (onIncreasingDamages != null)
                onIncreasingDamages.Invoke(summon.GetBuff().GetIncreaseDamages());
            if (onIncreasingDamagesRate != null)
                onIncreasingDamagesRate.Invoke(summon.GetBuff().GetIncreaseDamagesRate());
            if (onIncreasingSkills != null)
                onIncreasingSkills.Invoke(summon.GetBuff().GetIncreaseSkills());
            if (onIncreasingStatusEffectResistances != null)
                onIncreasingStatusEffectResistances.Invoke(summon.GetBuff().GetIncreaseStatusEffectResistances());
        }

        public static void GetBuffs(this IVehicleEntity vehicleEntity,
            System.Action<CharacterStats> onIncreasingStats,
            System.Action<CharacterStats> onIncreasingStatsRate,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributes,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributesRate,
            System.Action<Dictionary<DamageElement, float>> onIncreasingResistances,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmors,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmorsRate,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamages,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate,
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkills,
            System.Action<Dictionary<StatusEffect, float>> onIncreasingStatusEffectResistances)
        {
            if (vehicleEntity.IsNull())
                return;
            if (onIncreasingStats != null)
                onIncreasingStats.Invoke(vehicleEntity.GetBuff().GetIncreaseStats());
            if (onIncreasingStatsRate != null)
                onIncreasingStatsRate.Invoke(vehicleEntity.GetBuff().GetIncreaseStatsRate());
            if (onIncreasingAttributes != null)
                onIncreasingAttributes.Invoke(vehicleEntity.GetBuff().GetIncreaseAttributes());
            if (onIncreasingAttributesRate != null)
                onIncreasingAttributesRate.Invoke(vehicleEntity.GetBuff().GetIncreaseAttributesRate());
            if (onIncreasingResistances != null)
                onIncreasingResistances.Invoke(vehicleEntity.GetBuff().GetIncreaseResistances());
            if (onIncreasingArmors != null)
                onIncreasingArmors.Invoke(vehicleEntity.GetBuff().GetIncreaseArmors());
            if (onIncreasingArmorsRate != null)
                onIncreasingArmorsRate.Invoke(vehicleEntity.GetBuff().GetIncreaseArmorsRate());
            if (onIncreasingDamages != null)
                onIncreasingDamages.Invoke(vehicleEntity.GetBuff().GetIncreaseDamages());
            if (onIncreasingDamagesRate != null)
                onIncreasingDamagesRate.Invoke(vehicleEntity.GetBuff().GetIncreaseDamagesRate());
            if (onIncreasingSkills != null)
                onIncreasingSkills.Invoke(vehicleEntity.GetBuff().GetIncreaseSkills());
            if (onIncreasingStatusEffectResistances != null)
                onIncreasingStatusEffectResistances.Invoke(vehicleEntity.GetBuff().GetIncreaseStatusEffectResistances());
        }

        public static void GetBuffs(this PlayerTitle title,
            System.Action<CharacterStats> onIncreasingStats,
            System.Action<CharacterStats> onIncreasingStatsRate,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributes,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributesRate,
            System.Action<Dictionary<DamageElement, float>> onIncreasingResistances,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmors,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmorsRate,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamages,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate,
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkills,
            System.Action<Dictionary<StatusEffect, float>> onIncreasingStatusEffectResistances)
        {
            if (title == null)
                return;
            if (onIncreasingStats != null)
                onIncreasingStats.Invoke(title.CacheBuff.GetIncreaseStats());
            if (onIncreasingStatsRate != null)
                onIncreasingStatsRate.Invoke(title.CacheBuff.GetIncreaseStatsRate());
            if (onIncreasingAttributes != null)
                onIncreasingAttributes.Invoke(title.CacheBuff.GetIncreaseAttributes());
            if (onIncreasingAttributesRate != null)
                onIncreasingAttributesRate.Invoke(title.CacheBuff.GetIncreaseAttributesRate());
            if (onIncreasingResistances != null)
                onIncreasingResistances.Invoke(title.CacheBuff.GetIncreaseResistances());
            if (onIncreasingArmors != null)
                onIncreasingArmors.Invoke(title.CacheBuff.GetIncreaseArmors());
            if (onIncreasingArmorsRate != null)
                onIncreasingArmorsRate.Invoke(title.CacheBuff.GetIncreaseArmorsRate());
            if (onIncreasingDamages != null)
                onIncreasingDamages.Invoke(title.CacheBuff.GetIncreaseDamages());
            if (onIncreasingDamagesRate != null)
                onIncreasingDamagesRate.Invoke(title.CacheBuff.GetIncreaseDamagesRate());
            if (onIncreasingSkills != null)
                onIncreasingSkills.Invoke(title.CacheBuff.GetIncreaseSkills());
            if (onIncreasingStatusEffectResistances != null)
                onIncreasingStatusEffectResistances.Invoke(title.CacheBuff.GetIncreaseStatusEffectResistances());
        }

        public static void GetBuffs(this Faction faction,
            System.Action<CharacterStats> onIncreasingStats,
            System.Action<CharacterStats> onIncreasingStatsRate,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributes,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributesRate,
            System.Action<Dictionary<DamageElement, float>> onIncreasingResistances,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmors,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmorsRate,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamages,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate,
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkills,
            System.Action<Dictionary<StatusEffect, float>> onIncreasingStatusEffectResistances)
        {
            if (faction == null)
                return;
            if (onIncreasingStats != null)
                onIncreasingStats.Invoke(faction.CacheBuff.GetIncreaseStats());
            if (onIncreasingStatsRate != null)
                onIncreasingStatsRate.Invoke(faction.CacheBuff.GetIncreaseStatsRate());
            if (onIncreasingAttributes != null)
                onIncreasingAttributes.Invoke(faction.CacheBuff.GetIncreaseAttributes());
            if (onIncreasingAttributesRate != null)
                onIncreasingAttributesRate.Invoke(faction.CacheBuff.GetIncreaseAttributesRate());
            if (onIncreasingResistances != null)
                onIncreasingResistances.Invoke(faction.CacheBuff.GetIncreaseResistances());
            if (onIncreasingArmors != null)
                onIncreasingArmors.Invoke(faction.CacheBuff.GetIncreaseArmors());
            if (onIncreasingArmorsRate != null)
                onIncreasingArmorsRate.Invoke(faction.CacheBuff.GetIncreaseArmorsRate());
            if (onIncreasingDamages != null)
                onIncreasingDamages.Invoke(faction.CacheBuff.GetIncreaseDamages());
            if (onIncreasingDamagesRate != null)
                onIncreasingDamagesRate.Invoke(faction.CacheBuff.GetIncreaseDamagesRate());
            if (onIncreasingSkills != null)
                onIncreasingSkills.Invoke(faction.CacheBuff.GetIncreaseSkills());
            if (onIncreasingStatusEffectResistances != null)
                onIncreasingStatusEffectResistances.Invoke(faction.CacheBuff.GetIncreaseStatusEffectResistances());
        }

        public static void GetBuffs(this BaseSkill skill, int level,
            System.Action<CharacterStats> onIncreasingStats,
            System.Action<CharacterStats> onIncreasingStatsRate,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributes,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributesRate,
            System.Action<Dictionary<DamageElement, float>> onIncreasingResistances,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmors,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmorsRate,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamages,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate,
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkills,
            System.Action<Dictionary<StatusEffect, float>> onIncreasingStatusEffectResistances)
        {
            if (skill == null)
                return;
            if (!skill.IsPassive)
                return;
            if (level <= 0)
                return;
            if (!skill.TryGetBuff(out Buff buff))
                return;
            if (onIncreasingStats != null)
                onIncreasingStats.Invoke(buff.GetIncreaseStats(level));
            if (onIncreasingStatsRate != null)
                onIncreasingStatsRate.Invoke(buff.GetIncreaseStatsRate(level));
            if (onIncreasingAttributes != null)
            {
                using (CollectionPool<Dictionary<Attribute, float>, KeyValuePair<Attribute, float>>.Get(out Dictionary<Attribute, float> tempAttributes))
                {
                    buff.GetIncreaseAttributes(level, tempAttributes);
                    onIncreasingAttributes.Invoke(tempAttributes);
                }
            }
            if (onIncreasingAttributesRate != null)
            {
                using (CollectionPool<Dictionary<Attribute, float>, KeyValuePair<Attribute, float>>.Get(out Dictionary<Attribute, float> tempAttributes))
                {
                    buff.GetIncreaseAttributesRate(level, tempAttributes);
                    onIncreasingAttributesRate.Invoke(tempAttributes);
                }
            }
            if (onIncreasingResistances != null)
            {
                using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> tempResistances))
                {
                    buff.GetIncreaseResistances(level, tempResistances);
                    onIncreasingResistances.Invoke(tempResistances);
                }
            }
            if (onIncreasingArmors != null)
            {
                using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> tempArmors))
                {
                    buff.GetIncreaseArmors(level, tempArmors);
                    onIncreasingArmors.Invoke(tempArmors);
                }
            }
            if (onIncreasingArmorsRate != null)
            {
                using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> tempArmors))
                {
                    buff.GetIncreaseArmorsRate(level, tempArmors);
                    onIncreasingArmorsRate.Invoke(tempArmors);
                }
            }
            if (onIncreasingDamages != null)
            {
                using (CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Get(out Dictionary<DamageElement, MinMaxFloat> tempDamages))
                {
                    buff.GetIncreaseDamages(level, tempDamages);
                    onIncreasingDamages.Invoke(tempDamages);
                }
            }
            if (onIncreasingDamagesRate != null)
            {
                using (CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Get(out Dictionary<DamageElement, MinMaxFloat> tempDamages))
                {
                    buff.GetIncreaseDamagesRate(level, tempDamages);
                    onIncreasingDamagesRate.Invoke(tempDamages);
                }
            }
            if (onIncreasingSkills != null)
            {
                using (CollectionPool<Dictionary<BaseSkill, int>, KeyValuePair<BaseSkill, int>>.Get(out Dictionary<BaseSkill, int> tempSkills))
                {
                    buff.GetIncreaseSkills(level, tempSkills);
                    onIncreasingSkills.Invoke(tempSkills);
                }
            }
            if (onIncreasingStatusEffectResistances != null)
            {
                using (CollectionPool<Dictionary<StatusEffect, float>, KeyValuePair<StatusEffect, float>>.Get(out Dictionary<StatusEffect, float> tempStatusEffectResistance))
                {
                    buff.GetIncreaseStatusEffectResistances(level, tempStatusEffectResistance);
                    onIncreasingStatusEffectResistances.Invoke(tempStatusEffectResistance);
                }
            }
        }

        public static void GetBuffs(this GuildSkill skill, int level,
            System.Action<CharacterStats> onIncreasingStats,
            System.Action<CharacterStats> onIncreasingStatsRate,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributes,
            System.Action<Dictionary<Attribute, float>> onIncreasingAttributesRate,
            System.Action<Dictionary<DamageElement, float>> onIncreasingResistances,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmors,
            System.Action<Dictionary<DamageElement, float>> onIncreasingArmorsRate,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamages,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate,
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkills,
            System.Action<Dictionary<StatusEffect, float>> onIncreasingStatusEffectResistances)
        {
            if (skill == null)
                return;
            if (!skill.IsPassive)
                return;
            if (level <= 0)
                return;
            Buff buff = skill.Buff;
            if (onIncreasingStats != null)
                onIncreasingStats.Invoke(buff.GetIncreaseStats(level));
            if (onIncreasingStatsRate != null)
                onIncreasingStatsRate.Invoke(buff.GetIncreaseStatsRate(level));
            if (onIncreasingAttributes != null)
            {
                using (CollectionPool<Dictionary<Attribute, float>, KeyValuePair<Attribute, float>>.Get(out Dictionary<Attribute, float> tempAttributes))
                {
                    buff.GetIncreaseAttributes(level, tempAttributes);
                    onIncreasingAttributes.Invoke(tempAttributes);
                }
            }
            if (onIncreasingAttributesRate != null)
            {
                using (CollectionPool<Dictionary<Attribute, float>, KeyValuePair<Attribute, float>>.Get(out Dictionary<Attribute, float> tempAttributes))
                {
                    buff.GetIncreaseAttributesRate(level, tempAttributes);
                    onIncreasingAttributesRate.Invoke(tempAttributes);
                }
            }
            if (onIncreasingResistances != null)
            {
                using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> tempResistances))
                {
                    buff.GetIncreaseResistances(level, tempResistances);
                    onIncreasingResistances.Invoke(tempResistances);
                }
            }
            if (onIncreasingArmors != null)
            {
                using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> tempArmors))
                {
                    buff.GetIncreaseArmors(level, tempArmors);
                    onIncreasingArmors.Invoke(tempArmors);
                }
            }
            if (onIncreasingArmorsRate != null)
            {
                using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> tempArmors))
                {
                    buff.GetIncreaseArmorsRate(level, tempArmors);
                    onIncreasingArmorsRate.Invoke(tempArmors);
                }
            }
            if (onIncreasingDamages != null)
            {
                using (CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Get(out Dictionary<DamageElement, MinMaxFloat> tempDamages))
                {
                    buff.GetIncreaseDamages(level, tempDamages);
                    onIncreasingDamages.Invoke(tempDamages);
                }
            }
            if (onIncreasingDamagesRate != null)
            {
                using (CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Get(out Dictionary<DamageElement, MinMaxFloat> tempDamages))
                {
                    buff.GetIncreaseDamagesRate(level, tempDamages);
                    onIncreasingDamagesRate.Invoke(tempDamages);
                }
            }
            if (onIncreasingSkills != null)
            {
                using (CollectionPool<Dictionary<BaseSkill, int>, KeyValuePair<BaseSkill, int>>.Get(out Dictionary<BaseSkill, int> tempSkills))
                {
                    buff.GetIncreaseSkills(level, tempSkills);
                    onIncreasingSkills.Invoke(tempSkills);
                }
            }
            if (onIncreasingStatusEffectResistances != null)
            {
                using (CollectionPool<Dictionary<StatusEffect, float>, KeyValuePair<StatusEffect, float>>.Get(out Dictionary<StatusEffect, float> tempStatusEffectResistance))
                {
                    buff.GetIncreaseStatusEffectResistances(level, tempStatusEffectResistance);
                    onIncreasingStatusEffectResistances.Invoke(tempStatusEffectResistance);
                }
            }
        }

        public static void GetWeaponDamages(CharacterItem characterItem, IWeaponItem weaponItem, KeyValuePair<DamageElement, MinMaxFloat> weaponDamageAmount,
            AttributeAmounts attributes, DamageElementMinMaxFloatAmounts buffDamages, DamageElementMinMaxFloatAmounts buffDamagesRate, ref DamageElementMinMaxFloatAmounts resultDamages)
        {
            resultDamages.Clear();
            if (weaponItem != null)
                weaponDamageAmount = GameDataHelpers.GetDamageWithEffectiveness(weaponItem.WeaponType.CacheEffectivenessAttributes, attributes, weaponDamageAmount);
            DamageElement element = weaponDamageAmount.Key == null ? GameInstance.Singleton.DefaultDamageElement : weaponDamageAmount.Key;
            resultDamages.Add(RuntimeGameDataSlots.GetSlot(element), weaponDamageAmount.Value);

            DamageElementMinMaxFloatAmounts increaseDamages = default;
            attributes.GetIncreaseDamages(ref increaseDamages);
            increaseDamages.Combine(buffDamages);
            DamageElementMinMaxFloatAmounts multiplyDamages = resultDamages;
            resultDamages.Combine(increaseDamages);
            multiplyDamages.MultiplyRates(buffDamagesRate);
            resultDamages.Combine(multiplyDamages);
        }

        public static void GetAllStats(this ICharacterData data, bool sumWithEquipments, bool sumWithBuffs, bool sumWithSkills,
            System.Action<CharacterStats> onGetStats = null,
            System.Action<Dictionary<Attribute, float>> onGetAttributes = null,
            System.Action<Dictionary<DamageElement, float>> onGetResistances = null,
            System.Action<Dictionary<DamageElement, float>> onGetArmors = null,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onGetRightHandDamages = null,
            System.Action<KeyValuePair<DamageElement, MinMaxFloat>> onGetRightHandWeaponDamage = null,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onGetLeftHandDamages = null,
            System.Action<KeyValuePair<DamageElement, MinMaxFloat>> onGetLeftHandWeaponDamage = null,
            System.Action<Dictionary<BaseSkill, int>> onGetSkills = null,
            System.Action<Dictionary<StatusEffect, float>> onGetStatusEffectResistances = null,
            System.Action<Dictionary<EquipmentSet, int>> onGetEquipmentSets = null,
            System.Action<CharacterStats> onGetIncreasingStats = null,
            System.Action<CharacterStats> onGetIncreasingStatsRate = null,
            System.Action<Dictionary<Attribute, float>> onGetIncreasingAttributes = null,
            System.Action<Dictionary<Attribute, float>> onGetIncreasingAttributesRate = null,
            System.Action<Dictionary<DamageElement, float>> onGetIncreasingResistances = null,
            System.Action<Dictionary<DamageElement, float>> onGetIncreasingArmors = null,
            System.Action<Dictionary<DamageElement, float>> onGetIncreasingArmorsRate = null,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onGetIncreasingDamages = null,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onGetIncreasingDamagesRate = null,
            System.Action<Dictionary<BaseSkill, int>> onGetIncreasingSkills = null,
            System.Action<Dictionary<StatusEffect, float>> onGetIncreasingStatusEffectResistances = null,
            bool willReleaseAttributes = true,
            bool willReleaseResistances = true,
            bool willReleaseArmors = true,
            bool willReleaseStatusEffectResistances = true,
            bool willReleaseSkills = true,
            bool willReleaseRightHandDamages = true,
            bool willReleaseLeftHandDamages = true,
            bool willReleaseEquipmentSets = true,
            bool willReleaseBuffAttributes = true,
            bool willReleaseBuffAttributesRate = true,
            bool willReleaseBuffResistances = true,
            bool willReleaseBuffArmors = true,
            bool willReleaseBuffArmorsRate = true,
            bool willReleaseBuffStatusEffectResistances = true,
            bool willReleaseBuffSkills = true,
            bool willReleaseBuffDamages = true,
            bool willReleaseBuffDamagesRate = true)
        {
            // Prepare result stats, by using character's base stats
            // For weapons it will be based on equipped weapons
            CharacterStats resultStats = data.GetCharacterStats();
            AttributeAmounts resultAttributes = default;
            data.GetCharacterAttributes(ref resultAttributes);
            DamageElementFloatAmounts resultResistances = default;
            data.GetCharacterResistances(ref resultResistances);
            DamageElementFloatAmounts resultArmors = default;
            data.GetCharacterArmors(ref resultArmors);
            Dictionary<StatusEffect, float> resultStatusEffectResistances = CollectionPool<Dictionary<StatusEffect, float>, KeyValuePair<StatusEffect, float>>.Get();
            data.GetCharacterStatusEffectResistances(resultStatusEffectResistances);
            Dictionary<BaseSkill, int> resultSkills = CollectionPool<Dictionary<BaseSkill, int>, KeyValuePair<BaseSkill, int>>.Get();
            data.GetCharacterSkills(resultSkills);
            DamageElementMinMaxFloatAmounts resultRightHandDamages = default;
            DamageElementMinMaxFloatAmounts resultLeftHandDamages = default;
            Dictionary<EquipmentSet, int> resultEquipmentSets = CollectionPool<Dictionary<EquipmentSet, int>, KeyValuePair<EquipmentSet, int>>.Get();

            // Prepare buff stats
            CharacterStats buffStats = new CharacterStats();
            CharacterStats buffStatsRate = new CharacterStats();
            AttributeAmounts buffAttributes = default;
            AttributeAmounts buffAttributesRate = default;
            DamageElementFloatAmounts buffResistances = default;
            DamageElementFloatAmounts buffArmors = default;
            DamageElementFloatAmounts buffArmorsRate = default;
            Dictionary<StatusEffect, float> buffStatusEffectResistances = CollectionPool<Dictionary<StatusEffect, float>, KeyValuePair<StatusEffect, float>>.Get();
            DamageElementMinMaxFloatAmounts buffDamages = default;
            DamageElementMinMaxFloatAmounts buffDamagesRate = default;
            Dictionary<BaseSkill, int> buffSkills = CollectionPool<Dictionary<BaseSkill, int>, KeyValuePair<BaseSkill, int>>.Get();

            // If not found equipped weapon, it will use default weapon which set in game instance as equipped weapon
            bool foundEquippedRightHandWeapon = false;
            IWeaponItem rightHandWeapon = null;
            KeyValuePair<DamageElement, MinMaxFloat> rightHandWeaponDamageAmount = default;
            bool foundEquippedLeftHandWeapon = false;
            IWeaponItem leftHandWeapon = null;
            KeyValuePair<DamageElement, MinMaxFloat> leftHandWeaponDamageAmount = default;

            int i;
            if (sumWithEquipments)
            {
                IEquipmentItem tempEquipmentItem;
                // Equip items
                for (i = 0; i < data.EquipItems.Count; ++i)
                {
                    CharacterItem item = data.EquipItems[i];
                    if (item.IsEmptySlot())
                        continue;
                    tempEquipmentItem = item.GetEquipmentItem();
                    if (tempEquipmentItem == null)
                        continue;
                    if (!item.IsBroken())
                    {
                        resultArmors.Combine(item.GetArmorAmount());
                        GetBuffs(item,
                            stats => buffStats += stats,
                            statsRate => buffStatsRate += statsRate,
                            attributes => buffAttributes.Combine(attributes),
                            attributesRate => buffAttributesRate.Combine(attributesRate),
                            resistances => buffResistances.Combine(resistances),
                            armors => buffArmors.Combine(armors),
                            armorsRate => buffArmorsRate.Combine(armorsRate),
                            damages => buffDamages.Combine(damages),
                            damagesRate => buffDamagesRate.Combine(damagesRate),
                            skills => GameDataHelpers.CombineSkills(buffSkills, skills),
                            statusEffectResistances => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                    }
                    if (tempEquipmentItem.EquipmentSet != null)
                    {
                        if (resultEquipmentSets.ContainsKey(tempEquipmentItem.EquipmentSet))
                            ++resultEquipmentSets[tempEquipmentItem.EquipmentSet];
                        else
                            resultEquipmentSets.Add(tempEquipmentItem.EquipmentSet, 0);
                    }
                }
                // Right hand equipment
                tempEquipmentItem = data.EquipWeapons.GetRightHandEquipmentItem();
                if (tempEquipmentItem != null)
                {
                    foundEquippedRightHandWeapon = tempEquipmentItem.IsWeapon();
                    if (foundEquippedRightHandWeapon)
                    {
                        rightHandWeapon = data.EquipWeapons.rightHand.GetWeaponItem();
                        rightHandWeaponDamageAmount = data.EquipWeapons.rightHand.GetDamageAmount();
                    }
                    if (!data.EquipWeapons.rightHand.IsBroken())
                    {
                        resultArmors.Combine(data.EquipWeapons.rightHand.GetArmorAmount());
                        GetBuffs(data.EquipWeapons.rightHand,
                            stats => buffStats += stats,
                            statsRate => buffStatsRate += statsRate,
                            attributes => buffAttributes.Combine(attributes),
                            attributesRate => buffAttributesRate.Combine(attributesRate),
                            resistances => buffResistances.Combine(resistances),
                            armors => buffArmors.Combine(armors),
                            armorsRate => buffArmorsRate.Combine(armorsRate),
                            damages => buffDamages.Combine(damages),
                            damagesRate => buffDamagesRate.Combine(damagesRate),
                            skills => GameDataHelpers.CombineSkills(buffSkills, skills),
                            statusEffectResistances => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                    }
                    if (tempEquipmentItem.EquipmentSet != null)
                    {
                        if (resultEquipmentSets.ContainsKey(tempEquipmentItem.EquipmentSet))
                            ++resultEquipmentSets[tempEquipmentItem.EquipmentSet];
                        else
                            resultEquipmentSets.Add(tempEquipmentItem.EquipmentSet, 0);
                    }
                }
                // Left hand equipment
                tempEquipmentItem = data.EquipWeapons.GetLeftHandEquipmentItem();
                if (tempEquipmentItem != null)
                {
                    foundEquippedLeftHandWeapon = tempEquipmentItem.IsWeapon();
                    if (foundEquippedLeftHandWeapon)
                    {
                        leftHandWeapon = data.EquipWeapons.leftHand.GetWeaponItem();
                        leftHandWeaponDamageAmount = data.EquipWeapons.leftHand.GetDamageAmount();
                    }
                    if (!data.EquipWeapons.rightHand.IsBroken())
                    {
                        resultArmors.Combine(data.EquipWeapons.leftHand.GetArmorAmount());
                        GetBuffs(data.EquipWeapons.leftHand,
                            stats => buffStats += stats,
                            statsRate => buffStatsRate += statsRate,
                            attributes => buffAttributes.Combine(attributes),
                            attributesRate => buffAttributesRate.Combine(attributesRate),
                            resistances => buffResistances.Combine(resistances),
                            armors => buffArmors.Combine(armors),
                            armorsRate => buffArmorsRate.Combine(armorsRate),
                            damages => buffDamages.Combine(damages),
                            damagesRate => buffDamagesRate.Combine(damagesRate),
                            skills => GameDataHelpers.CombineSkills(buffSkills, skills),
                            statusEffectResistances => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                    }
                    if (tempEquipmentItem.EquipmentSet != null)
                    {
                        if (resultEquipmentSets.ContainsKey(tempEquipmentItem.EquipmentSet))
                            ++resultEquipmentSets[tempEquipmentItem.EquipmentSet];
                        else
                            resultEquipmentSets.Add(tempEquipmentItem.EquipmentSet, 0);
                    }
                }
                // Equipment set
                foreach (var cacheEquipmentSet in resultEquipmentSets)
                {
                    GetBuffs(cacheEquipmentSet.Key, cacheEquipmentSet.Value,
                        stats => buffStats += stats,
                        statsRate => buffStatsRate += statsRate,
                        attributes => buffAttributes.Combine(attributes),
                        attributesRate => buffAttributesRate.Combine(attributesRate),
                        resistances => buffResistances.Combine(resistances),
                        armors => buffArmors.Combine(armors),
                        armorsRate => buffArmorsRate.Combine(armorsRate),
                        damages => buffDamages.Combine(damages),
                        damagesRate => buffDamagesRate.Combine(damagesRate),
                        skills => GameDataHelpers.CombineSkills(buffSkills, skills),
                        statusEffectResistances => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
                // From title
                if (GameInstance.PlayerTitles.TryGetValue(data.TitleDataId, out PlayerTitle title))
                {
                    GetBuffs(title,
                        stats => buffStats += stats,
                        statsRate => buffStatsRate += statsRate,
                        attributes => buffAttributes.Combine(attributes),
                        attributesRate => buffAttributesRate.Combine(attributesRate),
                        resistances => buffResistances.Combine(resistances),
                        armors => buffArmors.Combine(armors),
                        armorsRate => buffArmorsRate.Combine(armorsRate),
                        damages => buffDamages.Combine(damages),
                        damagesRate => buffDamagesRate.Combine(damagesRate),
                        skills => GameDataHelpers.CombineSkills(buffSkills, skills),
                        statusEffectResistances => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
                // From faction
                if (GameInstance.Factions.TryGetValue(data.FactionId, out Faction faction))
                {
                    GetBuffs(faction,
                        stats => buffStats += stats,
                        statsRate => buffStatsRate += statsRate,
                        attributes => buffAttributes.Combine(attributes),
                        attributesRate => buffAttributesRate.Combine(attributesRate),
                        resistances => buffResistances.Combine(resistances),
                        armors => buffArmors.Combine(armors),
                        armorsRate => buffArmorsRate.Combine(armorsRate),
                        damages => buffDamages.Combine(damages),
                        damagesRate => buffDamagesRate.Combine(damagesRate),
                        skills => GameDataHelpers.CombineSkills(buffSkills, skills),
                        statusEffectResistances => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
            }

            // Default weapon
            if (!foundEquippedRightHandWeapon && !foundEquippedLeftHandWeapon)
            {
                BaseCharacter database = data.GetDatabase();
                if (database is MonsterCharacter monsterCharacter)
                {
                    foundEquippedRightHandWeapon = true;
                    DamageElement damageElement = monsterCharacter.DamageAmount.damageElement;
                    if (damageElement == null)
                        damageElement = GameInstance.Singleton.DefaultDamageElement;
                    rightHandWeaponDamageAmount = new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, monsterCharacter.DamageAmount.amount.GetAmount(data.Level));
                }
                else
                {
                    foundEquippedRightHandWeapon = true;
                    CharacterItem fakeDefaultItem = CharacterItem.CreateDefaultWeapon();
                    rightHandWeapon = fakeDefaultItem.GetWeaponItem();
                    rightHandWeaponDamageAmount = fakeDefaultItem.GetDamageAmount();
                    GetBuffs(fakeDefaultItem,
                        stats => buffStats += stats,
                        statsRate => buffStatsRate += statsRate,
                        attributes => buffAttributes.Combine(attributes),
                        attributesRate => buffAttributesRate.Combine(attributesRate),
                        resistances => buffResistances.Combine(resistances),
                        armors => buffArmors.Combine(armors),
                        armorsRate => buffArmorsRate.Combine(armorsRate),
                        damages => buffDamages.Combine(damages),
                        damagesRate => buffDamagesRate.Combine(damagesRate),
                        skills => GameDataHelpers.CombineSkills(buffSkills, skills),
                        statusEffectResistances => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
            }

            if (sumWithBuffs)
            {
                // From buffs
                for (i = 0; i < data.Buffs.Count; ++i)
                {
                    GetBuffs(data.Buffs[i],
                        stats => buffStats += stats,
                        statsRate => buffStatsRate += statsRate,
                        attributes => buffAttributes.Combine(attributes),
                        attributesRate => buffAttributesRate.Combine(attributesRate),
                        resistances => buffResistances.Combine(resistances),
                        armors => buffArmors.Combine(armors),
                        armorsRate => buffArmorsRate.Combine(armorsRate),
                        damages => buffDamages.Combine(damages),
                        damagesRate => buffDamagesRate.Combine(damagesRate),
                        skills => GameDataHelpers.CombineSkills(buffSkills, skills),
                        statusEffectResistances => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
                // From summon
                for (i = 0; i < data.Summons.Count; ++i)
                {
                    GetBuffs(data.Summons[i],
                        stats => buffStats += stats,
                        statsRate => buffStatsRate += statsRate,
                        attributes => buffAttributes.Combine(attributes),
                        attributesRate => buffAttributesRate.Combine(attributesRate),
                        resistances => buffResistances.Combine(resistances),
                        armors => buffArmors.Combine(armors),
                        armorsRate => buffArmorsRate.Combine(armorsRate),
                        damages => buffDamages.Combine(damages),
                        damagesRate => buffDamagesRate.Combine(damagesRate),
                        skills => GameDataHelpers.CombineSkills(buffSkills, skills),
                        statusEffectResistances => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
                if (data is BasePlayerCharacterEntity playerCharacterEntity)
                {
                    // From mount
                    GetBuffs(playerCharacterEntity.PassengingVehicleEntity,
                        stats => buffStats += stats,
                        statsRate => buffStatsRate += statsRate,
                        attributes => buffAttributes.Combine(attributes),
                        attributesRate => buffAttributesRate.Combine(attributesRate),
                        resistances => buffResistances.Combine(resistances),
                        armors => buffArmors.Combine(armors),
                        armorsRate => buffArmorsRate.Combine(armorsRate),
                        damages => buffDamages.Combine(damages),
                        damagesRate => buffDamagesRate.Combine(damagesRate),
                        skills => GameDataHelpers.CombineSkills(buffSkills, skills),
                        statusEffectResistances => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));// Guild skills
                    // Guild skills
                    if (sumWithSkills)
                    {
                        GuildSkill tempGuildSkill;
                        foreach (var guildSkillEntry in playerCharacterEntity.GuildSkills)
                        {
                            if (!GameInstance.GuildSkills.TryGetValue(guildSkillEntry.dataId, out tempGuildSkill))
                                continue;
                            GetBuffs(tempGuildSkill, guildSkillEntry.level,
                                stats => buffStats += stats,
                                statsRate => buffStatsRate += statsRate,
                                attributes => buffAttributes.Combine(attributes),
                                attributesRate => buffAttributesRate.Combine(attributesRate),
                                resistances => buffResistances.Combine(resistances),
                                armors => buffArmors.Combine(armors),
                                armorsRate => buffArmorsRate.Combine(armorsRate),
                                damages => buffDamages.Combine(damages),
                                damagesRate => buffDamagesRate.Combine(damagesRate),
                                skills => GameDataHelpers.CombineSkills(buffSkills, skills),
                                statusEffectResistances => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                        }
                    }
                }
            }

            // Sum skills from base and buffs
            GameDataHelpers.CombineSkills(resultSkills, buffSkills);

            if (sumWithSkills)
            {
                foreach (var skillEntry in resultSkills)
                {
                    GetBuffs(skillEntry.Key, skillEntry.Value,
                        stats => buffStats += stats,
                        statsRate => buffStatsRate += statsRate,
                        attributes => buffAttributes.Combine(attributes),
                        attributesRate => buffAttributesRate.Combine(attributesRate),
                        resistances => buffResistances.Combine(resistances),
                        armors => buffArmors.Combine(armors),
                        armorsRate => buffArmorsRate.Combine(armorsRate),
                        damages => buffDamages.Combine(damages),
                        damagesRate => buffDamagesRate.Combine(damagesRate),
                        skills => GameDataHelpers.CombineSkills(buffSkills, skills),
                        statusEffectResistances => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
            }

            // Attributes result
            resultAttributes.Combine(buffAttributes);
            resultAttributes.ApplyRates(buffAttributesRate);
            resultAttributes.ClampMaximums();
            Dictionary<Attribute, float> resultAttributesDictionary = null;
            if (onGetAttributes != null)
            {
                resultAttributesDictionary = CollectionPool<Dictionary<Attribute, float>, KeyValuePair<Attribute, float>>.Get();
                resultAttributes.CopyTo(resultAttributesDictionary);
                onGetAttributes.Invoke(resultAttributesDictionary);
                // A callback may edit the dictionary before dependent stats are calculated.
                resultAttributes.Clear();
                resultAttributes.Combine(resultAttributesDictionary);
            }

            // Stats result
            resultStats += resultAttributes.GetStats();
            resultStats += buffStats;
            resultStats += resultStats * buffStatsRate;
            if (onGetStats != null)
                onGetStats.Invoke(resultStats);

            // Skills result
            if (onGetSkills != null)
                onGetSkills.Invoke(resultSkills);

            // Resistances result
            DamageElementFloatAmounts increaseResistances = default;
            resultAttributes.GetIncreaseResistances(ref increaseResistances);
            resultResistances.Combine(increaseResistances);
            resultResistances.Combine(buffResistances);
            resultResistances.ClampResistances();
            Dictionary<DamageElement, float> resultResistancesDictionary = null;
            if (onGetResistances != null)
            {
                resultResistancesDictionary = CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get();
                resultResistances.CopyTo(resultResistancesDictionary);
                onGetResistances.Invoke(resultResistancesDictionary);
            }

            // Armors result
            DamageElementFloatAmounts increaseArmors = default;
            resultAttributes.GetIncreaseArmors(ref increaseArmors);
            resultArmors.Combine(increaseArmors);
            resultArmors.Combine(buffArmors);
            resultArmors.ApplyRates(buffArmorsRate);
            Dictionary<DamageElement, float> resultArmorsDictionary = null;
            if (onGetArmors != null)
            {
                resultArmorsDictionary = CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get();
                resultArmors.CopyTo(resultArmorsDictionary);
                onGetArmors.Invoke(resultArmorsDictionary);
            }

            // Right-hand damages result
            if (foundEquippedRightHandWeapon)
            {
                GetWeaponDamages(data.EquipWeapons.rightHand, rightHandWeapon, rightHandWeaponDamageAmount, resultAttributes, buffDamages, buffDamagesRate, ref resultRightHandDamages);
                if (onGetRightHandWeaponDamage != null)
                    onGetRightHandWeaponDamage.Invoke(rightHandWeaponDamageAmount);
            }
            Dictionary<DamageElement, MinMaxFloat> resultRightHandDamagesDictionary = null;
            if (onGetRightHandDamages != null)
            {
                resultRightHandDamagesDictionary = CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Get();
                resultRightHandDamages.CopyTo(resultRightHandDamagesDictionary);
                onGetRightHandDamages.Invoke(resultRightHandDamagesDictionary);
            }

            // Left-hand damages result
            if (foundEquippedLeftHandWeapon)
            {
                GetWeaponDamages(data.EquipWeapons.leftHand, leftHandWeapon, leftHandWeaponDamageAmount, resultAttributes, buffDamages, buffDamagesRate, ref resultLeftHandDamages);
                if (onGetLeftHandWeaponDamage != null)
                    onGetLeftHandWeaponDamage.Invoke(leftHandWeaponDamageAmount);
            }
            Dictionary<DamageElement, MinMaxFloat> resultLeftHandDamagesDictionary = null;
            if (onGetLeftHandDamages != null)
            {
                resultLeftHandDamagesDictionary = CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Get();
                resultLeftHandDamages.CopyTo(resultLeftHandDamagesDictionary);
                onGetLeftHandDamages.Invoke(resultLeftHandDamagesDictionary);
            }

            // Status effect resistances result
            using (CollectionPool<Dictionary<StatusEffect, float>, KeyValuePair<StatusEffect, float>>.Get(out Dictionary<StatusEffect, float> increaseStatusEffectResistances))
            {
                resultAttributes.GetIncreaseStatusEffectResistances(increaseStatusEffectResistances);
                GameDataHelpers.CombineStatusEffectResistances(resultStatusEffectResistances, increaseStatusEffectResistances);
            }
            GameDataHelpers.CombineStatusEffectResistances(resultStatusEffectResistances, buffStatusEffectResistances);
            using (CollectionPool<List<StatusEffect>, StatusEffect>.Get(out List<StatusEffect> statusEffectKeys))
            {
                resultStatusEffectResistances.AddKeysToList(statusEffectKeys);
                for (i = 0; i < statusEffectKeys.Count; ++i)
                {
                    StatusEffect key = statusEffectKeys[i];
                    float value = resultStatusEffectResistances[key];
                    if (value > key.MaxResistanceAmount)
                        resultStatusEffectResistances[key] = key.MaxResistanceAmount;
                }
                if (onGetStatusEffectResistances != null)
                    onGetStatusEffectResistances.Invoke(resultStatusEffectResistances);
            }

            // Equipment sets result
            if (onGetEquipmentSets != null)
                onGetEquipmentSets.Invoke(resultEquipmentSets);

            // Invoke get increase stats actions
            if (onGetIncreasingStats != null)
                onGetIncreasingStats.Invoke(buffStats);
            if (onGetIncreasingStatsRate != null)
                onGetIncreasingStatsRate.Invoke(buffStatsRate);
            Dictionary<Attribute, float> buffAttributesDictionary = null;
            if (onGetIncreasingAttributes != null)
            {
                buffAttributesDictionary = CollectionPool<Dictionary<Attribute, float>, KeyValuePair<Attribute, float>>.Get();
                buffAttributes.CopyTo(buffAttributesDictionary);
                onGetIncreasingAttributes.Invoke(buffAttributesDictionary);
            }
            Dictionary<Attribute, float> buffAttributesRateDictionary = null;
            if (onGetIncreasingAttributesRate != null)
            {
                buffAttributesRateDictionary = CollectionPool<Dictionary<Attribute, float>, KeyValuePair<Attribute, float>>.Get();
                buffAttributesRate.CopyTo(buffAttributesRateDictionary);
                onGetIncreasingAttributesRate.Invoke(buffAttributesRateDictionary);
            }
            Dictionary<DamageElement, float> buffResistancesDictionary = null;
            if (onGetIncreasingResistances != null)
            {
                buffResistancesDictionary = CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get();
                buffResistances.CopyTo(buffResistancesDictionary);
                onGetIncreasingResistances.Invoke(buffResistancesDictionary);
            }
            Dictionary<DamageElement, float> buffArmorsDictionary = null;
            if (onGetIncreasingArmors != null)
            {
                buffArmorsDictionary = CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get();
                buffArmors.CopyTo(buffArmorsDictionary);
                onGetIncreasingArmors.Invoke(buffArmorsDictionary);
            }
            Dictionary<DamageElement, float> buffArmorsRateDictionary = null;
            if (onGetIncreasingArmorsRate != null)
            {
                buffArmorsRateDictionary = CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get();
                buffArmorsRate.CopyTo(buffArmorsRateDictionary);
                onGetIncreasingArmorsRate.Invoke(buffArmorsRateDictionary);
            }
            Dictionary<DamageElement, MinMaxFloat> buffDamagesDictionary = null;
            if (onGetIncreasingDamages != null)
            {
                buffDamagesDictionary = CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Get();
                buffDamages.CopyTo(buffDamagesDictionary);
                onGetIncreasingDamages.Invoke(buffDamagesDictionary);
            }
            Dictionary<DamageElement, MinMaxFloat> buffDamagesRateDictionary = null;
            if (onGetIncreasingDamagesRate != null)
            {
                buffDamagesRateDictionary = CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Get();
                buffDamagesRate.CopyTo(buffDamagesRateDictionary);
                onGetIncreasingDamagesRate.Invoke(buffDamagesRateDictionary);
            }
            if (onGetIncreasingSkills != null)
                onGetIncreasingSkills.Invoke(buffSkills);
            if (onGetIncreasingStatusEffectResistances != null)
                onGetIncreasingStatusEffectResistances.Invoke(buffStatusEffectResistances);

            // Release buffs
            if (willReleaseBuffAttributes && buffAttributesDictionary != null)
                CollectionPool<Dictionary<Attribute, float>, KeyValuePair<Attribute, float>>.Release(buffAttributesDictionary);
            if (willReleaseBuffAttributesRate && buffAttributesRateDictionary != null)
                CollectionPool<Dictionary<Attribute, float>, KeyValuePair<Attribute, float>>.Release(buffAttributesRateDictionary);
            if (willReleaseBuffResistances && buffResistancesDictionary != null)
                CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Release(buffResistancesDictionary);
            if (willReleaseBuffArmors && buffArmorsDictionary != null)
                CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Release(buffArmorsDictionary);
            if (willReleaseBuffArmorsRate && buffArmorsRateDictionary != null)
                CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Release(buffArmorsRateDictionary);
            if (willReleaseBuffStatusEffectResistances)
                CollectionPool<Dictionary<StatusEffect, float>, KeyValuePair<StatusEffect, float>>.Release(buffStatusEffectResistances);
            if (willReleaseBuffSkills)
                CollectionPool<Dictionary<BaseSkill, int>, KeyValuePair<BaseSkill, int>>.Release(buffSkills);
            if (willReleaseBuffDamages && buffDamagesDictionary != null)
                CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Release(buffDamagesDictionary);
            if (willReleaseBuffDamagesRate && buffDamagesRateDictionary != null)
                CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Release(buffDamagesRateDictionary);

            // Release results
            if (willReleaseAttributes && resultAttributesDictionary != null)
                CollectionPool<Dictionary<Attribute, float>, KeyValuePair<Attribute, float>>.Release(resultAttributesDictionary);
            if (willReleaseResistances && resultResistancesDictionary != null)
                CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Release(resultResistancesDictionary);
            if (willReleaseArmors && resultArmorsDictionary != null)
                CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Release(resultArmorsDictionary);
            if (willReleaseStatusEffectResistances)
                CollectionPool<Dictionary<StatusEffect, float>, KeyValuePair<StatusEffect, float>>.Release(resultStatusEffectResistances);
            if (willReleaseSkills)
                CollectionPool<Dictionary<BaseSkill, int>, KeyValuePair<BaseSkill, int>>.Release(resultSkills);
            if (willReleaseRightHandDamages && resultRightHandDamagesDictionary != null)
                CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Release(resultRightHandDamagesDictionary);
            if (willReleaseLeftHandDamages && resultLeftHandDamagesDictionary != null)
                CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Release(resultLeftHandDamagesDictionary);
            if (willReleaseEquipmentSets)
                CollectionPool<Dictionary<EquipmentSet, int>, KeyValuePair<EquipmentSet, int>>.Release(resultEquipmentSets);
        }
    }
}
