namespace MultiplayerARPG
{
    public interface IClickActivatableEntity : IBaseActivatableEntity
    {
        bool CanActivateByClick();
        void OnActivateByClick();
    }
}
