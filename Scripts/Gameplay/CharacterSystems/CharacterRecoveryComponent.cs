using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterRecoveryComponent : BaseCharacterComponent
    {
        public const float RECOVERY_UPDATE_DURATION = 0.5f;

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
            UpdateRecovery(Time.unscaledDeltaTime, GameInstance.Singleton.GameplayRule, this, CacheCharacterEntity);
        }

        protected static void UpdateRecovery(float deltaTime, BaseGameplayRule gameplayRule, CharacterRecoveryComponent recoveryData, BaseCharacterEntity characterEntity)
        {
            if (characterEntity.isRecaching || characterEntity.IsDead() || !characterEntity.IsServer)
                return;

            recoveryData.updatingTime += deltaTime;
            if (recoveryData.updatingTime >= RECOVERY_UPDATE_DURATION)
            {
                // Hp
                recoveryData.recoveryingHp += recoveryData.updatingTime * gameplayRule.GetRecoveryHpPerSeconds(characterEntity);
                if (characterEntity.CurrentHp < characterEntity.CacheMaxHp)
                {
                    if (recoveryData.recoveryingHp >= 1)
                    {
                        int intRecoveryingHp = (int)recoveryData.recoveryingHp;
                        characterEntity.CurrentHp += intRecoveryingHp;
                        characterEntity.RequestCombatAmount(CombatAmountType.HpRecovery, intRecoveryingHp);
                        recoveryData.recoveryingHp -= intRecoveryingHp;
                    }
                }
                else
                    recoveryData.recoveryingHp = 0;

                // Decrease Hp
                recoveryData.decreasingHp += recoveryData.updatingTime * gameplayRule.GetDecreasingHpPerSeconds(characterEntity);
                if (!characterEntity.IsDead())
                {
                    if (recoveryData.decreasingHp >= 1)
                    {
                        int intDecreasingHp = (int)recoveryData.decreasingHp;
                        characterEntity.CurrentHp -= intDecreasingHp;
                        recoveryData.decreasingHp -= intDecreasingHp;
                    }
                }
                else
                    recoveryData.decreasingHp = 0;

                // Mp
                recoveryData.recoveryingMp += recoveryData.updatingTime * gameplayRule.GetRecoveryMpPerSeconds(characterEntity);
                if (characterEntity.CurrentMp < characterEntity.CacheMaxMp)
                {
                    if (recoveryData.recoveryingMp >= 1)
                    {
                        int intRecoveryingMp = (int)recoveryData.recoveryingMp;
                        characterEntity.CurrentMp += intRecoveryingMp;
                        characterEntity.RequestCombatAmount(CombatAmountType.MpRecovery, intRecoveryingMp);
                        recoveryData.recoveryingMp -= intRecoveryingMp;
                    }
                }
                else
                    recoveryData.recoveryingMp = 0;

                // Decrease Mp
                recoveryData.decreasingMp += recoveryData.updatingTime * gameplayRule.GetDecreasingMpPerSeconds(characterEntity);
                if (!characterEntity.IsDead() && characterEntity.CurrentMp > 0)
                {
                    if (recoveryData.decreasingMp >= 1)
                    {
                        int intDecreasingMp = (int)recoveryData.decreasingMp;
                        characterEntity.CurrentMp -= intDecreasingMp;
                        recoveryData.decreasingMp -= intDecreasingMp;
                    }
                }
                else
                    recoveryData.decreasingMp = 0;

                // Stamina
                recoveryData.recoveryingStamina += recoveryData.updatingTime * gameplayRule.GetRecoveryStaminaPerSeconds(characterEntity);
                if (characterEntity.CurrentStamina < characterEntity.CacheMaxStamina)
                {
                    if (recoveryData.recoveryingStamina >= 1)
                    {
                        int intRecoveryingStamina = (int)recoveryData.recoveryingStamina;
                        characterEntity.CurrentStamina += intRecoveryingStamina;
                        characterEntity.RequestCombatAmount(CombatAmountType.StaminaRecovery, intRecoveryingStamina);
                        recoveryData.recoveryingStamina -= intRecoveryingStamina;
                    }
                }
                else
                    recoveryData.recoveryingStamina = 0;

                // Decrease Stamina while sprinting
                recoveryData.decreasingStamina += recoveryData.updatingTime * gameplayRule.GetDecreasingStaminaPerSeconds(characterEntity);
                if (!characterEntity.IsDead() && characterEntity.isSprinting && characterEntity.CurrentStamina > 0)
                {
                    if (recoveryData.decreasingStamina >= 1)
                    {
                        int intDecreasingStamina = (int)recoveryData.decreasingStamina;
                        characterEntity.CurrentStamina -= intDecreasingStamina;
                        recoveryData.decreasingStamina -= intDecreasingStamina;
                    }
                }
                else
                    recoveryData.decreasingStamina = 0;

                // Food
                if (characterEntity.CurrentFood < characterEntity.CacheMaxFood)
                {
                    if (recoveryData.recoveryingFood >= 1)
                    {
                        int intRecoveryingFood = (int)recoveryData.recoveryingFood;
                        characterEntity.CurrentFood += intRecoveryingFood;
                        characterEntity.RequestCombatAmount(CombatAmountType.FoodRecovery, intRecoveryingFood);
                        recoveryData.recoveryingFood -= intRecoveryingFood;
                    }
                }
                else
                    recoveryData.recoveryingFood = 0;

                // Decrease Food
                recoveryData.decreasingFood += recoveryData.updatingTime * gameplayRule.GetDecreasingFoodPerSeconds(characterEntity);
                if (!characterEntity.IsDead() && characterEntity.CurrentFood > 0)
                {
                    if (recoveryData.decreasingFood >= 1)
                    {
                        int intDecreasingFood = (int)recoveryData.decreasingFood;
                        characterEntity.CurrentFood -= intDecreasingFood;
                        recoveryData.decreasingFood -= intDecreasingFood;
                    }
                }
                else
                    recoveryData.decreasingFood = 0;

                // Water
                if (characterEntity.CurrentWater < characterEntity.CacheMaxWater)
                {
                    if (recoveryData.recoveryingWater >= 1)
                    {
                        int intRecoveryingWater = (int)recoveryData.recoveryingWater;
                        characterEntity.CurrentWater += intRecoveryingWater;
                        characterEntity.RequestCombatAmount(CombatAmountType.WaterRecovery, intRecoveryingWater);
                        recoveryData.recoveryingWater -= intRecoveryingWater;
                    }
                }
                else
                    recoveryData.recoveryingWater = 0;

                // Decrease Water
                recoveryData.decreasingWater += recoveryData.updatingTime * gameplayRule.GetDecreasingWaterPerSeconds(characterEntity);
                if (!characterEntity.IsDead() && characterEntity.CurrentWater > 0)
                {
                    if (recoveryData.decreasingWater >= 1)
                    {
                        int intDecreasingWater = (int)recoveryData.decreasingWater;
                        characterEntity.CurrentWater -= intDecreasingWater;
                        recoveryData.decreasingWater -= intDecreasingWater;
                    }
                }
                else
                    recoveryData.decreasingWater = 0;

                recoveryData.updatingTime = 0;
            }

            characterEntity.ValidateRecovery();
        }
    }
}
