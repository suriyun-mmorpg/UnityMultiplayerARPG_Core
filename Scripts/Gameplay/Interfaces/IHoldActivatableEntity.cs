namespace MultiplayerARPG
{
    public interface IHoldActivatableEntity : IBaseActivatableEntity
    {
        bool CanHoldActivate();
        void OnHoldActivate();
    }
}
