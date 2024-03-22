namespace MultiplayerARPG
{
    public class CharacterItemCacheManager : BaseCacheManager<CharacterItem, CharacterItemCacheData>
    {
        public BaseItem GetItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetItem();
        }

        public IUsableItem GetUsableItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetUsableItem();
        }

        public IEquipmentItem GetEquipmentItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetEquipmentItem();
        }

        public IDefendEquipmentItem GetDefendItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetDefendItem();
        }

        public IArmorItem GetArmorItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetArmorItem();
        }

        public IWeaponItem GetWeaponItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetWeaponItem();
        }

        public IShieldItem GetShieldItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetShieldItem();
        }

        public IPotionItem GetPotionItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetPotionItem();
        }

        public IAmmoItem GetAmmoItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetAmmoItem();
        }

        public IBuildingItem GetBuildingItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetBuildingItem();
        }

        public IPetItem GetPetItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetPetItem();
        }

        public ISocketEnhancerItem GetSocketEnhancerItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetSocketEnhancerItem();
        }

        public IMountItem GetMountItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetMountItem();
        }

        public ISkillItem GetSkillItem(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetSkillItem();
        }

        public CalculatedItemBuff GetBuff(ref CharacterItem data)
        {
            return GetOrMakeCache(data.id, ref data).GetBuff();
        }
    }
}