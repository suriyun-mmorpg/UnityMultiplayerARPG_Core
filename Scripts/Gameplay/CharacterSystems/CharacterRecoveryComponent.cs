using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterRecoveryComponent : BaseCharacterComponent
    {
        private float updatingTime;
        private float deltaTime;
        private CharacterRecoveryData recoveryData;

        protected void Update()
        {
            if (!CacheEntity.IsServer)
                return;

            deltaTime = Time.unscaledDeltaTime;

            if (CacheEntity.IsRecaching || CacheEntity.IsDead())
                return;

            updatingTime += deltaTime;
            if (updatingTime >= gameplayRule.GetRecoveryUpdateDuration())
            {
                // Hp
                recoveryData.recoveryingHp += updatingTime * gameplayRule.GetRecoveryHpPerSeconds(CacheEntity);
                // Decrease Hp
                recoveryData.decreasingHp += updatingTime * gameplayRule.GetDecreasingHpPerSeconds(CacheEntity);
                // Mp
                recoveryData.recoveryingMp += updatingTime * gameplayRule.GetRecoveryMpPerSeconds(CacheEntity);
                // Decrease Mp
                recoveryData.decreasingMp += updatingTime * gameplayRule.GetDecreasingMpPerSeconds(CacheEntity);
                // Stamina
                recoveryData.recoveryingStamina += updatingTime * gameplayRule.GetRecoveryStaminaPerSeconds(CacheEntity);
                // Decrease Stamina
                recoveryData.decreasingStamina += updatingTime * gameplayRule.GetDecreasingStaminaPerSeconds(CacheEntity);
                // Decrease Food
                recoveryData.decreasingFood += updatingTime * gameplayRule.GetDecreasingFoodPerSeconds(CacheEntity);
                // Decrease Water
                recoveryData.decreasingWater += updatingTime * gameplayRule.GetDecreasingWaterPerSeconds(CacheEntity);

                recoveryData = recoveryData.Apply(CacheEntity, CacheEntity);
                CacheEntity.ValidateRecovery(CacheEntity);
                updatingTime = 0;
            }

        }
    }
}
