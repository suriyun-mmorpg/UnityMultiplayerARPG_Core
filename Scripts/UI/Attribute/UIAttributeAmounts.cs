using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIAttributeAmounts : UISelectionEntry<Dictionary<Attribute, float>>
    {
        public enum DisplayType
        {
            Simple,
            Rate,
            Requirement
        }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Attribute Title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_ATTRIBUTE);
        [Tooltip("Format => {0} = {Attribute Title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public UILocaleKeySetting formatKeyAmountNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_ATTRIBUTE_NOT_ENOUGH);
        [Tooltip("Format => {0} = {Attribute Title}, {1} = {Amount}")]
        public UILocaleKeySetting formatKeySimpleAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTRIBUTE_AMOUNT);
        [Tooltip("Format => {0} = {Attribute Title}, {1} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyRateAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTRIBUTE_RATE);

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UIAttributeTextPair[] textAmounts;
        public DisplayType displayType;

        private Dictionary<Attribute, TextWrapper> cacheTextAmounts;
        public Dictionary<Attribute, TextWrapper> CacheTextAmounts
        {
            get
            {
                if (cacheTextAmounts == null)
                {
                    cacheTextAmounts = new Dictionary<Attribute, TextWrapper>();
                    Attribute tempAttribute;
                    TextWrapper tempTextComponent;
                    foreach (UIAttributeTextPair textAmount in textAmounts)
                    {
                        if (textAmount.attribute == null || textAmount.uiText == null)
                            continue;
                        tempAttribute = textAmount.attribute;
                        tempTextComponent = textAmount.uiText;
                        tempTextComponent.text = string.Format(
                            LanguageManager.GetText(formatKeyAmount),
                            tempAttribute.Title,
                            "0",
                            "0");
                        cacheTextAmounts[tempAttribute] = tempTextComponent;
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
                    entry.Value.text = string.Format(
                        LanguageManager.GetText(formatKeyAmount),
                        entry.Key.Title,
                        "0",
                        "0");
                }
            }
            else
            {
                string tempAllText = string.Empty;
                Attribute tempAttribute;
                float tempCurrentAmount;
                float tempTargetAmount;
                string tempFormat;
                string tempAmountText;
                TextWrapper tempTextWrapper;
                foreach (KeyValuePair<Attribute, float> dataEntry in Data)
                {
                    if (dataEntry.Key == null || dataEntry.Value == 0)
                        continue;
                    // Set temp data
                    tempAttribute = dataEntry.Key;
                    tempTargetAmount = dataEntry.Value;
                    tempCurrentAmount = 0;
                    // Add new line if text is not empty
                    if (!string.IsNullOrEmpty(tempAllText))
                        tempAllText += "\n";
                    // Get attribute amount from character
                    if (owningCharacter != null)
                        owningCharacter.GetCaches().Attributes.TryGetValue(tempAttribute, out tempCurrentAmount);
                    // Use difference format by option 
                    switch (displayType)
                    {
                        case DisplayType.Rate:
                            // This will show only target amount, so current character attribute amount will not be shown
                            tempAmountText = string.Format(
                                LanguageManager.GetText(formatKeyRateAmount),
                                tempAttribute.Title,
                                (tempTargetAmount * 100).ToString("N2"));
                            break;
                        case DisplayType.Requirement:
                            // This will show both current character attribute amount and target amount
                            tempFormat = tempCurrentAmount >= tempTargetAmount ?
                                LanguageManager.GetText(formatKeyAmount) :
                                LanguageManager.GetText(formatKeyAmountNotEnough);
                            tempAmountText = string.Format(tempFormat, tempAttribute.Title, tempCurrentAmount.ToString("N0"), tempTargetAmount.ToString("N0"));
                            break;
                        default:
                            // This will show only target amount, so current character attribute amount will not be shown
                            tempAmountText = string.Format(
                                LanguageManager.GetText(formatKeySimpleAmount),
                                tempAttribute.Title,
                                tempTargetAmount.ToString("N0"));
                            break;
                    }
                    // Append current attribute amount text
                    tempAllText += tempAmountText;
                    // Set current attribute text to UI
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
