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
}
