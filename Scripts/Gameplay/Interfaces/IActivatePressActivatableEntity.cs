namespace MultiplayerARPG
{
    public interface IActivatePressActivatableEntity : IBaseActivatableEntity
    {
        bool CanActivateByActivateKey();
        void OnActivateByActivateKey();
    }
}
