using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDamageElementInflictions : UISelectionEntry<Dictionary<DamageElement, float>>
    {
        [Tooltip("Default Element Infliction Format => {1} = {Rate}")]
        public string defaultElementInflictionFormat = "Inflict {1}% damage";
        [Tooltip("Infliction Format => {0} = {Element title}, {1} = {Rate}")]
        public string inflictionFormat = "Inflict {1}% as {0} damage";

        [Header("UI Elements")]
        public TextWrapper uiTextAllInflictions;
        public UIDamageElementTextPair[] textInflictions;

        private Dictionary<DamageElement, TextWrapper> cacheTextInflictions;
        public Dictionary<DamageElement, TextWrapper> CacheTextInflictions
        {
            get
            {
                if (cacheTextInflictions == null)
                {
                    cacheTextInflictions = new Dictionary<DamageElement, TextWrapper>();
                    foreach (UIDamageElementTextPair textAmount in textInflictions)
                    {
                        if (textAmount.damageElement == null || textAmount.uiText == null)
                            continue;
                        DamageElement key = textAmount.damageElement;
                        TextWrapper textComp = textAmount.uiText;
                        textComp.text = string.Format(inflictionFormat, key.Title, "0");
                        cacheTextInflictions[key] = textComp;
                    }
                }
                return cacheTextInflictions;
            }
        }

        protected override void UpdateData()
        {
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllInflictions != null)
                    uiTextAllInflictions.gameObject.SetActive(false);

                foreach (KeyValuePair<DamageElement, TextWrapper> textAmount in CacheTextInflictions)
                {
                    DamageElement element = textAmount.Key;
                    string format = element == GameInstance.Singleton.DefaultDamageElement ? defaultElementInflictionFormat : inflictionFormat;
                    textAmount.Value.text = string.Format(format, element.Title, "0");
                }
            }
            else
            {
                string text = "";
                MinMaxFloat sumDamage = new MinMaxFloat();
                foreach (KeyValuePair<DamageElement, float> dataEntry in Data)
                {
                    if (dataEntry.Key == null || dataEntry.Value == 0)
                        continue;
                    DamageElement element = dataEntry.Key;
                    float rate = dataEntry.Value;
                    if (!string.IsNullOrEmpty(text))
                        text += "\n";
                    string format = element == GameInstance.Singleton.DefaultDamageElement ? defaultElementInflictionFormat : inflictionFormat;
                    string amountText = string.Format(format, element.Title, (rate * 100f).ToString("N0"));
                    text += amountText;
                    TextWrapper textDamages;
                    if (CacheTextInflictions.TryGetValue(dataEntry.Key, out textDamages))
                        textDamages.text = amountText;
                    sumDamage += rate;
                }

                if (uiTextAllInflictions != null)
                {
                    uiTextAllInflictions.gameObject.SetActive(!string.IsNullOrEmpty(text));
                    uiTextAllInflictions.text = text;
                }
            }
        }
    }
}
