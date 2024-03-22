namespace MultiplayerARPG
{
    public class CharacterBuffCacheManager : BaseCacheManager<CharacterBuff, CharacterBuffCacheData>
    {
        public BaseSkill GetSkill(CharacterBuff data)
        {
            return GetOrMakeCache(data.id, data).GetSkill();
        }

        public BaseItem GetItem(CharacterBuff data)
        {
            return GetOrMakeCache(data.id, data).GetItem();
        }

        public GuildSkill GetGuildSkill(CharacterBuff data)
        {
            return GetOrMakeCache(data.id, data).GetGuildSkill();
        }

        public StatusEffect GetStatusEffect(CharacterBuff data)
        {
            return GetOrMakeCache(data.id, data).GetStatusEffect();
        }

        public CalculatedBuff GetBuff(CharacterBuff data)
        {
            return GetOrMakeCache(data.id, data).GetBuff();
        }

        public string GetKey(CharacterBuff data)
        {
            return GetOrMakeCache(data.id, data).GetKey();
        }

        public void SetApplier(CharacterBuff data, EntityInfo buffApplier, CharacterItem buffApplierWeapon)
        {
            GetOrMakeCache(data.id, data).SetApplier(buffApplier, buffApplierWeapon);
        }

        public EntityInfo GetBuffApplier(CharacterBuff data)
        {
            return GetOrMakeCache(data.id, data).BuffApplier;
        }

        public CharacterItem GetBuffApplierWeapon(CharacterBuff data)
        {
            return GetOrMakeCache(data.id, data).BuffApplierWeapon;
        }
    }
}
