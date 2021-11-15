namespace MultiplayerARPG
{
    public interface ICharacterAttackComponent
    {
        bool IsAttacking { get; }
        float LastAttackEndTime { get; }
        float MoveSpeedRateWhileAttacking { get; }

        void CancelAttack();
        void ClearAttackStates();
        void Attack(bool isLeftHand);
    }
}
