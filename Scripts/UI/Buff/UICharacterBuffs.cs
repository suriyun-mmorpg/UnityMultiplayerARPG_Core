using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIList))]
public class UICharacterBuffs : UISelectionEntry<ICharacterData>
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

    protected override void UpdateData()
    {
        if (Data == null)
            return;

        var buffs = Data.Buffs;
        CacheList.Generate(buffs, (index, characterBuff, ui) =>
        {
            var uiCharacterBuff = ui.GetComponent<UICharacterBuff>();
            uiCharacterBuff.Data = characterBuff;
        });
    }
}
