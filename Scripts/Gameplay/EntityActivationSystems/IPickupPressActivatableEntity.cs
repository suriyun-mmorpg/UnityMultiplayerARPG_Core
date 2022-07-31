namespace MultiplayerARPG
{
    public interface IPickupPressActivatableEntity : IBaseActivatableEntity
    {
        bool CanPickupPressActivate();
        void OnPickupPressActivate();
    }
}
