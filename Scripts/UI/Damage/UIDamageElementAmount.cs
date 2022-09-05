using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDamageElementAmount : UISelectionEntry<UIDamageElementAmountData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Damage Element Title}, {1} = {Min Damage}, {2} = {Max Damage}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_DAMAGE_WITH_ELEMENTAL);

        [Header("UI Elements")]
        public TextWrapper uiTextAmount;

        protected override void UpdateData()
        {
            if (uiTextAmount != null)
            {
                uiTextAmount.text = ZString.Format(
                    LanguageManager.GetText(formatKeyAmount),
                    Data.damageElement.Title,
                    Data.amount.min.ToString("N0"),
                    Data.amount.max.ToString("N0"));
            }
        }
    }
}
