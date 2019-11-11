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

        public static bool IsLearned(this BaseSkill skill, ICharacterData skillLearner)
        {
            if (skill == null)
                return false;
            // Check is skill learned
            short skillLevel;
            return skillLearner.GetCaches().Skills.TryGetValue(skill, out skillLevel) && skillLevel > 0;
        }

        public static bool CanLevelUp(this BaseSkill skill, IPlayerCharacterData character, short level, out GameMessage.Type gameMessageType, bool checkSkillPoint = true)
        {
            gameMessageType = GameMessage.Type.None;
            if (skill == null || character == null || !character.GetDatabase().CacheSkillLevels.ContainsKey(skill))
                return false;

            // Check is it pass attribute requirement or not
            Dictionary<Attribute, float> attributeAmountsDict = character.GetAttributes(false, false);
            Dictionary<Attribute, float> requireAttributeAmounts = skill.CacheRequireAttributeAmounts;
            foreach (KeyValuePair<Attribute, float> requireAttributeAmount in requireAttributeAmounts)
            {
                if (!attributeAmountsDict.ContainsKey(requireAttributeAmount.Key) ||
                    attributeAmountsDict[requireAttributeAmount.Key] < requireAttributeAmount.Value)
                {
                    gameMessageType = GameMessage.Type.NotEnoughAttributeAmounts;
                    return false;
                }
            }
            // Check is it pass skill level requirement or not
            Dictionary<BaseSkill, int> skillLevelsDict = new Dictionary<BaseSkill, int>();
            foreach (CharacterSkill learnedSkill in character.Skills)
            {
                if (learnedSkill.GetSkill() == null)
                    continue;
                skillLevelsDict[learnedSkill.GetSkill()] = learnedSkill.level;
            }
            foreach (BaseSkill requireSkill in skill.CacheRequireSkillLevels.Keys)
            {
                if (!skillLevelsDict.ContainsKey(requireSkill) ||
                    skillLevelsDict[requireSkill] < skill.CacheRequireSkillLevels[requireSkill])
                {
                    gameMessageType = GameMessage.Type.NotEnoughSkillLevels;
                    return false;
                }
            }

            if (character.Level < skill.GetRequireCharacterLevel(level))
            {
                gameMessageType = GameMessage.Type.NotEnoughLevel;
                return false;
            }

            if (skill.maxLevel > 0 && level >= skill.maxLevel)
            {
                gameMessageType = GameMessage.Type.SkillReachedMaxLevel;
                return false;
            }

            if (checkSkillPoint && character.SkillPoint <= 0)
            {
                gameMessageType = GameMessage.Type.NotEnoughSkillPoint;
                return false;
            }

            return true;
        }

        public static bool CanUse(this BaseSkill skill, ICharacterData skillUser, short level, out GameMessage.Type gameMessageType, bool isItem = false)
        {
            gameMessageType = GameMessage.Type.None;
            if (skill == null || skillUser == null)
                return false;
            
            if (level <= 0)
            {
                gameMessageType = GameMessage.Type.SkillLevelIsZero;
                return false;
            }
            
            bool available = true;
            if (skillUser is IPlayerCharacterData)
            {
                // Only player character will check is skill is learned
                if (!isItem && !skill.IsLearned(skillUser))
                {
                    gameMessageType = GameMessage.Type.SkillIsNotLearned;
                    return false;
                }

                // Only player character will check for available weapons
                switch (skill.GetSkillType())
                {
                    case SkillType.Active:
                        available = skill.availableWeapons == null || skill.availableWeapons.Length == 0;
                        if (!available)
                        {
                            Item rightWeaponItem = skillUser.EquipWeapons.GetRightHandWeaponItem();
                            Item leftWeaponItem = skillUser.EquipWeapons.GetLeftHandWeaponItem();
                            foreach (WeaponType availableWeapon in skill.availableWeapons)
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
                        if (!(skillUser is BasePlayerCharacterEntity) ||
                            !skill.GetItemCraft().CanCraft(skillUser as BasePlayerCharacterEntity, out gameMessageType))
                            return false;
                        break;
                    default:
                        return false;
                }
            }
            
            if (!available)
            {
                gameMessageType = GameMessage.Type.CannotUseSkillByCurrentWeapon;
                return false;
            }
            
            if (skillUser.CurrentMp < skill.GetConsumeMp(level))
            {
                gameMessageType = GameMessage.Type.NotEnoughMp;
                return false;
            }
            
            int skillUsageIndex = skillUser.IndexOfSkillUsage(skill.DataId, SkillUsageType.Skill);
            if (skillUsageIndex >= 0 && skillUser.SkillUsages[skillUsageIndex].coolDownRemainsDuration > 0f)
            {
                gameMessageType = GameMessage.Type.SkillIsCoolingDown;
                return false;
            }
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
