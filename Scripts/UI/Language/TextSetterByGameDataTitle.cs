using UnityEngine;

namespace MultiplayerARPG
{
    public class TextSetterByGameDataTitle : MonoBehaviour
    {
        public BaseGameData gameData;
        public TextWrapper textWrapper;
        [InspectorButton(nameof(UpdateUI))]
        public bool updateUI;
        private string currentLanguageKey;

        private void Update()
        {
            if (!textWrapper || LanguageManager.CurrentLanguageKey.Equals(currentLanguageKey))
                return;
            currentLanguageKey = LanguageManager.CurrentLanguageKey;
            textWrapper.text = gameData.Title;
        }

        private void UpdateUI()
        {
            textWrapper.text = gameData.Title;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
