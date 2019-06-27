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
        public virtual MovementState MovementState
        {
            get { return (MovementState)movementState.Value; }
            set { movementState.Value = (byte)value; }
        }
        [SerializeField]
        protected SyncFieldVector2 currentDirection = new SyncFieldVector2();
        public virtual Vector2 CurrentDirection
        {
            get { return currentDirection.Value; }
            set { currentDirection.Value = value; }
        }
        public virtual DirectionType2D CurrentDirectionType
        {
            get { return GameplayUtils.GetDirectionTypeByVector2(CurrentDirection); }
        }
        [SerializeField]
        protected SyncFieldRidingVehicle ridingVehicle = new SyncFieldRidingVehicle();
        public virtual RidingVehicle RidingVehicle
        {
            get { return ridingVehicle.Value; }
            set { ridingVehicle.Value = value; }
        }

        protected Vector3? teleportingPosition;

        public bool IsGrounded { get { return ActiveMovement == null ? true : ActiveMovement.IsGrounded; } }
        public bool IsJumping { get { return ActiveMovement == null ? false : ActiveMovement.IsJumping; } }
        public float StoppingDistance { get { return ActiveMovement == null ? 0.1f : ActiveMovement.StoppingDistance; } }

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

        private GameEntityModel model;
        public virtual GameEntityModel Model
        {
            get
            {
                if (model == null)
                    model = GetComponent<GameEntityModel>();
                return model;
            }
        }

        private BaseEntityMovement movement;
        public virtual BaseEntityMovement Movement
        {
            get
            {
                if (movement == null)
                    movement = GetComponent<BaseEntityMovement>();
                return movement;
            }
            set { movement = value; }
        }

        private uint vehicleObjectId;
        private IVehicleEntity ridingVehicleEntity;
        public IVehicleEntity RidingVehicleEntity
        {
            get
            {
                if ((ridingVehicleEntity == null || vehicleObjectId != RidingVehicle.objectId) && RidingVehicle.objectId > 0)
                {
                    vehicleObjectId = RidingVehicle.objectId;
                    ridingVehicleEntity = null;
                    LiteNetLibIdentity identity;
                    if (BaseGameNetworkManager.Singleton.Assets.TryGetSpawnedObject(RidingVehicle.objectId, out identity))
                    {
                        ridingVehicleEntity = identity.GetComponent<IVehicleEntity>();
                        RidingVehicleSeat = ridingVehicleEntity.Seats[RidingVehicle.seatIndex];
                    }
                }
                // Clear current vehicle
                if (RidingVehicle.objectId == 0)
                    ridingVehicleEntity = null;
                return ridingVehicleEntity;
            }
        }

        public VehicleSeat RidingVehicleSeat { get; protected set; }

        public IEntityMovement ActiveMovement
        {
            get
            {
                if (RidingVehicleEntity == null)
                    return RidingVehicleEntity;
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
        protected virtual void EntityUpdate() { }

        private void LateUpdate()
        {
            if (textTitle != null)
                textTitle.text = Title;
            if (textTitleB != null)
                textTitleB.text = TitleB;
            // Snap character to vehicle seat
            if (ActiveMovement != null)
            {
                CacheTransform.position = RidingVehicleSeat.rideTransform.position;
                CacheTransform.rotation = RidingVehicleSeat.rideTransform.rotation;
            }
            EntityLateUpdate();
        }
        protected virtual void EntityLateUpdate() { }

        private void FixedUpdate()
        {
            EntityFixedUpdate();
        }
        protected virtual void EntityFixedUpdate() { }

        private void OnDestroy()
        {
            EntityOnDestroy();
            this.InvokeInstanceDevExtMethods("OnDestroy");
        }
        protected virtual void EntityOnDestroy()
        {
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
            RegisterNetFunction<uint>(NetFuncPlayEffect);
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
            ridingVehicle.deliveryMethod = DeliveryMethod.ReliableOrdered;
            ridingVehicle.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            ridingVehicle.doNotSyncInitialDataImmediately = true;
        }

        /// <summary>
        /// Override this function to initial required components
        /// </summary>
        public virtual void InitialRequiredComponents() { }

        #region Net Functions
        /// <summary>
        /// This will be called at every clients to play any effect
        /// </summary>
        /// <param name="effectId"></param>
        protected virtual void NetFuncPlayEffect(uint effectId)
        {
            GameEffectCollection gameEffectCollection;
            if (Model == null || !GameInstance.GameEffectCollections.TryGetValue(effectId, out gameEffectCollection))
                return;
            Model.InstantiateEffect(gameEffectCollection.effects);
        }
        #endregion

        #region Net Function Requests
        public virtual void RequestPlayEffect(uint effectId)
        {
            if (effectId <= 0)
                return;
            CallNetFunction(NetFuncPlayEffect, FunctionReceivers.All, effectId);
        }
        #endregion

        public override void OnNetworkDestroy(byte reasons)
        {
            base.OnNetworkDestroy(reasons);
            if (onNetworkDestroy != null)
                onNetworkDestroy.Invoke(reasons);
        }

        public bool TryGetEntityByObjectId<T>(uint objectId, out T result) where T : class
        {
            result = null;
            LiteNetLibIdentity identity;
            if (!Manager.Assets.TryGetSpawnedObject(objectId, out identity))
                return false;

            result = identity.GetComponent<T>();
            if (result == null)
                return false;

            return true;
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
            ActiveMovement.FindGroundedPosition(fromPosition, findDistance, out result);
        }

        protected void EnterVehicle(IVehicleEntity vehiclePrefab, byte seatIndex)
        {
            if (!IsServer || vehiclePrefab == null || RidingVehicle.objectId > 0)
                return;

            // Instantiate new mount entity
            GameObject spawnObj = Instantiate(vehiclePrefab.gameObject, CacheTransform.position, CacheTransform.rotation);
            IVehicleEntity vehicle = BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj).GetComponent<IVehicleEntity>();

            // Set mount info
            RidingVehicle ridingVehicle = new RidingVehicle()
            {
                objectId = vehicle.ObjectId,
                seatIndex = seatIndex,
            };
            RidingVehicle = ridingVehicle;
        }

        protected void ExitVehicle()
        {
            if (!IsServer || RidingVehicle.objectId == 0)
                return;
            
            if (RidingVehicleEntity != null)
            {
                Vector3 exitPosition = CacheTransform.position;
                if (RidingVehicleSeat.exitTransform == null)
                    exitPosition = RidingVehicleSeat.exitTransform.position;

                // Clear riding vehicle data
                RidingVehicle ridingVehicle = RidingVehicle;
                ridingVehicle.objectId = 0;
                ridingVehicle.seatIndex = 0;
                RidingVehicle = ridingVehicle;

                // Clear vehicle entity before teleport
                ridingVehicleEntity = null;

                // Teleport to exit transform
                Teleport(exitPosition);
            }
            else
            {
                // Not riding vehicle, just clear data
                RidingVehicle ridingVehicle = RidingVehicle;
                ridingVehicle.objectId = 0;
                ridingVehicle.seatIndex = 0;
                RidingVehicle = ridingVehicle;
            }
        }
    }
}
