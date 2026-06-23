namespace MultiplayerARPG
{
    public partial class WorkbenchEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                _cacheItemCrafts?.Clear();
                _cacheItemCrafts = null;
            }
        }
    }
}
