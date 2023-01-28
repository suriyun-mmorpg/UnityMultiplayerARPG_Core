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

        [Category(3, "Building Settings")]
        [SerializeField]
        private BuildingEntity buildingEntity = null;
        public BuildingEntity BuildingEntity
        {
            get { return buildingEntity; }
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
            return BasePlayerCharacterController.Singleton.BuildAimController.UpdateAimControls(aimAxes, BuildingEntity);
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
        }
    }
}
