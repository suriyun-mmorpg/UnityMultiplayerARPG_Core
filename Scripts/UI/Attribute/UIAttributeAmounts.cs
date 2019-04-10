using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIAttributeAmounts : UISelectionEntry<Dictionary<Attribute, short>>
    {
        [Tooltip("Attribute Amount Format => {0} = {Attribute title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public string amountFormat = "{0}: {1}/{2}";
        [Tooltip("Attribute Amount Format => {0} = {Attribute title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public string amountNotEnoughFormat = "{0}: <color=red>{1}/{2}</color>";
        [Tooltip("Attribute Amount Format without Current Amount => {0} = {Attribute title}, {1} = {Target Level}")]
        public string simpleAmountFormat = "{0}: {1}";

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UIAttributeTextPair[] textAmounts;
        public bool showAsRequirement;

        private Dictionary<Attribute, TextWrapper> cacheTextAmounts;
        public Dictionary<Attribute, TextWrapper> CacheTextAmounts
        {
            get
            {
                if (cacheTextAmounts == null)
                {
                    cacheTextAmounts = new Dictionary<Attribute, TextWrapper>();
                    foreach (UIAttributeTextPair textAmount in textAmounts)
                    {
                        if (textAmount.attribute == null || textAmount.uiText == null)
                            continue;
                        Attribute key = textAmount.attribute;
                        TextWrapper textComp = textAmount.uiText;
                        textComp.text = string.Format(amountFormat, key.Title, "0", "0");
                        cacheTextAmounts[key] = textComp;
                    }
                }
                return cacheTextAmounts;
            }
        }

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllAmounts != null)
                    uiTextAllAmounts.gameObject.SetActive(false);

                foreach (KeyValuePair<Attribute, TextWrapper> entry in CacheTextAmounts)
                {
                    entry.Value.text = string.Format(amountFormat, entry.Key.Title, "0", "0");
                }
            }
            else
            {
                string tempAllText = string.Empty;
                Attribute tempAttribute;
                short tempCurrentAmount;
                short tempTargetAmount;
                string tempFormat;
                string tempAmountText;
                TextWrapper tempTextWrapper;
                foreach (KeyValuePair<Attribute, short> dataEntry in Data)
                {
                    tempAttribute = dataEntry.Key;
                    tempTargetAmount = dataEntry.Value;
                    if (tempAttribute == null || tempTargetAmount == 0)
                        continue;
                    if (!string.IsNullOrEmpty(tempAllText))
                        tempAllText += "\n";
                    tempCurrentAmount = 0;
                    if (owningCharacter != null)
                        owningCharacter.CacheAttributes.TryGetValue(tempAttribute, out tempCurrentAmount);
                    if (showAsRequirement)
                    {
                        tempFormat = tempCurrentAmount >= tempTargetAmount ? amountFormat : amountNotEnoughFormat;
                        tempAmountText = string.Format(tempFormat, tempAttribute.Title, tempCurrentAmount.ToString("N0"), tempTargetAmount.ToString("N0"));
                    }
                    else
                    {
                        // This will show only target amount, so current character attribute amount will not be shown
                        tempAmountText = string.Format(simpleAmountFormat, tempAttribute.Title, tempTargetAmount.ToString("N0"));
                    }
                    tempAllText += tempAmountText;
                    if (CacheTextAmounts.TryGetValue(tempAttribute, out tempTextWrapper))
                        tempTextWrapper.text = tempAmountText;
                }
                if (uiTextAllAmounts != null)
                {
                    uiTextAllAmounts.gameObject.SetActive(!string.IsNullOrEmpty(tempAllText));
                    uiTextAllAmounts.text = tempAllText;
                }
            }
        }
    }
}
