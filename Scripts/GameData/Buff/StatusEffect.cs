using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Status Effect", menuName = "Create GameData/Status Effect", order = -4991)]
    public class StatusEffect : BaseGameData
    {
        [Header("Buffs")]
        [SerializeField]
        private Buff buff;

        [SerializeField]
        private int maxStack;

        public Buff GetBuff()
        {
            return buff;
        }

        public int GetMaxStack()
        {
            return maxStack;
        }
    }

    [System.Serializable]
    public struct StatusEffectApplying
    {
        public StatusEffect statusEffect;
        [Tooltip("Buff stats will be decreased by level")]
        public IncrementalShort buffLevel;
        [Tooltip("1 = 100% chance to apply the status effect")]
        public IncrementalFloat chance;

    }
}
