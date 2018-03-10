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

    private Dictionary<Resistance, Text> tempTextAmounts;
    public Dictionary<Resistance, Text> TempTextAmounts
    {
        get
        {
            if (tempTextAmounts == null)
            {
                tempTextAmounts = new Dictionary<Resistance, Text>();
                foreach (var textAmount in textAmounts)
                {
                    if (textAmount.resistance == null || textAmount.text == null)
                        continue;
                    var key = textAmount.resistance;
                    var textComp = textAmount.text;
                    textComp.text = string.Format(amountFormat, key.title, "0", "0");
                    tempTextAmounts[key] = textComp;
                }
            }
            return tempTextAmounts;
        }
    }

    protected override void UpdateData()
    {
        if (textAllAmounts != null)
        {
            if (Data == null || Data.Count == 0)
            {
                textAllAmounts.gameObject.SetActive(false);
                foreach (var textAmount in TempTextAmounts)
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
                    var amountText = string.Format(amountFormat, dataEntry.Key.title, (dataEntry.Value * 100f).ToString("N0"));
                    text += amountText + "\n";
                    if (TempTextAmounts.ContainsKey(dataEntry.Key))
                        TempTextAmounts[dataEntry.Key].text = amountText;
                }
                textAllAmounts.gameObject.SetActive(!string.IsNullOrEmpty(text));
                textAllAmounts.text = text;
            }
        }
    }
}
