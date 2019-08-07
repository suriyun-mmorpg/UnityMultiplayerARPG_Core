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
        if (data.DataId == 0)
        {
            // Data has not been set
            return null;
        }

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
        if (!GameInstance.CharacterEntities.TryGetValue(data.EntityId, out entityPrefab))
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
        IEntityMovement[] movements = result.GetComponentsInChildren<IEntityMovement>();
        foreach (IEntityMovement movement in movements)
        {
            movement.enabled = false;
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
    public static float GetTotalItemWeight(IList<CharacterItem> itemList)
    {
        float result = 0f;
        foreach (CharacterItem item in itemList)
        {
            if (!item.NotEmptySlot())
                continue;
            result += item.GetItem().weight;
        }
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
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            if (equipItem.IsEmpty()) continue;
            result = GameDataHelpers.CombineAttributes(result, equipItem.GetIncreaseAttributes());
            result = GameDataHelpers.CombineAttributes(result, equipItem.GetSocketsIncreaseAttributes());
        }
        // Weapons
        EquipWeapons equipWeapons = data.EquipWeapons;
        if (equipWeapons != null)
        {
            // Right hand equipment
            if (!equipWeapons.rightHand.IsEmpty())
            {
                result = GameDataHelpers.CombineAttributes(result, equipWeapons.rightHand.GetIncreaseAttributes());
                result = GameDataHelpers.CombineAttributes(result, equipWeapons.rightHand.GetSocketsIncreaseAttributes());
            }
            // Left hand equipment
            if (!equipWeapons.leftHand.IsEmpty())
            {
                result = GameDataHelpers.CombineAttributes(result, equipWeapons.leftHand.GetIncreaseAttributes());
                result = GameDataHelpers.CombineAttributes(result, equipWeapons.leftHand.GetSocketsIncreaseAttributes());
            }
        }
        return result;
    }

    public static Dictionary<Attribute, short> GetBuffAttributes(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<Attribute, short>();
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
            result = GameDataHelpers.CombineAttributes(result, skill.GetPassiveBuffIncreaseAttributes());
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

    public static Dictionary<Skill, short> GetCharacterSkills(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<Skill, short>();
        // Make dictionary of skills which set in `PlayerCharacter` or `MonsterCharacter`
        Dictionary<Skill, short> result = new Dictionary<Skill, short>(data.GetDatabase().CacheSkillLevels);
        // Combine with skills that character learnt
        IList<CharacterSkill> skills = data.Skills;
        Skill learntSkill;
        short learntSkillLevel;
        foreach (CharacterSkill characterSkill in skills)
        {
            learntSkill = characterSkill.GetSkill();
            learntSkillLevel = characterSkill.level;
            if (learntSkill == null)
                continue;
            if (!result.ContainsKey(learntSkill))
                result[learntSkill] = learntSkillLevel;
            else
                result[learntSkill] += learntSkillLevel;
        }
        return result;
    }

    public static Dictionary<Skill, short> GetEquipmentSkills(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<Skill, short>();
        Dictionary<Skill, short> result = new Dictionary<Skill, short>();
        // Armors
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            if (equipItem.IsEmpty()) continue;
            result = GameDataHelpers.CombineSkills(result, equipItem.GetIncreaseSkills());
            result = GameDataHelpers.CombineSkills(result, equipItem.GetSocketsIncreaseSkills());
        }
        // Weapons
        EquipWeapons equipWeapons = data.EquipWeapons;
        if (equipWeapons != null)
        {
            // Right hand equipment
            if (!equipWeapons.rightHand.IsEmpty())
            {
                result = GameDataHelpers.CombineSkills(result, equipWeapons.rightHand.GetIncreaseSkills());
                result = GameDataHelpers.CombineSkills(result, equipWeapons.rightHand.GetSocketsIncreaseSkills());
            }
            // Left hand equipment
            if (!equipWeapons.leftHand.IsEmpty())
            {
                result = GameDataHelpers.CombineSkills(result, equipWeapons.leftHand.GetIncreaseSkills());
                result = GameDataHelpers.CombineSkills(result, equipWeapons.leftHand.GetSocketsIncreaseSkills());
            }
        }
        return result;
    }

    public static Dictionary<Skill, short> GetSkills(this ICharacterData data, bool sumWithEquipments = true)
    {
        Dictionary<Skill, short> result = data.GetCharacterSkills();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineSkills(result, data.GetEquipmentSkills());
        return result;
    }

    public static Dictionary<DamageElement, float> GetCharacterResistances(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        BaseCharacter character = data.GetDatabase();
        if (character != null)
            result = GameDataHelpers.CombineResistances(result, character.GetCharacterResistances(data.Level));
        return result;
    }

    public static Dictionary<DamageElement, float> GetEquipmentResistances(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        // Armors
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            if (equipItem.IsEmpty()) continue;
            result = GameDataHelpers.CombineResistances(result, equipItem.GetIncreaseResistances());
            result = GameDataHelpers.CombineResistances(result, equipItem.GetSocketsIncreaseResistances());
        }
        // Weapons
        EquipWeapons equipWeapons = data.EquipWeapons;
        if (equipWeapons != null)
        {
            // Right hand equipment
            if (!equipWeapons.rightHand.IsEmpty())
            {
                result = GameDataHelpers.CombineResistances(result, equipWeapons.rightHand.GetIncreaseResistances());
                result = GameDataHelpers.CombineResistances(result, equipWeapons.rightHand.GetSocketsIncreaseResistances());
            }
            // Left hand equipment
            if (!equipWeapons.leftHand.IsEmpty())
            {
                result = GameDataHelpers.CombineResistances(result, equipWeapons.leftHand.GetIncreaseResistances());
                result = GameDataHelpers.CombineResistances(result, equipWeapons.leftHand.GetSocketsIncreaseResistances());
            }
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
            result = GameDataHelpers.CombineResistances(result, skill.GetPassiveBuffIncreaseResistances());
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
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            if (equipItem.IsEmpty()) continue;
            result = GameDataHelpers.CombineDamages(result, equipItem.GetIncreaseDamages());
            result = GameDataHelpers.CombineDamages(result, equipItem.GetSocketsIncreaseDamages());
        }
        // Weapons
        EquipWeapons equipWeapons = data.EquipWeapons;
        if (equipWeapons != null)
        {
            // Right hand equipment
            if (!equipWeapons.rightHand.IsEmpty())
            {
                result = GameDataHelpers.CombineDamages(result, equipWeapons.rightHand.GetIncreaseDamages());
                result = GameDataHelpers.CombineDamages(result, equipWeapons.rightHand.GetSocketsIncreaseDamages());
            }
            // Left hand equipment
            if (!equipWeapons.leftHand.IsEmpty())
            {
                result = GameDataHelpers.CombineDamages(result, equipWeapons.leftHand.GetIncreaseDamages());
                result = GameDataHelpers.CombineDamages(result, equipWeapons.leftHand.GetSocketsIncreaseDamages());
            }
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
            result = GameDataHelpers.CombineDamages(result, skill.GetPassiveBuffIncreaseDamages());
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
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            if (equipItem.IsEmpty()) continue;
            result += equipItem.GetIncreaseStats();
            result += equipItem.GetSocketsIncreaseStats();
            result += GameDataHelpers.GetStatsFromAttributes(equipItem.GetIncreaseAttributes());
            result += GameDataHelpers.GetStatsFromAttributes(equipItem.GetSocketsIncreaseAttributes());
        }
        // Weapons
        EquipWeapons equipWeapons = data.EquipWeapons;
        if (equipWeapons != null)
        {
            // Right hand equipment
            if (!equipWeapons.rightHand.IsEmpty())
            {
                result += equipWeapons.rightHand.GetIncreaseStats();
                result += equipWeapons.rightHand.GetSocketsIncreaseStats();
                result += GameDataHelpers.GetStatsFromAttributes(equipWeapons.rightHand.GetIncreaseAttributes());
                result += GameDataHelpers.GetStatsFromAttributes(equipWeapons.rightHand.GetSocketsIncreaseAttributes());
            }
            // Left hand equipment
            if (!equipWeapons.leftHand.IsEmpty())
            {
                result += equipWeapons.leftHand.GetIncreaseStats();
                result += equipWeapons.leftHand.GetSocketsIncreaseStats();
                result += GameDataHelpers.GetStatsFromAttributes(equipWeapons.leftHand.GetIncreaseAttributes());
                result += GameDataHelpers.GetStatsFromAttributes(equipWeapons.leftHand.GetSocketsIncreaseAttributes());
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
        foreach (CharacterSkill skill in skills)
        {
            if (skill.GetSkill() == null || skill.GetSkill().skillType != SkillType.Passive || skill.level <= 0)
                continue;
            result += skill.GetPassiveBuffIncreaseStats();
            result += GameDataHelpers.GetStatsFromAttributes(skill.GetPassiveBuffIncreaseAttributes());
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

    #region Fill Empty Slots
    public static void FillEmptySlots(IList<CharacterItem> itemList, bool isLimitSlot, short slotLimit)
    {
        if (!isLimitSlot)
        {
            // If it is not limit slots, don't fill it
            return;
        }

        // Fill empty slots
        int i;
        for (i = itemList.Count; i < slotLimit; ++i)
        {
            itemList.Add(CharacterItem.Empty);
        }
        i = itemList.Count - 1;
        while (itemList.Count > slotLimit)
        {
            itemList.RemoveAt(i--);
        }
    }

    public static void FillEmptySlots(this ICharacterData data)
    {
        FillEmptySlots(data.NonEquipItems, GameInstance.Singleton.IsLimitInventorySlot, (short)data.CacheStats.slotLimit);
    }
    #endregion

    #region Increasing Items Will Overwhelming
    public static bool IncreasingItemsWillOverwhelming(IList<CharacterItem> itemList, int dataId, short amount, bool isLimitWeight, short weightLimit, float totalItemWeight, bool isLimitSlot, short slotLimit)
    {
        Item itemData;
        if (amount <= 0 || !GameInstance.Items.TryGetValue(dataId, out itemData))
        {
            // If item not valid
            return false;
        }

        if (isLimitWeight && totalItemWeight > weightLimit)
        {
            // If overwhelming
            return true;
        }

        if (!isLimitSlot)
        {
            // If not limit slot then don't checking for slot amount
            return false;
        }

        short maxStack = itemData.maxStack;
        // Loop to all slots to add amount to any slots that item amount not max in stack
        CharacterItem tempItem;
        for (int i = 0; i < itemList.Count; ++i)
        {
            tempItem = itemList[i];
            if (!tempItem.NotEmptySlot())
            {
                // If current entry is not valid, assume that it is empty slot, so reduce amount of adding item here
                if (amount <= maxStack)
                {
                    // Can add all items, so assume that it is not overwhelming 
                    return false;
                }
                else
                    amount -= maxStack;
            }
            else if (tempItem.dataId == itemData.DataId)
            {
                // If same item id, increase its amount
                if (tempItem.amount + amount <= maxStack)
                {
                    // Can add all items, so assume that it is not overwhelming 
                    return false;
                }
                else if (maxStack - tempItem.amount >= 0)
                    amount -= (short)(maxStack - tempItem.amount);
            }
        }

        int slotCount = itemList.Count;
        // Count adding slot here
        while (amount > 0)
        {
            if (slotCount + 1 > slotLimit)
            {
                // If adding slot is more than slot limit, assume that it is overwhelming 
                return true;
            }
            ++slotCount;
            if (amount <= maxStack)
            {
                // Can add all items, so assume that it is not overwhelming 
                return false;
            }
            else
                amount -= maxStack;
        }

        return true;
    }

    public static bool IncreasingItemsWillOverwhelming(this ICharacterData data, int dataId, short amount)
    {
        return IncreasingItemsWillOverwhelming(data.NonEquipItems, dataId, amount, true, (short)data.CacheStats.weightLimit, data.CacheTotalItemWeight, GameInstance.Singleton.IsLimitInventorySlot, (short)data.CacheStats.slotLimit);
    }
    #endregion

    #region Increase Items
    public static bool IncreaseItems(IList<CharacterItem> itemList, CharacterItem addingItem)
    {
        // If item not valid
        if (!addingItem.NotEmptySlot())
            return false;

        Item itemData = addingItem.GetItem();
        short amount = addingItem.amount;

        short maxStack = itemData.maxStack;
        Dictionary<int, CharacterItem> emptySlots = new Dictionary<int, CharacterItem>();
        Dictionary<int, CharacterItem> changes = new Dictionary<int, CharacterItem>();
        // Loop to all slots to add amount to any slots that item amount not max in stack
        CharacterItem tempNonEquipItem;
        for (int i = 0; i < itemList.Count; ++i)
        {
            tempNonEquipItem = itemList[i];
            if (!tempNonEquipItem.NotEmptySlot())
            {
                // If current entry is not valid, add it to empty list, going to replacing it later
                emptySlots[i] = tempNonEquipItem;
            }
            else if (tempNonEquipItem.dataId == addingItem.dataId)
            {
                // If same item id, increase its amount
                if (tempNonEquipItem.amount + amount <= maxStack)
                {
                    tempNonEquipItem.amount += amount;
                    changes[i] = tempNonEquipItem;
                    amount = 0;
                    break;
                }
                else if (maxStack - tempNonEquipItem.amount >= 0)
                {
                    amount -= (short)(maxStack - tempNonEquipItem.amount);
                    tempNonEquipItem.amount = maxStack;
                    changes[i] = tempNonEquipItem;
                }
            }
        }

        // Adding item to new slots or empty slots if needed
        CharacterItem tempNewItem;
        if (changes.Count == 0 && emptySlots.Count > 0)
        {
            // If there are no changes and there are an empty entries, fill them
            foreach (int emptySlotIndex in emptySlots.Keys)
            {
                tempNewItem = addingItem.Clone();
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
                tempNewItem.amount = addAmount;
                changes[emptySlotIndex] = tempNewItem;
                if (amount == 0)
                    break;
            }
        }

        // Apply all changes
        foreach (KeyValuePair<int, CharacterItem> change in changes)
        {
            itemList[change.Key] = change.Value;
        }

        // Add new items to new slots
        while (amount > 0)
        {
            tempNewItem = addingItem.Clone();
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
            tempNewItem.amount = addAmount;
            itemList.Add(tempNewItem);
            if (amount == 0)
                break;
        }
        return true;
    }

    public static bool IncreaseItems(this ICharacterData data, CharacterItem addingItem)
    {
        return IncreaseItems(data.NonEquipItems, addingItem);
    }
    #endregion

    #region Decrease Items
    public static bool DecreaseItems(IList<CharacterItem> itemList, int dataId, short amount, out Dictionary<CharacterItem, short> decreaseItems)
    {
        decreaseItems = new Dictionary<CharacterItem, short>();
        Dictionary<int, short> decreasingItemIndexes = new Dictionary<int, short>();
        short tempDecresingAmount = 0;
        CharacterItem tempItem;
        for (int i = itemList.Count - 1; i >= 0; --i)
        {
            tempItem = itemList[i];
            if (tempItem.dataId == dataId)
            {
                if (amount - tempItem.amount > 0)
                    tempDecresingAmount = tempItem.amount;
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
            decreaseItems.Add(itemList[decreasingItem.Key], decreasingItem.Value);
            DecreaseItemsByIndex(itemList, decreasingItem.Key, decreasingItem.Value);
        }
        return true;
    }

    public static bool DecreaseItems(this ICharacterData data, int dataId, short amount, out Dictionary<CharacterItem, short> decreaseItems)
    {
        return DecreaseItems(data.NonEquipItems, dataId, amount, out decreaseItems);
    }

    public static bool DecreaseItems(this ICharacterData data, int dataId, short amount)
    {
        Dictionary<CharacterItem, short> decreaseItems;
        return DecreaseItems(data, dataId, amount, out decreaseItems);
    }
    #endregion

    #region Decrease Ammos
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

    public static bool DecreaseAmmos(this ICharacterData data, AmmoType ammoType, short amount)
    {
        Dictionary<CharacterItem, short> decreaseItems;
        return DecreaseAmmos(data, ammoType, amount, out decreaseItems);
    }
    #endregion

    #region Decrease Items By Index
    public static bool DecreaseItemsByIndex(IList<CharacterItem> itemList, int index, short amount)
    {
        if (index < 0 || index >= itemList.Count)
            return false;
        CharacterItem nonEquipItem = itemList[index];
        if (!nonEquipItem.NotEmptySlot() || amount > nonEquipItem.amount)
            return false;
        if (nonEquipItem.amount - amount == 0)
            itemList.RemoveAt(index);
        else
        {
            nonEquipItem.amount -= amount;
            itemList[index] = nonEquipItem;
        }
        return true;
    }

    public static bool DecreaseItemsByIndex(this ICharacterData data, int index, short amount)
    {
        if (DecreaseItemsByIndex(data.NonEquipItems, index, amount))
        {
            data.FillEmptySlots();
            return true;
        }
        return false;
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

    public static int CountAmmos(this ICharacterData data, AmmoType ammoType)
    {
        if (ammoType == null)
            return 0;
        int count = 0;
        if (data != null && data.NonEquipItems.Count > 0)
        {
            Item ammoItem;
            IList<CharacterItem> nonEquipItems = data.NonEquipItems;
            foreach (CharacterItem nonEquipItem in nonEquipItems)
            {
                ammoItem = nonEquipItem.GetAmmoItem();
                if (ammoItem != null && ammoType == ammoItem.ammoType)
                    count += nonEquipItem.amount;
            }
        }
        return count;
    }

    public static CharacterItem GetAvailableWeapon(this ICharacterData data, ref bool isLeftHand)
    {
        CharacterItem rightHand = data.EquipWeapons.rightHand;
        Item rightWeaponItem = rightHand.GetWeaponItem();
        CharacterItem leftHand = data.EquipWeapons.leftHand;
        Item leftWeaponItem = leftHand.GetWeaponItem();
        if (!isLeftHand)
        {
            if (rightWeaponItem != null)
                return rightHand;
            if (rightWeaponItem == null && leftWeaponItem != null)
            {
                isLeftHand = true;
                return leftHand;
            }
        }
        else
        {
            if (leftWeaponItem != null)
                return leftHand;
            if (leftWeaponItem == null && rightWeaponItem != null)
            {
                isLeftHand = false;
                return rightHand;
            }
        }
        isLeftHand = false;
        return CharacterItem.Create(GameInstance.Singleton.DefaultWeaponItem);
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

    public static int IndexOfEquipItem(this ICharacterData data, string id)
    {
        IList<CharacterItem> list = data.EquipItems;
        CharacterItem tempItem;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
        {
            tempItem = list[i];
            if (!string.IsNullOrEmpty(tempItem.id) && tempItem.id.Equals(id))
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfEquipItemByEquipPosition(this ICharacterData data, string equipPosition)
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

    public static bool IsEquipped(this ICharacterData data, string id, out int itemIndex, out CharacterItem characterItem, out InventoryType inventoryType)
    {
        itemIndex = -1;
        characterItem = CharacterItem.Empty;
        inventoryType = InventoryType.NonEquipItems;

        itemIndex = data.IndexOfEquipItem(id);
        if (itemIndex >= 0)
        {
            characterItem = data.EquipItems[itemIndex];
            inventoryType = InventoryType.EquipItems;
            return true;
        }

        if (!string.IsNullOrEmpty(data.EquipWeapons.rightHand.id) && 
            data.EquipWeapons.rightHand.id.Equals(id))
        {
            characterItem = data.EquipWeapons.rightHand;
            inventoryType = InventoryType.EquipWeaponRight;
            return true;
        }

        if (!string.IsNullOrEmpty(data.EquipWeapons.leftHand.id) &&
            data.EquipWeapons.leftHand.id.Equals(id))
        {
            characterItem = data.EquipWeapons.leftHand;
            inventoryType = InventoryType.EquipWeaponLeft;
            return true;
        }

        itemIndex = data.IndexOfNonEquipItem(id);
        if (itemIndex >= 0)
        {
            characterItem = data.NonEquipItems[itemIndex];
            inventoryType = InventoryType.NonEquipItems;
            return false;
        }

        return false;
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

    public static int IndexOfNonEquipItem(this ICharacterData data, string id)
    {
        IList<CharacterItem> list = data.NonEquipItems;
        CharacterItem tempItem;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
        {
            tempItem = list[i];
            if (!string.IsNullOrEmpty(tempItem.id) && tempItem.id.Equals(id))
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

    public static void GetEquipmentSetBonus(this ICharacterData data,
        out CharacterStats bonusStats,
        Dictionary<Attribute, short> bonusAttributes,
        Dictionary<DamageElement, float> bonusResistances,
        Dictionary<DamageElement, MinMaxFloat> bonusIncreaseDamages,
        Dictionary<Skill, short> bonusSkills,
        Dictionary<EquipmentSet, int> equipmentSets)
    {
        bonusStats = new CharacterStats();
        bonusAttributes.Clear();
        bonusResistances.Clear();
        bonusIncreaseDamages.Clear();
        bonusSkills.Clear();
        // Equipment Set
        equipmentSets.Clear();
        // Armor equipment set
        foreach (CharacterItem equipItem in data.EquipItems)
        {
            if (equipItem.NotEmptySlot() && equipItem.GetItem().equipmentSet != null)
            {
                if (equipmentSets.ContainsKey(equipItem.GetItem().equipmentSet))
                    ++equipmentSets[equipItem.GetItem().equipmentSet];
                else
                    equipmentSets.Add(equipItem.GetItem().equipmentSet, 0);
            }
        }
        // Weapon equipment set
        if (data.EquipWeapons != null)
        {
            // Right hand equipment set
            if (data.EquipWeapons.rightHand.NotEmptySlot() && data.EquipWeapons.rightHand.GetItem().equipmentSet != null)
            {
                if (equipmentSets.ContainsKey(data.EquipWeapons.rightHand.GetItem().equipmentSet))
                    ++equipmentSets[data.EquipWeapons.rightHand.GetItem().equipmentSet];
                else
                    equipmentSets.Add(data.EquipWeapons.rightHand.GetItem().equipmentSet, 0);
            }
            // Left hand equipment set
            if (data.EquipWeapons.leftHand.NotEmptySlot() && data.EquipWeapons.leftHand.GetItem().equipmentSet != null)
            {
                if (equipmentSets.ContainsKey(data.EquipWeapons.leftHand.GetItem().equipmentSet))
                    ++equipmentSets[data.EquipWeapons.leftHand.GetItem().equipmentSet];
                else
                    equipmentSets.Add(data.EquipWeapons.leftHand.GetItem().equipmentSet, 0);
            }
        }
        // Apply set items
        Dictionary<Attribute, short> tempIncreaseAttributes;
        Dictionary<DamageElement, float> tempIncreaseResistances;
        Dictionary<DamageElement, MinMaxFloat> tempIncreaseDamages;
        Dictionary<Skill, short> tempIncreaseSkills;
        CharacterStats tempIncreaseStats;
        foreach (KeyValuePair<EquipmentSet, int> cacheEquipmentSet in equipmentSets)
        {
            EquipmentBonus[] effects = cacheEquipmentSet.Key.effects;
            int setAmount = cacheEquipmentSet.Value;
            for (int i = 0; i < setAmount; ++i)
            {
                if (i < effects.Length)
                {
                    // Make temp of data
                    tempIncreaseAttributes = GameDataHelpers.CombineAttributes(effects[i].attributes, null, 1f);
                    tempIncreaseResistances = GameDataHelpers.CombineResistances(effects[i].resistances, null, 1f);
                    tempIncreaseDamages = GameDataHelpers.CombineDamages(effects[i].damages, null, 1f);
                    tempIncreaseSkills = GameDataHelpers.CombineSkills(effects[i].skills, null);
                    tempIncreaseStats = effects[i].stats + GameDataHelpers.GetStatsFromAttributes(tempIncreaseAttributes);
                    // Combine to result dictionaries
                    bonusAttributes = GameDataHelpers.CombineAttributes(bonusAttributes, tempIncreaseAttributes);
                    bonusResistances = GameDataHelpers.CombineResistances(bonusResistances, tempIncreaseResistances);
                    bonusIncreaseDamages = GameDataHelpers.CombineDamages(bonusIncreaseDamages, tempIncreaseDamages);
                    bonusSkills = GameDataHelpers.CombineSkills(bonusSkills, tempIncreaseSkills);
                    bonusStats += tempIncreaseStats;
                }
                else
                    break;
            }
        }
    }

    public static void GetAllStats(this ICharacterData data,
        out CharacterStats resultStats,
        Dictionary<Attribute, short> resultAttributes,
        Dictionary<DamageElement, float> resultResistances,
        Dictionary<DamageElement, MinMaxFloat> resultIncreaseDamages,
        Dictionary<Skill, short> resultSkills,
        Dictionary<EquipmentSet, int> resultEquipmentSets,
        out int resultMaxHp,
        out int resultMaxMp,
        out int resultMaxStamina,
        out int resultMaxFood,
        out int resultMaxWater,
        out float resultTotalItemWeight,
        out float resultAtkSpeed,
        out float resultMoveSpeed)
    {
        resultStats = new CharacterStats();
        resultAttributes.Clear();
        resultResistances.Clear();
        resultIncreaseDamages.Clear();
        resultSkills.Clear();
        resultEquipmentSets.Clear();

        GetEquipmentSetBonus(data, out resultStats, resultAttributes, resultResistances, resultIncreaseDamages, resultSkills, resultEquipmentSets);
        resultStats = resultStats + data.GetStats();
        resultAttributes = GameDataHelpers.CombineAttributes(resultAttributes, data.GetAttributes());
        resultResistances = GameDataHelpers.CombineResistances(resultResistances, data.GetResistances());
        resultIncreaseDamages = GameDataHelpers.CombineDamages(resultIncreaseDamages, data.GetIncreaseDamages());
        resultSkills = GameDataHelpers.CombineSkills(resultSkills, data.GetSkills());
        // Sum with other stats
        resultMaxHp = (int)resultStats.hp;
        resultMaxMp = (int)resultStats.mp;
        resultMaxStamina = (int)resultStats.stamina;
        resultMaxFood = (int)resultStats.food;
        resultMaxWater = (int)resultStats.water;
        resultTotalItemWeight = GameInstance.Singleton.GameplayRule.GetTotalWeight(data);
        resultAtkSpeed = resultStats.atkSpeed;
        resultMoveSpeed = resultStats.moveSpeed;
        // Validate max amount
        foreach (Attribute attribute in new List<Attribute>(resultAttributes.Keys))
        {
            if (attribute.maxAmount > 0 && resultAttributes[attribute] > attribute.maxAmount)
                resultAttributes[attribute] = attribute.maxAmount;
        }
    }
}
