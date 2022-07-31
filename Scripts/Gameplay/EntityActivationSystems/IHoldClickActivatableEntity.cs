namespace MultiplayerARPG
{
    public interface IHoldClickActivatableEntity : IBaseActivatableEntity
    {
        bool CanHoldClickActivate();
        void OnHoldClickActivate();
    }
}
