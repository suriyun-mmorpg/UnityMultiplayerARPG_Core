using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISceneGameplay : MonoBehaviour
{
    [System.Serializable]
    public struct UIToggleUI
    {
        public UIBase ui;
        public KeyCode key;
    }

    public static UISceneGameplay Singleton { get; private set; }

    public UICharacter[] uiCharacters;
    public UICharacterBuffs uiBuffList;
    public UIEquipItems uiEquipItems;
    public UINonEquipItems uiNonEquipItems;
    public UICharacterSkillLevels uiSkillLevelList;
    public UIToggleUI[] toggleUis;
    
    public UIEquipItemSlot SelectedEquipItem { get; private set; }
    public UICharacterItem SelectedNonEquipItem { get; private set; }
    public UICharacterSkillLevel SelectedSkillLevel { get; private set; }

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        foreach (var uiCharacter in uiCharacters)
        {
            if (uiCharacter != null)
                uiCharacter.data = CharacterEntity.OwningCharacter;
        }

        if (uiEquipItems != null)
            uiEquipItems.onSelectCharacterItem += OnSelectEquipItem;

        if (uiNonEquipItems != null)
            uiNonEquipItems.onSelectCharacterItem += OnSelectNonEquipItem;
    }

    private void Update()
    {
        foreach (var toggleUi in toggleUis)
        {
            if (Input.GetKeyDown(toggleUi.key))
                toggleUi.ui.Toggle();
        }
    }

    private void OnDestroy()
    {
        if (uiEquipItems != null)
            uiEquipItems.onSelectCharacterItem -= OnSelectEquipItem;

        if (uiNonEquipItems != null)
            uiNonEquipItems.onSelectCharacterItem -= OnSelectNonEquipItem;
    }

    private void OnSelectEquipItem(UICharacterItem ui)
    {
        var owningCharacter = CharacterEntity.OwningCharacter;
        var slot = ui as UIEquipItemSlot;
        if (SelectedEquipItem != null)
        {
            uiEquipItems.SelectionManager.DeSelectAll();
            SelectedEquipItem = null;
        }
        if (SelectedNonEquipItem != null)
        {
            owningCharacter.EquipItem(SelectedNonEquipItem.indexOfData, slot.equipPosition);
            uiEquipItems.SelectionManager.DeSelectAll();
            uiNonEquipItems.SelectionManager.DeSelectAll();
        }
        else if (ui.data.IsValid())
            SelectedEquipItem = slot;
        else
            uiEquipItems.SelectionManager.DeSelectAll();
    }

    private void OnSelectNonEquipItem(UICharacterItem ui)
    {
        var owningCharacter = CharacterEntity.OwningCharacter;
        if (SelectedNonEquipItem != null)
        {
            owningCharacter.SwapOrMergeItem(SelectedNonEquipItem.indexOfData, ui.indexOfData);
            uiNonEquipItems.SelectionManager.DeSelectAll();
            SelectedNonEquipItem = null;
        }
        else if (SelectedEquipItem != null)
        {
            owningCharacter.UnEquipItem(SelectedEquipItem.equipPosition, ui.indexOfData);
            uiEquipItems.SelectionManager.DeSelectAll();
            uiNonEquipItems.SelectionManager.DeSelectAll();
        }
        else if (ui.data.IsValid())
            SelectedNonEquipItem = ui;
        else
            uiNonEquipItems.SelectionManager.DeSelectAll();
    }

    private void OnSelectCharacterSkillLevel(UICharacterSkillLevel ui)
    {
        SelectedSkillLevel = ui;
    }

    public void UpdateBuffs()
    {
        if (uiBuffList != null)
            uiBuffList.UpdateData(CharacterEntity.OwningCharacter);
    }

    public void UpdateEquipItems()
    {
        if (uiEquipItems != null)
            uiEquipItems.UpdateData(CharacterEntity.OwningCharacter);
    }

    public void UpdateNonEquipItems()
    {
        if (uiNonEquipItems != null)
            uiNonEquipItems.UpdateData(CharacterEntity.OwningCharacter);
    }

    public void UpdateSkillLevels()
    {
        if (uiSkillLevelList != null)
            uiSkillLevelList.UpdateData(CharacterEntity.OwningCharacter);
    }
}
