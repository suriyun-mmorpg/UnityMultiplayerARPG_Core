using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGameplayRule : BaseGameplayRule
{
    public override float GetHitChance(ICharacterData attacker, ICharacterData damageReceiver)
    {
        // Damage receiver stats
        var dmgReceiverStats = damageReceiver.GetStatsWithBuffs();
        // Attacker stats
        var attackerStats = attacker.GetStatsWithBuffs();
        // Calculate chance to hit
        var hitChance = 2 * (attackerStats.accuracy / (attackerStats.accuracy + dmgReceiverStats.evasion)) * (attacker.Level / (attacker.Level + damageReceiver.Level));
        if (hitChance < 0.05f)
            hitChance = 0.05f;
        if (hitChance > 0.95f)
            hitChance = 0.95f;
        return hitChance;
    }

    public override float GetDamageReducedByResistance(ICharacterData damageReceiver, float damageAmount, Resistance resistance)
    {
        if (resistance == null)
            return damageAmount -= damageReceiver.GetArmor();
        var resistances = damageReceiver.GetResistancesWithBuffs();
        float resistanceAmount = 0f;
        resistances.TryGetValue(resistance, out resistanceAmount);
        if (resistanceAmount > resistance.maxAmount)
            resistanceAmount = resistance.maxAmount;
        return damageAmount -= damageAmount * resistanceAmount;
    }
}
