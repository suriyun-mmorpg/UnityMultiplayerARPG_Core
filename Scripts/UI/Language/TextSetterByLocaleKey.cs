using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [ExecuteInEditMode]
    public class TextSetterByLocaleKey : MonoBehaviour
    {
        public UILocaleKeySetting localeKeySetting;
        public string defaultText;
        public TextWrapper textWrapper;
        private string currentLanguageKey;

        private void Update()
        {
            if (textWrapper == null)
                return;
            if (LanguageManager.CurrentLanguageKey.Equals(currentLanguageKey))
                return;
            currentLanguageKey = LanguageManager.CurrentLanguageKey;
            textWrapper.text = LanguageManager.GetText(localeKeySetting, defaultText);
        }
    }
}
