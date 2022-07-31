namespace MultiplayerARPG
{
    public interface IHoldClickActivatableEntity
    {
        float GetActivatableDistance();
        bool ShouldBeAttackTarget();
        bool CanHoldClickActivate();
        void OnHoldClickActivate();
    }
}
