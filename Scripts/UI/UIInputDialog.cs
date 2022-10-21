using UnityEngine;
using UnityEngine.UI;

public class UIInputDialog : UIBase
{
    public TextWrapper uiTextTitle;
    public TextWrapper uiTextDescription;
    public InputFieldWrapper uiInputField;
    public Button buttonConfirm;
    private System.Action<string> onConfirmText;
    private System.Action<int> onConfirmInteger;
    private System.Action<float> onConfirmDecimal;
    private int intDefaultAmount;
    private int? intMinAmount;
    private int? intMaxAmount;
    private float floatDefaultAmount;
    private float? floatMinAmount;
    private float? floatMaxAmount;
    private string defaultPlaceHolderText;

    public string Title
    {
        get
        {
            return uiTextTitle == null ? string.Empty : uiTextTitle.text;
        }
        set
        {
            if (uiTextTitle != null) uiTextTitle.text = value;
        }
    }

    public string Description
    {
        get
        {
            return uiTextDescription == null ? string.Empty : uiTextDescription.text;
        }
        set
        {
            if (uiTextDescription != null) uiTextDescription.text = value;
        }
    }

    public string InputFieldText
    {
        get
        {
            return uiInputField == null ? string.Empty : uiInputField.text;
        }
        set
        {
            if (uiInputField != null) uiInputField.text = value;
        }
    }

    public string PlaceHolderText
    {
        get
        {

            if (uiInputField != null)
            {
                if (uiInputField.placeholder is Text)
                    return (uiInputField.placeholder as Text).text;
                if (uiInputField.placeholder is TMPro.TMP_Text)
                    return (uiInputField.placeholder as TMPro.TMP_Text).text;
            }
            return string.Empty;
        }
        set
        {
            if (uiInputField != null)
            {
                if (uiInputField.placeholder is Text)
                {
                    if (string.IsNullOrEmpty(defaultPlaceHolderText))
                        defaultPlaceHolderText = (uiInputField.placeholder as Text).text;
                    (uiInputField.placeholder as Text).text = !string.IsNullOrEmpty(value) ? value : defaultPlaceHolderText;
                }
                if (uiInputField.placeholder is TMPro.TMP_Text)
                {
                    if (string.IsNullOrEmpty(defaultPlaceHolderText))
                        defaultPlaceHolderText = (uiInputField.placeholder as TMPro.TMP_Text).text;
                    (uiInputField.placeholder as TMPro.TMP_Text).text = !string.IsNullOrEmpty(value) ? value : defaultPlaceHolderText;
                }
            }
        }
    }

    public InputField.ContentType ContentType
    {
        get
        {
            return uiInputField == null ? InputField.ContentType.Standard : uiInputField.contentType;
        }
        set
        {
            if (uiInputField != null) uiInputField.contentType = value;
        }
    }

    public int CharacterLimit
    {
        get
        {
            return uiInputField == null ? 0 : uiInputField.characterLimit;
        }
        set
        {
            if (uiInputField != null) uiInputField.characterLimit = value;
        }
    }

    protected virtual void OnEnable()
    {
        if (buttonConfirm != null)
        {
            buttonConfirm.onClick.RemoveListener(OnClickConfirm);
            buttonConfirm.onClick.AddListener(OnClickConfirm);
        }
    }

    public void Show(string title,
        string description,
        System.Action<string> onConfirmText,
        string defaultText = "",
        InputField.ContentType contentType = InputField.ContentType.Standard,
        int characterLimit = 0,
        string placeHolder = "")
    {
        Title = title;
        Description = description;
        InputFieldText = defaultText;
        ContentType = contentType;
        CharacterLimit = characterLimit;
        PlaceHolderText = placeHolder;
        this.onConfirmText = onConfirmText;
        Show();
    }

