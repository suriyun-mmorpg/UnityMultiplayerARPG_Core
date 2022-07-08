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
            recoveryData = new CharacterRecoveryData(Entity);
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
                recoveryData.RecoveryingHp = CurrentGameplayRule.GetRecoveryHpPerSeconds(Entity);
                recoveryData.DecreasingHp = CurrentGameplayRule.GetDecreasingHpPerSeconds(Entity);
                recoveryData.RecoveryingMp = CurrentGameplayRule.GetRecoveryMpPerSeconds(Entity);
                recoveryData.DecreasingMp = CurrentGameplayRule.GetDecreasingMpPerSeconds(Entity);
                recoveryData.RecoveryingStamina = CurrentGameplayRule.GetRecoveryStaminaPerSeconds(Entity);
                recoveryData.DecreasingStamina = CurrentGameplayRule.GetDecreasingStaminaPerSeconds(Entity);
                recoveryData.DecreasingFood = CurrentGameplayRule.GetDecreasingFoodPerSeconds(Entity);
                recoveryData.DecreasingWater = CurrentGameplayRule.GetDecreasingWaterPerSeconds(Entity);
                recoveryData.Apply(updatingTime);
                updatingTime = 0;
            }
        }
    }
}
