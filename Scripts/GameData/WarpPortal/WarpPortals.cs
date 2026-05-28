namespace MultiplayerARPG
{
    [System.Serializable]
    public class WarpPortals
    {
        public BaseMapInfo mapInfo;
        public WarpPortal[] warpPortals = new WarpPortal[0];

#if UNITY_EDITOR
        public bool ValidateAddressableHashAssetIDs()
        {
            bool hasChanges = false;
            foreach (WarpPortal warpPortal in warpPortals)
            {
                hasChanges |= warpPortal.ValidateAddressableHashAssetID();
            }
            return hasChanges;
        }
#endif
    }
}
