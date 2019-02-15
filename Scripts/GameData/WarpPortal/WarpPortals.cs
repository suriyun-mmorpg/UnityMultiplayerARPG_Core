using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct WarpPortals
    {
        public MapInfo mapInfo;
        public WarpPortal[] warpPortals;
        [Header("Deprecated")]
        [System.Obsolete("`Map` is deprecated, use `Map Info` instead")]
        [Tooltip("`Map` is deprecated, use `Map Info` instead")]
        public UnityScene map;
    }
}