    public void Show(string title,
        string description,
        System.Action<int> onConfirmInteger,
        int? minAmount = null,
        int? maxAmount = null,
        int defaultAmount = 0,
        string placeHolder = "")
    {
        if (!minAmount.HasValue)
            minAmount = int.MinValue;
        if (!maxAmount.HasValue)
            maxAmount = int.MaxValue;

        intDefaultAmount = defaultAmount;
        intMinAmount = minAmount;
        intMaxAmount = maxAmount;

        Title = title;
        Description = description;
        InputFieldText = defaultAmount.ToString();
        PlaceHolderText = placeHolder;
        if (uiInputField != null)
        {
            if (minAmount.Value > maxAmount.Value)
            {
                minAmount = null;
                Debug.LogWarning("min amount is more than max amount");
            }
            uiInputField.onValueChanged.RemoveAllListeners();
            uiInputField.onValueChanged.AddListener(ValidateIntAmount);
        }
        ContentType = InputField.ContentType.IntegerNumber;
        CharacterLimit = 0;
        this.onConfirmInteger = onConfirmInteger;
        Show();
    }

    protected void ValidateIntAmount(string result)
    {
        int amount = intDefaultAmount;
        if (int.TryParse(result, out amount))
        {
            uiInputField.onValueChanged.RemoveAllListeners();
            if (intMinAmount.HasValue && amount < intMinAmount.Value)
                InputFieldText = intMinAmount.Value.ToString();
            if (intMaxAmount.HasValue && amount > intMaxAmount.Value)
                InputFieldText = intMaxAmount.Value.ToString();
            uiInputField.onValueChanged.AddListener(ValidateIntAmount);
        }
    }

    public void Show(string title,
        string description,
        System.Action<float> onConfirmDecimal,
        float? minAmount = null,
        float? maxAmount = null,
        float defaultAmount = 0f,
        string placeHolder = "")
    {
        if (!minAmount.HasValue)
            minAmount = float.MinValue;
        if (!maxAmount.HasValue)
            maxAmount = float.MaxValue;

        floatDefaultAmount = defaultAmount;
        floatMinAmount = minAmount;
        floatMaxAmount = maxAmount;
        Title = title;
        Description = description;
        InputFieldText = defaultAmount.ToString();
        PlaceHolderText = placeHolder;
        if (uiInputField != null)
        {
            if (minAmount.Value > maxAmount.Value)
            {
                minAmount = null;
                Debug.LogWarning("min amount is more than max amount");
            }
            uiInputField.onValueChanged.RemoveAllListeners();
            uiInputField.onValueChanged.AddListener(ValidateFloatAmount);
        }
        ContentType = InputField.ContentType.DecimalNumber;
        CharacterLimit = 0;
        this.onConfirmDecimal = onConfirmDecimal;
        Show();
    }

    protected void ValidateFloatAmount(string result)
    {
        float amount = floatDefaultAmount;
        if (float.TryParse(result, out amount))
        {
            uiInputField.onValueChanged.RemoveAllListeners();
            if (floatMinAmount.HasValue && amount < floatMinAmount.Value)
                InputFieldText = floatMinAmount.Value.ToString();
            if (floatMaxAmount.HasValue && amount > floatMaxAmount.Value)
                InputFieldText = floatMaxAmount.Value.ToString();
            uiInputField.onValueChanged.AddListener(ValidateFloatAmount);
        }
    }

    public void OnClickConfirm()
    {
        switch (ContentType)
        {
            case InputField.ContentType.IntegerNumber:
                int intAmount = int.Parse(InputFieldText);
                if (onConfirmInteger != null)
                    onConfirmInteger.Invoke(intAmount);
                break;
            case InputField.ContentType.DecimalNumber:
                float floatAmount = float.Parse(InputFieldText);
                if (onConfirmDecimal != null)
                    onConfirmDecimal.Invoke(floatAmount);
                break;
            default:
                string text = InputFieldText;
                if (onConfirmText != null)
                    onConfirmText.Invoke(text);
                break;
        }
        Hide();
    }
}
