using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDamageElementAmounts : UISelectionEntry<Dictionary<DamageElement, MinMaxFloat>>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Damage Element Title}, {1} = {Min Damage}, {2} = {Max Damage}")]
        public UILocaleKeySetting formatKeyDamage = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_DAMAGE_WITH_ELEMENTAL);
        [Tooltip("Format => {0} = {Min Damage}, {1} = {Max Damage}")]
        public UILocaleKeySetting formatKeySumDamage = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_DAMAGE_AMOUNT);

        [Header("UI Elements")]
        public TextWrapper uiTextAllDamages;
        public TextWrapper uiTextSumDamage;
        public UIDamageElementTextPair[] textDamages;
        public bool isBonus;

        private Dictionary<DamageElement, TextWrapper> cacheTextDamages;
        public Dictionary<DamageElement, TextWrapper> CacheTextDamages
        {
            get
            {
                if (cacheTextDamages == null)
                {
                    cacheTextDamages = new Dictionary<DamageElement, TextWrapper>();
                    DamageElement tempElement;
                    TextWrapper tempTextComponent;
                    foreach (UIDamageElementTextPair textAmount in textDamages)
                    {
                        if (textAmount.uiText == null)
                            continue;
                        tempElement = textAmount.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : textAmount.damageElement;
                        tempTextComponent = textAmount.uiText;
                        tempTextComponent.text = string.Format(
                            LanguageManager.GetText(formatKeyDamage),
                            tempElement.Title,
                            isBonus ? "+0" : "0",
                            "0");
                        cacheTextDamages[tempElement] = tempTextComponent;
                    }
                }
                return cacheTextDamages;
            }
        }

        protected override void UpdateData()
        {
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllDamages != null)
                    uiTextAllDamages.gameObject.SetActive(false);

                if (uiTextSumDamage != null)
                {
                    uiTextSumDamage.text = string.Format(
                        LanguageManager.GetText(formatKeySumDamage),
                        isBonus ? "+0" : "0",
                        "0");
                }

                foreach (KeyValuePair<DamageElement, TextWrapper> entry in CacheTextDamages)
                {
                    entry.Value.text = string.Format(
                        LanguageManager.GetText(formatKeyDamage),
                        entry.Key.Title,
                        isBonus ? "+0" : "0",
                        "0");
                }
            }
            else
            {
                string tempAllText = string.Empty;
                MinMaxFloat sumDamage = new MinMaxFloat();
                DamageElement tempElement;
                MinMaxFloat tempAmount;
                string tempMinValue;
                string tempMaxValue;
                string tempAmountText;
                TextWrapper tempTextWrapper;
                foreach (KeyValuePair<DamageElement, MinMaxFloat> dataEntry in Data)
                {
                    if (dataEntry.Key == null || (dataEntry.Value.min == 0 && dataEntry.Value.max == 0))
                        continue;
                    // Set temp data
                    tempElement = dataEntry.Key;
                    tempAmount = dataEntry.Value;
                    // Add new line if text is not empty
                    if (!string.IsNullOrEmpty(tempAllText))
                        tempAllText += "\n";
                    // Set current elemental damage text
                    if (isBonus)
                        tempMinValue = tempAmount.min.ToBonusString("N0");
                    else
                        tempMinValue = tempAmount.min.ToString("N0");
                    tempMaxValue = tempAmount.max.ToString("N0");
                    tempAmountText = string.Format(
                        LanguageManager.GetText(formatKeyDamage),
                        tempElement.Title,
                        tempMinValue,
                        tempMaxValue);
                    // Append current elemental damage text
                    tempAllText += tempAmountText;
                    // Set current elemental damage text to UI
                    if (CacheTextDamages.TryGetValue(dataEntry.Key, out tempTextWrapper))
                        tempTextWrapper.text = tempAmountText;
                    sumDamage += tempAmount;
                }

                if (uiTextAllDamages != null)
                {
                    uiTextAllDamages.gameObject.SetActive(!string.IsNullOrEmpty(tempAllText));
                    uiTextAllDamages.text = tempAllText;
                }

                if (uiTextSumDamage != null)
                {
                    if (isBonus)
                        tempMinValue = sumDamage.min.ToBonusString("N0");
                    else
                        tempMinValue = sumDamage.min.ToString("N0");
                    tempMaxValue = sumDamage.max.ToString("N0");
                    uiTextSumDamage.text = string.Format(
                        LanguageManager.GetText(formatKeySumDamage),
                        tempMinValue,
                        tempMaxValue);
                }
            }
        }
    }
}
