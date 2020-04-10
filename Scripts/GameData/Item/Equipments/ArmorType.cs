using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Armor Type", menuName = "Create GameData/Armor Type", order = -4896)]
    public partial class ArmorType : BaseGameData
    {
        [Tooltip("Example: If you want to make it can equip 4 rings, set this to 4")]
        [Range(1, 16)]
        public byte equippableSlots = 1;

        public ArmorType GenerateDefaultArmorType()
        {
            name = GameDataConst.UNKNOW_ARMOR_TYPE_ID;
            title = GameDataConst.UNKNOW_ARMOR_TYPE_TITLE;
            return this;
        }
    }
}
