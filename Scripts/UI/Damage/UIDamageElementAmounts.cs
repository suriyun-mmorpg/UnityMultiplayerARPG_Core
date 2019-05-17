using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDamageElementAmounts : UISelectionEntry<Dictionary<DamageElement, MinMaxFloat>>
    {
        [Tooltip("Damage Amount Format => {0} = {Element title}, {1} = {Min damage}, {2} = {Max damage}")]
        public string damageFormat = "{0}: {1}~{2}";
        [Tooltip("Sum Damage Amount Format => {0} = {Min damage}, {1} = {Max damage}")]
        public string sumDamageFormat = "{0}~{1}";

        [Header("UI Elements")]
        public TextWrapper uiTextAllDamages;
        public TextWrapper uiTextSumDamage;
        public UIDamageElementTextPair[] textDamages;

        private Dictionary<DamageElement, TextWrapper> cacheTextDamages;
        public Dictionary<DamageElement, TextWrapper> CacheTextDamages
        {
            get
            {
                if (cacheTextDamages == null)
                {
                    cacheTextDamages = new Dictionary<DamageElement, TextWrapper>();
                    DamageElement tempElement;
                    TextWrapper tempTextComponent;
                    foreach (UIDamageElementTextPair textAmount in textDamages)
                    {
                        if (textAmount.damageElement == null || textAmount.uiText == null)
                            continue;
                        tempElement = textAmount.damageElement;
                        tempTextComponent = textAmount.uiText;
                        tempTextComponent.text = string.Format(damageFormat, tempElement.Title, "0", "0");
                        cacheTextDamages[tempElement] = tempTextComponent;
                    }
                }
                return cacheTextDamages;
            }
        }

        protected override void UpdateData()
        {
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllDamages != null)
                    uiTextAllDamages.gameObject.SetActive(false);

                if (uiTextSumDamage != null)
                    uiTextSumDamage.text = string.Format(sumDamageFormat, "0", "0");

                foreach (KeyValuePair<DamageElement, TextWrapper> entry in CacheTextDamages)
                {
                    entry.Value.text = string.Format(damageFormat, entry.Key.Title, "0", "0");
                }
            }
            else
            {
                string tempAllText = string.Empty;
                MinMaxFloat sumDamage = new MinMaxFloat();
                DamageElement tempElement;
                MinMaxFloat tempAmount;
                string tempAmountText;
                TextWrapper tempTextWrapper;
                foreach (KeyValuePair<DamageElement, MinMaxFloat> dataEntry in Data)
                {
                    if (dataEntry.Key == null || (dataEntry.Value.min == 0 && dataEntry.Value.max == 0))
                        continue;
                    // Set temp data
                    tempElement = dataEntry.Key;
                    tempAmount = dataEntry.Value;
                    // Add new line if text is not empty
                    if (!string.IsNullOrEmpty(tempAllText))
                        tempAllText += "\n";
                    // Set current elemental damage text
                    tempAmountText = string.Format(damageFormat, tempElement.Title, tempAmount.min.ToString("N0"), tempAmount.max.ToString("N0"));
                    // Append current elemental damage text
                    tempAllText += tempAmountText;
                    // Set current elemental damage text to UI
                    if (CacheTextDamages.TryGetValue(dataEntry.Key, out tempTextWrapper))
                        tempTextWrapper.text = tempAmountText;
                    sumDamage += tempAmount;
                }

                if (uiTextAllDamages != null)
                {
                    uiTextAllDamages.gameObject.SetActive(!string.IsNullOrEmpty(tempAllText));
                    uiTextAllDamages.text = tempAllText;
                }

                if (uiTextSumDamage != null)
                    uiTextSumDamage.text = string.Format(sumDamageFormat, sumDamage.min.ToString("N0"), sumDamage.max.ToString("N0"));
            }
        }
    }
}
