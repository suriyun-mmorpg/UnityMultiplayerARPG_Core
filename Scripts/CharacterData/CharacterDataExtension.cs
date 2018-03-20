using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterDataExtension
{
    public static CharacterPrototype GetPrototype(this ICharacterData data)
    {
        if (string.IsNullOrEmpty(data.PrototypeId))
            return null;
        return GameInstance.CharacterPrototypes[data.PrototypeId];
    }

    public static CharacterClass GetClass(this ICharacterData data)
    {
        var prototype = data.GetPrototype();
        if (prototype == null)
            return null;
        return prototype.characterClass;
    }

    public static CharacterModel InstantiateModel(this ICharacterData data, Transform parent)
    {
        var prototype = data.GetPrototype();
        if (prototype == null)
            return null;
        var result = Object.Instantiate(prototype.characterModel, parent);
        result.gameObject.SetActive(true);
        result.transform.localPosition = Vector3.zero;
        return result;
    }

    public static int GetNextLevelExp(this ICharacterData data)
    {
        var level = data.Level;
        var expTree = GameInstance.Singleton.expTree;
        if (level > expTree.Length)
            return 0;
        return expTree[level - 1];
    }

    #region Stats calculation, make saperate stats for buffs calculation
    public static float GetTotalItemWeight(this ICharacterData data)
    {
        var result = 0f;
        var equipItems = data.EquipItems;
        foreach (var equipItem in equipItems)
        {
            if (!equipItem.IsValid())
                continue;
            result += equipItem.GetItem().weight;
        }
        var nonEquipItems = data.NonEquipItems;
        foreach (var nonEquipItem in nonEquipItems)
        {
            if (!nonEquipItem.IsValid())
                continue;
            result += nonEquipItem.GetItem().weight * nonEquipItem.amount;
        }
        var equipWeapons = data.EquipWeapons;
        var rightHandItem = equipWeapons.rightHand;
        var leftHandItem = equipWeapons.leftHand;
        if (rightHandItem.IsValid())
            result += rightHandItem.GetItem().weight;
        if (leftHandItem.IsValid())
            result += leftHandItem.GetItem().weight;
        return result;
    }

    public static float GetArmor(this ICharacterData data)
    {
        var result = 0f;
        var equipItems = data.EquipItems;
        foreach (var equipItem in equipItems)
        {
            var defendItem = equipItem.GetDefendItem();
            if (defendItem == null)
                continue;
            result += defendItem.GetArmor(equipItem.level);
        }
        var equipWeapons = data.EquipWeapons;
        var leftHandItem = equipWeapons.leftHand;
        if (leftHandItem.IsValid())
            result += leftHandItem.GetShieldItem().GetArmor(leftHandItem.level);
        return result;
    }

    public static Dictionary<Attribute, int> GetAttributes(this ICharacterData data)
    {
        var result = new Dictionary<Attribute, int>();
        // Armors attributes
        var equipItems = data.EquipItems;
        foreach (var equipItem in equipItems)
        {
            var equipment = equipItem.GetEquipmentItem();
            if (equipment == null)
                continue;
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                equipment.GetIncreaseAttributes(equipItem.level));
        }
        // Weapons attributes
        var equipWeapons = data.EquipWeapons;
        var rightHandItem = equipWeapons.rightHand;
        var leftHandItem = equipWeapons.leftHand;
        if (rightHandItem.IsValid())
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                rightHandItem.GetEquipmentItem().GetIncreaseAttributes(rightHandItem.level));
        if (leftHandItem.IsValid())
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                leftHandItem.GetEquipmentItem().GetIncreaseAttributes(leftHandItem.level));
        // Character attributes
        var characterAttributes = data.Attributes;
        foreach (var characterAttribute in characterAttributes)
        {
            var key = characterAttribute.GetAttribute();
            var value = characterAttribute.amount;
            if (key == null)
                continue;
            if (!result.ContainsKey(key))
                result[key] = value;
            else
                result[key] += value;
        }
        return result;
    }

    public static Dictionary<Attribute, int> GetAttributesWithBuffs(this ICharacterData data)
    {
        var result = data.GetAttributes();
        var buffs = data.Buffs;
        foreach (var buff in buffs)
        {
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                buff.GetAttributes());
        }
        return result;
    }

    public static Dictionary<Resistance, float> GetResistances(this ICharacterData data)
    {
        var result = new Dictionary<Resistance, float>();
        var equipItems = data.EquipItems;
        foreach (var equipItem in equipItems)
        {
            if (equipItem.GetEquipmentItem() == null)
                continue;
            var equipment = equipItem.GetEquipmentItem();
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result,
                equipment.GetIncreaseResistances(equipItem.level));
        }
        return result;
    }

    public static Dictionary<Resistance, float> GetResistancesWithBuffs(this ICharacterData data)
    {
        var result = GetResistances(data);
        var buffs = data.Buffs;
        foreach (var buff in buffs)
        {
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result,
                buff.GetResistances());
        }
        return result;
    }

    public static CharacterStats GetStats(this ICharacterData data)
    {
        var level = data.Level;
        var characterClass = data.GetClass();
        var result = characterClass.baseStats + characterClass.statsIncreaseEachLevel * level;

        var equipItems = data.EquipItems;
        foreach (var equipment in equipItems)
        {
            result += equipment.GetEquipmentItem().GetStats(equipment.level);
        }
        result += GameDataHelpers.CaculateStats(GetAttributes(data));
        return result;
    }

    public static CharacterStats GetStatsWithBuffs(this ICharacterData data)
    {
        var result = data.GetStats();
        var buffs = data.Buffs;
        foreach (var buff in buffs)
        {
            result += buff.GetStats();
        }
        return result;
    }
    #endregion

    public static int GetMaxHp(this ICharacterData data)
    {
        return (int)data.GetStatsWithBuffs().hp;
    }

    public static int GetMaxMp(this ICharacterData data)
    {
        return (int)data.GetStatsWithBuffs().mp;
    }

    public static List<CharacterItem> GetWeapons(this ICharacterData data)
    {
        var gameInstance = GameInstance.Singleton;
        var result = new List<CharacterItem>(data.EquipItems);
        if (result.Count == 0)
        {
            var characterItem = new CharacterItem();
            characterItem.itemId = gameInstance.DefaultWeaponItem.Id;
            characterItem.level = 1;
            characterItem.amount = 1;
            result.Add(characterItem);
        }
        return result;
    }
}
