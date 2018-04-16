using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterDataExtension
{
    public static BaseCharacterDatabase GetDatabase(this ICharacterData data)
    {
        BaseCharacterDatabase database = null;
        if (string.IsNullOrEmpty(data.DatabaseId) || !GameInstance.AllCharacterDatabases.TryGetValue(data.DatabaseId, out database))
            return null;

        return database;
    }

    public static CharacterModel InstantiateModel(this ICharacterData data, Transform parent)
    {
        BaseCharacterDatabase characterDatabase = null;
        if (string.IsNullOrEmpty(data.DatabaseId) || !GameInstance.AllCharacterDatabases.TryGetValue(data.DatabaseId, out characterDatabase))
            return null;

        var result = Object.Instantiate(characterDatabase.model, parent);
        result.gameObject.SetLayerRecursively(GameInstance.Singleton.characterLayer, true);
        result.gameObject.SetActive(true);
        result.transform.localPosition = Vector3.zero;
        return result;
    }

    public static int GetNextLevelExp(this ICharacterData data)
    {
        var level = data.Level;
        if (level <= 0)
            return 0;
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

    public static Dictionary<Attribute, int> GetCharacterAttributes(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<Attribute, int>();
        var result = new Dictionary<Attribute, int>();
        // Attributes from character database
        var characterDatabase = data.GetDatabase();
        if (characterDatabase != null)
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                characterDatabase.GetCharacterAttributes(data.Level));

        // Added attributes
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

    public static Dictionary<Attribute, int> GetEquipmentAttributes(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<Attribute, int>();
        var result = new Dictionary<Attribute, int>();
        // Armors
        Item tempEquipment = null;
        var equipItems = data.EquipItems;
        foreach (var equipItem in equipItems)
        {
            tempEquipment = equipItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                    tempEquipment.GetIncreaseAttributes(equipItem.level));
        }
        // Weapons
        var equipWeapons = data.EquipWeapons;
        // Right hand equipment
        var rightHandItem = equipWeapons.rightHand;
        tempEquipment = rightHandItem.GetEquipmentItem();
        if (tempEquipment != null)
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                tempEquipment.GetIncreaseAttributes(rightHandItem.level));
        // Left hand equipment
        var leftHandItem = equipWeapons.leftHand;
        tempEquipment = leftHandItem.GetEquipmentItem();
        if (tempEquipment != null)
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                tempEquipment.GetIncreaseAttributes(leftHandItem.level));

        return result;
    }

    public static Dictionary<Attribute, int> GetBuffAttributes(this ICharacterData data)
    {
        var result = new Dictionary<Attribute, int>();
        var buffs = data.Buffs;
        foreach (var buff in buffs)
        {
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result, buff.GetIncreaseAttributes());
        }
        return result;
    }

    public static Dictionary<Attribute, int> GetAttributes(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        var result = data.GetCharacterAttributes();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result, data.GetEquipmentAttributes());
        if (sumWithBuffs)
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result, data.GetBuffAttributes());
        return result;
    }

    public static Dictionary<Resistance, float> GetCharacterResistances(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<Resistance, float>();
        var result = new Dictionary<Resistance, float>();
        var characterDatabase = data.GetDatabase();
        if (characterDatabase != null)
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result,
                characterDatabase.GetCharacterResistances(data.Level));
        return result;
    }

    public static Dictionary<Resistance, float> GetEquipmentResistances(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<Resistance, float>();
        var result = new Dictionary<Resistance, float>();
        // Armors
        Item tempEquipment = null;
        var equipItems = data.EquipItems;
        foreach (var equipItem in equipItems)
        {
            tempEquipment = equipItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineResistanceAmountsDictionary(result,
                    tempEquipment.GetIncreaseResistances(equipItem.level));
        }
        // Weapons
        var equipWeapons = data.EquipWeapons;
        // Right hand equipment
        var rightHandItem = equipWeapons.rightHand;
        tempEquipment = rightHandItem.GetEquipmentItem();
        if (tempEquipment != null)
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result,
                tempEquipment.GetIncreaseResistances(rightHandItem.level));
        // Left hand equipment
        var leftHandItem = equipWeapons.leftHand;
        tempEquipment = leftHandItem.GetEquipmentItem();
        if (tempEquipment != null)
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result,
                tempEquipment.GetIncreaseResistances(leftHandItem.level));
        return result;
    }

    public static Dictionary<Resistance, float> GetBuffResistances(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<Resistance, float>();
        var result = new Dictionary<Resistance, float>();
        var buffs = data.Buffs;
        foreach (var buff in buffs)
        {
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result, buff.GetIncreaseResistances());
        }
        return result;
    }

    public static Dictionary<Resistance, float> GetResistances(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        var result = data.GetCharacterResistances();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result, data.GetEquipmentResistances());
        if (sumWithBuffs)
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result, data.GetBuffResistances());
        return result;
    }

    public static CharacterStats GetCharacterStats(this ICharacterData data)
    {
        if (data == null)
            return new CharacterStats();
        var level = data.Level;
        var characterDatabase = data.GetDatabase();
        var result = new CharacterStats();
        if (characterDatabase != null)
            result += characterDatabase.GetCharacterStats(level);
        result += GameDataHelpers.CaculateStats(GetAttributes(data));
        return result;
    }

    public static CharacterStats GetEquipmentStats(this ICharacterData data)
    {
        if (data == null)
            return new CharacterStats();
        var result = new CharacterStats();
        // Armors
        Item tempEquipment = null;
        var equipItems = data.EquipItems;
        foreach (var equipment in equipItems)
        {
            tempEquipment = equipment.GetEquipmentItem();
            if (tempEquipment != null)
            {
                result += tempEquipment.GetIncreaseStats(equipment.level);
                result += GameDataHelpers.CaculateStats(tempEquipment.GetIncreaseAttributes(equipment.level));
            }
        }
        // Weapons
        var equipWeapons = data.EquipWeapons;
        // Right hand equipment
        var rightHandItem = equipWeapons.rightHand;
        tempEquipment = rightHandItem.GetEquipmentItem();
        if (tempEquipment != null)
        {
            result += tempEquipment.GetIncreaseStats(rightHandItem.level);
            result += GameDataHelpers.CaculateStats(tempEquipment.GetIncreaseAttributes(rightHandItem.level));
        }
        // Left hand equipment
        var leftHandItem = equipWeapons.leftHand;
        tempEquipment = leftHandItem.GetEquipmentItem();
        if (tempEquipment != null)
        {
            result += tempEquipment.GetIncreaseStats(leftHandItem.level);
            result += GameDataHelpers.CaculateStats(tempEquipment.GetIncreaseAttributes(leftHandItem.level));
        }
        return result;
    }

    public static CharacterStats GetBuffStats(this ICharacterData data)
    {
        if (data == null)
            return new CharacterStats();
        var result = new CharacterStats();
        var buffs = data.Buffs;
        foreach (var buff in buffs)
        {
            result += buff.GetIncreaseStats();
            result += GameDataHelpers.CaculateStats(buff.GetIncreaseAttributes());
        }
        return result;
    }

    public static CharacterStats GetStats(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        var result = data.GetCharacterStats();
        if (sumWithEquipments)
            result += data.GetEquipmentStats();
        if (sumWithBuffs)
            result += data.GetBuffStats();
        return result;
    }
    #endregion

    public static int GetMaxHp(this ICharacterData data)
    {
        return (int)data.GetStats().hp;
    }

    public static int GetMaxMp(this ICharacterData data)
    {
        return (int)data.GetStats().mp;
    }

    public static float GetMoveSpeed(this ICharacterData data)
    {
        return data.GetStats().moveSpeed;
    }

    public static float GetAttackSpeed(this ICharacterData data)
    {
        return data.GetStats().atkSpeed;
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
