using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterRecoveryData
    {
        public BaseCharacterEntity CharacterEntity { get; private set; }
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
        public float TotalDamageOverTime { get; set; } = 0f;
        public Dictionary<DamageElement, MinMaxFloat> DamageOverTimes { get; set; } = new Dictionary<DamageElement, MinMaxFloat>();

        private float calculatingRecoveryingHp = 0f;
        private float calculatingRecoveryingMp = 0f;
        private float calculatingRecoveryingStamina = 0f;
        private float calculatingRecoveryingFood = 0f;
        private float calculatingRecoveryingWater = 0f;
        private float calculatingDecreasingHp = 0f;
        private float calculatingDecreasingMp = 0f;
        private float calculatingDecreasingStamina = 0f;
        private float calculatingDecreasingFood = 0f;
        private float calculatingDecreasingWater = 0f;
        private float calculatingTotalDamageOverTime = 0f;
        private CharacterBuff buff;

        public CharacterRecoveryData(BaseCharacterEntity characterEntity)
        {
            CharacterEntity = characterEntity;
        }

        public void SetupByBuff(CharacterBuff buff)
        {
            this.buff = buff;

            // Damage over time
            TotalDamageOverTime = 0f;
            DamageOverTimes = buff.GetDamageOverTimes();
            foreach (KeyValuePair<DamageElement, MinMaxFloat> damageOverTime in DamageOverTimes)
            {
                TotalDamageOverTime += damageOverTime.Key.GetDamageReducedByResistance(CharacterEntity.GetCaches().Resistances, CharacterEntity.GetCaches().Armors, damageOverTime.Value.Random(Random.Range(0, 255)));
            }
            int tempAmount;
            // Hp recovery
            tempAmount = buff.GetRecoveryHp();
            if (tempAmount > 0)
                RecoveryingHp += tempAmount;
            else if (tempAmount < 0)
                DecreasingHp += -tempAmount;
            // Mp recovery
            tempAmount = buff.GetRecoveryMp();
            if (tempAmount > 0)
                RecoveryingMp += tempAmount;
            else if (tempAmount < 0)
                DecreasingMp += -tempAmount;
            // Stamina recovery
            tempAmount = buff.GetRecoveryStamina();
            if (tempAmount > 0)
                RecoveryingStamina += tempAmount;
            else if (tempAmount < 0)
                DecreasingStamina += -tempAmount;
            // Food recovery
            tempAmount = buff.GetRecoveryFood();
            if (tempAmount > 0)
                RecoveryingFood += tempAmount;
            else if (tempAmount < 0)
                DecreasingFood += -tempAmount;
            // Water recovery
            tempAmount = buff.GetRecoveryWater();
            if (tempAmount > 0)
                RecoveryingWater += tempAmount;
            else if (tempAmount < 0)
                DecreasingWater += -tempAmount;
        }

        public void Clear()
        {
            buff = null;
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
            TotalDamageOverTime = 0f;
            DamageOverTimes.Clear();
            calculatingRecoveryingHp = 0f;
            calculatingRecoveryingMp = 0f;
            calculatingRecoveryingStamina = 0f;
            calculatingRecoveryingFood = 0f;
            calculatingRecoveryingWater = 0f;
            calculatingDecreasingHp = 0f;
            calculatingDecreasingMp = 0f;
            calculatingDecreasingStamina = 0f;
            calculatingDecreasingFood = 0f;
            calculatingDecreasingWater = 0f;
            calculatingTotalDamageOverTime = 0f;
        }

        public void Apply(float rate)
        {
            EntityInfo instigator = buff != null ? buff.BuffApplier : EntityInfo.Empty;

            int tempAmount;
            // Hp
            if (CharacterEntity.CurrentHp < CharacterEntity.MaxHp)
            {
                calculatingRecoveryingHp += RecoveryingHp * rate;
                if (calculatingRecoveryingHp >= 1)
                {
                    tempAmount = (int)calculatingRecoveryingHp;
                    if (tempAmount < 0)
                        tempAmount = 0;
                    CharacterEntity.OnBuffHpRecovery(instigator, tempAmount);
                    calculatingRecoveryingHp -= tempAmount;
                }
            }

            // Decrease Hp
            if (CharacterEntity.CurrentHp > 0)
            {
                calculatingDecreasingHp += DecreasingHp * rate;
                if (calculatingDecreasingHp >= 1)
                {
                    tempAmount = (int)calculatingDecreasingHp;
                    if (tempAmount < 0)
                        tempAmount = 0;
                    CharacterEntity.OnBuffHpDecrease(instigator, tempAmount);
                    calculatingDecreasingHp -= tempAmount;
                }
            }

            // Mp
            if (CharacterEntity.CurrentMp < CharacterEntity.MaxMp)
            {
                calculatingRecoveryingMp += RecoveryingMp * rate;
                if (calculatingRecoveryingMp >= 1)
                {
                    tempAmount = (int)calculatingRecoveryingMp;
                    if (tempAmount < 0)
                        tempAmount = 0;
                    CharacterEntity.OnBuffMpRecovery(instigator, tempAmount);
                    calculatingRecoveryingMp -= tempAmount;
                }
            }

            // Decrease Mp
            if (CharacterEntity.CurrentMp > 0)
            {
                calculatingDecreasingMp += DecreasingMp * rate;
                if (calculatingDecreasingMp >= 1)
                {
                    tempAmount = (int)calculatingDecreasingMp;
                    if (tempAmount < 0)
                        tempAmount = 0;
                    CharacterEntity.OnBuffMpDecrease(instigator, tempAmount);
                    calculatingDecreasingMp -= tempAmount;
                }
            }

            // Stamina
            if (CharacterEntity.CurrentStamina < CharacterEntity.MaxStamina)
            {
                calculatingRecoveryingStamina += RecoveryingStamina * rate;
                if (calculatingRecoveryingStamina >= 1)
                {
                    tempAmount = (int)calculatingRecoveryingStamina;
                    if (tempAmount < 0)
                        tempAmount = 0;
                    CharacterEntity.OnBuffStaminaRecovery(instigator, tempAmount);
                    calculatingRecoveryingStamina -= tempAmount;
                }
            }

            // Decrease Stamina
            if (CharacterEntity.CurrentStamina > 0)
            {
                calculatingDecreasingStamina += DecreasingStamina * rate;
                if (calculatingDecreasingStamina >= 1)
                {
                    tempAmount = (int)calculatingDecreasingStamina;
                    if (tempAmount < 0)
                        tempAmount = 0;
                    CharacterEntity.OnBuffStaminaDecrease(instigator, tempAmount);
                    calculatingDecreasingStamina -= tempAmount;
                }
            }

            // Food
            if (CharacterEntity.CurrentFood < CharacterEntity.MaxFood)
            {
                calculatingRecoveryingFood += RecoveryingFood * rate;
                if (calculatingRecoveryingFood >= 1)
                {
                    tempAmount = (int)calculatingRecoveryingFood;
                    if (tempAmount < 0)
                        tempAmount = 0;
                    CharacterEntity.OnBuffFoodRecovery(instigator, tempAmount);
                    calculatingRecoveryingFood -= tempAmount;
                }
            }

            // Decrease Food
            if (CharacterEntity.CurrentFood > 0)
            {
                calculatingDecreasingFood += DecreasingFood * rate;
                if (calculatingDecreasingFood >= 1)
                {
                    tempAmount = (int)calculatingDecreasingFood;
                    if (tempAmount < 0)
                        tempAmount = 0;
                    CharacterEntity.OnBuffFoodDecrease(instigator, tempAmount);
                    calculatingDecreasingFood -= tempAmount;
                }
            }

            // Water
            if (CharacterEntity.CurrentWater < CharacterEntity.MaxWater)
            {
                calculatingRecoveryingWater += RecoveryingWater * rate;
                if (calculatingRecoveryingWater >= 1)
                {
                    tempAmount = (int)calculatingRecoveryingWater;
                    if (tempAmount < 0)
                        tempAmount = 0;
                    CharacterEntity.OnBuffWaterRecovery(instigator, tempAmount);
                    calculatingRecoveryingWater -= tempAmount;
                }
            }

            // Decrease Water
            if (CharacterEntity.CurrentWater > 0)
            {
                calculatingDecreasingWater += DecreasingWater * rate;
                if (calculatingDecreasingWater >= 1)
                {
                    tempAmount = (int)calculatingDecreasingWater;
                    if (tempAmount < 0)
                        tempAmount = 0;
                    CharacterEntity.OnBuffWaterDecrease(instigator, tempAmount);
                    calculatingDecreasingWater -= tempAmount;
                }
            }

            // Validate and do something if character dead
            CharacterEntity.ValidateRecovery(instigator);

            // Apply damage overtime
            if (CharacterEntity.CurrentHp > 0)
            {
                calculatingTotalDamageOverTime += TotalDamageOverTime * rate;
                if (calculatingTotalDamageOverTime >= 1)
                {
                    CharacterItem weapon = null;
                    BaseSkill skill = null;
                    short skillLevel = 0;
                    if (buff != null)
                    {
                        weapon = buff.BuffApplierWeapon;
                        skill = buff.GetSkill();
                        skillLevel = buff.level;
                    }
                    tempAmount = (int)calculatingTotalDamageOverTime;
                    if (tempAmount < 0)
                        tempAmount = 0;
                    CharacterEntity.CurrentHp -= tempAmount;
                    CharacterEntity.ReceivedDamage(HitBoxPosition.None, CharacterEntity.CacheTransform.position, instigator, DamageOverTimes, CombatAmountType.NormalDamage, tempAmount, weapon, skill, skillLevel, buff);
                    calculatingTotalDamageOverTime -= tempAmount;
                }
            }
        }
    }
}
