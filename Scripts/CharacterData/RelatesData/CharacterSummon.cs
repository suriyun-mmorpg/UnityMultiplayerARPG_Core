using UnityEngine;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public enum SummonType : byte
    {
        None,
        Skill,
        PetItem,
    }

    [System.Serializable]
    public class CharacterSummon : INetSerializable
    {
        public static readonly CharacterSummon Empty = new CharacterSummon();
        public SummonType type;
        public int dataId;
        public float summonRemainsDuration;
        public uint objectId;
        // For save / load
        public short level;
        public short Level { get { return CacheEntity != null ? CacheEntity.Level : level; } }
        public int exp;
        public int Exp { get { return CacheEntity != null ? CacheEntity.Exp : exp; } }
        public int currentHp;
        public int CurrentHp { get { return CacheEntity != null ? CacheEntity.CurrentHp : currentHp; } }
        public int currentMp;
        public int CurrentMp { get { return CacheEntity != null ? CacheEntity.CurrentMp : currentMp; } }

        [System.NonSerialized]
        private int dirtyDataId;

        [System.NonSerialized]
        private BaseSkill cacheSkill;
        [System.NonSerialized]
        private BaseItem cachePetItem;
        [System.NonSerialized]
        private BaseMonsterCharacterEntity cachePrefab;

        [System.NonSerialized]
        private BaseMonsterCharacterEntity cacheEntity;
        public BaseMonsterCharacterEntity CacheEntity
        {
            get
            {
                if (!cacheEntity && BaseGameNetworkManager.Singleton)
                    BaseGameNetworkManager.Singleton.Assets.TryGetSpawnedObject(objectId, out cacheEntity);
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
                            cachePrefab = cacheSkill.GetSummon().MonsterEntity;
                        break;
                    case SummonType.PetItem:
                        if (GameInstance.Items.TryGetValue(dataId, out cachePetItem) && cachePetItem is IPetItem)
                            cachePrefab = (cachePetItem as IPetItem).PetEntity;
                        break;
                }
            }
        }

        public BaseSkill GetSkill()
        {
            MakeCache();
            return cacheSkill;
        }

        public BaseItem GetPetItem()
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
            cacheEntity = BaseGameNetworkManager.Singleton.Assets
                .NetworkSpawn(Object.Instantiate(GetPrefab().gameObject, GameInstance.Singleton.GameplayRule.GetSummonPosition(summoner), GameInstance.Singleton.GameplayRule.GetSummonRotation(summoner)))
                .GetComponent<BaseMonsterCharacterEntity>();
            CacheEntity.Summon(summoner, type, summonLevel);
            objectId = CacheEntity.ObjectId;
            summonRemainsDuration = duration;
        }

        public void Summon(BaseCharacterEntity summoner, short summonLevel, float duration, int summonExp)
        {
            Summon(summoner, summonLevel, duration);
            CacheEntity.Exp = summonExp;
        }

        public void Summon(BaseCharacterEntity summoner, short summonLevel, float duration, int summonExp, int summonCurrentHp, int summonCurrentMp)
        {
            Summon(summoner, summonLevel, duration);
            CacheEntity.Exp = summonExp;
            CacheEntity.CurrentHp = summonCurrentHp;
            CacheEntity.CurrentMp = summonCurrentMp;
        }

        public void UnSummon(BaseCharacterEntity summoner)
        {
            if (type == SummonType.PetItem)
            {
                CharacterItem newItem = CharacterItem.Create(dataId, Level, 1);
                newItem.exp = Exp;
                if (CacheEntity && CacheEntity.CurrentHp <= 0)
                    newItem.Lock(GameInstance.Singleton.petDeadLockDuration);
                else
                    newItem.Lock(GameInstance.Singleton.petUnSummonLockDuration);
                summoner.AddOrSetNonEquipItems(newItem);
            }

            if (CacheEntity)
                CacheEntity.UnSummon();
        }

        public bool ShouldRemove()
        {
            return (CacheEntity && CacheEntity.CurrentHp <= 0) || (type == SummonType.Skill && summonRemainsDuration <= 0f);
        }

        public void Update(float deltaTime)
        {
            if (type == SummonType.Skill)
                summonRemainsDuration -= deltaTime;
            // Makes update in main thread to collects data to use in other threads (save to database thread)
            level = Level;
            exp = Exp;
            currentHp = CurrentHp;
            currentMp = CurrentMp;
        }

        public static CharacterSummon Create(SummonType type, int dataId)
        {
            CharacterSummon newSummon = new CharacterSummon();
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
                switch (type)
                {
                    case SummonType.Skill:
                        writer.Put(summonRemainsDuration);
                        break;
                }
                writer.PutPackedUInt(objectId);
                writer.PutPackedShort(level);
                writer.PutPackedInt(exp);
                writer.PutPackedInt(currentHp);
                writer.PutPackedInt(currentMp);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            type = (SummonType)reader.GetByte();
            if (type != SummonType.None)
            {
                dataId = reader.GetInt();
                switch (type)
                {
                    case SummonType.Skill:
                        summonRemainsDuration = reader.GetFloat();
                        break;
                }
                objectId = reader.GetPackedUInt();
                level = reader.GetPackedShort();
                exp = reader.GetPackedInt();
                currentHp = reader.GetPackedInt();
                currentMp = reader.GetPackedInt();
            }
        }
    }

    [System.Serializable]
    public class SyncListCharacterSummon : LiteNetLibSyncList<CharacterSummon>
    {
        protected override CharacterSummon DeserializeValueForSetOrDirty(int index, NetDataReader reader)
        {
            CharacterSummon result = this[index];
            result.summonRemainsDuration = reader.GetFloat();
            result.objectId = reader.GetPackedUInt();
            return result;
        }

        protected override void SerializeValueForSetOrDirty(int index, NetDataWriter writer, CharacterSummon value)
        {
            writer.Put(value.summonRemainsDuration);
            writer.PutPackedUInt(value.objectId);
        }
    }
}
