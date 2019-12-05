using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(Skill))]
    [CanEditMultipleObjects]
    public class SkillEditor : BaseCustomEditor
    {
        private static Skill cacheSkill;
        protected override void SetFieldCondition()
        {
            if (cacheSkill == null)
                cacheSkill = CreateInstance<Skill>();
            // Skill type
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.availableWeapons));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.consumeMp));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.coolDownDuration));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.skillAttackType));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.skillBuffType));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.moveSpeedRateWhileUsingSkill));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.skillCastEffects));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.castDuration));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.canBeInterruptedWhileCasting));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.CraftItem.ToString(), cacheSkill.GetMemberName(a => a.consumeMp));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.CraftItem.ToString(), cacheSkill.GetMemberName(a => a.coolDownDuration));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.CraftItem.ToString(), cacheSkill.GetMemberName(a => a.moveSpeedRateWhileUsingSkill));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.CraftItem.ToString(), cacheSkill.GetMemberName(a => a.skillCastEffects));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.CraftItem.ToString(), cacheSkill.GetMemberName(a => a.castDuration));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.CraftItem.ToString(), cacheSkill.GetMemberName(a => a.canBeInterruptedWhileCasting));
            // Normal Attack skill
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.damageHitEffects));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.damageInfo));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.damageAmount));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.effectivenessAttributes));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.weaponDamageInflictions));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.additionalDamageAmounts));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.increaseDamageAmountsWithBuffs));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.isDebuff));
            // Based On Weapon Attack skill
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.BasedOnWeapon.ToString(), cacheSkill.GetMemberName(a => a.damageHitEffects));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.BasedOnWeapon.ToString(), cacheSkill.GetMemberName(a => a.weaponDamageInflictions));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.BasedOnWeapon.ToString(), cacheSkill.GetMemberName(a => a.additionalDamageAmounts));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.BasedOnWeapon.ToString(), cacheSkill.GetMemberName(a => a.increaseDamageAmountsWithBuffs));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.BasedOnWeapon.ToString(), cacheSkill.GetMemberName(a => a.isDebuff));
            // Debuff
            ShowOnBool(cacheSkill.GetMemberName(a => a.isDebuff), true, cacheSkill.GetMemberName(a => a.debuff));
            // Buff
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillBuffType), Skill.SkillBuffType.BuffToNearbyAllies.ToString(), cacheSkill.GetMemberName(a => a.buffDistance));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillBuffType), Skill.SkillBuffType.BuffToNearbyCharacters.ToString(), cacheSkill.GetMemberName(a => a.buffDistance));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillBuffType), Skill.SkillBuffType.BuffToTarget.ToString(), cacheSkill.GetMemberName(a => a.buffDistance));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillBuffType), Skill.SkillBuffType.BuffToUser.ToString(), cacheSkill.GetMemberName(a => a.buff));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillBuffType), Skill.SkillBuffType.BuffToNearbyAllies.ToString(), cacheSkill.GetMemberName(a => a.buff));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillBuffType), Skill.SkillBuffType.BuffToNearbyCharacters.ToString(), cacheSkill.GetMemberName(a => a.buff));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillBuffType), Skill.SkillBuffType.BuffToTarget.ToString(), cacheSkill.GetMemberName(a => a.buff));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillBuffType), Skill.SkillBuffType.Toggle.ToString(), cacheSkill.GetMemberName(a => a.buff));
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Passive.ToString(), cacheSkill.GetMemberName(a => a.buff));
            // Summon
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.summon));
            // Mount
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.mount));
            // Craft
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.CraftItem.ToString(), cacheSkill.GetMemberName(a => a.itemCraft));
        }
    }
}
