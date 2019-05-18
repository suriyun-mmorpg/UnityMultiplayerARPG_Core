using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDamageElementAmount : UISelectionEntry<DamageElementAmountTuple>
    {
        /// <summary>
        /// Format => {0} = {Element Title}, {1} = {Min Damage}, {2} = {Max Damage}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Element Title}, {1} = {Min Damage}, {2} = {Max Damage}")]
        public string formatAmount = "{0}: {1}~{2}";

        [Header("UI Elements")]
        public TextWrapper uiTextAmount;

        protected override void UpdateData()
        {
            if (uiTextAmount != null)
            {
                DamageElement element = Data.damageElement;
                MinMaxFloat amount = Data.amount;
                uiTextAmount.text = string.Format(
                    formatAmount,
                    element.Title,
                    amount.min.ToString("N0"),
                    amount.max.ToString("N0"));
            }
        }
    }
}
