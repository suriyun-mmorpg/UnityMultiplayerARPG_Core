using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIList)), RequireComponent(typeof(UICharacterSkillSelectionManager))]
public class UICharacterSkills : UIBase
{
    public UICharacterSkill uiSkillDialog;

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

    private UICharacterSkillSelectionManager selectionManager;
    public UICharacterSkillSelectionManager SelectionManager
    {
        get
        {
            if (selectionManager == null)
                selectionManager = GetComponent<UICharacterSkillSelectionManager>();
            selectionManager.selectionMode = UISelectionMode.SelectSingle;
            return selectionManager;
        }
    }

    public override void Show()
    {
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSkill);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacterSkill);
        SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterSkill);
        SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterSkill);
        base.Show();
    }

    public override void Hide()
    {
        SelectionManager.DeselectSelectedUI();
        base.Hide();
    }

    protected void OnSelectCharacterSkill(UICharacterSkill ui)
    {
        if (uiSkillDialog != null)
        {
            uiSkillDialog.Data = ui.Data;
            uiSkillDialog.Show();
        }
    }

    protected void OnDeselectCharacterSkill(UICharacterSkill ui)
    {
        if (uiSkillDialog != null)
            uiSkillDialog.Hide();
    }

    public void UpdateData(CharacterEntity characterEntity)
    {
        if (characterEntity == null)
            return;
        SelectionManager.Clear();
        var skillLevels = characterEntity.skills;
        CacheList.Generate(skillLevels, (index, characterSkill, ui) =>
        {
            var uiCharacterSkill = ui.GetComponent<UICharacterSkill>();
            uiCharacterSkill.Setup(characterSkill, index);
            SelectionManager.Add(uiCharacterSkill);
        });
    }
}
