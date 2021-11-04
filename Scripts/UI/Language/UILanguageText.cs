using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MultiplayerARPG
{
    public class UILanguageText : MonoBehaviour
    {
        public string dataKey;
        [TextArea(1, 10)]
        public string defaultText;

        private Text unityText;
        public Text UnityText
        {
            get
            {
                if (unityText == null)
                    unityText = GetComponent<Text>();
                return unityText;
            }
        }

        private TextMeshProUGUI textMeshText;
        public TextMeshProUGUI TextMeshText
        {
            get
            {
                if (textMeshText == null)
                    textMeshText = GetComponent<TextMeshProUGUI>();
                return textMeshText;
            }
        }

        private string languageKey;

        private void Update()
        {
            if (languageKey != LanguageManager.CurrentLanguageKey)
            {
                string text = "";
                if (LanguageManager.Texts.TryGetValue(dataKey, out text))
                {
                    UnityText.text = text;
                    TextMeshText.text = text;
                }
                else
                {
                    UnityText.text = defaultText;
                    TextMeshText.text = defaultText;
                }
                languageKey = LanguageManager.CurrentLanguageKey;
            }
        }

        void OnValidate()
        {
            UnityText.text = defaultText;
            TextMeshText.text = defaultText;
        }
    }
}
