using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIItemAmounts : UISelectionEntry<Dictionary<BaseItem, short>>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Item Title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_ITEM);
        [Tooltip("Format => {0} = {Item Title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public UILocaleKeySetting formatKeyAmountNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_ITEM_NOT_ENOUGH);
        [Tooltip("Format => {0} = {Item Title}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeySimpleAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_AMOUNT);

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UIItemTextPair[] textAmounts;
        public bool showAsRequirement;

        private Dictionary<BaseItem, UIItemTextPair> cacheTextLevels;
        public Dictionary<BaseItem, UIItemTextPair> CacheTextLevels
        {
            get
            {
                if (cacheTextLevels == null)
                {
                    cacheTextLevels = new Dictionary<BaseItem, UIItemTextPair>();
                    BaseItem tempItem;
                    foreach (UIItemTextPair componentPair in textAmounts)
                    {
                        if (componentPair.item == null || componentPair.uiText == null)
                            continue;
                        tempItem = componentPair.item;
                        SetDefaultValue(componentPair);
                        cacheTextLevels[tempItem] = componentPair;
                    }
                }
                return cacheTextLevels;
            }
        }

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            // Reset number
            foreach (UIItemTextPair entry in CacheTextLevels.Values)
            {
                SetDefaultValue(entry);
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
                BaseItem tempItem;
                int tempCurrentAmount;
                short tempTargetAmount;
                string tempFormat;
                string tempAmountText;
                UIItemTextPair tempComponentPair;
                foreach (KeyValuePair<BaseItem, short> dataEntry in Data)
                {
                    if (dataEntry.Key == null)
                        continue;
                    // Set temp data
                    tempItem = dataEntry.Key;
                    tempTargetAmount = dataEntry.Value;
                    tempCurrentAmount = 0;
                    // Get item amount from character
                    if (owningCharacter != null)
                        tempCurrentAmount = owningCharacter.CountNonEquipItems(tempItem.DataId);
                    // Use difference format by option 
                    if (showAsRequirement)
                    {
                        // This will show both current character item amount and target amount
                        tempFormat = tempCurrentAmount >= tempTargetAmount ?
                            LanguageManager.GetText(formatKeyAmount) :
                            LanguageManager.GetText(formatKeyAmountNotEnough);
                        tempAmountText = string.Format(tempFormat, tempItem.Title, tempCurrentAmount.ToString("N0"), tempTargetAmount.ToString("N0"));
                    }
                    else
                    {
                        // This will show only target amount, so current character item amount will not be shown
                        tempAmountText = string.Format(
                            LanguageManager.GetText(formatKeySimpleAmount),
                            tempItem.Title,
                            tempTargetAmount.ToString("N0"));
                    }
                    // Append current item amount text
                    if (dataEntry.Value != 0)
                    {
                        // Add new line if text is not empty
                        if (!string.IsNullOrEmpty(tempAllText))
                            tempAllText += "\n";
                        tempAllText += tempAmountText;
                    }
                    // Set current item text to UI
                    if (CacheTextLevels.TryGetValue(dataEntry.Key, out tempComponentPair))
                        tempComponentPair.uiText.text = tempAmountText;
                }

                if (uiTextAllAmounts != null)
                {
                    uiTextAllAmounts.gameObject.SetActive(!string.IsNullOrEmpty(tempAllText));
                    uiTextAllAmounts.text = tempAllText;
                }
            }
        }

        private void SetDefaultValue(UIItemTextPair componentPair)
        {
            componentPair.uiText.text = string.Format(
                LanguageManager.GetText(formatKeyAmount),
                componentPair.item.Title,
                0.ToString("N0"),
                0.ToString("N0"));
            if (componentPair.imageIcon != null)
                componentPair.imageIcon.sprite = componentPair.item.icon;
        }
    }
}
