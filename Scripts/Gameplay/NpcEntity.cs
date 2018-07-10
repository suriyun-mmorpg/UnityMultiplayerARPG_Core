using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public sealed class NpcEntity : RpgNetworkEntity
    {
        [Tooltip("Set it to force to not change character model by data Id, when set it model container will not be used")]
        public NpcDialog startDialog;
        [Tooltip("Model will be instantiated inside this transform, if not set will use this component's transform")]
        [SerializeField]
        private Transform modelContainer;
        [Tooltip("Set it to force to not change character model by data Id, when set it model container will not be used")]
        [SerializeField]
        private CharacterModel permanentlyModel;

        #region Sync data
        [SerializeField]
        private SyncFieldInt modelId = new SyncFieldInt();
        #endregion

        public int ModelId
        {
            get { return modelId.Value; }
            set { modelId.Value = value; }
        }

        #region Caches Data
        private CharacterModel model;
        public CharacterModel Model
        {
            get { return permanentlyModel != null ? permanentlyModel : model; }
            private set { model = permanentlyModel != null ? null : value; }
        }
        #endregion

        #region Cache components
        private CapsuleCollider cacheCapsuleCollider;
        public CapsuleCollider CacheCapsuleCollider
        {
            get
            {
                if (cacheCapsuleCollider == null)
                    cacheCapsuleCollider = GetComponent<CapsuleCollider>();
                return cacheCapsuleCollider;
            }
        }

        public Transform CacheModelContainer
        {
            get
            {
                if (modelContainer == null)
                    modelContainer = GetComponent<Transform>();
                return modelContainer;
            }
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = gameInstance.npcTag;
            gameObject.layer = gameInstance.characterLayer;
        }
        public override void OnBehaviourValidate()
        {
            base.OnBehaviourValidate();
#if UNITY_EDITOR
            SetupNetElements();
            EditorUtility.SetDirty(this);
#endif
        }

        private void SetupNetElements()
        {
            modelId.sendOptions = SendOptions.ReliableOrdered;
            modelId.forOwnerOnly = false;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            SetupNetElements();
            // On data changes events
            modelId.onChange += OnModelIdChange;
        }
        
        private void OnDestroy()
        {
            // On data changes events
            modelId.onChange -= OnModelIdChange;
        }

        private void OnModelIdChange(int modelId)
        {
            if (permanentlyModel == null)
            {
                if (Model != null)
                    Destroy(Model.gameObject);

                CharacterModel modelPrefab = null;
                if (!GameInstance.CharacterModels.TryGetValue(modelId, out modelPrefab))
                    return;

                Model = Instantiate(modelPrefab, CacheModelContainer);
                Model.gameObject.SetLayerRecursively(GameInstance.Singleton.characterLayer, true);
                Model.gameObject.SetActive(true);
                Model.transform.localPosition = Vector3.zero;
            }
            // If model is ready set its states, collider data and equipments
            if (Model != null)
            {
                CacheCapsuleCollider.center = Model.center;
                CacheCapsuleCollider.radius = Model.radius;
                CacheCapsuleCollider.height = Model.height;
            }
        }
    }
}
