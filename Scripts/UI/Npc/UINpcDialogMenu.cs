using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UINpcDialogMenuActionType : byte
{
    Normal,
    QuestAccept,
    QuestDecline,
    QuestAbandon,
    QuestComplete,
}

public class UINpcDialogMenuAction
{
    public UINpcDialogMenuActionType type;
    public string title;
    public string nextDialogId;

    public void Setup(NpcDialogMenu menu)
    {
        type = UINpcDialogMenuActionType.Normal;
        title = menu.title;
        nextDialogId = menu.isCloseMenu || menu.dialog == null ? "" : menu.dialog.Id;
    }
}

public class UINpcDialogMenu : UISelectionEntry<UINpcDialogMenuAction>
{
    [Header("UI Elements")]
    public Text title;
    public UINpcDialog uiNpcDialog;

    protected override void UpdateData()
    {
        if (title != null)
            title.text = Data.title;
    }

    public void OnClick()
    {
        // TODO: Implement this
        switch (Data.type)
        {
            case UINpcDialogMenuActionType.Normal:
                if (string.IsNullOrEmpty(Data.nextDialogId))
                    uiNpcDialog.Hide();
                break;
            case UINpcDialogMenuActionType.QuestAccept:
                break;
            case UINpcDialogMenuActionType.QuestDecline:
                break;
            case UINpcDialogMenuActionType.QuestAbandon:
                break;
            case UINpcDialogMenuActionType.QuestComplete:
                break;
        }
    }
}
