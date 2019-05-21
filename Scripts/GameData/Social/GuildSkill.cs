using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum GuildSkillType
    {
        Active,
        Passive,
    }

    [CreateAssetMenu(fileName = "Guild Skill", menuName = "Create GameData/Guild Skill", order = -4698)]
    public partial class GuildSkill : BaseGameData
    {
        [Header("Guild Skill Configs")]
        public GuildSkillType skillType;

        [Range(1, 100)]
        public short maxLevel = 1;

        [Header("Bonus")]
        public IncrementalInt increaseMaxMember;
        public IncrementalFloat increaseExpGainPercentage;
        public IncrementalFloat increaseGoldGainPercentage;
        public IncrementalFloat increaseShareExpGainPercentage;
        public IncrementalFloat increaseShareGoldGainPercentage;
        public IncrementalFloat decreaseExpLostPercentage;

        [Header("Cool Down")]
        public IncrementalFloat coolDownDuration;

        [Header("Buffs")]
        public Buff buff;
    }
}
