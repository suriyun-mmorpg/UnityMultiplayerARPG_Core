using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct VehicleSeat
    {
        public static readonly VehicleSeat Empty = new VehicleSeat();
        public Transform passengingTransform;
        public Transform exitTransform;
        public bool canAttack;
        public bool canUseSkill;
        public VehicleSeatCameraTarget cameraTarget;
    }
}
