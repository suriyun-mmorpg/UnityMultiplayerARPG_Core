using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public class CharacterSkill
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
        cacheBaseDamageAttribute = new KeyValuePair<DamageElement, DamageAmount>();
        cacheAdditionalDamageAttributes.Clear();
        cacheInflictDamageAttributes.Clear();
        cacheBuffAttributes.Clear();
        cacheBuffResistances.Clear();
        cacheDebuffAttributes.Clear();
        cacheDebuffResistances.Clear();
        if (cacheSkill != null)
        {
            if (cacheSkill.skillAttackType != SkillAttackType.None)
            {
                cacheBaseDamageAttribute = GameDataHelpers.MakeDamageAttributePair(cacheSkill.baseDamageAttribute, level);
                GameDataHelpers.MakeDamageAttributesDictionary(cacheSkill.additionalDamageAttributes, cacheAdditionalDamageAttributes, level);
                GameDataHelpers.MakeDamageAttributesDictionary(cacheSkill.inflictDamageAttributes, cacheInflictDamageAttributes, level);
                if (cacheSkill.isDebuff)
                {
                    GameDataHelpers.MakeAttributeIncrementalDictionary(cacheSkill.debuff.increaseAttributes, cacheDebuffAttributes, level);
                    GameDataHelpers.MakeResistanceIncrementalDictionary(cacheSkill.debuff.increaseResistances, cacheDebuffResistances, level);
                }
            }
            if (cacheSkill.skillBuffType != SkillBuffType.None)
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

    public float GetInflictRate()
    {
        var skill = GetSkill();
        return skill == null ? 0f : skill.baseInflictRate + skill.inflictRateIncreaseEachLevel * level;
    }

    public Dictionary<DamageElement, DamageAmount> GetInflictDamageAttributes()
    {
        MakeCache();
        return cacheInflictDamageAttributes;
    }

    public Dictionary<Attribute, int> GetBuffAttributes()
    {
        MakeCache();
        return cacheBuffAttributes;
    }

    public Dictionary<Resistance, float> GetBuffResistances()
    {
        MakeCache();
        return cacheBuffResistances;
    }

    public Dictionary<Attribute, int> GetDebuffAttributes()
    {
        MakeCache();
        return cacheDebuffAttributes;
    }

    public Dictionary<Resistance, float> GetDebuffResistances()
    {
        MakeCache();
        return cacheDebuffResistances;
    }

    public int GetRequireCharacterLevel()
    {
        return GetSkill().GetRequireCharacterLevel(level);
    }

    public int GetConsumeMp()
    {
        return GetSkill().GetConsumeMp(level);
    }

    public float GetCoolDownDuration()
    {
        return GetSkill().GetCoolDownDuration(level);
    }
    
    public CharacterStats GetBuffStats()
    {
        return GetSkill().GetBuffStats(level);
    }

    public float GetBuffDuration()
    {
        return GetSkill().GetBuffDuration(level);
    }

    public int GetBuffRecoveryHp()
    {
        return GetSkill().GetBuffRecoveryHp(level);
    }

    public int GetBuffRecoveryMp()
    {
        return GetSkill().GetBuffRecoveryMp(level);
    }

    public CharacterStats GetDebuffStats()
    {
        return GetSkill().GetDebuffStats(level);
    }

    public float GetDebuffDuration()
    {
        return GetSkill().GetDebuffDuration(level);
    }

    public int GetDebuffRecoveryHp()
    {
        return GetSkill().GetDebuffRecoveryHp(level);
    }

    public int GetDebuffRecoveryMp()
    {
        return GetSkill().GetDebuffRecoveryMp(level);
    }

    public bool CanLevelUp(ICharacterData character)
    {
        return GetSkill().CanLevelUp(character, level);
    }

    public bool CanUse(int currentMp)
    {
        return level >= 1 && coolDownRemainsDuration <= 0f && currentMp >= GetConsumeMp();
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

public class NetFieldCharacterSkill : LiteNetLibNetField<CharacterSkill>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterSkill();
        newValue.skillId = reader.GetString();
        newValue.level = reader.GetInt();
        newValue.coolDownRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        if (Value == null)
            Value = new CharacterSkill();
        writer.Put(Value.skillId);
        writer.Put(Value.level);
        writer.Put(Value.coolDownRemainsDuration);
    }

    public override bool IsValueChanged(CharacterSkill newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterSkill : LiteNetLibSyncList<NetFieldCharacterSkill, CharacterSkill> { }
