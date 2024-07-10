using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.IMPACT_EFFECTS_FILE, menuName = GameDataMenuConsts.IMPACT_EFFECTS_MENU, order = GameDataMenuConsts.IMPACT_EFFECTS_ORDER)]
    public class ImpactEffects : ScriptableObject
    {
        [HideInInspector]
        public GameEffect defaultEffect;
        public ImpactEffect defaultImpactEffect;
        [FormerlySerializedAs("effects")]
        public ImpactEffect[] impaceEffects;

        [System.NonSerialized]
        private Dictionary<string, ImpactEffect> _cacheEffects;
        public Dictionary<string, ImpactEffect> Effects
        {
            get
            {
                if (_cacheEffects == null)
                {
                    _cacheEffects = new Dictionary<string, ImpactEffect>();
                    if (impaceEffects != null && impaceEffects.Length > 0)
                    {
                        _cacheEffects[defaultImpactEffect.tag.Tag] = defaultImpactEffect;
                        foreach (ImpactEffect effect in impaceEffects)
                        {
                            if (effect.effect == null)
                                continue;
                            _cacheEffects[effect.tag.Tag] = effect;
                        }
                    }
                }
                return _cacheEffects;
            }
        }

        public bool TryGetEffect(string tag, out ImpactEffect effect)
        {
            if (Effects.TryGetValue(tag, out effect))
                return true;
            if (defaultEffect != null)
            {
                effect = defaultImpactEffect;
                return true;
            }
            return false;
        }

        public void PlayEffect(string tag, Vector3 position, Quaternion rotation)
        {
            if (!TryGetEffect(tag, out ImpactEffect effect))
                return;
            effect.Play(position, rotation);
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (Migrate())
                EditorUtility.SetDirty(this);
#endif
        }

        private bool Migrate()
        {
            if (defaultEffect != null && defaultImpactEffect.effect == null)
            {
                ImpactEffect impactEffect = defaultImpactEffect;
                impactEffect.effect = defaultEffect;
                defaultImpactEffect = impactEffect;
                defaultEffect = null;
                return true;
            }
            return false;
        }

        public void PrepareRelatesData()
        {
            Migrate();
            List<GameEffect> effects = new List<GameEffect>();
            foreach (ImpactEffect effect in Effects.Values)
            {
                effects.Add(effect.effect);
            }
            GameInstance.AddPoolingObjects(effects);
        }
    }
}
