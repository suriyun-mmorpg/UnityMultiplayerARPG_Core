using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(SimpleAreaAttackSkill))]
    [CanEditMultipleObjects]
    public class SimpleAreaAttackSkillEditor : BaseCustomEditor
    {
        private static SimpleAreaAttackSkill cacheSkill;
        protected override void SetFieldCondition()
        {
            if (cacheSkill == null)
                cacheSkill = CreateInstance<SimpleAreaAttackSkill>();
            // Normal Attack skill
            ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), Skill.SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.damageHitEffects));
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
        }
    }
}
