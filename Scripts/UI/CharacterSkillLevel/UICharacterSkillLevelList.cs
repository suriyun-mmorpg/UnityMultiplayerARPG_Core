using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIList)), RequireComponent(typeof(UICharacterSkillLevelSelectionManager))]
public class UICharacterSkillLevelList : UIBase
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

    public void SetSkills(IList<CharacterSkillLevel> skillLevels)
    {
        SelectionManager.Clear();
        TempList.Generate(skillLevels, (characterSkillLevel, ui) =>
        {
            var uiCharacterSkillLevel = ui.GetComponent<UICharacterSkillLevel>();
            uiCharacterSkillLevel.data = characterSkillLevel;
            SelectionManager.Add(uiCharacterSkillLevel);
        });
    }
}
