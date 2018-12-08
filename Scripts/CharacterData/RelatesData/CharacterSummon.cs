using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;
using UnityEngine;

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
            cachePetItem = null;
            switch (type)
            {
                case SummonType.Skill:
                    GameInstance.Skills.TryGetValue(dataId, out cacheSkill);
                    break;
                case SummonType.Pet:
                    GameInstance.Items.TryGetValue(dataId, out cachePetItem);
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

    public void Summon(BaseCharacterEntity summoner, short level, int exp, int currentHp, int currentMp)
    {
        BaseMonsterCharacterEntity prefab = null;
        switch (type)
        {
            case SummonType.Skill:
                prefab = GetSkill().summon.monsterEntity;
                break;
            case SummonType.Pet:
                prefab = GetPetItem().petEntity;
                break;
        }
        if (prefab == null)
            return;
        var identity = BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(prefab.Identity, summoner.CacheTransform.position, summoner.CacheTransform.rotation);
        cacheEntity = identity.GetComponent<BaseMonsterCharacterEntity>();
        CacheEntity.Summon(summoner, type, level, exp, currentHp, currentMp);
        level = CacheEntity.Level;
        exp = CacheEntity.Exp;
        currentHp = CacheEntity.CurrentHp;
        currentMp = CacheEntity.CurrentMp;
        objectId = CacheEntity.ObjectId;
    }

    public bool ShouldRemove()
    {
        return CacheEntity == null || CacheEntity.CurrentHp <= 0;
    }

    public void Update()
    {
        if (CacheEntity == null)
            return;
        level = CacheEntity.Level;
        exp = CacheEntity.Exp;
        currentHp = CacheEntity.CurrentHp;
        currentMp = CacheEntity.CurrentMp;
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
            if (type == SummonType.Pet)
                writer.Put(exp);
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
            if (type == SummonType.Pet)
                exp = reader.GetInt();
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