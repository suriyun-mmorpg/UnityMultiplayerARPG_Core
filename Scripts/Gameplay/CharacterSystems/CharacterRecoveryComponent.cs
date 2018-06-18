using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float recoveryUpdateDeltaTime;
    #endregion

    protected void Update()
    {
        var deltaTime = Time.unscaledDeltaTime;
        var gameplayRule = GameInstance.Singleton.GameplayRule;
        UpdateRecovery(deltaTime, gameplayRule, this, CacheCharacterEntity);
    }

    protected static void UpdateRecovery(float deltaTime, BaseGameplayRule gameplayRule, CharacterRecoveryComponent recoveryData, BaseCharacterEntity characterEntity)
    {
        if (characterEntity.isRecaching || characterEntity.CurrentHp <= 0 || !characterEntity.IsServer)
            return;

        recoveryData.recoveryUpdateDeltaTime += deltaTime;
        if (recoveryData.recoveryUpdateDeltaTime >= RECOVERY_UPDATE_DURATION)
        {
            // Hp
            recoveryData.recoveryingHp += recoveryData.recoveryUpdateDeltaTime * gameplayRule.GetRecoveryHpPerSeconds(characterEntity);
            if (characterEntity.CurrentHp < characterEntity.CacheMaxHp)
            {
                if (recoveryData.recoveryingHp >= 1)
                {
                    var intRecoveryingHp = (int)recoveryData.recoveryingHp;
                    characterEntity.CurrentHp += intRecoveryingHp;
                    characterEntity.RequestCombatAmount(CombatAmountType.HpRecovery, intRecoveryingHp);
                    recoveryData.recoveryingHp -= intRecoveryingHp;
                }
            }
            else
                recoveryData.recoveryingHp = 0;

            // Decrease Hp
            recoveryData.decreasingHp += recoveryData.recoveryUpdateDeltaTime * gameplayRule.GetDecreasingHpPerSeconds(characterEntity);
            if (characterEntity.CurrentHp > 0)
            {
                if (recoveryData.decreasingHp >= 1)
                {
                    var intDecreasingHp = (int)recoveryData.decreasingHp;
                    characterEntity.CurrentHp -= intDecreasingHp;
                    recoveryData.decreasingHp -= intDecreasingHp;
                }
            }
            else
                recoveryData.decreasingHp = 0;

            // Mp
            recoveryData.recoveryingMp += recoveryData.recoveryUpdateDeltaTime * gameplayRule.GetRecoveryMpPerSeconds(characterEntity);
            if (characterEntity.CurrentMp < characterEntity.CacheMaxMp)
            {
                if (recoveryData.recoveryingMp >= 1)
                {
                    var intRecoveryingMp = (int)recoveryData.recoveryingMp;
                    characterEntity.CurrentMp += intRecoveryingMp;
                    characterEntity.RequestCombatAmount(CombatAmountType.MpRecovery, intRecoveryingMp);
                    recoveryData.recoveryingMp -= intRecoveryingMp;
                }
            }
            else
                recoveryData.recoveryingMp = 0;

            // Decrease Mp
            recoveryData.decreasingMp += recoveryData.recoveryUpdateDeltaTime * gameplayRule.GetDecreasingMpPerSeconds(characterEntity);
            if (characterEntity.CurrentMp > 0)
            {
                if (recoveryData.decreasingMp >= 1)
                {
                    var intDecreasingMp = (int)recoveryData.decreasingMp;
                    characterEntity.CurrentMp -= intDecreasingMp;
                    recoveryData.decreasingMp -= intDecreasingMp;
                }
            }
            else
                recoveryData.decreasingMp = 0;

            // Stamina
            recoveryData.recoveryingStamina += recoveryData.recoveryUpdateDeltaTime * gameplayRule.GetRecoveryStaminaPerSeconds(characterEntity);
            if (characterEntity.CurrentStamina < characterEntity.CacheMaxStamina)
            {
                if (recoveryData.recoveryingStamina >= 1)
                {
                    var intRecoveryingStamina = (int)recoveryData.recoveryingStamina;
                    characterEntity.CurrentStamina += intRecoveryingStamina;
                    characterEntity.RequestCombatAmount(CombatAmountType.StaminaRecovery, intRecoveryingStamina);
                    recoveryData.recoveryingStamina -= intRecoveryingStamina;
                }
            }
            else
                recoveryData.recoveryingStamina = 0;

            // Decrease Stamina while sprinting
            recoveryData.decreasingStamina += recoveryData.recoveryUpdateDeltaTime * gameplayRule.GetDecreasingStaminaPerSeconds(characterEntity);
            if (characterEntity.isSprinting && characterEntity.CurrentStamina > 0)
            {
                if (recoveryData.decreasingStamina >= 1)
                {
                    var intDecreasingStamina = (int)recoveryData.decreasingStamina;
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
                    var intRecoveryingFood = (int)recoveryData.recoveryingFood;
                    characterEntity.CurrentFood += intRecoveryingFood;
                    characterEntity.RequestCombatAmount(CombatAmountType.FoodRecovery, intRecoveryingFood);
                    recoveryData.recoveryingFood -= intRecoveryingFood;
                }
            }
            else
                recoveryData.recoveryingFood = 0;

            // Decrease Food
            recoveryData.decreasingFood += recoveryData.recoveryUpdateDeltaTime * gameplayRule.GetDecreasingFoodPerSeconds(characterEntity);
            if (characterEntity.CurrentFood > 0)
            {
                if (recoveryData.decreasingFood >= 1)
                {
                    var intDecreasingFood = (int)recoveryData.decreasingFood;
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
                    var intRecoveryingWater = (int)recoveryData.recoveryingWater;
                    characterEntity.CurrentWater += intRecoveryingWater;
                    characterEntity.RequestCombatAmount(CombatAmountType.WaterRecovery, intRecoveryingWater);
                    recoveryData.recoveryingWater -= intRecoveryingWater;
                }
            }
            else
                recoveryData.recoveryingWater = 0;

            // Decrease Water
            recoveryData.decreasingWater += recoveryData.recoveryUpdateDeltaTime * gameplayRule.GetDecreasingWaterPerSeconds(characterEntity);
            if (characterEntity.CurrentWater > 0)
            {
                if (recoveryData.decreasingWater >= 1)
                {
                    var intDecreasingWater = (int)recoveryData.decreasingWater;
                    characterEntity.CurrentWater -= intDecreasingWater;
                    recoveryData.decreasingWater -= intDecreasingWater;
                }
            }
            else
                recoveryData.decreasingWater = 0;

            recoveryData.recoveryUpdateDeltaTime = 0;
        }

        characterEntity.ValidateRecovery();
    }
}
