using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterSummon : INetSerializable
    {
        public uint ObjectId { get; internal set; }
        public int Level { get { return CacheEntity != null ? CacheEntity.Level : level; } }
        public int Exp { get { return CacheEntity != null ? CacheEntity.Exp : exp; } }
        public int CurrentHp { get { return CacheEntity != null ? CacheEntity.CurrentHp : currentHp; } }
        public int CurrentMp { get { return CacheEntity != null ? CacheEntity.CurrentMp : currentMp; } }

        [System.NonSerialized]
        private SummonType _dirtyType;
        [System.NonSerialized]
        private int _dirtyDataId;
        [System.NonSerialized]
        private int _dirtyLevel;

        [System.NonSerialized]
        private BaseSkill _cacheSkill;
        [System.NonSerialized]
        private BaseItem _cachePetItem;
        [System.NonSerialized]
        private BaseMonsterCharacterEntity _cachePrefab;
        [System.NonSerialized]
        private CalculatedBuff _cacheBuff = new CalculatedBuff();

        [System.NonSerialized]
        private BaseMonsterCharacterEntity _cacheEntity;
        public BaseMonsterCharacterEntity CacheEntity
        {
            get
            {
                if (_cacheEntity == null && ObjectId > 0)
                    BaseGameNetworkManager.Singleton.Assets.TryGetSpawnedObject(ObjectId, out _cacheEntity);
                return _cacheEntity;
            }
        }

        private void MakeCache()
        {
            if (_dirtyType == type && _dirtyDataId == dataId && _dirtyLevel == level)
                return;
            _dirtyType = type;
            _dirtyDataId = dataId;
            _dirtyLevel = level;
            _cacheSkill = null;
            _cachePetItem = null;
            _cachePrefab = null;
            Buff tempBuff = Buff.Empty;
            switch (type)
            {
                case SummonType.Skill:
                    if (GameInstance.Skills.TryGetValue(dataId, out _cacheSkill))
                        _cachePrefab = _cacheSkill.Summon.MonsterEntity;
                    break;
                case SummonType.PetItem:
                    if (GameInstance.Items.TryGetValue(dataId, out _cachePetItem) && _cachePetItem is IPetItem)
                        _cachePrefab = (_cachePetItem as IPetItem).PetEntity;
                    break;
                case SummonType.Custom:
                    _cachePrefab = GameInstance.CustomSummonManager.GetPrefab(dataId);
                    break;
            }
            if (_cachePrefab != null && _cachePrefab.CharacterDatabase != null)
                tempBuff = _cachePrefab.CharacterDatabase.SummonerBuff;
            _cacheBuff.Build(tempBuff, level);
        }

        public void Summon(BaseCharacterEntity summoner, int summonLevel, float duration)
        {
            if (GetPrefab() == null)
                return;

            LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                GetPrefab().Identity.HashAssetId,
                GameInstance.Singleton.GameplayRule.GetSummonPosition(summoner),
                GameInstance.Singleton.GameplayRule.GetSummonRotation(summoner));
            _cacheEntity = spawnObj.GetComponent<BaseMonsterCharacterEntity>();
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            CacheEntity.Summon(summoner, type, summonLevel);
            ObjectId = CacheEntity.ObjectId;
            summonRemainsDuration = duration;
            level = summonLevel;
        }

        public void Summon(BaseCharacterEntity summoner, int summonLevel, float duration, int summonExp)
        {
            Summon(summoner, summonLevel, duration);
            CacheEntity.Exp = summonExp;
            exp = summonExp;
        }

        public void Summon(BaseCharacterEntity summoner, int summonLevel, float duration, int summonExp, int summonCurrentHp, int summonCurrentMp)
        {
            Summon(summoner, summonLevel, duration, summonExp);
            CacheEntity.CurrentHp = summonCurrentHp;
            CacheEntity.CurrentMp = summonCurrentMp;
            currentHp = summonCurrentHp;
            currentMp = summonCurrentMp;

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
            return _cacheSkill;
        }

        public BaseItem GetPetItem()
        {
            MakeCache();
            return _cachePetItem;
        }

        public BaseMonsterCharacterEntity GetPrefab()
        {
            MakeCache();
            return _cachePrefab;
        }

        public CalculatedBuff GetBuff()
        {
            MakeCache();
            return _cacheBuff;
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
                ObjectId = ObjectId,
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
                writer.PutPackedUInt(ObjectId);
                writer.PutPackedInt(level);
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
                ObjectId = reader.GetPackedUInt();
                level = reader.GetPackedInt();
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
            result.ObjectId = reader.GetPackedUInt();
            return result;
        }

        protected override void SerializeValueForSetOrDirty(int index, NetDataWriter writer, CharacterSummon value)
        {
            writer.Put(value.summonRemainsDuration);
            writer.PutPackedUInt(value.ObjectId);
        }
    }
}
