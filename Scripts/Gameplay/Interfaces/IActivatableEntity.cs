namespace MultiplayerARPG
{
    public interface IActivatableEntity : IBaseActivatableEntity
    {
        bool CanActivate();
        void OnActivate();
    }
}
