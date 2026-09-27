using Cysharp.Text;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UINpcDialogConfirmRequirement : UISelectionEntry<NpcDialogConfirmRequirement>
    {
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD);
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGoldNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH);

        [Header("UI Elements")]
        public TextWrapper uiTextRequireGold;
        public UICurrencyAmounts uiRequireCurrencyAmounts;
        public UIItemAmounts uiRequireItemAmounts;

        protected CurrencyAmounts _tempRequireCurrencies;
        protected Dictionary<BaseItem, int> _tempRequireItems = new Dictionary<BaseItem, int>();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            uiTextRequireGold = null;
            uiRequireCurrencyAmounts = null;
            uiRequireItemAmounts = null;
            _tempRequireCurrencies.Clear();
            _tempRequireItems.Clear();
            _tempRequireItems = null;
        }

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
                    float characterGold = GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.Gold : 0;
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
                _tempRequireCurrencies.Clear();
                GameDataHelpers.CombineCurrencies(Data.currencyAmounts, ref _tempRequireCurrencies, 1f);
                uiRequireCurrencyAmounts.Data = _tempRequireCurrencies;
            }

            if (uiRequireItemAmounts != null)
            {
                uiRequireItemAmounts.displayType = UIItemAmounts.DisplayType.Requirement;
                uiRequireItemAmounts.isBonus = false;
                uiRequireItemAmounts.Show();
                _tempRequireItems.Clear();
                GameDataHelpers.CombineItems(Data.itemAmounts, _tempRequireItems);
                uiRequireItemAmounts.Data = _tempRequireItems;
            }
        }
    }
}
