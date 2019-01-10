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
                    foreach (UIDamageElementTextPair textAmount in textDamages)
                    {
                        if (textAmount.damageElement == null || textAmount.uiText == null)
                            continue;
                        DamageElement key = textAmount.damageElement;
                        TextWrapper textComp = textAmount.uiText;
                        textComp.text = string.Format(damageFormat, key.Title, "0", "0");
                        cacheTextDamages[key] = textComp;
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

                foreach (KeyValuePair<DamageElement, TextWrapper> textAmount in CacheTextDamages)
                {
                    DamageElement element = textAmount.Key;
                    textAmount.Value.text = string.Format(damageFormat, element.Title, "0", "0");
                }
            }
            else
            {
                string text = "";
                MinMaxFloat sumDamage = new MinMaxFloat();
                foreach (KeyValuePair<DamageElement, MinMaxFloat> dataEntry in Data)
                {
                    if (dataEntry.Key == null || (dataEntry.Value.min == 0 && dataEntry.Value.max == 0))
                        continue;
                    DamageElement element = dataEntry.Key;
                    MinMaxFloat amount = dataEntry.Value;
                    if (!string.IsNullOrEmpty(text))
                        text += "\n";
                    string amountText = string.Format(damageFormat, element.Title, amount.min.ToString("N0"), amount.max.ToString("N0"));
                    text += amountText;
                    TextWrapper textDamages;
                    if (CacheTextDamages.TryGetValue(dataEntry.Key, out textDamages))
                        textDamages.text = amountText;
                    sumDamage += amount;
                }

                if (uiTextAllDamages != null)
                {
                    uiTextAllDamages.gameObject.SetActive(!string.IsNullOrEmpty(text));
                    uiTextAllDamages.text = text;
                }

                if (uiTextSumDamage != null)
                    uiTextSumDamage.text = string.Format(sumDamageFormat, sumDamage.min.ToString("N0"), sumDamage.max.ToString("N0"));
            }
        }
    }
}
