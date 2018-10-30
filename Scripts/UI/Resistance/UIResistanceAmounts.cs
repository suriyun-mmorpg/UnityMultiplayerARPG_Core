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
                    foreach (var textAmount in textAmounts)
                    {
                        if (textAmount.damageElement == null || textAmount.uiText == null)
                            continue;
                        var key = textAmount.damageElement;
                        var textComp = textAmount.uiText;
                        textComp.text = string.Format(amountFormat, key.title, "0", "0");
                        cacheTextAmounts[key] = textComp;
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

                foreach (var textAmount in CacheTextAmounts)
                {
                    var element = textAmount.Key;
                    textAmount.Value.text = string.Format(amountFormat, element.title, "0", "0");
                }
            }
            else
            {
                var text = "";
                foreach (var dataEntry in Data)
                {
                    if (dataEntry.Key == null || dataEntry.Value == 0)
                        continue;
                    if (!string.IsNullOrEmpty(text))
                        text += "\n";
                    var amountText = string.Format(amountFormat, dataEntry.Key.title, (dataEntry.Value * 100f).ToString("N0"));
                    text += amountText;
                    TextWrapper cacheTextAmount;
                    if (CacheTextAmounts.TryGetValue(dataEntry.Key, out cacheTextAmount))
                        cacheTextAmount.text = amountText;
                }
                if (uiTextAllAmounts != null)
                {
                    uiTextAllAmounts.gameObject.SetActive(!string.IsNullOrEmpty(text));
                    uiTextAllAmounts.text = text;
                }
            }
        }
    }
}
