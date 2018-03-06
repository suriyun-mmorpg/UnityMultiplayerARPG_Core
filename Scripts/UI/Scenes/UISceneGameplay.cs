using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISceneGameplay : MonoBehaviour
{
    public static UISceneGameplay Singleton { get; private set; }

    public UICharacter[] uiCharacters;
    public UIEquipItems uiEquipItems;
    public UINonEquipItems uiNonEquipItems;

    public CharacterEntity OwningCharacterEntity { get; private set; }
    public UICharacterItem SelectedEquipItem { get; private set; }
    public UICharacterItem SelectedNonEquipItem { get; private set; }

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        if (uiEquipItems != null)
            uiEquipItems.onSelectCharacterItem += OnSelectEquipItem;

        if (uiNonEquipItems != null)
            uiNonEquipItems.onSelectCharacterItem += OnSelectNonEquipItem;
    }

    private void OnDestroy()
    {
        if (uiEquipItems != null)
            uiEquipItems.onSelectCharacterItem -= OnSelectEquipItem;

        if (uiNonEquipItems != null)
            uiNonEquipItems.onSelectCharacterItem -= OnSelectNonEquipItem;
    }

    public void SetOwningCharacter(CharacterEntity characterEntity)
    {
        OwningCharacterEntity = characterEntity;
        foreach (var uiCharacter in uiCharacters)
        {
            if (uiCharacter != null)
                uiCharacter.data = OwningCharacterEntity;
        }
    }

    private void OnSelectEquipItem(UICharacterItem ui)
    {
        if (SelectedEquipItem != null)
        {
            uiEquipItems.SelectionManager.DeSelectAll();
            SelectedEquipItem = null;
        }
        if (SelectedNonEquipItem != null)
        {
            OwningCharacterEntity.EquipItem(SelectedNonEquipItem.data, ui.data);
            uiEquipItems.SelectionManager.DeSelectAll();
            uiNonEquipItems.SelectionManager.DeSelectAll();
        }
        else if (ui.data.IsValid())
            SelectedEquipItem = ui;
        else
            uiEquipItems.SelectionManager.DeSelectAll();
    }

    private void OnSelectNonEquipItem(UICharacterItem ui)
    {
        if (SelectedNonEquipItem != null)
        {
            OwningCharacterEntity.SwapOrMergeItem(SelectedNonEquipItem.data, ui.data);
            uiNonEquipItems.SelectionManager.DeSelectAll();
            SelectedNonEquipItem = null;
        }
        else if (SelectedEquipItem != null)
        {
            OwningCharacterEntity.UnEquipItem(SelectedNonEquipItem.data, ui.data);
            uiEquipItems.SelectionManager.DeSelectAll();
            uiNonEquipItems.SelectionManager.DeSelectAll();
        }
        else if (ui.data.IsValid())
            SelectedNonEquipItem = ui;
        else
            uiNonEquipItems.SelectionManager.DeSelectAll();
    }

    public void SetEquipItems(IList<CharacterItem> items)
    {
        if (uiEquipItems != null)
            uiEquipItems.SetItems(items);
    }

    public void SetNonEquipItems(IList<CharacterItem> items)
    {
        if (uiNonEquipItems != null)
            uiNonEquipItems.SetItems(items);
    }
}
