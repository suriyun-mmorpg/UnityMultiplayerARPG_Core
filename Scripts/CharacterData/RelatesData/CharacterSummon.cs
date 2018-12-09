using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

public enum SummonType : byte
{
    None,
    Skill,
    Pet,
}

[System.Serializable]
public class CharacterSummon : INetSerializable
{
    public static readonly CharacterSummon Empty = new CharacterSummon();
    public SummonType type;
    public int dataId;
    public short level;
    public float summonRemainsDuration;
    public int exp;
    public int currentHp;
    public int currentMp;
    public uint objectId;
    [System.NonSerialized]
    private int dirtyDataId;
    [System.NonSerialized]
    private Skill cacheSkill;
    [System.NonSerialized]
    private Item cachePetItem;
    [System.NonSerialized]
    private BaseMonsterCharacterEntity cachePrefab;

    [System.NonSerialized]
    private BaseMonsterCharacterEntity cacheEntity;
    public BaseMonsterCharacterEntity CacheEntity
    {
        get
        {
            if (cacheEntity == null)
            {
                LiteNetLibIdentity identity;
                if (BaseGameNetworkManager.Singleton.Assets.SpawnedObjects.TryGetValue(objectId, out identity))
                    cacheEntity = identity.GetComponent<BaseMonsterCharacterEntity>();
            }
            return cacheEntity;
        }
    }

    private void MakeCache()
    {
        if (dirtyDataId != dataId)
        {
            dirtyDataId = dataId;
            cacheSkill = null;
            cachePetItem = null;
            cachePrefab = null;
            switch (type)
            {
                case SummonType.Skill:
                    if (GameInstance.Skills.TryGetValue(dataId, out cacheSkill))
                        cachePrefab = cacheSkill.summon.monsterEntity;
                    break;
                case SummonType.Pet:
                    if (GameInstance.Items.TryGetValue(dataId, out cachePetItem))
                        cachePrefab = cachePetItem.petEntity;
                    break;
            }
        }
    }

    public Skill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public Item GetPetItem()
    {
        MakeCache();
        return cachePetItem;
    }

    public BaseMonsterCharacterEntity GetPrefab()
    {
        MakeCache();
        return cachePrefab;
    }

    public void Summon(BaseCharacterEntity summoner, short summonLevel, float duration)
    {
        if (GetPrefab() == null)
            return;
        var identity = BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(GetPrefab().Identity, summoner.GetSummonPosition(), summoner.GetSummonRotation());
        cacheEntity = identity.GetComponent<BaseMonsterCharacterEntity>();
        CacheEntity.Summon(summoner, type, summonLevel);
        level = CacheEntity.Level;
        exp = CacheEntity.Exp;
        currentHp = CacheEntity.CurrentHp;
        currentMp = CacheEntity.CurrentMp;
        objectId = CacheEntity.ObjectId;
        summonRemainsDuration = duration;
    }

    public void Summon(BaseCharacterEntity summoner, short summonLevel, float duration, int summonExp, int summonCurrentHp, int summonCurrentMp)
    {
        Summon(summoner, summonLevel, duration);
        CacheEntity.Exp = summonExp;
        CacheEntity.CurrentHp = summonCurrentHp;
        CacheEntity.CurrentMp = summonCurrentMp;
        exp = CacheEntity.Exp;
        currentHp = CacheEntity.CurrentHp;
        currentMp = CacheEntity.CurrentMp;
        objectId = CacheEntity.ObjectId;
    }

    public void DeSummon()
    {
        if (CacheEntity != null)
            CacheEntity.DeSummon();
    }

    public bool ShouldRemove()
    {
        return CacheEntity == null || CacheEntity.CurrentHp <= 0 || (type == SummonType.Skill && summonRemainsDuration <= 0f);
    }

    public void Update(float deltaTime)
    {
        if (CacheEntity == null)
            return;
        level = CacheEntity.Level;
        exp = CacheEntity.Exp;
        currentHp = CacheEntity.CurrentHp;
        currentMp = CacheEntity.CurrentMp;
        summonRemainsDuration -= deltaTime;
    }

    public static CharacterSummon Create(SummonType type, int dataId)
    {
        var newSummon = new CharacterSummon();
        newSummon.type = type;
        newSummon.dataId = dataId;
        return newSummon;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((byte)type);
        if (type != SummonType.None)
        {
            writer.Put(dataId);
            writer.Put(level);
            switch (type)
            {
                case SummonType.Pet:
                    writer.Put(exp);
                    break;
                case SummonType.Skill:
                    writer.Put(summonRemainsDuration);
                    break;
            }
            writer.Put(currentHp);
            writer.Put(currentMp);
            writer.PutPackedUInt(objectId);
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        type = (SummonType)reader.GetByte();
        if (type != SummonType.None)
        {
            dataId = reader.GetInt();
            level = reader.GetShort();
            switch (type)
            {
                case SummonType.Pet:
                    exp = reader.GetInt();
                    break;
                case SummonType.Skill:
                    summonRemainsDuration = reader.GetFloat();
                    break;
            }
            currentHp = reader.GetInt();
            currentMp = reader.GetInt();
            objectId = reader.GetPackedUInt();
        }
    }
}

[System.Serializable]
public class SyncFieldCharacterSummon : LiteNetLibSyncField<CharacterSummon>
{
    protected override bool IsValueChanged(CharacterSummon newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterSummon : LiteNetLibSyncList<CharacterSummon>
{
}