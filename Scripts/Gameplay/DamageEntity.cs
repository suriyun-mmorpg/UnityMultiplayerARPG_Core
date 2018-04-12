using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibHighLevel;

public abstract class DamageEntity : RpgNetworkEntity
{
    protected BaseCharacterEntity attacker;
    protected Dictionary<DamageElement, DamageAmount> allDamageAttributes;
    protected CharacterBuff debuff;

    public virtual void SetupDamage(
        BaseCharacterEntity attacker,
        Dictionary<DamageElement, DamageAmount> allDamageAttributes,
        CharacterBuff debuff)
    {
        this.attacker = attacker;
        this.allDamageAttributes = allDamageAttributes;
        this.debuff = debuff;
    }

    public virtual void ApplyDamageTo(BaseCharacterEntity target)
    {
        if (target == null)
            return;
        target.ReceiveDamage(attacker, allDamageAttributes, debuff);
    }
}
