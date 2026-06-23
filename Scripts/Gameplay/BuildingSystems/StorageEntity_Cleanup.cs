namespace MultiplayerARPG
{
    public partial class StorageEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                onInitialOpen?.RemoveAllListeners();
                onInitialOpen = null;
                onInitialClose?.RemoveAllListeners();
                onInitialClose = null;
                onOpen?.RemoveAllListeners();
                onOpen = null;
                onClose?.RemoveAllListeners();
                onClose = null;
            }
            _dirtyIsOpen = false;
        }
    }
}
