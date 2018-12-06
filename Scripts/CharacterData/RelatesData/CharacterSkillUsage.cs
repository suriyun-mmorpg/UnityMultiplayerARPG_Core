using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

public enum SkillUsageType : byte
{
    Skill,
    GuildSkill,
}

[System.Serializable]
public class CharacterSkillUsage : INetSerializable
{
    public static readonly CharacterSkillUsage Empty = new CharacterSkillUsage();
    public SkillUsageType type;
    public int dataId;
    public float coolDownRemainsDuration;
    public bool isSummoned;
    public int currentSummonedHp;
    public int currentSummonedMp;
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
        coolDownRemainsDuration = 0f;
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

    public bool ShouldRemove()
    {
        return coolDownRemainsDuration <= 0f;
    }

    public void Update(float deltaTime)
    {
        coolDownRemainsDuration -= deltaTime;
    }

    public static CharacterSkillUsage Create(string characterId, SkillUsageType type, int dataId)
    {
        var newSkillUsage = new CharacterSkillUsage();
        newSkillUsage.type = type;
        newSkillUsage.dataId = dataId;
        newSkillUsage.coolDownRemainsDuration = 0f;
        return newSkillUsage;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((byte)type);
        writer.Put(dataId);
        writer.Put(coolDownRemainsDuration);
        writer.Put(isSummoned);
        if (isSummoned)
        {
            writer.Put(currentSummonedHp);
            writer.Put(currentSummonedMp);
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        type = (SkillUsageType)reader.GetByte();
        dataId = reader.GetInt();
        coolDownRemainsDuration = reader.GetFloat();
        isSummoned = reader.GetBool();
        if (isSummoned)
        {
            currentSummonedHp = reader.GetInt();
            currentSummonedMp = reader.GetInt();
        }
    }
}

[System.Serializable]
public class SyncListCharacterSkillUsage : LiteNetLibSyncList<CharacterSkillUsage>
{
}