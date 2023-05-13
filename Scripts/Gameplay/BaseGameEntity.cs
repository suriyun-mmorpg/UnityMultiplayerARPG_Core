using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using LiteNetLib;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(LiteNetLibIdentity))]
    [DefaultExecutionOrder(0)]
    public abstract class BaseGameEntity : LiteNetLibBehaviour, IGameEntity, IEntityMovement
    {
        public const float GROUND_DETECTION_DISTANCE = 10f;
        public const byte STATE_DATA_CHANNEL = 3;

        public int EntityId
        {
            get { return Identity.HashAssetId; }
            set { }
        }

        [Category(0, "Title Settings")]
        [Tooltip("This title will be used while `syncTitle` is empty.")]
        [FormerlySerializedAs("characterTitle")]
        [FormerlySerializedAs("itemTitle")]
        [SerializeField]
        protected string entityTitle;

        [Tooltip("Titles by language keys")]
        [FormerlySerializedAs("characterTitles")]
        [FormerlySerializedAs("itemTitles")]
        [SerializeField]
        protected LanguageData[] entityTitles;

        [Category(100, "Sync Fields", false)]
        [SerializeField]
        protected SyncFieldString syncTitle = new SyncFieldString();
        public SyncFieldString SyncTitle
        {
            get { return syncTitle; }
        }
        public string Title
        {
            get { return !string.IsNullOrEmpty(syncTitle.Value) ? syncTitle.Value : EntityTitle; }
            set { syncTitle.Value = value; }
        }

        [Category(1, "Relative GameObjects/Transforms")]
        [Tooltip("These objects will be hidden on non owner objects")]
        [SerializeField]
        private GameObject[] ownerObjects = new GameObject[0];
        public GameObject[] OwnerObjects
        {
            get { return ownerObjects; }
        }

        [Tooltip("These objects will be hidden on owner objects")]
        [SerializeField]
        private GameObject[] nonOwnerObjects = new GameObject[0];
        public GameObject[] NonOwnerObjects
        {
            get { return nonOwnerObjects; }
        }

        public virtual string EntityTitle
        {
            get { return Language.GetText(entityTitles, entityTitle); }
        }

        [Category(2, "Components")]
        [SerializeField]
        protected GameEntityModel model = null;
        public virtual GameEntityModel Model
        {
            get { return model; }
        }

        [Category("Relative GameObjects/Transforms")]
        [Tooltip("Transform for position which camera will look at and follow while playing in TPS view mode")]
        [SerializeField]
        private Transform cameraTargetTransform = null;
        public Transform CameraTargetTransform
        {
            get
            {
                if (PassengingVehicleEntity != null)
                {
                    if (PassengingVehicleSeat.cameraTarget == VehicleSeatCameraTarget.Vehicle)
                        return PassengingVehicleEntity.Entity.CameraTargetTransform;
                }
                return cameraTargetTransform;
            }
            set { cameraTargetTransform = value; }
        }

        [Tooltip("Transform for position which camera will look at and follow while playing in FPS view mode")]
        [SerializeField]
        private Transform fpsCameraTargetTransform = null;
        public Transform FpsCameraTargetTransform
        {
            get { return fpsCameraTargetTransform; }
            set { fpsCameraTargetTransform = value; }
        }

        [Category(3, "Entity Movement")]
        [SerializeField]
        protected bool canSideSprint = false;
        public bool CanSideSprint { get { return canSideSprint; } }

        [SerializeField]
        protected bool canBackwardSprint = false;
        public bool CanBackwardSprint { get { return canBackwardSprint; } }

        public IEntityMovementComponent Movement { get; private set; }

        public Transform MovementTransform
        {
            get
            {
                if (PassengingVehicleEntity != null)
                {
                    // Track movement position by vehicle entity
                    return PassengingVehicleEntity.Entity.EntityTransform;
                }
                return EntityTransform;
            }
        }

        public byte PassengingVehicleSeatIndex { get; private set; }

        private IVehicleEntity passengingVehicleEntity;
        public IVehicleEntity PassengingVehicleEntity
        {
            get
            {
                if (passengingVehicleEntity as Object == null)
                    passengingVehicleEntity = null;
                return passengingVehicleEntity;
            }
            private set
            {
                passengingVehicleEntity = value;
            }
        }

        public VehicleType PassengingVehicleType
        {
            get
            {
                if (PassengingVehicleEntity != null)
                    return PassengingVehicleEntity.VehicleType;
                return null;
            }
        }

        public VehicleSeat PassengingVehicleSeat
        {
            get
            {
                if (PassengingVehicleEntity != null)
                    return PassengingVehicleEntity.Seats[PassengingVehicleSeatIndex];
                return VehicleSeat.Empty;
            }
        }

        public GameEntityModel PassengingVehicleModel
        {
            get
            {
                if (PassengingVehicleEntity != null)
                    return PassengingVehicleEntity.Entity.Model;
                return null;
            }
        }

        public IEntityMovementComponent ActiveMovement
        {
            get
            {
                if (PassengingVehicleEntity != null)
                    return PassengingVehicleEntity.Entity.Movement;
                return Movement;
            }
        }

        public float StoppingDistance
        {
            get
            {
                return ActiveMovement == null ? 0.1f : ActiveMovement.StoppingDistance;
            }
        }
        public MovementState MovementState
        {
            get
            {
                return ActiveMovement == null ? MovementState.IsGrounded : ActiveMovement.MovementState;
            }
        }
        public ExtraMovementState ExtraMovementState
        {
            get
            {
                return ActiveMovement == null ? ExtraMovementState.None : ActiveMovement.ExtraMovementState;
            }
        }
        public DirectionVector2 Direction2D
        {
            get
            {
                return ActiveMovement == null ? (DirectionVector2)Vector2.down : ActiveMovement.Direction2D;
            }
            set
            {
                if (ActiveMovement != null)
                    ActiveMovement.Direction2D = value;
            }
        }
        public float CurrentMoveSpeed
        {
            get
            {
                return ActiveMovement == null ? 0f : ActiveMovement.CurrentMoveSpeed;
            }
        }
        public virtual float MoveAnimationSpeedMultiplier { get { return 1f; } }
        public virtual bool MuteFootstepSound { get { return false; } }

        protected bool _dirtyIsHide;
        protected bool _isTeleporting;
        protected Vector3 _teleportingPosition;
        protected Quaternion _teleportingRotation;

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

        public BaseMapInfo CurrentMapInfo
        {
            get { return BaseGameNetworkManager.CurrentMapInfo; }
        }

        public BaseGameEntity Entity
        {
            get { return this; }
        }
        public Transform EntityTransform
        {
            get { return transform; }
        }
        public GameObject EntityGameObject
        {
            get { return gameObject; }
        }

        protected IGameEntityComponent[] EntityComponents { get; private set; }
        protected virtual bool IsUpdateEntityComponents
        {
            get
            {
                if (IsServer && IsOwnedByServer && Identity.CountSubscribers() == 0)
                    return false;
                return true;
            }
        }
        protected NetDataWriter EntityStateMessageWriter { get; private set; } = new NetDataWriter();
        protected NetDataWriter EntityStateDataWriter { get; private set; } = new NetDataWriter();
        private bool? _wasUpdateEntityComponents;

        #region Events
        public event System.Action onStart;
        public event System.Action onEnable;
        public event System.Action onDisable;
        public event System.Action onUpdate;
        public event System.Action onLateUpdate;
        public event System.Action onSetup;
        public event System.Action onSetupNetElements;
        public event System.Action onSetOwnerClient;
        public event IsUpdateEntityComponentsDelegate onIsUpdateEntityComponentsChanged;
        public event NetworkDestroyDelegate onNetworkDestroy;
        #endregion

        /// <summary>
        /// Override this function to initial required components
        /// This function will be called by this entity when awake
        /// </summary>
        public virtual void InitialRequiredComponents()
        {
            // Cache components
            if (model == null)
                model = GetComponent<GameEntityModel>();
            if (cameraTargetTransform == null)
                cameraTargetTransform = EntityTransform;
            if (fpsCameraTargetTransform == null)
                fpsCameraTargetTransform = EntityTransform;
            Movement = GetComponent<IEntityMovementComponent>();
        }

        /// <summary>
        /// Override this function to add relates game data to game instance
        /// This function will be called by GameInstance when adding the entity
        /// </summary>
        public virtual void PrepareRelatesData()
        {
            // Add pooling game effects
            GameInstance.AddPoolingObjects(GetComponentsInChildren<IPoolDescriptorCollection>(true));
        }

        /// <summary>
        /// Override this function to set instigator when attacks other entities
        /// </summary>
        /// <returns></returns>
        public virtual EntityInfo GetInfo()
        {
            return EntityInfo.Empty;
        }

        public virtual Bounds MakeLocalBounds()
        {
            return GameplayUtils.MakeLocalBoundsByCollider(EntityTransform);
        }

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
            BaseGameNetworkManager.Singleton.RegisterGameEntity(this);
        }
        protected virtual void EntityStart() { }

        private void OnDestroy()
        {
            for (int i = 0; i < EntityComponents.Length; ++i)
            {
                EntityComponents[i].EntityOnDestroy();
            }
            EntityOnDestroy();
            this.InvokeInstanceDevExtMethods("OnDestroy");
            BaseGameNetworkManager.Singleton.UnregisterGameEntity(this);
        }
        protected virtual void EntityOnDestroy()
        {
            // Exit vehicle when destroy
            if (IsServer)
                ExitVehicle();
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

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
        }

        protected virtual void OnDrawGizmosSelected()
        {
        }
