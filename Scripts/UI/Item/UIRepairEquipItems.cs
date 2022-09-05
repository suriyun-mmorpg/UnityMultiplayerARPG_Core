using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIRepairEquipItems : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD);
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGoldNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH);
        [Tooltip("Format => {0} = {Target Amount}")]
        public UILocaleKeySetting formatKeySimpleRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper uiTextRequireGold;
        public TextWrapper uiTextSimpleRequireGold;

        private void LateUpdate()
        {
            int requireGold = 0;
            ItemRepairPrice tempRepairPrice;
            EquipWeapons equipWeapons = GameInstance.PlayingCharacterEntity.EquipWeapons;
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
            foreach (CharacterItem equipItem in GameInstance.PlayingCharacterEntity.EquipItems)
            {
                if (equipItem.IsEmptySlot())
                    continue;
                tempRepairPrice = equipItem.GetItem().GetRepairPrice(equipItem.durability);
                requireGold += tempRepairPrice.RequireGold;
            }

            if (uiTextRequireGold != null)
            {
                uiTextRequireGold.text = ZString.Format(
                    GameInstance.PlayingCharacter.Gold >= requireGold ?
                        LanguageManager.GetText(formatKeyRequireGold) :
                        LanguageManager.GetText(formatKeyRequireGoldNotEnough),
                    GameInstance.PlayingCharacter.Gold.ToString("N0"),
                    requireGold.ToString("N0"));
            }

            if (uiTextSimpleRequireGold != null)
                uiTextSimpleRequireGold.text = ZString.Format(LanguageManager.GetText(formatKeySimpleRequireGold), requireGold.ToString("N0"));
        }

        public void OnClickRepairEquipItems()
        {
            GameInstance.ClientInventoryHandlers.RequestRepairEquipItems(ClientInventoryActions.ResponseRepairEquipItems);
        }
    }
}
