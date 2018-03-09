using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public class CharacterBuff
{
    public string skillId;
    public bool isDebuff;
    public int level;
    public float buffRemainsDuration;

    private string dirtySkillId;
    private int dirtyLevel;
    private Skill cacheSkill;
    private readonly Dictionary<CharacterAttribute, int> cacheBuffAttributes = new Dictionary<CharacterAttribute, int>();
    private readonly Dictionary<CharacterResistance, float> cacheBuffResistances = new Dictionary<CharacterResistance, float>();
    private readonly Dictionary<CharacterAttribute, int> cacheDebuffAttributes = new Dictionary<CharacterAttribute, int>();
    private readonly Dictionary<CharacterResistance, float> cacheDebuffResistances = new Dictionary<CharacterResistance, float>();

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
        cacheSkill = GameInstance.Skills.ContainsKey(skillId) ? GameInstance.Skills[skillId] : null;
        cacheBuffAttributes.Clear();
        cacheBuffResistances.Clear();
        cacheDebuffAttributes.Clear();
        cacheDebuffResistances.Clear();
        if (cacheSkill != null)
        {
            if (isDebuff)
            {
                CharacterDataHelpers.MakeAttributeIncrementalCache(cacheSkill.debuff.increaseAttributes, cacheDebuffAttributes, level);
                CharacterDataHelpers.MakeResistanceIncrementalCache(cacheSkill.debuff.increaseResistances, cacheDebuffResistances, level);
            }
            else
            {
                CharacterDataHelpers.MakeAttributeIncrementalCache(cacheSkill.buff.increaseAttributes, cacheBuffAttributes, level);
                CharacterDataHelpers.MakeResistanceIncrementalCache(cacheSkill.buff.increaseResistances, cacheBuffResistances, level);
            }
        }
    }

    public Skill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public Dictionary<CharacterAttribute, int> GetBuffAttributes()
    {
        MakeCache();
        return cacheBuffAttributes;
    }

    public Dictionary<CharacterResistance, float> GetBuffResistances()
    {
        MakeCache();
        return cacheBuffResistances;
    }

    public Dictionary<CharacterAttribute, int> GetDebuffAttributes()
    {
        MakeCache();
        return cacheDebuffAttributes;
    }

    public Dictionary<CharacterResistance, float> GetDebuffResistances()
    {
        MakeCache();
        return cacheDebuffResistances;
    }

    #region Buff
    public CharacterStats GetBuffStats()
    {
        var skill = GetSkill();
        if (skill == null || isDebuff)
            return new CharacterStats();
        return skill.buff.baseStats + skill.buff.statsIncreaseEachLevel * level;
    }

    public float GetBuffDuration()
    {
        var skill = GetSkill();
        if (skill == null || isDebuff)
            return 0f;
        var duration = skill.buff.baseDuration + skill.buff.durationIncreaseEachLevel * level;
        if (duration < 0f)
            duration = 0f;
        return duration;
    }

    public float GetBuffRecoveryHp()
    {
        var skill = GetSkill();
        if (skill == null || isDebuff)
            return 0f;
        return skill.buff.baseRecoveryHp + skill.buff.recoveryHpIncreaseEachLevel * level;
    }

    public float GetBuffRecoveryMp()
    {
        var skill = GetSkill();
        if (skill == null || isDebuff)
            return 0f;
        return skill.buff.baseRecoveryMp + skill.buff.recoveryMpIncreaseEachLevel * level;
    }
    #endregion

    #region Debuff
    public CharacterStats GetDebuffStats()
    {
        var skill = GetSkill();
        if (skill == null || !isDebuff)
            return new CharacterStats();
        return skill.debuff.baseStats + skill.debuff.statsIncreaseEachLevel * level;
    }

    public float GetDebuffDuration()
    {
        var skill = GetSkill();
        if (skill == null || !isDebuff)
            return 0f;
        var duration = skill.debuff.baseDuration + skill.debuff.durationIncreaseEachLevel * level;
        if (duration < 0f)
            duration = 0f;
        return duration;
    }

    public float GetDebuffRecoveryHp()
    {
        var skill = GetSkill();
        if (skill == null || !isDebuff)
            return 0f;
        return skill.debuff.baseRecoveryHp + skill.debuff.recoveryHpIncreaseEachLevel * level;
    }

    public float GetDebuffRecoveryMp()
    {
        var skill = GetSkill();
        if (skill == null || !isDebuff)
            return 0f;
        return skill.debuff.baseRecoveryMp + skill.debuff.recoveryMpIncreaseEachLevel * level;
    }
    #endregion

    public bool ShouldRemove()
    {
        return buffRemainsDuration <= 0f;
    }

    public void Added()
    {
        buffRemainsDuration = GetBuffDuration();
    }

    public void Update(float deltaTime)
    {
        buffRemainsDuration -= deltaTime;
    }

    public static CharacterBuff MakeCharacterBuff(Skill skill, int level, bool isDebuff)
    {
        var newBuff = new CharacterBuff();
        newBuff.skillId = skill.Id;
        newBuff.level = level;
        newBuff.isDebuff = isDebuff;
        newBuff.buffRemainsDuration = 0f;
        return newBuff;
    }

    public CharacterBuff Clone()
    {
        var newBuff = new CharacterBuff();
        newBuff.skillId = skillId;
        newBuff.level = level;
        newBuff.isDebuff = isDebuff;
        newBuff.buffRemainsDuration = buffRemainsDuration;
        return newBuff;
    }
}

public class NetFieldCharacterBuff : LiteNetLibNetField<CharacterBuff>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterBuff();
        newValue.skillId = reader.GetString();
        newValue.isDebuff = reader.GetBool();
        newValue.level = reader.GetInt();
        newValue.buffRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        if (Value == null)
            Value = new CharacterBuff();
        writer.Put(Value.skillId);
        writer.Put(Value.isDebuff);
        writer.Put(Value.level);
        writer.Put(Value.buffRemainsDuration);
    }

    public override bool IsValueChanged(CharacterBuff newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterBuff : LiteNetLibSyncList<NetFieldCharacterBuff, CharacterBuff> { }
