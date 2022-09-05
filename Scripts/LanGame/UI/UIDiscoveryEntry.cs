using Cysharp.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIDiscoveryEntry : UISelectionEntry<DiscoveryData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Name}")]
        public UILocaleKeySetting formatKeyName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);

        [Header("UI Elements")]
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;

        [HideInInspector]
        public UIDiscoveryList list;

        protected override void UpdateData()
        {
            if (uiTextName != null)
            {
                uiTextName.text = ZString.Format(
                    LanguageManager.GetText(formatKeyName),
                    Data.characterName);
            }

            if (uiTextLevel != null)
            {
                uiTextLevel.text = ZString.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Data.level.ToString("N0"));
            }
        }
    }
}
