using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameplayRule : ScriptableObject
{
    public abstract float GetHitChance(ICharacterData attacker, ICharacterData damageReceiver);
    public abstract float GetCriticalChance(ICharacterData attacker, ICharacterData damageReceiver);
    public abstract float GetCriticalDamage(ICharacterData attacker, ICharacterData damageReceiver, float damage);
    public abstract float GetBlockChance(ICharacterData attacker, ICharacterData damageReceiver);
    public abstract float GetBlockDamage(ICharacterData attacker, ICharacterData damageReceiver, float damage);
    public abstract float GetDamageReducedByResistance(ICharacterData damageReceiver, float damageAmount, Resistance resistance);
    public abstract void IncreaseExp(ICharacterData character, int exp);
}
