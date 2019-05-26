using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Toggle))]
    public class OnToggleChangeLanguage : MonoBehaviour
    {
        public string languageKey;

        private void Awake()
        {
            Toggle toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnToggle);
            toggle.isOn = LanguageManager.CurrentLanguageKey.Equals(languageKey);
        }

        public void OnToggle(bool selected)
        {
            if (selected)
                LanguageManager.ChangeLanguage(languageKey);
        }
    }
}
