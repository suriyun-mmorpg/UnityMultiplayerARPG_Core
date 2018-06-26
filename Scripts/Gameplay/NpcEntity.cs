using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class NpcEntity : RpgNetworkEntity
    {
        [SerializeField]
        private SyncFieldInt modelId = new SyncFieldInt();
        [Tooltip("Model will be instantiated inside this transform, if not set will use this component's transform")]
        [SerializeField]
        private Transform modelContainer;
        [Tooltip("Set it to force to not change character model by data Id, when set it model container will not be used")]
        [SerializeField]
        private CharacterModel permanentlyModel;
        public NpcDialog startDialog;

        private CharacterModel model;
        public CharacterModel Model
        {
            get { return permanentlyModel != null ? permanentlyModel : model; }
            private set { model = permanentlyModel != null ? null : value; }
        }
        public int ModelId { get { return modelId.Value; } set { modelId.Value = value; } }

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

        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = gameInstance.npcTag;
            gameObject.layer = gameInstance.characterLayer;
        }

        public override void OnSetup()
        {
            modelId.onChange += OnModelIdChange;
        }

        private void OnDestroy()
        {
            modelId.onChange -= OnModelIdChange;
        }

        private void OnModelIdChange(int modelId)
        {
            // If permanently model has been set, it will not changes character model
            if (permanentlyModel == null)
            {
                if (Model != null)
                    Destroy(Model.gameObject);

                CharacterModel modelPrefab;
                if (GameInstance.CharacterModels.TryGetValue(modelId, out modelPrefab))
                {
                    Model = Instantiate(modelPrefab, CacheModelContainer);
                    Model.gameObject.SetLayerRecursively(GameInstance.Singleton.characterLayer, true);
                    Model.gameObject.SetActive(true);
                    Model.transform.localPosition = Vector3.zero;
                }
            }
            // If model is ready set its states and collider data
            if (Model != null)
            {
                CacheCapsuleCollider.center = Model.center;
                CacheCapsuleCollider.radius = Model.radius;
                CacheCapsuleCollider.height = Model.height;
            }
        }
    }
}
