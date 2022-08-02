namespace MultiplayerARPG
{
    public interface IPickupActivatableEntity : IBaseActivatableEntity
    {
        bool CanPickupActivate();
        void OnPickupActivate();
    }
}
