using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public interface ICharacterAttackComponent
    {
        bool IsAttacking { get; }
        float LastAttackEndTime { get; }
        bool LastAttackSkipMovementValidation { get; }
        float MoveSpeedRateWhileAttacking { get; }
        MovementRestriction MovementRestrictionWhileAttacking { get; }
        float AttackTotalDuration { get; set; }
        float[] AttackTriggerDurations { get; set; }

        void CancelAttack();
        void ClearAttackStates();
        void Attack(bool isLeftHand);
    }
}
