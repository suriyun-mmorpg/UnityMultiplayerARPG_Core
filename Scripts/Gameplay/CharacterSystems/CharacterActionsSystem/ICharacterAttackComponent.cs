namespace MultiplayerARPG
{
    public interface ICharacterAttackComponent
    {
        bool IsAttacking { get; }
        float LastAttackEndTime { get; }
        bool IsSkipMovementValidationWhileAttacking { get; }
        bool IsUseRootMotionWhileAttacking { get; }
        float MoveSpeedRateWhileAttacking { get; }
        MovementRestriction MovementRestrictionWhileAttacking { get; }

        void CancelAttack();
        void ClearAttackStates();
        void Attack(bool isLeftHand);
    }
}
