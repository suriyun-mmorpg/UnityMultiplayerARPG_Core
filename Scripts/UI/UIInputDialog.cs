using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInputDialog : UIBase
{
    public Text textTitle;
    public Text textDescription;
    public InputField inputField;
    public Button buttonConfirm;
    private System.Action<string> onConfirmText;
    private System.Action<int> onConfirmInteger;
    private System.Action<float> onConfirmDecimal;
    private InputField.ContentType contentType;
    private int intDefaultAmount;
    private int? intMinAmount;
    private int? intMaxAmount;
    private float floatDefaultAmount;
    private float? floatMinAmount;
    private float? floatMaxAmount;

    public string Title
    {
        get { return textTitle == null ? "" : textTitle.text; }
        set { if (textTitle != null) textTitle.text = value; }
    }

    public string Description
    {
        get { return textDescription == null ? "" : textDescription.text; }
        set { if (textDescription != null) textDescription.text = value; }
    }

    public string InputFieldText
    {
        get { return inputField == null ? "" : inputField.text; }
        set { if (inputField != null) inputField.text = value; }
    }

    public override void Show()
    {
        if (inputField != null)
            inputField.contentType = contentType;
        if (buttonConfirm != null)
        {
            buttonConfirm.onClick.RemoveListener(OnClickConfirm);
            buttonConfirm.onClick.AddListener(OnClickConfirm);
        }
        base.Show();
    }

    public void Show(string title,
        string description,
        System.Action<string> onConfirmText,
        string defaultText = "")
    {
        Title = title;
        Description = description;
        InputFieldText = defaultText;
        contentType = InputField.ContentType.Standard;
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
        intDefaultAmount = defaultAmount;
        intMinAmount = minAmount;
        intMaxAmount = maxAmount;
        Title = title;
        Description = description;
        InputFieldText = defaultAmount.ToString();
        if (inputField != null)
        {
            if (minAmount.Value > maxAmount.Value)
            {
                minAmount = null;
                Debug.LogWarning("min amount is more than max amount");
            }
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(ValidateIntAmount);
        }
        contentType = InputField.ContentType.IntegerNumber;
        this.onConfirmInteger = onConfirmInteger;
        Show();
    }

    protected void ValidateIntAmount(string result)
    {
        var amount = intDefaultAmount;
        if (int.TryParse(result, out amount))
        {
            inputField.onValueChanged.RemoveAllListeners();
            if (intMinAmount.HasValue && amount < intMinAmount.Value)
                InputFieldText = intMinAmount.Value.ToString();
            if (intMaxAmount.HasValue && amount > intMaxAmount.Value)
                InputFieldText = intMaxAmount.Value.ToString();
            inputField.onValueChanged.AddListener(ValidateIntAmount);
        }
    }

    public void Show(string title,
        string description,
        System.Action<float> onConfirmDecimal,
        float? minAmount = null,
        float? maxAmount = null,
        float defaultAmount = 0f)
    {
        floatDefaultAmount = defaultAmount;
        floatMinAmount = minAmount;
        floatMaxAmount = maxAmount;
        Title = title;
        Description = description;
        InputFieldText = defaultAmount.ToString();
        if (inputField != null)
        {
            if (minAmount.Value > maxAmount.Value)
            {
                minAmount = null;
                Debug.LogWarning("min amount is more than max amount");
            }
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(ValidateFloatAmount);
        }
        contentType = InputField.ContentType.DecimalNumber;
        this.onConfirmDecimal = onConfirmDecimal;
        Show();
    }

    protected void ValidateFloatAmount(string result)
    {
        var amount = floatDefaultAmount;
        if (float.TryParse(result, out amount))
        {
            inputField.onValueChanged.RemoveAllListeners();
            if (floatMinAmount.HasValue && amount < floatMinAmount.Value)
                InputFieldText = floatMinAmount.Value.ToString();
            if (floatMaxAmount.HasValue && amount > floatMaxAmount.Value)
                InputFieldText = floatMaxAmount.Value.ToString();
            inputField.onValueChanged.AddListener(ValidateFloatAmount);
        }
    }

    public void OnClickConfirm()
    {
        switch (contentType)
        {
            case InputField.ContentType.Standard:
                var text = InputFieldText;
                if (onConfirmText != null)
                    onConfirmText.Invoke(text);
                break;
            case InputField.ContentType.IntegerNumber:
                var intAmount = int.Parse(InputFieldText);
                if (onConfirmInteger != null)
                    onConfirmInteger.Invoke(intAmount);
                break;
            case InputField.ContentType.DecimalNumber:
                var floatAmount = float.Parse(InputFieldText);
                if (onConfirmDecimal != null)
                    onConfirmDecimal.Invoke(floatAmount);
                break;
        }
        Hide();
    }
}
