using System.Collections;
using System.Collections.Generic;

public abstract class BaseDamageEntity : RpgNetworkEntity
{
    protected BaseCharacterEntity attacker;
    protected Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;
    protected CharacterBuff debuff;
    protected int hitEffectsId;

    public virtual void SetupDamage(
        BaseCharacterEntity attacker,
        Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
        CharacterBuff debuff,
        int hitEffectsId)
    {
        this.attacker = attacker;
        this.allDamageAmounts = allDamageAmounts;
        this.debuff = debuff;
        this.hitEffectsId = hitEffectsId;
    }

    public virtual void ApplyDamageTo(BaseCharacterEntity target)
    {
        if (target == null)
            return;
        target.ReceiveDamage(attacker, allDamageAmounts, debuff, hitEffectsId);
    }
}
