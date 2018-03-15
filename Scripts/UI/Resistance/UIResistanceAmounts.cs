using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIResistanceAmounts : UISelectionEntry<Dictionary<Resistance, float>>
{
    [Tooltip("Resistance Amount Format => {0} = {Resistance title}, {1} = {Amount * 100f}")]
    public string amountFormat = "{0}: {1}%";

    [Header("UI Elements")]
    public Text textAllAmounts;
    public UIResistanceTextPair[] textAmounts;

    private Dictionary<Resistance, Text> cacheTextAmounts;
    public Dictionary<Resistance, Text> CacheTextAmounts
    {
        get
        {
            if (cacheTextAmounts == null)
            {
                cacheTextAmounts = new Dictionary<Resistance, Text>();
                foreach (var textAmount in textAmounts)
                {
                    if (textAmount.resistance == null || textAmount.text == null)
                        continue;
                    var key = textAmount.resistance;
                    var textComp = textAmount.text;
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
            if (textAllAmounts != null)
            {
                Debug.LogError("2");
                textAllAmounts.gameObject.SetActive(false);
            }

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
                if (CacheTextAmounts.ContainsKey(dataEntry.Key))
                    CacheTextAmounts[dataEntry.Key].text = amountText;
            }
            if (textAllAmounts != null)
            {
                textAllAmounts.gameObject.SetActive(!string.IsNullOrEmpty(text));
                textAllAmounts.text = text;
            }
        }
    }
}
