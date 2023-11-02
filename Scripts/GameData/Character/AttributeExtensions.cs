using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class AttributeExtensions
    {
        public static CharacterStats GetStats(this Dictionary<Attribute, float> entries)
        {
            CharacterStats result = new CharacterStats();
            if (entries == null || entries.Count == 0)
                return result;
            foreach (KeyValuePair<Attribute, float> entry in entries)
            {
                result += entry.Key.GetStats(entry.Value);
            }
            return result;
        }

        public static CharacterStats GetStats(this Attribute attribute, float level)
        {
            if (attribute == null)
                return new CharacterStats();
            return attribute.StatsIncreaseEachLevel * level;
        }

        public static CharacterStats GetStats(this AttributeAmount attributeAmount)
        {
            if (attributeAmount.attribute == null)
                return new CharacterStats();
            Attribute attribute = attributeAmount.attribute;
            return attribute.GetStats(attributeAmount.amount);
        }

        public static CharacterStats GetStats(this AttributeIncremental attributeIncremental, int level)
        {
            if (attributeIncremental.attribute == null)
                return new CharacterStats();
            Attribute attribute = attributeIncremental.attribute;
            return attribute.GetStats(attributeIncremental.amount.GetAmount(level));
        }

        public static Dictionary<DamageElement, float> GetIncreaseResistances(this Dictionary<Attribute, float> entries)
        {
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            if (entries == null || entries.Count == 0)
                return result;
            foreach (KeyValuePair<Attribute, float> entry in entries)
            {
                result = GameDataHelpers.CombineResistances(result, entry.Key.GetIncreaseResistances(entry.Value));
            }
            return result;
        }

        public static Dictionary<DamageElement, float> GetIncreaseResistances(this Attribute attribute, float amount)
        {
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            if (attribute != null)
                result = GameDataHelpers.CombineResistances(attribute.IncreaseResistances, result, Mathf.CeilToInt(amount), 1f);
            return result;
        }

        public static Dictionary<DamageElement, float> GetIncreaseArmors(this Dictionary<Attribute, float> entries)
        {
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            if (entries == null || entries.Count == 0)
                return result;
            foreach (KeyValuePair<Attribute, float> entry in entries)
            {
                result = GameDataHelpers.CombineArmors(result, entry.Key.GetIncreaseArmors(entry.Value));
            }
            return result;
        }

        public static Dictionary<DamageElement, float> GetIncreaseArmors(this Attribute attribute, float amount)
        {
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            if (attribute != null)
                result = GameDataHelpers.CombineArmors(attribute.IncreaseArmors, result, Mathf.CeilToInt(amount), 1f);
            return result;
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this Dictionary<Attribute, float> entries)
        {
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            if (entries == null || entries.Count == 0)
                return result;
            foreach (KeyValuePair<Attribute, float> entry in entries)
            {
                result = GameDataHelpers.CombineDamages(result, entry.Key.GetIncreaseDamages(entry.Value));
            }
            return result;
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this Attribute attribute, float amount)
        {
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            if (attribute != null)
                result = GameDataHelpers.CombineDamages(attribute.IncreaseDamages, result, Mathf.CeilToInt(amount), 1f);
            return result;
        }

        public static Dictionary<StatusEffect, float> GetIncreaseStatusEffectResistances(this Dictionary<Attribute, float> entries)
        {
            Dictionary<StatusEffect, float> result = new Dictionary<StatusEffect, float>();
            if (entries == null || entries.Count == 0)
                return result;
            foreach (KeyValuePair<Attribute, float> entry in entries)
            {
                result = GameDataHelpers.CombineStatusEffectResistances(result, entry.Key.GetIncreaseStatusEffectResistances(entry.Value));
            }
            return result;
        }

        public static Dictionary<StatusEffect, float> GetIncreaseStatusEffectResistances(this Attribute attribute, float amount)
        {
            Dictionary<StatusEffect, float> result = new Dictionary<StatusEffect, float>();
            if (attribute != null)
                result = GameDataHelpers.CombineStatusEffectResistances(attribute.IncreaseStatusEffectResistances, result, Mathf.CeilToInt(amount), 1f);
            return result;
        }
    }
}
