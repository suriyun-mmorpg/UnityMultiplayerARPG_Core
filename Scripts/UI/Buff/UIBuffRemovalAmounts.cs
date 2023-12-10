using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIBuffRemovalAmounts : UISelectionEntry<Dictionary<BuffRemoval, float>>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Level}, {1} = {Chance * 100}")]
        public UILocaleKeySetting formatKeyEntry = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BUFF_REMOVAL_ENTRY);
        [Tooltip("Format => {0} = {Status Effect Title}, {1} = {Entries}")]
        public UILocaleKeySetting formatKeyEntries = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BUFF_REMOVAL_ENTRIES);
        public string entriesSeparator = ",";

        [Header("UI Elements")]
        public TextWrapper uiTextAllAmounts;
        public UIBuffRemovalTextPair[] textAmounts;

        [Header("List UI Elements")]
        public UIBuffRemovalAmount uiEntryPrefab;
        public Transform uiListContainer;

        [Header("Options")]
        public bool inactiveIfAmountZero;

        private Dictionary<BuffRemoval, UIBuffRemovalTextPair> _cacheTextAmounts;
        public Dictionary<BuffRemoval, UIBuffRemovalTextPair> CacheTextAmounts
        {
            get
            {
                if (_cacheTextAmounts == null)
                {
                    _cacheTextAmounts = new Dictionary<BuffRemoval, UIBuffRemovalTextPair>();
                    BuffRemoval tempBuffRemoval;
                    foreach (UIBuffRemovalTextPair componentPair in textAmounts)
                    {
                        if (componentPair.uiText == null || componentPair.removal == null)
                            continue;
                        tempBuffRemoval = componentPair.removal;
                        SetDefaultValue(componentPair);
                        _cacheTextAmounts[tempBuffRemoval] = componentPair;
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
            foreach (UIBuffRemovalTextPair entry in CacheTextAmounts.Values)
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
                    BuffRemoval tempData;
                    float tempAmount;
                    string tempValue;
                    string tempAmountText;
                    UIBuffRemovalTextPair tempComponentPair;
                    foreach (KeyValuePair<BuffRemoval, float> dataEntry in Data)
                    {
                        if (dataEntry.Key == null)
                            continue;
                        // Set temp data
                        tempData = dataEntry.Key;
                        tempAmount = dataEntry.Value;
                        // Text format
                        tempValue = tempData.GetChanceEntriesText(tempAmount, LanguageManager.GetText(formatKeyEntry), entriesSeparator);
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

        private void SetDefaultValue(UIBuffRemovalTextPair componentPair)
        {
            BuffRemoval tempBuffRemoval = componentPair.removal;
            componentPair.uiText.text = ZString.Format(
                LanguageManager.GetText(formatKeyEntries),
                tempBuffRemoval.Title,
                LanguageManager.GetUnknowTitle());
            if (componentPair.imageIcon != null)
                componentPair.imageIcon.sprite = tempBuffRemoval.Icon;
            if (inactiveIfAmountZero && componentPair.root != null)
                componentPair.root.SetActive(false);
        }

        private void UpdateList()
        {
            if (uiEntryPrefab == null || uiListContainer == null)
                return;
            CacheList.HideAll();
            UIBuffRemovalAmount tempUI;
            CacheList.Generate(Data, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UIBuffRemovalAmount>();
                tempUI.Data = new UIBuffRemovalAmountData(data.Key, data.Value);
            });
        }
    }
}
