namespace MultiplayerARPG
{
    public interface IActivatePressActivatableEntity : IBaseActivatableEntity
    {
        bool CanKeyPressActivate();
        void OnKeyPressActivate();
    }
}
