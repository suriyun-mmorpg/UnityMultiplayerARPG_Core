using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemExtension {
    #region Equipment Extension
    public static bool CanEquip(this EquipmentItem equipmentItem, ICharacterData character, int level)
    {
        if (equipmentItem == null || character == null)
            return false;

        var isPass = true;
        var attributeAmountsDict = new Dictionary<Attribute, int>();
        var attributeAmounts = character.Attributes;
        foreach (var attributeAmount in attributeAmounts)
        {
            if (attributeAmount.GetAttribute() == null)
                continue;
            attributeAmountsDict[attributeAmount.GetAttribute()] = attributeAmount.amount;
        }
        var requireAttributeAmounts = equipmentItem.TempRequireAttributeAmounts;
        foreach (var requireAttributeAmount in requireAttributeAmounts)
        {
            if (!attributeAmountsDict.ContainsKey(requireAttributeAmount.Key) ||
                attributeAmountsDict[requireAttributeAmount.Key] < requireAttributeAmount.Value)
            {
                isPass = false;
                break;
            }
        }

        if (equipmentItem.requirement.characterClass != null && equipmentItem.requirement.characterClass != character.GetClass())
            isPass = false;

        return character.Level >= equipmentItem.requirement.characterLevel && isPass;
    }

    public static CharacterStats GetStats(this EquipmentItem equipmentItem, int level)
    {
        if (equipmentItem == null)
            return new CharacterStats();
        return equipmentItem.baseStats + equipmentItem.statsIncreaseEachLevel * level;
    }

    #endregion
}
