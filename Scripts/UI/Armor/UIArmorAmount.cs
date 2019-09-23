using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIArmorAmount : UISelectionEntry<UIArmorAmountData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Armor Title}, {1} = {Amount}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ARMOR_AMOUNT);

        [Header("UI Elements")]
        public TextWrapper uiTextAmount;

        protected override void UpdateData()
        {
            if (uiTextAmount != null)
            {
                uiTextAmount.text = string.Format(
                    LanguageManager.GetText(formatKeyAmount),
                    Data.damageElement.Title,
                    Data.amount.ToString("N0"));
            }
        }
    }
}
