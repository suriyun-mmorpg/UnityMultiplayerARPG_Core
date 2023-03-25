using Cysharp.Text;
using System.Collections.Generic;
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
        public UIItemAmounts uiRequireItemAmounts;
        public UICurrencyAmounts uiRequireCurrencyAmounts;
        public TextWrapper uiTextSimpleRequireGold;

        private void LateUpdate()
        {
            int requireGold = 0;
            List<ItemAmount> requireItems = new List<ItemAmount>();
            List<CurrencyAmount> requireCurrencies = new List<CurrencyAmount>();
            ItemRepairPrice tempRepairPrice;
            EquipWeapons equipWeapons = GameInstance.PlayingCharacterEntity.EquipWeapons;
            if (!equipWeapons.IsEmptyRightHandSlot())
            {
                tempRepairPrice = equipWeapons.rightHand.GetItem().GetRepairPrice(equipWeapons.rightHand.durability);
                requireGold += tempRepairPrice.RequireGold;
                if (tempRepairPrice.RequireItems != null && tempRepairPrice.RequireItems.Length > 0)
                    requireItems.AddRange(tempRepairPrice.RequireItems);
                if (tempRepairPrice.RequireCurrencies != null && tempRepairPrice.RequireCurrencies.Length > 0)
                    requireCurrencies.AddRange(tempRepairPrice.RequireCurrencies);
            }
            if (!equipWeapons.IsEmptyLeftHandSlot())
            {
                tempRepairPrice = equipWeapons.leftHand.GetItem().GetRepairPrice(equipWeapons.leftHand.durability);
                requireGold += tempRepairPrice.RequireGold;
                if (tempRepairPrice.RequireItems != null && tempRepairPrice.RequireItems.Length > 0)
                    requireItems.AddRange(tempRepairPrice.RequireItems);
                if (tempRepairPrice.RequireCurrencies != null && tempRepairPrice.RequireCurrencies.Length > 0)
                    requireCurrencies.AddRange(tempRepairPrice.RequireCurrencies);
            }
            foreach (CharacterItem equipItem in GameInstance.PlayingCharacterEntity.EquipItems)
            {
                if (equipItem.IsEmptySlot())
                    continue;
                tempRepairPrice = equipItem.GetItem().GetRepairPrice(equipItem.durability);
                requireGold += tempRepairPrice.RequireGold;
                if (tempRepairPrice.RequireItems != null && tempRepairPrice.RequireItems.Length > 0)
                    requireItems.AddRange(tempRepairPrice.RequireItems);
                if (tempRepairPrice.RequireCurrencies != null && tempRepairPrice.RequireCurrencies.Length > 0)
                    requireCurrencies.AddRange(tempRepairPrice.RequireCurrencies);
            }

            if (uiRequireItemAmounts != null)
            {
                if (requireItems.Count == 0)
                {
                    uiRequireItemAmounts.Hide();
                }
                else
                {
                    uiRequireItemAmounts.displayType = UIItemAmounts.DisplayType.Requirement;
                    uiRequireItemAmounts.Show();
                    uiRequireItemAmounts.Data = GameDataHelpers.CombineItems(requireItems, null);
                }
            }

            if (uiRequireCurrencyAmounts != null)
            {
                if (requireCurrencies.Count == 0)
                {
                    uiRequireCurrencyAmounts.Hide();
                }
                else
                {
                    uiRequireCurrencyAmounts.displayType = UICurrencyAmounts.DisplayType.Requirement;
                    uiRequireCurrencyAmounts.Show();
                    uiRequireCurrencyAmounts.Data = GameDataHelpers.CombineCurrencies(requireCurrencies, null);
                }
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
