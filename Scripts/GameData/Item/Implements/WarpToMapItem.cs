using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.WARP_TO_MAP_ITEM_FILE, menuName = GameDataMenuConsts.WARP_TO_MAP_ITEM_MENU, order = GameDataMenuConsts.WARP_TO_MAP_ITEM_ORDER)]
    public class WarpToMapItem : BaseItem, IUsableItem
    {
        [SerializeField]
        private WarpPortalType warpPortalType = WarpPortalType.Default;
        [Tooltip("Map which character will warp to when use the item, leave this empty to warp character to other position in the same map")]
        [SerializeField]
        private BaseMapInfo warpToMapInfo = null;
        [Tooltip("Position which character will warp to when use the item")]
        [SerializeField]
        private Vector3 warpToPosition = Vector3.zero;
        [Tooltip("If this is `TRUE` it will change character's rotation when warp")]
        [SerializeField]
        private bool warpOverrideRotation = false;
        [Tooltip("This will be used if `warpOverrideRotation` is `TRUE` to change character's rotation when warp")]
        [SerializeField]
        private Vector3 warpToRotation = Vector3.zero;

        [SerializeField]
        private float useItemCooldown = 0f;
        public float UseItemCooldown
        {
            get { return useItemCooldown; }
        }

        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_CONSUMABLE.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Potion; }
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

        public void UseItem(BaseCharacterEntity characterEntity, int itemIndex, CharacterItem characterItem)
        {
            BasePlayerCharacterEntity playerCharacterEntity = characterEntity as BasePlayerCharacterEntity;
            if (playerCharacterEntity == null || !characterEntity.CanUseItem() || characterItem.level <= 0 || !characterEntity.DecreaseItemsByIndex(itemIndex, 1, false))
                return;
            if (warpToMapInfo == null)
                BaseGameNetworkManager.Singleton.WarpCharacter(warpPortalType, playerCharacterEntity, string.Empty, warpToPosition, warpOverrideRotation, warpToRotation);
            else
                BaseGameNetworkManager.Singleton.WarpCharacter(warpPortalType, playerCharacterEntity, warpToMapInfo.Id, warpToPosition, warpOverrideRotation, warpToRotation);
        }
    }
}
