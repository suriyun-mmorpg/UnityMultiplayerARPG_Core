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
    private readonly Dictionary<Attribute, int> cacheBuffAttributes = new Dictionary<Attribute, int>();
    private readonly Dictionary<Resistance, float> cacheBuffResistances = new Dictionary<Resistance, float>();
    private readonly Dictionary<Attribute, int> cacheDebuffAttributes = new Dictionary<Attribute, int>();
    private readonly Dictionary<Resistance, float> cacheDebuffResistances = new Dictionary<Resistance, float>();

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
                GameDataHelpers.MakeAttributeIncrementalDictionary(cacheSkill.debuff.increaseAttributes, cacheDebuffAttributes, level);
                GameDataHelpers.MakeResistanceIncrementalDictionary(cacheSkill.debuff.increaseResistances, cacheDebuffResistances, level);
            }
            else
            {
                GameDataHelpers.MakeAttributeIncrementalDictionary(cacheSkill.buff.increaseAttributes, cacheBuffAttributes, level);
                GameDataHelpers.MakeResistanceIncrementalDictionary(cacheSkill.buff.increaseResistances, cacheBuffResistances, level);
            }
        }
    }

    public Skill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public Dictionary<Attribute, int> GetAttributes()
    {
        MakeCache();
        return !isDebuff ? cacheBuffAttributes : cacheDebuffAttributes;
    }

    public Dictionary<Resistance, float> GetResistances()
    {
        MakeCache();
        return !isDebuff ? cacheBuffResistances : cacheDebuffResistances;
    }

    public float GetDuration()
    {
        return !isDebuff ? GetSkill().GetBuffDuration(level) : GetSkill().GetDebuffDuration(level);
    }

    public CharacterStats GetStats()
    {
        return !isDebuff ? GetSkill().GetBuffStats(level) : GetSkill().GetDebuffStats(level);
    }

    public int GetBuffRecoveryHp()
    {
        return !isDebuff ? GetSkill().GetBuffRecoveryHp(level) : GetSkill().GetDebuffRecoveryHp(level);
    }

    public int GetBuffRecoveryMp()
    {
        return !isDebuff ? GetSkill().GetBuffRecoveryMp(level) : GetSkill().GetDebuffRecoveryMp(level);
    }

    public bool ShouldRemove()
    {
        return buffRemainsDuration <= 0f;
    }

    public void Added()
    {
        buffRemainsDuration = GetDuration();
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
