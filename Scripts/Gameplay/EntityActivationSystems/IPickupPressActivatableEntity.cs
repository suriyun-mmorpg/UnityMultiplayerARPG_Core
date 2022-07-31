namespace MultiplayerARPG
{
    public interface IPickupPressActivatableEntity
    {
        float GetActivatableDistance();
        bool ShouldBeAttackTarget();
        bool CanPickupPressActivate();
        void OnPickupPressActivate();
    }
}
