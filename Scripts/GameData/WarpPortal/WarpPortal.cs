using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct WarpPortal
    {
        public Vector3 position;
        public UnityScene warpToMap;
        public Vector3 warpToPosition;
    }
}
