namespace MultiplayerARPG
{
    public interface ICharacterAttackComponent
    {
        bool IsAttacking { get; }
        float MoveSpeedRateWhileAttacking { get; }

        void CancelAttack();
        void ClearAttackStates();
        void SimulateLaunchDamageEntity(SimulateLaunchDamageEntityData data);
        bool Attack(bool isLeftHand);
    }
}
