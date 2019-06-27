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

        [Header("Mount Entity have only 1 seat")]
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

        /// <summary>
        /// Mount Entity always drivable
        /// </summary>
        public bool IsDrivable { get { return true; } }
    }
}
