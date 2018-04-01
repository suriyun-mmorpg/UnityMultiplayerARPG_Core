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
            hitChance *= ((float)attackerLvl / (float)(attackerLvl + dmgReceiverLvl));

        // Minimum hit chance is 5%
        if (hitChance < 0.05f)
            hitChance = 0.05f;
        // Maximum hit chance is 95%
        if (hitChance > 0.95f)
            hitChance = 0.95f;
        return hitChance;
    }

    public override float GetCriticalChance(ICharacterData attacker, ICharacterData damageReceiver)
    {
        var criRate = damageReceiver.GetStatsWithBuffs().criRate;
        // Minimum critical chance is 5%
        if (criRate < 0.05f)
            criRate = 0.05f;
        // Maximum critical chance is 95%
        if (criRate > 0.95f)
            criRate = 0.95f;
        return criRate;
    }

    public override float GetCriticalDamage(ICharacterData attacker, ICharacterData damageReceiver, float damage)
    {
        return damage * attacker.GetStatsWithBuffs().criDmgRate;
    }

    public override float GetBlockChance(ICharacterData attacker, ICharacterData damageReceiver)
    {
        var blockRate = damageReceiver.GetStatsWithBuffs().blockRate;
        // Minimum block chance is 5%
        if (blockRate < 0.05f)
            blockRate = 0.05f;
        // Maximum block chance is 95%
        if (blockRate > 0.95f)
            blockRate = 0.95f;
        return blockRate;
    }

    public override float GetBlockDamage(ICharacterData attacker, ICharacterData damageReceiver, float damage)
    {
        var blockDmgRate = damageReceiver.GetStatsWithBuffs().blockDmgRate;
        // Minimum block damage is 5%
        if (blockDmgRate < 0.05f)
            blockDmgRate = 0.05f;
        // Maximum block damage is 5%
        if (blockDmgRate > 0.95f)
            blockDmgRate = 0.95f;
        return damage * blockDmgRate;
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

    public override void IncreaseExp(ICharacterData character, int exp)
    {
        var gameInstance = GameInstance.Singleton;
        var oldLevel = character.Level;
        character.Exp += exp;
        var nextLevelExp = character.GetNextLevelExp();
        if (nextLevelExp > 0 && character.Exp >= nextLevelExp)
        {
            character.Exp = nextLevelExp - character.Exp;
            ++character.Level;
        }
        if (character is IPlayerCharacterData && character.Level > oldLevel)
        {
            var playerCharacter = character as IPlayerCharacterData;
            playerCharacter.StatPoint += gameInstance.increaseStatPointEachLevel;
            playerCharacter.SkillPoint += gameInstance.increaseSkillPointEachLevel;
        }
    }
}
