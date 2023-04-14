using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class VehicleEntity : DamageableEntity, IVehicleEntity, IActivatableEntity
    {
        [Category(5, "Vehicle Settings")]
        [SerializeField]
        protected VehicleType vehicleType = null;
        public VehicleType VehicleType { get { return vehicleType; } }

        [SerializeField]
        protected VehicleMoveSpeedType moveSpeedType = VehicleMoveSpeedType.FixedMovedSpeed;

        [Tooltip("Vehicle move speed")]
        [SerializeField]
        protected float moveSpeed = 5f;

        [Tooltip("This will multiplies with driver move speed as vehicle move speed")]
        [SerializeField]
        protected float driverMoveSpeedRate = 1.5f;

        [Tooltip("First seat is for driver")]
        [SerializeField]
        protected List<VehicleSeat> seats = new List<VehicleSeat>();
        public List<VehicleSeat> Seats { get { return seats; } }

        [SerializeField]
        protected bool canBeAttacked;

        // TODO: Vehicle can level up?
        [SerializeField]
        protected int level = 0;

        [SerializeField]
        protected IncrementalInt hp = default;

        [SerializeField]
        [ArrayElementTitle("damageElement")]
        protected ResistanceIncremental[] resistances = new ResistanceIncremental[0];

        [SerializeField]
        [ArrayElementTitle("damageElement")]
        protected ArmorIncremental[] armors = new ArmorIncremental[0];

        [SerializeField]
        protected Buff buff = Buff.Empty;

        [SerializeField]
        [Tooltip("Delay before the entity destroyed, you may set some delay to play destroyed animation by `onVehicleDestroy` event before it's going to be destroyed from the game.")]
        protected float destroyDelay = 2f;

        [SerializeField]
        protected float destroyRespawnDelay = 5f;

        [Category("Events")]
        public UnityEvent onVehicleDestroy = new UnityEvent();

        [Category("Sync Fields")]
        [SerializeField]
        protected SyncListUInt syncPassengerIds = new SyncListUInt();

        public virtual bool IsDestroyWhenDriverExit { get { return false; } }
        public virtual bool HasDriver { get { return _passengers.ContainsKey(0); } }
        public Dictionary<DamageElement, float> Resistances { get; private set; }
        public Dictionary<DamageElement, float> Armors { get; private set; }
        public override bool IsImmune { get { return base.IsImmune || !canBeAttacked; } set { base.IsImmune = value; } }
        public override int MaxHp { get { return canBeAttacked ? hp.GetAmount(level) : 1; } }
        public Vector3 SpawnPosition { get; protected set; }
        public float DestroyDelay { get { return destroyDelay; } }
        public float DestroyRespawnDelay { get { return destroyRespawnDelay; } }

        protected readonly Dictionary<byte, BaseGameEntity> _passengers = new Dictionary<byte, BaseGameEntity>();
        protected readonly Dictionary<uint, UnityAction<LiteNetLibIdentity>> _spawnEvents = new Dictionary<uint, UnityAction<LiteNetLibIdentity>>();
        protected bool _isDestroyed;
        protected CalculatedBuff _cacheBuff = new CalculatedBuff();
        protected int _dirtyLevel = int.MinValue;

        protected override sealed void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.vehicleTag;
            gameObject.layer = CurrentGameInstance.vehicleLayer;
            _isDestroyed = false;
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
            SpawnPosition = EntityTransform.position;
            syncPassengerIds.onOperation += OnPassengerIdsOperation;
            if (IsServer)
            {
                // Prepare passengers data, add data at server then it wil be synced to clients
                while (syncPassengerIds.Count < Seats.Count)
                {
                    syncPassengerIds.Add(0);
                }
            }
            // Vehicle must not being destroyed when owner player is disconnect to avoid vehicle exiting issues
            Identity.DoNotDestroyWhenDisconnect = true;
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            syncPassengerIds.onOperation -= OnPassengerIdsOperation;
        }

        private void OnPassengerIdsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (index >= syncPassengerIds.Count)
                return;
            // Set passenger entity to dictionary if the id > 0
            uint passengerId = syncPassengerIds[index];
            if (passengerId == 0)
            {
                _passengers.Remove((byte)index);
                return;
            }
            if (Manager.Assets.TryGetSpawnedObject(passengerId, out LiteNetLibIdentity identity))
            {
                // Set the passenger
                BaseGameEntity passenger = identity.GetComponent<BaseGameEntity>();
                passenger.SetPassengingVehicle((byte)index, this);
                _passengers[(byte)index] = passenger;
            }
            else
            {
                // Create a new event to set passenger when passenger object spawn
                _spawnEvents[passengerId] = (identity) =>
                {
                    if (identity.ObjectId != passengerId)
                        return;
                    BaseGameEntity passenger = identity.GetComponent<BaseGameEntity>();
                    passenger.SetPassengingVehicle((byte)index, this);
                    _passengers[(byte)index] = passenger;
                    // Remove the event after passenger was set
                    Manager.Assets.onObjectSpawn.RemoveListener(_spawnEvents[passengerId]);
                    _spawnEvents.Remove(passengerId);
                };
                // Set the event
                Manager.Assets.onObjectSpawn.AddListener(_spawnEvents[passengerId]);
            }
        }

        public override sealed float GetMoveSpeed()
        {
            if (moveSpeedType == VehicleMoveSpeedType.FixedMovedSpeed)
                return moveSpeed;
            if (_passengers.TryGetValue(0, out BaseGameEntity driver))
                return driver.GetMoveSpeed() * driverMoveSpeedRate;
            return 0f;
        }

        public override bool CanMove()
        {
            if (_passengers.TryGetValue(0, out BaseGameEntity driver))
                return driver.CanMove();
            return false;
        }

        public override bool CanJump()
        {
            return true;
        }

        public override bool CanTurn()
        {
            return true;
        }

        public bool IsAttackable(byte seatIndex)
        {
            return Seats[seatIndex].canAttack;
        }

        public List<BaseGameEntity> GetAllPassengers()
        {
            List<BaseGameEntity> result = new List<BaseGameEntity>();
            foreach (BaseGameEntity passenger in _passengers.Values)
            {
                if (passenger)
                    result.Add(passenger);
            }
            return result;
        }

        public BaseGameEntity GetPassenger(byte seatIndex)
        {
            return _passengers[seatIndex];
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
            if (seatIndex >= syncPassengerIds.Count)
                return false;
            // Store exiting object ID
            uint passengerId = syncPassengerIds[seatIndex];
            // Set passenger ID to `0` to tell clients that the passenger is exiting
            syncPassengerIds[seatIndex] = 0;
            // Move passenger to exit transform
            if (Manager.TryGetEntityByObjectId(passengerId, out BaseGameEntity passenger))
            {
                if (Seats[seatIndex].exitTransform != null)
                {
                    passenger.ExitedVehicle(
                        Seats[seatIndex].exitTransform.position,
                        Seats[seatIndex].exitTransform.rotation);
                }
                else
                {
                    passenger.ExitedVehicle(
                        MovementTransform.position,
                        MovementTransform.rotation);
                }
            }
            return true;
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
            return !_isDestroyed && seatIndex < syncPassengerIds.Count && syncPassengerIds[seatIndex] == 0;
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

        protected override void ApplyReceiveDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel, int randomSeed, out CombatAmountType combatAmountType, out int totalDamage)
        {
            if (!canBeAttacked)
            {
                combatAmountType = CombatAmountType.Miss;
                totalDamage = 0;
                return;
            }
            // Calculate damages
            float calculatingTotalDamage = 0f;
            foreach (DamageElement damageElement in damageAmounts.Keys)
            {
                calculatingTotalDamage += damageElement.GetDamageReducedByResistance(Resistances, Armors, damageAmounts[damageElement].Random(randomSeed));
            }
            // Apply damages
            combatAmountType = CombatAmountType.NormalDamage;
            totalDamage = CurrentGameInstance.GameplayRule.GetTotalDamage(fromPosition, instigator, this, calculatingTotalDamage, weapon, skill, skillLevel);
            if (totalDamage < 0)
                totalDamage = 0;
            CurrentHp -= totalDamage;
        }

        public override void ReceivedDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CombatAmountType combatAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime = false)
        {
            base.ReceivedDamage(position, fromPosition, instigator, damageAmounts, combatAmountType, totalDamage, weapon, skill, skillLevel, buff, isDamageOverTime);

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
            if (_isDestroyed)
                return;
            _isDestroyed = true;
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
            _isDestroyed = false;
            InitStats();
            Manager.Assets.NetworkSpawnScene(
                Identity.ObjectId,
                SpawnPosition,
                CurrentGameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity);
        }

        public virtual float GetActivatableDistance()
        {
            return GameInstance.Singleton.conversationDistance;
        }

        public virtual bool ShouldClearTargetAfterActivated()
        {
            return true;
        }

        public virtual bool ShouldBeAttackTarget()
        {
            return HasDriver && canBeAttacked && !this.IsDead();
        }

        public virtual bool ShouldNotActivateAfterFollowed()
        {
            return false;
        }

        public virtual bool CanActivate()
        {
            return !this.IsDead() && GameInstance.PlayingCharacterEntity.PassengingVehicleEntity == null;
        }

        public virtual void OnActivate()
        {
            GameInstance.PlayingCharacterEntity.CallServerEnterVehicle(ObjectId);
        }

        private void MakeCache()
        {
            if (_dirtyLevel != level)
            {
                _dirtyLevel = level;
                _cacheBuff.Build(buff, level);
            }
        }

        public CalculatedBuff GetBuff()
        {
            MakeCache();
            return _cacheBuff;
        }
    }
}
