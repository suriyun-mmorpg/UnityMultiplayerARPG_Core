using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(GuildSkill))]
    [CanEditMultipleObjects]
    public class GuildSkillEditor : BaseCustomEditor
    {
        private static GuildSkill cacheGuildSkill;
        protected override void SetFieldCondition()
        {
            if (cacheGuildSkill == null)
                cacheGuildSkill = CreateInstance<GuildSkill>();
            // Passive skill
            ShowOnEnum(cacheGuildSkill.GetMemberName(a => a.skillType), SkillType.Passive.ToString(), cacheGuildSkill.GetMemberName(a => a.increaseMaxMember));
            ShowOnEnum(cacheGuildSkill.GetMemberName(a => a.skillType), SkillType.Passive.ToString(), cacheGuildSkill.GetMemberName(a => a.increaseExpGainPercentage));
            ShowOnEnum(cacheGuildSkill.GetMemberName(a => a.skillType), SkillType.Passive.ToString(), cacheGuildSkill.GetMemberName(a => a.increaseGoldGainPercentage));
            ShowOnEnum(cacheGuildSkill.GetMemberName(a => a.skillType), SkillType.Passive.ToString(), cacheGuildSkill.GetMemberName(a => a.increaseShareExpGainPercentage));
            ShowOnEnum(cacheGuildSkill.GetMemberName(a => a.skillType), SkillType.Passive.ToString(), cacheGuildSkill.GetMemberName(a => a.increaseShareGoldGainPercentage));
            ShowOnEnum(cacheGuildSkill.GetMemberName(a => a.skillType), SkillType.Passive.ToString(), cacheGuildSkill.GetMemberName(a => a.decreaseExpLostPercentage));
            // Active skill
            ShowOnEnum(cacheGuildSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheGuildSkill.GetMemberName(a => a.coolDownDuration));
            ShowOnEnum(cacheGuildSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheGuildSkill.GetMemberName(a => a.buff));
        }
    }
}
