using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class OnClickChangeLanguage : MonoBehaviour
    {
        public string languageKey;
        public void OnClick()
        {
            LanguageManager.ChangeLanguage(languageKey);
        }
    }
}
