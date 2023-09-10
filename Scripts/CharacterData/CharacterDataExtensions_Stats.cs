using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static partial class CharacterDataExtensions
    {
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
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkill)
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
            if (onIncreasingSkill != null)
                onIncreasingSkill.Invoke(item.GetBuff().GetIncreaseSkills());
            BaseItem tempItem;
            int i;
            int tempSocketId;
            ISocketEnhancerItem tempSocketEnhancerItem;
            for (i = 0; i < item.Sockets.Count; ++i)
            {
                tempSocketId = item.Sockets[i];
                if (!GameInstance.Items.TryGetValue(tempSocketId, out tempItem) || !tempItem.IsSocketEnhancer())
                    continue;
                tempSocketEnhancerItem = tempItem as ISocketEnhancerItem;
                if (onIncreasingStats != null)
                    onIncreasingStats.Invoke(tempSocketEnhancerItem.SocketEnhanceEffect.stats);
                if (onIncreasingStatsRate != null)
                    onIncreasingStatsRate.Invoke(tempSocketEnhancerItem.SocketEnhanceEffect.statsRate);
                if (onIncreasingAttributes != null)
                    onIncreasingAttributes.Invoke(GameDataHelpers.CombineAttributes(tempSocketEnhancerItem.SocketEnhanceEffect.attributes, new Dictionary<Attribute, float>(), 1f));
                if (onIncreasingAttributesRate != null)
                    onIncreasingAttributesRate.Invoke(GameDataHelpers.CombineAttributes(tempSocketEnhancerItem.SocketEnhanceEffect.attributesRate, new Dictionary<Attribute, float>(), 1f));
                if (onIncreasingResistances != null)
                    onIncreasingResistances.Invoke(GameDataHelpers.CombineResistances(tempSocketEnhancerItem.SocketEnhanceEffect.resistances, new Dictionary<DamageElement, float>(), 1f));
                if (onIncreasingArmors != null)
                    onIncreasingArmors.Invoke(GameDataHelpers.CombineArmors(tempSocketEnhancerItem.SocketEnhanceEffect.armors, new Dictionary<DamageElement, float>(), 1f));
                if (onIncreasingArmorsRate != null)
                    onIncreasingArmorsRate.Invoke(GameDataHelpers.CombineArmors(tempSocketEnhancerItem.SocketEnhanceEffect.armorsRate, new Dictionary<DamageElement, float>(), 1f));
                if (onIncreasingDamages != null)
                    onIncreasingDamages.Invoke(GameDataHelpers.CombineDamages(tempSocketEnhancerItem.SocketEnhanceEffect.damages, new Dictionary<DamageElement, MinMaxFloat>(), 1f));
                if (onIncreasingDamagesRate != null)
                    onIncreasingDamagesRate.Invoke(GameDataHelpers.CombineDamages(tempSocketEnhancerItem.SocketEnhanceEffect.damagesRate, new Dictionary<DamageElement, MinMaxFloat>(), 1f));
                if (onIncreasingSkill != null)
                    onIncreasingSkill.Invoke(GameDataHelpers.CombineSkills(tempSocketEnhancerItem.SocketEnhanceEffect.skills, new Dictionary<BaseSkill, int>(), 1f));
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
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkill)
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
                        onIncreasingStats.Invoke(effects[i].stats);
                    if (onIncreasingStatsRate != null)
                        onIncreasingStatsRate.Invoke(effects[i].statsRate);
                    if (onIncreasingAttributes != null)
                        onIncreasingAttributes.Invoke(GameDataHelpers.CombineAttributes(effects[i].attributes, new Dictionary<Attribute, float>(), 1f));
                    if (onIncreasingAttributesRate != null)
                        onIncreasingAttributesRate.Invoke(GameDataHelpers.CombineAttributes(effects[i].attributesRate, new Dictionary<Attribute, float>(), 1f));
                    if (onIncreasingResistances != null)
                        onIncreasingResistances.Invoke(GameDataHelpers.CombineResistances(effects[i].resistances, new Dictionary<DamageElement, float>(), 1f));
                    if (onIncreasingArmors != null)
                        onIncreasingArmors.Invoke(GameDataHelpers.CombineArmors(effects[i].armors, new Dictionary<DamageElement, float>(), 1f));
                    if (onIncreasingArmorsRate != null)
                        onIncreasingArmorsRate.Invoke(GameDataHelpers.CombineArmors(effects[i].armorsRate, new Dictionary<DamageElement, float>(), 1f));
                    if (onIncreasingDamages != null)
                        onIncreasingDamages.Invoke(GameDataHelpers.CombineDamages(effects[i].damages, new Dictionary<DamageElement, MinMaxFloat>(), 1f));
                    if (onIncreasingDamagesRate != null)
                        onIncreasingDamagesRate.Invoke(GameDataHelpers.CombineDamages(effects[i].damagesRate, new Dictionary<DamageElement, MinMaxFloat>(), 1f));
                    if (onIncreasingSkill != null)
                        onIncreasingSkill.Invoke(GameDataHelpers.CombineSkills(effects[i].skills, new Dictionary<BaseSkill, int>(), 1f));
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
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate)
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
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate)
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
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate)
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
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onIncreasingDamagesRate)
        {
            if (skill == null)
                return;
            if (!skill.IsPassive)
                return;
            if (level <= 0)
                return;
            if (onIncreasingStats != null)
                onIncreasingStats.Invoke(skill.Buff.GetIncreaseStats(level));
            if (onIncreasingStatsRate != null)
                onIncreasingStatsRate.Invoke(skill.Buff.GetIncreaseStatsRate(level));
            if (onIncreasingAttributes != null)
                onIncreasingAttributes.Invoke(skill.Buff.GetIncreaseAttributes(level));
            if (onIncreasingAttributesRate != null)
                onIncreasingAttributesRate.Invoke(skill.Buff.GetIncreaseAttributesRate(level));
            if (onIncreasingResistances != null)
                onIncreasingResistances.Invoke(skill.Buff.GetIncreaseResistances(level));
            if (onIncreasingArmors != null)
                onIncreasingArmors.Invoke(skill.Buff.GetIncreaseArmors(level));
            if (onIncreasingArmorsRate != null)
                onIncreasingArmorsRate.Invoke(skill.Buff.GetIncreaseArmorsRate(level));
            if (onIncreasingDamages != null)
                onIncreasingDamages.Invoke(skill.Buff.GetIncreaseDamages(level));
            if (onIncreasingDamagesRate != null)
                onIncreasingDamagesRate.Invoke(skill.Buff.GetIncreaseDamagesRate(level));
        }

        public static void GetBuffs(this ICharacterData data, bool sumWithEquipments, bool sumWithSkills, bool sumWithBuffs,
            System.Action<CharacterStats> onGetStats,
            System.Action<CharacterStats> onGetStatsRate,
            System.Action<Dictionary<Attribute, float>> onGetAttributes,
            System.Action<Dictionary<Attribute, float>> onGetAttributesRate,
            System.Action<Dictionary<DamageElement, float>> onGetResistances,
            System.Action<Dictionary<DamageElement, float>> onGetArmors,
            System.Action<Dictionary<DamageElement, float>> onGetArmorsRate,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onGetDamages,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onGetDamagesRate,
            System.Action<Dictionary<BaseSkill, int>> onGetSkills,
            System.Action<Dictionary<EquipmentSet, int>> onGetEquipmentSets)
        {
            bool isCalculateStats = onGetStats != null;
            bool isCalculateStatsRate = onGetStatsRate != null;
            bool isCalculateAttributes = onGetAttributes != null;
            bool isCalculateAttributesRate = onGetAttributesRate != null;
            bool isCalculateResistances = onGetResistances != null;
            bool isCalculateArmors = onGetArmors != null;
            bool isCalculateArmorsRate = onGetArmorsRate != null;
            bool isCalculateDamages = onGetDamages != null;
            bool isCalculateDamagesRate = onGetDamagesRate != null;
            bool isCalculateSkills = onGetSkills != null;
            bool isCalculateEquipmentSets = onGetEquipmentSets != null;

            CharacterStats resultStats = new CharacterStats();
            CharacterStats resultStatsRate = new CharacterStats();
            Dictionary<Attribute, float> resultAttributes = new Dictionary<Attribute, float>();
            Dictionary<Attribute, float> resultAttributesRate = new Dictionary<Attribute, float>();
            Dictionary<DamageElement, float> resultResistances = new Dictionary<DamageElement, float>();
            Dictionary<DamageElement, float> resultArmors = new Dictionary<DamageElement, float>();
            Dictionary<DamageElement, float> resultArmorsRate = new Dictionary<DamageElement, float>();
            Dictionary<DamageElement, MinMaxFloat> resultDamages = new Dictionary<DamageElement, MinMaxFloat>();
            Dictionary<DamageElement, MinMaxFloat> resultDamagesRate = new Dictionary<DamageElement, MinMaxFloat>();
            Dictionary<BaseSkill, int> resultSkills = new Dictionary<BaseSkill, int>();
            Dictionary<EquipmentSet, int> resultEquipmentSets = new Dictionary<EquipmentSet, int>();
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
                    GetBuffs(data.EquipItems[i],
                        isCalculateStats ? null : (stats) => resultStats += stats,
                        isCalculateStatsRate ? null : (statsRate) => resultStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(resultAttributes, attributes),
                        isCalculateAttributesRate ? null : (attributesRate) => GameDataHelpers.CombineAttributes(resultAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(resultResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(resultArmors, armors),
                        isCalculateArmorsRate ? null : (armorsRate) => GameDataHelpers.CombineArmors(resultArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(resultDamages, damages),
                        isCalculateDamagesRate ? null : (damagesRate) => GameDataHelpers.CombineDamages(resultDamagesRate, damagesRate),
                        isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(resultSkills, skills));
                    if (tempEquipmentItem.EquipmentSet != null)
                    {
                        if (resultEquipmentSets.ContainsKey(tempEquipmentItem.EquipmentSet))
                            ++resultEquipmentSets[tempEquipmentItem.EquipmentSet];
                        else
                            resultEquipmentSets.Add(tempEquipmentItem.EquipmentSet, 0);
                    }
                }
                // Right hand equipment
                if (data.EquipWeapons.NotEmptyRightHandSlot())
                {
                    tempEquipmentItem = data.EquipWeapons.GetRightHandEquipmentItem();
                    if (tempEquipmentItem != null)
                    {
                        GetBuffs(data.EquipWeapons.rightHand,
                            isCalculateStats ? null : (stats) => resultStats += stats,
                            isCalculateStatsRate ? null : (statsRate) => resultStatsRate += statsRate,
                            isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(resultAttributes, attributes),
                            isCalculateAttributesRate ? null : (attributesRate) => GameDataHelpers.CombineAttributes(resultAttributesRate, attributesRate),
                            isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(resultResistances, resistances),
                            isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(resultArmors, armors),
                            isCalculateArmorsRate ? null : (armorsRate) => GameDataHelpers.CombineArmors(resultArmorsRate, armorsRate),
                            isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(resultDamages, damages),
                            isCalculateDamagesRate ? null : (damagesRate) => GameDataHelpers.CombineDamages(resultDamagesRate, damagesRate),
                            isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(resultSkills, skills));
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
                if (data.EquipWeapons.NotEmptyLeftHandSlot())
                {
                    tempEquipmentItem = data.EquipWeapons.GetLeftHandEquipmentItem();
                    if (tempEquipmentItem != null)
                    {
                        GetBuffs(data.EquipWeapons.leftHand,
                        isCalculateStats ? null : (stats) => resultStats += stats,
                        isCalculateStatsRate ? null : (statsRate) => resultStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(resultAttributes, attributes),
                        isCalculateAttributesRate ? null : (attributesRate) => GameDataHelpers.CombineAttributes(resultAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(resultResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(resultArmors, armors),
                        isCalculateArmorsRate ? null : (armorsRate) => GameDataHelpers.CombineArmors(resultArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(resultDamages, damages),
                        isCalculateDamagesRate ? null : (damagesRate) => GameDataHelpers.CombineDamages(resultDamagesRate, damagesRate),
                        isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(resultSkills, skills));
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
                        isCalculateStats ? null : (stats) => resultStats += stats,
                        isCalculateStatsRate ? null : (statsRate) => resultStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(resultAttributes, attributes),
                        isCalculateAttributesRate ? null : (attributesRate) => GameDataHelpers.CombineAttributes(resultAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(resultResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(resultArmors, armors),
                        isCalculateArmorsRate ? null : (armorsRate) => GameDataHelpers.CombineArmors(resultArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(resultDamages, damages),
                        isCalculateDamagesRate ? null : (damagesRate) => GameDataHelpers.CombineDamages(resultDamagesRate, damagesRate),
                        isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(resultSkills, skills));
                }
            }

            if (sumWithSkills)
            {
                GameDataHelpers.CombineSkills(resultSkills, data.GetCharacterSkills());
                foreach (var skillEntry in resultSkills)
                {
                    GetBuffs(skillEntry.Key, skillEntry.Value,
                        isCalculateStats ? null : (stats) => resultStats += stats,
                        isCalculateStatsRate ? null : (statsRate) => resultStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(resultAttributes, attributes),
                        isCalculateAttributesRate ? null : (attributesRate) => GameDataHelpers.CombineAttributes(resultAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(resultResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(resultArmors, armors),
                        isCalculateArmorsRate ? null : (armorsRate) => GameDataHelpers.CombineArmors(resultArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(resultDamages, damages),
                        isCalculateDamagesRate ? null : (damagesRate) => GameDataHelpers.CombineDamages(resultDamagesRate, damagesRate));
                }
            }

            if (sumWithBuffs)
            {
                for (i = 0; i < data.Buffs.Count; ++i)
                {
                    GetBuffs(data.Buffs[i],
                        isCalculateStats ? null : (stats) => resultStats += stats,
                        isCalculateStatsRate ? null : (statsRate) => resultStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(resultAttributes, attributes),
                        isCalculateAttributesRate ? null : (attributesRate) => GameDataHelpers.CombineAttributes(resultAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(resultResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(resultArmors, armors),
                        isCalculateArmorsRate ? null : (armorsRate) => GameDataHelpers.CombineArmors(resultArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(resultDamages, damages),
                        isCalculateDamagesRate ? null : (damagesRate) => GameDataHelpers.CombineDamages(resultDamagesRate, damagesRate));
                }
                // From summon
                for (i = 0; i < data.Summons.Count; ++i)
                {
                    GetBuffs(data.Summons[i],
                        isCalculateStats ? null : (stats) => resultStats += stats,
                        isCalculateStatsRate ? null : (statsRate) => resultStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(resultAttributes, attributes),
                        isCalculateAttributesRate ? null : (attributesRate) => GameDataHelpers.CombineAttributes(resultAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(resultResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(resultArmors, armors),
                        isCalculateArmorsRate ? null : (armorsRate) => GameDataHelpers.CombineArmors(resultArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(resultDamages, damages),
                        isCalculateDamagesRate ? null : (damagesRate) => GameDataHelpers.CombineDamages(resultDamagesRate, damagesRate));
                }
                // From mount
                GetBuffs(data.PassengingVehicleEntity,
                    isCalculateStats ? null : (stats) => resultStats += stats,
                    isCalculateStatsRate ? null : (statsRate) => resultStatsRate += statsRate,
                    isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(resultAttributes, attributes),
                    isCalculateAttributesRate ? null : (attributesRate) => GameDataHelpers.CombineAttributes(resultAttributesRate, attributesRate),
                    isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(resultResistances, resistances),
                    isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(resultArmors, armors),
                    isCalculateArmorsRate ? null : (armorsRate) => GameDataHelpers.CombineArmors(resultArmorsRate, armorsRate),
                    isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(resultDamages, damages),
                    isCalculateDamagesRate ? null : (damagesRate) => GameDataHelpers.CombineDamages(resultDamagesRate, damagesRate));
            }
        }
    }
}