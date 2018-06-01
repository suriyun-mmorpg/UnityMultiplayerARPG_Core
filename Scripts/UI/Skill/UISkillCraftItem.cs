using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillCraftItem : UISelectionEntry<Skill>
{
    [Header("UI Elements")]
    public UICharacterItem uiCraftingItem;
    public UIItemAmounts uiRequireItemAmounts;

    protected override void UpdateData()
    {
        var skill = Data;
        if (uiCraftingItem != null)
        {
            if (skill == null || skill.craftingItem == null)
                uiCraftingItem.Hide();
            else
            {
                uiCraftingItem.Show();
                uiCraftingItem.Data = new Tuple<CharacterItem, int>(CharacterItem.Create(skill.craftingItem), 1);
            }
        }

        if (uiRequireItemAmounts != null)
        {
            if (skill == null)
                uiRequireItemAmounts.Hide();
            else
            {
                uiRequireItemAmounts.Show();
                uiRequireItemAmounts.Data = skill.CacheCraftRequirements;
            }
        }
    }
}
