using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct Summon
    {
        [Tooltip("Leave `Summon Monster` to NULL to not summon monster")]
        public MonsterCharacterEntity monsterEntity;
        [Tooltip("If duration less than or equals to 0, summoned monster will die")]
        public IncrementalFloat duration;
        public IncrementalInt amountEachTime;
        public IncrementalInt maxStack;
        public IncrementalShort level;
    }
}
