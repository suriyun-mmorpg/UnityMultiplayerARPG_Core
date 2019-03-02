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
        [SerializeField]
        private string title;
        [SerializeField]
        private string title2;
        public Text textTitle;
        public Text textTitle2;
        [Tooltip("These objects will be hidden on non owner objects")]
        public GameObject[] ownerObjects;
        [Tooltip("These objects will be hidden on owner objects")]
        public GameObject[] nonOwnerObjects;

        [SerializeField]
        protected SyncFieldString syncTitle = new SyncFieldString();
        public virtual string Title
        {
            get { return title; }
            set
            {
                title = value;
                if (IsServer)
                    syncTitle.Value = value;
            }
        }

        [SerializeField]
        protected SyncFieldString syncTitle2 = new SyncFieldString();
        public virtual string Title2
        {
            get { return title2; }
            set
            {
                title2 = value;
                if (IsServer)
                    syncTitle2.Value = value;
            }
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
            get { return Manager as BaseGameNetworkManager; }
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
            if (textTitle2 != null)
                textTitle2.text = Title2;
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
            syncTitle.onChange -= OnSyncTitleChange;
            syncTitle2.onChange -= OnSyncTitle2Change;
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
            this.InvokeInstanceDevExtMethods("OnSetup");
            SetupNetElements();
            syncTitle.onChange += OnSyncTitleChange;
            if (IsServer)
                syncTitle.Value = title;
            syncTitle2.onChange += OnSyncTitle2Change;
            if (IsServer)
                syncTitle2.Value = title;
            RegisterNetFunction<uint>(NetFuncPlayEffect);
        }

        protected virtual void SetupNetElements()
        {
            this.InvokeInstanceDevExtMethods("SetupNetElements");
            syncTitle.deliveryMethod = DeliveryMethod.ReliableSequenced;
            syncTitle.forOwnerOnly = false;
            syncTitle2.deliveryMethod = DeliveryMethod.ReliableSequenced;
            syncTitle2.forOwnerOnly = false;
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

        protected virtual void OnSyncTitleChange(string syncTitle)
        {
            title = syncTitle;
        }

        protected virtual void OnSyncTitle2Change(string syncTitle2)
        {
            title2 = syncTitle2;
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
