using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct CrosshairSetting
    {
        public bool hidden;
        public float expandPerFrameWhileMoving;
        public float expandPerFrameWhileAttacking;
        public float shrinkPerFrame;
        public float shrinkPerFrameWhenAttacked;
        public float minSpread;
        public float maxSpread;
        [FormerlySerializedAs("recoil")]
        [FormerlySerializedAs("recoilY")]
        [Tooltip("X axis rotation")]
        public float recoilPitch;
        [FormerlySerializedAs("recoilX")]
        [Tooltip("Y axis rotation")]
        public float recoilYaw;
        [Tooltip("Z axis rotation")]
        public float recoilRoll;
    }
}
