using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public sealed class NpcEntity : RpgNetworkEntity
    {
        [Tooltip("Model will be instantiated inside this transform, if not set will use this component's transform")]
        [SerializeField]
        private Transform modelContainer;
        [Tooltip("Set it to force to not change character model by data Id, when set it model container will not be used")]
        [SerializeField]
        private CharacterModel permanentlyModel;
        public NpcDialog startDialog;

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
    }
}
