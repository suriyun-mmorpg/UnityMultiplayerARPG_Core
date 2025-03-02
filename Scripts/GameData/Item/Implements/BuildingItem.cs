using Insthync.AddressableAssetTools;
using Insthync.UnityEditorUtils;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.BUILDING_ITEM_FILE, menuName = GameDataMenuConsts.BUILDING_ITEM_MENU, order = GameDataMenuConsts.BUILDING_ITEM_ORDER)]
    public partial class BuildingItem : BaseItem, IBuildingItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_BUILDING.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Building; }
        }

        [Category(2, "Requirements")]
        [SerializeField]
        private ItemRequirement requirement = new ItemRequirement();
        public ItemRequirement Requirement
        {
            get { return requirement; }
        }

        [System.NonSerialized]
        private Dictionary<Attribute, float> _cacheRequireAttributeAmounts = null;
        public Dictionary<Attribute, float> RequireAttributeAmounts
        {
            get
            {
                if (_cacheRequireAttributeAmounts == null)
                    _cacheRequireAttributeAmounts = GameDataHelpers.CombineAttributes(requirement.attributeAmounts, new Dictionary<Attribute, float>(), 1f);
                return _cacheRequireAttributeAmounts;
            }
        }

#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [Category(3, "Building Settings")]
        [SerializeField]
        [AddressableAssetConversion(nameof(addressableBuildingEntity))]
        private BuildingEntity buildingEntity = null;
#endif
        public BuildingEntity BuildingEntity
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return buildingEntity;
#else
                return null;
#endif
            }
        }

        [SerializeField]
        private AssetReferenceBuildingEntity addressableBuildingEntity = null;
        public AssetReferenceBuildingEntity AddressableBuildingEntity
        {
            get { return addressableBuildingEntity; }
        }

        [SerializeField]
        private float useItemCooldown = 0f;
        public float UseItemCooldown
        {
            get { return useItemCooldown; }
        }

        public bool UseItem(BaseCharacterEntity characterEntity, int itemIndex, CharacterItem characterItem)
        {
            // TODO: May changes this function later.
            return true;
        }

        public bool HasCustomAimControls()
        {
            return true;
        }

        public AimPosition UpdateAimControls(Vector2 aimAxes, params object[] data)
        {
            BuildingEntity tempBuildingEntity = AddressableBuildingEntity.GetOrLoadAssetOrUsePrefab(BuildingEntity);
            if (tempBuildingEntity != null)
                return BasePlayerCharacterController.Singleton.BuildAimController.UpdateAimControls(aimAxes, tempBuildingEntity);
            return default;
        }

        public void FinishAimControls(bool isCancel)
        {
            BasePlayerCharacterController.Singleton.BuildAimController.FinishAimControls(isCancel);
        }

        public bool IsChanneledAbility()
        {
            return false;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
#if !EXCLUDE_PREFAB_REFS
            GameInstance.AddBuildingEntities(BuildingEntity);
#endif
            GameInstance.AddAssetReferenceBuildingEntities(AddressableBuildingEntity);
        }
    }
}
