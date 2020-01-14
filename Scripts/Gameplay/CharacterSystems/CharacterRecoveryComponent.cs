using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterRecoveryComponent : BaseGameEntityComponent<BaseCharacterEntity>
    {
        private float updatingTime;
        private float deltaTime;
        private CharacterRecoveryData recoveryData;

        public override sealed void EntityUpdate()
        {
            if (!CacheEntity.IsServer)
                return;

            deltaTime = Time.unscaledDeltaTime;

            if (CacheEntity.IsRecaching || CacheEntity.IsDead())
                return;

            updatingTime += deltaTime;
            if (updatingTime >= CurrentGameplayRule.GetRecoveryUpdateDuration())
            {
                // Hp
                recoveryData.recoveryingHp += updatingTime * CurrentGameplayRule.GetRecoveryHpPerSeconds(CacheEntity);
                // Decrease Hp
                recoveryData.decreasingHp += updatingTime * CurrentGameplayRule.GetDecreasingHpPerSeconds(CacheEntity);
                // Mp
                recoveryData.recoveryingMp += updatingTime * CurrentGameplayRule.GetRecoveryMpPerSeconds(CacheEntity);
                // Decrease Mp
                recoveryData.decreasingMp += updatingTime * CurrentGameplayRule.GetDecreasingMpPerSeconds(CacheEntity);
                // Stamina
                recoveryData.recoveryingStamina += updatingTime * CurrentGameplayRule.GetRecoveryStaminaPerSeconds(CacheEntity);
                // Decrease Stamina
                recoveryData.decreasingStamina += updatingTime * CurrentGameplayRule.GetDecreasingStaminaPerSeconds(CacheEntity);
                // Decrease Food
                recoveryData.decreasingFood += updatingTime * CurrentGameplayRule.GetDecreasingFoodPerSeconds(CacheEntity);
                // Decrease Water
                recoveryData.decreasingWater += updatingTime * CurrentGameplayRule.GetDecreasingWaterPerSeconds(CacheEntity);

                recoveryData = recoveryData.Apply(CacheEntity, CacheEntity);
                // Dead by illness, so no causer
                CacheEntity.ValidateRecovery();
                updatingTime = 0;
            }

        }
    }
}
