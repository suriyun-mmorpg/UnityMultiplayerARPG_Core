using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class MountEntity : BaseGameEntity, IVehicleEntity
    {
        [SerializeField]
        private VehicleType vehicleType;
        public VehicleType VehicleType { get { return vehicleType; } }

        [Tooltip("Mount Entity have only 1 seat")]
        [SerializeField]
        private VehicleSeat seat;
        private List<VehicleSeat> seats;
        public List<VehicleSeat> Seats
        {
            get
            {
                if (seats == null)
                {
                    seats = new List<VehicleSeat>();
                    seats.Add(seat);
                }
                return seats;
            }
        }

        [SerializeField]
        private float moveSpeed = 5f;

        public override sealed float GetMoveSpeed()
        {
            return moveSpeed;
        }

        public bool IsDriveable(byte seatIndex)
        {
            // Mount entity always driveable
            return true;
        }

        public bool IsAttackable(byte seatIndex)
        {
            return seat.canAttack;
        }

        public bool IsDestroyWhenExit(byte seatIndex)
        {
            // Mount entity always destroyed when exit
            return true;
        }
    }
}
