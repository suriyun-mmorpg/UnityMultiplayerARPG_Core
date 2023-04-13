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
        private Dictionary<string, GameEffect> _cacheEffects;
        public Dictionary<string, GameEffect> Effects
        {
            get
            {
                if (_cacheEffects == null)
                {
                    _cacheEffects = new Dictionary<string, GameEffect>();
                    if (effects != null && effects.Length > 0)
                    {
                        foreach (ImpactEffect effect in effects)
                        {
                            if (effect.effect == null)
                                continue;
                            _cacheEffects[effect.tag.Tag] = effect.effect;
                        }
                    }
                }
                return _cacheEffects;
            }
        }

        public bool TryGetEffect(string tag, out GameEffect effect)
        {
            if (Effects.TryGetValue(tag, out effect))
                return true;
            if (defaultEffect != null)
            {
                effect = defaultEffect;
                return true;
            }
            return false;
        }

        public void PrepareRelatesData()
        {
            List<GameEffect> effects = new List<GameEffect>(Effects.Values)
            {
                defaultEffect
            };
            GameInstance.AddPoolingObjects(effects);
        }
    }
}
