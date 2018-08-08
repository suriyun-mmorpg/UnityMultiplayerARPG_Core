using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UIDamageElementAmount : UISelectionEntry<DamageElementAmountTuple>
    {
        [Tooltip("Damage Amount Format => {0} = {Element title}, {1} = {Min damage}, {2} = {Max damage}")]
        public string amountFormat = "{0}: {1}~{2}";

        [Header("UI Elements")]
        public Text textAmount;
        public TextWrapper uiTextAmount;

        protected override void UpdateData()
        {
            if (uiTextAmount != null)
            {
                var element = Data.damageElement;
                var amount = Data.amount;
                uiTextAmount.text = string.Format(amountFormat, element.title, amount.min.ToString("N0"), amount.max.ToString("N0"));
            }
        }

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextAmount = MigrateUIHelpers.SetWrapperToText(textAmount, uiTextAmount);
        }
    }
}
