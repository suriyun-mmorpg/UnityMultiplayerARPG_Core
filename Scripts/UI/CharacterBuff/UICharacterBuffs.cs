using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIList))]
public class UICharacterBuffs : UIBase
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

    public void UpdateData(CharacterEntity characterEntity)
    {
        if (characterEntity == null)
            return;
        var buffs = characterEntity.buffs;
        TempList.Generate(buffs, (index, characterBuff, ui) =>
        {
            var uiCharacterBuff = ui.GetComponent<UICharacterBuff>();
            uiCharacterBuff.data = characterBuff;
        });
    }
}
