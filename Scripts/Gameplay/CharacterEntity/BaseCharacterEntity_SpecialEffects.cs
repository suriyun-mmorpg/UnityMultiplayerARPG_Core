using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public void PlayActionSpecialEffect(AnimActionType animActionType, int skillOrWeaponTypeDataId)
        {
            // Will play action special effects on clients only, and do not call on host because it is already called
            if (!IsClient || IsServer)
                return;

            // Prepare requires data
            bool isLeftHand = animActionType == AnimActionType.AttackLeftHand || animActionType == AnimActionType.SkillLeftHand;
            CharacterItem weapon = null;
            DamageInfo damageInfo = null;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts = null;
            CharacterBuff debuff = CharacterBuff.Empty;
            Skill skill = null;
            short level;
            byte fireSpread = 0;
            Vector3 fireStagger = Vector3.zero;

            // Get data to use with `LaunchDamageEntity` function
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                case AnimActionType.AttackLeftHand:
                    GetAttackingData(
                        ref isLeftHand,
                        out animActionType,
                        out skillOrWeaponTypeDataId,
                        out weapon,
                        out damageInfo,
                        out allDamageAmounts);

                    if (weapon != null && weapon.GetWeaponItem() != null)
                    {
                        // For monsters, their weapon can be null so have to avoid null exception
                        fireSpread = weapon.GetWeaponItem().fireSpread;
                        fireStagger = weapon.GetWeaponItem().fireStagger;
                    }
                    break;
                case AnimActionType.SkillRightHand:
                case AnimActionType.SkillLeftHand:
                    if (!GameInstance.Skills.TryGetValue(skillOrWeaponTypeDataId, out skill) ||
                        !CacheSkills.TryGetValue(skill, out level))
                        return;

                    GetUsingSkillData(
                        skill,
                        level,
                        ref isLeftHand,
                        out animActionType,
                        out skillOrWeaponTypeDataId,
                        out weapon,
                        out damageInfo,
                        out allDamageAmounts);
                    break;
                default:
                    // Not attack or use skill animation, so don't do anything
                    return;
            }

            Vector3 stagger;
            for (int i = 0; i < fireSpread + 1; ++i)
            {
                stagger = new Vector3(Random.Range(-fireStagger.x, fireStagger.x), Random.Range(-fireStagger.y, fireStagger.y));
                // TODO: Working on aim position
                LaunchDamageEntity(
                    isLeftHand,
                    weapon,
                    damageInfo,
                    allDamageAmounts,
                    debuff,
                    skill,
                    false,
                    Vector3.zero,
                    stagger);
            }
        }
    }
}
