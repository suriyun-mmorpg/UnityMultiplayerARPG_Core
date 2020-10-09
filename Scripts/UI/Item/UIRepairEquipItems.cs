using UnityEngine;
using System.Collections;

namespace MultiplayerARPG
{
    public class UIRepairEquipItems : UIBase
    {
        public BasePlayerCharacterEntity OwningCharacter { get { return BasePlayerCharacterController.OwningCharacter; } }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD);
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGoldNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH);

        [Header("UI Elements")]
        public TextWrapper uiTextRequireGold;

        private void LateUpdate()
        {
            int requireGold = 0;
            ItemRepairPrice tempRepairPrice;
            EquipWeapons equipWeapons = BasePlayerCharacterController.OwningCharacter.EquipWeapons;
            if (!equipWeapons.IsEmptyRightHandSlot())
            {
                tempRepairPrice = equipWeapons.rightHand.GetItem().GetRepairPrice(equipWeapons.rightHand.durability);
                requireGold += tempRepairPrice.RequireGold;
            }
            if (!equipWeapons.IsEmptyLeftHandSlot())
            {
                tempRepairPrice = equipWeapons.leftHand.GetItem().GetRepairPrice(equipWeapons.leftHand.durability);
                requireGold += tempRepairPrice.RequireGold;
            }
            foreach (CharacterItem equipItem in BasePlayerCharacterController.OwningCharacter.EquipItems)
            {
                if (equipItem.IsEmptySlot())
                    continue;
                tempRepairPrice = equipItem.GetItem().GetRepairPrice(equipItem.durability);
                requireGold += tempRepairPrice.RequireGold;
            }

            if (uiTextRequireGold != null)
            {
                uiTextRequireGold.text = string.Format(
                    OwningCharacter.Gold >= requireGold ?
                        LanguageManager.GetText(formatKeyRequireGold) :
                        LanguageManager.GetText(formatKeyRequireGoldNotEnough),
                    OwningCharacter.Gold.ToString("N0"),
                    requireGold.ToString("N0"));
            }
        }

        public void OnClickRepairEquipItems()
        {
            BasePlayerCharacterController.OwningCharacter.CallServerRepairEquipItems();
        }
    }
}
