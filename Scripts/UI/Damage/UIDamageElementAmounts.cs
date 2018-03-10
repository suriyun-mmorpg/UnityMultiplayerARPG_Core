using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDamageElementAmounts : UISelectionEntry<Dictionary<DamageElement, DamageAmount>>
{
    [Tooltip("Damage Amount Format => {0} = {Element title}, {1} = {Min damage}, {2} = {Max damage}")]
    public string amountFormat = "{0}: {1}~{2}";

    [Header("UI Elements")]
    public Text textAllAmounts;
    public UIDamageElementTextPair[] textAmounts;

    private Dictionary<DamageElement, Text> tempTextAmounts;
    public Dictionary<DamageElement, Text> TempTextAmounts
    {
        get
        {
            if (tempTextAmounts == null)
            {
                tempTextAmounts = new Dictionary<DamageElement, Text>();
                foreach (var textAmount in textAmounts)
                {
                    if (textAmount.damageElement == null || textAmount.text == null)
                        continue;
                    var key = textAmount.damageElement;
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
                    var element = dataEntry.Key;
                    var amount = dataEntry.Value;
                    var amountText = string.Format(amountFormat, element.title, amount.minDamage, amount.maxDamage);
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
