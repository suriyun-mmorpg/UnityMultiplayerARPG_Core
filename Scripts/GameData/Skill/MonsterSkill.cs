using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct MonsterSkill
    {
        public BaseSkill skill;
        public int level;
        [Range(0.01f, 1f)]
        [Tooltip("Monster will random to use skill by this rate")]
        public float useRate;
        [Range(0f, 1f)]
        [Tooltip("Monster will use skill only when its Hp lower than this rate")]
        public float useWhenHpRate;
    }
}
