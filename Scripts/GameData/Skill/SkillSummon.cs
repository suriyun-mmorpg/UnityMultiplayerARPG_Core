using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct SkillSummon
    {
        public static readonly SkillSummon Empty = new SkillSummon();
        [Tooltip("Leave `Monster Entity` to NULL to not summon monster entity")]
        [SerializeField]
        [FormerlySerializedAs("monsterEntity")]
        private BaseMonsterCharacterEntity monsterCharacterEntity;
        public BaseMonsterCharacterEntity MonsterCharacterEntity { get { return monsterCharacterEntity; } }

        [SerializeField]
        private AssetReferenceBaseMonsterCharacterEntity addressableMonsterCharacterEntity;
        public AssetReferenceBaseMonsterCharacterEntity AddressableMonsterCharacterEntity { get { return addressableMonsterCharacterEntity; } }

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
            BaseMonsterCharacterEntity monsterCharacterEntity,
            AssetReferenceBaseMonsterCharacterEntity addressableMonsterCharacterEntity,
            IncrementalFloat duration,
            IncrementalInt amountEachTime,
            IncrementalInt maxStack,
            IncrementalInt level)
        {
            this.monsterCharacterEntity = monsterCharacterEntity;
            this.addressableMonsterCharacterEntity = addressableMonsterCharacterEntity;
            this.duration = duration;
            this.amountEachTime = amountEachTime;
            this.maxStack = maxStack;
            this.level = level;
        }
    }
}
