namespace MultiplayerARPG
{
    public interface IHoldClickActivatableEntity : IBaseActivatableEntity
    {
        bool CanActivateByHoldClick();
        void OnActivateByHoldClick();
    }
}
