using System.Collections.Generic;
using Cysharp.Text;
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

        [Header("List UI Elements")]
        public UIDamageElementAmount uiEntryPrefab;
        public Transform uiListContainer;

        [Header("Options")]
        public bool isBonus;
        public bool inactiveIfAmountZero;

        private Dictionary<DamageElement, UIDamageElementTextPair> _cacheTextDamages;
        public Dictionary<DamageElement, UIDamageElementTextPair> CacheTextDamages
        {
            get
            {
                if (_cacheTextDamages == null)
                {
                    _cacheTextDamages = new Dictionary<DamageElement, UIDamageElementTextPair>();
                    DamageElement tempElement;
                    foreach (UIDamageElementTextPair componentPair in textDamages)
                    {
                        if (componentPair.uiText == null)
                            continue;
                        tempElement = componentPair.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : componentPair.damageElement;
                        SetDefaultValue(componentPair);
                        _cacheTextDamages[tempElement] = componentPair;
                    }
                }
                return _cacheTextDamages;
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
            if (uiTextSumDamage != null)
            {
                uiTextSumDamage.text = ZString.Format(
                    LanguageManager.GetText(formatKeySumDamage),
                    isBonus ? 0.ToBonusString("N0") : "0",
                    "0");
            }
            foreach (UIDamageElementTextPair entry in CacheTextDamages.Values)
            {
                SetDefaultValue(entry);
            }
            // Set number by updated data
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllDamages != null)
                    uiTextAllDamages.SetGameObjectActive(false);
            }
            else
            {
                using (Utf16ValueStringBuilder tempAllText = ZString.CreateStringBuilder(false))
                {
                    MinMaxFloat sumDamage = new MinMaxFloat();
                    DamageElement tempElement;
                    MinMaxFloat tempAmount;
                    string tempMinValue;
                    string tempMaxValue;
                    string tempAmountText;
                    UIDamageElementTextPair tempComponentPair;
                    foreach (KeyValuePair<DamageElement, MinMaxFloat> dataEntry in Data)
                    {
                        if (dataEntry.Key == null)
                            continue;
                        // Set temp data
                        tempElement = dataEntry.Key;
                        tempAmount = dataEntry.Value;
                        // Set current elemental damage text
                        if (isBonus)
                            tempMinValue = tempAmount.min.ToBonusString("N0");
                        else
                            tempMinValue = tempAmount.min.ToString("N0");
                        tempMaxValue = tempAmount.max.ToString("N0");
                        tempAmountText = ZString.Format(
                            LanguageManager.GetText(formatKeyDamage),
                            tempElement.Title,
                            tempMinValue,
                            tempMaxValue);
                        // Append current elemental damage text
                        if (dataEntry.Value.min != 0 || dataEntry.Value.max != 0)
                        {
                            // Add new line if text is not empty
                            if (tempAllText.Length > 0)
                                tempAllText.Append('\n');
                            tempAllText.Append(tempAmountText);
                        }
                        // Set current elemental damage text to UI
                        if (CacheTextDamages.TryGetValue(dataEntry.Key, out tempComponentPair))
                        {
                            tempComponentPair.uiText.text = tempAmountText;
                            if (tempComponentPair.root != null)
                                tempComponentPair.root.SetActive(!inactiveIfAmountZero || (tempAmount.min != 0 && tempAmount.max != 0));
                        }
                        sumDamage += tempAmount;
                    }

                    if (uiTextAllDamages != null)
                    {
                        uiTextAllDamages.SetGameObjectActive(tempAllText.Length > 0);
                        uiTextAllDamages.text = tempAllText.ToString();
                    }

                    if (uiTextSumDamage != null)
                    {
                        if (isBonus)
                            tempMinValue = sumDamage.min.ToBonusString("N0");
                        else
                            tempMinValue = sumDamage.min.ToString("N0");
                        tempMaxValue = sumDamage.max.ToString("N0");
                        uiTextSumDamage.text = ZString.Format(
                            LanguageManager.GetText(formatKeySumDamage),
                            tempMinValue,
                            tempMaxValue);
                    }
                }
            }
            UpdateList();
        }

        private void SetDefaultValue(UIDamageElementTextPair componentPair)
        {
            DamageElement tempElement = componentPair.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : componentPair.damageElement;
            componentPair.uiText.text = ZString.Format(
                LanguageManager.GetText(formatKeyDamage),
                tempElement.Title,
                isBonus ? 0.ToBonusString("N0") : "0",
                "0");
            if (componentPair.imageIcon != null)
                componentPair.imageIcon.sprite = tempElement.Icon;
            if (inactiveIfAmountZero && componentPair.root != null)
                componentPair.root.SetActive(false);
        }

        private void UpdateList()
        {
            CacheList.HideAll();

            if (uiEntryPrefab == null || uiListContainer == null)
                return;

            UIDamageElementAmount tempUI;
            CacheList.Generate(Data, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UIDamageElementAmount>();
                tempUI.Data = new UIDamageElementAmountData(data.Key, data.Value);
            });
        }
    }
}
