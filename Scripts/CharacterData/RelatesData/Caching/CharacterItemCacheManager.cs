namespace MultiplayerARPG
{
    public class CharacterItemCacheManager : BaseCacheManager<CharacterItem, CharacterItemCacheData>
    {
        public BaseItem GetItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetItem();
        }

        public IUsableItem GetUsableItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetUsableItem();
        }

        public IEquipmentItem GetEquipmentItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetEquipmentItem();
        }

        public IDefendEquipmentItem GetDefendItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetDefendItem();
        }

        public IArmorItem GetArmorItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetArmorItem();
        }

        public IWeaponItem GetWeaponItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetWeaponItem();
        }

        public IShieldItem GetShieldItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetShieldItem();
        }

        public IPotionItem GetPotionItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetPotionItem();
        }

        public IAmmoItem GetAmmoItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetAmmoItem();
        }

        public IBuildingItem GetBuildingItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetBuildingItem();
        }

        public IPetItem GetPetItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetPetItem();
        }

        public ISocketEnhancerItem GetSocketEnhancerItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetSocketEnhancerItem();
        }

        public IMountItem GetMountItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetMountItem();
        }

        public ISkillItem GetSkillItem(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetSkillItem();
        }

        public CalculatedItemBuff GetBuff(CharacterItem data)
        {
            return GetOrMakeCache(data.id, data).GetBuff();
        }
    }
}