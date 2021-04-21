namespace MultiplayerARPG
{
    public interface ICharacterAttackComponent
    {
        bool IsAttacking { get; }
        float MoveSpeedRateWhileAttacking { get; }

        void CancelAttack();
        void ClearAttackStates();
        bool Attack(bool isLeftHand);
    }
}
