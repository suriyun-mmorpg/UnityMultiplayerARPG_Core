using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDamageElementInflictions : UISelectionEntry<Dictionary<DamageElement, float>>
    {
        [Header("UI Elements")]
        public TextWrapper uiTextAllInflictions;
        public UIDamageElementTextPair[] textInflictions;

        private Dictionary<DamageElement, TextWrapper> cacheTextInflictions;
        public Dictionary<DamageElement, TextWrapper> CacheTextInflictions
        {
            get
            {
                if (cacheTextInflictions == null)
                {
                    cacheTextInflictions = new Dictionary<DamageElement, TextWrapper>();
                    DamageElement tempElement;
                    TextWrapper tempTextComponent;
                    foreach (UIDamageElementTextPair textAmount in textInflictions)
                    {
                        if (textAmount.damageElement == null || textAmount.uiText == null)
                            continue;
                        tempElement = textAmount.damageElement;
                        tempTextComponent = textAmount.uiText;
                        tempTextComponent.text = string.Format(
                            LanguageManager.GetText(UILocaleKeys.UI_FORMAT_DAMAGE_INFLICTION_AS_ELEMENTAL.ToString()),
                            tempElement.Title,
                            "0");
                        cacheTextInflictions[tempElement] = tempTextComponent;
                    }
                }
                return cacheTextInflictions;
            }
        }

        protected override void UpdateData()
        {
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllInflictions != null)
                    uiTextAllInflictions.gameObject.SetActive(false);

                foreach (KeyValuePair<DamageElement, TextWrapper> textAmount in CacheTextInflictions)
                {
                    textAmount.Value.text = string.Format(
                        textAmount.Key == GameInstance.Singleton.DefaultDamageElement ?
                            LanguageManager.GetText(UILocaleKeys.UI_FORMAT_DAMAGE_INFLICTION.ToString()) :
                            LanguageManager.GetText(UILocaleKeys.UI_FORMAT_DAMAGE_INFLICTION_AS_ELEMENTAL.ToString()),
                        textAmount.Key.Title,
                        "0");
                }
            }
            else
            {
                string tempAllText = string.Empty;
                DamageElement tempElement;
                float tempInfliction;
                string tempAmountText;
                foreach (KeyValuePair<DamageElement, float> dataEntry in Data)
                {
                    if (dataEntry.Key == null || dataEntry.Value == 0)
                        continue;
                    // Set temp data
                    tempElement = dataEntry.Key;
                    tempInfliction = dataEntry.Value;
                    // Add new line if text is not empty
                    if (!string.IsNullOrEmpty(tempAllText))
                        tempAllText += "\n";
                    // Set current elemental damage infliction text
                    tempAmountText = string.Format(
                        tempElement == GameInstance.Singleton.DefaultDamageElement ?
                            LanguageManager.GetText(UILocaleKeys.UI_FORMAT_DAMAGE_INFLICTION.ToString()) :
                            LanguageManager.GetText(UILocaleKeys.UI_FORMAT_DAMAGE_INFLICTION_AS_ELEMENTAL.ToString()),
                        tempElement.Title,
                        (tempInfliction * 100f).ToString("N0"));
                    // Append current elemental damage infliction text
                    tempAllText += tempAmountText;
                    // Set current elemental damage infliction text to UI
                    TextWrapper textDamages;
                    if (CacheTextInflictions.TryGetValue(dataEntry.Key, out textDamages))
                        textDamages.text = tempAmountText;
                }

                if (uiTextAllInflictions != null)
                {
                    uiTextAllInflictions.gameObject.SetActive(!string.IsNullOrEmpty(tempAllText));
                    uiTextAllInflictions.text = tempAllText;
                }
            }
        }
    }
}
