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
            if (!Entity.IsServer)
                return;

            deltaTime = Time.unscaledDeltaTime;

            if (Entity.IsRecaching || Entity.IsDead())
                return;

            updatingTime += deltaTime;
            if (updatingTime >= CurrentGameplayRule.GetRecoveryUpdateDuration())
            {
                // Hp
                recoveryData.recoveryingHp += updatingTime * CurrentGameplayRule.GetRecoveryHpPerSeconds(Entity);
                // Decrease Hp
                recoveryData.decreasingHp += updatingTime * CurrentGameplayRule.GetDecreasingHpPerSeconds(Entity);
                // Mp
                recoveryData.recoveryingMp += updatingTime * CurrentGameplayRule.GetRecoveryMpPerSeconds(Entity);
                // Decrease Mp
                recoveryData.decreasingMp += updatingTime * CurrentGameplayRule.GetDecreasingMpPerSeconds(Entity);
                // Stamina
                recoveryData.recoveryingStamina += updatingTime * CurrentGameplayRule.GetRecoveryStaminaPerSeconds(Entity);
                // Decrease Stamina
                recoveryData.decreasingStamina += updatingTime * CurrentGameplayRule.GetDecreasingStaminaPerSeconds(Entity);
                // Decrease Food
                recoveryData.decreasingFood += updatingTime * CurrentGameplayRule.GetDecreasingFoodPerSeconds(Entity);
                // Decrease Water
                recoveryData.decreasingWater += updatingTime * CurrentGameplayRule.GetDecreasingWaterPerSeconds(Entity);

                recoveryData = recoveryData.Apply(Entity, Entity.GetInfo());
                // Dead by illness, so no causer
                Entity.ValidateRecovery(new EntityInfo());
                updatingTime = 0;
            }

        }
    }
}
