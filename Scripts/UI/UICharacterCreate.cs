using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(UIList))]
public class UICharacterCreate : UIBase
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
        // Show list of characters that can be create
        var creatableClasses = GameInstance.CharacterClasses.Values.Where(a => a.canCreateByPlayer).ToList();
        TempList.MakeList(creatableClasses, (creatableClass, ui) =>
        {
            var characterEntity = CharacterEntity.CreateNewCharacter("", creatableClass.Id);
            characterEntity.gameObject.SetActive(false);
            var uiCharacter = ui.GetComponent<UICharacter>();
            uiCharacter.characterEntity = characterEntity;
        });
    }
}
