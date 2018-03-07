using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public class CharacterSkillLevel
{
    public string skillId;
    public int level;
    public float coolDownRemainsDuration;

    private string dirtySkillId;
    private int dirtyLevel;
    private Skill cacheSkill;
    private readonly Dictionary<DamageElement, DamageAmount> cacheDamageElementAmountPairs = new Dictionary<DamageElement, DamageAmount>();

    private bool IsDirty()
    {
        return string.IsNullOrEmpty(dirtySkillId) ||
            !dirtySkillId.Equals(skillId) ||
            dirtyLevel != level;
    }

    private void MakeCache()
    {
        if (!IsDirty())
            return;

        dirtySkillId = skillId;
        dirtyLevel = level;
        var gameInstance = GameInstance.Singleton;
        cacheSkill = GameInstance.Skills.ContainsKey(skillId) ? GameInstance.Skills[skillId] : null;
        cacheDamageElementAmountPairs.Clear();
        if (cacheSkill != null)
        {
            var damageAttributes = cacheSkill.damageAttributes;
            foreach (var damageAttribute in damageAttributes)
            {
                var element = damageAttribute.damageElement;
                if (element == null)
                    element = gameInstance.DefaultDamageElement;
                if (!cacheDamageElementAmountPairs.ContainsKey(element))
                    cacheDamageElementAmountPairs[element] = damageAttribute.damageAmount + damageAttribute.damageAmountIncreaseEachLevel * level;
            }
        }
    }

    public Skill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public Dictionary<DamageElement, DamageAmount> GetDamageElementAmountPairs()
    {
        MakeCache();
        return cacheDamageElementAmountPairs;
    }

    public int GetMaxLevel()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0;
        return skill.maxLevel;
    }

    public float GetConsumeMp()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.baseConsumeMp + skill.consumeMpIncreaseEachLevel * level;
    }

    public float GetCoolDownDuration()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.baseCoolDownDuration + skill.coolDownDurationIncreaseEachLevel * level;
    }

    #region Buff
    public CharacterStats GetBuffStats()
    {
        var skill = GetSkill();
        if (skill == null)
            return new CharacterStats();
        return skill.buff.baseStats + skill.buff.statsIncreaseEachLevel * level;
    }

    public CharacterStatsPercentage GetBuffStatsPercentage()
    {
        var skill = GetSkill();
        if (skill == null)
            return new CharacterStatsPercentage();
        return skill.buff.baseStatsPercentage + skill.buff.statsPercentageIncreaseEachLevel * level;
    }

    public float GetBuffDuration()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        var duration = skill.buff.baseDuration + skill.buff.durationIncreaseEachLevel * level;
        if (duration < 0)
            duration = 0;
        return duration;
    }

    public float GetBuffRecoveryHp()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.buff.baseRecoveryHp + skill.buff.recoveryHpIncreaseEachLevel * level;
    }

    public float GetBuffRecoveryMp()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.buff.baseRecoveryMp + skill.buff.recoveryMpIncreaseEachLevel * level;
    }
    #endregion

    #region Debuff
    public CharacterStats GetDebuffStats()
    {
        var skill = GetSkill();
        if (skill == null)
            return new CharacterStats();
        return skill.debuff.baseStats + skill.debuff.statsIncreaseEachLevel * level;
    }

    public CharacterStatsPercentage GetDebuffStatsPercentage()
    {
        var skill = GetSkill();
        if (skill == null)
            return new CharacterStatsPercentage();
        return skill.debuff.baseStatsPercentage + skill.debuff.statsPercentageIncreaseEachLevel * level;
    }

    public float GetDebuffDuration()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        var duration = skill.debuff.baseDuration + skill.debuff.durationIncreaseEachLevel * level;
        if (duration < 0)
            duration = 0;
        return duration;
    }

    public float GetDebuffRecoveryHp()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.debuff.baseRecoveryHp + skill.debuff.recoveryHpIncreaseEachLevel * level;
    }

    public float GetDebuffRecoveryMp()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.debuff.baseRecoveryMp + skill.debuff.recoveryMpIncreaseEachLevel * level;
    }
    #endregion

    public bool CanLevelUp()
    {
        return GetSkill() != null && level < GetMaxLevel();
    }

    public bool CanUse(int currentMp)
    {
        return GetSkill() != null && level >= 1 && coolDownRemainsDuration <= 0 && currentMp >= GetConsumeMp();
    }

    public void Used()
    {
        coolDownRemainsDuration = GetCoolDownDuration();
    }

    public bool ShouldUpdate()
    {
        return coolDownRemainsDuration > 0;
    }

    public void Update(float deltaTime)
    {
        coolDownRemainsDuration -= deltaTime;
    }
}

public class NetFieldCharacterSkillLevel : LiteNetLibNetField<CharacterSkillLevel>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterSkillLevel();
        newValue.skillId = reader.GetString();
        newValue.level = reader.GetInt();
        newValue.coolDownRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        if (Value == null)
            Value = new CharacterSkillLevel();
        writer.Put(Value.skillId);
        writer.Put(Value.level);
        writer.Put(Value.coolDownRemainsDuration);
    }

    public override bool IsValueChanged(CharacterSkillLevel newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterSkillLevel : LiteNetLibSyncList<NetFieldCharacterSkillLevel, CharacterSkillLevel> { }
