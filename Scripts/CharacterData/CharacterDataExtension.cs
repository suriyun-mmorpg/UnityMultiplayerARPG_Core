using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using MultiplayerARPG;

public static partial class CharacterDataExtension
{
    public static BaseCharacter GetDatabase(this ICharacterData data)
    {
        BaseCharacter database = null;
        if (!GameInstance.AllCharacters.TryGetValue(data.DataId, out database))
        {
            Debug.LogWarning("[GetDatabase] Cannot find character database with id: " + data.DataId);
            return null;
        }

        return database;
    }

    public static BaseCharacterEntity GetEntityPrefab(this ICharacterData data)
    {
        BaseCharacterEntity entityPrefab = null;
        if (!GameInstance.AllCharacterEntities.TryGetValue(data.EntityId, out entityPrefab))
        {
            Debug.LogWarning("[GetEntityPrefab] Cannot find character entity with id: " + data.EntityId);
            return null;
        }
        return entityPrefab;
    }

    public static BaseCharacterModel InstantiateModel(this ICharacterData data, Transform parent)
    {
        BaseCharacterEntity entityPrefab = data.GetEntityPrefab();
        if (entityPrefab == null)
        {
            Debug.LogWarning("[InstantiateModel] Cannot find character entity with id: " + data.EntityId);
            return null;
        }

        BaseCharacterEntity result = Object.Instantiate(entityPrefab, parent);
        LiteNetLibBehaviour[] networkBehaviours = result.GetComponentsInChildren<LiteNetLibBehaviour>();
        foreach (LiteNetLibBehaviour networkBehaviour in networkBehaviours)
        {
            networkBehaviour.enabled = false;
        }
        GameObject[] ownerObjects = result.ownerObjects;
        foreach (GameObject ownerObject in ownerObjects)
        {
            ownerObject.SetActive(false);
        }
        GameObject[] nonOwnerObjects = result.nonOwnerObjects;
        foreach (GameObject nonOwnerObject in nonOwnerObjects)
        {
            nonOwnerObject.SetActive(false);
        }
        result.gameObject.SetLayerRecursively(GameInstance.Singleton.characterLayer, true);
        result.gameObject.SetActive(true);
        result.transform.localPosition = Vector3.zero;
        return result.CharacterModel;
    }

    public static int GetNextLevelExp(this ICharacterData data)
    {
        short level = data.Level;
        if (level <= 0)
            return 0;
        int[] expTree = GameInstance.Singleton.ExpTree;
        if (level > expTree.Length)
            return 0;
        return expTree[level - 1];
    }

    #region Stats calculation, make saperate stats for buffs calculation
    public static float GetTotalItemWeight(this ICharacterData data)
    {
        float result = 0f;
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            if (!equipItem.IsValid())
                continue;
            result += equipItem.GetItem().weight;
        }
        IList<CharacterItem> nonEquipItems = data.NonEquipItems;
        foreach (CharacterItem nonEquipItem in nonEquipItems)
        {
            if (!nonEquipItem.IsValid())
                continue;
            result += nonEquipItem.GetItem().weight * nonEquipItem.amount;
        }
        EquipWeapons equipWeapons = data.EquipWeapons;
        CharacterItem rightHandItem = equipWeapons.rightHand;
        CharacterItem leftHandItem = equipWeapons.leftHand;
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
        Dictionary<Attribute, short> result = new Dictionary<Attribute, short>();
        // Attributes from character database
        BaseCharacter character = data.GetDatabase();
        if (character != null)
            result = GameDataHelpers.CombineAttributes(result,
                character.GetCharacterAttributes(data.Level));

