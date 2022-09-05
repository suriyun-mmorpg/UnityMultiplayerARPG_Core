using System.Collections.Generic;
using Cysharp.Text;
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

        [Header("Options")]
        public DisplayType displayType;
        public bool includeEquipmentsForCurrentAmounts;
        public bool includeBuffsForCurrentAmounts;
        public bool includeSkillsForCurrentAmounts;
        public bool isBonus;
        public bool inactiveIfAmountZero;
        public bool useSimpleFormatIfAmountEnough = true;

        private Dictionary<Attribute, UIAttributeTextPair> cacheTextAmounts;
        public Dictionary<Attribute, UIAttributeTextPair> CacheTextAmounts
        {
            get
            {
                if (cacheTextAmounts == null)
                {
                    cacheTextAmounts = new Dictionary<Attribute, UIAttributeTextPair>();
                    Attribute tempData;
                    foreach (UIAttributeTextPair componentPair in textAmounts)
                    {
                        if (componentPair.attribute == null || componentPair.uiText == null)
                            continue;
                        tempData = componentPair.attribute;
                        SetDefaultValue(componentPair);
                        cacheTextAmounts[tempData] = componentPair;
                    }
                }
                return cacheTextAmounts;
            }
        }

        protected override void UpdateData()
        {
            // Reset number
            foreach (UIAttributeTextPair entry in CacheTextAmounts.Values)
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
                // Prepare attribute data
                IPlayerCharacterData character = GameInstance.PlayingCharacter;
                Dictionary<Attribute, float> currentAttributeAmounts = new Dictionary<Attribute, float>();
                if (character != null)
                    currentAttributeAmounts = character.GetAttributes(includeEquipmentsForCurrentAmounts, includeBuffsForCurrentAmounts, includeSkillsForCurrentAmounts ? character.GetSkills(includeEquipmentsForCurrentAmounts) : null);
                // In-loop temp data
                using (Utf16ValueStringBuilder tempAllText = ZString.CreateStringBuilder(false))
                {
                    Attribute tempData;
                    float tempCurrentAmount;
                    float tempTargetAmount;
                    bool tempAmountEnough;
                    string tempCurrentValue;
                    string tempTargetValue;
                    string tempFormat;
                    string tempAmountText;
                    UIAttributeTextPair tempComponentPair;
                    foreach (KeyValuePair<Attribute, float> dataEntry in Data)
                    {
                        if (dataEntry.Key == null)
                            continue;
                        // Set temp data
                        tempData = dataEntry.Key;
                        tempTargetAmount = dataEntry.Value;
                        tempCurrentAmount = 0;
                        // Get attribute amount
                        currentAttributeAmounts.TryGetValue(tempData, out tempCurrentAmount);
                        // Use difference format by option
                        switch (displayType)
                        {
                            case DisplayType.Rate:
                                // This will show only target amount, so current character attribute amount will not be shown
                                if (isBonus)
                                    tempTargetValue = (tempTargetAmount * 100).ToBonusString("N2");
                                else
                                    tempTargetValue = (tempTargetAmount * 100).ToString("N2");
                                tempAmountText = ZString.Format(
                                    LanguageManager.GetText(formatKeyRateAmount),
                                    tempData.Title,
                                    tempTargetValue);
                                break;
                            case DisplayType.Requirement:
                                // This will show both current character attribute amount and target amount
                                tempAmountEnough = tempCurrentAmount >= tempTargetAmount;
                                tempFormat = LanguageManager.GetText(tempAmountEnough ? formatKeyAmount : formatKeyAmountNotEnough);
                                tempCurrentValue = tempCurrentAmount.ToString("N0");
                                tempTargetValue = tempTargetAmount.ToString("N0");
                                if (useSimpleFormatIfAmountEnough && tempAmountEnough)
                                    tempAmountText = ZString.Format(LanguageManager.GetText(formatKeySimpleAmount), tempData.Title, tempTargetValue);
                                else
                                    tempAmountText = ZString.Format(tempFormat, tempData.Title, tempCurrentValue, tempTargetValue);
                                break;
                            default:
                                // This will show only target amount, so current character attribute amount will not be shown
                                if (isBonus)
                                    tempTargetValue = tempTargetAmount.ToBonusString("N0");
                                else
                                    tempTargetValue = tempTargetAmount.ToString("N0");
                                tempAmountText = ZString.Format(
                                    LanguageManager.GetText(formatKeySimpleAmount),
                                    tempData.Title,
                                    tempTargetValue);
                                break;
                        }
                        // Append current attribute amount text
                        if (dataEntry.Value != 0)
                        {
                            // Add new line if text is not empty
                            if (tempAllText.Length > 0)
                                tempAllText.Append('\n');
                            tempAllText.Append(tempAmountText);
                        }
                        // Set current attribute text to UI
                        if (CacheTextAmounts.TryGetValue(tempData, out tempComponentPair))
                        {
                            tempComponentPair.uiText.text = tempAmountText;
                            if (tempComponentPair.root != null)
                                tempComponentPair.root.SetActive(!inactiveIfAmountZero || tempTargetAmount != 0);
                        }
                    }

                    if (uiTextAllAmounts != null)
                    {
                        uiTextAllAmounts.SetGameObjectActive(tempAllText.Length > 0);
                        uiTextAllAmounts.text = tempAllText.ToString();
                    }
                }
            }
        }

        private void SetDefaultValue(UIAttributeTextPair componentPair)
        {
            switch (displayType)
            {
                case DisplayType.Rate:
                    componentPair.uiText.text = ZString.Format(
                        LanguageManager.GetText(formatKeyRateAmount),
                        componentPair.attribute.Title,
                        isBonus ? 0f.ToBonusString("N2") : 0f.ToString("N2"));
                    break;
                case DisplayType.Requirement:
                    if (useSimpleFormatIfAmountEnough)
                    {
                        componentPair.uiText.text = ZString.Format(
                            LanguageManager.GetText(formatKeySimpleAmount),
                            componentPair.attribute.Title,
                            "0");
                    }
                    else
                    {
                        componentPair.uiText.text = ZString.Format(
                            LanguageManager.GetText(formatKeyAmount),
                            componentPair.attribute.Title,
                            "0", "0");
                    }
                    break;
                case DisplayType.Simple:
                    componentPair.uiText.text = ZString.Format(
                        LanguageManager.GetText(formatKeySimpleAmount),
                        componentPair.attribute.Title,
                        isBonus ? 0f.ToBonusString("N0") : "0");
                    break;
            }
            if (componentPair.imageIcon != null)
                componentPair.imageIcon.sprite = componentPair.attribute.Icon;
            if (inactiveIfAmountZero && componentPair.root != null)
                componentPair.root.SetActive(false);
        }
    }
}
