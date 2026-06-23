namespace MultiplayerARPG
{
    public partial class QueuedWorkbenchEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                itemCraftFormulas.Nullify();
                _cacheItemCraftFormulas?.Clear();
                _cacheItemCraftFormulas = null;
            }
            TimeCounter = 0f;
        }
    }
}