        // Added attributes
        IList<CharacterAttribute> characterAttributes = data.Attributes;
        foreach (CharacterAttribute characterAttribute in characterAttributes)
        {
            Attribute key = characterAttribute.GetAttribute();
            short value = characterAttribute.amount;
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
        Dictionary<Attribute, short> result = new Dictionary<Attribute, short>();
        // Armors
        Item tempEquipment = null;
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            tempEquipment = equipItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineAttributes(result,
                    tempEquipment.GetIncreaseAttributes(equipItem.level, equipItem.GetEquipmentBonusRate()));
        }
        // Weapons
        EquipWeapons equipWeapons = data.EquipWeapons;
        if (equipWeapons != null)
        {
            // Right hand equipment
            CharacterItem rightHandItem = equipWeapons.rightHand;
            tempEquipment = rightHandItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineAttributes(result,
                    tempEquipment.GetIncreaseAttributes(rightHandItem.level, rightHandItem.GetEquipmentBonusRate()));
            // Left hand equipment
            CharacterItem leftHandItem = equipWeapons.leftHand;
            tempEquipment = leftHandItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineAttributes(result,
                    tempEquipment.GetIncreaseAttributes(leftHandItem.level, leftHandItem.GetEquipmentBonusRate()));
        }
        return result;
    }

    public static Dictionary<Attribute, short> GetBuffAttributes(this ICharacterData data)
    {
        Dictionary<Attribute, short> result = new Dictionary<Attribute, short>();
        IList<CharacterBuff> buffs = data.Buffs;
        foreach (CharacterBuff buff in buffs)
        {
            result = GameDataHelpers.CombineAttributes(result, buff.GetIncreaseAttributes());
        }

        // Passive skills
        IList<CharacterSkill> skills = data.Skills;
        foreach (CharacterSkill skill in skills)
        {
            if (skill.GetSkill() == null || skill.GetSkill().skillType != SkillType.Passive || skill.level <= 0)
                continue;
            result = GameDataHelpers.CombineAttributes(result,
                skill.GetSkill().buff.GetIncreaseAttributes(skill.level));
        }
        return result;
    }

    public static Dictionary<Attribute, short> GetAttributes(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        Dictionary<Attribute, short> result = data.GetCharacterAttributes();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineAttributes(result, data.GetEquipmentAttributes());
        if (sumWithBuffs)
            result = GameDataHelpers.CombineAttributes(result, data.GetBuffAttributes());
        return result;
    }

    public static Dictionary<Skill, short> GetSkills(this ICharacterData data)
    {
        Dictionary<Skill, short> result = new Dictionary<Skill, short>();
        // Added skills
        IList<CharacterSkill> skills = data.Skills;
        foreach (CharacterSkill characterSkill in skills)
        {
            Skill key = characterSkill.GetSkill();
            short value = characterSkill.level;
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
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        BaseCharacter character = data.GetDatabase();
        if (character != null)
            result = GameDataHelpers.CombineResistances(result,
                character.GetCharacterResistances(data.Level));
        return result;
    }

    public static Dictionary<DamageElement, float> GetEquipmentResistances(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        // Armors
        Item tempEquipment = null;
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            tempEquipment = equipItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineResistances(result,
                    tempEquipment.GetIncreaseResistances(equipItem.level, equipItem.GetEquipmentBonusRate()));
        }
        // Weapons
        EquipWeapons equipWeapons = data.EquipWeapons;
        if (equipWeapons != null)
        {
            // Right hand equipment
            CharacterItem rightHandItem = equipWeapons.rightHand;
            tempEquipment = rightHandItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineResistances(result,
                    tempEquipment.GetIncreaseResistances(rightHandItem.level, rightHandItem.GetEquipmentBonusRate()));
            // Left hand equipment
            CharacterItem leftHandItem = equipWeapons.leftHand;
            tempEquipment = leftHandItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineResistances(result,
                    tempEquipment.GetIncreaseResistances(leftHandItem.level, leftHandItem.GetEquipmentBonusRate()));
        }
        return result;
    }

    public static Dictionary<DamageElement, float> GetBuffResistances(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        IList<CharacterBuff> buffs = data.Buffs;
        foreach (CharacterBuff buff in buffs)
        {
            result = GameDataHelpers.CombineResistances(result, buff.GetIncreaseResistances());
        }

        // Passive skills
        IList<CharacterSkill> skills = data.Skills;
        foreach (CharacterSkill skill in skills)
        {
            if (skill.GetSkill() == null || skill.GetSkill().skillType != SkillType.Passive || skill.level <= 0)
                continue;
            result = GameDataHelpers.CombineResistances(result,
                skill.GetSkill().buff.GetIncreaseResistances(skill.level));
        }
        return result;
    }

    public static Dictionary<DamageElement, float> GetResistances(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        Dictionary<DamageElement, float> result = data.GetCharacterResistances();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineResistances(result, data.GetEquipmentResistances());
        if (sumWithBuffs)
            result = GameDataHelpers.CombineResistances(result, data.GetBuffResistances());
        return result;
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetEquipmentIncreaseDamages(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, MinMaxFloat>();
        Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
        // Armors
        Item tempEquipment = null;
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            tempEquipment = equipItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineDamages(result,
                    tempEquipment.GetIncreaseDamages(equipItem.level, equipItem.GetEquipmentBonusRate()));
        }
        // Weapons
        EquipWeapons equipWeapons = data.EquipWeapons;
        if (equipWeapons != null)
        {
            // Right hand equipment
            CharacterItem rightHandItem = equipWeapons.rightHand;
            tempEquipment = rightHandItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineDamages(result,
                    tempEquipment.GetIncreaseDamages(rightHandItem.level, rightHandItem.GetEquipmentBonusRate()));
            // Left hand equipment
            CharacterItem leftHandItem = equipWeapons.leftHand;
            tempEquipment = leftHandItem.GetEquipmentItem();
            if (tempEquipment != null)
                result = GameDataHelpers.CombineDamages(result,
                    tempEquipment.GetIncreaseDamages(leftHandItem.level, leftHandItem.GetEquipmentBonusRate()));
        }
        return result;
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetBuffIncreaseDamages(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, MinMaxFloat>();
        Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
        IList<CharacterBuff> buffs = data.Buffs;
        foreach (CharacterBuff buff in buffs)
        {
            result = GameDataHelpers.CombineDamages(result, buff.GetIncreaseDamages());
        }

        // Passive skills
        IList<CharacterSkill> skills = data.Skills;
        foreach (CharacterSkill skill in skills)
        {
            if (skill.GetSkill() == null || skill.GetSkill().skillType != SkillType.Passive || skill.level <= 0)
                continue;
            result = GameDataHelpers.CombineDamages(result,
                skill.GetSkill().buff.GetIncreaseDamages(skill.level));
        }
        return result;
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineDamages(result, data.GetEquipmentIncreaseDamages());
        if (sumWithBuffs)
            result = GameDataHelpers.CombineDamages(result, data.GetBuffIncreaseDamages());
        return result;
    }

    public static CharacterStats GetCharacterStats(this ICharacterData data)
    {
        if (data == null)
            return new CharacterStats();
        short level = data.Level;
        BaseCharacter character = data.GetDatabase();
        CharacterStats result = new CharacterStats();
        if (character != null)
            result += character.GetCharacterStats(level);
        result += GameDataHelpers.GetStatsFromAttributes(GetAttributes(data));
        return result;
    }

    public static CharacterStats GetEquipmentStats(this ICharacterData data)
    {
        if (data == null)
            return new CharacterStats();
        CharacterStats result = new CharacterStats();
        // Armors
        Item tempEquipment = null;
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            tempEquipment = equipItem.GetEquipmentItem();
            if (tempEquipment != null)
            {
                result += tempEquipment.GetIncreaseStats(equipItem.level, equipItem.GetEquipmentBonusRate());
                result += GameDataHelpers.GetStatsFromAttributes(tempEquipment.GetIncreaseAttributes(equipItem.level, equipItem.GetEquipmentBonusRate()));
            }
        }
        // Weapons
        EquipWeapons equipWeapons = data.EquipWeapons;
        if (equipWeapons != null)
        {
            // Right hand equipment
            CharacterItem rightHandItem = equipWeapons.rightHand;
            tempEquipment = rightHandItem.GetEquipmentItem();
            if (tempEquipment != null)
            {
                result += tempEquipment.GetIncreaseStats(rightHandItem.level, rightHandItem.GetEquipmentBonusRate());
                result += GameDataHelpers.GetStatsFromAttributes(tempEquipment.GetIncreaseAttributes(rightHandItem.level, rightHandItem.GetEquipmentBonusRate()));
            }
            // Left hand equipment
            CharacterItem leftHandItem = equipWeapons.leftHand;
            tempEquipment = leftHandItem.GetEquipmentItem();
            if (tempEquipment != null)
            {
                result += tempEquipment.GetIncreaseStats(leftHandItem.level, leftHandItem.GetEquipmentBonusRate());
                result += GameDataHelpers.GetStatsFromAttributes(tempEquipment.GetIncreaseAttributes(leftHandItem.level, leftHandItem.GetEquipmentBonusRate()));
            }
        }
        return result;
    }

    public static CharacterStats GetBuffStats(this ICharacterData data)
    {
        if (data == null)
            return new CharacterStats();
        CharacterStats result = new CharacterStats();
        IList<CharacterBuff> buffs = data.Buffs;
        foreach (CharacterBuff buff in buffs)
        {
            result += buff.GetIncreaseStats();
            result += GameDataHelpers.GetStatsFromAttributes(buff.GetIncreaseAttributes());
        }

        // Passive skills
        IList<CharacterSkill> skills = data.Skills;
        Buff tempBuff;
        foreach (CharacterSkill skill in skills)
        {
            if (skill.GetSkill() == null || skill.GetSkill().skillType != SkillType.Passive || skill.level <= 0)
                continue;
            tempBuff = skill.GetSkill().buff;
            result += tempBuff.GetIncreaseStats(skill.level);
            result += GameDataHelpers.GetStatsFromAttributes(tempBuff.GetIncreaseAttributes(skill.level));
        }
        return result;
    }

    public static CharacterStats GetStats(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        CharacterStats result = data.GetCharacterStats();
        if (sumWithEquipments)
            result += data.GetEquipmentStats();
        if (sumWithBuffs)
            result += data.GetBuffStats();
        return result;
    }
    #endregion

    public static int CountNonEquipItems(this ICharacterData data, int dataId)
    {
        int count = 0;
        if (data != null && data.NonEquipItems.Count > 0)
        {
            IList<CharacterItem> nonEquipItems = data.NonEquipItems;
            foreach (CharacterItem nonEquipItem in nonEquipItems)
            {
                if (nonEquipItem.dataId == dataId)
                    count += nonEquipItem.amount;
            }
        }
        return count;
    }

    public static bool IncreaseItems(this ICharacterData data, CharacterItem dropData)
    {
        // If item not valid
        if (!dropData.IsValid())
            return false;

        Item itemData = dropData.GetItem();
        short amount = dropData.amount;

        short maxStack = itemData.maxStack;
        Dictionary<int, CharacterItem> emptySlots = new Dictionary<int, CharacterItem>();
        Dictionary<int, CharacterItem> changes = new Dictionary<int, CharacterItem>();
        // Loop to all slots to add amount to any slots that item amount not max in stack
        for (int i = 0; i < data.NonEquipItems.Count; ++i)
        {
            CharacterItem nonEquipItem = data.NonEquipItems[i];
            if (!nonEquipItem.IsValid())
            {
                // If current entry is not valid, add it to empty list, going to replacing it later
                emptySlots[i] = nonEquipItem;
            }
            else if (nonEquipItem.dataId == dropData.dataId)
            {
                // If same item id, increase its amount
                if (nonEquipItem.amount + amount <= maxStack)
                {
                    nonEquipItem.amount += amount;
                    changes[i] = nonEquipItem;
                    amount = 0;
                    break;
                }
                else if (maxStack - nonEquipItem.amount >= 0)
                {
                    amount = (short)(amount - (maxStack - nonEquipItem.amount));
                    nonEquipItem.amount = maxStack;
                    changes[i] = nonEquipItem;
                }
            }
        }

        if (changes.Count == 0 && emptySlots.Count > 0)
        {
            // If there are no changes and there are an empty entries, fill them
            foreach (KeyValuePair<int, CharacterItem> emptySlot in emptySlots)
            {
                CharacterItem value = emptySlot.Value;
                CharacterItem newItem = dropData.Clone();
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
        foreach (KeyValuePair<int, CharacterItem> change in changes)
        {
            data.NonEquipItems[change.Key] = change.Value;
        }

        // Add new items
        while (amount > 0)
        {
            CharacterItem newItem = dropData.Clone();
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
        Dictionary<int, short> decreasingItemIndexes = new Dictionary<int, short>();
        IList<CharacterItem> nonEquipItems = data.NonEquipItems;
        short tempDecresingAmount = 0;
        for (int i = nonEquipItems.Count - 1; i >= 0; --i)
        {
            CharacterItem nonEquipItem = nonEquipItems[i];
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
        foreach (KeyValuePair<int, short> decreasingItem in decreasingItemIndexes)
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
        Dictionary<int, short> decreasingItemIndexes = new Dictionary<int, short>();
        IList<CharacterItem> nonEquipItems = data.NonEquipItems;
        short tempDecresingAmount = 0;
        for (int i = nonEquipItems.Count - 1; i >= 0; --i)
        {
            CharacterItem nonEquipItem = nonEquipItems[i];
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
        foreach (KeyValuePair<int, short> decreasingItem in decreasingItemIndexes)
        {
            decreaseItems.Add(data.NonEquipItems[decreasingItem.Key], decreasingItem.Value);
            DecreaseItemsByIndex(data, decreasingItem.Key, decreasingItem.Value);
        }
        return true;
    }

    public static bool DecreaseItemsByIndex(this ICharacterData data, int index, short amount)
    {
        if (index < 0 || index >= data.NonEquipItems.Count)
            return false;
        CharacterItem nonEquipItem = data.NonEquipItems[index];
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
        CharacterItem rightHand = data.EquipWeapons.rightHand;
        CharacterItem leftHand = data.EquipWeapons.leftHand;
        Item rightWeaponItem = rightHand.GetWeaponItem();
        Item leftWeaponItem = leftHand.GetWeaponItem();
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
        return CharacterItem.Create(GameInstance.Singleton.DefaultWeaponItem);
    }

    public static bool CanAttack(this ICharacterData data)
    {
        Item rightWeapon = data.EquipWeapons.rightHand.GetWeaponItem();
        Item leftWeapon = data.EquipWeapons.leftHand.GetWeaponItem();
        if (rightWeapon != null && leftWeapon != null)
            return leftWeapon.CanAttack(data) && rightWeapon.CanAttack(data);
        else if (rightWeapon != null)
            return rightWeapon.CanAttack(data);
        else if (leftWeapon != null)
            return leftWeapon.CanAttack(data);
        return GameInstance.Singleton.DefaultWeaponItem.CanAttack(data);
    }

    public static int IndexOfAttribute(this ICharacterData data, int dataId)
    {
        IList<CharacterAttribute> list = data.Attributes;
        CharacterAttribute tempAttribute;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
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
        IList<CharacterSkill> list = data.Skills;
        CharacterSkill tempSkill;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
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

    public static int IndexOfSkillUsage(this ICharacterData data, int dataId, SkillUsageType type)
    {
        IList<CharacterSkillUsage> list = data.SkillUsages;
        CharacterSkillUsage tempSkillUsage;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
        {
            tempSkillUsage = list[i];
            if (tempSkillUsage.dataId == dataId && tempSkillUsage.type == type)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfBuff(this ICharacterData data, int dataId, BuffType type)
    {
        IList<CharacterBuff> list = data.Buffs;
        CharacterBuff tempBuff;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
        {
            tempBuff = list[i];
            if (tempBuff.dataId == dataId && tempBuff.type == type)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfEquipItem(this ICharacterData data, int dataId)
    {
        IList<CharacterItem> list = data.EquipItems;
        CharacterItem tempItem;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
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

    public static int IndexOfEquipItem(this ICharacterData data, string equipPosition)
    {
        if (string.IsNullOrEmpty(equipPosition))
            return -1;

        IList<CharacterItem> list = data.EquipItems;
        CharacterItem tempItem;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
        {
            tempItem = list[i];
            if (tempItem.GetEquipmentItem() == null)
                continue;

            if (equipPosition.Equals(tempItem.GetEquipmentItem().EquipPosition))
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfNonEquipItem(this ICharacterData data, int dataId)
    {
        IList<CharacterItem> list = data.NonEquipItems;
        CharacterItem tempItem;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
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

    public static int IndexOfSummon(this ICharacterData data, uint objectId)
    {
        IList<CharacterSummon> list = data.Summons;
        CharacterSummon tempSummon;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
        {
            tempSummon = list[i];
            if (tempSummon.objectId == objectId)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfSummon(this ICharacterData data, SummonType type)
    {
        IList<CharacterSummon> list = data.Summons;
        CharacterSummon tempSummon;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
        {
            tempSummon = list[i];
            if (tempSummon.type == type)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfSummon(this ICharacterData data, int dataId, SummonType type)
    {
        IList<CharacterSummon> list = data.Summons;
        CharacterSummon tempSummon;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
        {
            tempSummon = list[i];
            if (tempSummon.dataId == dataId && tempSummon.type == type)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfAmmoItem(this ICharacterData data, AmmoType ammoType)
    {
        IList<CharacterItem> list = data.NonEquipItems;
        Item tempAmmoItem;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
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

    public static void GetAllStats(this ICharacterData data,
        out CharacterStats cacheStats,
        out Dictionary<Attribute, short> cacheAttributes,
        out Dictionary<Skill, short> cacheSkills,
        out Dictionary<DamageElement, float> cacheResistances,
        out Dictionary<DamageElement, MinMaxFloat> cacheIncreaseDamages,
        out Dictionary<EquipmentSet, int> cacheEquipmentSets,
        out int cacheMaxHp,
        out int cacheMaxMp,
        out int cacheMaxStamina,
        out int cacheMaxFood,
        out int cacheMaxWater,
        out float cacheTotalItemWeight,
        out float cacheAtkSpeed,
        out float cacheMoveSpeed)
    {
        cacheStats = data.GetStats();
        cacheAttributes = data.GetAttributes();
        cacheSkills = data.GetSkills();
        cacheResistances = data.GetResistances();
        cacheIncreaseDamages = data.GetIncreaseDamages();
        // Equipment Set
        cacheEquipmentSets = new Dictionary<EquipmentSet, int>();
        // Armor equipment set
        foreach (CharacterItem equipItem in data.EquipItems)
        {
            if (equipItem.IsValid() && equipItem.GetItem().equipmentSet != null)
            {
                if (cacheEquipmentSets.ContainsKey(equipItem.GetItem().equipmentSet))
                    ++cacheEquipmentSets[equipItem.GetItem().equipmentSet];
                else
                    cacheEquipmentSets.Add(equipItem.GetItem().equipmentSet, 0);
            }
        }
        if (data.EquipWeapons != null)
        {
            // Right hand equipment set
            if (data.EquipWeapons.rightHand.IsValid() && data.EquipWeapons.rightHand.GetItem().equipmentSet != null)
            {
                if (cacheEquipmentSets.ContainsKey(data.EquipWeapons.rightHand.GetItem().equipmentSet))
                    ++cacheEquipmentSets[data.EquipWeapons.rightHand.GetItem().equipmentSet];
                else
                    cacheEquipmentSets.Add(data.EquipWeapons.rightHand.GetItem().equipmentSet, 0);
            }
            // Left hand equipment set
            if (data.EquipWeapons.leftHand.IsValid() && data.EquipWeapons.leftHand.GetItem().equipmentSet != null)
            {
                if (cacheEquipmentSets.ContainsKey(data.EquipWeapons.leftHand.GetItem().equipmentSet))
                    ++cacheEquipmentSets[data.EquipWeapons.leftHand.GetItem().equipmentSet];
                else
                    cacheEquipmentSets.Add(data.EquipWeapons.leftHand.GetItem().equipmentSet, 0);
            }
        }
        // Apply set items
        foreach (KeyValuePair<EquipmentSet, int> cacheEquipmentSet in cacheEquipmentSets)
        {
            EquipmentSetEffect[] effects = cacheEquipmentSet.Key.effects;
            int setAmount = cacheEquipmentSet.Value;
            for (int i = 0; i < setAmount; ++i)
            {
                if (i < effects.Length)
                {
                    cacheStats += effects[i].stats;
                    cacheAttributes = GameDataHelpers.CombineAttributes(cacheAttributes, 
                        GameDataHelpers.MakeAttributes(effects[i].attributes, null, 1f));
                    cacheResistances = GameDataHelpers.CombineResistances(cacheResistances,
                        GameDataHelpers.MakeResistances(effects[i].resistances, null, 1f));
                    cacheIncreaseDamages = GameDataHelpers.CombineDamages(cacheIncreaseDamages,
                        GameDataHelpers.MakeDamages(effects[i].damages, null, 1f));
                }
                else
                    break;
            }
        }
        // Sum with other stats
        cacheMaxHp = (int)cacheStats.hp;
        cacheMaxMp = (int)cacheStats.mp;
        cacheMaxStamina = (int)cacheStats.stamina;
        cacheMaxFood = (int)cacheStats.food;
        cacheMaxWater = (int)cacheStats.water;
        cacheTotalItemWeight = data.GetTotalItemWeight();
        cacheAtkSpeed = cacheStats.atkSpeed;
        cacheMoveSpeed = cacheStats.moveSpeed;
        // Validate max amount
        foreach (Attribute attribute in new List<Attribute>(cacheAttributes.Keys))
        {
            if (attribute.maxAmount > 0 && cacheAttributes[attribute] > attribute.maxAmount)
                cacheAttributes[attribute] = attribute.maxAmount;
        }
    }
}
