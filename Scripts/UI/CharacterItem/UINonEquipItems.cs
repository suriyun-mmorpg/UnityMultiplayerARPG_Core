using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIList)), RequireComponent(typeof(UICharacterItemSelectionManager))]
public class UINonEquipItems : UIBase
{
    public System.Action<UICharacterItem> onSelectCharacterItem;

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

    private UICharacterItemSelectionManager selectionManager;
    public UICharacterItemSelectionManager SelectionManager
    {
        get
        {
            if (selectionManager == null)
                selectionManager = GetComponent<UICharacterItemSelectionManager>();
            return selectionManager;
        }
    }

    public override void Show()
    {
        base.Show();
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
    }

    protected void OnSelectCharacterItem(UICharacterItem ui)
    {
        if (ui == null)
            return;

        if (onSelectCharacterItem != null)
            onSelectCharacterItem(ui);
    }

    public void SetItems(IList<CharacterItem> equipItems)
    {
        SelectionManager.Clear();
        TempList.Generate(equipItems, (characterItem, ui) =>
        {
            var uiCharacterItem = ui.GetComponent<UICharacterItem>();
            uiCharacterItem.data = characterItem;
            SelectionManager.Add(uiCharacterItem);
        });
    }
}
