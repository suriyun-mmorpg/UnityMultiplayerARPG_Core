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
        public Text unityText;
        public TextMeshProUGUI textMeshText;

        private string languageKey;

        private void Awake()
        {
            if (unityText == null)
                unityText = GetComponent<Text>();
            if (textMeshText == null)
                textMeshText = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (languageKey != LanguageManager.CurrentLanguageKey)
            {
                string text = "";
                if (LanguageManager.Texts.TryGetValue(dataKey, out text))
                {
                    if (unityText != null)
                        unityText.text = text;
                    if (textMeshText != null)
                        textMeshText.text = text;
                }
                else
                {
                    if (unityText != null)
                        unityText.text = defaultText;
                    if (textMeshText != null)
                        textMeshText.text = defaultText;
                }
                languageKey = LanguageManager.CurrentLanguageKey;
            }
        }

        void OnValidate()
        {
            if (unityText != null)
                unityText.text = defaultText;
            if (textMeshText != null)
                textMeshText.text = defaultText;
        }
    }
}
