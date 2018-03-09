using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIList)), RequireComponent(typeof(UICharacterSkillLevelSelectionManager))]
public class UICharacterSkillLevels : UIBase
{
    public System.Action<UICharacterSkillLevel> onSelectCharacterSkillLevel;

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

    private UICharacterSkillLevelSelectionManager selectionManager;
    public UICharacterSkillLevelSelectionManager SelectionManager
    {
        get
        {
            if (selectionManager == null)
                selectionManager = GetComponent<UICharacterSkillLevelSelectionManager>();
            return selectionManager;
        }
    }

    public override void Show()
    {
        base.Show();
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSkillLevel);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacterSkillLevel);
    }

    protected void OnSelectCharacterSkillLevel(UICharacterSkillLevel ui)
    {
        if (ui == null)
            return;

        if (onSelectCharacterSkillLevel != null)
            onSelectCharacterSkillLevel(ui);
    }

    public void UpdateData(CharacterEntity characterEntity)
    {
        if (characterEntity == null)
            return;
        SelectionManager.Clear();
        var skillLevels = characterEntity.skillLevels;
        TempList.Generate(skillLevels, (index, characterSkillLevel, ui) =>
        {
            var uiCharacterSkillLevel = ui.GetComponent<UICharacterSkillLevel>();
            uiCharacterSkillLevel.data = characterSkillLevel;
            uiCharacterSkillLevel.owningCharacter = characterEntity;
            uiCharacterSkillLevel.indexOfData = index;
            SelectionManager.Add(uiCharacterSkillLevel);
        });
    }
}
