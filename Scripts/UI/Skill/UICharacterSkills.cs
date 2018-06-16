using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UICharacterSkillSelectionManager))]
public class UICharacterSkills : UIBase
{
    public ICharacterData character { get; protected set; }
    public enum ListingMode
    {
        DefiningByCharacter,
        Predefined,
    }
    public UICharacterSkill uiSkillDialog;
    public UICharacterSkill uiCharacterSkillPrefab;
    public Transform uiCharacterSkillContainer;

    [Tooltip("If listing mode is `Defining By Character` it will make list of skills by `UI List` component, with data from character. If it's `Predefined`, it will showing predefined skills")]
    public ListingMode listingMode;

    [Header("Predefined Listing Mode")]
    public UICharacterSkillPair[] uiCharacterSkills;

    private Dictionary<Skill, UICharacterSkill> cacheUICharacterSkills = null;
    public Dictionary<Skill, UICharacterSkill> CacheUICharacterSkills
    {
        get
        {
            if (cacheUICharacterSkills == null)
            {
                cacheUICharacterSkills = new Dictionary<Skill, UICharacterSkill>();
                foreach (var uiCharacterSkill in uiCharacterSkills)
                {
                    if (uiCharacterSkill.skill != null &&
                        uiCharacterSkill.ui != null &&
                        !cacheUICharacterSkills.ContainsKey(uiCharacterSkill.skill))
                        cacheUICharacterSkills.Add(uiCharacterSkill.skill, uiCharacterSkill.ui);
                }
            }
            return cacheUICharacterSkills;
        }
    }

    private UIList cacheList;
    public UIList CacheList
    {
        get
        {
            if (cacheList == null)
            {
                cacheList = gameObject.AddComponent<UIList>();
                cacheList.uiPrefab = uiCharacterSkillPrefab.gameObject;
                cacheList.uiContainer = uiCharacterSkillContainer;
            }
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
            uiSkillDialog.selectionManager = SelectionManager;
            uiSkillDialog.Setup(ui.Data, character, ui.indexOfData);
            uiSkillDialog.Show();
        }
    }

    protected void OnDeselectCharacterSkill(UICharacterSkill ui)
    {
        if (uiSkillDialog != null)
            uiSkillDialog.Hide();
    }

    public void UpdateData(ICharacterData character)
    {
        this.character = character;
        var selectedSkillId = SelectionManager.SelectedUI != null ? SelectionManager.SelectedUI.Data.characterSkill.dataId : 0;
        SelectionManager.DeselectSelectedUI();
        SelectionManager.Clear();

        if (character == null)
        {
            CacheList.HideAll();
            return;
        }

        var characterSkills = character.Skills;
        switch (listingMode)
        {
            case ListingMode.DefiningByCharacter:
                CacheList.Generate(characterSkills, (index, characterSkill, ui) =>
                {
                    var uiCharacterSkill = ui.GetComponent<UICharacterSkill>();
                    uiCharacterSkill.Setup(new CharacterSkillLevelTuple(characterSkill, characterSkill.level), character, index);
                    uiCharacterSkill.Show();
                    SelectionManager.Add(uiCharacterSkill);
                    if (selectedSkillId.Equals(characterSkill.dataId))
                        uiCharacterSkill.OnClickSelect();
                });
                break;
            case ListingMode.Predefined:
                CacheList.HideAll();
                for (var i = 0; i < characterSkills.Count; ++i)
                {
                    var characterSkill = characterSkills[i];
                    var level = characterSkill.level;
                    var skill = characterSkill.GetSkill();
                    UICharacterSkill cacheUICharacterSkill;
                    if (CacheUICharacterSkills.TryGetValue(skill, out cacheUICharacterSkill))
                    {
                        cacheUICharacterSkill.Setup(new CharacterSkillLevelTuple(characterSkill, level), character, i);
                        cacheUICharacterSkill.Show();
                        if (selectedSkillId.Equals(characterSkill.dataId))
                            cacheUICharacterSkill.OnClickSelect();
                    }
                    else
                        cacheUICharacterSkill.Hide();
                }
                break;
        }
    }
}
