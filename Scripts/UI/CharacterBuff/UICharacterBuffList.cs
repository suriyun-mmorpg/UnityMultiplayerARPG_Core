using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIList))]
public class UICharacterBuffList : UIBase
{
    private UIList tempList;
    public UIList TempList
    {
        get
        {
            if (tempList == null)
                tempList = GetComponent<UIList>();
            return tempList;
        }
    }

    public override void Show()
    {
        base.Show();
    }

    public void SetBuffs(IList<CharacterBuff> buffs)
    {
        TempList.Generate(buffs, (characterBuff, ui) =>
        {
            var uiCharacterBuff = ui.GetComponent<UICharacterBuff>();
            uiCharacterBuff.data = characterBuff;
        });
    }
}
