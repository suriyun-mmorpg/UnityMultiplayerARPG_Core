using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UnityEngine.UI;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(LiteNetLibIdentity))]
    public abstract class BaseGameEntity : LiteNetLibBehaviour, IGameEntity, IEntityMovement
    {
        [Header("Game Entity Settings")]
        [SerializeField]
        private Text textTitle;
        public Text TextTitle
        {
            get { return textTitle; }
        }

        [SerializeField]
        private Text textTitleB;
        public Text TextTitleB
        {
            get { return textTitleB; }
        }

        [Tooltip("These objects will be hidden on non owner objects")]
        [SerializeField]
        private GameObject[] ownerObjects;
        public GameObject[] OwnerObjects
        {
            get { return ownerObjects; }
        }

        [Tooltip("These objects will be hidden on owner objects")]
        [SerializeField]
        private GameObject[] nonOwnerObjects;
        public GameObject[] NonOwnerObjects
        {
            get { return nonOwnerObjects; }
        }

        [Header("Game Entity Sync Fields")]
        [SerializeField]
        protected SyncFieldString syncTitle = new SyncFieldString();
        public virtual string Title
        {
            get { return syncTitle.Value; }
            set { syncTitle.Value = value; }
        }

        [SerializeField]
        protected SyncFieldString syncTitleB = new SyncFieldString();
        public virtual string TitleB
        {
            get { return syncTitleB.Value; }
            set { syncTitleB.Value = value; }
        }

        [Header("Model and Transform Settings")]
        [SerializeField]
        protected GameEntityModel model;
        public GameEntityModel Model
        {
            get { return model; }
        }

        private Transform cacheTransform;
        public Transform CacheTransform
        {
            get
            {
                if (cacheTransform == null)
                    cacheTransform = GetComponent<Transform>();
                return cacheTransform;
            }
        }

        [Tooltip("Transform for position which camera will look at")]
        [SerializeField]
        private Transform cameraTargetTransform;
        public Transform CameraTargetTransform
        {
            get
            {
                if (cameraTargetTransform == null)
                    cameraTargetTransform = CacheTransform;
                if (PassengingVehicleEntity != null)
                {
                    if (PassengingVehicleSeat.cameraTarget == VehicleSeatCameraTarget.Vehicle)
                    {
                        if (PassengingVehicleEntity is BaseGameEntity)
                            return (PassengingVehicleEntity as BaseGameEntity).CameraTargetTransform;
                        else
                            return PassengingVehicleEntity.transform;
                    }
                }
                return cameraTargetTransform;
            }
        }

        [Header("Entity Movement Settings")]
        [SerializeField]
        private MovementSecure movementSecure;
        public MovementSecure MovementSecure { get { return movementSecure; } set { movementSecure = value; } }

        private BaseEntityMovement movement;
        public BaseEntityMovement Movement
        {
            get
            {
                if (movement == null)
                    movement = GetComponent<BaseEntityMovement>();
                return movement;
            }
            set { movement = value; }
        }

        public Transform MovementTransform
        {
            get
            {
                if (PassengingVehicleEntity != null)
                {
                    // Track movement position by vehicle entity
                    return PassengingVehicleEntity.transform;
                }
                return CacheTransform;
            }
        }

        private uint dirtyVehicleObjectId;
        private IVehicleEntity passengingVehicleEntity;
        public IVehicleEntity PassengingVehicleEntity
        {
            get
            {
                if ((passengingVehicleEntity == null || dirtyVehicleObjectId != PassengingVehicle.objectId) && PassengingVehicle.objectId > 0)
                {
                    dirtyVehicleObjectId = PassengingVehicle.objectId;
                    passengingVehicleEntity = null;
                    LiteNetLibIdentity identity;
                    if (BaseGameNetworkManager.Singleton.Assets.TryGetSpawnedObject(PassengingVehicle.objectId, out identity))
                    {
                        passengingVehicleEntity = identity.GetComponent<IVehicleEntity>();
                    }
                }
                // Clear current vehicle
                if (PassengingVehicle.objectId == 0)
                    passengingVehicleEntity = null;
                return passengingVehicleEntity;
            }
        }

        public VehicleType PassengingVehicleType
        {
            get
            {
                if (PassengingVehicleEntity == null)
                    return null;
                return PassengingVehicleEntity.VehicleType;
            }
        }

        public VehicleSeat PassengingVehicleSeat
        {
            get
            {
                if (PassengingVehicleEntity == null)
                    return default(VehicleSeat);
                return PassengingVehicleEntity.Seats[PassengingVehicle.seatIndex];
            }
        }

        public IEntityMovement ActiveMovement
        {
            get
            {
                if (PassengingVehicleEntity != null)
                    return PassengingVehicleEntity;
                return Movement;
            }
        }

        [Header("Entity Movement Sync Fields")]
        [SerializeField]
        protected SyncFieldByte movementState = new SyncFieldByte();
        public MovementState LocalMovementState { get; set; }
        public MovementState MovementState
        {
            get
            {
                if (IsOwnerClient && MovementSecure == MovementSecure.NotSecure)
                    return LocalMovementState;
                return (MovementState)movementState.Value;
            }
            set { movementState.Value = (byte)value; }
        }

        [SerializeField]
        protected SyncFieldByte extraMovementState = new SyncFieldByte();
        public ExtraMovementState LocalExtraMovementState { get; set; }
        public ExtraMovementState ExtraMovementState
        {
            get
            {
                if (IsOwnerClient && MovementSecure == MovementSecure.NotSecure)
                    return LocalExtraMovementState;
                return (ExtraMovementState)extraMovementState.Value;
            }
            set { extraMovementState.Value = (byte)value; }
        }

        [SerializeField]
        [FormerlySerializedAs("currentDirection")]
        protected SyncFieldDirectionVector2 direction2D = new SyncFieldDirectionVector2();
        public Vector2 LocalDirection2D { get; set; }
        public Vector2 Direction2D
        {
            get
            {
                if (IsOwnerClient && MovementSecure == MovementSecure.NotSecure)
                    return LocalDirection2D;
                return direction2D.Value;
            }
            set { direction2D.Value = value; }
        }

        public DirectionType2D DirectionType2D
        {
            get { return GameplayUtils.GetDirectionTypeByVector2(Direction2D); }
        }
        
        [SerializeField]
        protected SyncFieldPassengingVehicle passengingVehicle = new SyncFieldPassengingVehicle();
        public PassengingVehicle PassengingVehicle
        {
            get { return passengingVehicle.Value; }
            set { passengingVehicle.Value = value; }
        }
        
        public float StoppingDistance { get { return ActiveMovement == null ? 0.1f : ActiveMovement.StoppingDistance; } }
        public virtual float MoveAnimationSpeedMultiplier { get { return 1f; } }
        protected Vector3? teleportingPosition;

        public GameInstance CurrentGameInstance
        {
            get { return GameInstance.Singleton; }
        }

        public BaseGameplayRule CurrentGameplayRule
        {
            get { return CurrentGameInstance.GameplayRule; }
        }

        public BaseGameNetworkManager CurrentGameManager
        {
            get { return BaseGameNetworkManager.Singleton; }
        }

        public BaseGameEntity Entity
        {
            get { return this; }
        }

        protected IGameEntityComponent[] EntityComponents { get; private set; }
        
        #region Enter Area States
        // This will be TRUE when this character enter to safe area
        public bool IsInSafeArea { get; set; }
        // This will be TRUE when this character enter to water area
        public bool IsUnderWater { get; set; }
        #endregion

        #region Events
        public event GenericDelegate onStart;
        public event GenericDelegate onEnable;
        public event GenericDelegate onDisable;
        public event GenericDelegate onUpdate;
        public event GenericDelegate onLateUpdate;
        public event GenericDelegate onFixedUpdate;
        public event GenericDelegate onSetup;
        public event GenericDelegate onSetupNetElements;
        public event GenericDelegate onSetOwnerClient;
        public event NetworkDestroyDelegate onNetworkDestroy;
        #endregion

        private void Awake()
        {
            InitialRequiredComponents();

            EntityComponents = GetComponents<IGameEntityComponent>();
            for (int i = 0; i < EntityComponents.Length; ++i)
            {
                EntityComponents[i].EntityAwake();
                EntityComponents[i].Enabled = true;
            }

            EntityAwake();
            this.InvokeInstanceDevExtMethods("Awake");
        }

        protected virtual void EntityAwake() { }

        private void Start()
        {
            for (int i = 0; i < EntityComponents.Length; ++i)
            {
                if (EntityComponents[i].Enabled)
                    EntityComponents[i].EntityStart();
            }
            EntityStart();
            if (onStart != null)
                onStart.Invoke();
        }
        protected virtual void EntityStart() { }

        public override void OnSetOwnerClient(bool isOwnerClient)
        {
            EntityOnSetOwnerClient();
            if (onSetOwnerClient != null)
                onSetOwnerClient.Invoke();
        }
        protected virtual void EntityOnSetOwnerClient()
        {
            foreach (GameObject ownerObject in ownerObjects)
            {
                if (ownerObject == null) continue;
                ownerObject.SetActive(IsOwnerClient);
            }
            foreach (GameObject nonOwnerObject in nonOwnerObjects)
            {
                if (nonOwnerObject == null) continue;
                nonOwnerObject.SetActive(!IsOwnerClient);
            }
        }

        private void OnEnable()
        {
            EntityOnEnable();
            if (onEnable != null)
                onEnable.Invoke();
        }
        protected virtual void EntityOnEnable() { }

        private void OnDisable()
        {
            EntityOnDisable();
            if (onDisable != null)
                onDisable.Invoke();
        }
        protected virtual void EntityOnDisable() { }

        private void Update()
        {
            Profiler.BeginSample("Entity Components - Update");
            for (int i = 0; i < EntityComponents.Length; ++i)
            {
                if (EntityComponents[i].Enabled)
                    EntityComponents[i].EntityUpdate();
            }
            Profiler.EndSample();
            EntityUpdate();
            if (onUpdate != null)
                onUpdate.Invoke();
        }
        protected virtual void EntityUpdate()
        {
            if (Movement != null && Movement.Enabled != (PassengingVehicleEntity == null))
            {
                // Enable movement while not passenging any vehicle
                Movement.Enabled = PassengingVehicleEntity == null;
            }
            if (IsClient)
            {
                if (Model != null && Model is IMoveableModel)
                {
                    // Update movement animation
                    (Model as IMoveableModel).SetMoveAnimationSpeedMultiplier(MoveAnimationSpeedMultiplier);
                    (Model as IMoveableModel).SetMovementState(MovementState, ExtraMovementState, DirectionType2D, IsUnderWater);
                }
            }
        }

        private void LateUpdate()
        {
            Profiler.BeginSample("Entity Components - LateUpdate");
            for (int i = 0; i < EntityComponents.Length; ++i)
            {
                if (EntityComponents[i].Enabled)
                    EntityComponents[i].EntityLateUpdate();
            }
            Profiler.EndSample();
            EntityLateUpdate();
            if (onLateUpdate != null)
                onLateUpdate.Invoke();
        }
        protected virtual void EntityLateUpdate()
        {
            if (textTitle != null)
                textTitle.text = Title;
            if (textTitleB != null)
                textTitleB.text = TitleB;

            if (PassengingVehicleEntity != null)
            {
                // Snap character to vehicle seat
                CacheTransform.position = PassengingVehicleSeat.passengingTransform.position;
                CacheTransform.rotation = PassengingVehicleSeat.passengingTransform.rotation;
            }
        }

        private void FixedUpdate()
        {
            Profiler.BeginSample("Entity Components - FixedUpdate");
            for (int i = 0; i < EntityComponents.Length; ++i)
            {
                if (EntityComponents[i].Enabled)
                    EntityComponents[i].EntityFixedUpdate();
            }
            Profiler.EndSample();
            EntityFixedUpdate();
            if (onFixedUpdate != null)
                onFixedUpdate.Invoke();
        }
        protected virtual void EntityFixedUpdate() { }

        private void OnDestroy()
        {
            for (int i = 0; i < EntityComponents.Length; ++i)
            {
                EntityComponents[i].EntityOnDestroy();
            }
            EntityOnDestroy();
            this.InvokeInstanceDevExtMethods("OnDestroy");
        }
        protected virtual void EntityOnDestroy()
        {
            // Exit vehicle when destroy
            ExitVehicle();
        }

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            SetupNetElements();
#endif
        }

        public override void OnSetup()
        {
            base.OnSetup();

            if (onSetup != null)
                onSetup.Invoke();

            SetupNetElements();

            // Register network functions
            RegisterNetFunction<PackedUInt>(NetFuncEnterVehicle);
            RegisterNetFunction<PackedUInt, byte>(NetFuncEnterVehicleToSeat);
            RegisterNetFunction(NetFuncExitVehicle);
            RegisterNetFunction<byte>(NetFuncSetMovement);
            RegisterNetFunction<byte>(NetFuncSetExtraMovement);
            RegisterNetFunction<DirectionVector2>(NetFuncUpdateDirection);

            // Setup entity movement here to make it able to register net elements / functions
            for (int i = 0; i < EntityComponents.Length; ++i)
            {
                EntityComponents[i].EntityOnSetup();
            }

            if (teleportingPosition.HasValue)
            {
                Teleport(teleportingPosition.Value);
                teleportingPosition = null;
            }
        }

        protected virtual void SetupNetElements()
        {
            if (onSetupNetElements != null)
                onSetupNetElements.Invoke();
            syncTitle.deliveryMethod = DeliveryMethod.ReliableOrdered;
            syncTitle.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            syncTitleB.deliveryMethod = DeliveryMethod.ReliableOrdered;
            syncTitleB.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            // Movement data
            movementState.deliveryMethod = DeliveryMethod.Sequenced;
            movementState.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            movementState.doNotSyncInitialDataImmediately = true;
            extraMovementState.deliveryMethod = DeliveryMethod.Sequenced;
            extraMovementState.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            extraMovementState.doNotSyncInitialDataImmediately = true;
            direction2D.deliveryMethod = DeliveryMethod.Sequenced;
            direction2D.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            direction2D.doNotSyncInitialDataImmediately = true;
            passengingVehicle.deliveryMethod = DeliveryMethod.ReliableOrdered;
            passengingVehicle.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            passengingVehicle.doNotSyncInitialDataImmediately = true;
        }

        /// <summary>
        /// Override this function to initial required components
        /// </summary>
        public virtual void InitialRequiredComponents() { }

        #region Net Functions
        protected void NetFuncEnterVehicle(PackedUInt objectId)
        {
            LiteNetLibIdentity identity;
            if (BaseGameNetworkManager.Singleton.Assets.TryGetSpawnedObject(objectId, out identity))
            {
                IVehicleEntity vehicleEntity = identity.GetComponent<IVehicleEntity>();
                byte seatIndex;
                if (vehicleEntity != null &&
                    vehicleEntity.GetAvailableSeat(out seatIndex))
                    EnterVehicle(vehicleEntity, seatIndex);
            }
        }

        protected void NetFuncEnterVehicleToSeat(PackedUInt objectId, byte seatIndex)
        {
            LiteNetLibIdentity identity;
            if (BaseGameNetworkManager.Singleton.Assets.TryGetSpawnedObject(objectId, out identity))
            {
                IVehicleEntity vehicleEntity = identity.GetComponent<IVehicleEntity>();
                if (vehicleEntity != null)
                    EnterVehicle(vehicleEntity, seatIndex);
            }
        }

        protected void NetFuncExitVehicle()
        {
            // Call exit vehicle at server
            ExitVehicle();
        }

        protected void NetFuncSetMovement(byte movementState)
        {
            // Set data at server and sync to clients later
            MovementState = (MovementState)movementState;
        }

        protected void NetFuncSetExtraMovement(byte extraMovementState)
        {
            // Set data at server and sync to clients later
            ExtraMovementState = (ExtraMovementState)extraMovementState;
        }

        protected void NetFuncUpdateDirection(DirectionVector2 direction)
        {
            // Set data at server and sync to clients later
            Direction2D = direction;
        }
        #endregion

        #region Net Function Requests
        public void RequestEnterVehicle(uint objectId)
        {
            CallNetFunction(NetFuncEnterVehicle, FunctionReceivers.Server, new PackedUInt(objectId));
        }

        public void RequestEnterVehicleToSeat(uint objectId, byte seatIndex)
        {
            CallNetFunction(NetFuncEnterVehicleToSeat, FunctionReceivers.Server, new PackedUInt(objectId), seatIndex);
        }

        public void RequestExitVehicle()
        {
            CallNetFunction(NetFuncExitVehicle, FunctionReceivers.Server);
        }
        #endregion

        public override void OnNetworkDestroy(byte reasons)
        {
            base.OnNetworkDestroy(reasons);
            if (onNetworkDestroy != null)
                onNetworkDestroy.Invoke(reasons);
        }

        public virtual float GetMoveSpeed()
        {
            return 0;
        }

        public virtual bool CanMove()
        {
            return false;
        }

        public virtual bool CanSprint()
        {
            return false;
        }

        public virtual bool CanCrouch()
        {
            return false;
        }

        public virtual bool CanCrawl()
        {
            return false;
        }

        public void StopMove()
        {
            if (ActiveMovement != null)
                ActiveMovement.StopMove();
        }

        public void KeyMovement(Vector3 moveDirection, MovementState moveState)
        {
            if (ActiveMovement != null)
                ActiveMovement.KeyMovement(moveDirection, moveState);
        }

        public void PointClickMovement(Vector3 position)
        {
            if (ActiveMovement != null)
                ActiveMovement.PointClickMovement(position);
        }

        public void SetLookRotation(Quaternion rotation)
        {
            if (ActiveMovement != null)
                ActiveMovement.SetLookRotation(rotation);
        }

        public Quaternion GetLookRotation()
        {
            if (ActiveMovement != null)
                return ActiveMovement.GetLookRotation();
            return Quaternion.identity;
        }

        public void Teleport(Vector3 position)
        {
            if (ActiveMovement == null)
            {
                teleportingPosition = position;
                return;
            }
            ActiveMovement.Teleport(position);
        }

        public void FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = CacheTransform.position;
            if (ActiveMovement != null)
                ActiveMovement.FindGroundedPosition(fromPosition, findDistance, out result);
        }

        public void SetMovement(MovementState movementState)
        {
            // Set local movement state which will be used by owner client
            LocalMovementState = movementState;

            if (MovementSecure == MovementSecure.ServerAuthoritative && IsServer)
                MovementState = movementState;

            if (MovementSecure == MovementSecure.NotSecure && IsOwnerClient)
                CallNetFunction(NetFuncSetMovement, DeliveryMethod.Sequenced, FunctionReceivers.Server, (byte)movementState);
        }

        public void SetExtraMovement(ExtraMovementState extraMovementState)
        {
            // Set local movement state which will be used by owner client
            if (IsUnderWater)
            {
                // Extra movement states always none while under water
                extraMovementState = ExtraMovementState.None;
            }
            else
            {
                switch (extraMovementState)
                {
                    case ExtraMovementState.IsSprinting:
                        if (!ActiveMovement.Entity.CanSprint())
                            extraMovementState = ExtraMovementState.None;
                        break;
                    case ExtraMovementState.IsCrouching:
                        if (!ActiveMovement.Entity.CanCrouch())
                            extraMovementState = ExtraMovementState.None;
                        break;
                    case ExtraMovementState.IsCrawling:
                        if (!ActiveMovement.Entity.CanCrawl())
                            extraMovementState = ExtraMovementState.None;
                        break;
                }
            }
            
            LocalExtraMovementState = extraMovementState;

            if (MovementSecure == MovementSecure.ServerAuthoritative && IsServer)
                ExtraMovementState = extraMovementState;

            if (MovementSecure == MovementSecure.NotSecure && IsOwnerClient)
                CallNetFunction(NetFuncSetExtraMovement, DeliveryMethod.Sequenced, FunctionReceivers.Server, (byte)extraMovementState);
        }

        public void SetDirection2D(Vector2 direction)
        {
            // Set local movement state which will be used by owner client
            LocalDirection2D = direction;

            if (MovementSecure == MovementSecure.ServerAuthoritative && IsServer)
                Direction2D = direction;

            if (MovementSecure == MovementSecure.NotSecure && IsOwnerClient)
                CallNetFunction(NetFuncUpdateDirection, FunctionReceivers.Server, new DirectionVector2(LocalDirection2D));
        }

        protected bool EnterVehicle(IVehicleEntity vehicle, byte seatIndex)
        {
            if (!IsServer || vehicle == null || PassengingVehicle.objectId > 0 || !vehicle.IsSeatAvailable(seatIndex))
                return false;

            // Set passenger to vehicle
            vehicle.SetPassenger(seatIndex, this);

            // Character when enter vehicle should stop movement, and set movement state to is grounded
            Movement.StopMove();
            MovementState = MovementState.IsGrounded;

            // Set mount info
            PassengingVehicle passengingVehicle = new PassengingVehicle()
            {
                objectId = vehicle.ObjectId,
                seatIndex = seatIndex,
            };
            PassengingVehicle = passengingVehicle;

            return true;
        }

        protected Vector3 ExitVehicle()
        {
            Vector3 exitPosition = CacheTransform.position;
            if (!IsServer || PassengingVehicleEntity == null)
                return exitPosition;

            uint vehicleObjectId = PassengingVehicleEntity.ObjectId;
            bool isDestroying = false;

            if (PassengingVehicleEntity != null)
            {
                // Remove this from vehicle
                PassengingVehicleEntity.RemovePassenger(PassengingVehicle.seatIndex);
                isDestroying = PassengingVehicleEntity.IsDestroyWhenExit(PassengingVehicle.seatIndex);

                exitPosition = PassengingVehicleEntity.transform.position;
                if (PassengingVehicleSeat.exitTransform != null)
                    exitPosition = PassengingVehicleSeat.exitTransform.position;

                // Clear passenging vehicle data
                PassengingVehicle passengingVehicle = PassengingVehicle;
                passengingVehicle.objectId = 0;
                passengingVehicle.seatIndex = 0;
                PassengingVehicle = passengingVehicle;

                // Clear vehicle entity before teleport
                passengingVehicleEntity = null;

                // Teleport to exit transform
                Teleport(exitPosition);
            }
            else
            {
                // Not passenging vehicle, just clear data
                PassengingVehicle passengingVehicle = PassengingVehicle;
                passengingVehicle.objectId = 0;
                passengingVehicle.seatIndex = 0;
                PassengingVehicle = passengingVehicle;
            }

            // Destroy mount entity
            if (isDestroying)
            {
                LiteNetLibIdentity identity;
                if (BaseGameNetworkManager.Singleton.Assets.TryGetSpawnedObject(vehicleObjectId, out identity))
                    identity.NetworkDestroy();
            }

            return exitPosition;
        }
    }
}
