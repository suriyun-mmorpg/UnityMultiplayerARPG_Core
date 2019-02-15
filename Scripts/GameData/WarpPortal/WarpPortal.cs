using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct WarpPortal
    {
        public WarpPortalEntity entityPrefab;
        public Vector3 position;
        public WarpPortalType warpPortalType;
        public MapInfo warpToMapInfo;
        public Vector3 warpToPosition;
        [Header("Deprecated")]
        [System.Obsolete("`Warp To Map` is deprecated, use `Warp To Map Info` instead")]
        [Tooltip("`Warp To Map` is deprecated, use `Warp To Map Info` instead")]
        public UnityScene warpToMap;
    }
}
