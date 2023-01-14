using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UINpcDialogConfirmRequirement : UISelectionEntry<NpcDialogConfirmRequirement>
    {
        [Tooltip("Format => {0} = {Require Gold}")]
        public UILocaleKeySetting formatKeyRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD);
        [Tooltip("Format => {0} = {Current Gold}, {1} = {Require Gold}")]
        public UILocaleKeySetting formatKeyRequireGoldNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH);

        [Header("UI Elements")]
        public TextWrapper uiTextRequireGold;
        public UICurrencyAmounts uiRequireCurrencyAmounts;
        public UIItemAmounts uiRequireItemAmounts;

        protected override void UpdateData()
        {
            if (uiTextRequireGold != null)
            {
                if (Data.gold <= 0)
                {
                    // Hide require level label when require level <= 0
                    uiTextRequireGold.SetGameObjectActive(false);
                }
                else
                {
                    uiTextRequireGold.SetGameObjectActive(true);
                    float characterGold = (GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.Gold : 0);
                    float requireCharacterGold = Data.gold;
                    if (characterGold >= requireCharacterGold)
                    {
                        uiTextRequireGold.text = ZString.Format(
                            LanguageManager.GetText(formatKeyRequireGold),
                            requireCharacterGold.ToString("N0"));
                    }
                    else
                    {
                        uiTextRequireGold.text = ZString.Format(
                            LanguageManager.GetText(formatKeyRequireGoldNotEnough),
                            characterGold,
                            requireCharacterGold.ToString("N0"));
                    }
                }
            }

            if (uiRequireCurrencyAmounts != null)
            {
                uiRequireCurrencyAmounts.displayType = UICurrencyAmounts.DisplayType.Requirement;
                uiRequireCurrencyAmounts.isBonus = false;
                uiRequireCurrencyAmounts.Show();
                uiRequireCurrencyAmounts.Data = GameDataHelpers.CombineCurrencies(Data.currencyAmounts, null);
            }

            if (uiRequireItemAmounts != null)
            {
                uiRequireItemAmounts.displayType = UIItemAmounts.DisplayType.Requirement;
                uiRequireItemAmounts.isBonus = false;
                uiRequireItemAmounts.Show();
                uiRequireItemAmounts.Data = GameDataHelpers.CombineItems(Data.itemAmounts, null);
            }
        }
    }
}
