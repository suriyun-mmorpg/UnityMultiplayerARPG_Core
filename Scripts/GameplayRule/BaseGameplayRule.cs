using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameplayRule : ScriptableObject
{
    public abstract float GetHitChance(ICharacterData attacker, ICharacterData damageReceiver);
    public abstract float GetDamageReducedByResistance(ICharacterData damageReceiver, float damageAmount, Resistance resistance);
}
