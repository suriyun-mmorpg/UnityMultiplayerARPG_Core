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

    public virtual bool multiLine
    {
        get
        {
            if (unityInputField != null) return unityInputField.multiLine;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null) return textMeshInputField.multiLine;
#endif
            return false;
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

    public virtual InputField.ContentType contentType
    {
        get
        {
            if (unityInputField != null) return unityInputField.contentType;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null)
            {
                switch (textMeshInputField.contentType)
                {
                    case TMP_InputField.ContentType.Standard:
                        return InputField.ContentType.Standard;
                    case TMP_InputField.ContentType.Autocorrected:
                        return InputField.ContentType.Autocorrected;
                    case TMP_InputField.ContentType.IntegerNumber:
                        return InputField.ContentType.IntegerNumber;
                    case TMP_InputField.ContentType.DecimalNumber:
                        return InputField.ContentType.DecimalNumber;
                    case TMP_InputField.ContentType.Alphanumeric:
                        return InputField.ContentType.Alphanumeric;
                    case TMP_InputField.ContentType.Name:
                        return InputField.ContentType.Name;
                    case TMP_InputField.ContentType.EmailAddress:
                        return InputField.ContentType.EmailAddress;
                    case TMP_InputField.ContentType.Password:
                        return InputField.ContentType.Password;
                    case TMP_InputField.ContentType.Pin:
                        return InputField.ContentType.Pin;
                    case TMP_InputField.ContentType.Custom:
                        return InputField.ContentType.Custom;
                }
            }
#endif
            return InputField.ContentType.Standard;
        }

        set
        {
            if (unityInputField != null) unityInputField.contentType = value;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null)
            {
                switch (value)
                {
                    case InputField.ContentType.Standard:
                        textMeshInputField.contentType = TMP_InputField.ContentType.Standard;
                        break;
                    case InputField.ContentType.Autocorrected:
                        textMeshInputField.contentType = TMP_InputField.ContentType.Autocorrected;
                        break;
                    case InputField.ContentType.IntegerNumber:
                        textMeshInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                        break;
                    case InputField.ContentType.DecimalNumber:
                        textMeshInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
                        break;
                    case InputField.ContentType.Alphanumeric:
                        textMeshInputField.contentType = TMP_InputField.ContentType.Alphanumeric;
                        break;
                    case InputField.ContentType.Name:
                        textMeshInputField.contentType = TMP_InputField.ContentType.Name;
                        break;
                    case InputField.ContentType.EmailAddress:
                        textMeshInputField.contentType = TMP_InputField.ContentType.EmailAddress;
                        break;
                    case InputField.ContentType.Password:
                        textMeshInputField.contentType = TMP_InputField.ContentType.Password;
                        break;
                    case InputField.ContentType.Pin:
                        textMeshInputField.contentType = TMP_InputField.ContentType.Pin;
                        break;
                    case InputField.ContentType.Custom:
                        textMeshInputField.contentType = TMP_InputField.ContentType.Custom;
                        break;
                }
            }
#endif
        }
    }

    public virtual InputField.InputType inputType
    {
        get
        {
            if (unityInputField != null) return unityInputField.inputType;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null)
            {
                switch (textMeshInputField.inputType)
                {
                    case TMP_InputField.InputType.Standard:
                        return InputField.InputType.Standard;
                    case TMP_InputField.InputType.AutoCorrect:
                        return InputField.InputType.AutoCorrect;
                    case TMP_InputField.InputType.Password:
                        return InputField.InputType.Password;
                }
            }
#endif
            return InputField.InputType.Standard;
        }

        set
        {
            if (unityInputField != null) unityInputField.inputType = value;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null)
            {
                switch (value)
                {
                    case InputField.InputType.Standard:
                        textMeshInputField.inputType = TMP_InputField.InputType.Standard;
                        break;
                    case InputField.InputType.AutoCorrect:
                        textMeshInputField.inputType = TMP_InputField.InputType.AutoCorrect;
                        break;
                    case InputField.InputType.Password:
                        textMeshInputField.inputType = TMP_InputField.InputType.Password;
                        break;
                }
            }
#endif
        }
    }

    public virtual InputField.LineType lineType
    {
        get
        {
            if (unityInputField != null) return unityInputField.lineType;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null)
            {
                switch (textMeshInputField.lineType)
                {
                    case TMP_InputField.LineType.SingleLine:
                        return InputField.LineType.SingleLine;
                    case TMP_InputField.LineType.MultiLineSubmit:
                        return InputField.LineType.MultiLineSubmit;
                    case TMP_InputField.LineType.MultiLineNewline:
                        return InputField.LineType.MultiLineNewline;
                }
            }
#endif
            return InputField.LineType.SingleLine;
        }

        set
        {
            if (unityInputField != null) unityInputField.lineType = value;
#if USE_TEXT_MESH_PRO
            if (textMeshInputField != null)
            {
                switch (value)
                {
                    case InputField.LineType.SingleLine:
                        textMeshInputField.lineType = TMP_InputField.LineType.SingleLine;
                        break;
                    case InputField.LineType.MultiLineSubmit:
                        textMeshInputField.lineType = TMP_InputField.LineType.MultiLineSubmit;
                        break;
                    case InputField.LineType.MultiLineNewline:
                        textMeshInputField.lineType = TMP_InputField.LineType.MultiLineNewline;
                        break;
                }
            }
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
            bool result = false;
            if (unityInputField != null) result = unityInputField.isFocused;
#if USE_TEXT_MESH_PRO
        if (textMeshInputField != null) result = result || textMeshInputField.isFocused;
#endif
            return result;
        }
    }
}
