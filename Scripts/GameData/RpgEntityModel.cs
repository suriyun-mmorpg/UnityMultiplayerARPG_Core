using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class RpgEntityModel : MonoBehaviour
    {
        [SerializeField]
        private int dataId;
        public int DataId { get { return dataId; } }

        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }

        [Header("Effect Containers")]
        public EffectContainer[] effectContainers;

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
                    foreach (var effectContainer in effectContainers)
                    {
                        if (effectContainer.transform != null && !cacheEffectContainers.ContainsKey(effectContainer.effectSocket))
                            cacheEffectContainers[effectContainer.effectSocket] = effectContainer;
                    }
                }
                return cacheEffectContainers;
            }
        }

        // Optimize garbage collector
        private readonly List<GameEffect> tempAddingEffects = new List<GameEffect>();
        private GameEffect tempGameEffect;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying && dataId != name.GenerateHashId())
            {
                dataId = name.GenerateHashId();
                EditorUtility.SetDirty(gameObject);
            }
        }
#endif

        public List<GameEffect> InstantiateEffect(GameEffect[] effects)
        {
            if (effects == null || effects.Length == 0)
                return new List<GameEffect>();
            tempAddingEffects.Clear();
            foreach (var effect in effects)
            {
                if (effect == null)
                    continue;
                if (string.IsNullOrEmpty(effect.effectSocket))
                    continue;
                EffectContainer container;
                if (!CacheEffectContainers.TryGetValue(effect.effectSocket, out container))
                    continue;
                tempGameEffect = effect.InstantiateTo(null);
                tempGameEffect.followingTarget = container.transform;
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
