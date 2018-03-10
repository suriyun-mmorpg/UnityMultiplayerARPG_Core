using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIList))]
public class UICharacterBuffs : UIBase
{
    private UIList cacheList;
    public UIList CacheList
    {
        get
        {
            if (cacheList == null)
                cacheList = GetComponent<UIList>();
            return cacheList;
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
        CacheList.Generate(buffs, (index, characterBuff, ui) =>
        {
            var uiCharacterBuff = ui.GetComponent<UICharacterBuff>();
            uiCharacterBuff.Data = characterBuff;
        });
    }
}