#endif

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

        internal void DoUpdate()
        {
            Profiler.BeginSample("EntityComponents - Update");
            if (IsUpdateEntityComponents)
            {
                for (int i = 0; i < EntityComponents.Length; ++i)
                {
                    if (EntityComponents[i].Enabled)
                        EntityComponents[i].EntityUpdate();
                }
            }
            Profiler.EndSample();
            Profiler.BeginSample("BaseGameEntity - EntityUpdate");
            EntityUpdate();
            Profiler.EndSample();
            Profiler.BeginSample("BaseGameEntity - OnUpdateInvoke");
            if (onUpdate != null)
                onUpdate.Invoke();
            Profiler.EndSample();
            // Update identity's hide status
            bool isHide = IsHide();
            if (_dirtyIsHide != isHide)
            {
                _dirtyIsHide = isHide;
                Identity.IsHide = _dirtyIsHide;
            }
        }

        protected virtual void EntityUpdate()
        {
            if (!Movement.IsNull())
            {
                bool tempEnableMovement = PassengingVehicleEntity.IsNull();
                // Enable movement or not
                if (Movement.Enabled != tempEnableMovement)
                {
                    if (!tempEnableMovement)
                        Movement.StopMove();
                    // Enable movement while not passenging any vehicle
                    Movement.Enabled = tempEnableMovement;
                }
            }

            if (Model != null && (IsClient || GameInstance.Singleton.updateAnimationAtServer))
            {
                if (Model is IMoveableModel moveableModel)
                {
                    // Update move speed multiplier
                    moveableModel.SetMoveAnimationSpeedMultiplier(MoveAnimationSpeedMultiplier);
                    // Update movement state
                    moveableModel.SetMovementState(MovementState, ExtraMovementState, Direction2D, false);
                }
                Model.UpdateAnimation(Time.unscaledDeltaTime);
            }
        }

        internal void DoLateUpdate()
        {
            bool isUpdateEntityComponents = IsUpdateEntityComponents;
            Profiler.BeginSample("EntityComponents - LateUpdate");
            if (isUpdateEntityComponents)
            {
                for (int i = 0; i < EntityComponents.Length; ++i)
                {
                    if (EntityComponents[i].Enabled)
                        EntityComponents[i].EntityLateUpdate();
                }
            }
            Profiler.EndSample();
            Profiler.BeginSample("BaseGameEntity - OnUpdateEntityComponentsChanged");
            if (!_wasUpdateEntityComponents.HasValue || _wasUpdateEntityComponents.Value != isUpdateEntityComponents)
            {
                _wasUpdateEntityComponents = isUpdateEntityComponents;
                if (onIsUpdateEntityComponentsChanged != null)
                    onIsUpdateEntityComponentsChanged.Invoke(isUpdateEntityComponents);
            }
            Profiler.EndSample();
            Profiler.BeginSample("BaseGameEntity - EntityLateUpdate");
            EntityLateUpdate();
            Profiler.EndSample();
            Profiler.BeginSample("BaseGameEntity - OnLateUpdateInvoke");
            if (onLateUpdate != null)
                onLateUpdate.Invoke();
            Profiler.EndSample();
        }

        protected virtual void EntityLateUpdate()
        {
            if (PassengingVehicleSeat.passengingTransform != null)
            {
                // Snap character to vehicle seat
                EntityTransform.position = PassengingVehicleSeat.passengingTransform.position;
                EntityTransform.rotation = PassengingVehicleSeat.passengingTransform.rotation;
            }

            if (_isTeleporting && ActiveMovement != null)
            {
                Teleport(_teleportingPosition, _teleportingRotation);
                _isTeleporting = false;
            }
        }

        public virtual void SendClientState()
        {
            if (Movement != null && Movement.Enabled)
            {
                bool shouldSendReliably;
                EntityStateDataWriter.Reset();
                if (Movement.WriteClientState(EntityStateDataWriter, out shouldSendReliably))
                {
                    TransportHandler.WritePacket(EntityStateMessageWriter, GameNetworkingConsts.EntityState);
                    EntityStateMessageWriter.PutPackedUInt(ObjectId);
                    EntityStateMessageWriter.Put(EntityStateDataWriter.Data, 0, EntityStateDataWriter.Length);
                    ClientSendMessage(STATE_DATA_CHANNEL, shouldSendReliably ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Sequenced, EntityStateMessageWriter);
                }
            }
        }

        public virtual void SendServerState()
        {
            if (Movement != null && Movement.Enabled)
            {
                bool shouldSendReliably;
                EntityStateDataWriter.Reset();
                if (Movement.WriteServerState(EntityStateDataWriter, out shouldSendReliably))
                {
                    TransportHandler.WritePacket(EntityStateMessageWriter, GameNetworkingConsts.EntityState);
                    EntityStateMessageWriter.PutPackedUInt(ObjectId);
                    EntityStateMessageWriter.Put(EntityStateDataWriter.Data, 0, EntityStateDataWriter.Length);
                    ServerSendMessageToSubscribers(STATE_DATA_CHANNEL, shouldSendReliably ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Sequenced, EntityStateMessageWriter);
                }
            }
        }

        public virtual void ReadClientStateAtServer(NetDataReader reader)
        {
            if (Movement != null)
                Movement.ReadClientStateAtServer(reader);
        }

        public virtual void ReadServerStateAtClient(NetDataReader reader)
        {
            if (Movement != null)
                Movement.ReadServerStateAtClient(reader);
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
        }

        protected virtual void SetupNetElements()
        {
            if (onSetupNetElements != null)
                onSetupNetElements.Invoke();
            syncTitle.deliveryMethod = DeliveryMethod.ReliableOrdered;
            syncTitle.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
        }

        #region Net Functions
        [ServerRpc]
        protected void ServerEnterVehicle(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Call this function at server
            if (Manager.Assets.TryGetSpawnedObject(objectId, out LiteNetLibIdentity identity))
            {
                IVehicleEntity vehicleEntity = identity.GetComponent<IVehicleEntity>();
                if (!vehicleEntity.IsNull() && vehicleEntity.GetAvailableSeat(out byte seatIndex))
                    EnterVehicle(vehicleEntity, seatIndex);
            }
#endif
        }

        [ServerRpc]
        protected void ServerEnterVehicleToSeat(uint objectId, byte seatIndex)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Call this function at server
            if (Manager.Assets.TryGetSpawnedObject(objectId, out LiteNetLibIdentity identity))
            {
                IVehicleEntity vehicleEntity = identity.GetComponent<IVehicleEntity>();
                if (!vehicleEntity.IsNull())
                    EnterVehicle(vehicleEntity, seatIndex);
            }
