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
            ShowOnEnum(nameof(cacheGuildSkill.skillType), nameof(SkillType.Passive), nameof(cacheGuildSkill.increaseMaxMember));
            ShowOnEnum(nameof(cacheGuildSkill.skillType), nameof(SkillType.Passive), nameof(cacheGuildSkill.increaseExpGainPercentage));
            ShowOnEnum(nameof(cacheGuildSkill.skillType), nameof(SkillType.Passive), nameof(cacheGuildSkill.increaseGoldGainPercentage));
            ShowOnEnum(nameof(cacheGuildSkill.skillType), nameof(SkillType.Passive), nameof(cacheGuildSkill.increaseShareExpGainPercentage));
            ShowOnEnum(nameof(cacheGuildSkill.skillType), nameof(SkillType.Passive), nameof(cacheGuildSkill.increaseShareGoldGainPercentage));
            ShowOnEnum(nameof(cacheGuildSkill.skillType), nameof(SkillType.Passive), nameof(cacheGuildSkill.decreaseExpLostPercentage));
            // Active skill
            ShowOnEnum(nameof(cacheGuildSkill.skillType), nameof(SkillType.Active), nameof(cacheGuildSkill.coolDownDuration));
            ShowOnEnum(nameof(cacheGuildSkill.skillType), nameof(SkillType.Active), nameof(cacheGuildSkill.buff));
        }
    }
}
