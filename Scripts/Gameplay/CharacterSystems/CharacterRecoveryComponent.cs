using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterRecoveryComponent : BaseGameEntityComponent<BaseCharacterEntity>
    {
        private float updatingTime;
        private float deltaTime;
        private CharacterRecoveryData recoveryData;
        private bool isClearRecoveryData;

        public override void EntityStart()
        {
            recoveryData = new CharacterRecoveryData(Entity, null);
        }

        public override sealed void EntityUpdate()
        {
            if (!Entity.IsServer)
                return;

            deltaTime = Time.unscaledDeltaTime;

            if (Entity.IsRecaching)
                return;

            if (Entity.IsDead())
            {
                if (!isClearRecoveryData)
                {
                    isClearRecoveryData = true;
                    recoveryData.Clear();
                }
                return;
            }
            isClearRecoveryData = false;

            updatingTime += deltaTime;
            if (updatingTime >= CurrentGameplayRule.GetRecoveryUpdateDuration())
            {
                // Hp
                recoveryData.RecoveryingHp += updatingTime * CurrentGameplayRule.GetRecoveryHpPerSeconds(Entity);
                // Decrease Hp
                recoveryData.DecreasingHp += updatingTime * CurrentGameplayRule.GetDecreasingHpPerSeconds(Entity);
                // Mp
                recoveryData.RecoveryingMp += updatingTime * CurrentGameplayRule.GetRecoveryMpPerSeconds(Entity);
                // Decrease Mp
                recoveryData.DecreasingMp += updatingTime * CurrentGameplayRule.GetDecreasingMpPerSeconds(Entity);
                // Stamina
                recoveryData.RecoveryingStamina += updatingTime * CurrentGameplayRule.GetRecoveryStaminaPerSeconds(Entity);
                // Decrease Stamina
                recoveryData.DecreasingStamina += updatingTime * CurrentGameplayRule.GetDecreasingStaminaPerSeconds(Entity);
                // Decrease Food
                recoveryData.DecreasingFood += updatingTime * CurrentGameplayRule.GetDecreasingFoodPerSeconds(Entity);
                // Decrease Water
                recoveryData.DecreasingWater += updatingTime * CurrentGameplayRule.GetDecreasingWaterPerSeconds(Entity);
                // Apply
                recoveryData.Apply();
                updatingTime = 0;
            }
        }
    }
}
