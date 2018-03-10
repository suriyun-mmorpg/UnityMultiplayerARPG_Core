using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemExtension
{
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

    public static CharacterStats GetStats(this EquipmentItem equipmentItem, int level)
    {
        if (equipmentItem == null)
            return new CharacterStats();
        return equipmentItem.baseStats + equipmentItem.statsIncreaseEachLevel * level;
    }

    public static Dictionary<Attribute, int> GetIncreaseAttributes(this EquipmentItem equipmentItem, int level)
    {
        return GameDataHelpers.MakeAttributeIncrementalsDictionary(equipmentItem.increaseAttributes, new Dictionary<Attribute, int>(), level);
    }

    public static Dictionary<Resistance, float> GetIncreaseResistances(this EquipmentItem equipmentItem, int level)
    {
        return GameDataHelpers.MakeResistanceIncrementalsDictionary(equipmentItem.increaseResistances, new Dictionary<Resistance, float>(), level);
    }
    #endregion

    #region Weapon Extension
    public static KeyValuePair<DamageElement, DamageAmount> GetBaseDamageAttribute(this WeaponItem weaponItem, int level, float inflictRate)
    {
        var result = new KeyValuePair<DamageElement, DamageAmount>();
        if (weaponItem != null)
            result = GameDataHelpers.MakeDamageAttributePair(weaponItem.baseDamageAttribute, level, inflictRate);
        return result;
    }

    public static Dictionary<DamageElement, DamageAmount> GetAdditionalDamageAttributes(this WeaponItem weaponItem, int level)
    {
        var result = new Dictionary<DamageElement, DamageAmount>();
        if (weaponItem != null)
            result = GameDataHelpers.MakeDamageAttributesDictionary(weaponItem.additionalDamageAttributes, result, level);
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
        var baseDamageAttribute = weaponItem.GetBaseDamageAttribute(level, weaponItem.GetDamageEffectiveness(character));
        var additionalDamageAttributes = weaponItem.GetAdditionalDamageAttributes(level);
        return GameDataHelpers.CombineDamageAttributesDictionary(additionalDamageAttributes, baseDamageAttribute);
    }
    #endregion
}
