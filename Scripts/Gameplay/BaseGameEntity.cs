using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using LiteNetLib;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(LiteNetLibIdentity))]
    [DefaultExecutionOrder(DefaultExecutionOrders.BASE_GAME_ENTITY)]
    public abstract partial class BaseGameEntity : LiteNetLibBehaviour, IGameEntity, IEntityMovement
    {
        public const byte STATE_DATA_CHANNEL = 3;
        protected static readonly NetDataWriter s_EntityStateMessageWriter = new NetDataWriter();
        protected static readonly NetDataWriter s_EntityStateDataWriter = new NetDataWriter();

        public int EntityId
        {
            get { return Identity.HashAssetId; }
            set { }
        }

        public bool ForceHide { get; set; }

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
                EntityComponents[i].Clean();
                EntityComponents[i] = null;
            }
            EntityOnDestroy();
            this.InvokeInstanceDevExtMethods("OnDestroy");
            BaseGameNetworkManager.Singleton.UnregisterGameEntity(this);
            Clean();
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
            for (int i = 0; i < EntityComponents.Length; ++i)
            {
                if (EntityComponents[i].Enabled && (IsUpdateEntityComponents || EntityComponents[i].AlwaysUpdate))
                    EntityComponents[i].EntityUpdate();
            }
            Profiler.EndSample();
            Profiler.BeginSample("BaseGameEntity - EntityUpdate");
            EntityUpdate();
            Profiler.EndSample();
            Profiler.BeginSample("BaseGameEntity - OnUpdateInvoke");
            if (onUpdate != null)
                onUpdate.Invoke();
            Profiler.EndSample();
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
            for (int i = 0; i < EntityComponents.Length; ++i)
            {
                if (EntityComponents[i].Enabled && (IsUpdateEntityComponents || EntityComponents[i].AlwaysUpdate))
                    EntityComponents[i].EntityLateUpdate();
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

        public virtual void SendClientState(long writeTimestamp)
        {
            if (Movement != null && Movement.Enabled)
            {
                bool shouldSendReliably;
                s_EntityStateDataWriter.Reset();
                if (Movement.WriteClientState(writeTimestamp, s_EntityStateDataWriter, out shouldSendReliably))
                {
                    TransportHandler.WritePacket(s_EntityStateMessageWriter, GameNetworkingConsts.EntityState);
                    s_EntityStateMessageWriter.PutPackedUInt(ObjectId);
                    s_EntityStateMessageWriter.PutPackedLong(writeTimestamp);
                    s_EntityStateMessageWriter.Put(s_EntityStateDataWriter.Data, 0, s_EntityStateDataWriter.Length);
                    ClientSendMessage(STATE_DATA_CHANNEL, shouldSendReliably ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Sequenced, s_EntityStateMessageWriter);
                }
            }
        }

        public virtual void SendServerState(long writeTimestamp)
        {
            if (Movement != null && Movement.Enabled)
            {
                bool shouldSendReliably;
                s_EntityStateDataWriter.Reset();
                if (Movement.WriteServerState(writeTimestamp, s_EntityStateDataWriter, out shouldSendReliably))
                {
                    TransportHandler.WritePacket(s_EntityStateMessageWriter, GameNetworkingConsts.EntityState);
                    s_EntityStateMessageWriter.PutPackedUInt(ObjectId);
                    s_EntityStateMessageWriter.PutPackedLong(writeTimestamp);
                    s_EntityStateMessageWriter.Put(s_EntityStateDataWriter.Data, 0, s_EntityStateDataWriter.Length);
                    ServerSendMessageToSubscribers(STATE_DATA_CHANNEL, shouldSendReliably ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Sequenced, s_EntityStateMessageWriter);
                }
            }
        }

        public virtual void ReadClientStateAtServer(long peerTimestamp, NetDataReader reader)
        {
            if (Movement != null)
            {
                Movement.ReadClientStateAtServer(peerTimestamp, reader);
            }
        }

        public virtual void ReadServerStateAtClient(long peerTimestamp, NetDataReader reader)
        {
            if (Movement != null)
            {
                Movement.ReadServerStateAtClient(peerTimestamp, reader);
            }
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

        public virtual bool IsRevealsHide()
        {
            return false;
        }

        public virtual bool IsBlind()
        {
            return false;
        }

        public virtual bool SetAsTargetInOneClick()
        {
            return false;
        }

        public virtual bool NotBeingSelectedOnClick()
        {
            return false;
        }

        #region Animations
        public void CallRpcPlayJumpAnimation()
        {
            RPC(RpcPlayJumpAnimation);
        }

        [AllRpc]
        protected void RpcPlayJumpAnimation()
        {
            PlayJumpAnimation();
        }

        public void CallRpcPlayPickupAnimation()
        {
            RPC(RpcPlayPickupAnimation);
        }

        [AllRpc]
        protected void RpcPlayPickupAnimation()
        {
            PlayPickupAnimation();
        }

        public void CallRpcPlayCustomAnimation(int id)
        {
            RPC(RpcPlayCustomAnimation, id);
        }

        [AllRpc]
        protected virtual void RpcPlayCustomAnimation(int id)
        {
            PlayCustomAnimation(id);
        }

        public void CallRpcStopCustomAnimation()
        {
            RPC(RpcStopCustomAnimation);
        }

        [AllRpc]
        protected virtual void RpcStopCustomAnimation()
        {
            StopCustomAnimation();
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

        public virtual void StopCustomAnimation()
        {
            if (Model is ICustomAnimationModel customAnimationModel)
                customAnimationModel.StopCustomAnimation();
        }
        #endregion

        public virtual void CallCmdPerformHitRegValidation(HitRegisterData hitData)
        {
            RPC(CmdPerformHitRegValidation, STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, hitData);
        }

        [ServerRpc]
        protected virtual void CmdPerformHitRegValidation(HitRegisterData hitData)
        {
            CurrentGameManager.HitRegistrationManager.PerformValidation(this, hitData);
        }
    }
}
