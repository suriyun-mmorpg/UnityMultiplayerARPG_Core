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
        SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
        SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
        base.Show();
    }

    public override void Hide()
    {
        SelectionManager.DeselectSelectedUI();
        base.Hide();
    }

    protected void OnSelectCharacterItem(UICharacterItem ui)
    {
        var uiGameplay = UISceneGameplay.Singleton;
        if (uiGameplay != null && uiGameplay.uiEquipItems != null)
            uiGameplay.uiEquipItems.SelectionManager.DeselectSelectedUI();

        if (uiItemDialog != null)
        {
            uiItemDialog.Show();
            uiItemDialog.selectionManager = selectionManager;
            uiItemDialog.Setup(ui.Data, ui.indexOfData, ui.equipPosition);
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
            uiCharacterItem.Setup(characterItem, index, string.Empty);
            SelectionManager.Add(uiCharacterItem);
        });
    }
}
