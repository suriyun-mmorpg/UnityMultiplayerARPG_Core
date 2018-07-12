using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class RpgNetworkEntity : LiteNetLibBehaviour
    {
        [SerializeField]
        protected SyncFieldString title = new SyncFieldString();
        public Text textTitle;

        public virtual string Title { get { return title.Value; } set { title.Value = value; } }
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

        public override void OnSetup()
        {
            base.OnSetup();
            this.InvokeClassAddOnMethods("OnSetup");
        }

        public override void OnNetworkDestroy(DestroyObjectReasons reasons)
        {
            base.OnNetworkDestroy(reasons);
            this.InvokeClassAddOnMethods("OnNetworkDestroy", reasons);
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
