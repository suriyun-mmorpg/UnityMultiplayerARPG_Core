using System.Collections.Generic;
using UnityEngine.Pool;

namespace MultiplayerARPG
{
    public static partial class CharacterDataExtensions
    {
        private static Dictionary<Attribute, float> _tempAttributes = new Dictionary<Attribute, float>();
        private static Dictionary<DamageElement, float> _tempResistances = new Dictionary<DamageElement, float>();
        private static Dictionary<DamageElement, float> _tempArmors = new Dictionary<DamageElement, float>();
        private static Dictionary<DamageElement, MinMaxFloat> _tempDamages = new Dictionary<DamageElement, MinMaxFloat>();
        private static Dictionary<BaseSkill, int> _tempSkills = new Dictionary<BaseSkill, int>();
        private static Dictionary<StatusEffect, float> _tempStatusEffectResistance = new Dictionary<StatusEffect, float>();

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

        private static void GetCharacterArmors(this ICharacterData data, Dictionary<DamageElement, float> result)
        {
            result.Clear();
            BaseCharacter database = data.GetDatabase();
            if (database != null)
                database.GetCharacterArmors(data.Level, result);
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
                buff.GetIncreaseAttributes(level, _tempAttributes);
                onIncreasingAttributes.Invoke(_tempAttributes);
            }
            if (onIncreasingAttributesRate != null)
            {
                buff.GetIncreaseAttributesRate(level, _tempAttributes);
                onIncreasingAttributesRate.Invoke(_tempAttributes);
            }
            if (onIncreasingResistances != null)
            {
                buff.GetIncreaseResistances(level, _tempResistances);
                onIncreasingResistances.Invoke(_tempResistances);
            }
            if (onIncreasingArmors != null)
            {
                buff.GetIncreaseArmors(level, _tempArmors);
                onIncreasingArmors.Invoke(_tempArmors);
            }
            if (onIncreasingArmorsRate != null)
            {
                buff.GetIncreaseArmorsRate(level, _tempArmors);
                onIncreasingArmorsRate.Invoke(_tempArmors);
            }
            if (onIncreasingDamages != null)
            {
                buff.GetIncreaseDamages(level, _tempDamages);
                onIncreasingDamages.Invoke(_tempDamages);
            }
            if (onIncreasingDamagesRate != null)
            {
                buff.GetIncreaseDamagesRate(level, _tempDamages);
                onIncreasingDamagesRate.Invoke(_tempDamages);
            }
            if (onIncreasingSkills != null)
            {
                buff.GetIncreaseSkills(level, _tempSkills);
                onIncreasingSkills.Invoke(_tempSkills);
            }
            if (onIncreasingStatusEffectResistances != null)
            {
                buff.GetIncreaseStatusEffectResistances(level, _tempStatusEffectResistance);
                onIncreasingStatusEffectResistances.Invoke(_tempStatusEffectResistance);
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
                buff.GetIncreaseAttributes(level, _tempAttributes);
                onIncreasingAttributes.Invoke(_tempAttributes);
            }
            if (onIncreasingAttributesRate != null)
            {
                buff.GetIncreaseAttributesRate(level, _tempAttributes);
                onIncreasingAttributesRate.Invoke(_tempAttributes);
            }
            if (onIncreasingResistances != null)
            {
                buff.GetIncreaseResistances(level, _tempResistances);
                onIncreasingResistances.Invoke(_tempResistances);
            }
            if (onIncreasingArmors != null)
            {
                buff.GetIncreaseArmors(level, _tempArmors);
                onIncreasingArmors.Invoke(_tempArmors);
            }
            if (onIncreasingArmorsRate != null)
            {
                buff.GetIncreaseArmorsRate(level, _tempArmors);
                onIncreasingArmorsRate.Invoke(_tempArmors);
            }
            if (onIncreasingDamages != null)
            {
                buff.GetIncreaseDamages(level, _tempDamages);
                onIncreasingDamages.Invoke(_tempDamages);
            }
            if (onIncreasingDamagesRate != null)
            {
                buff.GetIncreaseDamagesRate(level, _tempDamages);
                onIncreasingDamagesRate.Invoke(_tempDamages);
            }
            if (onIncreasingSkills != null)
            {
                buff.GetIncreaseSkills(level, _tempSkills);
                onIncreasingSkills.Invoke(_tempSkills);
            }
            if (onIncreasingStatusEffectResistances != null)
            {
                buff.GetIncreaseStatusEffectResistances(level, _tempStatusEffectResistance);
                onIncreasingStatusEffectResistances.Invoke(_tempStatusEffectResistance);
            }
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetWeaponDamages(CharacterItem characterItem, IWeaponItem weaponItem, KeyValuePair<DamageElement, MinMaxFloat> weaponDamageAmount,
            Dictionary<Attribute, float> attributes, Dictionary<DamageElement, MinMaxFloat> buffDamages, Dictionary<DamageElement, MinMaxFloat> buffDamagesRate)
        {
            Dictionary<DamageElement, MinMaxFloat> resultDamages = new Dictionary<DamageElement, MinMaxFloat>();
            if (weaponItem != null)
                weaponDamageAmount = GameDataHelpers.GetDamageWithEffectiveness(weaponItem.WeaponType.CacheEffectivenessAttributes, attributes, weaponDamageAmount);
            GameDataHelpers.CombineDamages(resultDamages, weaponDamageAmount);
            using (CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Get(out Dictionary<DamageElement, MinMaxFloat> increaseDamages))
            {
                attributes.GetIncreaseDamages(increaseDamages);
                GameDataHelpers.CombineDamages(resultDamages, increaseDamages);
            }
            GameDataHelpers.CombineDamages(resultDamages, buffDamages);
            using (CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Get(out Dictionary<DamageElement, MinMaxFloat> multiplyDamages))
            {
                GameDataHelpers.CombineDamages(multiplyDamages, resultDamages);
                GameDataHelpers.MultiplyDamages(multiplyDamages, buffDamagesRate);
                GameDataHelpers.CombineDamages(resultDamages, multiplyDamages);
            }
            /*
            // Sum with ammo
            if (weaponItem != null)
            {
                // Ammo stored in magazine?
                if (weaponItem.AmmoCapacity > 0)
                {
                    // Sum with ammo only when it have ammo in magazine
                    if (characterItem.ammo > 0 && GameInstance.Items.TryGetValue(characterItem.ammoDataId, out BaseItem tempItemData) && tempItemData is IAmmoItem tempAmmoItem)
                    {
                        tempAmmoItem.GetIncreaseDamages(_tempDamages);
                        GameDataHelpers.CombineDamages(resultDamages, _tempDamages);
                    }
                }
                else
                {
                    // No special condition, just sum with ammo
                    if (GameInstance.Items.TryGetValue(characterItem.ammoDataId, out BaseItem tempItemData) && tempItemData is IAmmoItem tempAmmoItem)
                    {
                        tempAmmoItem.GetIncreaseDamages(_tempDamages);
                        GameDataHelpers.CombineDamages(resultDamages, _tempDamages);
                    }
                }
            }
            */
            return resultDamages;
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
            System.Action<Dictionary<StatusEffect, float>> onGetIncreasingStatusEffectResistances = null)
        {
            bool isCalculateRightHandWeaponDamages = onGetRightHandDamages != null || onGetRightHandWeaponDamage != null;
            bool isCalculateLeftHandWeaponDamages = onGetLeftHandDamages != null || onGetLeftHandWeaponDamage != null;
            bool isCalculateDamages = isCalculateRightHandWeaponDamages || isCalculateLeftHandWeaponDamages || onGetIncreasingDamages != null || onGetIncreasingDamagesRate != null;
            bool isCalculateStats = onGetStats != null || onGetIncreasingStats != null || onGetIncreasingStatsRate != null;
            bool isCalculateResistances = onGetResistances != null || onGetIncreasingResistances != null;
            bool isCalculateArmors = onGetArmors != null || onGetIncreasingArmors != null || onGetIncreasingArmorsRate != null;
            bool isCalculateAttributes = onGetAttributes != null || onGetIncreasingAttributes != null || onGetIncreasingAttributesRate != null || isCalculateDamages || isCalculateStats || isCalculateResistances || isCalculateArmors;
            bool isCalculateStatusEffectResistances = onGetStatusEffectResistances != null || onGetIncreasingStatusEffectResistances != null;
            bool isCalculateSkills = onGetSkills != null || onGetIncreasingSkills != null || isCalculateDamages || isCalculateStats || isCalculateResistances || isCalculateArmors || isCalculateAttributes || isCalculateStatusEffectResistances;

            // Prepare result stats, by using character's base stats
            // For weapons it will be based on equipped weapons
            CharacterStats resultStats = !isCalculateStats ? new CharacterStats() : data.GetCharacterStats();
            Dictionary<Attribute, float> resultAttributes = new Dictionary<Attribute, float>();
            if (isCalculateAttributes)
                data.GetCharacterAttributes(resultAttributes);
            Dictionary<DamageElement, float> resultResistances = new Dictionary<DamageElement, float>();
            if (isCalculateResistances)
                data.GetCharacterResistances(resultResistances);
            Dictionary<DamageElement, float> resultArmors = new Dictionary<DamageElement, float>();
            if (isCalculateArmors)
                data.GetCharacterArmors(resultArmors);
            Dictionary<StatusEffect, float> resultStatusEffectResistances = new Dictionary<StatusEffect, float>();
            if (isCalculateStatusEffectResistances)
                data.GetCharacterStatusEffectResistances(resultStatusEffectResistances);
            Dictionary<BaseSkill, int> resultSkills = new Dictionary<BaseSkill, int>();
            if (isCalculateSkills)
                data.GetCharacterSkills(resultSkills);
            Dictionary<DamageElement, MinMaxFloat> resultRightHandDamages = new Dictionary<DamageElement, MinMaxFloat>();
            Dictionary<DamageElement, MinMaxFloat> resultLeftHandDamages = new Dictionary<DamageElement, MinMaxFloat>();
            Dictionary<EquipmentSet, int> resultEquipmentSets = new Dictionary<EquipmentSet, int>();

            // Prepare buff stats
            CharacterStats buffStats = new CharacterStats();
            CharacterStats buffStatsRate = new CharacterStats();
            Dictionary<Attribute, float> buffAttributes = new Dictionary<Attribute, float>();
            Dictionary<Attribute, float> buffAttributesRate = new Dictionary<Attribute, float>();
            Dictionary<DamageElement, float> buffResistances = new Dictionary<DamageElement, float>();
            Dictionary<DamageElement, float> buffArmors = new Dictionary<DamageElement, float>();
            Dictionary<DamageElement, float> buffArmorsRate = new Dictionary<DamageElement, float>();
            Dictionary<DamageElement, MinMaxFloat> buffDamages = new Dictionary<DamageElement, MinMaxFloat>();
            Dictionary<DamageElement, MinMaxFloat> buffDamagesRate = new Dictionary<DamageElement, MinMaxFloat>();
            Dictionary<BaseSkill, int> buffSkills = new Dictionary<BaseSkill, int>();
            Dictionary<StatusEffect, float> buffStatusEffectResistances = new Dictionary<StatusEffect, float>();

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
                    if (data.EquipItems[i].IsEmptySlot())
                        continue;
                    tempEquipmentItem = data.EquipItems[i].GetEquipmentItem();
                    if (tempEquipmentItem == null)
                        continue;
                    GameDataHelpers.CombineArmors(resultArmors, data.EquipItems[i].GetArmorAmount());
                    GetBuffs(data.EquipItems[i],
                        !isCalculateStats ? null : (stats) => buffStats += stats,
                        !isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        !isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        !isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        !isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        !isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        !isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        !isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        !isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        !isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills),
                        !isCalculateStatusEffectResistances ? null : (statusEffectResistances) => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
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
                    GameDataHelpers.CombineArmors(resultArmors, data.EquipWeapons.rightHand.GetArmorAmount());
                    GetBuffs(data.EquipWeapons.rightHand,
                        !isCalculateStats ? null : (stats) => buffStats += stats,
                        !isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        !isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        !isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        !isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        !isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        !isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        !isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        !isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        !isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills),
                        !isCalculateStatusEffectResistances ? null : (statusEffectResistances) => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
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
                    GameDataHelpers.CombineArmors(resultArmors, data.EquipWeapons.leftHand.GetArmorAmount());
                    GetBuffs(data.EquipWeapons.leftHand,
                        !isCalculateStats ? null : (stats) => buffStats += stats,
                        !isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        !isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        !isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        !isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        !isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        !isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        !isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        !isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        !isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills),
                        !isCalculateStatusEffectResistances ? null : (statusEffectResistances) => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
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
                        !isCalculateStats ? null : (stats) => buffStats += stats,
                        !isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        !isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        !isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        !isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        !isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        !isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        !isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        !isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        !isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills),
                        !isCalculateStatusEffectResistances ? null : (statusEffectResistances) => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
                // From title
                if (GameInstance.PlayerTitles.TryGetValue(data.TitleDataId, out PlayerTitle title))
                {
                    GetBuffs(title,
                        !isCalculateStats ? null : (stats) => buffStats += stats,
                        !isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        !isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        !isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        !isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        !isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        !isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        !isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        !isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        !isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills),
                        !isCalculateStatusEffectResistances ? null : (statusEffectResistances) => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
                // From faction
                if (GameInstance.Factions.TryGetValue(data.FactionId, out Faction faction))
                {
                    GetBuffs(faction,
                        !isCalculateStats ? null : (stats) => buffStats += stats,
                        !isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        !isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        !isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        !isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        !isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        !isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        !isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        !isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        !isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills),
                        !isCalculateStatusEffectResistances ? null : (statusEffectResistances) => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
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
                        !isCalculateStats ? null : (stats) => buffStats += stats,
                        !isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        !isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        !isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        !isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        !isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        !isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        !isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        !isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        !isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills),
                        !isCalculateStatusEffectResistances ? null : (statusEffectResistances) => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
            }

