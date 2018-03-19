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
        int defaultAmount = 0)
    {
        Title = title;
        Description = description;
        InputFieldText = defaultAmount.ToString();
        contentType = InputField.ContentType.IntegerNumber;
        this.onConfirmInteger = onConfirmInteger;
        Show();
    }

    public void Show(string title,
        string description, 
        System.Action<float> onConfirmDecimal,
        float defaultAmount = 0f)
    {
        Title = title;
        Description = description;
        InputFieldText = defaultAmount.ToString();
        contentType = InputField.ContentType.DecimalNumber;
        this.onConfirmDecimal = onConfirmDecimal;
        Show();
    }

    private void OnClickConfirm()
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
