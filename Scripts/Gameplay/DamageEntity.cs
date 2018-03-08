using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageEntity : RpgNetworkEntity
{
    protected CharacterEntity attacker;
    protected KeyValuePair<DamageElement, DamageAmount> baseDamageAttribute;
    protected Dictionary<DamageElement, DamageAmount> additionalDamageAttributes;
    protected Dictionary<string, float> effectivenessAttributes;
    protected CharacterBuff debuff;

    public virtual void SetupDamage(CharacterEntity attacker,
        KeyValuePair<DamageElement, DamageAmount> baseDamageAttribute,
        Dictionary<DamageElement, DamageAmount> additionalDamageAttributes,
        Dictionary<string, float> effectivenessAttributes,
        CharacterBuff debuff)
    {
        this.attacker = attacker;
        this.baseDamageAttribute = baseDamageAttribute;
        this.additionalDamageAttributes = additionalDamageAttributes;
        this.effectivenessAttributes = effectivenessAttributes;
        this.debuff = debuff;
    }

    public virtual void ApplyDamageTo(CharacterEntity target)
    {
        if (target == null)
            return;
        target.ReceiveDamage(attacker, baseDamageAttribute, additionalDamageAttributes, effectivenessAttributes, debuff);
    }
}
