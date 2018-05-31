using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(Skill))]
[CanEditMultipleObjects]
public class SkillEditor : BaseCustomEditor
{
    private static Skill cacheSkill;
    protected override void SetFieldCondition()
    {
        if (cacheSkill == null)
            cacheSkill = CreateInstance<Skill>();
        // Attack type
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.castAnimations));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.availableWeapons));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.consumeMp));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.coolDownDuration));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.skillAttackType));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Active.ToString(), cacheSkill.GetMemberName(a => a.skillBuffType));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.CraftItem.ToString(), cacheSkill.GetMemberName(a => a.castAnimations));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.CraftItem.ToString(), cacheSkill.GetMemberName(a => a.consumeMp));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.CraftItem.ToString(), cacheSkill.GetMemberName(a => a.coolDownDuration));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.CraftItem.ToString(), cacheSkill.GetMemberName(a => a.craftingItem));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.CraftItem.ToString(), cacheSkill.GetMemberName(a => a.craftRequirements));
        // Normal Attack skill
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.hitEffects));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.damageInfo));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.effectivenessAttributes));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.damageAmount));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.weaponDamageInflictions));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.additionalDamageAmounts));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), SkillAttackType.Normal.ToString(), cacheSkill.GetMemberName(a => a.isDebuff));
        // Based On Weapon Attack skill
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), SkillAttackType.BasedOnWeapon.ToString(), cacheSkill.GetMemberName(a => a.hitEffects));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), SkillAttackType.BasedOnWeapon.ToString(), cacheSkill.GetMemberName(a => a.weaponDamageInflictions));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), SkillAttackType.BasedOnWeapon.ToString(), cacheSkill.GetMemberName(a => a.additionalDamageAmounts));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillAttackType), SkillAttackType.BasedOnWeapon.ToString(), cacheSkill.GetMemberName(a => a.isDebuff));
        // Debuff
        ShowOnBool(cacheSkill.GetMemberName(a => a.isDebuff), true, cacheSkill.GetMemberName(a => a.debuff));
        // Buff
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillBuffType), SkillBuffType.BuffToUser.ToString(), cacheSkill.GetMemberName(a => a.buff));
        ShowOnEnum(cacheSkill.GetMemberName(a => a.skillType), SkillType.Passive.ToString(), cacheSkill.GetMemberName(a => a.buff));
    }
}
