using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Status Effect", menuName = "Create GameData/Status Effect", order = -4991)]
    public class StatusEffect : BaseGameData
    {
        [Header("Cool Down")]
        [SerializeField]
        private IncrementalFloat coolDownDuration;

        [Header("Buffs")]
        [SerializeField]
        private Buff buff;

        public Buff GetBuff()
        {
            return buff;
        }

        public float GetCoolDownDuration(short level)
        {
            float duration = coolDownDuration.GetAmount(level);
            if (duration < 0f)
                duration = 0f;
            return duration;
        }
    }
}
