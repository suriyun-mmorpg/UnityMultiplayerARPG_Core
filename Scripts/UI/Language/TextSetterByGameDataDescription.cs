using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class TextSetterByGameDataDescription : MonoBehaviour
    {
        public BaseGameData gameData;
        public TextWrapper textWrapper;
        private string currentLanguageKey;

        private void Update()
        {
            if (textWrapper == null)
                return;
            if (LanguageManager.CurrentLanguageKey.Equals(currentLanguageKey))
                return;
            currentLanguageKey = LanguageManager.CurrentLanguageKey;
            textWrapper.text = gameData.Description;
        }
    }
}
