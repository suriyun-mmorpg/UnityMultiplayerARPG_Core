using Insthync.DevExtension;
using Insthync.UnityEditorUtils;
using LiteNetLib;
using LiteNetLibManager;
using LiteNetLib.Utils;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(LiteNetLibIdentity))]
    [DefaultExecutionOrder(DefaultExecutionOrders.BASE_GAME_ENTITY)]
    public abstract partial class BaseGameEntity : LiteNetLibBehaviour, IGameEntity, IEntityMovement
    {
        public const byte MOVEMENT_DATA_CHANNEL = 2;
        public const byte ACTION_DATA_CHANNEL = 3;
        protected static readonly NetDataWriter s_EntityStateMessageWriter = new NetDataWriter();
        protected static readonly NetDataWriter s_EntityStateDataWriter = new NetDataWriter();
        protected static readonly ProfilerMarker s_ComponentsUpdateProfilerMarker = new ProfilerMarker("BaseGameEntity - ComponentsUpdate");
        protected static readonly ProfilerMarker s_EntityUpdateProfilerMarker = new ProfilerMarker("BaseGameEntity - EntityUpdate");
        protected static readonly ProfilerMarker s_OnUpdateInvokeProfilerMarker = new ProfilerMarker("BaseGameEntity - OnUpdateInvoke");
        protected static readonly ProfilerMarker s_ComponentsLateUpdateProfilerMarker = new ProfilerMarker("BaseGameEntity - ComponentsLateUpdate");
        protected static readonly ProfilerMarker s_ComponentsChangedStateUpdateProfilerMarker = new ProfilerMarker("BaseGameEntity - ComponentsChangedStateUpdate");
        protected static readonly ProfilerMarker s_EntityLateUpdateProfilerMarker = new ProfilerMarker("BaseGameEntity - EntityLateUpdate");
        protected static readonly ProfilerMarker s_OnLateUpdateInvokeProfilerMarker = new ProfilerMarker("BaseGameEntity - OnLateUpdateInvoke");

        public int HashAssetId
        {
            get { return Identity.HashAssetId; }
        }

        public virtual int EntityId
        {
            get
            {
                if (MetaDataId != 0)
                    return MetaDataId;
                return HashAssetId;
            }
            set { }
        }

        [SerializeField]
        protected SyncFieldInt syncMetaDataId = new SyncFieldInt();
        private int _metaDataId;
        public int MetaDataId
        {
            get
            {
                if (syncMetaDataId.Value != 0)
                    return syncMetaDataId.Value;
                return _metaDataId;
            }
            set
            {
                if (CurrentGameManager.IsServer)
                    syncMetaDataId.Value = value;
                _metaDataId = value;
            }
        }

        public bool ForceHide { get; set; }

        [Category(0, "Title Settings")]
        [Tooltip("This title will be used while `syncTitle` is empty.")]
        [FormerlySerializedAs("characterTitle")]
        [FormerlySerializedAs("itemTitle")]
        [SerializeField]
        protected string entityTitle;
        public string EntityTitleSetting
        {
            get { return entityTitle; }
        }

        [Tooltip("Titles by language keys")]
        [FormerlySerializedAs("characterTitles")]
        [FormerlySerializedAs("itemTitles")]
        [SerializeField]
        protected LanguageData[] entityTitles;
        public LanguageData[] EntityTitlesSetting
        {
            get { return entityTitles; }
        }

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

        protected LogicUpdater _logicUpdater;
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
        }

        /// <summary>
        /// Override this function to set instigator when attacks other entities
        /// </summary>
        /// <returns></returns>
        public virtual EntityInfo GetInfo()
        {
            return EntityInfo.Empty;
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
        }
        protected virtual void EntityStart() { }

        public override void OnIdentityInitialize()
        {
            for (int i = 0; i < EntityComponents.Length; ++i)
            {
                EntityComponents[i].EntityOnIdentityInitialize();
            }
            if (_logicUpdater == null)
            {
                _logicUpdater = Manager.LogicUpdater;
                _logicUpdater.OnTick += OnTickServer;
                _logicUpdater.OnTick += OnTickClient;
            }
        }

        private void OnTickServer(LogicUpdater updater)
        {
            if (!isActiveAndEnabled)
                return;
            if (!IsServer)
                return;
            SendServerState(Manager.LocalTick);
        }

        private void OnTickClient(LogicUpdater updater)
        {
            if (!isActiveAndEnabled)
                return;
            if (IsServer)
                return;
            if (!IsOwnerClient)
                return;
            SendClientState(Manager.LocalTick);
        }

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
            Clean();
            if (_logicUpdater != null)
            {
                _logicUpdater.OnTick -= OnTickServer;
                _logicUpdater.OnTick -= OnTickClient;
            }
        }
        protected virtual void EntityOnDestroy()
        {
            if (IsServer)
            {
                // Exit vehicle when destroy
                ExitVehicleAndForget();
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

        internal void Update()
        {
            using (s_ComponentsUpdateProfilerMarker.Auto())
            {
                for (int i = 0; i < EntityComponents.Length; ++i)
                {
                    try
                    {
                        if (EntityComponents[i].Enabled && (IsUpdateEntityComponents || EntityComponents[i].AlwaysUpdate))
                            EntityComponents[i].EntityUpdate();
                    }
                    catch (System.Exception ex)
                    {
                        Logging.LogException(LogTag, ex);
                    }
                }
            }
            using (s_EntityUpdateProfilerMarker.Auto())
            {
                try
                {
                    EntityUpdate();
                }
                catch (System.Exception ex)
                {
                    Logging.LogException(LogTag, ex);
                }
            }
            using (s_OnUpdateInvokeProfilerMarker.Auto())
            {
                if (onUpdate != null)
                    onUpdate.Invoke();
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

        internal void LateUpdate()
        {
            bool isUpdateEntityComponents = IsUpdateEntityComponents;
            using (s_ComponentsLateUpdateProfilerMarker.Auto())
            {
                for (int i = 0; i < EntityComponents.Length; ++i)
                {
                    try
                    {
                        if (EntityComponents[i].Enabled && (IsUpdateEntityComponents || EntityComponents[i].AlwaysUpdate))
                            EntityComponents[i].EntityLateUpdate();
                    }
                    catch (System.Exception ex)
                    {
                        Logging.LogException(LogTag, ex);
                    }
                }
            }
            using (s_ComponentsChangedStateUpdateProfilerMarker.Auto())
            {
                if (!_wasUpdateEntityComponents.HasValue || _wasUpdateEntityComponents.Value != isUpdateEntityComponents)
                {
                    _wasUpdateEntityComponents = isUpdateEntityComponents;
                    if (onIsUpdateEntityComponentsChanged != null)
                        onIsUpdateEntityComponentsChanged.Invoke(isUpdateEntityComponents);
                }
            }
            using (s_EntityLateUpdateProfilerMarker.Auto())
            {
                try
                {
                    EntityLateUpdate();
                }
                catch (System.Exception ex)
                {
                    Logging.LogException(LogTag, ex);
                }
            }
            using (s_OnLateUpdateInvokeProfilerMarker.Auto())
            {
                if (onLateUpdate != null)
                    onLateUpdate.Invoke();
            }
        }

        protected virtual void EntityLateUpdate()
        {
            if (PassengingVehicleSeat != null)
            {
                // Snap character to vehicle seat
                if (PassengingVehicleSeat.passengingTransform != null)
                {
                    EntityTransform.position = PassengingVehicleSeat.passengingTransform.position;
                    EntityTransform.rotation = PassengingVehicleSeat.passengingTransform.rotation;
                }
                else
                {
                    EntityTransform.position = PassengingVehicleEntity.EntityTransform.position;
                    EntityTransform.rotation = PassengingVehicleEntity.EntityTransform.rotation;
                }
            }

            if (_isTeleporting && ActiveMovement != null)
            {
                Teleport(_teleportingPosition, _teleportingRotation, _stillMoveAfterTeleport);
                _isTeleporting = false;
            }
        }

        public virtual void SendClientState(uint writeTick)
        {
            if (Movement != null && Movement.Enabled)
            {
                bool shouldSendReliably;
                s_EntityStateDataWriter.Reset();
                if (Movement.WriteClientState(writeTick, s_EntityStateDataWriter, out shouldSendReliably))
                {
                    TransportHandler.WritePacket(s_EntityStateMessageWriter, GameNetworkingConsts.EntityState);
                    s_EntityStateMessageWriter.PutPackedUInt(ObjectId);
                    s_EntityStateMessageWriter.PutPackedUInt(writeTick);
                    s_EntityStateMessageWriter.Put(s_EntityStateDataWriter.Data, 0, s_EntityStateDataWriter.Length);
                    ClientSendMessage(MOVEMENT_DATA_CHANNEL, shouldSendReliably ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Sequenced, s_EntityStateMessageWriter);
                }
            }
        }

        public virtual void SendServerState(uint writeTick)
        {
            if (Movement != null && Movement.Enabled)
            {
                bool shouldSendReliably;
                s_EntityStateDataWriter.Reset();
                if (Movement.WriteServerState(writeTick, s_EntityStateDataWriter, out shouldSendReliably))
                {
                    TransportHandler.WritePacket(s_EntityStateMessageWriter, GameNetworkingConsts.EntityState);
                    s_EntityStateMessageWriter.PutPackedUInt(ObjectId);
                    s_EntityStateMessageWriter.PutPackedUInt(writeTick);
                    s_EntityStateMessageWriter.Put(s_EntityStateDataWriter.Data, 0, s_EntityStateDataWriter.Length);
                    ServerSendMessageToSubscribers(MOVEMENT_DATA_CHANNEL, shouldSendReliably ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Sequenced, s_EntityStateMessageWriter);
                }
            }
        }

        public virtual void ReadClientStateAtServer(uint peerTick, NetDataReader reader)
        {
            if (Movement != null)
                Movement.ReadClientStateAtServer(peerTick, reader);
        }

        public virtual void ReadServerStateAtClient(uint peerTick, NetDataReader reader)
        {
            if (Movement != null)
                Movement.ReadServerStateAtClient(peerTick, reader);
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
            syncMetaDataId.syncMode = LiteNetLibSyncFieldMode.ServerToClients;
            syncTitle.syncMode = LiteNetLibSyncFieldMode.ServerToClients;
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

        public virtual void CallCmdPerformHitRegValidation(HitRegisterData hitData)
        {
            RPC(CmdPerformHitRegValidation, ACTION_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, hitData);
        }

        [ServerRpc]
        protected virtual void CmdPerformHitRegValidation(HitRegisterData hitData)
        {
            CurrentGameManager.HitRegistrationManager.PerformValidation(this, hitData);
        }
    }
}
