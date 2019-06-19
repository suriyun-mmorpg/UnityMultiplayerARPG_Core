using UnityEngine;
using UnityEngine.UI;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public abstract class BaseGameEntity : LiteNetLibBehaviour, IGameEntity
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
        [SerializeField]
        protected SyncFieldByte currentDirectionType = new SyncFieldByte();
        public virtual DirectionType2D CurrentDirectionType
        {
            get { return (DirectionType2D)currentDirectionType.Value; }
            set { currentDirectionType.Value = (byte)value; }
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

        private GameEntityModel model;
        public GameEntityModel Model
        {
            get
            {
                if (model == null)
                    model = GetComponent<GameEntityModel>();
                return model;
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
        protected virtual void EntityOnDestroy() { }

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
            currentDirectionType.deliveryMethod = DeliveryMethod.Sequenced;
            currentDirectionType.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentDirectionType.doNotSyncInitialDataImmediately = true;
        }

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
            return false;
        }

        public virtual void PlayJumpAnimation()
        {

        }
    }
}
