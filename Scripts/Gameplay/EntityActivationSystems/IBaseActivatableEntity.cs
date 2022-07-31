namespace MultiplayerARPG
{
    public interface IBaseActivatableEntity
    {
        byte GetActivatablePriority();
        float GetActivatableDistance();
        bool ShouldBeAttackTarget();
    }
}
