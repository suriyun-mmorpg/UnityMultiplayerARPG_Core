using Insthync.AddressableAssetTools;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class SkillSummon : IAddressableAssetConversable
    {
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [Tooltip("Leave `Monster Entity` to NULL to not summon monster entity")]
        [SerializeField]
        [FormerlySerializedAs("monsterEntity")]
        private BaseMonsterCharacterEntity monsterCharacterEntity;
#endif
        public BaseMonsterCharacterEntity MonsterCharacterEntity
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return monsterCharacterEntity;
#else
                return null;
#endif
            }
        }

        [SerializeField]
        private AssetReferenceBaseMonsterCharacterEntity addressableMonsterCharacterEntity;
        public AssetReferenceBaseMonsterCharacterEntity AddressableMonsterCharacterEntity
        {
            get
            {
                return addressableMonsterCharacterEntity;
            }
        }

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

        public void ProceedAddressableAssetConversion()
        {
#if UNITY_EDITOR
            AddressableEditorUtils.ConvertObjectRefToAddressable(ref monsterCharacterEntity, ref addressableMonsterCharacterEntity);
#endif
        }
    }
}
