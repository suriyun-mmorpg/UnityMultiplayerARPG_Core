using LiteNetLib.Utils;
using LiteNetLibManager;
using Newtonsoft.Json;

namespace MultiplayerARPG
{
    public partial class CharacterSummon
    {
        [System.NonSerialized]
        private SummonType _dirtyType;
        [System.NonSerialized]
        private int _dirtyDataId;
        [System.NonSerialized]
        private int _dirtyLevel;

        [System.NonSerialized]
        private BaseSkill _cacheSkill;
        [System.NonSerialized]
        private IPetItem _cachePetItem;
        [System.NonSerialized]
        private BaseMonsterCharacterEntity _cachePrefab;
        [System.NonSerialized]
        private CalculatedBuff _cacheBuff = new CalculatedBuff();
        [System.NonSerialized]
        private bool _recachingBuff = false;

        [System.NonSerialized]
        private BaseMonsterCharacterEntity _cacheEntity;
        [JsonIgnore]
        public BaseMonsterCharacterEntity CacheEntity
        {
            get
            {
                if (_cacheEntity == null && objectId > 0)
                    BaseGameNetworkManager.Singleton.Assets.TryGetSpawnedObject(objectId, out _cacheEntity);
                return _cacheEntity;
            }
        }
        [JsonIgnore]
        public int Level { get { return CacheEntity != null ? CacheEntity.Level : level; } }
        [JsonIgnore]
        public int Exp { get { return CacheEntity != null ? CacheEntity.Exp : exp; } }
        [JsonIgnore]
        public int CurrentHp { get { return CacheEntity != null ? CacheEntity.CurrentHp : currentHp; } }
        [JsonIgnore]
        public int CurrentMp { get { return CacheEntity != null ? CacheEntity.CurrentMp : currentMp; } }

        ~CharacterSummon()
        {
            ClearCachedData();
            _cacheBuff = null;
            _cacheEntity = null;
        }

        private void ClearCachedData()
        {
            _cacheSkill = null;
            _cachePetItem = null;
            _cachePrefab = null;
        }

        private bool IsRecaching()
        {
            return _dirtyType != type || _dirtyDataId != dataId || _dirtyLevel != level;
        }

        private void MakeAsCached()
        {
            _dirtyType = type;
            _dirtyDataId = dataId;
            _dirtyLevel = level;
        }

        private void MakeCache()
        {
            if (!IsRecaching())
                return;
            MakeAsCached();
            ClearCachedData();
            _recachingBuff = true;
            switch (type)
            {
                case SummonType.Skill:
                    if (!GameInstance.Skills.TryGetValue(dataId, out _cacheSkill) || !_cacheSkill.TryGetSummon(out SkillSummon skillSummon))
                    {
                        _cacheSkill = null;
                        _cachePrefab = null;
                    }
                    else
                    {
                        _cachePrefab = skillSummon.MonsterEntity;
                    }
                    break;
                case SummonType.PetItem:
                    if (!GameInstance.Items.TryGetValue(dataId, out BaseItem item) || !item.IsPet())
                    {
                        _cachePetItem = null;
                        _cachePrefab = null;
                    }
                    else
                    {
                        _cachePetItem = item as IPetItem;
                        _cachePrefab = _cachePetItem.MonsterCharacterEntity;
                    }
                    break;
                case SummonType.Custom:
                    _cachePrefab = GameInstance.CustomSummonManager.GetPrefab(dataId);
                    break;
            }
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
            objectId = CacheEntity.ObjectId;
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

        public IPetItem GetPetItem()
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
            if (_recachingBuff)
            {
                _recachingBuff = false;
                Buff tempBuff = Buff.Empty;
                if (_cachePrefab != null && _cachePrefab.CharacterDatabase != null)
                    tempBuff = _cachePrefab.CharacterDatabase.SummonerBuff;
                _cacheBuff.Build(tempBuff, level);
            }
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
