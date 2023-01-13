using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct SkillSummon
    {
        public static readonly SkillSummon Empty = new SkillSummon();
        [Tooltip("Leave `Monster Entity` to NULL to not summon monster entity")]
        [SerializeField]
        private BaseMonsterCharacterEntity monsterEntity;
        public BaseMonsterCharacterEntity MonsterEntity { get { return monsterEntity; } }

        [Tooltip("If duration less than or equals to 0, summoned monster will die")]
        [SerializeField]
        private IncrementalFloat duration;
        public IncrementalFloat Duration { get { return duration; } }

        [SerializeField]
        private IncrementalInt amountEachTime;
        public IncrementalInt AmountEachTime { get { return amountEachTime; } }

        [SerializeField]
        private IncrementalInt maxStack;
        public IncrementalInt MaxStack { get { return maxStack; } }

        [SerializeField]
        private IncrementalInt level;
        public IncrementalInt Level { get { return level; } }

        public SkillSummon(
            BaseMonsterCharacterEntity monsterEntity,
            IncrementalFloat duration,
            IncrementalInt amountEachTime,
            IncrementalInt maxStack,
            IncrementalInt level)
        {
            this.monsterEntity = monsterEntity;
            this.duration = duration;
            this.amountEachTime = amountEachTime;
            this.maxStack = maxStack;
            this.level = level;
        }
    }
}
