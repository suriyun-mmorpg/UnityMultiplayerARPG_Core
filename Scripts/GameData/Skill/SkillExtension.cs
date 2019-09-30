using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class SkillExtension
    {
        #region Skill Extension
        public static short GetRequireCharacterLevel(this BaseSkill skill, short level)
        {
            if (skill == null)
                return 0;
            return skill.requirement.characterLevel.GetAmount((short)(level + 1));
        }

        public static int GetMaxLevel(this BaseSkill skill)
        {
            if (skill == null)
                return 0;
            return skill.maxLevel;
        }

        public static bool CanLevelUp(this BaseSkill skill, IPlayerCharacterData character, short level, bool checkSkillPoint = true)
        {
            if (skill == null || character == null || !character.GetDatabase().CacheSkillLevels.ContainsKey(skill))
                return false;

            // Check is it pass attribute requirement or not
            Dictionary<Attribute, float> attributeAmountsDict = character.GetAttributes(false, false);
            Dictionary<Attribute, float> requireAttributeAmounts = skill.CacheRequireAttributeAmounts;
            foreach (KeyValuePair<Attribute, float> requireAttributeAmount in requireAttributeAmounts)
            {
                if (!attributeAmountsDict.ContainsKey(requireAttributeAmount.Key) ||
                    attributeAmountsDict[requireAttributeAmount.Key] < requireAttributeAmount.Value)
                    return false;
            }
            // Check is it pass skill level requirement or not
            Dictionary<BaseSkill, int> skillLevelsDict = new Dictionary<BaseSkill, int>();
            foreach (CharacterSkill skillLevel in character.Skills)
            {
                if (skillLevel.GetSkill() == null)
                    continue;
                skillLevelsDict[skillLevel.GetSkill()] = skillLevel.level;
            }
            Dictionary<BaseSkill, short> requireSkillLevels = skill.CacheRequireSkillLevels;
            foreach (KeyValuePair<BaseSkill, short> requireSkillLevel in requireSkillLevels)
            {
                if (!skillLevelsDict.ContainsKey(requireSkillLevel.Key) ||
                    skillLevelsDict[requireSkillLevel.Key] < requireSkillLevel.Value)
                    return false;
            }
            // Check another requirements
            return (!checkSkillPoint || character.SkillPoint > 0) && level < skill.maxLevel && character.Level >= skill.GetRequireCharacterLevel(level);
        }

        public static bool CanUse(this BaseSkill skill, ICharacterData character, short level)
        {
            if (skill == null || character == null)
                return false;

            bool available = true;
            if (character is IPlayerCharacterData)
            {
                // Only player character will check for available weapons
                switch (skill.GetSkillType())
                {
                    case SkillType.Active:
                        WeaponType[] availableWeapons = skill.availableWeapons;
                        available = availableWeapons == null || availableWeapons.Length == 0;
                        if (!available)
                        {
                            Item rightWeaponItem = character.EquipWeapons.rightHand.GetWeaponItem();
                            Item leftWeaponItem = character.EquipWeapons.leftHand.GetWeaponItem();
                            foreach (WeaponType availableWeapon in availableWeapons)
                            {
                                if (rightWeaponItem != null && rightWeaponItem.WeaponType == availableWeapon)
                                {
                                    available = true;
                                    break;
                                }
                                else if (leftWeaponItem != null && leftWeaponItem.WeaponType == availableWeapon)
                                {
                                    available = true;
                                    break;
                                }
                                else if (rightWeaponItem == null && leftWeaponItem == null && GameInstance.Singleton.DefaultWeaponItem.WeaponType == availableWeapon)
                                {
                                    available = true;
                                    break;
                                }
                            }
                        }
                        break;
                    case SkillType.CraftItem:
                        if (!(character is BasePlayerCharacterEntity) || !skill.GetItemCraft().CanCraft(character as BasePlayerCharacterEntity))
                            return false;
                        break;
                    default:
                        return false;
                }
            }

            if (level <= 0)
                return false;

            if (!available)
                return false;

            if (character.CurrentMp < skill.GetConsumeMp(level))
                return false;

            int skillUsageIndex = character.IndexOfSkillUsage(skill.DataId, SkillUsageType.Skill);
            if (skillUsageIndex >= 0 && character.SkillUsages[skillUsageIndex].coolDownRemainsDuration > 0f)
                return false;
            return true;
        }

        public static short GetAdjustedLevel(this BaseSkill skill, short level)
        {
            if (skill == null)
                return 0;
            if (level > skill.maxLevel)
                level = skill.maxLevel;
            return level;
        }

        public static float GetCastDuration(this BaseSkill skill, short level)
        {
            if (skill == null)
                return 0;
            level = skill.GetAdjustedLevel(level);
            return skill.castDuration.GetAmount(level);
        }

        public static int GetConsumeMp(this BaseSkill skill, short level)
        {
            if (skill == null)
                return 0;
            level = skill.GetAdjustedLevel(level);
            return skill.consumeMp.GetAmount(level);
        }

        public static float GetCoolDownDuration(this BaseSkill skill, short level)
        {
            if (skill == null)
                return 0f;
            level = skill.GetAdjustedLevel(level);
            float duration = skill.coolDownDuration.GetAmount(level);
            if (duration < 0f)
                duration = 0f;
            return duration;
        }
        #endregion
    }
}
