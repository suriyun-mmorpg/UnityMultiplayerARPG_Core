using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterDataExtension
{
    public static BaseCharacter GetDatabase(this ICharacterData data)
    {
        BaseCharacter database = null;
        if (string.IsNullOrEmpty(data.DatabaseId) || !GameInstance.AllCharacters.TryGetValue(data.DatabaseId, out database))
            return null;

        return database;
    }

    public static CharacterModel InstantiateModel(this ICharacterData data, Transform parent)
    {
        BaseCharacter character = null;
        if (string.IsNullOrEmpty(data.DatabaseId) || !GameInstance.AllCharacters.TryGetValue(data.DatabaseId, out character))
            return null;

        var result = Object.Instantiate(character.model, parent);
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
        var character = data.GetDatabase();
        if (character != null)
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                character.GetCharacterAttributes(data.Level));

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

    public static Dictionary<DamageElement, float> GetCharacterResistances(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        var result = new Dictionary<DamageElement, float>();
        var character = data.GetDatabase();
        if (character != null)
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result,
                character.GetCharacterResistances(data.Level));
        return result;
    }

    public static Dictionary<DamageElement, float> GetEquipmentResistances(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        var result = new Dictionary<DamageElement, float>();
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

    public static Dictionary<DamageElement, float> GetBuffResistances(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        var result = new Dictionary<DamageElement, float>();
        var buffs = data.Buffs;
        foreach (var buff in buffs)
        {
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result, buff.GetIncreaseResistances());
        }
        return result;
    }

    public static Dictionary<DamageElement, float> GetResistances(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        var result = data.GetCharacterResistances();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result, data.GetEquipmentResistances());
        if (sumWithBuffs)
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result, data.GetBuffResistances());
        return result;
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetEquipmentIncreaseDamages(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, MinMaxFloat>();
        var result = new Dictionary<DamageElement, MinMaxFloat>();
        // Armors
        Item tempEquipment = null;
        var equipItems = data.EquipItems;
        foreach (var equipItem in equipItems)
        {
            tempEquipment = equipItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineDamageAmountsDictionary(result,
                    tempEquipment.GetIncreaseDamages(equipItem.level));
        }
        // Weapons
        var equipWeapons = data.EquipWeapons;
        // Right hand equipment
        var rightHandItem = equipWeapons.rightHand;
        tempEquipment = rightHandItem.GetEquipmentItem();
        if (tempEquipment != null)
            result = GameDataHelpers.CombineDamageAmountsDictionary(result,
                tempEquipment.GetIncreaseDamages(rightHandItem.level));
        // Left hand equipment
        var leftHandItem = equipWeapons.leftHand;
        tempEquipment = leftHandItem.GetEquipmentItem();
        if (tempEquipment != null)
            result = GameDataHelpers.CombineDamageAmountsDictionary(result,
                tempEquipment.GetIncreaseDamages(leftHandItem.level));
        return result;
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetBuffIncreaseDamages(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, MinMaxFloat>();
        var result = new Dictionary<DamageElement, MinMaxFloat>();
        var buffs = data.Buffs;
        foreach (var buff in buffs)
        {
            result = GameDataHelpers.CombineDamageAmountsDictionary(result, buff.GetIncreaseDamages());
        }
        return result;
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        var result = new Dictionary<DamageElement, MinMaxFloat>();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineDamageAmountsDictionary(result, data.GetEquipmentIncreaseDamages());
        if (sumWithBuffs)
            result = GameDataHelpers.CombineDamageAmountsDictionary(result, data.GetBuffIncreaseDamages());
        return result;
    }

    public static CharacterStats GetCharacterStats(this ICharacterData data)
    {
        if (data == null)
            return new CharacterStats();
        var level = data.Level;
        var character = data.GetDatabase();
        var result = new CharacterStats();
        if (character != null)
            result += character.GetCharacterStats(level);
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

    public static int CountNonEquipItems(this ICharacterData data, string itemId)
    {
        var count = 0;
        if (data != null && data.NonEquipItems.Count > 0)
        {
            var nonEquipItems = data.NonEquipItems;
            foreach (var nonEquipItem in nonEquipItems)
            {
                if (nonEquipItem.itemId.Equals(itemId))
                    count += nonEquipItem.amount;
            }
        }
        return count;
    }

    public static int IndexOfAttribute(this ICharacterData data, string attributeId)
    {
        var list = data.Attributes;
        CharacterAttribute tempAttribute;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempAttribute = list[i];
            if (!string.IsNullOrEmpty(tempAttribute.attributeId) &&
                tempAttribute.attributeId.Equals(attributeId))
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfSkill(this ICharacterData data, string skillId)
    {
        var list = data.Skills;
        CharacterSkill tempSkill;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempSkill = list[i];
            if (!string.IsNullOrEmpty(tempSkill.skillId) &&
                tempSkill.skillId.Equals(skillId))
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfBuff(this ICharacterData data, string characterId, string dataId, BuffType type)
    {
        var list = data.Buffs;
        CharacterBuff tempBuff;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempBuff = list[i];
            if (tempBuff.characterId.Equals(characterId) && tempBuff.dataId.Equals(dataId) && tempBuff.type == type)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfEquipItem(this ICharacterData data, string itemId)
    {
        var list = data.EquipItems;
        CharacterItem tempItem;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempItem = list[i];
            if (!string.IsNullOrEmpty(tempItem.itemId) &&
                tempItem.itemId.Equals(itemId))
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfNonEquipItem(this ICharacterData data, string itemId)
    {
        var list = data.NonEquipItems;
        CharacterItem tempItem;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempItem = list[i];
            if (!string.IsNullOrEmpty(tempItem.itemId) &&
                tempItem.itemId.Equals(itemId))
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
