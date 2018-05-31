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

    public static bool CanAttack(this Item weaponItem, ICharacterData character)
    {
        if (weaponItem == null ||
            !weaponItem.IsWeapon() ||
            character == null)
            return false;

        var requireAmmoType = weaponItem.WeaponType.requireAmmoType;
        return requireAmmoType == null || character.IndexOfAmmoItem(requireAmmoType) >= 0;
    }

    public static CharacterStats GetIncreaseStats(this Item equipmentItem, int level)
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

    public static Dictionary<DamageElement, float> GetIncreaseResistances(this Item equipmentItem, int level)
    {
        var result = new Dictionary<DamageElement, float>();
        if (equipmentItem != null &&
            equipmentItem.IsEquipment())
            result = GameDataHelpers.MakeResistanceAmountsDictionary(equipmentItem.increaseResistances, result, level);
        return result;
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this Item equipmentItem, int level)
    {
        var result = new Dictionary<DamageElement, MinMaxFloat>();
        if (equipmentItem != null &&
            equipmentItem.IsEquipment())
            result = GameDataHelpers.MakeDamageAmountsDictionary(equipmentItem.increaseDamages, result, level);
        return result;
    }
    #endregion    

    #region Weapon Extension
    public static KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount(this Item weaponItem, int level, ICharacterData character)
    {
        if (weaponItem == null ||
            !weaponItem.IsWeapon())
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        return GameDataHelpers.MakeDamageAmountPair(weaponItem.damageAmount, level, weaponItem.GetEffectivenessDamage(character));
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetDamageAmountWithInflictions(this Item weaponItem, int level, ICharacterData character, Dictionary<DamageElement, float> damageInflictionAmounts)
    {
        if (weaponItem == null ||
            !weaponItem.IsWeapon())
            return new Dictionary<DamageElement, MinMaxFloat>();
        return GameDataHelpers.MakeDamageAmountWithInflictions(weaponItem.damageAmount, level, weaponItem.GetEffectivenessDamage(character), damageInflictionAmounts);
    }

    public static float GetEffectivenessDamage(this Item weaponItem, ICharacterData character)
    {
        if (weaponItem == null ||
            !weaponItem.IsWeapon() ||
            character == null)
            return 0f;
        return GameDataHelpers.CalculateEffectivenessDamage(weaponItem.WeaponType.CacheEffectivenessAttributes, character);
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
