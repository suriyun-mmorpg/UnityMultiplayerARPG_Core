using System.Collections.Generic;
using UnityEngine;
using UtilsComponents;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [DefaultExecutionOrder(100)]
    public partial class GameEntityModel : MonoBehaviour
    {
        public enum EVisibleState : byte
        {
            Visible,
            Invisible,
            Fps
        }

        internal static int GeneratingId { get; set; } = 0;

        [SerializeField]
        [ReadOnlyField]
        protected int _id;

        public int Id
        {
            get
            {
                if (_id <= 0)
                    _id = ++GeneratingId;
                return _id;
            }
            protected set
            {
                _id = value;
            }
        }

        public EVisibleState VisibleState { get; protected set; }
        protected GameInstance CurrentGameInstance { get { return GameInstance.Singleton; } }

        [Tooltip("Materials which will be applied while entity is visible")]
        [SerializeField]
        protected MaterialCollection[] visibleMaterials;
        public MaterialCollection[] VisibleMaterials
        {
            get { return visibleMaterials; }
            set { visibleMaterials = value; }
        }

        [Tooltip("Materials which will be applied while entity is invisible")]
        [SerializeField]
        protected MaterialCollection[] invisibleMaterials;
        public virtual MaterialCollection[] InvisibleMaterials
        {
            get { return invisibleMaterials; }
            set { invisibleMaterials = value; }
        }

        [Tooltip("Materials which will be applied while view mode is FPS")]
        [SerializeField]
        protected MaterialCollection[] fpsMaterials;
        public MaterialCollection[] FpsMaterials
        {
            get { return fpsMaterials; }
            set { fpsMaterials = value; }
        }

        [Tooltip("These objects will be deactivated while entity is invisible")]
        [SerializeField]
        protected GameObject[] hiddingObjects;
        public GameObject[] HiddingObjects
        {
            get { return hiddingObjects; }
            set { hiddingObjects = value; }
        }

        [Tooltip("These renderers will be disabled while entity is invisible")]
        [SerializeField]
        protected Renderer[] hiddingRenderers;
        public Renderer[] HiddingRenderers
        {
            get { return hiddingRenderers; }
            set { hiddingRenderers = value; }
        }

        [Tooltip("These object will be deactivated while view mode is FPS")]
        [SerializeField]
        protected GameObject[] fpsHiddingObjects;
        public GameObject[] FpsHiddingObjects
        {
            get { return fpsHiddingObjects; }
            set { fpsHiddingObjects = value; }
        }

        [Tooltip("These renderers will be disabled while view mode is FPS")]
        [SerializeField]
        protected Renderer[] fpsHiddingRenderers;
        public Renderer[] FpsHiddingRenderers
        {
            get { return fpsHiddingRenderers; }
            set { fpsHiddingRenderers = value; }
        }

        [Tooltip("Generic audio source which will be used to play sound effects")]
        [SerializeField]
        protected AudioSource genericAudioSource;
        public AudioSource GenericAudioSource
        {
            get { return genericAudioSource; }
        }

        [Header("Effect Containers")]
        [SerializeField]
        protected EffectContainer[] effectContainers;
        public virtual EffectContainer[] EffectContainers
        {
            get { return effectContainers; }
            set { effectContainers = value; }
        }

        [Header("Effect Layer Settings")]
        [SerializeField]
        protected bool setEffectLayerFollowEntity = true;
        public bool SetEffectLayerFollowEntity
        {
            get { return setEffectLayerFollowEntity; }
            set { setEffectLayerFollowEntity = value; }
        }

        [SerializeField]
        protected UnityLayer effectLayer;
        public int EffectLayer
        {
            get { return effectLayer.LayerIndex; }
            set { effectLayer = new UnityLayer(value); }
        }

#if UNITY_EDITOR
        [InspectorButton(nameof(SetEffectContainersBySetters))]
        public bool setEffectContainersBySetters;
#endif

        public Transform CacheTransform { get; protected set; }
        public BaseGameEntity CacheEntity { get; protected set; }

        protected Dictionary<string, EffectContainer> _cacheEffectContainers = null;
        /// <summary>
        /// Dictionary[effectSocket(String), container(CharacterModelContainer)]
        /// </summary>
        public virtual Dictionary<string, EffectContainer> CacheEffectContainers
        {
            get { return _cacheEffectContainers; }
        }

        internal void AssignId()
        {
            Id = ++GeneratingId;
        }

        internal void UnassignId()
        {
            Id = 0;
        }

        protected virtual void Awake()
        {
            CacheTransform = transform;
            CacheEntity = GetComponentInParent<BaseGameEntity>();
            if (genericAudioSource == null)
            {
                genericAudioSource = gameObject.GetOrAddComponent<AudioSource>((obj) =>
                {
                    obj.spatialBlend = 1f;
                });
            }
            _cacheEffectContainers = new Dictionary<string, EffectContainer>();
            if (effectContainers != null && effectContainers.Length > 0)
            {
                foreach (EffectContainer effectContainer in effectContainers)
                {
                    if (effectContainer.transform != null && !_cacheEffectContainers.ContainsKey(effectContainer.effectSocket))
                        _cacheEffectContainers[effectContainer.effectSocket] = effectContainer;
                }
            }
        }

        protected virtual void OnValidate() { }

        public virtual void UpdateAnimation(float deltaTime) { }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (effectContainers != null)
            {
                foreach (EffectContainer effectContainer in effectContainers)
                {
                    if (effectContainer.transform == null) continue;
                    Gizmos.color = new Color(0, 0, 1, 0.5f);
                    Gizmos.DrawWireSphere(effectContainer.transform.position, 0.1f);
                    Gizmos.DrawSphere(effectContainer.transform.position, 0.03f);
                    Handles.Label(effectContainer.transform.position, effectContainer.effectSocket + "(Effect)");
                }
            }
        }
