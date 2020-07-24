using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Building Item", menuName = "Create GameData/Item/Building Item", order = -4885)]
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

        [Header("Building Configs")]
        [SerializeField]
        private BuildingEntity buildingEntity;
        public BuildingEntity BuildingEntity
        {
            get { return buildingEntity; }
        }

        public void UseItem(BaseCharacterEntity characterEntity, short itemIndex, CharacterItem characterItem)
        {
            // TODO: May changes this function later.
        }

        public bool HasCustomAimControls()
        {
            return true;
        }

        public Vector3? UpdateAimControls(Vector2 aimAxes, params object[] data)
        {
            return BasePlayerCharacterController.Singleton.UpdateBuildAimControls(aimAxes, BuildingEntity);
        }

        public void FinishAimControls(bool isCancel)
        {
            BasePlayerCharacterController.Singleton.FinishBuildAimControls(isCancel);
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            // Add building entity
            GameInstance.AddBuildingEntities(new BuildingEntity[] { buildingEntity });
        }
    }
}