            if (sumWithBuffs)
            {
                // From buffs
                for (i = 0; i < data.Buffs.Count; ++i)
                {
                    GetBuffs(data.Buffs[i],
                        !isCalculateStats ? null : (stats) => buffStats += stats,
                        !isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        !isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        !isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        !isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        !isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        !isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        !isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        !isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        !isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills),
                        !isCalculateStatusEffectResistances ? null : (statusEffectResistances) => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
                // From summon
                for (i = 0; i < data.Summons.Count; ++i)
                {
                    GetBuffs(data.Summons[i],
                        !isCalculateStats ? null : (stats) => buffStats += stats,
                        !isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        !isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        !isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        !isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        !isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        !isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        !isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        !isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        !isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills),
                        !isCalculateStatusEffectResistances ? null : (statusEffectResistances) => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
                if (data is BasePlayerCharacterEntity playerCharacterEntity)
                {
                    // From mount
                    GetBuffs(playerCharacterEntity.PassengingVehicleEntity,
                        !isCalculateStats ? null : (stats) => buffStats += stats,
                        !isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        !isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        !isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        !isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        !isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        !isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        !isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        !isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        !isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills),
                        !isCalculateStatusEffectResistances ? null : (statusEffectResistances) => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));// Guild skills
                    // Guild skills
                    if (sumWithSkills)
                    {
                        GuildSkill tempGuildSkill;
                        foreach (var guildSkillEntry in playerCharacterEntity.GuildSkills)
                        {
                            if (!GameInstance.GuildSkills.TryGetValue(guildSkillEntry.dataId, out tempGuildSkill))
                                continue;
                            GetBuffs(tempGuildSkill, guildSkillEntry.level,
                                !isCalculateStats ? null : (stats) => buffStats += stats,
                                !isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                                !isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                                !isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                                !isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                                !isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                                !isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                                !isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                                !isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                                !isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills),
                                !isCalculateStatusEffectResistances ? null : (statusEffectResistances) => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
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
                        !isCalculateStats ? null : (stats) => buffStats += stats,
                        !isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        !isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        !isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        !isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        !isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        !isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        !isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        !isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        !isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills),
                        !isCalculateStatusEffectResistances ? null : (statusEffectResistances) => GameDataHelpers.CombineStatusEffectResistances(buffStatusEffectResistances, statusEffectResistances));
                }
            }

