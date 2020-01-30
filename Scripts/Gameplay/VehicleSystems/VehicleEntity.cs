using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    // TODO: Derived from damageable entity
    public class VehicleEntity : BaseGameEntity, IVehicleEntity
    {
        [Header("Vehicle Settings")]
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

        [SerializeField]
        private SyncListUInt syncPassengerIds = new SyncListUInt();

        private readonly Dictionary<byte, BaseGameEntity> passengers = new Dictionary<byte, BaseGameEntity>();

        public virtual bool IsDestroyWhenDriverExit { get { return false; } }
        public virtual bool HasDriver { get { return passengers.ContainsKey(0); } }
        private MovementSecure defaultMovementSecure;

        protected override sealed void EntityAwake()
        {
            base.EntityAwake();
            gameObject.layer = CurrentGameInstance.characterLayer;
            defaultMovementSecure = MovementSecure;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            syncPassengerIds.onOperation = (op, index) =>
            {
                LiteNetLibIdentity identity;
                if (index < syncPassengerIds.Count)
                {
                    // Add passenger entity to dictionary if the id > 0
                    if (syncPassengerIds[index] == 0)
                        passengers.Remove((byte)index);
                    else if (Manager.Assets.TryGetSpawnedObject(syncPassengerIds[index], out identity))
                        passengers[(byte)index] = identity.GetComponent<BaseGameEntity>();
                }
            };
            if (IsServer)
            {
                // Prepare passengers data, add data at server then it wil be synced to clients
                while (syncPassengerIds.Count < Seats.Count)
                {
                    syncPassengerIds.Add(0);
                }
            }
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            if (HasDriver)
            {
                // Client will control movement
                MovementSecure = defaultMovementSecure;
            }
            else
            {
                // Server will control movement
                MovementSecure = MovementSecure.ServerAuthoritative;
            }
        }

        public override sealed float GetMoveSpeed()
        {
            if (moveSpeedType == VehicleMoveSpeedType.FixedMovedSpeed)
                return moveSpeed;
            BaseGameEntity driver;
            if (IsOwnerClient && passengers.TryGetValue(0, out driver))
                return driver.GetMoveSpeed() * driverMoveSpeedRate;
            return 0f;
        }

        public override bool CanMove()
        {
            BaseGameEntity driver;
            if (IsOwnerClient && passengers.TryGetValue(0, out driver))
                return driver.CanMove();
            return false;
        }

        public bool IsAttackable(byte seatIndex)
        {
            return Seats[seatIndex].canAttack;
        }

        public void SetPassenger(byte seatIndex, BaseGameEntity gameEntity)
        {
            if (!IsServer)
                return;
            syncPassengerIds[seatIndex] = gameEntity.ObjectId;
        }

        public bool RemovePassenger(byte seatIndex)
        {
            if (!IsServer)
                return false;
            if (seatIndex < syncPassengerIds.Count)
            {
                syncPassengerIds[seatIndex] = 0;
                return true;
            }
            return false;
        }

        public bool IsSeatAvailable(byte seatIndex)
        {
            return seatIndex < syncPassengerIds.Count && syncPassengerIds[seatIndex] == 0;
        }

        public bool GetAvailableSeat(out byte seatIndex)
        {
            seatIndex = 0;
            byte count = (byte)Seats.Count;
            for (byte i = 0; i < count; ++i)
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
