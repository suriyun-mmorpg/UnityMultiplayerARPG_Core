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
    private KeyValuePair<DamageElement, DamageAmount> cacheBaseDamageAttribute;
    private readonly Dictionary<DamageElement, DamageAmount> cacheAdditionalDamageAttributes = new Dictionary<DamageElement, DamageAmount>();
    private readonly Dictionary<DamageElement, DamageAmount> cacheInflictDamageAttributes = new Dictionary<DamageElement, DamageAmount>();

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
        cacheBaseDamageAttribute = new KeyValuePair<DamageElement, DamageAmount>();
        cacheAdditionalDamageAttributes.Clear();
        cacheInflictDamageAttributes.Clear();
        if (cacheSkill != null)
        {
            var baseDamageAttribute = cacheSkill.baseDamageAttribute;
            var baseElement = baseDamageAttribute.damageElement;
            if (baseElement == null)
                baseElement = gameInstance.DefaultDamageElement;
            cacheBaseDamageAttribute = new KeyValuePair<DamageElement, DamageAmount>(baseElement, baseDamageAttribute.baseDamageAmount + baseDamageAttribute.damageAmountIncreaseEachLevel * level);

            var additionalDamageAttributes = cacheSkill.additionalDamageAttributes;
            foreach (var damageAttribute in additionalDamageAttributes)
            {
                var element = damageAttribute.damageElement;
                if (element == null)
                    element = gameInstance.DefaultDamageElement;
                if (!cacheAdditionalDamageAttributes.ContainsKey(element))
                    cacheAdditionalDamageAttributes[element] = damageAttribute.baseDamageAmount + damageAttribute.damageAmountIncreaseEachLevel * level;
                else
                    cacheAdditionalDamageAttributes[element] += damageAttribute.baseDamageAmount + damageAttribute.damageAmountIncreaseEachLevel * level;
            }
            var inflictDamageAttributes = cacheSkill.inflictDamageAttributes;
            foreach (var damageAttribute in inflictDamageAttributes)
            {
                var element = damageAttribute.damageElement;
                if (element == null)
                    element = gameInstance.DefaultDamageElement;
                if (!cacheInflictDamageAttributes.ContainsKey(element))
                    cacheInflictDamageAttributes[element] = damageAttribute.baseDamageAmount + damageAttribute.damageAmountIncreaseEachLevel * level;
                else
                    cacheInflictDamageAttributes[element] += damageAttribute.baseDamageAmount + damageAttribute.damageAmountIncreaseEachLevel * level;
            }
        }
    }

    public Skill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public KeyValuePair<DamageElement, DamageAmount> GetBaseDamageAttribute()
    {
        MakeCache();
        return cacheBaseDamageAttribute;
    }

    public Dictionary<DamageElement, DamageAmount> GetAdditionalDamageAttributes()
    {
        MakeCache();
        return cacheAdditionalDamageAttributes;
    }

    public Dictionary<DamageElement, DamageAmount> GetInflictDamageAttributes()
    {
        MakeCache();
        return cacheInflictDamageAttributes;
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
        if (skill == null || skill.skillBuffType == SkillBuffType.None)
            return new CharacterStats();
        return skill.buff.baseStats + skill.buff.statsIncreaseEachLevel * level;
    }

    public CharacterStatsPercentage GetBuffStatsPercentage()
    {
        var skill = GetSkill();
        if (skill == null || skill.skillBuffType == SkillBuffType.None)
            return new CharacterStatsPercentage();
        return skill.buff.baseStatsPercentage + skill.buff.statsPercentageIncreaseEachLevel * level;
    }

    public float GetBuffDuration()
    {
        var skill = GetSkill();
        if (skill == null || skill.skillBuffType == SkillBuffType.None)
            return 0f;
        var duration = skill.buff.baseDuration + skill.buff.durationIncreaseEachLevel * level;
        if (duration < 0f)
            duration = 0f;
        return duration;
    }

    public float GetBuffRecoveryHp()
    {
        var skill = GetSkill();
        if (skill == null || skill.skillBuffType == SkillBuffType.None)
            return 0f;
        return skill.buff.baseRecoveryHp + skill.buff.recoveryHpIncreaseEachLevel * level;
    }

    public float GetBuffRecoveryMp()
    {
        var skill = GetSkill();
        if (skill == null || skill.skillBuffType == SkillBuffType.None)
            return 0f;
        return skill.buff.baseRecoveryMp + skill.buff.recoveryMpIncreaseEachLevel * level;
    }
    #endregion

    #region Debuff
    public CharacterStats GetDebuffStats()
    {
        var skill = GetSkill();
        if (skill == null || !skill.isDebuff)
            return new CharacterStats();
        return skill.debuff.baseStats + skill.debuff.statsIncreaseEachLevel * level;
    }

    public CharacterStatsPercentage GetDebuffStatsPercentage()
    {
        var skill = GetSkill();
        if (skill == null || !skill.isDebuff)
            return new CharacterStatsPercentage();
        return skill.debuff.baseStatsPercentage + skill.debuff.statsPercentageIncreaseEachLevel * level;
    }

    public float GetDebuffDuration()
    {
        var skill = GetSkill();
        if (skill == null || !skill.isDebuff)
            return 0f;
        var duration = skill.debuff.baseDuration + skill.debuff.durationIncreaseEachLevel * level;
        if (duration < 0f)
            duration = 0f;
        return duration;
    }

    public float GetDebuffRecoveryHp()
    {
        var skill = GetSkill();
        if (skill == null || !skill.isDebuff)
            return 0f;
        return skill.debuff.baseRecoveryHp + skill.debuff.recoveryHpIncreaseEachLevel * level;
    }

    public float GetDebuffRecoveryMp()
    {
        var skill = GetSkill();
        if (skill == null || !skill.isDebuff)
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
        return GetSkill() != null && level >= 1 && coolDownRemainsDuration <= 0f && currentMp >= GetConsumeMp();
    }

    public void Used()
    {
        coolDownRemainsDuration = GetCoolDownDuration();
    }

    public bool ShouldUpdate()
    {
        return coolDownRemainsDuration > 0f;
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
