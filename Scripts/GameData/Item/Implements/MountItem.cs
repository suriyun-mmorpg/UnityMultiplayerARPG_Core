using Insthync.AddressableAssetTools;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.MOUNT_ITEM_FILE, menuName = GameDataMenuConsts.MOUNT_ITEM_MENU, order = GameDataMenuConsts.MOUNT_ITEM_ORDER)]
    public partial class MountItem : BaseItem, IMountItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_MOUNT.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Mount; }
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

        [Category(3, "Mount Settings")]
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [SerializeField]
        [AddressableAssetConversion(nameof(addressableMountEntity))]
        private VehicleEntity mountEntity = null;
#endif
        public VehicleEntity VehicleEntity
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return mountEntity;
#else
                return null;
#endif
            }
        }

        [SerializeField]
        private AssetReferenceVehicleEntity addressableMountEntity = null;
        public AssetReferenceVehicleEntity AddressableVehicleEntity
        {
            get { return addressableMountEntity; }
        }

        [SerializeField]
        private float useItemCooldown = 0f;
        public float UseItemCooldown
        {
            get { return useItemCooldown; }
        }

        public void UseItem(BaseCharacterEntity characterEntity, int itemIndex, CharacterItem characterItem)
        {
            if (!characterEntity.CanUseItem() || characterItem.level <= 0)
                return;

            characterEntity.Mount(VehicleEntity, AddressableVehicleEntity);
        }

        public bool HasCustomAimControls()
        {
            return false;
        }

        public AimPosition UpdateAimControls(Vector2 aimAxes, params object[] data)
        {
            return default;
        }

        public void FinishAimControls(bool isCancel)
        {

        }

        public bool IsChanneledAbility()
        {
            return false;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddVehicleEntities(VehicleEntity);
            GameInstance.AddAssetReferenceVehicleEntities(AddressableVehicleEntity);
        }
    }
}
