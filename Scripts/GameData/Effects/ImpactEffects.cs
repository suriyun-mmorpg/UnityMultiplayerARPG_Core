using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.IMPACT_EFFECTS_FILE, menuName = GameDataMenuConsts.IMPACT_EFFECTS_MENU, order = GameDataMenuConsts.IMPACT_EFFECTS_ORDER)]
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
            List<GameEffect> effects = new List<GameEffect>(Effects.Values);
            effects.Add(defaultEffect);
            GameInstance.AddPoolingObjects(effects);
        }
    }
}
