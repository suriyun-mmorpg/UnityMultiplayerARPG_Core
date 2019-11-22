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

        if (!GameInstance.Characters.TryGetValue(data.DataId, out database))
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
    public static float GetTotalItemWeight(this IList<CharacterItem> itemList)
    {
        float result = 0f;
        foreach (CharacterItem item in itemList)
        {
            if (item.IsEmptySlot()) continue;
            result += item.GetItem().weight * item.amount;
        }
        return result;
    }

    public static Dictionary<Attribute, float> GetCharacterAttributes(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<Attribute, float>();
        Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
        // Attributes from character database
        if (data.GetDatabase() != null)
            result = GameDataHelpers.CombineAttributes(result, data.GetDatabase().GetCharacterAttributes(data.Level));

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

    public static Dictionary<Attribute, float> GetEquipmentAttributes(this ICharacterData data, Dictionary<Attribute, float> baseAttributes)
    {
        if (data == null)
            return new Dictionary<Attribute, float>();
        Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
        // Increase attributes from armors
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            if (equipItem.IsEmptySlot()) continue;
            result = GameDataHelpers.CombineAttributes(result, equipItem.GetIncreaseAttributes());
            result = GameDataHelpers.CombineAttributes(result, equipItem.GetSocketsIncreaseAttributes());
            // Increase by rate
            result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), equipItem.GetIncreaseAttributesRate()));
            result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), equipItem.GetSocketsIncreaseAttributesRate()));
        }
        // Increase attributes from right hand equipment
        if (data.EquipWeapons.NotEmptyRightHandSlot())
        {
            result = GameDataHelpers.CombineAttributes(result, data.EquipWeapons.rightHand.GetIncreaseAttributes());
            result = GameDataHelpers.CombineAttributes(result, data.EquipWeapons.rightHand.GetSocketsIncreaseAttributes());
            // Increase by rate
            result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.rightHand.GetIncreaseAttributesRate()));
            result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.rightHand.GetSocketsIncreaseAttributesRate()));
        }
        // Increase attributes from left hand equipment
        if (data.EquipWeapons.NotEmptyLeftHandSlot())
        {
            result = GameDataHelpers.CombineAttributes(result, data.EquipWeapons.leftHand.GetIncreaseAttributes());
            result = GameDataHelpers.CombineAttributes(result, data.EquipWeapons.leftHand.GetSocketsIncreaseAttributes());
            // Increase by rate
            result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.leftHand.GetIncreaseAttributesRate()));
            result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.leftHand.GetSocketsIncreaseAttributesRate()));
        }
        return result;
    }

    public static Dictionary<Attribute, float> GetBuffAttributes(this ICharacterData data, Dictionary<Attribute, float> baseAttributes)
    {
        if (data == null)
            return new Dictionary<Attribute, float>();
        Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
        // Increase stats from buffs
        IList<CharacterBuff> buffs = data.Buffs;
        foreach (CharacterBuff buff in buffs)
        {
            result = GameDataHelpers.CombineAttributes(result, buff.GetIncreaseAttributes());
            // Increase with rates
            result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), buff.GetIncreaseAttributesRate()));
        }
        // Increase attributes from passive skills
        foreach (CharacterSkill learnedSkill in data.Skills)
        {
            if (learnedSkill.GetSkill() == null || learnedSkill.GetSkill().GetSkillType() != SkillType.Passive || learnedSkill.level <= 0)
                continue;
            result = GameDataHelpers.CombineAttributes(result, learnedSkill.GetPassiveBuffIncreaseAttributes());
            // Increase with rates
            result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), learnedSkill.GetPassiveBuffIncreaseAttributesRate()));
        }
        return result;
    }

    public static Dictionary<Attribute, float> GetAttributes(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        Dictionary<Attribute, float> result = data.GetCharacterAttributes();
        if (sumWithEquipments || sumWithBuffs)
        {
            Dictionary<Attribute, float> baseAttributes = data.GetCharacterAttributes();
            if (sumWithEquipments)
                result = GameDataHelpers.CombineAttributes(result, data.GetEquipmentAttributes(baseAttributes));
            if (sumWithBuffs)
                result = GameDataHelpers.CombineAttributes(result, data.GetBuffAttributes(baseAttributes));
        }
        return result;
    }

    public static Dictionary<BaseSkill, short> GetCharacterSkills(this ICharacterData data)
    {
        if (data == null || data.GetDatabase() == null)
            return new Dictionary<BaseSkill, short>();
        // Make dictionary of skills which set in `PlayerCharacter` or `MonsterCharacter`
        Dictionary<BaseSkill, short> result = new Dictionary<BaseSkill, short>(data.GetDatabase().CacheSkillLevels);
        // Combine with skills that character learnt
        IList<CharacterSkill> skills = data.Skills;
        BaseSkill learnedSkill;
        short learnedSkillLevel;
        foreach (CharacterSkill characterSkill in skills)
        {
            learnedSkill = characterSkill.GetSkill();
            learnedSkillLevel = characterSkill.level;
            if (learnedSkill == null)
                continue;
            if (!result.ContainsKey(learnedSkill))
                result[learnedSkill] = learnedSkillLevel;
            else
                result[learnedSkill] += learnedSkillLevel;
        }
        return result;
    }

    public static Dictionary<BaseSkill, short> GetEquipmentSkills(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<BaseSkill, short>();
        Dictionary<BaseSkill, short> result = new Dictionary<BaseSkill, short>();
        // Armors
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            if (equipItem.IsEmptySlot()) continue;
            result = GameDataHelpers.CombineSkills(result, equipItem.GetIncreaseSkills());
            result = GameDataHelpers.CombineSkills(result, equipItem.GetSocketsIncreaseSkills());
        }
        // Right hand equipment
        if (data.EquipWeapons.NotEmptyRightHandSlot())
        {
            result = GameDataHelpers.CombineSkills(result, data.EquipWeapons.rightHand.GetIncreaseSkills());
            result = GameDataHelpers.CombineSkills(result, data.EquipWeapons.rightHand.GetSocketsIncreaseSkills());
        }
        // Left hand equipment
        if (data.EquipWeapons.NotEmptyLeftHandSlot())
        {
            result = GameDataHelpers.CombineSkills(result, data.EquipWeapons.leftHand.GetIncreaseSkills());
            result = GameDataHelpers.CombineSkills(result, data.EquipWeapons.leftHand.GetSocketsIncreaseSkills());
        }
        return result;
    }

    public static Dictionary<BaseSkill, short> GetSkills(this ICharacterData data, bool sumWithEquipments = true)
    {
        Dictionary<BaseSkill, short> result = data.GetCharacterSkills();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineSkills(result, data.GetEquipmentSkills());
        return result;
    }

    public static Dictionary<DamageElement, float> GetCharacterResistances(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        if (data.GetDatabase() != null)
            result = GameDataHelpers.CombineResistances(result, data.GetDatabase().GetCharacterResistances(data.Level));
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
            if (equipItem.IsEmptySlot()) continue;
            result = GameDataHelpers.CombineResistances(result, equipItem.GetIncreaseResistances());
            result = GameDataHelpers.CombineResistances(result, equipItem.GetSocketsIncreaseResistances());
        }
        // Right hand equipment
        if (data.EquipWeapons.NotEmptyRightHandSlot())
        {
            result = GameDataHelpers.CombineResistances(result, data.EquipWeapons.rightHand.GetIncreaseResistances());
            result = GameDataHelpers.CombineResistances(result, data.EquipWeapons.rightHand.GetSocketsIncreaseResistances());
        }
        // Left hand equipment
        if (data.EquipWeapons.NotEmptyLeftHandSlot())
        {
            result = GameDataHelpers.CombineResistances(result, data.EquipWeapons.leftHand.GetIncreaseResistances());
            result = GameDataHelpers.CombineResistances(result, data.EquipWeapons.leftHand.GetSocketsIncreaseResistances());
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
        foreach (CharacterSkill learnedSkill in data.Skills)
        {
            if (learnedSkill.GetSkill() == null || learnedSkill.GetSkill().GetSkillType() != SkillType.Passive || learnedSkill.level <= 0)
                continue;
            result = GameDataHelpers.CombineResistances(result, learnedSkill.GetPassiveBuffIncreaseResistances());
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

    public static Dictionary<DamageElement, float> GetCharacterArmors(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        if (data.GetDatabase() != null)
            result = GameDataHelpers.CombineArmors(result, data.GetDatabase().GetCharacterArmors(data.Level));
        return result;
    }

    public static Dictionary<DamageElement, float> GetEquipmentArmors(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        // Armors
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            if (equipItem.IsEmptySlot() || equipItem.GetDefendItem() == null) continue;
            result = GameDataHelpers.CombineArmors(result, equipItem.GetArmorAmount());
            result = GameDataHelpers.CombineArmors(result, equipItem.GetIncreaseArmors());
            result = GameDataHelpers.CombineArmors(result, equipItem.GetSocketsIncreaseArmors());
        }
        // Right hand equipment
        if (data.EquipWeapons.NotEmptyRightHandSlot())
        {
            if (data.EquipWeapons.rightHand.GetDefendItem() != null)
                result = GameDataHelpers.CombineArmors(result, data.EquipWeapons.rightHand.GetArmorAmount());
            result = GameDataHelpers.CombineArmors(result, data.EquipWeapons.rightHand.GetIncreaseArmors());
            result = GameDataHelpers.CombineArmors(result, data.EquipWeapons.rightHand.GetSocketsIncreaseArmors());
        }
        // Left hand equipment
        if (data.EquipWeapons.NotEmptyLeftHandSlot())
        {
            if (data.EquipWeapons.leftHand.GetDefendItem() != null)
                result = GameDataHelpers.CombineArmors(result, data.EquipWeapons.leftHand.GetArmorAmount());
            result = GameDataHelpers.CombineArmors(result, data.EquipWeapons.leftHand.GetIncreaseArmors());
            result = GameDataHelpers.CombineArmors(result, data.EquipWeapons.leftHand.GetSocketsIncreaseArmors());
        }
        return result;
    }

    public static Dictionary<DamageElement, float> GetBuffArmors(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        IList<CharacterBuff> buffs = data.Buffs;
        foreach (CharacterBuff buff in buffs)
        {
            result = GameDataHelpers.CombineArmors(result, buff.GetIncreaseArmors());
        }

        // Passive skills
        foreach (CharacterSkill learnedSkill in data.Skills)
        {
            if (learnedSkill.GetSkill() == null || learnedSkill.GetSkill().GetSkillType() != SkillType.Passive || learnedSkill.level <= 0)
                continue;
            result = GameDataHelpers.CombineArmors(result, learnedSkill.GetPassiveBuffIncreaseArmors());
        }
        return result;
    }

    public static Dictionary<DamageElement, float> GetArmors(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        Dictionary<DamageElement, float> result = data.GetCharacterArmors();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineArmors(result, data.GetEquipmentArmors());
        if (sumWithBuffs)
            result = GameDataHelpers.CombineArmors(result, data.GetBuffArmors());
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
            if (equipItem.IsEmptySlot()) continue;
            result = GameDataHelpers.CombineDamages(result, equipItem.GetIncreaseDamages());
            result = GameDataHelpers.CombineDamages(result, equipItem.GetSocketsIncreaseDamages());
        }
        // Right hand equipment
        if (data.EquipWeapons.NotEmptyRightHandSlot())
        {
            result = GameDataHelpers.CombineDamages(result, data.EquipWeapons.rightHand.GetIncreaseDamages());
            result = GameDataHelpers.CombineDamages(result, data.EquipWeapons.rightHand.GetSocketsIncreaseDamages());
        }
        // Left hand equipment
        if (data.EquipWeapons.NotEmptyLeftHandSlot())
        {
            result = GameDataHelpers.CombineDamages(result, data.EquipWeapons.leftHand.GetIncreaseDamages());
            result = GameDataHelpers.CombineDamages(result, data.EquipWeapons.leftHand.GetSocketsIncreaseDamages());
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
        foreach (CharacterSkill learnedSkill in data.Skills)
        {
            if (learnedSkill.GetSkill() == null || learnedSkill.GetSkill().GetSkillType() != SkillType.Passive || learnedSkill.level <= 0)
                continue;
            result = GameDataHelpers.CombineDamages(result, learnedSkill.GetPassiveBuffIncreaseDamages());
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
        CharacterStats result = new CharacterStats();
        if (data.GetDatabase() != null)
            result += data.GetDatabase().GetCharacterStats(data.Level);
        result += GameDataHelpers.GetStatsFromAttributes(data.GetCharacterAttributes());
        return result;
    }

    public static CharacterStats GetEquipmentStats(this ICharacterData data, CharacterStats baseStats, Dictionary<Attribute, float> baseAttributes)
    {
        if (data == null)
            return new CharacterStats();
        CharacterStats result = new CharacterStats();
        // Increase stats from armors
        IList<CharacterItem> equipItems = data.EquipItems;
        foreach (CharacterItem equipItem in equipItems)
        {
            if (equipItem.IsEmptySlot()) continue;
            result += equipItem.GetIncreaseStats();
            result += equipItem.GetSocketsIncreaseStats();
            result += GameDataHelpers.GetStatsFromAttributes(equipItem.GetIncreaseAttributes());
            result += GameDataHelpers.GetStatsFromAttributes(equipItem.GetSocketsIncreaseAttributes());
            // Increase with rates
            result += baseStats * equipItem.GetIncreaseStatsRate();
            result += baseStats * equipItem.GetSocketsIncreaseStatsRate();
            result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), equipItem.GetIncreaseAttributesRate()));
            result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), equipItem.GetSocketsIncreaseAttributesRate()));
        }
        // Increase stats from right hand equipment
        if (data.EquipWeapons.NotEmptyRightHandSlot())
        {
            result += data.EquipWeapons.rightHand.GetIncreaseStats();
            result += data.EquipWeapons.rightHand.GetSocketsIncreaseStats();
            result += GameDataHelpers.GetStatsFromAttributes(data.EquipWeapons.rightHand.GetIncreaseAttributes());
            result += GameDataHelpers.GetStatsFromAttributes(data.EquipWeapons.rightHand.GetSocketsIncreaseAttributes());
            // Increase with rates
            result += baseStats * data.EquipWeapons.rightHand.GetIncreaseStatsRate();
            result += baseStats * data.EquipWeapons.rightHand.GetSocketsIncreaseStatsRate();
            result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.rightHand.GetIncreaseAttributesRate()));
            result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.rightHand.GetSocketsIncreaseAttributesRate()));
        }
        // Increase stats from left hand equipment
        if (data.EquipWeapons.NotEmptyLeftHandSlot())
        {
            result += data.EquipWeapons.leftHand.GetIncreaseStats();
            result += data.EquipWeapons.leftHand.GetSocketsIncreaseStats();
            result += GameDataHelpers.GetStatsFromAttributes(data.EquipWeapons.leftHand.GetIncreaseAttributes());
            result += GameDataHelpers.GetStatsFromAttributes(data.EquipWeapons.leftHand.GetSocketsIncreaseAttributes());
            // Increase with rates
            result += baseStats * data.EquipWeapons.leftHand.GetIncreaseStatsRate();
            result += baseStats * data.EquipWeapons.leftHand.GetSocketsIncreaseStatsRate();
            result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.leftHand.GetIncreaseAttributesRate()));
            result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.leftHand.GetSocketsIncreaseAttributesRate()));
        }
        return result;
    }

    public static CharacterStats GetBuffStats(this ICharacterData data, CharacterStats baseStats, Dictionary<Attribute, float> baseAttributes)
    {
        if (data == null)
            return new CharacterStats();
        CharacterStats result = new CharacterStats();
        // Increase stats from buffs
        IList<CharacterBuff> buffs = data.Buffs;
        foreach (CharacterBuff buff in buffs)
        {
            result += buff.GetIncreaseStats();
            result += GameDataHelpers.GetStatsFromAttributes(buff.GetIncreaseAttributes());
            // Increase with rates
            result += baseStats * buff.GetIncreaseStatsRate();
            result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), buff.GetIncreaseAttributesRate()));
        }
        // Increase stats from passive skills
        foreach (CharacterSkill learnedSkill in data.Skills)
        {
            if (learnedSkill.GetSkill() == null || learnedSkill.GetSkill().GetSkillType() != SkillType.Passive || learnedSkill.level <= 0)
                continue;
            result += learnedSkill.GetPassiveBuffIncreaseStats();
            result += GameDataHelpers.GetStatsFromAttributes(learnedSkill.GetPassiveBuffIncreaseAttributes());
            // Increase with rates
            result += baseStats * learnedSkill.GetPassiveBuffIncreaseStatsRate();
            result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), learnedSkill.GetPassiveBuffIncreaseAttributesRate()));
        }
        return result;
    }

    public static CharacterStats GetStats(this ICharacterData data, bool sumWithEquipments = true, bool sumWithBuffs = true)
    {
        CharacterStats result = new CharacterStats();
        result += data.GetCharacterStats();
        if (sumWithEquipments || sumWithBuffs)
        {
            // Prepare base stats, it will be multiplied with increase stats rate
            CharacterStats baseStats = new CharacterStats();
            if (data.GetDatabase() != null)
                baseStats += data.GetDatabase().GetCharacterStats(data.Level);
            Dictionary<Attribute, float> baseAttributes = data.GetCharacterAttributes();
            baseStats += GameDataHelpers.GetStatsFromAttributes(baseAttributes);
            // Sum stats with equipments and buffs
            if (sumWithEquipments)
                result += data.GetEquipmentStats(baseStats, baseAttributes);
            if (sumWithBuffs)
                result += data.GetBuffStats(baseStats, baseAttributes);
        }
        return result;
    }
    #endregion

    #region Fill Empty Slots
    public static void FillEmptySlots(this IList<CharacterItem> itemList, bool isLimitSlot, short slotLimit)
    {
        int i;
        if (!isLimitSlot)
        {
            // If it is not limit slots, don't fill it, and also remove empty slots
            for (i = itemList.Count - 1; i >= 0; --i)
            {
                if (itemList[i].IsEmptySlot())
                    itemList.RemoveAt(i);
            }
            return;
        }

        // Fill empty slots
        for (i = itemList.Count; i < slotLimit; ++i)
        {
            itemList.Add(CharacterItem.CreateEmptySlot());
        }

        // Remove empty slots if it's over limit
        for (i = itemList.Count - 1; itemList.Count > slotLimit && i >= 0; --i)
        {
            if (itemList[i].IsEmptySlot())
                itemList.RemoveAt(i);
        }
    }

    public static void FillEmptySlots(this ICharacterData data)
    {
        data.NonEquipItems.FillEmptySlots(GameInstance.Singleton.IsLimitInventorySlot, GameInstance.Singleton.GameplayRule.GetTotalSlot(data));
    }

    public static void FillWeaponSetsIfNeeded(this ICharacterData data, byte equipWeaponSet)
    {
        while (data.SelectableWeaponSets.Count <= equipWeaponSet)
            data.SelectableWeaponSets.Add(new EquipWeapons());
    }
    #endregion

    #region Increasing Items Will Overwhelming
    public static bool IncreasingItemsWillOverwhelming(this IList<CharacterItem> itemList, int dataId, short amount, bool isLimitWeight, float weightLimit, float totalItemWeight, bool isLimitSlot, short slotLimit)
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
            if (tempItem.IsEmptySlot())
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

    public static bool UnEquipItemWillOverwhelming(this ICharacterData data, int unEquipCount = 1)
    {
        if (!GameInstance.Singleton.IsLimitInventorySlot)
            return false;
        int countUsedSlots = 0;
        for (int i = 0; i < data.NonEquipItems.Count; ++i)
        {
            if (!data.NonEquipItems[i].IsEmptySlot())
                ++countUsedSlots;
        }
        return countUsedSlots + unEquipCount > GameInstance.Singleton.GameplayRule.GetTotalSlot(data);
    }

    public static bool IncreasingItemsWillOverwhelming(this ICharacterData data, int dataId, short amount)
    {
        return data.NonEquipItems.IncreasingItemsWillOverwhelming(
            dataId,
            amount,
            true,
            data.GetCaches().Stats.weightLimit,
            data.GetCaches().TotalItemWeight,
            GameInstance.Singleton.IsLimitInventorySlot,
            GameInstance.Singleton.GameplayRule.GetTotalSlot(data));
    }

    public static bool IncreasingItemsWillOverwhelming(this ICharacterData data, IEnumerable<ItemAmount> increasingItems)
    {
        if (increasingItems == null)
            return false;
        List<CharacterItem> tempCharacterItems = new List<CharacterItem>(data.NonEquipItems);
        foreach (ItemAmount receiveItem in increasingItems)
        {
            if (receiveItem.item == null || receiveItem.amount <= 0) continue;
            if (tempCharacterItems.IncreasingItemsWillOverwhelming(
                receiveItem.item.DataId,
                receiveItem.amount,
                true,
                data.GetCaches().Stats.weightLimit,
                GameInstance.Singleton.GameplayRule.GetTotalWeight(data),
                GameInstance.Singleton.IsLimitInventorySlot,
                GameInstance.Singleton.GameplayRule.GetTotalSlot(data)))
            {
                // Overwhelming
                return true;
            }
            else
            {
                // Add item to temp list to check it will overwhelming or not later
                tempCharacterItems.AddOrSetItems(CharacterItem.Create(receiveItem.item, 1, receiveItem.amount));
            }
        }
        return false;
    }
    #endregion

    #region Increase Items
    public static bool IncreaseItems(this IList<CharacterItem> itemList, CharacterItem addingItem)
    {
        // If item not valid
        if (addingItem.IsEmptySlot()) return false;

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
            if (tempNonEquipItem.IsEmptySlot())
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
            itemList.AddOrSetItems(tempNewItem);
            if (amount == 0)
                break;
        }
        return true;
    }

    public static bool IncreaseItems(this ICharacterData data, CharacterItem addingItem)
    {
        if (data.NonEquipItems.IncreaseItems(addingItem))
        {
            data.FillEmptySlots();
            return true;
        }
        return false;
    }
    #endregion

    #region Decrease Items
    public static bool DecreaseItems(this IList<CharacterItem> itemList, int dataId, short amount, out Dictionary<CharacterItem, short> decreaseItems)
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
            itemList.DecreaseItemsByIndex(decreasingItem.Key, decreasingItem.Value);
        }
        return true;
    }

    public static bool DecreaseItems(this ICharacterData data, int dataId, short amount, out Dictionary<CharacterItem, short> decreaseItems)
    {
        return data.NonEquipItems.DecreaseItems(dataId, amount, out decreaseItems);
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
    public static bool DecreaseItemsByIndex(this IList<CharacterItem> itemList, int index, short amount)
    {
        if (index < 0 || index >= itemList.Count)
            return false;
        CharacterItem nonEquipItem = itemList[index];
        if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
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
        if (data.NonEquipItems.DecreaseItemsByIndex(index, amount))
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
        Item rightWeaponItem = data.EquipWeapons.GetRightHandWeaponItem();
        Item leftWeaponItem = data.EquipWeapons.GetLeftHandWeaponItem();
        if (!isLeftHand)
        {
            if (rightWeaponItem != null)
                return data.EquipWeapons.rightHand;
            if (rightWeaponItem == null && leftWeaponItem != null)
            {
                isLeftHand = true;
                return data.EquipWeapons.leftHand;
            }
        }
        else
        {
            if (leftWeaponItem != null)
                return data.EquipWeapons.leftHand;
            if (leftWeaponItem == null && rightWeaponItem != null)
            {
                isLeftHand = false;
                return data.EquipWeapons.rightHand;
            }
        }
        isLeftHand = false;
        return CharacterItem.Create(GameInstance.Singleton.DefaultWeaponItem);
    }

    public static DamageInfo GetWeaponDamageInfo(this ICharacterData data, ref bool isLeftHand)
    {
        if (data is BaseMonsterCharacterEntity)
        {
            BaseMonsterCharacterEntity monsterCharacterEntity = data as BaseMonsterCharacterEntity;
            isLeftHand = false;
            return monsterCharacterEntity.MonsterDatabase.damageInfo;
        }
        else
        {
            return data.GetAvailableWeapon(ref isLeftHand).GetWeaponItem().WeaponType.damageInfo;
        }
    }

    public static KeyValuePair<DamageElement, MinMaxFloat> GetWeaponDamage(this ICharacterData data, ref bool isLeftHand)
    {
        if (data is BaseMonsterCharacterEntity)
        {
            BaseMonsterCharacterEntity monsterCharacterEntity = data as BaseMonsterCharacterEntity;
            isLeftHand = false;
            return GameDataHelpers.MakeDamage(monsterCharacterEntity.MonsterDatabase.damageAmount, monsterCharacterEntity.Level, 1f, 0f);
        }
        else
        {
            return data.GetAvailableWeapon(ref isLeftHand).GetDamageAmount(data);
        }
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

    public static int IndexOfEquipItemByEquipPosition(this ICharacterData data, string equipPosition, byte equipSlotIndex)
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

            if (tempItem.equipSlotIndex == equipSlotIndex &&
                equipPosition.Equals(tempItem.GetEquipmentItem().EquipPosition))
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static bool IsEquipped(
        this ICharacterData data,
        string id,
        out InventoryType inventoryType,
        out int itemIndex,
        out byte equipWeaponSet,
        out CharacterItem characterItem)
    {
        inventoryType = InventoryType.NonEquipItems;
        itemIndex = -1;
        equipWeaponSet = 0;
        characterItem = CharacterItem.Empty;

        itemIndex = data.IndexOfEquipItem(id);
        if (itemIndex >= 0)
        {
            characterItem = data.EquipItems[itemIndex];
            inventoryType = InventoryType.EquipItems;
            return true;
        }

        EquipWeapons tempEquipWeapons;
        for (byte i = 0; i < data.SelectableWeaponSets.Count; ++i)
        {
            tempEquipWeapons = data.SelectableWeaponSets[i];
            if (!string.IsNullOrEmpty(tempEquipWeapons.rightHand.id) &&
                tempEquipWeapons.rightHand.id.Equals(id))
            {
                equipWeaponSet = i;
                characterItem = tempEquipWeapons.rightHand;
                inventoryType = InventoryType.EquipWeaponRight;
                return true;
            }

            if (!string.IsNullOrEmpty(tempEquipWeapons.leftHand.id) &&
                tempEquipWeapons.leftHand.id.Equals(id))
            {
                equipWeaponSet = i;
                characterItem = tempEquipWeapons.leftHand;
                inventoryType = InventoryType.EquipWeaponLeft;
                return true;
            }
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

    public static void AddOrSetNonEquipItems(this ICharacterData data, CharacterItem characterItem)
    {
        data.NonEquipItems.AddOrSetItems(characterItem);
    }

    public static void AddOrSetItems(this IList<CharacterItem> itemList, CharacterItem characterItem)
    {
        int setIndex = IndexOfEmptyItemSlot(itemList);
        if (setIndex >= 0)
        {
            // Insert to empty slot
            itemList[setIndex] = characterItem;
        }
        else
        {
            // Add to last index
            itemList.Add(characterItem);
        }
    }

    public static int IndexOfEmptyNonEquipItemSlot(this ICharacterData data)
    {
        return data.NonEquipItems.IndexOfEmptyItemSlot();
    }

    public static int IndexOfEmptyItemSlot(this IList<CharacterItem> list)
    {
        CharacterItem tempItem;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
        {
            tempItem = list[i];
            if (tempItem.IsEmptySlot())
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
        ref CharacterStats bonusStats,
        Dictionary<Attribute, float> bonusAttributes,
        Dictionary<DamageElement, float> bonusResistances,
        Dictionary<DamageElement, float> bonusArmors,
        Dictionary<DamageElement, MinMaxFloat> bonusDamages,
        Dictionary<BaseSkill, short> bonusSkills,
        Dictionary<EquipmentSet, int> equipmentSets,
        bool combine = false)
    {
        if (!combine)
        {
            bonusStats = new CharacterStats();
            bonusAttributes.Clear();
            bonusResistances.Clear();
            bonusArmors.Clear();
            bonusDamages.Clear();
            bonusSkills.Clear();
            equipmentSets.Clear();
        }

        Item tempEquipmentItem;
        // Armor equipment set
        foreach (CharacterItem equipItem in data.EquipItems)
        {
            tempEquipmentItem = equipItem.GetEquipmentItem();
            if (tempEquipmentItem != null && tempEquipmentItem.equipmentSet != null)
            {
                if (equipmentSets.ContainsKey(tempEquipmentItem.equipmentSet))
                    ++equipmentSets[tempEquipmentItem.equipmentSet];
                else
                    equipmentSets.Add(tempEquipmentItem.equipmentSet, 0);
            }
        }
        // Weapon equipment set
        tempEquipmentItem = data.EquipWeapons.GetRightHandEquipmentItem();
        // Right hand equipment set
        if (tempEquipmentItem != null && tempEquipmentItem.equipmentSet != null)
        {
            if (equipmentSets.ContainsKey(tempEquipmentItem.equipmentSet))
                ++equipmentSets[tempEquipmentItem.equipmentSet];
            else
                equipmentSets.Add(tempEquipmentItem.equipmentSet, 0);
        }
        tempEquipmentItem = data.EquipWeapons.GetLeftHandEquipmentItem();
        // Left hand equipment set
        if (tempEquipmentItem != null && tempEquipmentItem.equipmentSet != null)
        {
            if (equipmentSets.ContainsKey(tempEquipmentItem.equipmentSet))
                ++equipmentSets[tempEquipmentItem.equipmentSet];
            else
                equipmentSets.Add(tempEquipmentItem.equipmentSet, 0);
        }
        // Prepare base stats, it will be multiplied with increase stats rate
        CharacterStats baseStats = new CharacterStats();
        if (data.GetDatabase() != null)
            baseStats += data.GetDatabase().GetCharacterStats(data.Level);
        Dictionary<Attribute, float> baseAttributes = data.GetCharacterAttributes();
        baseStats += GameDataHelpers.GetStatsFromAttributes(baseAttributes);
        // Apply set items
        Dictionary<Attribute, float> tempAttributes;
        Dictionary<Attribute, float> tempAttributesRate;
        Dictionary<DamageElement, float> tempResistances;
        Dictionary<DamageElement, float> tempArmors;
        Dictionary<DamageElement, MinMaxFloat> tempDamages;
        Dictionary<BaseSkill, short> tempSkillLevels;
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
                    tempAttributes = GameDataHelpers.CombineAttributes(effects[i].attributes, null, 1f);
                    tempAttributesRate = GameDataHelpers.CombineAttributes(effects[i].attributesRate, null, 1f);
                    tempResistances = GameDataHelpers.CombineResistances(effects[i].resistances, null, 1f);
                    tempArmors = GameDataHelpers.CombineArmors(effects[i].armors, null, 1f);
                    tempDamages = GameDataHelpers.CombineDamages(effects[i].damages, null, 1f);
                    tempSkillLevels = GameDataHelpers.CombineSkills(effects[i].skills, null);
                    tempIncreaseStats = effects[i].stats + GameDataHelpers.GetStatsFromAttributes(tempAttributes);
                    // Increase with rates
                    tempIncreaseStats += baseStats * effects[i].statsRate;
                    tempIncreaseStats += GameDataHelpers.GetStatsFromAttributes(
                        GameDataHelpers.MultiplyAttributes(
                            new Dictionary<Attribute, float>(baseAttributes),
                            tempAttributesRate));
                    tempAttributes = GameDataHelpers.CombineAttributes(
                        tempAttributes,
                        GameDataHelpers.MultiplyAttributes(
                            new Dictionary<Attribute, float>(baseAttributes),
                            tempAttributesRate));
                    // Combine to result dictionaries
                    bonusAttributes = GameDataHelpers.CombineAttributes(bonusAttributes, tempAttributes);
                    bonusResistances = GameDataHelpers.CombineResistances(bonusResistances, tempResistances);
                    bonusArmors = GameDataHelpers.CombineArmors(bonusArmors, tempArmors);
                    bonusDamages = GameDataHelpers.CombineDamages(bonusDamages, tempDamages);
                    bonusSkills = GameDataHelpers.CombineSkills(bonusSkills, tempSkillLevels);
                    bonusStats += tempIncreaseStats;
                }
                else
                    break;
            }
        }
    }

    public static void GetAllStats(this ICharacterData data,
        ref CharacterStats resultStats,
        Dictionary<Attribute, float> resultAttributes,
        Dictionary<DamageElement, float> resultResistances,
        Dictionary<DamageElement, float> resultArmors,
        Dictionary<DamageElement, MinMaxFloat> resultIncreaseDamages,
        Dictionary<BaseSkill, short> resultSkills,
        Dictionary<EquipmentSet, int> resultEquipmentSets,
        out int resultMaxHp,
        out int resultMaxMp,
        out int resultMaxStamina,
        out int resultMaxFood,
        out int resultMaxWater,
        out float resultTotalItemWeight,
        out float resultAtkSpeed,
        out float resultMoveSpeed,
        bool combine = false)
    {
        if (!combine)
        {
            resultStats = new CharacterStats();
            resultAttributes.Clear();
            resultResistances.Clear();
            resultArmors.Clear();
            resultIncreaseDamages.Clear();
            resultSkills.Clear();
            resultEquipmentSets.Clear();
        }

        resultStats = resultStats + data.GetStats();
        resultAttributes = GameDataHelpers.CombineAttributes(resultAttributes, data.GetAttributes());
        resultResistances = GameDataHelpers.CombineResistances(resultResistances, data.GetResistances());
        resultArmors = GameDataHelpers.CombineArmors(resultArmors, data.GetArmors());
        resultIncreaseDamages = GameDataHelpers.CombineDamages(resultIncreaseDamages, data.GetIncreaseDamages());
        resultSkills = GameDataHelpers.CombineSkills(resultSkills, data.GetSkills());
        GetEquipmentSetBonus(data, ref resultStats, resultAttributes, resultResistances, resultArmors, resultIncreaseDamages, resultSkills, resultEquipmentSets, true);
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
