using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if USE_TEXT_MESH_PRO
using TMPro;
#endif

public class InputFieldWrapper : MonoBehaviour
{
    public InputField unityInputField;
#if USE_TEXT_MESH_PRO
    public TMP_InputField textMeshInputField;
#endif
    public virtual string text
    {
        get
        {
            if (unityInputField != null) return unityInputField.text;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null) return textMeshInputField.text;
#endif
            return string.Empty;
        }

        set
        {
            if (unityInputField != null) unityInputField.text = value;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null) textMeshInputField.text = value;
#endif
        }
    }

    public virtual bool interactable
    {
        get
        {
            if (unityInputField != null) return unityInputField.interactable;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null) return textMeshInputField.interactable;
#endif
            return false;
        }

        set
        {
            if (unityInputField != null) unityInputField.interactable = value;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null) textMeshInputField.interactable = value;
#endif
        }
    }

    public virtual UnityEvent<string> onValueChanged
    {
        get
        {
            if (unityInputField != null) return unityInputField.onValueChanged;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null) return textMeshInputField.onValueChanged;
#endif
            return null;
        }

        set
        {
            if (unityInputField != null) unityInputField.onValueChanged = value as InputField.OnChangeEvent;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null) textMeshInputField.onValueChanged = value as TMP_InputField.OnChangeEvent;
#endif
        }
    }

    void Awake()
    {
        if (unityInputField == null) unityInputField = GetComponent<InputField>();
#if USE_TEXT_MESH_PRO
        if (textMeshInputField == null) textMeshInputField = GetComponent<TMP_InputField>();
#endif
    }

    public void DeactivateInputField()
    {
        if (unityInputField != null) unityInputField.DeactivateInputField();
#if USE_TEXT_MESH_PRO
        if (textMeshInputField != null) textMeshInputField.DeactivateInputField();
#endif
    }

    public void Select()
    {
        if (unityInputField != null) unityInputField.Select();
#if USE_TEXT_MESH_PRO
        if (textMeshInputField != null) textMeshInputField.Select();
#endif
    }

    public void ActivateInputField()
    {
        if (unityInputField != null) unityInputField.ActivateInputField();
#if USE_TEXT_MESH_PRO
        if (textMeshInputField != null) textMeshInputField.ActivateInputField();
#endif
    }

    public bool isFocused
    {
        get
        {
            var result = false;
            if (unityInputField != null) result = unityInputField.isFocused;
#if USE_TEXT_MESH_PRO
        if (textMeshInputField != null) result = result || textMeshInputField.isFocused;
#endif
            return result;
        }
    }
}
