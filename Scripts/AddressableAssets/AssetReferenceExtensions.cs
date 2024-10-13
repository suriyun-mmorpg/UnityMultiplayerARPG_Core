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
                    if (GameInstance.Skills.TryGetValue(dataId, out BaseSkill skill) && skill.TryGetSummon(out SkillSummon skillSummon))
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
                    if (GameInstance.Items.TryGetValue(dataId, out BaseItem item) && item.IsPet())
                    {
                        IPetItem petItem = item as IPetItem;
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

        public static bool GetPrefab(this MountType mountType, int dataId, out VehicleEntity prefab, out AssetReferenceVehicleEntity addressablePrefab)
        {
            prefab = null;
            addressablePrefab = null;
            switch (mountType)
            {
                case MountType.Skill:
                    if (GameInstance.Skills.TryGetValue(dataId, out BaseSkill skill) && skill.TryGetMount(out SkillMount skillMount))
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
                    if (GameInstance.Items.TryGetValue(dataId, out BaseItem item) && item.IsMount())
                    {
                        IMountItem mountItem = item as IMountItem;
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
                case MountType.Custom:
                    // TODO: Implement this
                    return false;
            }
            return false;
        }
    }
}
