using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class VehicleSeat
    {
        public Transform passengingTransform;
        public bool hidePassenger;
        public Transform exitTransform;
        [Tooltip("If this is not empty, it will used this transform instead of character's one")]
        public Transform meleeDamageTransform;
        [Tooltip("If this is not empty, it will used this transform instead of character's one")]
        public Transform missileDamageTransform;
        public bool canAttack;
        public bool canUseSkill;
        public bool overrideActionAnimation;
        public VehicleSeatCameraTarget cameraTarget;
    }
}
