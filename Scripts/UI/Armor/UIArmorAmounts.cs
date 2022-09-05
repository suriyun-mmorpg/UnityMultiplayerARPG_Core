using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIArmorAmounts : UISelectionEntry<Dictionary<DamageElement, float>>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Armor Title}, {1} = {Amount}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ARMOR_AMOUNT);

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UIArmorTextPair[] textAmounts;

        [Header("Options")]
        public bool isBonus;
        public bool inactiveIfAmountZero;

        private Dictionary<DamageElement, UIArmorTextPair> cacheTextAmounts;
        public Dictionary<DamageElement, UIArmorTextPair> CacheTextAmounts
        {
            get
            {
                if (cacheTextAmounts == null)
                {
                    cacheTextAmounts = new Dictionary<DamageElement, UIArmorTextPair>();
                    DamageElement tempElement;
                    foreach (UIArmorTextPair componentPair in textAmounts)
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
            foreach (UIArmorTextPair entry in CacheTextAmounts.Values)
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
                    UIArmorTextPair tempComponentPair;
                    foreach (KeyValuePair<DamageElement, float> dataEntry in Data)
                    {
                        if (dataEntry.Key == null)
                            continue;
                        // Set temp data
                        tempElement = dataEntry.Key;
                        tempAmount = dataEntry.Value;
                        // Set current elemental armor text
                        if (isBonus)
                            tempValue = tempAmount.ToBonusString("N0");
                        else
                            tempValue = tempAmount.ToString("N0");
                        tempAmountText = ZString.Format(
                            LanguageManager.GetText(formatKeyAmount),
                            tempElement.Title,
                            tempValue);
                        // Append current elemental armor text
                        if (dataEntry.Value != 0)
                        {
                            // Add new line if text is not empty
                            if (tempAllText.Length > 0)
                                tempAllText.Append('\n');
                            tempAllText.Append(tempAmountText);
                        }
                        // Set current elemental armor text to UI
                        if (CacheTextAmounts.TryGetValue(dataEntry.Key, out tempComponentPair))
                        {
                            tempComponentPair.uiText.text = tempAmountText;
                            if (tempComponentPair.root != null)
                                tempComponentPair.root.SetActive(!inactiveIfAmountZero || tempAmount != 0);
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

        private void SetDefaultValue(UIArmorTextPair componentPair)
        {
            DamageElement tempElement = componentPair.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : componentPair.damageElement;
            componentPair.uiText.text = ZString.Format(
                    LanguageManager.GetText(formatKeyAmount),
                    tempElement.Title,
                    isBonus ? 0f.ToBonusString("N0") : "0");
            if (componentPair.imageIcon != null)
                componentPair.imageIcon.sprite = tempElement.Icon;
            if (inactiveIfAmountZero && componentPair.root != null)
                componentPair.root.SetActive(false);
        }
    }
}
