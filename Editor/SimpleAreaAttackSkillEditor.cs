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
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(SimpleAreaAttackSkill.SkillAttackType.Normal), nameof(cacheSkill.damageHitEffects));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(SimpleAreaAttackSkill.SkillAttackType.Normal), nameof(cacheSkill.damageAmount));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(SimpleAreaAttackSkill.SkillAttackType.Normal), nameof(cacheSkill.effectivenessAttributes));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(SimpleAreaAttackSkill.SkillAttackType.Normal), nameof(cacheSkill.weaponDamageInflictions));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(SimpleAreaAttackSkill.SkillAttackType.Normal), nameof(cacheSkill.additionalDamageAmounts));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(SimpleAreaAttackSkill.SkillAttackType.Normal), nameof(cacheSkill.increaseDamageAmountsWithBuffs));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(SimpleAreaAttackSkill.SkillAttackType.Normal), nameof(cacheSkill.isDebuff));
            // Based On Weapon Attack skill
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(SimpleAreaAttackSkill.SkillAttackType.BasedOnWeapon), nameof(cacheSkill.damageHitEffects));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(SimpleAreaAttackSkill.SkillAttackType.BasedOnWeapon), nameof(cacheSkill.weaponDamageInflictions));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(SimpleAreaAttackSkill.SkillAttackType.BasedOnWeapon), nameof(cacheSkill.additionalDamageAmounts));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(SimpleAreaAttackSkill.SkillAttackType.BasedOnWeapon), nameof(cacheSkill.increaseDamageAmountsWithBuffs));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(SimpleAreaAttackSkill.SkillAttackType.BasedOnWeapon), nameof(cacheSkill.isDebuff));
            // Debuff
            ShowOnBool(nameof(cacheSkill.isDebuff), true, nameof(cacheSkill.debuff));
        }
    }
}
