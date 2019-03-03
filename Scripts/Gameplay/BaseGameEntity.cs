using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLib;
using LiteNetLibManager;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract class BaseGameEntity : LiteNetLibBehaviour
    {
        [Header("Game Entity Settings")]
        public Text textTitle;
        public Text textTitleB;
        [Tooltip("These objects will be hidden on non owner objects")]
        public GameObject[] ownerObjects;
        [Tooltip("These objects will be hidden on owner objects")]
        public GameObject[] nonOwnerObjects;

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
            this.InvokeInstanceDevExtMethods("Start");
        }
        protected virtual void EntityStart() { }

        public override void OnSetOwnerClient()
        {
            EntityOnSetOwnerClient();
            this.InvokeInstanceDevExtMethods("OnSetOwnerClient");
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
            this.InvokeInstanceDevExtMethods("OnEnable");
        }
        protected virtual void EntityOnEnable() { }

        private void OnDisable()
        {
            EntityOnDisable();
            this.InvokeInstanceDevExtMethods("OnDisable");
        }
        protected virtual void EntityOnDisable() { }

        private void Update()
        {
            EntityUpdate();
            Profiler.BeginSample("RpgNetworkEntity - DevExUpdate");
            this.InvokeInstanceDevExtMethods("Update");
            Profiler.EndSample();
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
            this.InvokeInstanceDevExtMethods("OnSetup");
            SetupNetElements();
            RegisterNetFunction<uint>(NetFuncPlayEffect);
        }

        protected virtual void SetupNetElements()
        {
            this.InvokeInstanceDevExtMethods("SetupNetElements");
            syncTitle.deliveryMethod = DeliveryMethod.ReliableSequenced;
            syncTitle.forOwnerOnly = false;
            syncTitleB.deliveryMethod = DeliveryMethod.ReliableSequenced;
            syncTitleB.forOwnerOnly = false;
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
            this.InvokeInstanceDevExtMethods("OnNetworkDestroy", reasons);
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
    }
}
