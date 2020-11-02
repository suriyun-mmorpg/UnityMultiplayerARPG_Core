using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class VehicleEntity : DamageableEntity, IVehicleEntity
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
        public List<VehicleSeat> Seats { get { return seats; } }

        [SerializeField]
        protected bool canBeAttacked;

        [Header("Stats/Attributes")]
        // TODO: Vehicle can level up?
        [SerializeField]
        private short level;

        [SerializeField]
        private IncrementalInt hp;

        [SerializeField]
        [ArrayElementTitle("damageElement")]
        private ResistanceIncremental[] resistances;

        [SerializeField]
        [ArrayElementTitle("damageElement")]
        private ArmorIncremental[] armors;

        [SerializeField]
        [Tooltip("Delay before the entity destroyed, you may set some delay to play destroyed animation by `onVehicleDestroy` event before it's going to be destroyed from the game.")]
        protected float destroyDelay = 2f;

        [SerializeField]
        protected float destroyRespawnDelay = 5f;

        [Header("Events")]
        public UnityEvent onVehicleDestroy = new UnityEvent();

        [SerializeField]
        private SyncListUInt syncPassengerIds = new SyncListUInt();

        public virtual bool IsDestroyWhenDriverExit { get { return false; } }
        public virtual bool HasDriver { get { return passengers.ContainsKey(0); } }
        public virtual bool ShouldBeAttackTarget { get { return HasDriver && canBeAttacked && !this.IsDead(); } }
        public Dictionary<DamageElement, float> Resistances { get; private set; }
        public Dictionary<DamageElement, float> Armors { get; private set; }
        public override int MaxHp { get { return canBeAttacked ? hp.GetAmount(level) : 1; } }
        public Vector3 SpawnPosition { get; protected set; }

        // Private variables
        private readonly Dictionary<byte, BaseGameEntity> passengers = new Dictionary<byte, BaseGameEntity>();
        private MovementSecure defaultMovementSecure;
        private bool isDestroyed;

        protected override sealed void EntityAwake()
        {
            base.EntityAwake();
            gameObject.layer = CurrentGameInstance.characterLayer;
            isDestroyed = false;
            defaultMovementSecure = MovementSecure;
        }

        protected virtual void InitStats()
        {
            if (!IsServer)
                return;
            UpdateStats();
            CurrentHp = MaxHp;
        }

        /// <summary>
        /// Call this when vehicle level up
        /// </summary>
        public void UpdateStats()
        {
            Resistances = GameDataHelpers.CombineResistances(resistances, new Dictionary<DamageElement, float>(), level, 1);
            Armors = GameDataHelpers.CombineArmors(armors, new Dictionary<DamageElement, float>(), level, 1);
        }

        public override void OnSetup()
        {
            base.OnSetup();
            InitStats();
            SpawnPosition = CacheTransform.position;
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
            if (passengers.TryGetValue(0, out driver))
                return driver.GetMoveSpeed() * driverMoveSpeedRate;
            return 0f;
        }

        public override bool CanMove()
        {
            BaseGameEntity driver;
            if (passengers.TryGetValue(0, out driver))
                return driver.CanMove();
            return false;
        }

        public bool IsAttackable(byte seatIndex)
        {
            return Seats[seatIndex].canAttack;
        }

        public List<BaseGameEntity> GetAllPassengers()
        {
            List<BaseGameEntity> result = new List<BaseGameEntity>();
            foreach (BaseGameEntity passenger in passengers.Values)
            {
                if (passenger)
                    result.Add(passenger);
            }
            return result;
        }

        public BaseGameEntity GetPassenger(byte seatIndex)
        {
            return passengers[seatIndex];
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
                BaseGameEntity passenger;
                if (Manager.TryGetEntityByObjectId(syncPassengerIds[seatIndex], out passenger))
                {
                    passenger.ExitedVehicle(
                        Seats[seatIndex].exitTransform != null ?
                        Seats[seatIndex].exitTransform.position :
                        passenger.CacheTransform.position);
                }
                syncPassengerIds[seatIndex] = 0;
                return true;
            }
            return false;
        }

        public void RemoveAllPassengers()
        {
            if (!IsServer)
                return;
            for (byte i = 0; i < syncPassengerIds.Count; ++i)
            {
                RemovePassenger(i);
            }
        }

        public bool IsSeatAvailable(byte seatIndex)
        {
            return !isDestroyed && seatIndex < syncPassengerIds.Count && syncPassengerIds[seatIndex] == 0;
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

        [AllRpc]
        private void AllOnVehicleDestroy()
        {
            if (onVehicleDestroy != null)
                onVehicleDestroy.Invoke();
        }

        public void CallAllOnVehicleDestroy()
        {
            RPC(AllOnVehicleDestroy);
        }

        protected override void ApplyReceiveDamage(Vector3 fromPosition, IGameEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, out CombatAmountType combatAmountType, out int totalDamage)
        {
            combatAmountType = CombatAmountType.Miss;
            totalDamage = 0;
            if (!canBeAttacked)
                return;

            // Calculate damages
            float calculatingTotalDamage = 0f;
            float calculatingDamage;
            MinMaxFloat damageAmount;
            foreach (DamageElement damageElement in damageAmounts.Keys)
            {
                damageAmount = damageAmounts[damageElement];
                calculatingDamage = damageElement.GetDamageReducedByResistance(Resistances, Armors, damageAmount.Random());
                if (calculatingDamage > 0f)
                    calculatingTotalDamage += calculatingDamage;
            }

            // Apply damages
            combatAmountType = CombatAmountType.NormalDamage;
            totalDamage = (int)calculatingTotalDamage;
            CurrentHp -= totalDamage;
        }

        public override void ReceivedDamage(Vector3 fromPosition, IGameEntity attacker, CombatAmountType combatAmountType, int damage, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            base.ReceivedDamage(fromPosition, attacker, combatAmountType, damage, weapon, skill, skillLevel);

            if (combatAmountType == CombatAmountType.Miss)
                return;

            // Do something when entity dead
            if (this.IsDead())
                Destroy();
        }

        public virtual void Destroy()
        {
            if (!IsServer)
                return;
            CurrentHp = 0;
            if (isDestroyed)
                return;
            isDestroyed = true;
            // Kick passengers
            RemoveAllPassengers();
            // Tell clients that the vehicle destroy to play animation at client
            CallAllOnVehicleDestroy();
            // Respawning later
            if (Identity.IsSceneObject)
                Manager.StartCoroutine(RespawnRoutine());
            // Destroy this entity
            NetworkDestroy(destroyDelay);
        }

        protected IEnumerator RespawnRoutine()
        {
            yield return new WaitForSecondsRealtime(destroyDelay + destroyRespawnDelay);
            isDestroyed = false;
            InitStats();
            Manager.Assets.NetworkSpawnScene(
                Identity.ObjectId,
                SpawnPosition,
                CurrentGameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity);
        }
    }
}
