using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterRecoveryComponent : BaseCharacterComponent
    {
        #region Recovery System Data
        [HideInInspector, System.NonSerialized]
        public float recoveryingHp;
        [HideInInspector, System.NonSerialized]
        public float recoveryingMp;
        [HideInInspector, System.NonSerialized]
        public float recoveryingStamina;
        [HideInInspector, System.NonSerialized]
        public float recoveryingFood;
        [HideInInspector, System.NonSerialized]
        public float recoveryingWater;
        [HideInInspector, System.NonSerialized]
        public float decreasingHp;
        [HideInInspector, System.NonSerialized]
        public float decreasingMp;
        [HideInInspector, System.NonSerialized]
        public float decreasingStamina;
        [HideInInspector, System.NonSerialized]
        public float decreasingFood;
        [HideInInspector, System.NonSerialized]
        public float decreasingWater;
        [HideInInspector, System.NonSerialized]
        public float updatingTime;
        #endregion

        protected void Update()
        {
            UpdateRecovery(Time.unscaledDeltaTime, GameInstance.Singleton.GameplayRule, this, CacheEntity);
        }

        protected static void UpdateRecovery(float deltaTime, BaseGameplayRule gameplayRule, CharacterRecoveryComponent recoveryData, BaseCharacterEntity characterEntity)
        {
            if (characterEntity.IsRecaching || characterEntity.IsDead() || !characterEntity.IsServer)
                return;

            recoveryData.updatingTime += deltaTime;
            int tempAmount;
            if (recoveryData.updatingTime >= gameplayRule.GetRecoveryUpdateDuration())
            {
                // Hp
                recoveryData.recoveryingHp += recoveryData.updatingTime * gameplayRule.GetRecoveryHpPerSeconds(characterEntity);
                if (characterEntity.CurrentHp < characterEntity.MaxHp)
                {
                    if (recoveryData.recoveryingHp >= 1)
                    {
                        tempAmount = (int)recoveryData.recoveryingHp;
                        characterEntity.CurrentHp += tempAmount;
                        characterEntity.RequestCombatAmount(CombatAmountType.HpRecovery, tempAmount);
                        recoveryData.recoveryingHp -= tempAmount;
                    }
                }
                else
                    recoveryData.recoveryingHp = 0;

                // Decrease Hp
                recoveryData.decreasingHp += recoveryData.updatingTime * gameplayRule.GetDecreasingHpPerSeconds(characterEntity);
                if (characterEntity.CurrentHp > 0)
                {
                    if (recoveryData.decreasingHp >= 1)
                    {
                        tempAmount = (int)recoveryData.decreasingHp;
                        characterEntity.CurrentHp -= tempAmount;
                        characterEntity.RequestCombatAmount(CombatAmountType.NormalDamage, tempAmount);
                        recoveryData.decreasingHp -= tempAmount;
                    }
                }
                else
                    recoveryData.decreasingHp = 0;

                if (characterEntity.CurrentHp <= 0)
                    characterEntity.CurrentHp = 0;

                // Mp
                recoveryData.recoveryingMp += recoveryData.updatingTime * gameplayRule.GetRecoveryMpPerSeconds(characterEntity);
                if (characterEntity.CurrentMp < characterEntity.MaxMp)
                {
                    if (recoveryData.recoveryingMp >= 1)
                    {
                        tempAmount = (int)recoveryData.recoveryingMp;
                        characterEntity.CurrentMp += tempAmount;
                        characterEntity.RequestCombatAmount(CombatAmountType.MpRecovery, tempAmount);
                        recoveryData.recoveryingMp -= tempAmount;
                    }
                }
                else
                    recoveryData.recoveryingMp = 0;

                // Decrease Mp
                recoveryData.decreasingMp += recoveryData.updatingTime * gameplayRule.GetDecreasingMpPerSeconds(characterEntity);
                if (characterEntity.CurrentMp > 0)
                {
                    if (recoveryData.decreasingMp >= 1)
                    {
                        tempAmount = (int)recoveryData.decreasingMp;
                        characterEntity.CurrentMp -= tempAmount;
                        characterEntity.RequestCombatAmount(CombatAmountType.MpRecovery, tempAmount);
                        recoveryData.decreasingMp -= tempAmount;
                    }
                }
                else
                    recoveryData.decreasingMp = 0;

                if (characterEntity.CurrentMp <= 0)
                    characterEntity.CurrentMp = 0;

                // Stamina
                recoveryData.recoveryingStamina += recoveryData.updatingTime * gameplayRule.GetRecoveryStaminaPerSeconds(characterEntity);
                if (characterEntity.CurrentStamina < characterEntity.MaxStamina)
                {
                    if (recoveryData.recoveryingStamina >= 1)
                    {
                        tempAmount = (int)recoveryData.recoveryingStamina;
                        characterEntity.CurrentStamina += tempAmount;
                        characterEntity.RequestCombatAmount(CombatAmountType.StaminaRecovery, tempAmount);
                        recoveryData.recoveryingStamina -= tempAmount;
                    }
                }
                else
                    recoveryData.recoveryingStamina = 0;

                // Decrease Stamina while sprinting
                recoveryData.decreasingStamina += recoveryData.updatingTime * gameplayRule.GetDecreasingStaminaPerSeconds(characterEntity);
                if (characterEntity.CurrentStamina > 0)
                {
                    if (recoveryData.decreasingStamina >= 1)
                    {
                        tempAmount = (int)recoveryData.decreasingStamina;
                        characterEntity.CurrentStamina -= tempAmount;
                        characterEntity.RequestCombatAmount(CombatAmountType.StaminaRecovery, tempAmount);
                        recoveryData.decreasingStamina -= tempAmount;
                    }
                }
                else
                    recoveryData.decreasingStamina = 0;

                if (characterEntity.CurrentStamina <= 0)
                    characterEntity.CurrentStamina = 0;

                // Food
                if (characterEntity.CurrentFood < characterEntity.MaxFood)
                {
                    if (recoveryData.recoveryingFood >= 1)
                    {
                        tempAmount = (int)recoveryData.recoveryingFood;
                        characterEntity.CurrentFood += tempAmount;
                        characterEntity.RequestCombatAmount(CombatAmountType.FoodRecovery, tempAmount);
                        recoveryData.recoveryingFood -= tempAmount;
                    }
                }
                else
                    recoveryData.recoveryingFood = 0;

                // Decrease Food
                recoveryData.decreasingFood += recoveryData.updatingTime * gameplayRule.GetDecreasingFoodPerSeconds(characterEntity);
                if (characterEntity.CurrentFood > 0)
                {
                    if (recoveryData.decreasingFood >= 1)
                    {
                        tempAmount = (int)recoveryData.decreasingFood;
                        characterEntity.CurrentFood -= tempAmount;
                        characterEntity.RequestCombatAmount(CombatAmountType.FoodRecovery, tempAmount);
                        recoveryData.decreasingFood -= tempAmount;
                    }
                }
                else
                    recoveryData.decreasingFood = 0;

                if (characterEntity.CurrentFood <= 0)
                    characterEntity.CurrentFood = 0;

                // Water
                if (characterEntity.CurrentWater < characterEntity.MaxWater)
                {
                    if (recoveryData.recoveryingWater >= 1)
                    {
                        tempAmount = (int)recoveryData.recoveryingWater;
                        characterEntity.CurrentWater += tempAmount;
                        characterEntity.RequestCombatAmount(CombatAmountType.WaterRecovery, tempAmount);
                        recoveryData.recoveryingWater -= tempAmount;
                    }
                }
                else
                    recoveryData.recoveryingWater = 0;

                // Decrease Water
                recoveryData.decreasingWater += recoveryData.updatingTime * gameplayRule.GetDecreasingWaterPerSeconds(characterEntity);
                if (characterEntity.CurrentWater > 0)
                {
                    if (recoveryData.decreasingWater >= 1)
                    {
                        tempAmount = (int)recoveryData.decreasingWater;
                        characterEntity.CurrentWater -= tempAmount;
                        characterEntity.RequestCombatAmount(CombatAmountType.WaterRecovery, tempAmount);
                        recoveryData.decreasingWater -= tempAmount;
                    }
                }
                else
                    recoveryData.decreasingWater = 0;

                if (characterEntity.CurrentWater <= 0)
                    characterEntity.CurrentWater = 0;

                recoveryData.updatingTime = 0;
            }

            characterEntity.ValidateRecovery();
        }
    }
}
