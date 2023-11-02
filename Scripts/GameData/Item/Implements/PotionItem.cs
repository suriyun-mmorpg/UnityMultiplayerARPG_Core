using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.POTION_ITEM_FILE, menuName = GameDataMenuConsts.POTION_ITEM_MENU, order = GameDataMenuConsts.POTION_ITEM_ORDER)]
    public partial class PotionItem : BaseItem, IPotionItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_POTION.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Potion; }
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

        [Category(3, "Potion Settings")]
        [SerializeField]
        private Buff buff = Buff.Empty;
        public Buff? BuffData
        {
            get { return buff; }
        }

        [SerializeField]
        private string autoUseSettingKey;
        public string AutoUseKey
        {
            get { return autoUseSettingKey; }
        }

        [SerializeField]
        private float useItemCooldown = 0f;
        public float UseItemCooldown
        {
            get { return useItemCooldown; }
        }

        public void UseItem(BaseCharacterEntity characterEntity, int itemIndex, CharacterItem characterItem)
        {
            if (!characterEntity.CanUseItem() || characterItem.level <= 0 || !characterEntity.DecreaseItemsByIndex(itemIndex, 1, false))
                return;
            characterEntity.FillEmptySlots();
            characterEntity.ApplyBuff(DataId, BuffType.PotionBuff, characterItem.level, characterEntity.GetInfo(), null);
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
            BuffData.Value.PrepareRelatesData();
        }
    }
}
