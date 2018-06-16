using System.Collections.Generic;
using UnityEngine;

public class UICharacterHotkeyAssigner : UIBase
{
    public string hotkeyId { get; protected set; }

    public UICharacterSkill uiCharacterSkillPrefab;
    public UICharacterItem uiCharacterItemPrefab;
    public Transform uiCharacterSkillContainer;
    public Transform uiCharacterItemContainer;

    private UIList cacheSkillList;
    public UIList CacheSkillList
    {
        get
        {
            if (cacheSkillList == null)
            {
                cacheSkillList = gameObject.AddComponent<UIList>();
                cacheSkillList.uiPrefab = uiCharacterSkillPrefab.gameObject;
                cacheSkillList.uiContainer = uiCharacterSkillContainer;
            }
            return cacheSkillList;
        }
    }

    private UIList cacheItemList;
    public UIList CacheItemList
    {
        get
        {
            if (cacheItemList == null)
            {
                cacheItemList = gameObject.AddComponent<UIList>();
                cacheItemList.uiPrefab = uiCharacterItemPrefab.gameObject;
                cacheItemList.uiContainer = uiCharacterItemContainer;
            }
            return cacheItemList;
        }
    }

    private UICharacterSkillSelectionManager cacheSkillSelectionManager;
    public UICharacterSkillSelectionManager CacheSkillSelectionManager
    {
        get
        {
            if (cacheSkillSelectionManager == null)
            {
                cacheSkillSelectionManager = gameObject.AddComponent<UICharacterSkillSelectionManager>();
                cacheSkillSelectionManager.eventOnSelect = new UICharacterSkillEvent();
                cacheSkillSelectionManager.eventOnDeselect = new UICharacterSkillEvent();
                cacheSkillSelectionManager.eventOnSelected = new UICharacterSkillEvent();
                cacheSkillSelectionManager.eventOnDeselected = new UICharacterSkillEvent();
            }
            cacheSkillSelectionManager.selectionMode = UISelectionMode.SelectSingle;
            return cacheSkillSelectionManager;
        }
    }

    private UICharacterItemSelectionManager cacheItemSelectionManager;
    public UICharacterItemSelectionManager CacheItemSelectionManager
    {
        get
        {
            if (cacheItemSelectionManager == null)
            {
                cacheItemSelectionManager = gameObject.AddComponent<UICharacterItemSelectionManager>();
                cacheItemSelectionManager.eventOnSelect = new UICharacterItemEvent();
                cacheItemSelectionManager.eventOnDeselect = new UICharacterItemEvent();
                cacheItemSelectionManager.eventOnSelected = new UICharacterItemEvent();
                cacheItemSelectionManager.eventOnDeselected = new UICharacterItemEvent();
            }
                cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
            return cacheItemSelectionManager;
        }
    }

    public void Setup(string hotkeyId)
    {
        this.hotkeyId = hotkeyId;
    }

    public override void Show()
    {
        CacheSkillSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSkill);
        CacheSkillSelectionManager.eventOnSelect.AddListener(OnSelectCharacterSkill);
        CacheItemSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
        CacheItemSelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
        var owningCharacter = BasePlayerCharacterController.OwningCharacter;
        if (owningCharacter == null)
        {
            CacheSkillList.HideAll();
            CacheItemList.HideAll();
            return;
        }
        var filterSkills = new List<CharacterSkill>();
        var filterItems = new List<CharacterItem>();
        var characterSkills = owningCharacter.Skills;
        var characterItems = owningCharacter.NonEquipItems;
        foreach (var characterSkill in characterSkills)
        {
            var skill = characterSkill.GetSkill();
            if (skill != null && characterSkill.level > 0 && 
                (skill.skillType == SkillType.Active || skill.skillType == SkillType.CraftItem))
                filterSkills.Add(characterSkill);
        }
        foreach (var characterItem in characterItems)
        {
            var item = characterItem.GetItem();
            if (item != null && item.IsPotion() && characterItem.level > 0 && characterItem.amount > 0)
                filterItems.Add(characterItem);
        }
        CacheSkillList.Generate(filterSkills, (index, characterSkill, ui) =>
        {
            var uiCharacterSkill = ui.GetComponent<UICharacterSkill>();
            uiCharacterSkill.Setup(new CharacterSkillLevelTuple(characterSkill, characterSkill.level), null, -1);
            uiCharacterSkill.Show();
            CacheSkillSelectionManager.Add(uiCharacterSkill);
        });
        CacheItemList.Generate(filterItems, (index, characterItem, ui) =>
        {
            var uiCharacterItem = ui.GetComponent<UICharacterItem>();
            uiCharacterItem.Setup(new CharacterItemLevelTuple(characterItem, characterItem.level), null, -1, string.Empty);
            uiCharacterItem.Show();
            CacheItemSelectionManager.Add(uiCharacterItem);
        });
        base.Show();
    }

    public override void Hide()
    {
        CacheSkillSelectionManager.DeselectSelectedUI();
        CacheItemSelectionManager.DeselectSelectedUI();
        base.Hide();
    }

    protected void OnSelectCharacterSkill(UICharacterSkill ui)
    {
        var owningCharacter = BasePlayerCharacterController.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.RequestAssignHotkey(hotkeyId, HotkeyType.Skill, ui.Data.characterSkill.dataId);
        Hide();
    }

    protected void OnSelectCharacterItem(UICharacterItem ui)
    {
        var owningCharacter = BasePlayerCharacterController.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.RequestAssignHotkey(hotkeyId, HotkeyType.Item, ui.Data.characterItem.dataId);
        Hide();
    }

    public void OnClickUnAssign()
    {
        var owningCharacter = BasePlayerCharacterController.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.RequestAssignHotkey(hotkeyId, HotkeyType.None, 0);
        Hide();
    }
}
