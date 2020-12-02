using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDamageElementInflictions : UISelectionEntry<Dictionary<DamageElement, float>>
    {
        [Header("String Formats")]
        [Tooltip("Format => {1} = {Infliction * 100}")]
        public UILocaleKeySetting formatKeyInfliction = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_DAMAGE_INFLICTION);
        [Tooltip("Format => {0} = {Damage Element Title}, {1} = {Infliction * 100}")]
        public UILocaleKeySetting formatKeyInflictionAsElemental = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_DAMAGE_INFLICTION_AS_ELEMENTAL);

        [Header("UI Elements")]
        public TextWrapper uiTextAllInflictions;
        public UIDamageElementTextPair[] textInflictions;

        private Dictionary<DamageElement, UIDamageElementTextPair> cacheTextInflictions;
        public Dictionary<DamageElement, UIDamageElementTextPair> CacheTextInflictions
        {
            get
            {
                if (cacheTextInflictions == null)
                {
                    cacheTextInflictions = new Dictionary<DamageElement, UIDamageElementTextPair>();
                    DamageElement tempElement;
                    foreach (UIDamageElementTextPair componentPair in textInflictions)
                    {
                        if (componentPair.uiText == null)
                            continue;
                        tempElement = componentPair.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : componentPair.damageElement;
                        SetDefaultValue(componentPair);
                        cacheTextInflictions[tempElement] = componentPair;
                    }
                }
                return cacheTextInflictions;
            }
        }

        protected override void UpdateData()
        {
            // Reset number
            foreach (UIDamageElementTextPair entry in CacheTextInflictions.Values)
            {
                SetDefaultValue(entry);
            }
            // Set number by updated data
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllInflictions != null)
                    uiTextAllInflictions.gameObject.SetActive(false);
            }
            else
            {
                string tempAllText = string.Empty;
                DamageElement tempElement;
                float tempInfliction;
                string tempAmountText;
                UIDamageElementTextPair tempComponentPair;
                foreach (KeyValuePair<DamageElement, float> dataEntry in Data)
                {
                    if (dataEntry.Key == null)
                        continue;
                    // Set temp data
                    tempElement = dataEntry.Key;
                    tempInfliction = dataEntry.Value;
                    // Set current elemental damage infliction text
                    tempAmountText = string.Format(
                        tempElement == GameInstance.Singleton.DefaultDamageElement ?
                            LanguageManager.GetText(formatKeyInfliction) :
                            LanguageManager.GetText(formatKeyInflictionAsElemental),
                        tempElement.Title,
                        (tempInfliction * 100f).ToString("N0"));
                    // Append current elemental damage infliction text
                    if (dataEntry.Value != 0)
                    {
                        // Add new line if text is not empty
                        if (!string.IsNullOrEmpty(tempAllText))
                            tempAllText += "\n";
                        tempAllText += tempAmountText;
                    }
                    // Set current elemental damage infliction text to UI
                    if (CacheTextInflictions.TryGetValue(dataEntry.Key, out tempComponentPair))
                        tempComponentPair.uiText.text = tempAmountText;
                }

                if (uiTextAllInflictions != null)
                {
                    uiTextAllInflictions.gameObject.SetActive(!string.IsNullOrEmpty(tempAllText));
                    uiTextAllInflictions.text = tempAllText;
                }
            }
        }

        private void SetDefaultValue(UIDamageElementTextPair componentPair)
        {
            DamageElement tempElement = componentPair.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : componentPair.damageElement;
            componentPair.uiText.text = string.Format(
                tempElement == GameInstance.Singleton.DefaultDamageElement ?
                    LanguageManager.GetText(formatKeyInfliction) :
                    LanguageManager.GetText(formatKeyInflictionAsElemental),
                tempElement.Title,
                0.ToString("N0"));
            if (componentPair.imageIcon != null)
                componentPair.imageIcon.sprite = tempElement.icon;
        }
    }
}
