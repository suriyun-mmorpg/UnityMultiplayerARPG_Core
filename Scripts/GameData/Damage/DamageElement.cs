using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Damage Element", menuName = "Create GameData/Damage Element", order = -4994)]
    public partial class DamageElement : BaseGameData
    {
        [Header("Damage Element Configs")]
        [Range(0f, 1f)]
        public float maxResistanceAmount;
        [SerializeField]
        [System.Obsolete("`GameEffectCollection` will be removed in future version")]
        private GameEffectCollection hitEffects;
        [SerializeField]
        private GameEffect[] damageHitEffects;

        public override bool Validate()
        {
            bool hasChanges = false;
            if (hitEffects.effects != null && hitEffects.effects.Length > 0)
            {
                damageHitEffects = hitEffects.effects;
                hitEffects.effects = null;
                hasChanges = true;
            }
            return hasChanges;
        }

        public float GetDamageReducedByResistance(BaseCharacterEntity damageReceiver, float damageAmount)
        {
            return GameInstance.Singleton.GameplayRule.GetDamageReducedByResistance(damageReceiver, damageAmount, this);
        }

        public void SetDamageHitEffects(GameEffect[] effects)
        {
            damageHitEffects = effects;
        }

        public GameEffect[] GetDamageHitEffects()
        {
            return damageHitEffects;
        }
    }
}
