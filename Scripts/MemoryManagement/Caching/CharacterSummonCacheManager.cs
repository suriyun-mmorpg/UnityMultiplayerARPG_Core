namespace MultiplayerARPG
{
    public class CharacterSummonCacheManager : BaseCacheManager<CharacterSummon, CharacterSummonCacheData>
    {
        public BaseMonsterCharacterEntity GetEntity(in CharacterSummon data)
        {
            return GetOrMakeCache(data.id, in data)?.CacheEntity;
        }

        public void SetEntity(in CharacterSummon data, BaseMonsterCharacterEntity value)
        {
            CharacterSummonCacheData cachedData = GetOrMakeCache(data.id, in data);
            if (cachedData != null)
                cachedData.CacheEntity = value;
        }

        public BaseSkill GetSkill(in CharacterSummon data)
        {
            return GetOrMakeCache(data.id, in data)?.GetSkill();
        }

        public IPetItem GetPetItem(in CharacterSummon data)
        {
            return GetOrMakeCache(data.id, in data)?.GetPetItem();
        }

        public BaseMonsterCharacterEntity GetPrefab(in CharacterSummon data)
        {
            return GetOrMakeCache(data.id, in data)?.GetPrefab();
        }

        public CalculatedBuff GetBuff(in CharacterSummon data)
        {
            return GetOrMakeCache(data.id, in data)?.GetBuff();
        }
    }
}