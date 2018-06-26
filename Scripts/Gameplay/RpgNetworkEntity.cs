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
        private SyncFieldString title = new SyncFieldString();
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

        protected virtual void Awake() { }

        protected virtual void Start() { }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected virtual void Update() { }

        protected virtual void LateUpdate()
        {
            if (textTitle != null)
                textTitle.text = Title;
        }

        protected virtual void FixedUpdate() { }
    }
}
