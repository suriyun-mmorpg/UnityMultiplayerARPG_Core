namespace MultiplayerARPG
{
    public class CharacterSummonCacheManager : BaseCacheManager<CharacterSummon, CharacterSummonCacheData>
    {
        public BaseMonsterCharacterEntity GetEntity(ref CharacterSummon data)
        {
            return GetOrMakeCache(data.id, ref data).CacheEntity;
        }

        public void SetEntity(ref CharacterSummon data, BaseMonsterCharacterEntity value)
        {
            GetOrMakeCache(data.id, ref data).CacheEntity = value;
        }

        public BaseSkill GetSkill(ref CharacterSummon data)
        {
            return GetOrMakeCache(data.id, ref data).GetSkill();
        }

        public IPetItem GetPetItem(ref CharacterSummon data)
        {
            return GetOrMakeCache(data.id, ref data).GetPetItem();
        }

        public BaseMonsterCharacterEntity GetPrefab(ref CharacterSummon data)
        {
            return GetOrMakeCache(data.id, ref data).GetPrefab();
        }

        public CalculatedBuff GetBuff(ref CharacterSummon data)
        {
            return GetOrMakeCache(data.id, ref data).GetBuff();
        }
    }
}