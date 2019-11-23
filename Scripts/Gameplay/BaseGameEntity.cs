using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public abstract class BaseGameEntity : LiteNetLibBehaviour, IGameEntity, IEntityMovement
    {
        [Header("Game Entity Settings")]
        public Text textTitle;
        public Text textTitleB;
        [Tooltip("These objects will be hidden on non owner objects")]
        public GameObject[] ownerObjects;
        [Tooltip("These objects will be hidden on owner objects")]
        public GameObject[] nonOwnerObjects;

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

        public BaseGameEntity Entity { get { return this; } }

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

        // Movement data
        [SerializeField]
        protected SyncFieldByte movementState = new SyncFieldByte();
        public MovementState MovementState
        {
            get { return (MovementState)movementState.Value; }
            set { movementState.Value = (byte)value; }
        }
        [SerializeField]
        protected SyncFieldDirectionVector2 currentDirection = new SyncFieldDirectionVector2();
        public Vector2 CurrentDirection
        {
            get { return currentDirection.Value; }
            set { currentDirection.Value = value; }
        }
        public DirectionType2D CurrentDirectionType
        {
            get { return GameplayUtils.GetDirectionTypeByVector2(CurrentDirection); }
        }
        [SerializeField]
        protected SyncFieldPassengingVehicle passengingVehicle = new SyncFieldPassengingVehicle();
        public PassengingVehicle PassengingVehicle
        {
            get { return passengingVehicle.Value; }
            set { passengingVehicle.Value = value; }
        }

        protected Vector3? teleportingPosition;

        public bool IsGrounded { get { return ActiveMovement == null ? true : ActiveMovement.IsGrounded; } }
        public bool IsJumping { get { return ActiveMovement == null ? false : ActiveMovement.IsJumping; } }
        public float StoppingDistance { get { return ActiveMovement == null ? 0.1f : ActiveMovement.StoppingDistance; } }
        public virtual float MoveAnimationSpeedMultiplier { get { return 1f; } }

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

        [SerializeField]
        protected GameEntityModel model;
        public GameEntityModel Model
        {
            get { return model; }
        }

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

        public GameInstance gameInstance
        {
            get { return GameInstance.Singleton; }
        }

        public BaseGameplayRule gameplayRule
        {
            get { return gameInstance.GameplayRule; }
        }

        public BaseGameNetworkManager gameManager
        {
            get { return BaseGameNetworkManager.Singleton; }
        }

        private void Awake()
        {
            EntityAwake();
            this.InvokeInstanceDevExtMethods("Awake");
        }

        protected virtual void EntityAwake() { }

        private void Start()
        {
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
            EntityUpdate();
            if (onUpdate != null)
                onUpdate.Invoke();
        }
        protected virtual void EntityUpdate()
        {
            if (Model != null && Model is IMoveableModel)
            {
                // Update movement animation
                (Model as IMoveableModel).SetMoveAnimationSpeedMultiplier(MoveAnimationSpeedMultiplier);
                (Model as IMoveableModel).SetMovementState(MovementState);
            }

            if (Movement != null && Movement.enabled != (PassengingVehicleEntity == null))
            {
                // Enable movement while not passenging any vehicle
                Movement.enabled = PassengingVehicleEntity == null;
            }
        }

        private void LateUpdate()
        {
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
            EntityFixedUpdate();
            if (onFixedUpdate != null)
                onFixedUpdate.Invoke();
        }
        protected virtual void EntityFixedUpdate() { }

        private void OnDestroy()
        {
            EntityOnDestroy();
            this.InvokeInstanceDevExtMethods("OnDestroy");
        }
        protected virtual void EntityOnDestroy()
        {
            // Exit vehicle when destroy
            ExitVehicle();
            if (Movement != null)
                Movement.EntityOnDestroy(this);
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

            // Setup relates component
            InitialRequiredComponents();

            // Setup entity movement here to make it able to register net elements / functions
            if (Movement != null)
                Movement.EntityOnSetup(this);

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
            currentDirection.deliveryMethod = DeliveryMethod.Sequenced;
            currentDirection.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentDirection.doNotSyncInitialDataImmediately = true;
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
            ExitVehicle();
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
            return ActiveMovement != null;
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

        public void SetExtraMovement(MovementState movementState)
        {
            if (ActiveMovement != null)
                ActiveMovement.SetExtraMovement(movementState);
        }

        public void SetLookRotation(Vector3 eulerAngles)
        {
            if (ActiveMovement != null)
                ActiveMovement.SetLookRotation(eulerAngles);
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