#endif

#if UNITY_EDITOR
        [ContextMenu("Set Effect Containers By Setters", false, 1000101)]
        public void SetEffectContainersBySetters()
        {
            EffectContainerSetter[] setters = GetComponentsInChildren<EffectContainerSetter>();
            if (setters != null && setters.Length > 0)
            {
                foreach (EffectContainerSetter setter in setters)
                {
                    setter.ApplyToCharacterModel(this);
                }
            }
            this.InvokeInstanceDevExtMethods("SetEffectContainersBySetters");
            EditorUtility.SetDirty(this);
        }
#endif

        public void SetVisibleState(EVisibleState visibleState)
        {
            if (VisibleState == visibleState)
                return;
            VisibleState = visibleState;
            switch (VisibleState)
            {
                case EVisibleState.Visible:
                    // Visible state is Visible, show all objects and renderers
                    SetHiddingObjectsAndRenderers(HiddingObjects, HiddingRenderers, false);
                    SetHiddingObjectsAndRenderers(FpsHiddingObjects, FpsHiddingRenderers, false);
                    VisibleMaterials.ApplyMaterials();
                    break;
                case EVisibleState.Invisible:
                    // Visible state is Invisible, hide all objects and renderers
                    SetHiddingObjectsAndRenderers(HiddingObjects, HiddingRenderers, true);
                    SetHiddingObjectsAndRenderers(FpsHiddingObjects, FpsHiddingRenderers, true);
                    InvisibleMaterials.ApplyMaterials();
                    break;
                case EVisibleState.Fps:
                    // Visible state is Fps, hide Fps objects and renderers
                    SetHiddingObjectsAndRenderers(HiddingObjects, HiddingRenderers, false);
                    SetHiddingObjectsAndRenderers(FpsHiddingObjects, FpsHiddingRenderers, true);
                    FpsMaterials.ApplyMaterials();
                    break;
            }
        }

        protected void SetHiddingObjectsAndRenderers(GameObject[] hiddingObjects, Renderer[] hiddingRenderers, bool isHidding)
        {
            int i;
            if (hiddingObjects != null && hiddingObjects.Length > 0)
            {
                for (i = 0; i < hiddingObjects.Length; ++i)
                {
                    hiddingObjects[i].SetActive(!isHidding);
                }
            }
            if (hiddingRenderers != null && hiddingRenderers.Length > 0)
            {
                for (i = 0; i < hiddingRenderers.Length; ++i)
                {
                    hiddingRenderers[i].forceRenderingOff = isHidding;
                }
            }
        }

        public List<GameEffect> InstantiateEffect(params GameEffect[] effects)
        {
            if (effects == null || effects.Length == 0)
                return null;
            return InstantiateEffect((IEnumerable<GameEffect>)effects);
        }

        public List<GameEffect> InstantiateEffect(IEnumerable<GameEffect> effects)
        {
            if (effects == null)
                return null;
            List<GameEffect> tempAddingEffects = new List<GameEffect>();
            EffectContainer tempContainer;
            GameEffect tempGameEffect;
            foreach (GameEffect effect in effects)
            {
                if (effect == null)
                    continue;
                if (string.IsNullOrEmpty(effect.effectSocket))
                    continue;
                if (!CacheEffectContainers.TryGetValue(effect.effectSocket, out tempContainer))
                    continue;
                // Setup transform and activate effect
                tempGameEffect = PoolSystem.GetInstance(effect, tempContainer.transform.position, tempContainer.transform.rotation);
                tempGameEffect.FollowingTarget = tempContainer.transform;
                if (SetEffectLayerFollowEntity)
                    tempGameEffect.gameObject.GetOrAddComponent<SetLayerFollowGameObject>((comp) => comp.source = CacheEntity.gameObject);
                else
                    tempGameEffect.gameObject.SetLayerRecursively(EffectLayer, true);
                AddingNewEffect(tempGameEffect);
                tempAddingEffects.Add(tempGameEffect);
            }
            return tempAddingEffects;
        }

        public virtual void AddingNewEffect(GameEffect newEffect) { }
    }
}
