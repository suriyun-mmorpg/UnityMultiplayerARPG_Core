using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class GuildSkillExtension
    {
        public static bool IsBuff(this GuildSkill guildSkill)
        {
            if (guildSkill == null)
                return false;
            return guildSkill.skillType == GuildSkillType.Active;
        }

        public static int GetMaxLevel(this GuildSkill guildSkill)
        {
            if (guildSkill == null)
                return 0;
            return guildSkill.maxLevel;
        }

        public static bool CanLevelUp(this GuildSkill guildSkill, IPlayerCharacterData character, short level)
        {
            if (guildSkill == null || character == null)
                return false;

            BaseGameNetworkManager gameManager = BaseGameNetworkManager.Singleton;
            if (gameManager == null)
                return false;

            GuildData guildData = null;
            if (!gameManager.TryGetGuild(character.GuildId, out guildData))
                return false;

            return guildData.skillPoint > 0 && level < guildSkill.maxLevel;
        }

        public static bool CanUse(this GuildSkill guildSkill, ICharacterData character, short level)
        {
            if (guildSkill == null || character == null)
                return false;
            if (level <= 0)
                return false;
            int skillUsageIndex = character.IndexOfSkillUsage(guildSkill.DataId, SkillUsageType.GuildSkill);
            if (skillUsageIndex >= 0 && character.SkillUsages[skillUsageIndex].coolDownRemainsDuration > 0f)
                return false;
            return true;
        }

        public static short GetAdjustedLevel(this GuildSkill guildSkill, short level)
        {
            if (guildSkill == null)
                return 0;
            if (level > guildSkill.maxLevel)
                level = guildSkill.maxLevel;
            return level;
        }

        public static float GetCoolDownDuration(this GuildSkill guildSkill, short level)
        {
            if (guildSkill == null)
                return 0f;
            level = guildSkill.GetAdjustedLevel(level);
            float duration = guildSkill.coolDownDuration.GetAmount(level);
            if (duration < 0f)
                duration = 0f;
            return duration;
        }
    }
}
