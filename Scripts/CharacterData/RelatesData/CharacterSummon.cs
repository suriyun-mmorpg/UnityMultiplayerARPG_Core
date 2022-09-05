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
        Custom = byte.MaxValue
    }

    [System.Serializable]
    public class CharacterSummon : INetSerializable
    {
        public static readonly CharacterSummon Empty = new CharacterSummon();
        public string id;
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
        private short dirtyLevel;

        [System.NonSerialized]
        private BaseSkill cacheSkill;
        [System.NonSerialized]
        private BaseItem cachePetItem;
        [System.NonSerialized]
        private BaseMonsterCharacterEntity cachePrefab;
        [System.NonSerialized]
        private Buff cacheBuff;
        [System.NonSerialized]
        private float cacheDuration;
        [System.NonSerialized]
        private int cacheRecoveryHp;
        [System.NonSerialized]
        private int cacheRecoveryMp;
        [System.NonSerialized]
        private int cacheRecoveryStamina;
        [System.NonSerialized]
        private int cacheRecoveryFood;
        [System.NonSerialized]
        private int cacheRecoveryWater;
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
        private Dictionary<DamageElement, MinMaxFloat> cacheDamageOverTimes;

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
            if (dirtyType != type || dirtyDataId != dataId || dirtyLevel != level)
            {
                dirtyType = type;
                dirtyDataId = dataId;
                dirtyType = type;
                dirtyLevel = level;
                cacheSkill = null;
                cachePetItem = null;
                cachePrefab = null;
                cacheBuff = Buff.Empty;
                cacheDuration = 0;
                cacheRecoveryHp = 0;
                cacheRecoveryMp = 0;
                cacheRecoveryStamina = 0;
                cacheRecoveryFood = 0;
                cacheRecoveryWater = 0;
                cacheIncreaseStats = CharacterStats.Empty;
                cacheIncreaseStatsRate = CharacterStats.Empty;
                cacheIncreaseAttributes = null;
                cacheIncreaseAttributesRate = null;
                cacheIncreaseResistances = null;
                cacheIncreaseArmors = null;
                cacheIncreaseDamages = null;
                cacheDamageOverTimes = null;
                switch (type)
                {
                    case SummonType.Skill:
                        if (GameInstance.Skills.TryGetValue(dataId, out cacheSkill))
                            cachePrefab = cacheSkill.Summon.MonsterEntity;
                        break;
                    case SummonType.PetItem:
                        if (GameInstance.Items.TryGetValue(dataId, out cachePetItem) && cachePetItem is IPetItem)
                            cachePrefab = (cachePetItem as IPetItem).PetEntity;
                        break;
                    case SummonType.Custom:
                        cachePrefab = GameInstance.CustomSummonManager.GetPrefab(dataId);
                        break;
                }
                if (cachePrefab != null && cachePrefab.CharacterDatabase != null)
                {
                    MonsterCharacter database = cachePrefab.CharacterDatabase;
                    cacheBuff = database.SummonerBuff;
                    cacheDuration = cacheBuff.GetDuration(level);
                    cacheRecoveryHp = cacheBuff.GetRecoveryHp(level);
                    cacheRecoveryMp = cacheBuff.GetRecoveryMp(level);
                    cacheRecoveryStamina = cacheBuff.GetRecoveryStamina(level);
                    cacheRecoveryFood = cacheBuff.GetRecoveryFood(level);
                    cacheRecoveryWater = cacheBuff.GetRecoveryWater(level);
                    cacheIncreaseStats = cacheBuff.GetIncreaseStats(level);
                    cacheIncreaseStatsRate = cacheBuff.GetIncreaseStatsRate(level);
                    cacheIncreaseAttributes = cacheBuff.GetIncreaseAttributes(level);
                    cacheIncreaseAttributesRate = cacheBuff.GetIncreaseAttributesRate(level);
                    cacheIncreaseResistances = cacheBuff.GetIncreaseResistances(level);
                    cacheIncreaseArmors = cacheBuff.GetIncreaseArmors(level);
                    cacheIncreaseDamages = cacheBuff.GetIncreaseDamages(level);
                    cacheDamageOverTimes = cacheBuff.GetDamageOverTimes(level);
                }
            }
        }

        public void Summon(BaseCharacterEntity summoner, short summonLevel, float duration)
        {
            if (GetPrefab() == null)
                return;

            LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                GetPrefab().Identity.HashAssetId,
                GameInstance.Singleton.GameplayRule.GetSummonPosition(summoner),
                GameInstance.Singleton.GameplayRule.GetSummonRotation(summoner));
            cacheEntity = spawnObj.GetComponent<BaseMonsterCharacterEntity>();
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
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
                case SummonType.Custom:
                    GameInstance.CustomSummonManager.UnSummon(this);
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

        public BaseMonsterCharacterEntity GetPrefab()
        {
            MakeCache();
            return cachePrefab;
        }

        public Buff GetBuff()
        {
            MakeCache();
            return cacheBuff;
        }

        public float GetDuration()
        {
            MakeCache();
            return cacheDuration;
        }

        public int GetRecoveryHp()
        {
            MakeCache();
            return cacheRecoveryHp;
        }

        public int GetRecoveryMp()
        {
            MakeCache();
            return cacheRecoveryMp;
        }

        public int GetRecoveryStamina()
        {
            MakeCache();
            return cacheRecoveryStamina;
        }

        public int GetRecoveryFood()
        {
            MakeCache();
            return cacheRecoveryFood;
        }

        public int GetRecoveryWater()
        {
            MakeCache();
            return cacheRecoveryWater;
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

        public Dictionary<DamageElement, MinMaxFloat> GetDamageOverTimes()
        {
            MakeCache();
            return cacheDamageOverTimes;
        }

        public bool ShouldRemove()
        {
            return (CacheEntity && CacheEntity.CurrentHp <= 0) || (type == SummonType.Skill && summonRemainsDuration <= 0f);
        }

        public void Update(float deltaTime)
        {
            switch (type)
            {
                case SummonType.Skill:
                    // Update remains duration when it reached 0 it will be unsummoned
                    summonRemainsDuration -= deltaTime;
                    break;
            }
            // Makes update in main thread to collects data to use in other threads (save to database thread)
            level = Level;
            exp = Exp;
            currentHp = CurrentHp;
            currentMp = CurrentMp;
        }

        public CharacterSummon Clone(bool generateNewId = false)
        {
            return new CharacterSummon()
            {
                id = generateNewId ? GenericUtils.GetUniqueId() : id,
                type = type,
                dataId = dataId,
                summonRemainsDuration = summonRemainsDuration,
                objectId = objectId,
                level = level,
                exp = exp,
                currentHp = currentHp,
                currentMp = currentMp,
            };
        }

        public static CharacterSummon Create(SummonType type, int dataId)
        {
            return new CharacterSummon()
            {
                id = GenericUtils.GetUniqueId(),
                type = type,
                dataId = dataId,
            };
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
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
            id = reader.GetString();
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
