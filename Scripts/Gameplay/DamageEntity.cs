using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageEntity : RpgNetworkEntity
{
    protected CharacterEntity attacker;
    protected Dictionary<DamageElement, DamageAmount> damageElementAmountPairs;
    protected Dictionary<string, DamageEffectivenessAttribute> effectivenessAttributes;
    protected CharacterSkillLevel attackerSkillLevel;

    public virtual void SetupDamage(CharacterEntity attacker,
        Dictionary<DamageElement, DamageAmount> damageElementAmountPairs,
        Dictionary<string, DamageEffectivenessAttribute> effectivenessAttributes,
        CharacterSkillLevel attackerSkillLevel)
    {
        this.attacker = attacker;
        this.damageElementAmountPairs = damageElementAmountPairs;
        this.effectivenessAttributes = effectivenessAttributes;
        this.attackerSkillLevel = attackerSkillLevel;
    }

    public virtual void ApplyDamageTo(CharacterEntity target)
    {
        if (target == null)
            return;
        target.ReceiveDamage(attacker, damageElementAmountPairs, effectivenessAttributes, attackerSkillLevel);
    }
}
