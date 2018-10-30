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
                    foreach (var textAmount in textInflictions)
                    {
                        if (textAmount.damageElement == null || textAmount.uiText == null)
                            continue;
                        var key = textAmount.damageElement;
                        var textComp = textAmount.uiText;
                        textComp.text = string.Format(inflictionFormat, key.title, "0");
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

                foreach (var textAmount in CacheTextInflictions)
                {
                    var element = textAmount.Key;
                    var format = element == GameInstance.Singleton.DefaultDamageElement ? defaultElementInflictionFormat : inflictionFormat;
                    textAmount.Value.text = string.Format(format, element.title, "0");
                }
            }
            else
            {
                var text = "";
                var sumDamage = new MinMaxFloat();
                foreach (var dataEntry in Data)
                {
                    if (dataEntry.Key == null || dataEntry.Value == 0)
                        continue;
                    var element = dataEntry.Key;
                    var rate = dataEntry.Value;
                    if (!string.IsNullOrEmpty(text))
                        text += "\n";
                    var format = element == GameInstance.Singleton.DefaultDamageElement ? defaultElementInflictionFormat : inflictionFormat;
                    var amountText = string.Format(format, element.title, (rate * 100f).ToString("N0"));
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
