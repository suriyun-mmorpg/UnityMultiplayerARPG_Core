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
    }

    [System.Serializable]
    public struct StatusEffectApplying
    {
        public StatusEffect statusEffect;
        [Tooltip("Buff stats will be decreased by level")]
        public IncrementalInt buffLevel;

    }
}
