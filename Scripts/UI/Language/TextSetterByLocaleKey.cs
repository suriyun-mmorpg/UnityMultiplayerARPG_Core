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

        private void Update()
        {
            if (textWrapper == null)
                return;
            textWrapper.text = LanguageManager.GetText(localeKeySetting, defaultText);
        }
    }
}
