using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Damage Element", menuName = "Create GameData/Damage Element", order = -4994)]
    public partial class DamageElement : BaseGameData
    {
        [Header("Damage Element Configs")]
        [Range(0f, 1f)]
        public float maxResistanceAmount;
        [HideInInspector]
        [System.Obsolete("`GameEffectCollection` is deprecated and will be removed in future version")]
        public GameEffectCollection hitEffects;
        public GameEffect[] damageHitEffects;

        public override bool Validate()
        {
            bool hasChanges = false;
            if (hitEffects.effects != null && hitEffects.effects.Length > 0)
            {
                if (damageHitEffects == null || damageHitEffects.Length == 0)
                    damageHitEffects = hitEffects.effects;
                hitEffects.effects = null;
                hasChanges = true;
            }
            return base.Validate() || hasChanges;
        }

        public float GetDamageReducedByResistance(BaseCharacterEntity damageReceiver, float damageAmount)
        {
            return GameInstance.Singleton.GameplayRule.GetDamageReducedByResistance(damageReceiver, damageAmount, this);
        }

        public GameEffect[] GetDamageHitEffects()
        {
            return damageHitEffects;
        }
    }
}
