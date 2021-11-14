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
    private InputField.ContentType contentType;
    private int characterLimit;
    private int intDefaultAmount;
    private int? intMinAmount;
    private int? intMaxAmount;
    private float floatDefaultAmount;
    private float? floatMinAmount;
    private float? floatMaxAmount;

    public string Title
    {
        get
        {
            return uiTextTitle == null ? "" : uiTextTitle.text;
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
            return uiTextDescription == null ? "" : uiTextDescription.text;
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
            return uiInputField == null ? "" : uiInputField.text;
        }
        set
        {
            if (uiInputField != null) uiInputField.text = value;
        }
    }

    protected virtual void OnEnable()
    {
        if (uiInputField != null)
        {
            uiInputField.contentType = contentType;
            uiInputField.characterLimit = characterLimit;
        }
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
        int characterLimit = 0)
    {
        Title = title;
        Description = description;
        InputFieldText = defaultText;
        this.contentType = contentType;
        this.characterLimit = characterLimit;
        this.onConfirmText = onConfirmText;
        Show();
    }

    public void Show(string title,
        string description,
        System.Action<int> onConfirmInteger,
        int? minAmount = null,
        int? maxAmount = null,
        int defaultAmount = 0)
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
        contentType = InputField.ContentType.IntegerNumber;
        characterLimit = 0;
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
        float defaultAmount = 0f)
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
        contentType = InputField.ContentType.DecimalNumber;
        characterLimit = 0;
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
        switch (contentType)
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
