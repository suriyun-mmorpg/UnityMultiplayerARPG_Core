using UnityEngine;

namespace MultiplayerARPG
{
    public class TextSetterByLanguageKeys : MonoBehaviour
    {
        public string defaultText;
        public LanguageData[] textByLanguageKeys;
        public TextWrapper textWrapper;
        [InspectorButton(nameof(UpdateUI))]
        public bool updateUI;
        private string currentLanguageKey;

        public string Title
        {
            get { return Language.GetText(textByLanguageKeys, defaultText); }
        }

        private void Update()
        {
            if (!textWrapper || LanguageManager.CurrentLanguageKey.Equals(currentLanguageKey))
                return;
            currentLanguageKey = LanguageManager.CurrentLanguageKey;
            textWrapper.text = Title;
        }

        private void UpdateUI()
        {
            textWrapper.text = defaultText;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}