using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAttributeAmounts : UISelectionEntry<Dictionary<Attribute, int>>
{
    [Tooltip("Attribute Amount Format => {0} = {Attribute title}, {1} = {Amount}")]
    public string amountFormat = "{0}: {1}";

    [Header("UI Elements")]
    public Text textAllAmounts;
    public UIAttributeTextTuple[] textAmounts;

    private Dictionary<Attribute, Text> cacheTextAmounts;
    public Dictionary<Attribute, Text> CacheTextAmounts
    {
        get
        {
            if (cacheTextAmounts == null)
            {
                cacheTextAmounts = new Dictionary<Attribute, Text>();
                foreach (var textAmount in textAmounts)
                {
                    if (textAmount.attribute == null || textAmount.text == null)
                        continue;
                    var key = textAmount.attribute;
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
                textAllAmounts.gameObject.SetActive(false);

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
                var amountText = string.Format(amountFormat, dataEntry.Key.title, dataEntry.Value);
                text += amountText;
                Text cacheTextAmount;
                if (CacheTextAmounts.TryGetValue(dataEntry.Key, out cacheTextAmount))
                    cacheTextAmount.text = amountText;
            }
            if (textAllAmounts != null)
            {
                textAllAmounts.gameObject.SetActive(!string.IsNullOrEmpty(text));
                textAllAmounts.text = text;
            }
        }
    }
}
