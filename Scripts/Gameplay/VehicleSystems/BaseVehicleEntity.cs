using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseVehicleEntity : BaseGameEntity, IVehicleEntity
    {
        [SerializeField]
        private VehicleType vehicleType;
        public VehicleType VehicleType { get { return vehicleType; } }

        [Tooltip("First seat is for driver")]
        [SerializeField]
        private List<VehicleSeat> seats = new List<VehicleSeat>();
        public List<VehicleSeat> Seats
        {
            get
            {
                return seats;
            }
        }

        private readonly Dictionary<byte, IGameEntity> passengers = new Dictionary<byte, IGameEntity>();

        public abstract bool IsDestroyWhenDriverExit { get; }

        protected override sealed void EntityAwake()
        {
            base.EntityAwake();
            gameObject.layer = gameInstance.characterLayer;
        }

        public bool IsAttackable(byte seatIndex)
        {
            return Seats[seatIndex].canAttack;
        }

        public void SetPassenger(byte seatIndex, IGameEntity gameEntity)
        {
            if (!passengers.ContainsKey(seatIndex))
                passengers.Add(seatIndex, gameEntity);
        }

        public bool RemovePassenger(byte seatIndex)
        {
            return passengers.Remove(seatIndex);
        }

        public bool IsSeatAvailable(byte seatIndex)
        {
            return !passengers.ContainsKey(seatIndex);
        }

        public bool GetAvailableSeat(out byte seatIndex)
        {
            seatIndex = 0;
            for (byte i = 0; i < (byte)Seats.Count; ++i)
            {
                if (IsSeatAvailable(i))
                {
                    seatIndex = i;
                    return true;
                }
            }
            return false;
        }
    }
}
