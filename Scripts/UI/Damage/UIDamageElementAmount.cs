using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDamageElementAmount : UISelectionEntry<Tuple<DamageElement, MinMaxFloat>>
{
    [Tooltip("Damage Amount Format => {0} = {Element title}, {1} = {Min damage}, {2} = {Max damage}")]
    public string amountFormat = "{0}: {1}~{2}";

    [Header("UI Elements")]
    public Text textAmount;

    protected override void UpdateData()
    {
        if (textAmount != null)
        {
            var element = Data.Item1;
            var amount = Data.Item2;
            textAmount.text = string.Format(amountFormat, element.title, amount.min.ToString("N0"), amount.max.ToString("N0"));
        }
    }
}
