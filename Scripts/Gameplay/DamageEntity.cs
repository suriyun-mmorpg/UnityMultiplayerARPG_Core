using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageEntity : RpgNetworkEntity
{
    protected CharacterEntity attacker;
    protected Dictionary<DamageElement, DamageAmount> damageElementAmountPairs;
    protected Dictionary<string, DamageEffectivenessAttribute> effectivenessAttributes;
    protected CharacterBuff debuff;

    public virtual void SetupDamage(CharacterEntity attacker,
        Dictionary<DamageElement, DamageAmount> damageElementAmountPairs,
        Dictionary<string, DamageEffectivenessAttribute> effectivenessAttributes,
        CharacterBuff debuff)
    {
        this.attacker = attacker;
        this.damageElementAmountPairs = damageElementAmountPairs;
        this.effectivenessAttributes = effectivenessAttributes;
        this.debuff = debuff;
    }

    public virtual void ApplyDamageTo(CharacterEntity target)
    {
        if (target == null)
            return;
        target.ReceiveDamage(attacker, damageElementAmountPairs, effectivenessAttributes, debuff);
    }
}
