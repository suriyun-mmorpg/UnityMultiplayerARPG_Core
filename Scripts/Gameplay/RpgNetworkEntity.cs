using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLib;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class RpgNetworkEntity : LiteNetLibBehaviour
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
        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }

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

        protected virtual void Awake()
        {
            this.InvokeClassAddOnMethods("Awake");
        }

        protected virtual void Start()
        {
            this.InvokeClassAddOnMethods("Start");
        }

        protected virtual void OnEnable()
        {
            this.InvokeClassAddOnMethods("OnEnable");
        }

        protected virtual void OnDisable()
        {
            this.InvokeClassAddOnMethods("OnDisable");
        }

        protected virtual void Update()
        {
            this.InvokeClassAddOnMethods("Update");
        }

        protected virtual void LateUpdate()
        {
            this.InvokeClassAddOnMethods("LateUpdate");
            if (textTitle != null)
                textTitle.text = Title;
        }

        protected virtual void FixedUpdate()
        {
            this.InvokeClassAddOnMethods("FixedUpdate");
        }

        protected virtual void OnDestroy()
        {
            this.InvokeClassAddOnMethods("OnDestroy");
        }

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
            this.InvokeClassAddOnMethods("OnSetup");
            SetupNetElements();
            syncTitle.onChange += OnSyncTitleChange;
            if (IsServer)
                syncTitle.Value = title;
        }

        protected virtual void SetupNetElements()
        {
            syncTitle.sendOptions = SendOptions.ReliableUnordered;
            syncTitle.forOwnerOnly = false;
        }

        public override void OnNetworkDestroy(DestroyObjectReasons reasons)
        {
            base.OnNetworkDestroy(reasons);
            this.InvokeClassAddOnMethods("OnNetworkDestroy", reasons);
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
