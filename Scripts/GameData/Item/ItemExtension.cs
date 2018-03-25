using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemExtension
{
    #region Equipment Extension
    public static bool CanEquip(this Item equipmentItem, ICharacterData character, int level)
    {
        if (equipmentItem == null || 
            !equipmentItem.IsEquipment() || 
            character == null)
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

        if (equipmentItem.requirement.character != null && equipmentItem.requirement.character != character.GetDatabase())
            isPass = false;

        return character.Level >= equipmentItem.requirement.level && isPass;
    }

    public static CharacterStats GetStats(this Item equipmentItem, int level)
    {
        if (equipmentItem == null ||
            !equipmentItem.IsEquipment())
            return new CharacterStats();
        return equipmentItem.increaseStats.GetCharacterStats(level);
    }

    public static Dictionary<Attribute, int> GetIncreaseAttributes(this Item equipmentItem, int level)
    {
        var result = new Dictionary<Attribute, int>();
        if (equipmentItem != null &&
            equipmentItem.IsEquipment())
            result = GameDataHelpers.MakeAttributeAmountsDictionary(equipmentItem.increaseAttributes, result, level);
        return result;
    }

    public static Dictionary<Resistance, float> GetIncreaseResistances(this Item equipmentItem, int level)
    {
        var result = new Dictionary<Resistance, float>();
        if (equipmentItem != null &&
            equipmentItem.IsEquipment())
            result = GameDataHelpers.MakeResistanceAmountsDictionary(equipmentItem.increaseResistances, result, level);
        return result;
    }

    public static Dictionary<DamageElement, DamageAmount> GetIncreaseDamageAttributes(this Item equipmentItem, int level)
    {
        var result = new Dictionary<DamageElement, DamageAmount>();
        if (equipmentItem != null &&
            equipmentItem.IsEquipment())
            result = GameDataHelpers.MakeDamageAttributesDictionary(equipmentItem.increaseDamageAttributes, result, level);
        return result;
    }
    #endregion    

    #region Weapon Extension
    public static KeyValuePair<DamageElement, DamageAmount> GetDamageAttribute(this Item weaponItem, int level, float effectiveness, float inflictRate)
    {
        if (weaponItem == null ||
            !weaponItem.IsWeapon())
            return new KeyValuePair<DamageElement, DamageAmount>();
        return GameDataHelpers.MakeDamageAttributePair(weaponItem.damageAttribute, level, effectiveness, inflictRate);
    }

    public static float GetEffectivenessDamage(this Item weaponItem, ICharacterData character)
    {
        if (weaponItem == null ||
            !weaponItem.IsWeapon())
            return 0f;
        return GameDataHelpers.CalculateEffectivenessDamage(weaponItem.WeaponType.CacheEffectivenessAttributes, character);
    }

    public static Dictionary<DamageElement, DamageAmount> GetAllDamages(this Item weaponItem, ICharacterData character, int level)
    {
        if (weaponItem == null ||
            !weaponItem.IsWeapon())
            return new Dictionary<DamageElement, DamageAmount>();
        var baseDamageAttribute = weaponItem.GetDamageAttribute(level, weaponItem.GetEffectivenessDamage(character), 1f);
        var additionalDamageAttributes = weaponItem.GetIncreaseDamageAttributes(level);
        return GameDataHelpers.CombineDamageAttributesDictionary(additionalDamageAttributes, baseDamageAttribute);
    }

    public static bool TryGetWeaponItemEquipType(this Item weaponItem, out WeaponItemEquipType equipType)
    {
        equipType = WeaponItemEquipType.OneHand;
        if (weaponItem == null ||
            !weaponItem.IsWeapon())
            return false;
        equipType = weaponItem.EquipType;
        return true;
    }
    #endregion
}
