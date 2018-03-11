using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemExtension
{
    #region Equipment Extension
    public static bool CanEquip(this BaseEquipmentItem equipmentItem, ICharacterData character, int level)
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
        var requireAttributeAmounts = equipmentItem.CacheRequireAttributeAmounts;
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

    public static CharacterStats GetStats(this BaseEquipmentItem equipmentItem, int level)
    {
        if (equipmentItem == null)
            return new CharacterStats();
        return equipmentItem.baseStats + equipmentItem.statsIncreaseEachLevel * level;
    }

    public static Dictionary<Attribute, int> GetIncreaseAttributes(this BaseEquipmentItem equipmentItem, int level)
    {
        var result = new Dictionary<Attribute, int>();
        if (equipmentItem != null)
            result = GameDataHelpers.MakeAttributeIncrementalsDictionary(equipmentItem.increaseAttributes, result, level);
        return result;
    }

    public static Dictionary<Resistance, float> GetIncreaseResistances(this BaseEquipmentItem equipmentItem, int level)
    {
        var result = new Dictionary<Resistance, float>();
        if (equipmentItem != null)
            result = GameDataHelpers.MakeResistanceIncrementalsDictionary(equipmentItem.increaseResistances, result, level);
        return result;
    }

    public static Dictionary<DamageElement, DamageAmount> GetIncreaseDamageAttributes(this BaseEquipmentItem equipmentItem, int level)
    {
        var result = new Dictionary<DamageElement, DamageAmount>();
        if (equipmentItem != null)
            result = GameDataHelpers.MakeDamageAttributesDictionary(equipmentItem.increaseDamageAttributes, result, level);
        return result;
    }
    #endregion

    #region Weapon Extension
    public static KeyValuePair<DamageElement, DamageAmount> GetDamageAttribute(this WeaponItem weaponItem, int level, float inflictRate)
    {
        var result = new KeyValuePair<DamageElement, DamageAmount>();
        if (weaponItem != null)
            result = GameDataHelpers.MakeDamageAttributePair(weaponItem.damageAttribute, level, inflictRate);
        return result;
    }

    public static float GetDamageEffectiveness(this WeaponItem weaponItem, ICharacterData character)
    {
        if (weaponItem == null)
            return 1f;
        return GameDataHelpers.CalculateDamageEffectiveness(weaponItem.WeaponType.CacheEffectivenessAttributes, character);
    }

    public static Dictionary<DamageElement, DamageAmount> GetAllDamages(this WeaponItem weaponItem, ICharacterData character, int level)
    {
        var baseDamageAttribute = weaponItem.GetDamageAttribute(level, weaponItem.GetDamageEffectiveness(character));
        var additionalDamageAttributes = weaponItem.GetIncreaseDamageAttributes(level);
        return GameDataHelpers.CombineDamageAttributesDictionary(additionalDamageAttributes, baseDamageAttribute);
    }
    #endregion
}
