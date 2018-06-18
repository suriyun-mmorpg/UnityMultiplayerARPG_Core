using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterDataExtension
{
    public static BaseCharacter GetDatabase(this ICharacterData data)
    {
        BaseCharacter database = null;
        if (!GameInstance.AllCharacters.TryGetValue(data.DataId, out database))
            return null;

        return database;
    }

    public static CharacterModel InstantiateModel(this ICharacterData data, Transform parent)
    {
        BaseCharacter character = null;
        if (!GameInstance.AllCharacters.TryGetValue(data.DataId, out character))
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

    public static Dictionary<Attribute, short> GetCharacterAttributes(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<Attribute, short>();
        var result = new Dictionary<Attribute, short>();
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

    public static Dictionary<Attribute, short> GetEquipmentAttributes(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<Attribute, short>();
        var result = new Dictionary<Attribute, short>();
        // Armors
        Item tempEquipment = null;
        var equipItems = data.EquipItems;
        foreach (var equipItem in equipItems)
        {
            tempEquipment = equipItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                    tempEquipment.GetIncreaseAttributes(equipItem.level, equipItem.GetEquipmentBonusRate()));
        }
        // Weapons
        var equipWeapons = data.EquipWeapons;
        // Right hand equipment
        var rightHandItem = equipWeapons.rightHand;
        tempEquipment = rightHandItem.GetEquipmentItem();
        if (tempEquipment != null)
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                tempEquipment.GetIncreaseAttributes(rightHandItem.level, rightHandItem.GetEquipmentBonusRate()));
        // Left hand equipment
        var leftHandItem = equipWeapons.leftHand;
        tempEquipment = leftHandItem.GetEquipmentItem();
        if (tempEquipment != null)
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                tempEquipment.GetIncreaseAttributes(leftHandItem.level, leftHandItem.GetEquipmentBonusRate()));

        return result;
    }

    public static Dictionary<Attribute, short> GetBuffAttributes(this ICharacterData data)
    {
        var result = new Dictionary<Attribute, short>();
        var buffs = data.Buffs;
        foreach (var buff in buffs)
        {
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result, buff.GetIncreaseAttributes());
        }

        // Passive skills
        var skills = data.Skills;
        foreach (var skill in skills)
        {
            if (skill.GetSkill() == null || skill.GetSkill().skillType != SkillType.Passive || skill.level <= 0)
                continue;
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result,
                skill.GetSkill().buff.GetIncreaseAttributes(skill.level));
        }
        return result;
    }

    public static Dictionary<Attribute, short> GetAttributes(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        var result = data.GetCharacterAttributes();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result, data.GetEquipmentAttributes());
        if (sumWithBuffs)
            result = GameDataHelpers.CombineAttributeAmountsDictionary(result, data.GetBuffAttributes());
        return result;
    }

    public static Dictionary<Skill, short> GetSkills(this ICharacterData data)
    {
        var result = new Dictionary<Skill, short>();
        // Added skills
        var skills = data.Skills;
        foreach (var characterSkill in skills)
        {
            var key = characterSkill.GetSkill();
            var value = characterSkill.level;
            if (key == null)
                continue;
            if (!result.ContainsKey(key))
                result[key] = value;
            else
                result[key] += value;
        }
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
                    tempEquipment.GetIncreaseResistances(equipItem.level, equipItem.GetEquipmentBonusRate()));
        }
        // Weapons
        var equipWeapons = data.EquipWeapons;
        // Right hand equipment
        var rightHandItem = equipWeapons.rightHand;
        tempEquipment = rightHandItem.GetEquipmentItem();
        if (tempEquipment != null)
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result,
                tempEquipment.GetIncreaseResistances(rightHandItem.level, rightHandItem.GetEquipmentBonusRate()));
        // Left hand equipment
        var leftHandItem = equipWeapons.leftHand;
        tempEquipment = leftHandItem.GetEquipmentItem();
        if (tempEquipment != null)
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result,
                tempEquipment.GetIncreaseResistances(leftHandItem.level, leftHandItem.GetEquipmentBonusRate()));
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

        // Passive skills
        var skills = data.Skills;
        foreach (var skill in skills)
        {
            if (skill.GetSkill() == null || skill.GetSkill().skillType != SkillType.Passive || skill.level <= 0)
                continue;
            result = GameDataHelpers.CombineResistanceAmountsDictionary(result,
                skill.GetSkill().buff.GetIncreaseResistances(skill.level));
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
                    tempEquipment.GetIncreaseDamages(equipItem.level, equipItem.GetEquipmentBonusRate()));
        }
        // Weapons
        var equipWeapons = data.EquipWeapons;
        // Right hand equipment
        var rightHandItem = equipWeapons.rightHand;
        tempEquipment = rightHandItem.GetEquipmentItem();
        if (tempEquipment != null)
            result = GameDataHelpers.CombineDamageAmountsDictionary(result,
                tempEquipment.GetIncreaseDamages(rightHandItem.level, rightHandItem.GetEquipmentBonusRate()));
        // Left hand equipment
        var leftHandItem = equipWeapons.leftHand;
        tempEquipment = leftHandItem.GetEquipmentItem();
        if (tempEquipment != null)
            result = GameDataHelpers.CombineDamageAmountsDictionary(result,
                tempEquipment.GetIncreaseDamages(leftHandItem.level, leftHandItem.GetEquipmentBonusRate()));
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

        // Passive skills
        var skills = data.Skills;
        foreach (var skill in skills)
        {
            if (skill.GetSkill() == null || skill.GetSkill().skillType != SkillType.Passive || skill.level <= 0)
                continue;
            result = GameDataHelpers.CombineDamageAmountsDictionary(result,
                skill.GetSkill().buff.GetIncreaseDamages(skill.level));
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
        foreach (var equipItem in equipItems)
        {
            tempEquipment = equipItem.GetEquipmentItem();
            if (tempEquipment != null)
            {
                result += tempEquipment.GetIncreaseStats(equipItem.level, equipItem.GetEquipmentBonusRate());
                result += GameDataHelpers.CaculateStats(tempEquipment.GetIncreaseAttributes(equipItem.level, equipItem.GetEquipmentBonusRate()));
            }
        }
        // Weapons
        var equipWeapons = data.EquipWeapons;
        // Right hand equipment
        var rightHandItem = equipWeapons.rightHand;
        tempEquipment = rightHandItem.GetEquipmentItem();
        if (tempEquipment != null)
        {
            result += tempEquipment.GetIncreaseStats(rightHandItem.level, rightHandItem.GetEquipmentBonusRate());
            result += GameDataHelpers.CaculateStats(tempEquipment.GetIncreaseAttributes(rightHandItem.level, rightHandItem.GetEquipmentBonusRate()));
        }
        // Left hand equipment
        var leftHandItem = equipWeapons.leftHand;
        tempEquipment = leftHandItem.GetEquipmentItem();
        if (tempEquipment != null)
        {
            result += tempEquipment.GetIncreaseStats(leftHandItem.level, leftHandItem.GetEquipmentBonusRate());
            result += GameDataHelpers.CaculateStats(tempEquipment.GetIncreaseAttributes(leftHandItem.level, leftHandItem.GetEquipmentBonusRate()));
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

        // Passive skills
        var skills = data.Skills;
        foreach (var skill in skills)
        {
            if (skill.GetSkill() == null || skill.GetSkill().skillType != SkillType.Passive || skill.level <= 0)
                continue;
            result += skill.GetSkill().buff.GetIncreaseStats(skill.level);
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

    public static int CountNonEquipItems(this ICharacterData data, int dataId)
    {
        var count = 0;
        if (data != null && data.NonEquipItems.Count > 0)
        {
            var nonEquipItems = data.NonEquipItems;
            foreach (var nonEquipItem in nonEquipItems)
            {
                if (nonEquipItem.dataId == dataId)
                    count += nonEquipItem.amount;
            }
        }
        return count;
    }

    public static bool IncreaseItems(this ICharacterData data, int dataId, short level, short amount)
    {
        Item itemData;
        // If item not valid
        if (amount <= 0 || !GameInstance.Items.TryGetValue(dataId, out itemData))
            return false;

        var maxStack = itemData.maxStack;
        var emptySlots = new Dictionary<int, CharacterItem>();
        var changes = new Dictionary<int, CharacterItem>();
        // Loop to all slots to add amount to any slots that item amount not max in stack
        for (var i = 0; i < data.NonEquipItems.Count; ++i)
        {
            var nonEquipItem = data.NonEquipItems[i];
            if (!nonEquipItem.IsValid())
            {
                // If current entry is not valid, add it to empty list, going to replacing it later
                emptySlots[i] = nonEquipItem;
            }
            else if (nonEquipItem.dataId == dataId)
            {
                // If same item id, increase its amount
                if (nonEquipItem.amount + amount <= maxStack)
                {
                    nonEquipItem.amount += amount;
                    changes[i] = nonEquipItem;
                    amount = 0;
                    break;
                }
                else if (maxStack - nonEquipItem.amount > 0)
                {
                    amount = (short)(maxStack - (maxStack - nonEquipItem.amount));
                    nonEquipItem.amount = maxStack;
                    changes[i] = nonEquipItem;
                }
            }
        }

        if (changes.Count == 0 && emptySlots.Count > 0)
        {
            // If there are no changes and there are an empty entries, fill them
            foreach (var emptySlot in emptySlots)
            {
                var value = emptySlot.Value;
                var newItem = CharacterItem.Create(dataId, level);
                short addAmount = 0;
                if (amount - maxStack >= 0)
                {
                    addAmount = maxStack;
                    amount -= maxStack;
                }
                else
                {
                    addAmount = amount;
                    amount = 0;
                }
                newItem.amount = addAmount;
                changes[emptySlot.Key] = newItem;
            }
        }

        // Apply all changes
        foreach (var change in changes)
        {
            data.NonEquipItems[change.Key] = change.Value;
        }

        // Add new items
        while (amount > 0)
        {
            var newItem = CharacterItem.Create(dataId, level);
            short addAmount = 0;
            if (amount - maxStack >= 0)
            {
                addAmount = maxStack;
                amount -= maxStack;
            }
            else
            {
                addAmount = amount;
                amount = 0;
            }
            newItem.amount = addAmount;
            data.NonEquipItems.Add(newItem);
        }
        return true;
    }

    public static bool DecreaseItems(this ICharacterData data, int dataId, short amount)
    {
        Dictionary<CharacterItem, short> decreaseItems;
        return DecreaseItems(data, dataId, amount, out decreaseItems);
    }

    public static bool DecreaseItems(this ICharacterData data, int dataId, short amount, out Dictionary<CharacterItem, short> decreaseItems)
    {
        decreaseItems = new Dictionary<CharacterItem, short>();
        var decreasingItemIndexes = new Dictionary<int, short>();
        var nonEquipItems = data.NonEquipItems;
        short tempDecresingAmount = 0;
        for (var i = 0; i < nonEquipItems.Count; ++i)
        {
            var nonEquipItem = nonEquipItems[i];
            if (nonEquipItem.dataId == dataId)
            {
                if (amount - nonEquipItem.amount > 0)
                    tempDecresingAmount = nonEquipItem.amount;
                else
                    tempDecresingAmount = amount;
                amount -= tempDecresingAmount;
                decreasingItemIndexes[i] = tempDecresingAmount;
            }
            if (amount == 0)
                break;
        }
        if (amount > 0)
            return false;
        foreach (var decreasingItem in decreasingItemIndexes)
        {
            decreaseItems.Add(data.NonEquipItems[decreasingItem.Key], decreasingItem.Value);
            DecreaseItemsByIndex(data, decreasingItem.Key, decreasingItem.Value);
        }
        return true;
    }

    public static bool DecreaseAmmos(this ICharacterData data, AmmoType ammoType, short amount)
    {
        Dictionary<CharacterItem, short> decreaseItems;
        return DecreaseAmmos(data, ammoType, amount, out decreaseItems);
    }

    public static bool DecreaseAmmos(this ICharacterData data, AmmoType ammoType, short amount, out Dictionary<CharacterItem, short> decreaseItems)
    {
        decreaseItems = new Dictionary<CharacterItem, short>();
        var decreasingItemIndexes = new Dictionary<int, short>();
        var nonEquipItems = data.NonEquipItems;
        short tempDecresingAmount = 0;
        for (var i = 0; i < nonEquipItems.Count; ++i)
        {
            var nonEquipItem = nonEquipItems[i];
            if (nonEquipItem.GetAmmoItem() != null && nonEquipItem.GetAmmoItem().ammoType == ammoType)
            {
                if (amount - nonEquipItem.amount > 0)
                    tempDecresingAmount = nonEquipItem.amount;
                else
                    tempDecresingAmount = amount;
                amount -= tempDecresingAmount;
                decreasingItemIndexes[i] = tempDecresingAmount;
            }
            if (amount == 0)
                break;
        }
        if (amount > 0)
            return false;
        foreach (var decreasingItem in decreasingItemIndexes)
        {
            decreaseItems.Add(data.NonEquipItems[decreasingItem.Key], decreasingItem.Value);
            DecreaseItemsByIndex(data, decreasingItem.Key, decreasingItem.Value);
        }
        return true;
    }

    public static bool DecreaseItemsByIndex(this ICharacterData data, int index, short amount)
    {
        if (index < 0 || index > data.NonEquipItems.Count)
            return false;
        var nonEquipItem = data.NonEquipItems[index];
        if (!nonEquipItem.IsValid() || amount > nonEquipItem.amount)
            return false;
        if (nonEquipItem.amount - amount == 0)
            data.NonEquipItems.RemoveAt(index);
        else
        {
            nonEquipItem.amount -= amount;
            data.NonEquipItems[index] = nonEquipItem;
        }
        return true;
    }

    public static CharacterItem GetRandomedWeapon(this ICharacterData data, out bool isLeftHand)
    {
        isLeftHand = false;
        // Find right hand and left and to set result weapon
        var rightHand = data.EquipWeapons.rightHand;
        var leftHand = data.EquipWeapons.leftHand;
        var rightWeaponItem = rightHand.GetWeaponItem();
        var leftWeaponItem = leftHand.GetWeaponItem();
        if (rightWeaponItem != null && leftWeaponItem != null)
        {
            // Random right hand or left hand weapon
            isLeftHand = Random.Range(0, 1) == 1;
            return !isLeftHand ? rightHand : leftHand;
        }
        else if (rightWeaponItem != null)
        {
            isLeftHand = false;
            return rightHand;
        }
        else if (leftWeaponItem != null)
        {
            isLeftHand = true;
            return leftHand;
        }
        return CharacterItem.Create(GameInstance.Singleton.defaultWeaponItem);
    }

    public static bool CanAttack(this ICharacterData data)
    {
        var rightWeapon = data.EquipWeapons.rightHand.GetWeaponItem();
        var leftWeapon = data.EquipWeapons.leftHand.GetWeaponItem();
        if (rightWeapon != null && leftWeapon != null)
            return leftWeapon.CanAttack(data) && rightWeapon.CanAttack(data);
        else if (rightWeapon != null)
            return rightWeapon.CanAttack(data);
        else if (leftWeapon != null)
            return leftWeapon.CanAttack(data);
        return GameInstance.Singleton.defaultWeaponItem.CanAttack(data);
    }

    public static int IndexOfAttribute(this ICharacterData data, int dataId)
    {
        var list = data.Attributes;
        CharacterAttribute tempAttribute;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempAttribute = list[i];
            if (tempAttribute.dataId == dataId)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfSkill(this ICharacterData data, int dataId)
    {
        var list = data.Skills;
        CharacterSkill tempSkill;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempSkill = list[i];
            if (tempSkill.dataId == dataId)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfBuff(this ICharacterData data, string characterId, int dataId, BuffType type)
    {
        var list = data.Buffs;
        CharacterBuff tempBuff;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempBuff = list[i];
            if (tempBuff.characterId.Equals(characterId) && tempBuff.dataId == dataId && tempBuff.type == type)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfEquipItem(this ICharacterData data, int dataId)
    {
        var list = data.EquipItems;
        CharacterItem tempItem;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempItem = list[i];
            if (tempItem.dataId == dataId)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfNonEquipItem(this ICharacterData data, int dataId)
    {
        var list = data.NonEquipItems;
        CharacterItem tempItem;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempItem = list[i];
            if (tempItem.dataId == dataId)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfAmmoItem(this ICharacterData data, AmmoType ammoType)
    {
        var list = data.NonEquipItems;
        Item tempAmmoItem;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempAmmoItem = list[i].GetAmmoItem();
            if (tempAmmoItem != null && tempAmmoItem.ammoType == ammoType)
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
