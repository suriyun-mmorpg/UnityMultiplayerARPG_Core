using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UIItemAmounts : UISelectionEntry<Dictionary<Item, short>>
    {
        [Tooltip("Item Level Format => {0} = {Item title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public string amountFormat = "{0}: {1}/{2}";
        [Tooltip("Item Level Format => {0} = {Item title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public string amountNotReachTargetFormat = "{0}: <color=red>{1}/{2}</color>";

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UIItemTextPair[] textAmounts;

        private Dictionary<Item, TextWrapper> cacheTextLevels;
        public Dictionary<Item, TextWrapper> CacheTextLevels
        {
            get
            {
                if (cacheTextLevels == null)
                {
                    cacheTextLevels = new Dictionary<Item, TextWrapper>();
                    foreach (var textLevel in textAmounts)
                    {
                        if (textLevel.item == null || textLevel.uiText == null)
                            continue;
                        var key = textLevel.item;
                        var textComp = textLevel.uiText;
                        textComp.text = string.Format(amountFormat, key.title, "0", "0");
                        cacheTextLevels[key] = textComp;
                    }
                }
                return cacheTextLevels;
            }
        }

        protected override void UpdateData()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllAmounts != null)
                    uiTextAllAmounts.gameObject.SetActive(false);

                foreach (var textLevel in CacheTextLevels)
                {
                    var element = textLevel.Key;
                    textLevel.Value.text = string.Format(amountFormat, element.title, "0", "0");
                }
            }
            else
            {
                var text = "";
                foreach (var dataEntry in Data)
                {
                    var item = dataEntry.Key;
                    var targetAmount = dataEntry.Value;
                    if (dataEntry.Key == null || targetAmount == 0)
                        continue;
                    if (!string.IsNullOrEmpty(text))
                        text += "\n";
                    var currentAmount = 0;
                    if (owningCharacter != null)
                        currentAmount = owningCharacter.CountNonEquipItems(item.DataId);
                    var format = currentAmount >= targetAmount ? amountFormat : amountNotReachTargetFormat;
                    var amountText = string.Format(format, dataEntry.Key.title, currentAmount.ToString("N0"), targetAmount.ToString("N0"));
                    text += amountText;
                    TextWrapper cacheTextAmount;
                    if (CacheTextLevels.TryGetValue(dataEntry.Key, out cacheTextAmount))
                        cacheTextAmount.text = amountText;
                }
                if (uiTextAllAmounts != null)
                {
                    uiTextAllAmounts.gameObject.SetActive(!string.IsNullOrEmpty(text));
                    uiTextAllAmounts.text = text;
                }
            }
        }
    }
}
