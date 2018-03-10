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
    public UICharacterSkills uiSkills;
    public UIToggleUI[] toggleUis;
    
    public UICharacterItem SelectedEquipItem { get; private set; }
    public UICharacterItem SelectedNonEquipItem { get; private set; }
    public UICharacterSkill SelectedSkillLevel { get; private set; }

    private void Awake()
    {
        Singleton = this;
    }

    private void Update()
    {
        foreach (var toggleUi in toggleUis)
        {
            if (Input.GetKeyDown(toggleUi.key))
                toggleUi.ui.Toggle();
        }
    }

    private void OnSelectEquipItem(UICharacterItem ui)
    {
        var owningCharacter = CharacterEntity.OwningCharacter;
        var slot = ui as UICharacterItem;
        if (SelectedEquipItem != null)
        {
            uiEquipItems.SelectionManager.DeselectAll();
            SelectedEquipItem = null;
        }
        if (SelectedNonEquipItem != null)
        {
            owningCharacter.EquipItem(SelectedNonEquipItem.indexOfData, slot.equipPosition);
            uiEquipItems.SelectionManager.DeselectAll();
            uiNonEquipItems.SelectionManager.DeselectAll();
        }
        else if (ui.Data.IsValid())
            SelectedEquipItem = slot;
        else
            uiEquipItems.SelectionManager.DeselectAll();
    }

    private void OnSelectNonEquipItem(UICharacterItem ui)
    {
        var owningCharacter = CharacterEntity.OwningCharacter;
        if (SelectedNonEquipItem != null)
        {
            owningCharacter.SwapOrMergeItem(SelectedNonEquipItem.indexOfData, ui.indexOfData);
            uiNonEquipItems.SelectionManager.DeselectAll();
            SelectedNonEquipItem = null;
        }
        else if (SelectedEquipItem != null)
        {
            owningCharacter.UnEquipItem(SelectedEquipItem.equipPosition, ui.indexOfData);
            uiEquipItems.SelectionManager.DeselectAll();
            uiNonEquipItems.SelectionManager.DeselectAll();
        }
        else if (ui.Data.IsValid())
            SelectedNonEquipItem = ui;
        else
            uiNonEquipItems.SelectionManager.DeselectAll();
    }

    public void UpdateCharacter()
    {
        foreach (var uiCharacter in uiCharacters)
        {
            if (uiCharacter != null)
                uiCharacter.Data = CharacterEntity.OwningCharacter;
        }
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

    public void UpdateSkills()
    {
        if (uiSkills != null)
            uiSkills.UpdateData(CharacterEntity.OwningCharacter);
    }
}
