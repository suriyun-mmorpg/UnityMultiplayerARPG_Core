using Insthync.AddressableAssetTools;
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
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [HideInInspector]
        public GameEffect defaultEffect;
#endif
        public ImpactEffect defaultImpactEffect;
        [FormerlySerializedAs("effects")]
        [FormerlySerializedAs("impaceEffects")]
        public ImpactEffect[] impactEffects;

        [System.NonSerialized]
        private Dictionary<string, ImpactEffect> _cacheEffects;
        public Dictionary<string, ImpactEffect> Effects
        {
            get
            {
                if (_cacheEffects == null)
                {
                    _cacheEffects = new Dictionary<string, ImpactEffect>();
                    if (impactEffects != null && impactEffects.Length > 0)
                    {
                        _cacheEffects[defaultImpactEffect.tag.Tag] = defaultImpactEffect;
                        foreach (ImpactEffect effect in impactEffects)
                        {
                            GameEffect gameEffect = null;
#if !EXCLUDE_PREFAB_REFS
                            gameEffect = effect.effect;
#endif
                            if (gameEffect == null && !effect.addressableEffect.IsDataValid())
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
            GameEffect gameEffect = null;
#if !EXCLUDE_PREFAB_REFS
            gameEffect = defaultImpactEffect.effect;
#endif
            if (gameEffect == null && !defaultImpactEffect.addressableEffect.IsDataValid())
            {
                effect = default;
                return false;
            }
            effect = defaultImpactEffect;
            return true;
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

        public void PrepareRelatesData()
        {
            Migrate();
        }

        private bool Migrate()
        {
#if !EXCLUDE_PREFAB_REFS
            if (defaultEffect != null && defaultImpactEffect.effect == null)
            {
                ImpactEffect impactEffect = defaultImpactEffect;
                impactEffect.effect = defaultEffect;
                defaultImpactEffect = impactEffect;
                defaultEffect = null;
                return true;
            }
#endif
            return false;
        }
    }
}
