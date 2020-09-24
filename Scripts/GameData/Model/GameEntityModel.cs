using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class GameEntityModel : MonoBehaviour
    {
        [System.Flags]
        public enum EVisibleState : byte
        {
            Visible,
            Invisible,
            Fps
        }

        [Tooltip("These object will be deactivated while hidding")]
        public GameObject[] hiddingObjects;
        [Tooltip("These renderers will be disabled while hidding")]
        public Renderer[] hiddingRenderers;
        [Tooltip("These object will be deactivated while view mode is FPS")]
        public GameObject[] fpsHiddingObjects;
        [Tooltip("These renderers will be disabled while view mode is FPS")]
        public Renderer[] fpsHiddingRenderers;

        public EVisibleState VisibleState { get; protected set; }
        protected GameInstance CurrentGameInstance { get { return GameInstance.Singleton; } }

        [Header("Effect Containers")]
        public EffectContainer[] effectContainers;
#if UNITY_EDITOR
        [InspectorButton(nameof(SetEffectContainersBySetters))]
        public bool setEffectContainersBySetters;
#endif

        public Transform CacheTransform { get; private set; }

        private Dictionary<string, EffectContainer> cacheEffectContainers = null;
        /// <summary>
        /// Dictionary[effectSocket(String), container(CharacterModelContainer)]
        /// </summary>
        public Dictionary<string, EffectContainer> CacheEffectContainers
        {
            get
            {
                if (cacheEffectContainers == null)
                {
                    cacheEffectContainers = new Dictionary<string, EffectContainer>();
                    foreach (EffectContainer effectContainer in effectContainers)
                    {
                        if (effectContainer.transform != null && !cacheEffectContainers.ContainsKey(effectContainer.effectSocket))
                            cacheEffectContainers[effectContainer.effectSocket] = effectContainer;
                    }
                }
                return cacheEffectContainers;
            }
        }

        // Optimize garbage collector
        private GameEffect tempGameEffect;

        protected virtual void Awake()
        {
            CacheTransform = transform;
        }

        protected virtual void OnValidate() { }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (effectContainers != null)
            {
                foreach (EffectContainer effectContainer in effectContainers)
                {
                    if (effectContainer.transform == null) continue;
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(effectContainer.transform.position, 0.1f);
                    Handles.Label(effectContainer.transform.position, effectContainer.effectSocket + "(Effect)");
                }
            }
        }
#endif

#if UNITY_EDITOR
        [ContextMenu("Set Effect Containers By Setters")]
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
                    SetHiddingObjectsAndRenderers(hiddingObjects, hiddingRenderers, false);
                    SetHiddingObjectsAndRenderers(fpsHiddingObjects, fpsHiddingRenderers, false);
                    break;
                case EVisibleState.Invisible:
                    // Visible state is Visible, hide all objects and renderers
                    SetHiddingObjectsAndRenderers(hiddingObjects, hiddingRenderers, true);
                    SetHiddingObjectsAndRenderers(fpsHiddingObjects, fpsHiddingRenderers, true);
                    break;
                case EVisibleState.Fps:
                    // Visible state is Fps, hide Fps objects and renderers
                    SetHiddingObjectsAndRenderers(hiddingObjects, hiddingRenderers, false);
                    SetHiddingObjectsAndRenderers(fpsHiddingObjects, fpsHiddingRenderers, true);
                    break;
            }
        }

        private void SetHiddingObjectsAndRenderers(GameObject[] hiddingObjects, Renderer[] hiddingRenderers, bool isHidding)
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
            List<GameEffect> tempAddingEffects = new List<GameEffect>();
            EffectContainer tempContainer;
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
                tempGameEffect.gameObject.SetLayerRecursively(CurrentGameInstance.characterLayer.LayerIndex, true);
                AddingNewEffect(tempGameEffect);
                tempAddingEffects.Add(tempGameEffect);
            }
            return tempAddingEffects;
        }

        public virtual void AddingNewEffect(GameEffect newEffect) { }
    }

    [System.Serializable]
    public struct EffectContainer
    {
        public string effectSocket;
        public Transform transform;
    }
}