#endif
        }

        [ServerRpc]
        protected void ServerExitVehicle()
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Call this function at server
            ExitVehicle();
#endif
        }

        public void CallAllOnExitVehicle()
        {
            RPC(AllOnExitVehicle);
        }

        [AllRpc]
        protected void AllOnExitVehicle()
        {
            ClearPassengingVehicle();
        }

        public void CallAllPlayJumpAnimation()
        {
            RPC(AllPlayJumpAnimation);
        }

        [AllRpc]
        protected void AllPlayJumpAnimation()
        {
            PlayJumpAnimation();
        }

        public void CallAllPlayPickupAnimation()
        {
            RPC(AllPlayPickupAnimation);
        }

        [AllRpc]
        protected void AllPlayPickupAnimation()
        {
            PlayPickupAnimation();
        }

        public void CallAllPlayCustomAnimation(int id)
        {
            RPC(AllPlayCustomAnimation, id);
        }

        [AllRpc]
        protected virtual void AllPlayCustomAnimation(int id)
        {
            PlayCustomAnimation(id);
        }
        #endregion

        #region RPC Calls
        public void CallServerEnterVehicle(uint objectId)
        {
            RPC(ServerEnterVehicle, objectId);
        }

        public void CallServerEnterVehicleToSeat(uint objectId, byte seatIndex)
        {
            RPC(ServerEnterVehicleToSeat, objectId, seatIndex);
        }

        public void CallServerExitVehicle()
        {
            RPC(ServerExitVehicle);
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

        public virtual bool CanWalk()
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

        public virtual bool CanJump()
        {
            return false;
        }

        public virtual bool CanTurn()
        {
            return false;
        }

        public virtual bool IsHide()
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

        public void SetExtraMovementState(ExtraMovementState extraMovementState)
        {
            if (ActiveMovement != null)
                ActiveMovement.SetExtraMovementState(extraMovementState);
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

        public void SetSmoothTurnSpeed(float speed)
        {
            if (ActiveMovement != null)
                ActiveMovement.SetSmoothTurnSpeed(speed);
        }

        public float GetSmoothTurnSpeed()
        {
            if (ActiveMovement != null)
                return ActiveMovement.GetSmoothTurnSpeed();
            return 0f;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (ActiveMovement == null)
            {
                // Can't teleport properly yet, try to teleport later
                _teleportingPosition = position;
                _teleportingRotation = rotation;
                _isTeleporting = true;
                return;
            }
            if (FindGroundedPosition(position, GROUND_DETECTION_DISTANCE, out Vector3 groundedPosition))
            {
                // Set position to grounded position, to make it not float and fall
                position = groundedPosition;
            }
            if (IsServer)
            {
                // Teleport to the `position`, `rotation`
                ActiveMovement.Teleport(position, rotation);
            }
            OnTeleport(position, rotation);
        }

        protected virtual void OnTeleport(Vector3 position, Quaternion rotation)
        {

        }

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = EntityTransform.position;
            if (ActiveMovement != null)
                return ActiveMovement.FindGroundedPosition(fromPosition, findDistance, out result);
            return true;
        }

        public virtual void PlayJumpAnimation()
        {
            if (Model is IJumppableModel jumppableModel)
                jumppableModel.PlayJumpAnimation();
        }

        public virtual void PlayPickupAnimation()
        {
            if (Model is IPickupableModel pickupableModel)
                pickupableModel.PlayPickupAnimation();
        }

        public virtual void PlayCustomAnimation(int id)
        {
            if (Model is ICustomAnimationModel customAnimationModel)
                customAnimationModel.PlayCustomAnimation(id);
        }

        protected bool EnterVehicle(IVehicleEntity vehicle, byte seatIndex)
        {
            if (!IsServer || vehicle == null || !vehicle.IsSeatAvailable(seatIndex))
                return false;

            // Change object owner to driver
            if (vehicle.IsDriver(seatIndex))
                Manager.Assets.SetObjectOwner(vehicle.Entity.ObjectId, ConnectionId);

            // Set passenger to vehicle
            vehicle.SetPassenger(seatIndex, this);

            return true;
        }

        protected void ExitVehicle()
        {
            if (!IsServer || PassengingVehicleEntity == null)
                return;

            bool isDriver = PassengingVehicleEntity.IsDriver(PassengingVehicleSeatIndex);
            bool isDestroying = PassengingVehicleEntity.IsDestroyWhenExit(PassengingVehicleSeatIndex);

            // Clear object owner from driver
            if (PassengingVehicleEntity.IsDriver(PassengingVehicleSeatIndex))
                Manager.Assets.SetObjectOwner(PassengingVehicleEntity.Entity.ObjectId, -1);

            BaseGameEntity vehicleEntity = PassengingVehicleEntity.Entity;
            if (isDestroying)
            {
                // Remove all entity from vehicle
                PassengingVehicleEntity.RemoveAllPassengers();
                // Destroy vehicle entity
                vehicleEntity.NetworkDestroy();
            }
            else
            {
                // Remove this from vehicle
                PassengingVehicleEntity.RemovePassenger(PassengingVehicleSeatIndex);
                // Stop move if driver exit (if not driver continue move by driver controls)
                if (isDriver)
                    vehicleEntity.StopMove();
            }
        }

        /// <summary>
        /// This function will be called by Vehicle Entity to inform that this entity exited vehicle
        /// </summary>
        public void ExitedVehicle(Vector3 exitPosition, Quaternion exitRotation)
        {
            CallAllOnExitVehicle();
            Teleport(exitPosition, exitRotation);
        }

        public virtual void ClearPassengingVehicle()
        {
            SetPassengingVehicle(0, null);
        }

        public virtual void SetPassengingVehicle(byte seatIndex, IVehicleEntity vehicleEntity)
        {
            PassengingVehicleSeatIndex = seatIndex;
            PassengingVehicleEntity = vehicleEntity;
        }

        public virtual bool SetAsTargetInOneClick()
        {
            return false;
        }

        public virtual bool NotBeingSelectedOnClick()
        {
            return false;
        }
    }
}
