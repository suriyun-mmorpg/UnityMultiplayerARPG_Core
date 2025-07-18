using Insthync.AddressableAssetTools;

namespace MultiplayerARPG
{
    public static class AssetReferenceExtensions
    {
        /// <summary>
        /// Return `TRUE` if it is addressable
        /// </summary>
        /// <param name="summonType"></param>
        /// <param name="dataId"></param>
        /// <param name="prefab"></param>
        /// <param name="addressablePrefab"></param>
        /// <returns></returns>
        public static bool GetPrefab(this SummonType summonType, int dataId, out BaseMonsterCharacterEntity prefab, out AssetReferenceBaseMonsterCharacterEntity addressablePrefab)
        {
            prefab = null;
            addressablePrefab = null;
            switch (summonType)
            {
                case SummonType.Skill:
                    if (GameInstance.Skills.TryGetValue(dataId, out BaseSkill skill) &&
                        skill.TryGetSummon(out SkillSummon skillSummon))
                    {
                        if (skillSummon.MonsterCharacterEntity != null)
                        {
                            prefab = skillSummon.MonsterCharacterEntity;
                            return false;
                        }
                        else if (skillSummon.AddressableMonsterCharacterEntity.IsDataValid())
                        {
                            addressablePrefab = skillSummon.AddressableMonsterCharacterEntity;
                            return true;
                        }
                    }
                    break;
                case SummonType.PetItem:
                    if (GameInstance.Items.TryGetValue(dataId, out BaseItem item) &&
                        item.IsPet() && item is IPetItem petItem)
                    {
                        if (petItem.MonsterCharacterEntity != null)
                        {
                            prefab = petItem.MonsterCharacterEntity;
                            return false;
                        }
                        else if (petItem.AddressableMonsterCharacterEntity.IsDataValid())
                        {
                            addressablePrefab = petItem.AddressableMonsterCharacterEntity;
                            return true;
                        }
                    }
                    break;
                case SummonType.Custom:
                    return GameInstance.CustomSummonManager.GetPrefab(out prefab, out addressablePrefab);
            }
            return false;
        }

        public static int GetPrefabEntityId(this SummonType summonType, int dataId)
        {
            BaseMonsterCharacterEntity prefab = null;
            AssetReferenceBaseMonsterCharacterEntity addressablePrefab = null;
            switch (summonType)
            {
                case SummonType.Skill:
                    if (GameInstance.Skills.TryGetValue(dataId, out BaseSkill skill) &&
                        skill.TryGetSummon(out SkillSummon skillSummon))
                    {
                        if (skillSummon.MonsterCharacterEntity != null)
                        {
                            prefab = skillSummon.MonsterCharacterEntity;
                            return prefab.HashAssetId;
                        }
                        else if (skillSummon.AddressableMonsterCharacterEntity.IsDataValid())
                        {
                            addressablePrefab = skillSummon.AddressableMonsterCharacterEntity;
                            return addressablePrefab.HashAssetId;
                        }
                    }
                    break;
                case SummonType.PetItem:
                    if (GameInstance.Items.TryGetValue(dataId, out BaseItem item) &&
                        item.IsPet() && item is IPetItem petItem)
                    {
                        if (petItem.MonsterCharacterEntity != null)
                        {
                            prefab = petItem.MonsterCharacterEntity;
                            return prefab.HashAssetId;
                        }
                        else if (petItem.AddressableMonsterCharacterEntity.IsDataValid())
                        {
                            addressablePrefab = petItem.AddressableMonsterCharacterEntity;
                            return addressablePrefab.HashAssetId;
                        }
                    }
                    break;
                case SummonType.Custom:
                    if (GameInstance.CustomSummonManager.GetPrefab(out prefab, out addressablePrefab))
                    {
                        return addressablePrefab.HashAssetId;
                    }
                    else
                    {
                        return prefab.HashAssetId;
                    }
            }
            return 0;
        }

        public static bool GetPrefab(this MountType mountType, ICharacterData characterData, string sourceId,
            out VehicleEntity prefab, out AssetReferenceVehicleEntity addressablePrefab)
        {
            prefab = null;
            addressablePrefab = null;
            int tempIndexOfData;
            BaseItem tempItem;
            BaseSkill tempSkill;
            CalculatedBuff tempCalculatedBuff;
            BuffMount tempBuffMount;
            switch (mountType)
            {
                case MountType.Skill:
                    if (GameInstance.Skills.TryGetValue(BaseGameData.MakeDataId(sourceId), out tempSkill) &&
                        tempSkill.TryGetMount(out SkillMount skillMount))
                    {
                        if (skillMount.MountEntity != null)
                        {
                            prefab = skillMount.MountEntity;
                            return false;
                        }
                        else if (skillMount.AddressableMountEntity.IsDataValid())
                        {
                            addressablePrefab = skillMount.AddressableMountEntity;
                            return true;
                        }
                    }
                    break;
                case MountType.MountItem:
                    tempIndexOfData = characterData.IndexOfNonEquipItem(sourceId);
                    if (tempIndexOfData < 0)
                        return false;
                    tempItem = characterData.NonEquipItems[tempIndexOfData].GetItem();
                    if (tempItem.IsMount() && tempItem is IMountItem mountItem)
                    {
                        if (mountItem.VehicleEntity != null)
                        {
                            prefab = mountItem.VehicleEntity;
                            return false;
                        }
                        else if (mountItem.AddressableVehicleEntity.IsDataValid())
                        {
                            addressablePrefab = mountItem.AddressableVehicleEntity;
                            return true;
                        }
                    }
                    break;
                case MountType.Buff:
                    tempIndexOfData = characterData.IndexOfBuff(sourceId);
                    if (tempIndexOfData < 0)
                        return false;
                    tempCalculatedBuff = characterData.Buffs[tempIndexOfData].GetBuff();
                    if (tempCalculatedBuff != null && tempCalculatedBuff.TryGetMount(out tempBuffMount))
                    {
                        if (tempBuffMount.MountEntity != null)
                        {
                            prefab = tempBuffMount.MountEntity;
                            return false;
                        }
                        else if (tempBuffMount.AddressableMountEntity.IsDataValid())
                        {
                            addressablePrefab = tempBuffMount.AddressableMountEntity;
                            return true;
                        }
                    }
                    break;
                case MountType.Custom:
                    // TODO: Implement this
                    return false;
            }
            return false;
        }
    }
}
