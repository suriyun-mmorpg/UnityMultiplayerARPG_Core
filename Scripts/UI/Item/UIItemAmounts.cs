using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIItemAmounts : UISelectionEntry<Dictionary<BaseItem, int>>
    {
        public enum DisplayType
        {
            Simple,
            Requirement
        }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Item Title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_ITEM);
        [Tooltip("Format => {0} = {Item Title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public UILocaleKeySetting formatKeyAmountNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_ITEM_NOT_ENOUGH);
        [Tooltip("Format => {0} = {Item Title}, {1} = {Amount}")]
        public UILocaleKeySetting formatKeySimpleAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_AMOUNT);

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UIItemTextPair[] textAmounts;

        [Header("Options")]
        public DisplayType displayType;
        public bool isBonus;
        public bool inactiveIfAmountZero;
        public bool useSimpleFormatIfAmountEnough = true;

        private Dictionary<BaseItem, UIItemTextPair> cacheTextAmounts;
        public Dictionary<BaseItem, UIItemTextPair> CacheTextAmounts
        {
            get
            {
                if (cacheTextAmounts == null)
                {
                    cacheTextAmounts = new Dictionary<BaseItem, UIItemTextPair>();
                    BaseItem tempData;
                    foreach (UIItemTextPair componentPair in textAmounts)
                    {
                        if (componentPair.item == null || componentPair.uiText == null)
                            continue;
                        tempData = componentPair.item;
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
            foreach (UIItemTextPair entry in CacheTextAmounts.Values)
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
                using (Utf16ValueStringBuilder tempAllText = ZString.CreateStringBuilder(false))
                {
                    BaseItem tempData;
                    int tempCurrentAmount;
                    int tempTargetAmount;
                    bool tempAmountEnough;
                    string tempCurrentValue;
                    string tempTargetValue;
                    string tempFormat;
                    string tempAmountText;
                    UIItemTextPair tempComponentPair;
                    foreach (KeyValuePair<BaseItem, int> dataEntry in Data)
                    {
                        if (dataEntry.Key == null)
                            continue;
                        // Set temp data
                        tempData = dataEntry.Key;
                        tempTargetAmount = dataEntry.Value;
                        tempCurrentAmount = 0;
                        // Get item amount from character
                        if (GameInstance.PlayingCharacter != null)
                            tempCurrentAmount = GameInstance.PlayingCharacter.CountNonEquipItems(tempData.DataId);
                        // Use difference format by option 
                        switch (displayType)
                        {
                            case DisplayType.Requirement:
                                // This will show both current character item amount and target amount
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
                                // This will show only target amount, so current character item amount will not be shown
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
                        // Append current item amount text
                        if (dataEntry.Value != 0)
                        {
                            // Add new line if text is not empty
                            if (tempAllText.Length > 0)
                                tempAllText.Append('\n');
                            tempAllText.Append(tempAmountText);
                        }
                        // Set current item text to UI
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

        private void SetDefaultValue(UIItemTextPair componentPair)
        {
            switch (displayType)
            {
                case DisplayType.Requirement:
                    if (useSimpleFormatIfAmountEnough)
                    {
                        componentPair.uiText.text = ZString.Format(
                            LanguageManager.GetText(formatKeySimpleAmount),
                            componentPair.item.Title,
                            "0");
                    }
                    else
                    {
                        componentPair.uiText.text = ZString.Format(
                            LanguageManager.GetText(formatKeyAmount),
                            componentPair.item.Title,
                            "0", "0");
                    }
                    break;
                case DisplayType.Simple:
                    componentPair.uiText.text = ZString.Format(
                        LanguageManager.GetText(formatKeySimpleAmount),
                        componentPair.item.Title,
                        isBonus ? 0f.ToBonusString("N0") : "0");
                    break;
            }
            if (componentPair.imageIcon != null)
                componentPair.imageIcon.sprite = componentPair.item.Icon;
            if (inactiveIfAmountZero && componentPair.root != null)
                componentPair.root.SetActive(false);
        }
    }
}
