using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial struct CharacterMount
    {
        public BaseSkill GetSkill()
        {
            if (type != MountType.Skill)
                return null;
            if (GameInstance.Skills.TryGetValue(dataId, out BaseSkill skill))
                return skill;
            return null;
        }

        public IMountItem GetMountItem()
        {
            if (type != MountType.MountItem)
                return null;
            if (GameInstance.Items.TryGetValue(dataId, out BaseItem item) && item.IsMount())
                return item as IMountItem;
            return null;
        }

        /// <summary>
        /// Return `TRUE` if it is addressable
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="addressablePrefab"></param>
        /// <returns></returns>
        public bool GetPrefab(out VehicleEntity prefab, out AssetReferenceVehicleEntity addressablePrefab)
        {
            return type.GetPrefab(dataId, out prefab, out addressablePrefab);
        }

        public bool ShouldRemove()
        {
            switch (type)
            {
                case MountType.Skill:
                    BaseSkill skill = GetSkill();
                    if (skill == null || !skill.TryGetMount(out SkillMount mount))
                        return true;
                    if (mount.NoDuration)
                        return false;
                    return mountRemainsDuration <= 0f;
                case MountType.MountItem:
                    IMountItem mountItem = GetMountItem();
                    if (mountItem == null)
                        return true;
                    if (mountItem.NoMountDuration)
                        return false;
                    return mountRemainsDuration <= 0f;
                case MountType.Custom:
                    // TODO: Implement this
                    return false;
            }
            return false;
        }

        public void Update(IVehicleEntity vehicleEntity, float deltaTime)
        {
            if (mountRemainsDuration > 0f)
            {
                mountRemainsDuration -= deltaTime;
                if (mountRemainsDuration < 0f)
                    mountRemainsDuration = 0f;
            }
            currentHp = vehicleEntity.CurrentHp;
        }
    }

    [System.Serializable]
    public class SyncFieldCharacterMount : LiteNetLibSyncField<CharacterMount>
    {
    }


    [System.Serializable]
    public class SyncListCharacterMount : LiteNetLibSyncList<CharacterMount>
    {
    }
}
