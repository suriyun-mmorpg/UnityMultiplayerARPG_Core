using System.Collections.Generic;
using Insthync.AddressableAssetTools;
using LiteNetLibManager;
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
        private ItemRequirement requirement = default;
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

#if UNITY_EDITOR && EXCLUDE_PREFAB_REFS
        public UnityHelpBox entityHelpBox = new UnityHelpBox("`EXCLUDE_PREFAB_REFS` is set, you have to use only addressable assets!", UnityHelpBox.Type.Warning);
#endif
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [Category(3, "Building Settings")]
        [SerializeField]
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

        public void UseItem(BaseCharacterEntity characterEntity, int itemIndex, CharacterItem characterItem)
        {
            // TODO: May changes this function later.
        }

        public bool HasCustomAimControls()
        {
            return true;
        }

        public AimPosition UpdateAimControls(Vector2 aimAxes, params object[] data)
        {
            if (BuildingEntity != null)
            {
                return BasePlayerCharacterController.Singleton.BuildAimController.UpdateAimControls(aimAxes, BuildingEntity);
            }
            else if (AddressableBuildingEntity.IsDataValid())
            {
                return BasePlayerCharacterController.Singleton.BuildAimController.UpdateAimControls(aimAxes, AddressableBuildingEntity.GetOrLoadAsset<BuildingEntity>());
            }
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
            GameInstance.AddBuildingEntities(BuildingEntity);
            GameInstance.AddAssetReferenceBuildingEntities(AddressableBuildingEntity);
        }
    }
}
