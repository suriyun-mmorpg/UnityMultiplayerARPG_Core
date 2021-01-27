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

    public virtual Color color
    {
        get
        {
            if (unityText != null) return unityText.color;
#if USE_TEXT_MESH_PRO
            if (textMeshText != null) return textMeshText.color;
#endif
            return Color.clear;
        }

        set
        {
            if (unityText != null) unityText.color = value;
#if USE_TEXT_MESH_PRO
            if (textMeshText != null) textMeshText.color = value;
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

    public void SetGameObjectActive(bool isActive)
    {
        if (unityText != null)
            unityText.gameObject.SetActive(isActive);
#if USE_TEXT_MESH_PRO
        if (textMeshText != null)
            textMeshText.gameObject.SetActive(isActive);
#endif
        gameObject.SetActive(isActive);
    }
}
