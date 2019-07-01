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

        [SerializeField]
        protected VehicleMoveSpeedType moveSpeedType;

        [Tooltip("Vehicle move speed")]
        [SerializeField]
        protected float moveSpeed = 5f;

        [Tooltip("This will multiplies with driver move speed as vehicle move speed")]
        [SerializeField]
        protected float driverMoveSpeedRate = 1.5f;

        [Tooltip("First seat is for driver")]
        [SerializeField]
        private List<VehicleSeat> seats = new List<VehicleSeat>();
        public List<VehicleSeat> Seats
        {
            get { return seats; }
        }

        private readonly Dictionary<byte, BaseGameEntity> passengers = new Dictionary<byte, BaseGameEntity>();

        public abstract bool IsDestroyWhenDriverExit { get; }

        protected override sealed void EntityAwake()
        {
            base.EntityAwake();
            gameObject.layer = gameInstance.characterLayer;
        }

        public override sealed float GetMoveSpeed()
        {
            if (moveSpeedType == VehicleMoveSpeedType.FixedMovedSpeed)
                return moveSpeed;
            BaseGameEntity driver;
            if (passengers.TryGetValue(0, out driver))
                return driver.GetMoveSpeed() * driverMoveSpeedRate;
            return 0f;
        }

        public bool IsAttackable(byte seatIndex)
        {
            return Seats[seatIndex].canAttack;
        }

        public void SetPassenger(byte seatIndex, BaseGameEntity gameEntity)
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
