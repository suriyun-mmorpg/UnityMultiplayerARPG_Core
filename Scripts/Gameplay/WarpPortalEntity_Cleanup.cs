namespace MultiplayerARPG
{
    public partial class WarpPortalEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                warpSignals?.Nullify();
                warpToMapInfo = null;
            }
        }
    }
}