            // Calculate stats by buffs
            if (isCalculateAttributes)
            {
                GameDataHelpers.CombineAttributes(resultAttributes, buffAttributes);
                using (CollectionPool<Dictionary<Attribute, float>, KeyValuePair<Attribute, float>>.Get(out Dictionary<Attribute, float> multiplyAttributes))
                {
                    GameDataHelpers.CombineAttributes(multiplyAttributes, resultAttributes);
                    GameDataHelpers.MultiplyAttributes(multiplyAttributes, buffAttributesRate);
                    GameDataHelpers.CombineAttributes(resultAttributes, multiplyAttributes);
                }
                List<Attribute> keys = new List<Attribute>(resultAttributes.Keys);
                for (i = 0; i < keys.Count; ++i)
                {
                    if (keys[i].MaxAmount <= 0)
                        continue;
                    if (resultAttributes[keys[i]] > keys[i].MaxAmount)
                        resultAttributes[keys[i]] = keys[i].MaxAmount;
                }
                if (onGetAttributes != null)
                    onGetAttributes.Invoke(resultAttributes);
            }
            if (isCalculateResistances)
            {
                using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> increaseResistances))
                {
                    resultAttributes.GetIncreaseResistances(increaseResistances);
                    GameDataHelpers.CombineResistances(resultResistances, increaseResistances);
                }
                GameDataHelpers.CombineResistances(resultResistances, buffResistances);
                List<DamageElement> keys = new List<DamageElement>(resultResistances.Keys);
                for (i = 0; i < keys.Count; ++i)
                {
                    if (resultResistances[keys[i]] > keys[i].MaxResistanceAmount)
                        resultResistances[keys[i]] = keys[i].MaxResistanceAmount;
                }
                if (onGetResistances != null)
                    onGetResistances.Invoke(resultResistances);
            }
            if (isCalculateArmors)
            {
                using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> increaseArmors))
                {
                    resultAttributes.GetIncreaseArmors(increaseArmors);
                    GameDataHelpers.CombineArmors(resultArmors, increaseArmors);
                }
                GameDataHelpers.CombineArmors(resultArmors, buffArmors);
                using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> multiplyArmors))
                {
                    GameDataHelpers.CombineArmors(multiplyArmors, resultArmors);
                    GameDataHelpers.MultiplyArmors(multiplyArmors, buffArmorsRate);
                    GameDataHelpers.CombineArmors(resultArmors, multiplyArmors);
                }
                if (onGetArmors != null)
                    onGetArmors.Invoke(resultArmors);
            }
            if (isCalculateRightHandWeaponDamages && foundEquippedRightHandWeapon)
            {
                resultRightHandDamages = GetWeaponDamages(data.EquipWeapons.rightHand, rightHandWeapon, rightHandWeaponDamageAmount, resultAttributes, buffDamages, buffDamagesRate);
                if (onGetRightHandDamages != null)
                    onGetRightHandDamages.Invoke(resultRightHandDamages);
                if (onGetRightHandWeaponDamage != null)
                    onGetRightHandWeaponDamage.Invoke(rightHandWeaponDamageAmount);
            }
            if (isCalculateLeftHandWeaponDamages && foundEquippedLeftHandWeapon)
            {
                resultLeftHandDamages = GetWeaponDamages(data.EquipWeapons.leftHand, leftHandWeapon, leftHandWeaponDamageAmount, resultAttributes, buffDamages, buffDamagesRate);
                if (onGetLeftHandDamages != null)
                    onGetLeftHandDamages.Invoke(resultLeftHandDamages);
                if (onGetLeftHandWeaponDamage != null)
                    onGetLeftHandWeaponDamage.Invoke(leftHandWeaponDamageAmount);
            }
            if (isCalculateStats)
            {
                resultStats += resultAttributes.GetStats();
                resultStats += buffStats;
                resultStats += resultStats * buffStatsRate;
                if (onGetStats != null)
                    onGetStats.Invoke(resultStats);
            }
            if (isCalculateSkills)
            {
                if (onGetSkills != null)
                    onGetSkills.Invoke(resultSkills);
            }
            if (isCalculateStatusEffectResistances)
            {
                using (CollectionPool<Dictionary<StatusEffect, float>, KeyValuePair<StatusEffect, float>>.Get(out Dictionary<StatusEffect, float> increaseStatusEffectResistances))
                {
                    resultAttributes.GetIncreaseStatusEffectResistances(increaseStatusEffectResistances);
                    GameDataHelpers.CombineStatusEffectResistances(resultStatusEffectResistances, increaseStatusEffectResistances);
                }
                GameDataHelpers.CombineStatusEffectResistances(resultStatusEffectResistances, buffStatusEffectResistances);
                List<StatusEffect> keys = new List<StatusEffect>(resultStatusEffectResistances.Keys);
                for (i = 0; i < keys.Count; ++i)
                {
                    if (resultStatusEffectResistances[keys[i]] > keys[i].MaxResistanceAmount)
                        resultStatusEffectResistances[keys[i]] = keys[i].MaxResistanceAmount;
                }
                if (onGetStatusEffectResistances != null)
                    onGetStatusEffectResistances.Invoke(resultStatusEffectResistances);
            }

            if (onGetEquipmentSets != null)
                onGetEquipmentSets.Invoke(resultEquipmentSets);

            // Invoke get increase stats actions
            if (onGetIncreasingStats != null)
                onGetIncreasingStats.Invoke(buffStats);
            if (onGetIncreasingStatsRate != null)
                onGetIncreasingStatsRate.Invoke(buffStatsRate);
            if (onGetIncreasingAttributes != null)
                onGetIncreasingAttributes.Invoke(buffAttributes);
            if (onGetIncreasingAttributesRate != null)
                onGetIncreasingAttributesRate.Invoke(buffAttributesRate);
            if (onGetIncreasingResistances != null)
                onGetIncreasingResistances.Invoke(buffResistances);
            if (onGetIncreasingArmors != null)
                onGetIncreasingArmors.Invoke(buffArmors);
            if (onGetIncreasingArmorsRate != null)
                onGetIncreasingArmorsRate.Invoke(buffArmorsRate);
            if (onGetIncreasingDamages != null)
                onGetIncreasingDamages.Invoke(buffDamages);
            if (onGetIncreasingDamagesRate != null)
                onGetIncreasingDamagesRate.Invoke(buffDamagesRate);
            if (onGetIncreasingSkills != null)
                onGetIncreasingSkills.Invoke(buffSkills);
            if (onGetIncreasingStatusEffectResistances != null)
                onGetIncreasingStatusEffectResistances.Invoke(buffStatusEffectResistances);
        }
    }
}
