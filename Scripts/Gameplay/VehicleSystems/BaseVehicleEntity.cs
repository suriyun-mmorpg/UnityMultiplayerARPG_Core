using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class BaseVehicleEntity : DamageableEntity, IVehicleEntity
    {
        [SerializeField]
        private VehicleType vehicleType;
        public VehicleType VehicleType { get { return vehicleType; } }

        public bool IsDrivable { get { return vehicleType != null; } }

        [SerializeField]
        private List<Transform> seats = new List<Transform>();
        public List<Transform> Seats { get { return seats; } }

        public float StoppingDistance { get { return 0; } }

        public override int MaxHp
        {
            get { return 1; }
        }

        public void FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            throw new System.NotImplementedException();
        }

        public void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            throw new System.NotImplementedException();
        }

        public void PointClickMovement(Vector3 position)
        {
            throw new System.NotImplementedException();
        }

        public void SetLookRotation(Vector3 eulerAngles)
        {
            throw new System.NotImplementedException();
        }

        public void StopMove()
        {
            throw new System.NotImplementedException();
        }

        public void Teleport(Vector3 position)
        {
            throw new System.NotImplementedException();
        }
    }
}
