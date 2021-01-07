using UnityEngine;
using UnityEngine.UI;
#if USE_TEXT_MESH_PRO
using TMPro;
#endif

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

#if USE_TEXT_MESH_PRO
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
#endif

        private string languageKey;

        private void Update()
        {
            if (languageKey != LanguageManager.CurrentLanguageKey)
            {
                string text = "";
                if (LanguageManager.Texts.TryGetValue(dataKey, out text))
                {
                    UnityText.text = text;
#if USE_TEXT_MESH_PRO
                TextMeshText.text = text;
#endif
                }
                else
                {
                    UnityText.text = defaultText;
#if USE_TEXT_MESH_PRO
                TextMeshText.text = defaultText;
#endif
                }
                languageKey = LanguageManager.CurrentLanguageKey;
            }
        }

        void OnValidate()
        {
            UnityText.text = defaultText;
#if USE_TEXT_MESH_PRO
        TextMeshText.text = defaultText;
#endif
        }
    }
}
