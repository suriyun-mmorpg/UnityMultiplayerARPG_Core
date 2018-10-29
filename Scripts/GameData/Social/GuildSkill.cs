using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Guild Skill", menuName = "Create GameData/Guild Skill")]
    public partial class GuildSkill : BaseGameData
    {
        [Header("Bonus")]
        public IncrementalInt increaseMaxMember;
        public IncrementalFloat increaseExpGainPercentage;
        public IncrementalFloat increaseGoldGainPercentage;
        public IncrementalFloat increaseShareExpGainPercentage;
        public IncrementalFloat increaseShareGoldGainPercentage;
        public IncrementalFloat decreaseExpLostPercentage;
        public CharacterStatsIncremental increaseStats;
        public AttributeIncremental[] increaseAttributes;
        public ResistanceIncremental[] increaseResistances;
        public DamageIncremental[] increaseDamages;
    }
}
