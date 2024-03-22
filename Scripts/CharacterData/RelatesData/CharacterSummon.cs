using LiteNetLib.Utils;
using LiteNetLibManager;
using Newtonsoft.Json;

namespace MultiplayerARPG
{
    public partial struct CharacterSummon
    {
        [JsonIgnore]
        public BaseMonsterCharacterEntity CacheEntity
        {
            set => CharacterRelatesDataCacheManager.CharacterSummons.SetEntity(this, value);
            get => CharacterRelatesDataCacheManager.CharacterSummons.GetEntity(this);
        }
        [JsonIgnore]
        public int Level { get { return CacheEntity != null ? CacheEntity.Level : level; } }
        [JsonIgnore]
        public int Exp { get { return CacheEntity != null ? CacheEntity.Exp : exp; } }
        [JsonIgnore]
        public int CurrentHp { get { return CacheEntity != null ? CacheEntity.CurrentHp : currentHp; } }
        [JsonIgnore]
        public int CurrentMp { get { return CacheEntity != null ? CacheEntity.CurrentMp : currentMp; } }

        public void Summon(BaseCharacterEntity summoner, int summonLevel, float duration)
        {
            if (GetPrefab() == null)
                return;

            LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                GetPrefab().Identity.HashAssetId,
                GameInstance.Singleton.GameplayRule.GetSummonPosition(summoner),
                GameInstance.Singleton.GameplayRule.GetSummonRotation(summoner));
            CacheEntity = spawnObj.GetComponent<BaseMonsterCharacterEntity>();
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
            return CharacterRelatesDataCacheManager.CharacterSummons.GetSkill(this);
        }

        public IPetItem GetPetItem()
        {
            return CharacterRelatesDataCacheManager.CharacterSummons.GetPetItem(this);
        }

        public BaseMonsterCharacterEntity GetPrefab()
        {
            return CharacterRelatesDataCacheManager.CharacterSummons.GetPrefab(this);
        }

        public CalculatedBuff GetBuff()
        {
            return CharacterRelatesDataCacheManager.CharacterSummons.GetBuff(this);
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
