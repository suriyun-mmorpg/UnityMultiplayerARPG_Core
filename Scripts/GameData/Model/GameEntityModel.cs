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
        [InspectorButton("SetEffectContainersBySetters")]
        public bool setEffectContainersBySetters;
        
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

        protected virtual void OnDrawGizmos()
        {
#if UNITY_EDITOR
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
#endif
        }

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
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void SetVisibleState(EVisibleState visibleState)
        {
            if (VisibleState == visibleState)
                return;
            VisibleState = visibleState;
            int i = 0;
            switch (VisibleState)
            {
                case EVisibleState.Visible:
                    if (hiddingObjects != null && hiddingObjects.Length > 0)
                    {
                        for (i = 0; i < hiddingObjects.Length; ++i)
                        {
                            if (!hiddingObjects[i].activeSelf)
                                hiddingObjects[i].SetActive(true);
                        }
                    }
                    if (hiddingRenderers != null && hiddingRenderers.Length > 0)
                    {
                        for (i = 0; i < hiddingRenderers.Length; ++i)
                        {
                            if (!hiddingRenderers[i].enabled)
                                hiddingRenderers[i].enabled = true;
                        }
                    }
                    if (fpsHiddingObjects != null && fpsHiddingObjects.Length > 0)
                    {
                        for (i = 0; i < fpsHiddingObjects.Length; ++i)
                        {
                            if (fpsHiddingObjects[i].activeSelf)
                                fpsHiddingObjects[i].SetActive(true);
                        }
                    }
                    if (fpsHiddingRenderers != null && fpsHiddingRenderers.Length > 0)
                    {
                        for (i = 0; i < fpsHiddingRenderers.Length; ++i)
                        {
                            if (fpsHiddingRenderers[i].enabled)
                                fpsHiddingRenderers[i].enabled = true;
                        }
                    }
                    break;
                case EVisibleState.Invisible:
                    if (hiddingObjects != null && hiddingObjects.Length > 0)
                    {
                        for (i = 0; i < hiddingObjects.Length; ++i)
                        {
                            if (hiddingObjects[i].activeSelf)
                                hiddingObjects[i].SetActive(false);
                        }
                    }
                    if (hiddingRenderers != null && hiddingRenderers.Length > 0)
                    {
                        for (i = 0; i < hiddingRenderers.Length; ++i)
                        {
                            if (hiddingRenderers[i].enabled)
                                hiddingRenderers[i].enabled = false;
                        }
                    }
                    if (fpsHiddingObjects != null && fpsHiddingObjects.Length > 0)
                    {
                        for (i = 0; i < fpsHiddingObjects.Length; ++i)
                        {
                            if (fpsHiddingObjects[i].activeSelf)
                                fpsHiddingObjects[i].SetActive(false);
                        }
                    }
                    if (fpsHiddingRenderers != null && fpsHiddingRenderers.Length > 0)
                    {
                        for (i = 0; i < fpsHiddingRenderers.Length; ++i)
                        {
                            if (fpsHiddingRenderers[i].enabled)
                                fpsHiddingRenderers[i].enabled = false;
                        }
                    }
                    break;
                case EVisibleState.Fps:
                    if (hiddingObjects != null && hiddingObjects.Length > 0)
                    {
                        for (i = 0; i < hiddingObjects.Length; ++i)
                        {
                            if (!hiddingObjects[i].activeSelf)
                                hiddingObjects[i].SetActive(true);
                        }
                    }
                    if (hiddingRenderers != null && hiddingRenderers.Length > 0)
                    {
                        for (i = 0; i < hiddingRenderers.Length; ++i)
                        {
                            if (!hiddingRenderers[i].enabled)
                                hiddingRenderers[i].enabled = true;
                        }
                    }
                    if (fpsHiddingObjects != null && fpsHiddingObjects.Length > 0)
                    {
                        for (i = 0; i < fpsHiddingObjects.Length; ++i)
                        {
                            if (fpsHiddingObjects[i].activeSelf)
                                fpsHiddingObjects[i].SetActive(false);
                        }
                    }
                    if (fpsHiddingRenderers != null && fpsHiddingRenderers.Length > 0)
                    {
                        for (i = 0; i < fpsHiddingRenderers.Length; ++i)
                        {
                            if (fpsHiddingRenderers[i].enabled)
                                fpsHiddingRenderers[i].enabled = false;
                        }
                    }
                    break;
            }
        }

        public List<GameEffect> InstantiateEffect(GameEffect[] effects)
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
                tempGameEffect.followingTarget = tempContainer.transform;
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
