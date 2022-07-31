namespace MultiplayerARPG
{
    public interface IClickActivatableEntity : IBaseActivatableEntity
    {
        bool CanClickActivate();
        void OnClickActivate();
    }
}
