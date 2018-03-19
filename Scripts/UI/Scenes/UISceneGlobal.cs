using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISceneGlobal : MonoBehaviour
{
    public static UISceneGlobal Singleton { get; private set; }
    public UIMessageDialog uiMessageDialog;
    public UIInputDialog uiInputDialog;

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowMessageDialog(string title,
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
        uiMessageDialog.Show(title,
            description,
            showButtonOkay,
            showButtonYes,
            showButtonNo,
            showButtonCancel,
            onClickOkay,
            onClickYes,
            onClickNo,
            onClickCancel);
    }
    
    public void ShowInputDialog(string title,
        string description,
        System.Action<string> onConfirmText,
        string defaultText = "")
    {
        uiInputDialog.Show(title,
            description,
            onConfirmText,
            defaultText);
    }

    public void ShowInputDialog(string title,
        string description,
        System.Action<int> onConfirmInteger,
        int defaultAmount = 0)
    {
        uiInputDialog.Show(title,
            description,
            onConfirmInteger,
            defaultAmount);
    }

    public void ShowInputDialog(string title,
        string description,
        System.Action<float> onConfirmDecimal,
        float defaultAmount = 0f)
    {
        uiInputDialog.Show(title,
            description,
            onConfirmDecimal,
            defaultAmount);
    }
}
