using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDamageElementAmount : UISelectionEntry<DamageElementAmountTuple>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Damage Element Title}, {1} = {Min Damage}, {2} = {Max Damage}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_DAMAGE_WITH_ELEMENTAL);

        [Header("UI Elements")]
        public TextWrapper uiTextAmount;

        protected override void UpdateData()
        {
            if (uiTextAmount != null)
            {
                DamageElement element = Data.damageElement;
                MinMaxFloat amount = Data.amount;
                uiTextAmount.text = string.Format(
                    LanguageManager.GetText(formatKeyAmount),
                    element.Title,
                    amount.min.ToString("N0"),
                    amount.max.ToString("N0"));
            }
        }
    }
}
