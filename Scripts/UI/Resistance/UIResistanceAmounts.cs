using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIResistanceAmounts : UISelectionEntry<Dictionary<DamageElement, float>>
    {
        /// <summary>
        /// Format => {0} = {Resistance Title}, {1} = {Amount * 100}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Resistance Title}, {1} = {Amount * 100}")]
        public string formatAmount = "{0}: {1}%";

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UIResistanceTextPair[] textAmounts;

        private Dictionary<DamageElement, TextWrapper> cacheTextAmounts;
        public Dictionary<DamageElement, TextWrapper> CacheTextAmounts
        {
            get
            {
                if (cacheTextAmounts == null)
                {
                    cacheTextAmounts = new Dictionary<DamageElement, TextWrapper>();
                    DamageElement tempElement;
                    TextWrapper tempTextComponent;
                    foreach (UIResistanceTextPair textAmount in textAmounts)
                    {
                        if (textAmount.damageElement == null || textAmount.uiText == null)
                            continue;
                        tempElement = textAmount.damageElement;
                        tempTextComponent = textAmount.uiText;
                        tempTextComponent.text = string.Format(formatAmount, tempElement.Title, "0", "0");
                        cacheTextAmounts[tempElement] = tempTextComponent;
                    }
                }
                return cacheTextAmounts;
            }
        }

        protected override void UpdateData()
        {
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllAmounts != null)
                    uiTextAllAmounts.gameObject.SetActive(false);

                foreach (KeyValuePair<DamageElement, TextWrapper> entry in CacheTextAmounts)
                {
                    entry.Value.text = string.Format(formatAmount, entry.Key.Title, "0", "0");
                }
            }
            else
            {
                string tempAllText = string.Empty;
                DamageElement tempElement;
                float tempAmount;
                string tempAmountText;
                TextWrapper tempTextWarpper;
                foreach (KeyValuePair<DamageElement, float> dataEntry in Data)
                {
                    if (dataEntry.Key == null || dataEntry.Value == 0)
                        continue;
                    // Set temp data
                    tempElement = dataEntry.Key;
                    tempAmount = dataEntry.Value;
                    // Add new line if text is not empty
                    if (!string.IsNullOrEmpty(tempAllText))
                        tempAllText += "\n";
                    // Set current elemental resistance text
                    tempAmountText = string.Format(
                        formatAmount,
                        tempElement.Title,
                        (tempAmount * 100).ToString("N2"));
                    // Append current elemental resistance text
                    tempAllText += tempAmountText;
                    // Set current elemental resistance text to UI
                    if (CacheTextAmounts.TryGetValue(dataEntry.Key, out tempTextWarpper))
                        tempTextWarpper.text = tempAmountText;
                }

                if (uiTextAllAmounts != null)
                {
                    uiTextAllAmounts.gameObject.SetActive(!string.IsNullOrEmpty(tempAllText));
                    uiTextAllAmounts.text = tempAllText;
                }
            }
        }
    }
}
