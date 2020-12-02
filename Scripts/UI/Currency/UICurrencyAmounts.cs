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
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_CURRENCY);
        [Tooltip("Format => {0} = {Currency Title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public UILocaleKeySetting formatKeyAmountNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_CURRENCY_NOT_ENOUGH);
        [Tooltip("Format => {0} = {Currency Title}, {1} = {Amount}")]
        public UILocaleKeySetting formatKeySimpleAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENCY_AMOUNT);

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UICurrencyTextPair[] textAmounts;
        public DisplayType displayType;
        public bool isBonus;

        private Dictionary<Currency, UICurrencyTextPair> cacheTextAmounts;
        public Dictionary<Currency, UICurrencyTextPair> CacheTextAmounts
        {
            get
            {
                if (cacheTextAmounts == null)
                {
                    cacheTextAmounts = new Dictionary<Currency, UICurrencyTextPair>();
                    Currency tempCurrency;
                    foreach (UICurrencyTextPair componentPair in textAmounts)
                    {
                        if (componentPair.currency == null || componentPair.uiText == null)
                            continue;
                        tempCurrency = componentPair.currency;
                        SetDefaultValue(componentPair);
                        cacheTextAmounts[tempCurrency] = componentPair;
                    }
                }
                return cacheTextAmounts;
            }
        }

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            // Reset number
            foreach (UICurrencyTextPair entry in CacheTextAmounts.Values)
            {
                SetDefaultValue(entry);
            }
            // Set number by updated data
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllAmounts != null)
                    uiTextAllAmounts.SetGameObjectActive(false);
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
                UICurrencyTextPair tempComponentPair;
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
                    if (CacheTextAmounts.TryGetValue(tempCurrency, out tempComponentPair))
                        tempComponentPair.uiText.text = tempAmountText;
                }

                if (uiTextAllAmounts != null)
                {
                    uiTextAllAmounts.SetGameObjectActive(!string.IsNullOrEmpty(tempAllText));
                    uiTextAllAmounts.text = tempAllText;
                }
            }
        }

        private void SetDefaultValue(UICurrencyTextPair componentPair)
        {
            switch (displayType)
            {
                case DisplayType.Requirement:
                    componentPair.uiText.text = string.Format(
                        LanguageManager.GetText(formatKeyAmount),
                        componentPair.currency.title,
                        0f.ToString("N0"), 0f.ToString("N0"));
                    break;
                case DisplayType.Simple:
                    componentPair.uiText.text = string.Format(
                        LanguageManager.GetText(formatKeySimpleAmount),
                        componentPair.currency.title,
                        isBonus ? 0f.ToBonusString("N0") : 0f.ToString("N0"));
                    break;
            }
            if (componentPair.imageIcon != null)
                componentPair.imageIcon.sprite = componentPair.currency.icon;
        }
    }
}
