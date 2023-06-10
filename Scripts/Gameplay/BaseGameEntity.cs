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
    public abstract partial class BaseGameEntity : LiteNetLibBehaviour, IGameEntity, IEntityMovement
    {
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
                if (!PassengingVehicleEntity.IsNull())
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

        public virtual float MoveAnimationSpeedMultiplier { get { return 1f; } }
        public virtual bool MuteFootstepSound { get { return false; } }

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

        protected bool _dirtyIsHide;
        protected bool _isTeleporting;
        protected bool _stillMoveAfterTeleport;
        protected Vector3 _teleportingPosition;
        protected Quaternion _teleportingRotation;
        private bool? _wasUpdateEntityComponents;

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
                Teleport(_teleportingPosition, _teleportingRotation, _stillMoveAfterTeleport);
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

        #region RPCs
        public void CallServerEnterVehicle(uint objectId)
        {
            RPC(ServerEnterVehicle, objectId);
        }

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
        public void CallServerEnterVehicleToSeat(uint objectId, byte seatIndex)
        {
            RPC(ServerEnterVehicleToSeat, objectId, seatIndex);
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

        public void CallServerExitVehicle()
        {
            RPC(ServerExitVehicle);
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

        public override void OnNetworkDestroy(byte reasons)
        {
            base.OnNetworkDestroy(reasons);
            if (onNetworkDestroy != null)
                onNetworkDestroy.Invoke(reasons);
        }

        public virtual bool IsHide()
        {
            return false;
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
