using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct SkillLevel
    {
        public BaseSkill skill;
        public short level;
    }

    [System.Serializable]
    public struct SkillRandomLevel
    {
        public BaseSkill skill;
        public short minLevel;
        public short maxLevel;
        [Range(0, 1f)]
        public float applyRate;

        public bool Apply(int seed)
        {
            return GenericUtils.RandomFloat(seed, 0f, 1f) <= applyRate;
        }

        public SkillLevel GetRandomedAmount(int seed)
        {
            return new SkillLevel()
            {
                skill = skill,
                level = (short)GenericUtils.RandomInt(seed, minLevel, maxLevel),
            };
        }
    }
}
