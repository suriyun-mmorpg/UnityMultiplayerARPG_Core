using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIStatusEffectResistanceAmounts : UISelectionEntry<Dictionary<StatusEffect, float>>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Level}, {1} = {Chance * 100}")]
        public UILocaleKeySetting formatKeyEntry = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STATUS_EFFECT_RESISTANCE_ENTRY);
        [Tooltip("Format => {0} = {Status Effect Title}, {1} = {Entries}")]
        public UILocaleKeySetting formatKeyEntries = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STATUS_EFFECT_RESISTANCE_ENTRIES);
        public string entriesSeparator = ",";

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UIStatusEffectResistanceTextPair[] textAmounts;

        [Header("List UI Elements")]
        public UIStatusEffectResistanceAmount uiEntryPrefab;
        public Transform uiListContainer;

        [Header("Options")]
        public bool isBonus;
        public bool inactiveIfAmountZero;

        private Dictionary<StatusEffect, UIStatusEffectResistanceTextPair> _cacheTextAmounts;
        public Dictionary<StatusEffect, UIStatusEffectResistanceTextPair> CacheTextAmounts
        {
            get
            {
                if (_cacheTextAmounts == null)
                {
                    _cacheTextAmounts = new Dictionary<StatusEffect, UIStatusEffectResistanceTextPair>();
                    StatusEffect tempStatusEffect;
                    foreach (UIStatusEffectResistanceTextPair componentPair in textAmounts)
                    {
                        if (componentPair.uiText == null || componentPair.statusEffect == null)
                            continue;
                        tempStatusEffect = componentPair.statusEffect;
                        SetDefaultValue(componentPair);
                        _cacheTextAmounts[tempStatusEffect] = componentPair;
                    }
                }
                return _cacheTextAmounts;
            }
        }

        private UIList _cacheList;
        public UIList CacheList
        {
            get
            {
                if (_cacheList == null)
                {
                    _cacheList = gameObject.AddComponent<UIList>();
                    _cacheList.uiPrefab = uiEntryPrefab.gameObject;
                    _cacheList.uiContainer = uiListContainer;
                }
                return _cacheList;
            }
        }

        protected override void UpdateData()
        {
            // Reset number
            foreach (UIStatusEffectResistanceTextPair entry in CacheTextAmounts.Values)
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
                    StatusEffect tempData;
                    float tempAmount;
                    string tempValue;
                    string tempAmountText;
                    UIStatusEffectResistanceTextPair tempComponentPair;
                    foreach (KeyValuePair<StatusEffect, float> dataEntry in Data)
                    {
                        if (dataEntry.Key == null)
                            continue;
                        // Set temp data
                        tempData = dataEntry.Key;
                        tempAmount = dataEntry.Value;
                        // Text format
                        tempValue = tempData.GetResistanceEntriesText(tempAmount, LanguageManager.GetText(formatKeyEntry), isBonus, entriesSeparator);
                        tempAmountText = ZString.Format(
                            LanguageManager.GetText(formatKeyEntries),
                            tempData.Title,
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
            UpdateList();
        }

        private void SetDefaultValue(UIStatusEffectResistanceTextPair componentPair)
        {
            StatusEffect tempStatusEffect = componentPair.statusEffect;
            componentPair.uiText.text = ZString.Format(
                LanguageManager.GetText(formatKeyEntries),
                tempStatusEffect.Title,
                LanguageManager.GetUnknowTitle());
            if (componentPair.imageIcon != null)
                componentPair.imageIcon.sprite = tempStatusEffect.Icon;
            if (inactiveIfAmountZero && componentPair.root != null)
                componentPair.root.SetActive(false);
        }

        private void UpdateList()
        {
            if (uiEntryPrefab == null || uiListContainer == null)
                return;
            CacheList.HideAll();
            UIStatusEffectResistanceAmount tempUI;
            CacheList.Generate(Data, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UIStatusEffectResistanceAmount>();
                tempUI.Data = new UIStatusEffectResistanceAmountData(data.Key, data.Value);
            });
        }
    }
}
