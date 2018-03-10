using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIList)), RequireComponent(typeof(UICharacterSkillSelectionManager))]
public class UICharacterSkills : UIBase
{
    public System.Action<UICharacterSkill> onSelectCharacterSkill;

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
            return selectionManager;
        }
    }

    public override void Show()
    {
        base.Show();
        SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSkill);
        SelectionManager.eventOnSelect.AddListener(OnSelectCharacterSkill);
    }

    protected void OnSelectCharacterSkill(UICharacterSkill ui)
    {
        if (ui == null)
            return;

        if (onSelectCharacterSkill != null)
            onSelectCharacterSkill(ui);
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
            uiCharacterSkill.Data = characterSkill;
            uiCharacterSkill.indexOfData = index;
            SelectionManager.Add(uiCharacterSkill);
        });
    }
}
