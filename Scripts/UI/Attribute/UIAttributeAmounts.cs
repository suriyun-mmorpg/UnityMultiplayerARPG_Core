using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIAttributeAmounts : UISelectionEntry<Dictionary<Attribute, short>>
    {
        [Tooltip("Attribute Amount Format => {0} = {Attribute title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public string amountFormat = "{0}: {1}/{2}";
        [Tooltip("Attribute Amount Format => {0} = {Attribute title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public string amountNotReachTargetFormat = "{0}: <color=red>{1}/{2}</color>";

        [Header("UI Elements")]
        public Text textAllAmounts;
        public UIAttributeTextPair[] textAmounts;

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
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
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
                    var attribute = dataEntry.Key;
                    var targetAmount = dataEntry.Value;
                    if (attribute == null || targetAmount == 0)
                        continue;
                    if (!string.IsNullOrEmpty(text))
                        text += "\n";
                    short currentAmount = 0;
                    if (owningCharacter != null)
                        owningCharacter.CacheAttributes.TryGetValue(attribute, out currentAmount);
                    var format = currentAmount >= targetAmount ? amountFormat : amountNotReachTargetFormat;
                    var amountText = string.Format(format, attribute.title, currentAmount.ToString("N0"), targetAmount.ToString("N0"));
                    text += amountText;
                    Text cacheTextAmount;
                    if (CacheTextAmounts.TryGetValue(attribute, out cacheTextAmount))
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
}
