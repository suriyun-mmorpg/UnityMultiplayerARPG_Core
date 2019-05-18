using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIItemAmounts : UISelectionEntry<Dictionary<Item, short>>
    {
        /// <summary>
        /// Format => {0} = {Item Title}, {1} = {Current Amount}, {2} = {Target Amount}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Item Title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public string formatAmount = "{0}: {1}/{2}";
        /// <summary>
        /// Format => {0} = {Item Title}, {1} = {Current Amount}, {2} = {Target Amount}
        /// </summary>
        [Tooltip("Format => {0} = {Item Title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public string formatAmountNotEnough = "{0}: <color=red>{1}/{2}</color>";
        /// <summary>
        /// Format => {0} = {Item Title}, {1} = {Target Amount}
        /// </summary>
        [Tooltip("Format => {0} = {Item Title}, {1} = {Target Amount}")]
        public string formatSimpleAmount = "{0}: {1}";

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UIItemTextPair[] textAmounts;
        public bool showAsRequirement;

        private Dictionary<Item, TextWrapper> cacheTextLevels;
        public Dictionary<Item, TextWrapper> CacheTextLevels
        {
            get
            {
                if (cacheTextLevels == null)
                {
                    cacheTextLevels = new Dictionary<Item, TextWrapper>();
                    Item tempItem;
                    TextWrapper tempTextComponent;
                    foreach (UIItemTextPair textLevel in textAmounts)
                    {
                        if (textLevel.item == null || textLevel.uiText == null)
                            continue;
                        tempItem = textLevel.item;
                        tempTextComponent = textLevel.uiText;
                        tempTextComponent.text = string.Format(formatAmount, tempItem.Title, "0", "0");
                        cacheTextLevels[tempItem] = tempTextComponent;
                    }
                }
                return cacheTextLevels;
            }
        }

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllAmounts != null)
                    uiTextAllAmounts.gameObject.SetActive(false);

                foreach (KeyValuePair<Item, TextWrapper> textLevel in CacheTextLevels)
                {
                    textLevel.Value.text = string.Format(formatAmount, textLevel.Key.Title, "0", "0");
                }
            }
            else
            {
                string tempAllText = string.Empty;
                Item tempItem;
                int tempCurrentAmount;
                short tempTargetAmount;
                string tempFormat;
                string tempAmountText;
                TextWrapper tempTextWrapper;
                foreach (KeyValuePair<Item, short> dataEntry in Data)
                {
                    if (dataEntry.Key == null || dataEntry.Value == 0)
                        continue;
                    // Set temp data
                    tempItem = dataEntry.Key;
                    tempTargetAmount = dataEntry.Value;
                    tempCurrentAmount = 0;
                    // Add new line if text is not empty
                    if (!string.IsNullOrEmpty(tempAllText))
                        tempAllText += "\n";
                    // Get item amount from character
                    if (owningCharacter != null)
                        tempCurrentAmount = owningCharacter.CountNonEquipItems(tempItem.DataId);
                    // Use difference format by option 
                    if (showAsRequirement)
                    {
                        // This will show both current character item amount and target amount
                        tempFormat = tempCurrentAmount >= tempTargetAmount ? formatAmount : formatAmountNotEnough;
                        tempAmountText = string.Format(tempFormat, tempItem.Title, tempCurrentAmount.ToString("N0"), tempTargetAmount.ToString("N0"));
                    }
                    else
                    {
                        // This will show only target amount, so current character item amount will not be shown
                        tempAmountText = string.Format(formatSimpleAmount, tempItem.Title, tempTargetAmount.ToString("N0"));
                    }
                    // Append current attribute amount text
                    tempAllText += tempAmountText;
                    if (CacheTextLevels.TryGetValue(dataEntry.Key, out tempTextWrapper))
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
