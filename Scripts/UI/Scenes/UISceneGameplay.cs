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
    public UICharacterBuffList uiBuffList;
    public UIEquipItems uiEquipItems;
    public UINonEquipItems uiNonEquipItems;
    public UICharacterSkillLevelList uiSkillLevelList;
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

    public void SetOwningCharacter()
    {
        foreach (var uiCharacter in uiCharacters)
        {
            if (uiCharacter != null)
                uiCharacter.data = CharacterEntity.OwningCharacter;
        }
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
            owningCharacter.EquipItem(owningCharacter.nonEquipItems.IndexOf(SelectedNonEquipItem.data), slot.equipPosition);
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
            owningCharacter.SwapOrMergeItem(owningCharacter.nonEquipItems.IndexOf(SelectedNonEquipItem.data), owningCharacter.nonEquipItems.IndexOf(ui.data));
            uiNonEquipItems.SelectionManager.DeSelectAll();
            SelectedNonEquipItem = null;
        }
        else if (SelectedEquipItem != null)
        {
            owningCharacter.UnEquipItem(SelectedEquipItem.equipPosition, owningCharacter.nonEquipItems.IndexOf(ui.data));
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

    public void SetBuffs(IList<CharacterBuff> buffs)
    {
        if (uiBuffList != null)
            uiBuffList.SetBuffs(buffs);
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

    public void SetSkillLevels(IList<CharacterSkillLevel> skillLevels)
    {
        if (uiSkillLevelList != null)
            uiSkillLevelList.SetSkills(skillLevels);
    }
}
