using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINpcDialogMenuAction
{
    public string title;
    public int menuIndex;
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

    public void OnClickMenu()
    {
        var owningCharacter = BasePlayerCharacterController.OwningCharacter;
        owningCharacter.RequestSelectNpcDialogMenu(Data.menuIndex);
    }
}
