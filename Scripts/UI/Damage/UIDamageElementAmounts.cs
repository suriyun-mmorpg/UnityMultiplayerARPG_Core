using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDamageElementAmounts : UISelectionEntry<Dictionary<DamageElement, MinMaxFloat>>
{
    [Tooltip("Damage Amount Format => {0} = {Element title}, {1} = {Min damage}, {2} = {Max damage}")]
    public string damageFormat = "{0}: {1}~{2}";
    [Tooltip("Sum Damage Amount Format => {0} = {Min damage}, {1} = {Max damage}")]
    public string sumDamageFormat = "{0}~{1}";

    [Header("UI Elements")]
    public Text textAllDamages;
    public Text textSumDamage;
    public UIDamageElementTextPair[] textDamages;

    private Dictionary<DamageElement, Text> cacheTextDamages;
    public Dictionary<DamageElement, Text> CacheTextDamages
    {
        get
        {
            if (cacheTextDamages == null)
            {
                cacheTextDamages = new Dictionary<DamageElement, Text>();
                foreach (var textAmount in textDamages)
                {
                    if (textAmount.damageElement == null || textAmount.text == null)
                        continue;
                    var key = textAmount.damageElement;
                    var textComp = textAmount.text;
                    textComp.text = string.Format(damageFormat, key.title, "0", "0");
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
            if (textAllDamages != null)
                textAllDamages.gameObject.SetActive(false);

            if (textSumDamage != null)
                textSumDamage.text = string.Format(sumDamageFormat, "0", "0");

            foreach (var textAmount in CacheTextDamages)
            {
                var element = textAmount.Key;
                textAmount.Value.text = string.Format(damageFormat, element.title, "0", "0");
            }
        }
        else
        {
            var text = "";
            var sumDamage = new MinMaxFloat();
            foreach (var dataEntry in Data)
            {
                if (dataEntry.Key == null || (dataEntry.Value.min == 0 && dataEntry.Value.max == 0))
                    continue;
                var element = dataEntry.Key;
                var amount = dataEntry.Value;
                if (!string.IsNullOrEmpty(text))
                    text += "\n";
                var amountText = string.Format(damageFormat, element.title, amount.min.ToString("N0"), amount.max.ToString("N0"));
                text += amountText;
                Text textDamages;
                if (CacheTextDamages.TryGetValue(dataEntry.Key, out textDamages))
                    textDamages.text = amountText;
                sumDamage += amount;
            }

            if (textAllDamages != null)
            {
                textAllDamages.gameObject.SetActive(!string.IsNullOrEmpty(text));
                textAllDamages.text = text;
            }

            if (textSumDamage != null)
                textSumDamage.text = string.Format(sumDamageFormat, sumDamage.min.ToString("N0"), sumDamage.max.ToString("N0"));
        }
    }
}
