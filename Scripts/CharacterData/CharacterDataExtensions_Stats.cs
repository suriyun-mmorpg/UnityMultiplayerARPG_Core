using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static partial class CharacterDataExtensions
    {
        private static Dictionary<Attribute, float> GetCharacterAttributes(this ICharacterData data)
        {
            if (data == null)
                return new Dictionary<Attribute, float>();
            Dictionary<Attribute, float> result;
            BaseCharacter database = data.GetDatabase();
            // Attributes from character database
            if (database == null)
                result = new Dictionary<Attribute, float>();
            else
                result = database.GetCharacterAttributes(data.Level);
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
            return result;
        }

        private static Dictionary<BaseSkill, int> GetCharacterSkills(this ICharacterData data)
        {
            if (data == null)
                return new Dictionary<BaseSkill, int>();
            Dictionary<BaseSkill, int> result;
            BaseCharacter database = data.GetDatabase();
            // Skills from character database
            if (database == null)
                result = new Dictionary<BaseSkill, int>();
            else
                result = new Dictionary<BaseSkill, int>(database.CacheSkillLevels);
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
            return result;
        }

        private static Dictionary<DamageElement, float> GetCharacterResistances(this ICharacterData data)
        {
            if (data == null)
                return new Dictionary<DamageElement, float>();
            Dictionary<DamageElement, float> result;
            BaseCharacter database = data.GetDatabase();
            if (database == null)
                result = new Dictionary<DamageElement, float>();
            else
                result = new Dictionary<DamageElement, float>(database.GetCharacterResistances(data.Level));
            return result;
        }

        private static Dictionary<DamageElement, float> GetCharacterArmors(this ICharacterData data)
        {
            if (data == null)
                return new Dictionary<DamageElement, float>();
            Dictionary<DamageElement, float> result;
            BaseCharacter database = data.GetDatabase();
            if (database == null)
                result = new Dictionary<DamageElement, float>();
            else
                result = new Dictionary<DamageElement, float>(database.GetCharacterArmors(data.Level));
            return result;
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
            System.Action<Dictionary<BaseSkill, int>> onIncreasingSkill)
        {
            if (socketEnhancerItem == null)
                return;
            if (onIncreasingStats != null)
                onIncreasingStats.Invoke(socketEnhancerItem.SocketEnhanceEffect.stats);
            if (onIncreasingStatsRate != null)
                onIncreasingStatsRate.Invoke(socketEnhancerItem.SocketEnhanceEffect.statsRate);
            if (onIncreasingAttributes != null)
                onIncreasingAttributes.Invoke(GameDataHelpers.CombineAttributes(socketEnhancerItem.SocketEnhanceEffect.attributes, new Dictionary<Attribute, float>(), 1f));
            if (onIncreasingAttributesRate != null)
                onIncreasingAttributesRate.Invoke(GameDataHelpers.CombineAttributes(socketEnhancerItem.SocketEnhanceEffect.attributesRate, new Dictionary<Attribute, float>(), 1f));
            if (onIncreasingResistances != null)
                onIncreasingResistances.Invoke(GameDataHelpers.CombineResistances(socketEnhancerItem.SocketEnhanceEffect.resistances, new Dictionary<DamageElement, float>(), 1f));
            if (onIncreasingArmors != null)
                onIncreasingArmors.Invoke(GameDataHelpers.CombineArmors(socketEnhancerItem.SocketEnhanceEffect.armors, new Dictionary<DamageElement, float>(), 1f));
            if (onIncreasingArmorsRate != null)
                onIncreasingArmorsRate.Invoke(GameDataHelpers.CombineArmors(socketEnhancerItem.SocketEnhanceEffect.armorsRate, new Dictionary<DamageElement, float>(), 1f));
            if (onIncreasingDamages != null)
                onIncreasingDamages.Invoke(GameDataHelpers.CombineDamages(socketEnhancerItem.SocketEnhanceEffect.damages, new Dictionary<DamageElement, MinMaxFloat>(), 1f));
            if (onIncreasingDamagesRate != null)
                onIncreasingDamagesRate.Invoke(GameDataHelpers.CombineDamages(socketEnhancerItem.SocketEnhanceEffect.damagesRate, new Dictionary<DamageElement, MinMaxFloat>(), 1f));
            if (onIncreasingSkill != null)
                onIncreasingSkill.Invoke(GameDataHelpers.CombineSkills(socketEnhancerItem.SocketEnhanceEffect.skills, new Dictionary<BaseSkill, int>(), 1f));
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
            for (i = 0; i < item.Sockets.Count; ++i)
            {
                if (!GameInstance.Items.TryGetValue(item.Sockets[i], out tempItem) || !tempItem.IsSocketEnhancer())
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
                    onIncreasingSkill);
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

        public static void GetBuffs(this PlayerTitle title,
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
            if (title == null)
                return;
            if (onIncreasingStats != null)
                onIncreasingStats.Invoke(title.Buff.GetIncreaseStats(1));
            if (onIncreasingStatsRate != null)
                onIncreasingStatsRate.Invoke(title.Buff.GetIncreaseStatsRate(1));
            if (onIncreasingAttributes != null)
                onIncreasingAttributes.Invoke(title.Buff.GetIncreaseAttributes(1));
            if (onIncreasingAttributesRate != null)
                onIncreasingAttributesRate.Invoke(title.Buff.GetIncreaseAttributesRate(1));
            if (onIncreasingResistances != null)
                onIncreasingResistances.Invoke(title.Buff.GetIncreaseResistances(1));
            if (onIncreasingArmors != null)
                onIncreasingArmors.Invoke(title.Buff.GetIncreaseArmors(1));
            if (onIncreasingArmorsRate != null)
                onIncreasingArmorsRate.Invoke(title.Buff.GetIncreaseArmorsRate(1));
            if (onIncreasingDamages != null)
                onIncreasingDamages.Invoke(title.Buff.GetIncreaseDamages(1));
            if (onIncreasingDamagesRate != null)
                onIncreasingDamagesRate.Invoke(title.Buff.GetIncreaseDamagesRate(1));
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

        public static void GetBuffs(this ICharacterData data, bool sumWithEquipments, bool sumWithBuffs, bool sumWithSkills,
            System.Action<CharacterStats> onGetStats = null,
            System.Action<Dictionary<Attribute, float>> onGetAttributes = null,
            System.Action<Dictionary<DamageElement, float>> onGetResistances = null,
            System.Action<Dictionary<DamageElement, float>> onGetArmors = null,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onGetRightHandDamages = null,
            System.Action<Dictionary<DamageElement, MinMaxFloat>> onGetLeftHandDamages = null,
            System.Action<Dictionary<BaseSkill, int>> onGetSkills = null,
            System.Action<Dictionary<EquipmentSet, int>> onGetEquipmentSets = null)
        {
            bool isCalculateStats = onGetStats != null;
            bool isCalculateAttributes = onGetStats != null || onGetAttributes != null;
            bool isCalculateResistances = onGetResistances != null;
            bool isCalculateArmors = onGetArmors != null;
            bool isCalculateDamages = onGetRightHandDamages != null || onGetLeftHandDamages != null;
            bool isCalculateSkills = onGetSkills != null;

            // Prepare result stats, by using character's base stats
            // For weapons it will be based on equipped weapons
            CharacterStats resultStats = !isCalculateStats ? new CharacterStats() : data.GetCharacterStats();
            Dictionary<Attribute, float> resultAttributes = !isCalculateAttributes ? new Dictionary<Attribute, float>() : data.GetCharacterAttributes();
            Dictionary<DamageElement, float> resultResistances = !isCalculateResistances ? new Dictionary<DamageElement, float>() : data.GetCharacterResistances();
            Dictionary<DamageElement, float> resultArmors = !isCalculateArmors ? new Dictionary<DamageElement, float>() : data.GetCharacterArmors();
            Dictionary<DamageElement, MinMaxFloat> resultRightHandDamages = new Dictionary<DamageElement, MinMaxFloat>();
            Dictionary<DamageElement, MinMaxFloat> resultLeftHandDamages = new Dictionary<DamageElement, MinMaxFloat>();
            Dictionary<BaseSkill, int> resultSkills = !isCalculateSkills ? new Dictionary<BaseSkill, int>() : data.GetCharacterSkills();
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

            // If not found equipped weapon, it will use default weapon which set in game instance as equipped weapon
            bool foundEquippedRightHandWeapon = false;
            KeyValuePair<DamageElement, MinMaxFloat> equippedRightHandWeaponDamageAmount = default;
            bool foundEquippedLeftHandWeapon = false;
            KeyValuePair<DamageElement, MinMaxFloat> equippedLeftHandWeaponDamageAmount = default;

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
                    resultArmors = GameDataHelpers.CombineArmors(resultArmors, data.EquipItems[i].GetArmorAmount());
                    GetBuffs(data.EquipItems[i],
                        isCalculateStats ? null : (stats) => buffStats += stats,
                        isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills));
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
                        equippedRightHandWeaponDamageAmount = data.EquipWeapons.rightHand.GetDamageAmount();
                        resultRightHandDamages = GameDataHelpers.CombineDamages(resultRightHandDamages, equippedRightHandWeaponDamageAmount);
                    }
                    resultRightHandDamages = GameDataHelpers.CombineDamages(resultRightHandDamages, data.EquipWeapons.rightHand.GetDamageAmount());
                    resultArmors = GameDataHelpers.CombineArmors(resultArmors, data.EquipWeapons.rightHand.GetArmorAmount());
                    GetBuffs(data.EquipWeapons.rightHand,
                        isCalculateStats ? null : (stats) => buffStats += stats,
                        isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills));
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
                        equippedLeftHandWeaponDamageAmount = data.EquipWeapons.leftHand.GetDamageAmount();
                        resultLeftHandDamages = GameDataHelpers.CombineDamages(resultLeftHandDamages, equippedLeftHandWeaponDamageAmount);
                    }
                    resultArmors = GameDataHelpers.CombineArmors(resultArmors, data.EquipWeapons.leftHand.GetArmorAmount());
                    GetBuffs(data.EquipWeapons.leftHand,
                        isCalculateStats ? null : (stats) => buffStats += stats,
                        isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills));
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
                        isCalculateStats ? null : (stats) => buffStats += stats,
                        isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                        isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills));
                }
                // From title
                if (GameInstance.PlayerTitles.TryGetValue(data.TitleDataId, out PlayerTitle title))
                {
                    GetBuffs(title,
                        isCalculateStats ? null : (stats) => buffStats += stats,
                        isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate));
                }
            }

            // Default weapon
            if (!foundEquippedRightHandWeapon && !foundEquippedLeftHandWeapon)
            {
                foundEquippedRightHandWeapon = true;
                CharacterItem fakeDefaultItem = CharacterItem.CreateDefaultWeapon();
                equippedRightHandWeaponDamageAmount = fakeDefaultItem.GetDamageAmount();
                resultRightHandDamages = GameDataHelpers.CombineDamages(resultRightHandDamages, equippedRightHandWeaponDamageAmount);
                GetBuffs(fakeDefaultItem,
                    isCalculateStats ? null : (stats) => buffStats += stats,
                    isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                    isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                    isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                    isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                    isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                    isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                    isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                    isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate),
                    isCalculateSkills ? null : (skills) => GameDataHelpers.CombineSkills(buffSkills, skills));
            }

            // Only items will have skill buffs
            resultSkills = GameDataHelpers.CombineSkills(resultSkills, buffSkills);

            if (sumWithBuffs)
            {
                // From buffs
                for (i = 0; i < data.Buffs.Count; ++i)
                {
                    GetBuffs(data.Buffs[i],
                        isCalculateStats ? null : (stats) => buffStats += stats,
                        isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate));
                }
                // From summon
                for (i = 0; i < data.Summons.Count; ++i)
                {
                    GetBuffs(data.Summons[i],
                        isCalculateStats ? null : (stats) => buffStats += stats,
                        isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate));
                }
                // From mount
                GetBuffs(data.PassengingVehicleEntity,
                    isCalculateStats ? null : (stats) => buffStats += stats,
                    isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                    isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                    isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                    isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                    isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                    isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                    isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                    isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate));
            }

            if (sumWithSkills)
            {
                foreach (var skillEntry in resultSkills)
                {
                    GetBuffs(skillEntry.Key, skillEntry.Value,
                        isCalculateStats ? null : (stats) => buffStats += stats,
                        isCalculateStats ? null : (statsRate) => buffStatsRate += statsRate,
                        isCalculateAttributes ? null : (attributes) => GameDataHelpers.CombineAttributes(buffAttributes, attributes),
                        isCalculateAttributes ? null : (attributesRate) => GameDataHelpers.CombineAttributes(buffAttributesRate, attributesRate),
                        isCalculateResistances ? null : (resistances) => GameDataHelpers.CombineResistances(buffResistances, resistances),
                        isCalculateArmors ? null : (armors) => GameDataHelpers.CombineArmors(buffArmors, armors),
                        isCalculateArmors ? null : (armorsRate) => GameDataHelpers.CombineArmors(buffArmorsRate, armorsRate),
                        isCalculateDamages ? null : (damages) => GameDataHelpers.CombineDamages(buffDamages, damages),
                        isCalculateDamages ? null : (damagesRate) => GameDataHelpers.CombineDamages(buffDamagesRate, damagesRate));
                }
            }

            // Calculate stats by buffs
            resultAttributes = GameDataHelpers.CombineAttributes(resultAttributes, buffAttributes);
            resultAttributes = GameDataHelpers.MultiplyAttributes(resultAttributes, buffAttributesRate);
            resultResistances = GameDataHelpers.CombineResistances(resultResistances, buffResistances);
            resultArmors = GameDataHelpers.CombineArmors(resultArmors, buffArmors);
            resultArmors = GameDataHelpers.MultiplyArmors(resultArmors, buffArmorsRate);
            float weaponEffectiveness = 0f;
            if (foundEquippedRightHandWeapon || foundEquippedLeftHandWeapon)
            {
                weaponEffectiveness = GameDataHelpers.GetEffectivenessDamage(data.EquipWeapons.GetRightHandWeaponItem().WeaponType.CacheEffectivenessAttributes, resultAttributes);
            }
            if (foundEquippedRightHandWeapon)
            {
                resultRightHandDamages = GameDataHelpers.CombineDamages(resultRightHandDamages, buffDamages);
                resultRightHandDamages = GameDataHelpers.MultiplyDamages(resultRightHandDamages, buffDamagesRate);
                resultRightHandDamages = GameDataHelpers.CombineDamages(resultRightHandDamages, equippedRightHandWeaponDamageAmount, weaponEffectiveness);
            }
            if (foundEquippedLeftHandWeapon)
            {
                resultLeftHandDamages = GameDataHelpers.CombineDamages(resultLeftHandDamages, buffDamages);
                resultLeftHandDamages = GameDataHelpers.MultiplyDamages(resultLeftHandDamages, buffDamagesRate);
                resultLeftHandDamages = GameDataHelpers.CombineDamages(resultLeftHandDamages, equippedLeftHandWeaponDamageAmount, weaponEffectiveness);
            }
            resultStats += buffStats;
            resultStats += GameDataHelpers.GetStatsFromAttributes(resultAttributes);
            resultStats *= buffStatsRate;

            // Invoke actions
            if (onGetStats != null)
                onGetStats.Invoke(resultStats);
            if (onGetAttributes != null)
                onGetAttributes.Invoke(resultAttributes);
            if (onGetResistances != null)
                onGetResistances.Invoke(resultResistances);
            if (onGetArmors != null)
                onGetArmors.Invoke(resultArmors);
            if (onGetRightHandDamages != null)
                onGetRightHandDamages.Invoke(resultRightHandDamages);
            if (onGetLeftHandDamages != null)
                onGetLeftHandDamages.Invoke(resultLeftHandDamages);
            if (onGetSkills != null)
                onGetSkills.Invoke(resultSkills);
            if (onGetEquipmentSets != null)
                onGetEquipmentSets.Invoke(resultEquipmentSets);
        }
    }
}
