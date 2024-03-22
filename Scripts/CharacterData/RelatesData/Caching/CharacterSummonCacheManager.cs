namespace MultiplayerARPG
{
    public class CharacterSummonCacheManager : BaseCacheManager<CharacterSummon, CharacterSummonCacheData>
    {
        public BaseMonsterCharacterEntity GetEntity(CharacterSummon data)
        {
            return GetOrMakeCache(data.id, data).CacheEntity;
        }

        public void SetEntity(CharacterSummon data, BaseMonsterCharacterEntity value)
        {
            GetOrMakeCache(data.id, data).CacheEntity = value;
        }

        public BaseSkill GetSkill(CharacterSummon data)
        {
            return GetOrMakeCache(data.id, data).GetSkill();
        }

        public IPetItem GetPetItem(CharacterSummon data)
        {
            return GetOrMakeCache(data.id, data).GetPetItem();
        }

        public BaseMonsterCharacterEntity GetPrefab(CharacterSummon data)
        {
            return GetOrMakeCache(data.id, data).GetPrefab();
        }

        public CalculatedBuff GetBuff(CharacterSummon data)
        {
            return GetOrMakeCache(data.id, data).GetBuff();
        }
    }
}