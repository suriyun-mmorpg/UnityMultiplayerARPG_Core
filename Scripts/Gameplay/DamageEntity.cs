using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageEntity : RpgNetworkEntity
{
    protected CharacterEntity attacker;
    protected Dictionary<string, DamageAmount> damageAmounts;
    protected Dictionary<string, DamageEffectivenessAttribute> effectivenessAttributes;

    public virtual void SetupDamage(CharacterEntity attacker,
        Dictionary<string, DamageAmount> damageAmounts,
        Dictionary<string, DamageEffectivenessAttribute> effectivenessAttributes)
    {
        this.attacker = attacker;
        this.damageAmounts = damageAmounts;
        this.effectivenessAttributes = effectivenessAttributes;
    }

    public virtual void ApplyDamageTo(CharacterEntity target)
    {
        if (target == null)
            return;
        target.ReceiveDamage(attacker, damageAmounts, effectivenessAttributes);
    }
}
