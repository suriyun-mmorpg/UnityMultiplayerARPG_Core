using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGameplayRule : BaseGameplayRule
{
    public override float GetHitChance(ICharacterData attacker, ICharacterData damageReceiver)
    {
        // Attacker stats
        var attackerStats = attacker.GetStatsWithBuffs();
        // Damage receiver stats
        var dmgReceiverStats = damageReceiver.GetStatsWithBuffs();
        // Calculate chance to hit
        var attackerAcc = attackerStats.accuracy;
        var dmgReceiverEva = dmgReceiverStats.evasion;
        var attackerLvl = attacker.Level;
        var dmgReceiverLvl = damageReceiver.Level;
        var hitChance = 2f;
        if (attackerAcc != 0 && dmgReceiverEva != 0)
            hitChance *= (attackerAcc / (attackerAcc + dmgReceiverEva));
        if (attackerLvl != 0 && dmgReceiverLvl != 0)
            hitChance *= (attackerLvl / (attackerLvl + dmgReceiverLvl));
        // Minimum hit chance is 5%
        if (hitChance < 0.05f)
            hitChance = 0.05f;
        // Maximum hit chance is 95%
        if (hitChance > 0.95f)
            hitChance = 0.95f;
        return hitChance;
    }

    public override float GetDamageReducedByResistance(ICharacterData damageReceiver, float damageAmount, Resistance resistance)
    {
        if (resistance == null)
            return damageAmount -= damageReceiver.GetStats().armor; // If armor is minus damage will be increased
        var resistances = damageReceiver.GetResistancesWithBuffs();
        float resistanceAmount = 0f;
        resistances.TryGetValue(resistance, out resistanceAmount);
        if (resistanceAmount > resistance.maxAmount)
            resistanceAmount = resistance.maxAmount;
        return damageAmount -= damageAmount * resistanceAmount; // If resistance is minus damage will be increased
    }
}
