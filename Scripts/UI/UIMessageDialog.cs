using UnityEngine;
using UnityEngine.UI;

public class UIMessageDialog : UIBase
{
    public TextWrapper uiTextTitle;
    public TextWrapper uiTextDescription;
    public Button buttonOkay;
    public Button buttonYes;
    public Button buttonNo;
    public Button buttonCancel;

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

    public bool ShowButtonOkay
    {
        get { return buttonOkay == null ? false : buttonOkay.gameObject.activeSelf; }
        set { if (buttonOkay != null) buttonOkay.gameObject.SetActive(value); }
    }

    public bool ShowButtonYes
    {
        get { return buttonYes == null ? false : buttonYes.gameObject.activeSelf; }
        set { if (buttonYes != null) buttonYes.gameObject.SetActive(value); }
    }

    public bool ShowButtonNo
    {
        get { return buttonNo == null ? false : buttonNo.gameObject.activeSelf; }
        set { if (buttonNo != null) buttonNo.gameObject.SetActive(value); }
    }

    public bool ShowButtonCancel
    {
        get { return buttonCancel == null ? false : buttonCancel.gameObject.activeSelf; }
        set { if (buttonCancel != null) buttonCancel.gameObject.SetActive(value); }
    }

    private System.Action onClickOkay;
    private System.Action onClickYes;
    private System.Action onClickNo;
    private System.Action onClickCancel;

    protected virtual void OnEnable()
    {
        // Set click events to all buttons
        if (buttonOkay != null)
        {
            buttonOkay.onClick.RemoveListener(OnClickOkay);
            buttonOkay.onClick.AddListener(OnClickOkay);
        }
        if (buttonYes != null)
        {
            buttonYes.onClick.RemoveListener(OnClickYes);
            buttonYes.onClick.AddListener(OnClickYes);
        }
        if (buttonNo != null)
        {
            buttonNo.onClick.RemoveListener(OnClickNo);
            buttonNo.onClick.AddListener(OnClickNo);
        }
        if (buttonCancel != null)
        {
            buttonCancel.onClick.RemoveListener(OnClickCancel);
            buttonCancel.onClick.AddListener(OnClickCancel);
        }
    }

    public void Show(string title,
        string description,
        bool showButtonOkay = true,
        bool showButtonYes = false,
        bool showButtonNo = false,
        bool showButtonCancel = false,
        System.Action onClickOkay = null,
        System.Action onClickYes = null,
        System.Action onClickNo = null,
        System.Action onClickCancel = null)
    {
        Title = title;
        Description = description;
        ShowButtonOkay = showButtonOkay;
        ShowButtonYes = showButtonYes;
        ShowButtonNo = showButtonNo;
        ShowButtonCancel = showButtonCancel;
        this.onClickOkay = onClickOkay;
        this.onClickYes = onClickYes;
        this.onClickNo = onClickNo;
        this.onClickCancel = onClickCancel;
        Show();
    }

    public void OnClickOkay()
    {
        if (onClickOkay != null)
            onClickOkay.Invoke();
        Hide();
    }

    public void OnClickYes()
    {
        if (onClickYes != null)
            onClickYes.Invoke();
        Hide();
    }

    public void OnClickNo()
    {
        if (onClickNo != null)
            onClickNo.Invoke();
        Hide();
    }

    public void OnClickCancel()
    {
        if (onClickCancel != null)
            onClickCancel.Invoke();
        Hide();
    }
}
