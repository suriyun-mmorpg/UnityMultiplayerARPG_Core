using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterRecoveryData
    {
        private readonly Dictionary<DamageElement, MinMaxFloat> damageOverTimes;
        private readonly List<DamageElement> damageElements;
        private float totalDamage = 0f;

        public BaseCharacterEntity CharacterEntity { get; private set; }
        public EntityInfo Instigator { get; private set; }
        public float RecoveryingHp { get; set; } = 0f;
        public float RecoveryingMp { get; set; } = 0f;
        public float RecoveryingStamina { get; set; } = 0f;
        public float RecoveryingFood { get; set; } = 0f;
        public float RecoveryingWater { get; set; } = 0f;
        public float DecreasingHp { get; set; } = 0f;
        public float DecreasingMp { get; set; } = 0f;
        public float DecreasingStamina { get; set; } = 0f;
        public float DecreasingFood { get; set; } = 0f;
        public float DecreasingWater { get; set; } = 0f;

        public CharacterRecoveryData(BaseCharacterEntity characterEntity, EntityInfo instigator)
        {
            CharacterEntity = characterEntity;
            Instigator = instigator;
            damageOverTimes = new Dictionary<DamageElement, MinMaxFloat>();
            damageElements = new List<DamageElement>();
        }

        public void IncreaseDamageOverTimes(DamageElement damageElement, MinMaxFloat damageAmount)
        {
            if (!damageOverTimes.ContainsKey(damageElement))
            {
                damageOverTimes.Add(damageElement, default(MinMaxFloat));
                damageElements.Add(damageElement);
            }
            damageOverTimes[damageElement] = damageOverTimes[damageElement] + damageAmount;
        }

        public void Clear()
        {
            RecoveryingHp = 0f;
            RecoveryingMp = 0f;
            RecoveryingStamina = 0f;
            RecoveryingFood = 0f;
            RecoveryingWater = 0f;
            DecreasingHp = 0f;
            DecreasingMp = 0f;
            DecreasingStamina = 0f;
            DecreasingFood = 0f;
            DecreasingWater = 0f;
            totalDamage = 0f;
            damageOverTimes.Clear();
            damageElements.Clear();
        }

        public void Apply()
        {
            int tempAmount;
            // Hp
            if (CharacterEntity.CurrentHp < CharacterEntity.MaxHp)
            {
                if (RecoveryingHp >= 1)
                {
                    tempAmount = (int)RecoveryingHp;
                    CharacterEntity.OnBuffHpRecovery(Instigator, tempAmount);
                    RecoveryingHp -= tempAmount;
                }
            }
            else
                RecoveryingHp = 0;

            // Decrease Hp
            if (CharacterEntity.CurrentHp > 0)
            {
                if (DecreasingHp >= 1)
                {
                    tempAmount = (int)DecreasingHp;
                    CharacterEntity.OnBuffHpDecrease(Instigator, tempAmount);
                    DecreasingHp -= tempAmount;
                }
            }
            else
                DecreasingHp = 0;

            // Mp
            if (CharacterEntity.CurrentMp < CharacterEntity.MaxMp)
            {
                if (RecoveryingMp >= 1)
                {
                    tempAmount = (int)RecoveryingMp;
                    CharacterEntity.OnBuffMpRecovery(Instigator, tempAmount);
                    RecoveryingMp -= tempAmount;
                }
            }
            else
                RecoveryingMp = 0;

            // Decrease Mp
            if (CharacterEntity.CurrentMp > 0)
            {
                if (DecreasingMp >= 1)
                {
                    tempAmount = (int)DecreasingMp;
                    CharacterEntity.OnBuffMpDecrease(Instigator, tempAmount);
                    DecreasingMp -= tempAmount;
                }
            }
            else
                DecreasingMp = 0;

            // Stamina
            if (CharacterEntity.CurrentStamina < CharacterEntity.MaxStamina)
            {
                if (RecoveryingStamina >= 1)
                {
                    tempAmount = (int)RecoveryingStamina;
                    CharacterEntity.OnBuffStaminaRecovery(Instigator, tempAmount);
                    RecoveryingStamina -= tempAmount;
                }
            }
            else
                RecoveryingStamina = 0;

            // Decrease Stamina
            if (CharacterEntity.CurrentStamina > 0)
            {
                if (DecreasingStamina >= 1)
                {
                    tempAmount = (int)DecreasingStamina;
                    CharacterEntity.OnBuffStaminaDecrease(Instigator, tempAmount);
                    DecreasingStamina -= tempAmount;
                }
            }
            else
                DecreasingStamina = 0;

            // Food
            if (CharacterEntity.CurrentFood < CharacterEntity.MaxFood)
            {
                if (RecoveryingFood >= 1)
                {
                    tempAmount = (int)RecoveryingFood;
                    CharacterEntity.OnBuffFoodRecovery(Instigator, tempAmount);
                    RecoveryingFood -= tempAmount;
                }
            }
            else
                RecoveryingFood = 0;

            // Decrease Food
            if (CharacterEntity.CurrentFood > 0)
            {
                if (DecreasingFood >= 1)
                {
                    tempAmount = (int)DecreasingFood;
                    CharacterEntity.OnBuffFoodDecrease(Instigator, tempAmount);
                    DecreasingFood -= tempAmount;
                }
            }
            else
                DecreasingFood = 0;

            // Water
            if (CharacterEntity.CurrentWater < CharacterEntity.MaxWater)
            {
                if (RecoveryingWater >= 1)
                {
                    tempAmount = (int)RecoveryingWater;
                    CharacterEntity.OnBuffWaterRecovery(Instigator, tempAmount);
                    RecoveryingWater -= tempAmount;
                }
            }
            else
                RecoveryingWater = 0;

            // Decrease Water
            if (CharacterEntity.CurrentWater > 0)
            {
                if (DecreasingWater >= 1)
                {
                    tempAmount = (int)DecreasingWater;
                    CharacterEntity.OnBuffWaterDecrease(Instigator, tempAmount);
                    DecreasingWater -= tempAmount;
                }
            }
            else
                DecreasingWater = 0;

            // Apply damage overtime
            if (CharacterEntity.CurrentHp > 0 && damageOverTimes.Count > 0)
            {
                if (damageElements.Count > 0)
                {
                    foreach (DamageElement damageElement in damageElements)
                    {
                        totalDamage += damageElement.GetDamageReducedByResistance(CharacterEntity.GetCaches().Resistances, CharacterEntity.GetCaches().Armors, damageOverTimes[damageElement].Random(Random.Range(0, 255)));
                    }
                }
                if (totalDamage >= 1)
                {
                    tempAmount = (int)totalDamage;
                    CharacterEntity.CurrentHp -= tempAmount;
                    CharacterEntity.ReceivedDamage(CharacterEntity.CacheTransform.position, Instigator, damageOverTimes, CombatAmountType.NormalDamage, tempAmount, null, null, 0);
                    totalDamage -= tempAmount;
                    damageOverTimes.Clear();
                    damageElements.Clear();
                }
            }
        }
    }
}
