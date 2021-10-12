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

        public bool Apply(System.Random random)
        {
            return random.NextDouble() <= applyRate;
        }

        public SkillLevel GetRandomedAmount(System.Random random)
        {
            return new SkillLevel()
            {
                skill = skill,
                level = (short)random.RandomInt(minLevel, maxLevel),
            };
        }
    }
}
