using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.STATUS_EFFECT_FILE, menuName = GameDataMenuConsts.STATUS_EFFECT_MENU, order = GameDataMenuConsts.STATUS_EFFECT_ORDER)]
    public partial class StatusEffect : BaseGameData
    {
        [Category("Status Effect Settings")]
        [SerializeField]
        private Buff buff = Buff.Empty;

        public Buff Buff
        {
            get { return buff; }
        }

        [SerializeField]
        [Min(0f)]
        [Tooltip("If status effect resistance is `1.5`, it will `100%` resist status effect level `1` and `50%` resist status effect level `2`.")]
        private float maxResistanceAmount = 1f;
        public float MaxResistanceAmount { get { return maxResistanceAmount; } }

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("If value is `[0.8, 0.5, 0.25]`, and your character's status effect resistance is `2.15`, it will have chance `80%` to resist status effect level `1`, `50%` to resist level `2`, and `15%` to resist level `3`.")]
        private float[] maxResistanceAmountEachLevels = new float[0];
        public float[] MaxResistanceAmountEachLevels { get { return maxResistanceAmountEachLevels; } }

        /// <summary>
        /// This will be called when the buff is applied to `target` (by `applier`)
        /// </summary>
        /// <param name="target">The status effect receiver</param>
        /// <param name="applier">The status effect applier</param>
        /// <param name="weapon">The applier's weapon</param>
        /// <param name="sourceLevel">Level of a *thing* (skill or buff) which causes this status effect</param>
        /// <param name="applyBuffLevel">Level of a buff which applied to target</param>
        public virtual void OnApply(BaseCharacterEntity target, EntityInfo applier, CharacterItem weapon, int sourceLevel, int applyBuffLevel)
        {

        }
    }

    [System.Serializable]
    public struct StatusEffectApplying
    {
        public StatusEffect statusEffect;
        [Tooltip("Buff stats will be calculated by level")]
        public IncrementalInt buffLevel;
    }

    [System.Serializable]
    public struct StatusEffectResistanceAmount
    {
        public StatusEffect statusEffect;
        public float amount;
    }

    [System.Serializable]
    public struct StatusEffectResistanceIncremental
    {
        public StatusEffect statusEffect;
        public IncrementalFloat amount;
    }
}
