using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct VehicleSeat
    {
        public Transform rideTransform;
        public Transform exitTransform;
        public bool canDrive;
        public bool canAttack;
    }
}
