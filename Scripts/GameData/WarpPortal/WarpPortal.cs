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
    }
}
