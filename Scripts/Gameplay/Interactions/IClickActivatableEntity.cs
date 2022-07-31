namespace MultiplayerARPG
{
    public interface IClickActivatableEntity
    {
        float GetActivatableDistance();
        bool ShouldBeAttackTarget();
        bool CanClickActivate();
        void OnClickActivate();
    }
}
