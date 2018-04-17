using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameplayRule : MonoBehaviour
{
    public abstract float GetHitChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver);
    public abstract float GetCriticalChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver);
    public abstract float GetCriticalDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage);
    public abstract float GetBlockChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver);
    public abstract float GetBlockDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage);
    public abstract float GetDamageReducedByResistance(BaseCharacterEntity damageReceiver, float damageAmount, DamageElement damageElement);
    public abstract float GetRecoveryHpPerSeconds(BaseCharacterEntity character);
    public abstract float GetRecoveryMpPerSeconds(BaseCharacterEntity character);
    public abstract bool IncreaseExp(BaseCharacterEntity character, int exp);
}
