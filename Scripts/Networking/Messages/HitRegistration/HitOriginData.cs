using UnityEngine;

namespace MultiplayerARPG
{
    public struct HitOriginData
    {
        public byte TriggerIndex { get; set; }
        public byte SpreadIndex { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
    }
}
