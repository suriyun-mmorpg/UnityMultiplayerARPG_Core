using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Impact Effects", menuName = "Create GameDatabase/Impact Effects", order = -5897)]
    public class ImpactEffects : ScriptableObject
    {
        public GameEffect defaultEffect;
        public ImpactEffect[] effects;

        [System.NonSerialized]
        private Dictionary<string, GameEffect> cacheEffects;
        public Dictionary<string, GameEffect> Effects
        {
            get
            {
                if (cacheEffects == null)
                {
                    cacheEffects = new Dictionary<string, GameEffect>();
                    if (effects != null && effects.Length > 0)
                    {
                        foreach (ImpactEffect effect in effects)
                        {
                            if (effect.effect == null)
                                continue;
                            cacheEffects[effect.tag.Tag] = effect.effect;
                        }
                    }
                }
                return cacheEffects;
            }
        }

        public GameEffect TryGetEffect(string tag)
        {
            if (Effects.ContainsKey(tag))
                return Effects[tag];
            return defaultEffect;
        }

        public void PrepareRelatesData()
        {
            PoolSystem.InitPool(defaultEffect);
            foreach (GameEffect effect in Effects.Values)
            {
                PoolSystem.InitPool(effect);
            }
        }
    }
}
