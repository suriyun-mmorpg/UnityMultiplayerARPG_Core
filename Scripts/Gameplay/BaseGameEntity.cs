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
        [SerializeField]
        private string title;
        public Text textTitle;

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

        private RpgEntityModel model;
        public RpgEntityModel Model
        {
            get
            {
                if (model == null)
                    model = GetComponent<RpgEntityModel>();
                return model;
            }
        }

        public GameInstance GameInstance
        {
            get { return GameInstance.Singleton; }
        }

        public BaseGameNetworkManager GameManager
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
        protected virtual void EntityOnSetOwnerClient() { }

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

        public override void OnBehaviourValidate()
        {
            base.OnBehaviourValidate();
#if UNITY_EDITOR
            SetupNetElements();
            EditorUtility.SetDirty(gameObject);
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
            RegisterNetFunction("PlayEffect", new LiteNetLibFunction<NetFieldPackedUInt>((effectId) => NetFuncPlayEffect(effectId)));
        }

        protected virtual void SetupNetElements()
        {
            this.InvokeInstanceDevExtMethods("SetupNetElements");
            syncTitle.sendOptions = SendOptions.ReliableUnordered;
            syncTitle.forOwnerOnly = false;
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
            CallNetFunction("PlayEffect", FunctionReceivers.All, effectId);
        }
        #endregion

        public override void OnNetworkDestroy(DestroyObjectReasons reasons)
        {
            base.OnNetworkDestroy(reasons);
            this.InvokeInstanceDevExtMethods("OnNetworkDestroy", reasons);
            syncTitle.onChange -= OnSyncTitleChange;
        }

        protected virtual void OnSyncTitleChange(string syncTitle)
        {
            title = syncTitle;
        }

        public bool TryGetEntityByObjectId<T>(uint objectId, out T result) where T : LiteNetLibBehaviour
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
