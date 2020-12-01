using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICurrencyAmounts : UISelectionEntry<Dictionary<Currency, int>>
    {
        public enum DisplayType
        {
            Simple,
            Requirement
        }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Currency Title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_ATTRIBUTE);
        [Tooltip("Format => {0} = {Currency Title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public UILocaleKeySetting formatKeyAmountNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_ATTRIBUTE_NOT_ENOUGH);
        [Tooltip("Format => {0} = {Currency Title}, {1} = {Amount}")]
        public UILocaleKeySetting formatKeySimpleAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTRIBUTE_AMOUNT);

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UICurrencyTextPair[] textAmounts;
        public DisplayType displayType;
        public bool isBonus;

        private Dictionary<Currency, TextWrapper> cacheTextAmounts;
        public Dictionary<Currency, TextWrapper> CacheTextAmounts
        {
            get
            {
                if (cacheTextAmounts == null)
                {
                    cacheTextAmounts = new Dictionary<Currency, TextWrapper>();
                    Currency tempCurrency;
                    TextWrapper tempTextComponent;
                    foreach (UICurrencyTextPair textAmount in textAmounts)
                    {
                        if (textAmount.currency == null || textAmount.uiText == null)
                            continue;
                        tempCurrency = textAmount.currency;
                        tempTextComponent = textAmount.uiText;
                        SetDefaultText(tempTextComponent, tempCurrency.Title);
                        cacheTextAmounts[tempCurrency] = tempTextComponent;
                    }
                }
                return cacheTextAmounts;
            }
        }

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            // Reset number
            foreach (KeyValuePair<Currency, TextWrapper> entry in CacheTextAmounts)
            {
                SetDefaultText(entry.Value, entry.Key.Title);
            }
            // Set number by updated data
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllAmounts != null)
                    uiTextAllAmounts.gameObject.SetActive(false);
            }
            else
            {
                string tempAllText = string.Empty;
                Currency tempCurrency;
                int tempCurrentAmount;
                int tempTargetAmount;
                string tempCurrentValue;
                string tempTargetValue;
                string tempFormat;
                string tempAmountText;
                TextWrapper tempTextWrapper;
                foreach (KeyValuePair<Currency, int> dataEntry in Data)
                {
                    if (dataEntry.Key == null)
                        continue;
                    // Set temp data
                    tempCurrency = dataEntry.Key;
                    tempTargetAmount = dataEntry.Value;
                    tempCurrentAmount = 0;
                    // Get currency amount from character
                    if (owningCharacter != null)
                    {
                        int indexOfCurrency = owningCharacter.IndexOfCurrency(tempCurrency.DataId);
                        if (indexOfCurrency >= 0)
                            tempCurrentAmount = owningCharacter.Currencies[indexOfCurrency].amount;
                    }
                    // Use difference format by option 
                    switch (displayType)
                    {
                        case DisplayType.Requirement:
                            // This will show both current character currency amount and target amount
                            tempFormat = tempCurrentAmount >= tempTargetAmount ?
                                LanguageManager.GetText(formatKeyAmount) :
                                LanguageManager.GetText(formatKeyAmountNotEnough);
                            tempCurrentValue = tempCurrentAmount.ToString("N0");
                            tempTargetValue = tempTargetAmount.ToString("N0");
                            tempAmountText = string.Format(tempFormat, tempCurrency.Title, tempCurrentValue, tempTargetValue);
                            break;
                        default:
                            // This will show only target amount, so current character currency amount will not be shown
                            if (isBonus)
                                tempTargetValue = tempTargetAmount.ToBonusString("N0");
                            else
                                tempTargetValue = tempTargetAmount.ToString("N0");
                            tempAmountText = string.Format(
                                LanguageManager.GetText(formatKeySimpleAmount),
                                tempCurrency.Title,
                                tempTargetValue);
                            break;
                    }
                    // Append current currency amount text
                    if (dataEntry.Value != 0)
                    {
                        // Add new line if text is not empty
                        if (!string.IsNullOrEmpty(tempAllText))
                            tempAllText += "\n";
                        tempAllText += tempAmountText;
                    }
                    // Set current currency text to UI
                    if (CacheTextAmounts.TryGetValue(tempCurrency, out tempTextWrapper))
                        tempTextWrapper.text = tempAmountText;
                }

                if (uiTextAllAmounts != null)
                {
                    uiTextAllAmounts.gameObject.SetActive(!string.IsNullOrEmpty(tempAllText));
                    uiTextAllAmounts.text = tempAllText;
                }
            }
        }

        private void SetDefaultText(TextWrapper text, string title)
        {
            switch (displayType)
            {
                case DisplayType.Requirement:
                    text.text = string.Format(
                        LanguageManager.GetText(formatKeyAmount),
                        title,
                        0f.ToString("N0"), 0f.ToString("N0"));
                    break;
                case DisplayType.Simple:
                    text.text = string.Format(
                        LanguageManager.GetText(formatKeySimpleAmount),
                        title,
                        isBonus ? 0f.ToBonusString("N0") : 0f.ToString("N0"));
                    break;
            }
        }
    }
}
