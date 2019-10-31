using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIResistanceAmounts : UISelectionEntry<Dictionary<DamageElement, float>>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Resistance Title}, {1} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_RESISTANCE_AMOUNT);

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UIResistanceTextPair[] textAmounts;
        public bool isBonus;

        private Dictionary<DamageElement, TextWrapper> cacheTextAmounts;
        public Dictionary<DamageElement, TextWrapper> CacheTextAmounts
        {
            get
            {
                if (cacheTextAmounts == null)
                {
                    cacheTextAmounts = new Dictionary<DamageElement, TextWrapper>();
                    DamageElement tempElement;
                    TextWrapper tempTextComponent;
                    foreach (UIResistanceTextPair textAmount in textAmounts)
                    {
                        if (textAmount.uiText == null)
                            continue;
                        tempElement = textAmount.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : textAmount.damageElement;
                        tempTextComponent = textAmount.uiText;
                        tempTextComponent.text = string.Format(
                            LanguageManager.GetText(formatKeyAmount),
                            tempElement.Title,
                            isBonus ? "+0" : "0");
                        cacheTextAmounts[tempElement] = tempTextComponent;
                    }
                }
                return cacheTextAmounts;
            }
        }

        protected override void UpdateData()
        {
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllAmounts != null)
                    uiTextAllAmounts.gameObject.SetActive(false);

                foreach (KeyValuePair<DamageElement, TextWrapper> entry in CacheTextAmounts)
                {
                    entry.Value.text = string.Format(
                            LanguageManager.GetText(formatKeyAmount),
                            entry.Key.Title,
                            isBonus ? "+0" : "0");
                }
            }
            else
            {
                string tempAllText = string.Empty;
                DamageElement tempElement;
                float tempAmount;
                string tempValue;
                string tempAmountText;
                TextWrapper tempTextWarpper;
                foreach (KeyValuePair<DamageElement, float> dataEntry in Data)
                {
                    if (dataEntry.Key == null || dataEntry.Value == 0)
                        continue;
                    // Set temp data
                    tempElement = dataEntry.Key;
                    tempAmount = dataEntry.Value;
                    // Add new line if text is not empty
                    if (!string.IsNullOrEmpty(tempAllText))
                        tempAllText += "\n";
                    // Set current elemental resistance text
                    if (isBonus)
                        tempValue = (tempAmount * 100).ToBonusString("N2");
                    else
                        tempValue = (tempAmount * 100).ToString("N2");
                    tempAmountText = string.Format(
                        LanguageManager.GetText(formatKeyAmount),
                        tempElement.Title,
                        tempValue);
                    // Append current elemental resistance text
                    tempAllText += tempAmountText;
                    // Set current elemental resistance text to UI
                    if (CacheTextAmounts.TryGetValue(dataEntry.Key, out tempTextWarpper))
                        tempTextWarpper.text = tempAmountText;
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
