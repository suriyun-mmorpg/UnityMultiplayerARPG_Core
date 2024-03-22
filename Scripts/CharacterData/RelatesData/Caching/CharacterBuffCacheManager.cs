namespace MultiplayerARPG
{
    public class CharacterBuffCacheManager : BaseCacheManager<CharacterBuff, CharacterBuffCacheData>
    {
        public BaseSkill GetSkill(ref CharacterBuff data)
        {
            return GetOrMakeCache(data.id, ref data).GetSkill();
        }

        public BaseItem GetItem(ref CharacterBuff data)
        {
            return GetOrMakeCache(data.id, ref data).GetItem();
        }

        public GuildSkill GetGuildSkill(ref CharacterBuff data)
        {
            return GetOrMakeCache(data.id, ref data).GetGuildSkill();
        }

        public StatusEffect GetStatusEffect(ref CharacterBuff data)
        {
            return GetOrMakeCache(data.id, ref data).GetStatusEffect();
        }

        public CalculatedBuff GetBuff(ref CharacterBuff data)
        {
            return GetOrMakeCache(data.id, ref data).GetBuff();
        }

        public string GetKey(ref CharacterBuff data)
        {
            return GetOrMakeCache(data.id, ref data).GetKey();
        }

        public void SetApplier(ref CharacterBuff data, EntityInfo buffApplier, CharacterItem buffApplierWeapon)
        {
            GetOrMakeCache(data.id, ref data).SetApplier(buffApplier, buffApplierWeapon);
        }

        public EntityInfo GetBuffApplier(ref CharacterBuff data)
        {
            return GetOrMakeCache(data.id, ref data).BuffApplier;
        }

        public CharacterItem GetBuffApplierWeapon(ref CharacterBuff data)
        {
            return GetOrMakeCache(data.id, ref data).BuffApplierWeapon;
        }
    }
}
