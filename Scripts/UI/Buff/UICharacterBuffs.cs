using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICharacterBuffs : UIBase
{
    public ICharacterData character { get; protected set; }
    public UICharacterBuff uiCharacterBuffPrefab;
    public Transform uiCharacterBuffContainer;

    private UIList cacheList;
    public UIList CacheList
    {
        get
        {
            if (cacheList == null)
            {
                cacheList = gameObject.AddComponent<UIList>();
                cacheList.uiPrefab = uiCharacterBuffPrefab.gameObject;
                cacheList.uiContainer = uiCharacterBuffContainer;
            }
            return cacheList;
        }
    }

    public void UpdateData(ICharacterData character)
    {
        this.character = character;

        if (character == null)
        {
            CacheList.HideAll();
            return;
        }

        var buffs = character.Buffs;
        CacheList.Generate(buffs, (index, characterBuff, ui) =>
        {
            var uiCharacterBuff = ui.GetComponent<UICharacterBuff>();
            uiCharacterBuff.Setup(characterBuff, character, index);
            uiCharacterBuff.Show();
        });
    }
}
