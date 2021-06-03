using UnityEngine;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public enum SummonType : byte
    {
        None,
        Skill,
        PetItem,
        Companion
    }

    [System.Serializable]
    public class CharacterSummon : INetSerializable
    {
        public static readonly CharacterSummon Empty = new CharacterSummon();
        public SummonType type;
        public int dataId;
        public float summonRemainsDuration;
        public uint objectId;
        public short level;
        public int exp;
        public int currentHp;
        public int currentMp;
        // Properties for save / load
        public short Level { get { return CacheEntity != null ? CacheEntity.Level : level; } }
        public int Exp { get { return CacheEntity != null ? CacheEntity.Exp : exp; } }
        public int CurrentHp { get { return CacheEntity != null ? CacheEntity.CurrentHp : currentHp; } }
        public int CurrentMp { get { return CacheEntity != null ? CacheEntity.CurrentMp : currentMp; } }

        [System.NonSerialized]
        private SummonType dirtyType;
        [System.NonSerialized]
        private int dirtyDataId;

        [System.NonSerialized]
        private BaseSkill cacheSkill;
        [System.NonSerialized]
        private BaseItem cachePetItem;
        [System.NonSerialized]
        private Companion cacheCompanion;
        [System.NonSerialized]
        private BaseMonsterCharacterEntity cachePrefab;
        [System.NonSerialized]
        private Buff cacheSummonerBuff;
        [System.NonSerialized]
        private CharacterStats cacheIncreaseStats;
        [System.NonSerialized]
        private CharacterStats cacheIncreaseStatsRate;
        [System.NonSerialized]
        private Dictionary<Attribute, float> cacheIncreaseAttributes;
        [System.NonSerialized]
        private Dictionary<Attribute, float> cacheIncreaseAttributesRate;
        [System.NonSerialized]
        private Dictionary<DamageElement, float> cacheIncreaseResistances;
        [System.NonSerialized]
        private Dictionary<DamageElement, float> cacheIncreaseArmors;
        [System.NonSerialized]
        private Dictionary<DamageElement, MinMaxFloat> cacheIncreaseDamages;

        [System.NonSerialized]
        private BaseMonsterCharacterEntity cacheEntity;
        public BaseMonsterCharacterEntity CacheEntity
        {
            get
            {
                if (cacheEntity == null && objectId > 0)
                    BaseGameNetworkManager.Singleton.Assets.TryGetSpawnedObject(objectId, out cacheEntity);
                return cacheEntity;
            }
        }

        private void MakeCache()
        {
            if (dirtyType != type || dirtyDataId != dataId)
            {
                dirtyType = type;
                dirtyDataId = dataId;
                cacheSkill = null;
                cachePetItem = null;
                cachePrefab = null;
                cacheSummonerBuff = default(Buff);
                cacheIncreaseStats = new CharacterStats();
                cacheIncreaseStatsRate = new CharacterStats();
                cacheIncreaseAttributes = null;
                cacheIncreaseAttributesRate = null;
                cacheIncreaseResistances = null;
                cacheIncreaseArmors = null;
                cacheIncreaseDamages = null;
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
                    case SummonType.Companion:
                        if (GameInstance.Companions.TryGetValue(dataId, out cacheCompanion))
                            cachePrefab = cacheCompanion.CompanionEntity;
                        break;
                }
                if (cachePrefab != null && cachePrefab.CharacterDatabase != null)
                {
                    MonsterCharacter database = cachePrefab.CharacterDatabase;
                    cacheSummonerBuff = database.SummonerBuff;
                    cacheIncreaseStats = cacheSummonerBuff.GetIncreaseStats(level);
                    cacheIncreaseStatsRate = cacheSummonerBuff.GetIncreaseStatsRate(level);
                    cacheIncreaseAttributes = cacheSummonerBuff.GetIncreaseAttributes(level);
                    cacheIncreaseAttributesRate = cacheSummonerBuff.GetIncreaseAttributesRate(level);
                    cacheIncreaseResistances = cacheSummonerBuff.GetIncreaseResistances(level);
                    cacheIncreaseArmors = cacheSummonerBuff.GetIncreaseArmors(level);
                    cacheIncreaseDamages = cacheSummonerBuff.GetIncreaseDamages(level);
                }
            }
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
            switch (type)
            {
                case SummonType.PetItem:
                    // Return to character as a pet item
                    CharacterItem newItem = CharacterItem.Create(dataId, Level, 1);
                    newItem.exp = Exp;
                    newItem.Lock(CurrentHp <= 0 ?
                        GameInstance.Singleton.petDeadLockDuration :
                        GameInstance.Singleton.petUnSummonLockDuration);
                    summoner.AddOrSetNonEquipItems(newItem);
                    break;
                case SummonType.Companion:
                    // Update companion data
                    if (summoner is BasePlayerCharacterEntity)
                    {
                        (summoner as BasePlayerCharacterEntity).UnSummonCompanion(Level, Exp, CurrentHp <= 0 ?
                            GameInstance.Singleton.companionDeadLockDuration :
                            GameInstance.Singleton.companionUnSummonLockDuration);
                    }
                    break;
            }

            if (CacheEntity)
                CacheEntity.UnSummon();
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

        public Companion GetCompanion()
        {
            MakeCache();
            return cacheCompanion;
        }

        public BaseMonsterCharacterEntity GetPrefab()
        {
            MakeCache();
            return cachePrefab;
        }

        public CharacterStats GetIncreaseStats()
        {
            MakeCache();
            return cacheIncreaseStats;
        }

        public CharacterStats GetIncreaseStatsRate()
        {
            MakeCache();
            return cacheIncreaseStatsRate;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributes()
        {
            MakeCache();
            return cacheIncreaseAttributes;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributesRate()
        {
            MakeCache();
            return cacheIncreaseAttributesRate;
        }

        public Dictionary<DamageElement, float> GetIncreaseResistances()
        {
            MakeCache();
            return cacheIncreaseResistances;
        }

        public Dictionary<DamageElement, float> GetIncreaseArmors()
        {
            MakeCache();
            return cacheIncreaseArmors;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages()
        {
            MakeCache();
            return cacheIncreaseDamages;
        }

        public bool ShouldRemove()
        {
            return (CacheEntity && CacheEntity.CurrentHp <= 0) || (type == SummonType.Skill && summonRemainsDuration <= 0f);
        }

        public void Update(BaseCharacterEntity entity, float deltaTime)
        {
            switch (type)
            {
                case SummonType.Skill:
                    // Update remains duration when it reached 0 it will be unsummoned
                    summonRemainsDuration -= deltaTime;
                    break;
                case SummonType.Companion:
                    // Update companion data
                    if (entity is BasePlayerCharacterEntity)
                    {
                        (entity as BasePlayerCharacterEntity).UpdateCompanion(Level, Exp);
                    }
                    break;
            }
            // Makes update in main thread to collects data to use in other threads (save to database thread)
            level = Level;
            exp = Exp;
            currentHp = CurrentHp;
            currentMp = CurrentMp;
        }

        public static CharacterSummon Create(SummonType type, int dataId)
        {
            return new CharacterSummon()
            {
                type = type,
                dataId = dataId,
            };
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            if (type != SummonType.None)
            {
                writer.PutPackedInt(dataId);
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
                dataId = reader.GetPackedInt();
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
