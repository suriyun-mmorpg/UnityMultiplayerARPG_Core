using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Equipment Skin Item", menuName = "Create GameData/Item/Equipment Skin Item", order = -4877)]
    public partial class EquipmentSkinItem : BaseEquipmentItem, IEquipmentSkinItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_EQUIPMENT_SKIN.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.EquipmentSkin; }
        }

        [Category("Equipment Settings")]
        [Header("Equipment Skin Settings")]
        [SerializeField]
        private BaseItem baseEquipmentItem = null;
        public BaseItem BaseEquipmentItem
        {
            get { return baseEquipmentItem; }
        }

        public override bool Validate()
        {
            bool hasChanges = false;
            if (baseEquipmentItem != null && !baseEquipmentItem.IsEquipment())
            {
                baseEquipmentItem = null;
                hasChanges = true;
            }
            return hasChanges || base.Validate();
        }
    }
}
