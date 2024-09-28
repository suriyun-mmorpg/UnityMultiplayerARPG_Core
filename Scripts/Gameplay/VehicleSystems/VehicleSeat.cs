using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class VehicleSeat
    {
        public Transform passengingTransform;
        public Transform exitTransform;
        public bool canAttack;
        public bool canUseSkill;
        public VehicleSeatCameraTarget cameraTarget;
    }
}
