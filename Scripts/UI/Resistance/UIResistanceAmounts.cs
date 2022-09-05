using System.Collections.Generic;
using Cysharp.Text;
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

        private Dictionary<DamageElement, UIResistanceTextPair> cacheTextAmounts;
        public Dictionary<DamageElement, UIResistanceTextPair> CacheTextAmounts
        {
            get
            {
                if (cacheTextAmounts == null)
                {
                    cacheTextAmounts = new Dictionary<DamageElement, UIResistanceTextPair>();
                    DamageElement tempElement;
                    foreach (UIResistanceTextPair componentPair in textAmounts)
                    {
                        if (componentPair.uiText == null)
                            continue;
                        tempElement = componentPair.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : componentPair.damageElement;
                        SetDefaultValue(componentPair);
                        cacheTextAmounts[tempElement] = componentPair;
                    }
                }
                return cacheTextAmounts;
            }
        }

        protected override void UpdateData()
        {
            // Reset number
            foreach (UIResistanceTextPair entry in CacheTextAmounts.Values)
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
                    DamageElement tempElement;
                    float tempAmount;
                    string tempValue;
                    string tempAmountText;
                    UIResistanceTextPair tempComponentPair;
                    foreach (KeyValuePair<DamageElement, float> dataEntry in Data)
                    {
                        if (dataEntry.Key == null)
                            continue;
                        // Set temp data
                        tempElement = dataEntry.Key;
                        tempAmount = dataEntry.Value;
                        // Set current elemental resistance text
                        if (isBonus)
                            tempValue = (tempAmount * 100).ToBonusString("N2");
                        else
                            tempValue = (tempAmount * 100).ToString("N2");
                        tempAmountText = ZString.Format(
                            LanguageManager.GetText(formatKeyAmount),
                            tempElement.Title,
                            tempValue);
                        // Append current elemental resistance text
                        if (dataEntry.Value != 0)
                        {
                            // Add new line if text is not empty
                            if (tempAllText.Length > 0)
                                tempAllText.Append('\n');
                            tempAllText.Append(tempAmountText);
                        }
                        // Set current elemental resistance text to UI
                        if (CacheTextAmounts.TryGetValue(dataEntry.Key, out tempComponentPair))
                            tempComponentPair.uiText.text = tempAmountText;
                    }

                    if (uiTextAllAmounts != null)
                    {
                        uiTextAllAmounts.SetGameObjectActive(tempAllText.Length > 0);
                        uiTextAllAmounts.text = tempAllText.ToString();
                    }
                }
            }
        }

        private void SetDefaultValue(UIResistanceTextPair componentPair)
        {
            DamageElement tempElement = componentPair.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : componentPair.damageElement;
            componentPair.uiText.text = ZString.Format(
                    LanguageManager.GetText(formatKeyAmount),
                    tempElement.Title,
                    isBonus ? 0f.ToBonusString("N2") : 0f.ToString("N2"));
            if (componentPair.imageIcon != null)
                componentPair.imageIcon.sprite = tempElement.Icon;
        }
    }
}
