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
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
        SelectionManager.eventOnSelected.RemoveListener(OnSelectedCharacterItem);
        SelectionManager.eventOnSelected.AddListener(OnSelectedCharacterItem);
        SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
        SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
        base.Show();
    }

    public override void Hide()
    {
        var uiGameplay = UISceneGameplay.Singleton;
        if (uiGameplay != null)
            uiGameplay.DeselectSelectedItem();
        base.Hide();
    }

    protected void OnSelectCharacterItem(UICharacterItem ui)
    {
        var uiGameplay = UISceneGameplay.Singleton;

        if (uiGameplay != null)
            uiGameplay.DeselectSelectedItem();

        if (uiItemDialog != null && ui.Data.IsValid())
        {
            uiItemDialog.Show();
            uiItemDialog.selectionManager = selectionManager;
            uiItemDialog.Setup(ui.Data, ui.indexOfData, ui.equipPosition);
        }
        else if (uiGameplay != null)
            uiGameplay.DeselectSelectedItem();
    }

    protected void OnSelectedCharacterItem(UICharacterItem ui)
    {
        var uiGameplay = UISceneGameplay.Singleton;

        if (uiGameplay != null && !ui.Data.IsValid())
            uiGameplay.DeselectSelectedItem();
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
            uiCharacterItem.Setup(characterItem, index, string.Empty);
            SelectionManager.Add(uiCharacterItem);
        });
    }
}
