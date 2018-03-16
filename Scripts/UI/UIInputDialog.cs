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
        System.Action<string> onConfirmText)
    {
        Title = title;
        Description = description;
        contentType = InputField.ContentType.Standard;
        this.onConfirmText = onConfirmText;
        Show();
    }

    public void Show(string title,
        string description, 
        System.Action<int> onConfirmInteger)
    {
        Title = title;
        Description = description;
        contentType = InputField.ContentType.IntegerNumber;
        this.onConfirmInteger = onConfirmInteger;
        Show();
    }

    public void Show(string title,
        string description, 
        System.Action<float> onConfirmDecimal)
    {
        Title = title;
        Description = description;
        contentType = InputField.ContentType.DecimalNumber;
        this.onConfirmDecimal = onConfirmDecimal;
        Show();
    }

    private void OnClickConfirm()
    {
        switch (contentType)
        {
            case InputField.ContentType.Standard:
                var text = "";
                if (inputField != null)
                    text = inputField.text;
                if (onConfirmText != null)
                    onConfirmText.Invoke(text);
                break;
            case InputField.ContentType.IntegerNumber:
                var intAmount = 0;
                if (inputField != null)
                    intAmount = int.Parse(inputField.text);
                if (onConfirmInteger != null)
                    onConfirmInteger.Invoke(intAmount);
                break;
            case InputField.ContentType.DecimalNumber:
                var floatAmount = 0f;
                if (inputField != null)
                    floatAmount = float.Parse(inputField.text);
                if (onConfirmDecimal != null)
                    onConfirmDecimal.Invoke(floatAmount);
                break;
        }
        Hide();
    }
}
