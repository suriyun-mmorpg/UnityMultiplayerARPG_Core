using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIResistanceAmounts : UISelectionEntry<Dictionary<DamageElement, float>>
    {
        [Tooltip("Resistance Amount Format => {0} = {Resistance title}, {1} = {Amount * 100f}")]
        public string amountFormat = "{0}: {1}%";

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
                        tempTextComponent.text = string.Format(amountFormat, tempElement.Title, "0", "0");
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
                    entry.Value.text = string.Format(amountFormat, entry.Key.Title, "0", "0");
                }
            }
            else
            {
                string tempAllText = string.Empty;
                string tempAmountText;
                TextWrapper tempTextWarpper;
                foreach (KeyValuePair<DamageElement, float> dataEntry in Data)
                {
                    if (dataEntry.Key == null || dataEntry.Value == 0)
                        continue;

                    if (!string.IsNullOrEmpty(tempAllText))
                        tempAllText += "\n";
                    tempAmountText = string.Format(amountFormat, dataEntry.Key.Title, (dataEntry.Value * 100f).ToString("N2"));
                    tempAllText += tempAmountText;
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
