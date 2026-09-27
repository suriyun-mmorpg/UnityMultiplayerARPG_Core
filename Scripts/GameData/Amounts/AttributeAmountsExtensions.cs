using System.Collections.Generic;
using UnityEngine.Pool;

namespace MultiplayerARPG
{
    public static class AttributeAmountsExtensions
    {
        public static CharacterStats GetStats(this AttributeAmounts amounts)
        {
            CharacterStats result = new CharacterStats();
            uint mask = amounts.OccupiedMask;
            for (int i = 0; i < RuntimeGameDataSlots.AttributeCount; ++i)
            {
                if ((mask & (1u << i)) != 0)
                    result += RuntimeGameDataSlots.GetAttribute(i).GetStats(amounts[i]);
            }
            return result;
        }

        public static void GetIncreaseResistances(this AttributeAmounts amounts, ref DamageElementFloatAmounts result)
        {
            result.Clear();
            uint mask = amounts.OccupiedMask;
            using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> temporary))
            {
                for (int i = 0; i < RuntimeGameDataSlots.AttributeCount; ++i)
                {
                    if ((mask & (1u << i)) == 0)
                        continue;
                    temporary.Clear();
                    RuntimeGameDataSlots.GetAttribute(i).GetIncreaseResistancesByLevel(amounts[i], temporary);
                    result.Combine(temporary);
                }
            }
        }

        public static void GetIncreaseArmors(this AttributeAmounts amounts, ref DamageElementFloatAmounts result)
        {
            result.Clear();
            uint mask = amounts.OccupiedMask;
            using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> temporary))
            {
                for (int i = 0; i < RuntimeGameDataSlots.AttributeCount; ++i)
                {
                    if ((mask & (1u << i)) == 0)
                        continue;
                    temporary.Clear();
                    RuntimeGameDataSlots.GetAttribute(i).GetIncreaseArmorsByLevel(amounts[i], temporary);
                    result.Combine(temporary);
                }
            }
        }

        public static void GetIncreaseDamages(this AttributeAmounts amounts, ref DamageElementMinMaxFloatAmounts result)
        {
            result.Clear();
            uint mask = amounts.OccupiedMask;
            using (CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Get(out Dictionary<DamageElement, MinMaxFloat> temporary))
            {
                for (int i = 0; i < RuntimeGameDataSlots.AttributeCount; ++i)
                {
                    if ((mask & (1u << i)) == 0)
                        continue;
                    temporary.Clear();
                    RuntimeGameDataSlots.GetAttribute(i).GetIncreaseDamagesByLevel(amounts[i], temporary);
                    result.Combine(temporary);
                }
            }
        }

        public static void GetIncreaseStatusEffectResistances(this AttributeAmounts amounts, Dictionary<StatusEffect, float> result)
        {
            result.Clear();
            uint mask = amounts.OccupiedMask;
            using (CollectionPool<Dictionary<StatusEffect, float>, KeyValuePair<StatusEffect, float>>.Get(out Dictionary<StatusEffect, float> temporary))
            {
                for (int i = 0; i < RuntimeGameDataSlots.AttributeCount; ++i)
                {
                    if ((mask & (1u << i)) == 0)
                        continue;
                    temporary.Clear();
                    RuntimeGameDataSlots.GetAttribute(i).GetIncreaseStatusEffectResistancesByLevel(amounts[i], temporary);
                    GameDataHelpers.CombineStatusEffectResistances(result, temporary);
                }
            }
        }
    }
}
