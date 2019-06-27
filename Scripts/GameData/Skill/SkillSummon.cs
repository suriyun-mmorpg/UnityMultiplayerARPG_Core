using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct SkillSummon
    {
        [Tooltip("Leave `Monster Entity` to NULL to not summon monster entity")]
        public BaseMonsterCharacterEntity monsterEntity;
        [Tooltip("If duration less than or equals to 0, summoned monster will die")]
        public IncrementalFloat duration;
        public IncrementalInt amountEachTime;
        public IncrementalInt maxStack;
        public IncrementalShort level;
    }
}
