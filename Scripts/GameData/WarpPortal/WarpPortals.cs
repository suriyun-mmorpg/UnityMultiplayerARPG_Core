using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct WarpPortals
    {
        [System.Obsolete("`Map` is deprecated, use `Map Info` instead")]
        [Tooltip("`Map` is deprecated, use `Map Info` instead")]
        public UnityScene map;
        public MapInfo mapInfo;
        public WarpPortal[] warpPortals;
    }
}
