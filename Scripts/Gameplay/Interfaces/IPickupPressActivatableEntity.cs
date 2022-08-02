namespace MultiplayerARPG
{
    public interface IPickupPressActivatableEntity : IBaseActivatableEntity
    {
        bool CanActivateByPickupKey();
        void OnActivateByPickupKey();
    }
}
