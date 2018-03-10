using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageEntity : RpgNetworkEntity
{
    protected CharacterEntity attacker;
    protected Dictionary<DamageElement, DamageAmount> allDamageAttributes;
    protected CharacterBuff debuff;

    public virtual void SetupDamage(CharacterEntity attacker,
        Dictionary<DamageElement, DamageAmount> allDamageAttributes,
        CharacterBuff debuff)
    {
        this.attacker = attacker;
        this.allDamageAttributes = allDamageAttributes;
        this.debuff = debuff;
    }

    public virtual void ApplyDamageTo(CharacterEntity target)
    {
        if (target == null)
            return;
        target.ReceiveDamage(attacker, allDamageAttributes, debuff);
    }
}
