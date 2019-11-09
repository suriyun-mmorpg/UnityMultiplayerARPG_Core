using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class GameEntityModel : MonoBehaviour
    {
        [Tooltip("These object will be deactivate while hidding")]
        public GameObject[] hiddingObjects;

        public bool IsHide { get; protected set; }
        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }

        [Header("Effect Containers")]
        public EffectContainer[] effectContainers;
        [InspectorButton("SetEffectContainersBySetters")]
        public bool setEffectContainersBySetters;

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

        protected virtual void Awake() { }

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
        }

        public void SetHide(bool isHide)
        {
            if (IsHide == isHide)
                return;
            IsHide = isHide;
            foreach (GameObject hiddingObject in hiddingObjects)
            {
                if (hiddingObject.activeSelf != !IsHide)
                    hiddingObject.SetActive(!IsHide);
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
                tempGameEffect = effect.InstantiateTo(null);
                tempGameEffect.followingTarget = tempContainer.transform;
                tempGameEffect.CacheTransform.position = tempGameEffect.followingTarget.position;
                tempGameEffect.CacheTransform.rotation = tempGameEffect.followingTarget.rotation;
                tempGameEffect.gameObject.SetActive(true);
                tempGameEffect.gameObject.SetLayerRecursively(gameInstance.characterLayer.LayerIndex, true);
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
