using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDamageElementAmounts : UISelectionEntry<Dictionary<DamageElement, DamageAmount>>
{
    [Tooltip("Damage Amount Format => {0} = {Element title}, {1} = {Min damage}, {2} = {Max damage}")]
    public string damageFormat = "{0}: {1}~{2}";
    [Tooltip("Average Damage Amount Format => {0} = {Min damage}, {1} = {Max damage}")]
    public string averageDamageFormat = "{0}~{1}";

    [Header("UI Elements")]
    public Text textAllDamages;
    public Text textAverageDamage;
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

            if (textAverageDamage != null)
                textAverageDamage.text = string.Format(averageDamageFormat, "0", "0");

            foreach (var textAmount in CacheTextDamages)
            {
                var element = textAmount.Key;
                textAmount.Value.text = string.Format(damageFormat, element.title, "0", "0");
            }
        }
        else
        {
            var text = "";
            var averageMinDamage = 0f;
            var averageMaxDamage = 0f;
            foreach (var dataEntry in Data)
            {
                var element = dataEntry.Key;
                var amount = dataEntry.Value;
                var amountText = string.Format(damageFormat, element.title, amount.minDamage.ToString("N0"), amount.maxDamage.ToString("N0"));
                text += amountText + "\n";
                if (CacheTextDamages.ContainsKey(dataEntry.Key))
                    CacheTextDamages[dataEntry.Key].text = amountText;
                averageMinDamage += amount.minDamage;
                averageMaxDamage += amount.maxDamage;
            }

            averageMinDamage /= Data.Count;
            averageMaxDamage /= Data.Count;

            if (textAllDamages != null)
            {
                textAllDamages.gameObject.SetActive(!string.IsNullOrEmpty(text));
                textAllDamages.text = text;
            }

            if (textAverageDamage != null)
                textAverageDamage.text = string.Format(averageDamageFormat, averageMinDamage.ToString("N0"), averageMaxDamage.ToString("N0"));
        }
    }
}
