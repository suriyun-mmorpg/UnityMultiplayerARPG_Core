using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SimpleGameplayRule", menuName = "Create GameplayRule/SimpleGameplayRule")]
public class SimpleGameplayRule : BaseGameplayRule
{
    public int increaseStatPointEachLevel = 5;
    public int increaseSkillPointEachLevel = 1;
    public int hungryWhenFoodLowerThan = 40;
    public int thirstyWhenWaterLowerThan = 40;
    public int staminaRecoveryPerSeconds = 5;
    public int staminaDecreasePerSeconds = 5;
    public int foodDecreasePerSeconds = 4;
    public int waterDecreasePerSeconds = 2;
    public float moveSpeedRateWhileSprint = 1.5f;
    [Range(0f, 1f)]
    public float hpRecoveryRatePerSeconds = 0.05f;
    [Range(0f, 1f)]
    public float mpRecoveryRatePerSeconds = 0.05f;
    [Range(0f, 1f)]
    public float hpDecreaseRatePerSecondsWhenHungry = 0.05f;
    [Range(0f, 1f)]
    public float mpDecreaseRatePerSecondsWhenHungry = 0.05f;
    [Range(0f, 1f)]
    public float hpDecreaseRatePerSecondsWhenThirsty = 0.05f;
    [Range(0f, 1f)]
    public float mpDecreaseRatePerSecondsWhenThirsty = 0.05f;

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
        // Maximum block damage is 95%
        if (blockDmgRate > 0.95f)
            blockDmgRate = 0.95f;
        return damage - (damage * blockDmgRate);
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
        return character.CacheMaxHp * hpRecoveryRatePerSeconds;
    }

    public override float GetRecoveryMpPerSeconds(BaseCharacterEntity character)
    {
        return character.CacheMaxMp * mpRecoveryRatePerSeconds;
    }

    public override float GetRecoveryStaminaPerSeconds(BaseCharacterEntity character)
    {
        return staminaRecoveryPerSeconds;
    }

    public override float GetDecreasingHpPerSeconds(BaseCharacterEntity character)
    {
        if (character is MonsterCharacterEntity)
            return 0f;
        var result = 0f;
        if (character.CurrentFood < hungryWhenFoodLowerThan)
            result += character.CacheMaxHp * hpDecreaseRatePerSecondsWhenHungry;
        if (character.CurrentWater < thirstyWhenWaterLowerThan)
            result += character.CacheMaxHp * hpDecreaseRatePerSecondsWhenThirsty;
        return result;
    }

    public override float GetDecreasingMpPerSeconds(BaseCharacterEntity character)
    {
        if (character is MonsterCharacterEntity)
            return 0f;
        var result = 0f;
        if (character.CurrentFood < hungryWhenFoodLowerThan)
            result += character.CacheMaxMp * mpDecreaseRatePerSecondsWhenHungry;
        if (character.CurrentWater < thirstyWhenWaterLowerThan)
            result += character.CacheMaxMp * mpDecreaseRatePerSecondsWhenThirsty;
        return result;
    }

    public override float GetDecreasingStaminaPerSeconds(BaseCharacterEntity character)
    {
        if (character is MonsterCharacterEntity)
            return 0f;
        return staminaDecreasePerSeconds;
    }

    public override float GetDecreasingFoodPerSeconds(BaseCharacterEntity character)
    {
        if (character is MonsterCharacterEntity)
            return 0f;
        return foodDecreasePerSeconds;
    }

    public override float GetDecreasingWaterPerSeconds(BaseCharacterEntity character)
    {
        if (character is MonsterCharacterEntity)
            return 0f;
        return waterDecreasePerSeconds;
    }

    public override float GetMoveSpeed(BaseCharacterEntity character)
    {
        if (character is MonsterCharacterEntity)
        {
            var monsterCharacter = character as MonsterCharacterEntity;
            return monsterCharacter.isWandering ? monsterCharacter.MonsterDatabase.wanderMoveSpeed : monsterCharacter.CacheMoveSpeed;
        }
        return character.CacheMoveSpeed * (character.isSprinting ? moveSpeedRateWhileSprint : 1f);
    }

    public override bool IncreaseExp(BaseCharacterEntity character, int exp)
    {
        var isLevelUp = false;
        var oldLevel = character.Level;
        character.Exp += exp;
        var playerCharacter = character as IPlayerCharacterData;
        var nextLevelExp = character.GetNextLevelExp();
        while (nextLevelExp > 0 && character.Exp >= nextLevelExp)
        {
            character.Exp = character.Exp - nextLevelExp;
            ++character.Level;
            nextLevelExp = character.GetNextLevelExp();
            if (playerCharacter != null)
            {
                playerCharacter.StatPoint += increaseStatPointEachLevel;
                playerCharacter.SkillPoint += increaseSkillPointEachLevel;
            }
            isLevelUp = true;
        }
        return isLevelUp;
    }
}
