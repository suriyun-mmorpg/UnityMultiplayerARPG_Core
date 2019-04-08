using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class SkillExtension
    {
        #region Skill Extension
        public static bool IsAttack(this Skill skill)
        {
            if (skill == null)
                return false;
            return skill.skillAttackType != SkillAttackType.None;
        }

        public static bool IsBuff(this Skill skill)
        {
            if (skill == null)
                return false;
            return skill.skillType == SkillType.Passive || skill.skillBuffType != SkillBuffType.None;
        }

        public static bool IsDebuff(this Skill skill)
        {
            if (skill == null)
                return false;
            return skill.IsAttack() && skill.isDebuff;
        }

        public static short GetRequireCharacterLevel(this Skill skill, short level)
        {
            if (skill == null)
                return 0;
            return skill.requirement.characterLevel.GetAmount((short)(level + 1));
        }

        public static int GetMaxLevel(this Skill skill)
        {
            if (skill == null)
                return 0;
            return skill.maxLevel;
        }

        public static bool CanLevelUp(this Skill skill, IPlayerCharacterData character, short level)
        {
            if (skill == null || character == null || !character.GetDatabase().CacheSkillLevels.ContainsKey(skill))
                return false;

            // Check is it pass attribute requirement or not
            Dictionary<Attribute, short> attributeAmountsDict = character.GetAttributes(false, false);
            Dictionary<Attribute, short> requireAttributeAmounts = skill.CacheRequireAttributeAmounts;
            foreach (KeyValuePair<Attribute, short> requireAttributeAmount in requireAttributeAmounts)
            {
                if (!attributeAmountsDict.ContainsKey(requireAttributeAmount.Key) ||
                    attributeAmountsDict[requireAttributeAmount.Key] < requireAttributeAmount.Value)
                    return false;
            }
            // Check is it pass skill level requirement or not
            Dictionary<Skill, int> skillLevelsDict = new Dictionary<Skill, int>();
            foreach (CharacterSkill skillLevel in character.Skills)
            {
                if (skillLevel.GetSkill() == null)
                    continue;
                skillLevelsDict[skillLevel.GetSkill()] = skillLevel.level;
            }
            Dictionary<Skill, short> requireSkillLevels = skill.CacheRequireSkillLevels;
            foreach (KeyValuePair<Skill, short> requireSkillLevel in requireSkillLevels)
            {
                if (!skillLevelsDict.ContainsKey(requireSkillLevel.Key) ||
                    skillLevelsDict[requireSkillLevel.Key] < requireSkillLevel.Value)
                    return false;
            }
            // Check another requirements
            return character.SkillPoint > 0 && level < skill.maxLevel && character.Level >= skill.GetRequireCharacterLevel(level);
        }

        public static bool CanUse(this Skill skill, ICharacterData character, short level)
        {
            if (skill == null || character == null)
                return false;

            bool available = true;
            switch (skill.skillType)
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
                    if (!(character is BasePlayerCharacterEntity) || !skill.itemCraft.CanCraft(character as BasePlayerCharacterEntity))
                        return false;
                    break;
                default:
                    return false;
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

        public static short GetAdjustedLevel(this Skill skill, short level)
        {
            if (skill == null)
                return 0;
            if (level > skill.maxLevel)
                level = skill.maxLevel;
            return level;
        }

        public static float GetCastDuration(this Skill skill, short level)
        {
            if (skill == null)
                return 0;
            level = skill.GetAdjustedLevel(level);
            return skill.castDuration.GetAmount(level);
        }

        public static int GetConsumeMp(this Skill skill, short level)
        {
            if (skill == null)
                return 0;
            level = skill.GetAdjustedLevel(level);
            return skill.consumeMp.GetAmount(level);
        }

        public static float GetCoolDownDuration(this Skill skill, short level)
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

        #region Attack
        public static KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount(this Skill skill, short level, ICharacterData character)
        {
            if (!skill.IsAttack() || skill.skillAttackType != SkillAttackType.Normal)
                return new KeyValuePair<DamageElement, MinMaxFloat>();
            return GameDataHelpers.MakeDamage(skill.damageAmount, level, 1f, skill.GetEffectivenessDamage(character));
        }

        public static float GetEffectivenessDamage(this Skill skill, ICharacterData character)
        {
            if (skill == null)
                return 1f;
            return GameDataHelpers.GetEffectivenessDamage(skill.CacheEffectivenessAttributes, character);
        }

        public static Dictionary<DamageElement, float> GetWeaponDamageInflictions(this Skill skill, short level)
        {
            if (!skill.IsAttack())
                return new Dictionary<DamageElement, float>();
            level = skill.GetAdjustedLevel(level);
            return GameDataHelpers.CombineDamageInflictions(skill.weaponDamageInflictions, new Dictionary<DamageElement, float>(), level);
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetAdditionalDamageAmounts(this Skill skill, short level)
        {
            if (!skill.IsAttack())
                return new Dictionary<DamageElement, MinMaxFloat>();
            level = skill.GetAdjustedLevel(level);
            return GameDataHelpers.CombineDamages(skill.additionalDamageAmounts, new Dictionary<DamageElement, MinMaxFloat>(), level, 1f);
        }
        #endregion
    }
}
