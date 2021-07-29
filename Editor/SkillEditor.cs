using UnityEditor;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(Skill))]
    [CanEditMultipleObjects]
    public class SkillEditor : BaseGameDataEditor
    {
        private static Skill cacheSkill;
        protected override void SetFieldCondition()
        {
            if (cacheSkill == null)
                cacheSkill = CreateInstance<Skill>();
            // Skill type
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.availableWeapons));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.consumeHp));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.consumeMp));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.consumeStamina));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.coolDownDuration));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.skillAttackType));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.skillBuffType));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.moveSpeedRateWhileUsingSkill));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.skillCastEffects));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.castDuration));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.canBeInterruptedWhileCasting));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.requireShield));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.availableWeapons));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.availableArmors));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.availableVehicles));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.requireItems));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.requireAmmoType));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.requireAmmoAmount));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.requireAmmos));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.consumeHp));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.consumeMp));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.consumeStamina));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.coolDownDuration));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.moveSpeedRateWhileUsingSkill));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.skillCastEffects));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.castDuration));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.canBeInterruptedWhileCasting));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.requireShield));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.availableWeapons));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.availableArmors));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.availableVehicles));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.requireItems));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.requireAmmoType));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.requireAmmoAmount));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.requireAmmos));
            // Normal Attack skill
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.Normal), nameof(cacheSkill.damageHitEffects));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.Normal), nameof(cacheSkill.damageInfo));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.Normal), nameof(cacheSkill.damageAmount));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.Normal), nameof(cacheSkill.effectivenessAttributes));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.Normal), nameof(cacheSkill.weaponDamageInflictions));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.Normal), nameof(cacheSkill.additionalDamageAmounts));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.Normal), nameof(cacheSkill.increaseDamageAmountsWithBuffs));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.Normal), nameof(cacheSkill.isDebuff));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.Normal), nameof(cacheSkill.harvestType));
            // Based On Weapon Attack skill
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.BasedOnWeapon), nameof(cacheSkill.damageHitEffects));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.BasedOnWeapon), nameof(cacheSkill.weaponDamageInflictions));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.BasedOnWeapon), nameof(cacheSkill.additionalDamageAmounts));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.BasedOnWeapon), nameof(cacheSkill.increaseDamageAmountsWithBuffs));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.BasedOnWeapon), nameof(cacheSkill.isDebuff));
            ShowOnEnum(nameof(cacheSkill.skillAttackType), nameof(Skill.SkillAttackType.BasedOnWeapon), nameof(cacheSkill.harvestType));
            // Harvest
            ShowOnEnum(nameof(cacheSkill.harvestType), nameof(HarvestType.BasedOnWeapon), nameof(cacheSkill.harvestDamageAmount));
            ShowOnEnum(nameof(cacheSkill.harvestType), nameof(HarvestType.BasedOnSkill), nameof(cacheSkill.harvestDamageAmount));
            // Debuff
            ShowOnBool(nameof(cacheSkill.isDebuff), true, nameof(cacheSkill.debuff));
            // Buff
            ShowOnEnum(nameof(cacheSkill.skillBuffType), nameof(Skill.SkillBuffType.BuffToNearbyAllies), nameof(cacheSkill.buffDistance));
            ShowOnEnum(nameof(cacheSkill.skillBuffType), nameof(Skill.SkillBuffType.BuffToNearbyCharacters), nameof(cacheSkill.buffDistance));
            ShowOnEnum(nameof(cacheSkill.skillBuffType), nameof(Skill.SkillBuffType.BuffToTarget), nameof(cacheSkill.buffDistance));
            ShowOnEnum(nameof(cacheSkill.skillBuffType), nameof(Skill.SkillBuffType.BuffToUser), nameof(cacheSkill.buff));
            ShowOnEnum(nameof(cacheSkill.skillBuffType), nameof(Skill.SkillBuffType.BuffToNearbyAllies), nameof(cacheSkill.buff));
            ShowOnEnum(nameof(cacheSkill.skillBuffType), nameof(Skill.SkillBuffType.BuffToNearbyCharacters), nameof(cacheSkill.buff));
            ShowOnEnum(nameof(cacheSkill.skillBuffType), nameof(Skill.SkillBuffType.BuffToTarget), nameof(cacheSkill.buff));
            ShowOnEnum(nameof(cacheSkill.skillBuffType), nameof(Skill.SkillBuffType.Toggle), nameof(cacheSkill.buff));
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Passive), nameof(cacheSkill.buff));
            // Summon
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.summon));
            // Mount
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.Active), nameof(cacheSkill.mount));
            // Craft
            ShowOnEnum(nameof(cacheSkill.skillType), nameof(SkillType.CraftItem), nameof(cacheSkill.itemCraft));
        }
    }
}
