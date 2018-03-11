using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIList)), RequireComponent(typeof(UICharacterItemSelectionManager))]
public class UINonEquipItems : UIBase
{
    public UICharacterItem uiItemDialog;

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

    private UICharacterItemSelectionManager selectionManager;
    public UICharacterItemSelectionManager SelectionManager
    {
        get
        {
            if (selectionManager == null)
                selectionManager = GetComponent<UICharacterItemSelectionManager>();
            selectionManager.selectionMode = UISelectionMode.SelectSingle;
            return selectionManager;
        }
    }

    public override void Show()
    {
        base.Show();
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
        SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
        SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
    }

    public override void Hide()
    {
        base.Hide();
        SelectionManager.DeselectSelectedUI();
    }

    protected void OnSelectCharacterItem(UICharacterItem ui)
    {
        var uiGameplay = UISceneGameplay.Singleton;
        if (uiGameplay != null && uiGameplay.uiNonEquipItems != null)
            uiGameplay.uiNonEquipItems.SelectionManager.DeselectSelectedUI();

        if (uiItemDialog != null)
        {
            uiItemDialog.Data = ui.Data;
            uiItemDialog.Show();
        }
    }

    protected void OnDeselectCharacterItem(UICharacterItem ui)
    {
        if (uiItemDialog != null)
            uiItemDialog.Hide();
    }

    public void UpdateData(CharacterEntity characterEntity)
    {
        if (characterEntity == null)
            return;
        SelectionManager.Clear();
        var nonEquipItems = characterEntity.nonEquipItems;
        CacheList.Generate(nonEquipItems, (index, characterItem, ui) =>
        {
            var uiCharacterItem = ui.GetComponent<UICharacterItem>();
            uiCharacterItem.Data = characterItem;
            uiCharacterItem.indexOfData = index;
            SelectionManager.Add(uiCharacterItem);
        });
    }
}
