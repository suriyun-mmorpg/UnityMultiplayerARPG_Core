using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

public enum SkillUsageType : byte
{
    Skill,
    GuildSkill,
}

[System.Serializable]
public class CharacterSkillUsage
{
    public SkillUsageType type;
    public int dataId;
    public float coolDownRemainsDuration;
    [System.NonSerialized]
    private int dirtyDataId;
    [System.NonSerialized]
    private Skill cacheSkill;
    [System.NonSerialized]
    private GuildSkill cacheGuildSkill;

    private void MakeCache()
    {
        if (dirtyDataId != dataId)
        {
            dirtyDataId = dataId;
            switch (type)
            {
                case SkillUsageType.Skill:
                    GameInstance.Skills.TryGetValue(dataId, out cacheSkill);
                    break;
                case SkillUsageType.GuildSkill:
                    GameInstance.GuildSkills.TryGetValue(dataId, out cacheGuildSkill);
                    break;
            }
        }
    }

    public Skill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public GuildSkill GetGuildSkill()
    {
        MakeCache();
        return cacheGuildSkill;
    }

    public void Use(ICharacterData character, short level)
    {
        coolDownRemainsDuration = 0;
        switch (type)
        {
            case SkillUsageType.Skill:
                coolDownRemainsDuration = GetSkill().GetCoolDownDuration(level);
                var consumeMp = GetSkill().GetConsumeMp(level);
                if (character.CurrentMp >= consumeMp)
                    character.CurrentMp -= consumeMp;
                break;
            case SkillUsageType.GuildSkill:
                coolDownRemainsDuration = GetGuildSkill().GetCoolDownDuration(level);
                break;
        }
    }

    public bool ShouldUpdate()
    {
        return coolDownRemainsDuration > 0f;
    }

    public void Update(float deltaTime)
    {
        coolDownRemainsDuration -= deltaTime;
    }

    public void ClearCoolDown()
    {
        coolDownRemainsDuration = 0;
    }

    public static CharacterSkillUsage Create(string characterId, SkillUsageType type, int dataId)
    {
        var newSkillUsage = new CharacterSkillUsage();
        newSkillUsage.type = type;
        newSkillUsage.dataId = dataId;
        newSkillUsage.coolDownRemainsDuration = 0f;
        return newSkillUsage;
    }
}

public class NetFieldCharacterSkillUsage : LiteNetLibNetField<CharacterSkillUsage>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterSkillUsage();
        newValue.type = (SkillUsageType)reader.GetByte();
        newValue.dataId = reader.GetInt();
        newValue.coolDownRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put((byte)Value.type);
        writer.Put(Value.dataId);
        writer.Put(Value.coolDownRemainsDuration);
    }

    public override bool IsValueChanged(CharacterSkillUsage newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterSkillUsage : LiteNetLibSyncList<NetFieldCharacterSkillUsage, CharacterSkillUsage>
{
}