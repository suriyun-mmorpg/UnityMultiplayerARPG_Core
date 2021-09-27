using UnityEditor;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(GuildSkill))]
    [CanEditMultipleObjects]
    public class GuildSkillEditor : BaseGameDataEditor
    {
        protected override void SetFieldCondition()
        {
            GuildSkill guildSkill = CreateInstance<GuildSkill>();
            // Passive skill
            ShowOnEnum(nameof(guildSkill.skillType), nameof(SkillType.Passive), nameof(guildSkill.increaseMaxMember));
            ShowOnEnum(nameof(guildSkill.skillType), nameof(SkillType.Passive), nameof(guildSkill.increaseExpGainPercentage));
            ShowOnEnum(nameof(guildSkill.skillType), nameof(SkillType.Passive), nameof(guildSkill.increaseGoldGainPercentage));
            ShowOnEnum(nameof(guildSkill.skillType), nameof(SkillType.Passive), nameof(guildSkill.increaseShareExpGainPercentage));
            ShowOnEnum(nameof(guildSkill.skillType), nameof(SkillType.Passive), nameof(guildSkill.increaseShareGoldGainPercentage));
            ShowOnEnum(nameof(guildSkill.skillType), nameof(SkillType.Passive), nameof(guildSkill.decreaseExpLostPercentage));
            // Active skill
            ShowOnEnum(nameof(guildSkill.skillType), nameof(SkillType.Active), nameof(guildSkill.coolDownDuration));
            ShowOnEnum(nameof(guildSkill.skillType), nameof(SkillType.Active), nameof(guildSkill.buff));
        }
    }
}
