using UnityEngine;
using UnityEngine.UI;
#if USE_TEXT_MESH_PRO
using TMPro;
#endif

public class TextWrapper : MonoBehaviour
{
    public Text unityText;
#if USE_TEXT_MESH_PRO
    public TextMeshProUGUI textMeshText;
#endif
    public virtual string text
    {
        get
        {
            if (unityText != null) return unityText.text;
#if USE_TEXT_MESH_PRO
            if (textMeshText != null) return textMeshText.text;
#endif
            return string.Empty;
        }

        set
        {
            if (unityText != null) unityText.text = value;
#if USE_TEXT_MESH_PRO
            if (textMeshText != null) textMeshText.text = value;
#endif
        }
    }

    void Awake()
    {
        if (unityText == null) unityText = GetComponent<Text>();
#if USE_TEXT_MESH_PRO
        if (textMeshText == null) textMeshText = GetComponent<TextMeshProUGUI>();
#endif
    }
}
