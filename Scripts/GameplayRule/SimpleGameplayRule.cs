using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGameplayRule : BaseGameplayRule
{
    public override float GetHitChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver)
    {
        // Attacker stats
        var attackerStats = attacker.CacheStats;
        // Damage receiver stats
        var dmgReceiverStats = damageReceiver.CacheStats;
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

    public override float GetCriticalChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver)
    {
        var criRate = damageReceiver.CacheStats.criRate;
        // Minimum critical chance is 5%
        if (criRate < 0.05f)
            criRate = 0.05f;
        // Maximum critical chance is 95%
        if (criRate > 0.95f)
            criRate = 0.95f;
        return criRate;
    }

    public override float GetCriticalDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage)
    {
        return damage * attacker.CacheStats.criDmgRate;
    }

    public override float GetBlockChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver)
    {
        var blockRate = damageReceiver.CacheStats.blockRate;
        // Minimum block chance is 5%
        if (blockRate < 0.05f)
            blockRate = 0.05f;
        // Maximum block chance is 95%
        if (blockRate > 0.95f)
            blockRate = 0.95f;
        return blockRate;
    }

    public override float GetBlockDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage)
    {
        var blockDmgRate = damageReceiver.CacheStats.blockDmgRate;
        // Minimum block damage is 5%
        if (blockDmgRate < 0.05f)
            blockDmgRate = 0.05f;
        // Maximum block damage is 5%
        if (blockDmgRate > 0.95f)
            blockDmgRate = 0.95f;
        return damage * blockDmgRate;
    }

    public override float GetDamageReducedByResistance(BaseCharacterEntity damageReceiver, float damageAmount, DamageElement damageElement)
    {
        if (damageElement == null)
            return damageAmount -= damageReceiver.CacheStats.armor; // If armor is minus damage will be increased
        var resistances = damageReceiver.CacheResistances;
        float resistanceAmount = 0f;
        resistances.TryGetValue(damageElement, out resistanceAmount);
        if (resistanceAmount > damageElement.maxResistanceAmount)
            resistanceAmount = damageElement.maxResistanceAmount;
        return damageAmount -= damageAmount * resistanceAmount; // If resistance is minus damage will be increased
    }

    public override float GetRecoveryHpPerSeconds(BaseCharacterEntity character)
    {
        return character.CacheMaxHp * 0.01f; // 1% of total hp
    }

    public override float GetRecoveryMpPerSeconds(BaseCharacterEntity character)
    {
        return character.CacheMaxMp * 0.01f; // 1% of total mp
    }

    public override bool IncreaseExp(BaseCharacterEntity character, int exp)
    {
        var isLevelUp = false;
        var gameInstance = GameInstance.Singleton;
        var oldLevel = character.Level;
        character.Exp += exp;
        var nextLevelExp = character.GetNextLevelExp();
        if (nextLevelExp > 0 && character.Exp >= nextLevelExp)
        {
            character.Exp = character.Exp - nextLevelExp;
            ++character.Level;
            isLevelUp = true;
        }
        if (character is IPlayerCharacterData && character.Level > oldLevel)
        {
            var playerCharacter = character as IPlayerCharacterData;
            playerCharacter.StatPoint += gameInstance.increaseStatPointEachLevel;
            playerCharacter.SkillPoint += gameInstance.increaseSkillPointEachLevel;
        }
        return isLevelUp;
    }
}
