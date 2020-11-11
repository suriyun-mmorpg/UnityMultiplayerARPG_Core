using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using MultiplayerARPG;

public static partial class CharacterDataExtension
{
    public static BaseCharacter GetDatabase(this ICharacterData data)
    {
        if (data == null || data.DataId == 0)
        {
            // Data has not been set
            return null;
        }

        BaseCharacter database;
        if (!GameInstance.Characters.TryGetValue(data.DataId, out database))
        {
            Logging.LogWarning("[GetDatabase] Cannot find character database with id: " + data.DataId);
            return null;
        }

        return database;
    }

    public static BaseCharacterEntity GetEntityPrefab(this ICharacterData data)
    {
        BaseCharacterEntity entityPrefab;
        if (!GameInstance.CharacterEntities.TryGetValue(data.EntityId, out entityPrefab))
        {
            Logging.LogWarning("[GetEntityPrefab] Cannot find character entity with id: " + data.EntityId);
            return null;
        }
        return entityPrefab;
    }

    public static BaseCharacterModel InstantiateModel(this ICharacterData data, Transform parent)
    {
        BaseCharacterEntity entityPrefab = data.GetEntityPrefab();
        if (entityPrefab == null)
        {
            Logging.LogWarning("[InstantiateModel] Cannot find character entity with id: " + data.EntityId);
            return null;
        }

        BaseCharacterEntity result = Object.Instantiate(entityPrefab, parent);
        LiteNetLibBehaviour[] networkBehaviours = result.GetComponentsInChildren<LiteNetLibBehaviour>();
        foreach (LiteNetLibBehaviour networkBehaviour in networkBehaviours)
        {
            networkBehaviour.enabled = false;
        }
        GameObject[] ownerObjects = result.OwnerObjects;
        foreach (GameObject ownerObject in ownerObjects)
        {
            ownerObject.SetActive(false);
        }
        GameObject[] nonOwnerObjects = result.NonOwnerObjects;
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
            result += item.GetItem().Weight * item.amount;
        }
        return result;
    }

    public static short GetTotalItemSlot(this IList<CharacterItem> itemList)
    {
        short result = 0;
        foreach (CharacterItem item in itemList)
        {
            if (item.IsEmptySlot()) continue;
            result++;
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
        foreach (CharacterAttribute characterAttribute in data.Attributes)
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
        foreach (CharacterItem equipItem in data.EquipItems)
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
        foreach (CharacterBuff buff in data.Buffs)
        {
            result = GameDataHelpers.CombineAttributes(result, buff.GetIncreaseAttributes());
            // Increase with rates
            result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), buff.GetIncreaseAttributesRate()));
        }
        return result;
    }

    public static Dictionary<Attribute, float> GetPassiveSkillAttributes(this ICharacterData data, Dictionary<Attribute, float> baseAttributes)
    {
        if (data == null)
            return new Dictionary<Attribute, float>();
        return data.GetSkills(true).GetPassiveSkillAttributes(baseAttributes);
    }

    public static Dictionary<Attribute, float> GetPassiveSkillAttributes(this Dictionary<BaseSkill, short> skills, Dictionary<Attribute, float> baseAttributes)
    {
        if (skills == null)
            return new Dictionary<Attribute, float>();
        Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
        foreach (KeyValuePair<BaseSkill, short> skill in skills)
        {
            if (skill.Key == null || !skill.Key.IsPassive() || skill.Value <= 0)
                continue;
            result = GameDataHelpers.CombineAttributes(result, skill.Key.GetBuff().GetIncreaseAttributes(skill.Value));
            // Increase with rates
            result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), skill.Key.GetBuff().GetIncreaseAttributesRate(skill.Value)));
        }
        return result;
    }

    public static Dictionary<Attribute, float> GetAttributes(this ICharacterData data, bool sumWithEquipments, bool sumWithBuffs, Dictionary<BaseSkill, short> skills)
    {
        Dictionary<Attribute, float> result = data.GetCharacterAttributes();
        if (sumWithEquipments || sumWithBuffs)
        {
            Dictionary<Attribute, float> baseAttributes = data.GetCharacterAttributes();
            if (sumWithEquipments)
                result = GameDataHelpers.CombineAttributes(result, data.GetEquipmentAttributes(baseAttributes));
            if (sumWithBuffs)
                result = GameDataHelpers.CombineAttributes(result, data.GetBuffAttributes(baseAttributes));
            if (skills != null)
                result = GameDataHelpers.CombineAttributes(result, skills.GetPassiveSkillAttributes(baseAttributes));
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
        BaseSkill learnedSkill;
        short learnedSkillLevel;
        foreach (CharacterSkill characterSkill in data.Skills)
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
        foreach (CharacterItem equipItem in data.EquipItems)
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

    public static Dictionary<BaseSkill, short> GetSkills(this ICharacterData data, bool sumWithEquipments)
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
        foreach (CharacterItem equipItem in data.EquipItems)
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
        foreach (CharacterBuff buff in data.Buffs)
        {
            result = GameDataHelpers.CombineResistances(result, buff.GetIncreaseResistances());
        }
        return result;
    }

    public static Dictionary<DamageElement, float> GetPassiveSkillResistances(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        return data.GetSkills(true).GetPassiveSkillResistances();
    }

    public static Dictionary<DamageElement, float> GetPassiveSkillResistances(this Dictionary<BaseSkill, short> skills)
    {
        if (skills == null)
            return new Dictionary<DamageElement, float>();
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        foreach (KeyValuePair<BaseSkill, short> skill in skills)
        {
            if (skill.Key == null || !skill.Key.IsPassive() || skill.Value <= 0)
                continue;
            result = GameDataHelpers.CombineResistances(result, skill.Key.GetBuff().GetIncreaseResistances(skill.Value));
        }
        return result;
    }

    public static Dictionary<DamageElement, float> GetResistances(this ICharacterData data, bool sumWithEquipments, bool sumWithBuffs, Dictionary<BaseSkill, short> skills)
    {
        Dictionary<DamageElement, float> result = data.GetCharacterResistances();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineResistances(result, data.GetEquipmentResistances());
        if (sumWithBuffs)
            result = GameDataHelpers.CombineResistances(result, data.GetBuffResistances());
        if (skills != null)
            result = GameDataHelpers.CombineResistances(result, skills.GetPassiveSkillResistances());
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
        foreach (CharacterItem equipItem in data.EquipItems)
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
        foreach (CharacterBuff buff in data.Buffs)
        {
            result = GameDataHelpers.CombineArmors(result, buff.GetIncreaseArmors());
        }
        return result;
    }

    public static Dictionary<DamageElement, float> GetPassiveSkillArmors(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, float>();
        return data.GetPassiveSkillArmors();
    }

    public static Dictionary<DamageElement, float> GetPassiveSkillArmors(this Dictionary<BaseSkill, short> skills)
    {
        if (skills == null)
            return new Dictionary<DamageElement, float>();
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        foreach (KeyValuePair<BaseSkill, short> skill in skills)
        {
            if (skill.Key == null || !skill.Key.IsPassive() || skill.Value <= 0)
                continue;
            result = GameDataHelpers.CombineArmors(result, skill.Key.GetBuff().GetIncreaseArmors(skill.Value));
        }
        return result;
    }

    public static Dictionary<DamageElement, float> GetArmors(this ICharacterData data, bool sumWithEquipments, bool sumWithBuffs, Dictionary<BaseSkill, short> skills)
    {
        Dictionary<DamageElement, float> result = data.GetCharacterArmors();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineArmors(result, data.GetEquipmentArmors());
        if (sumWithBuffs)
            result = GameDataHelpers.CombineArmors(result, data.GetBuffArmors());
        if (skills != null)
            result = GameDataHelpers.CombineArmors(result, skills.GetPassiveSkillArmors());
        return result;
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetEquipmentIncreaseDamages(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, MinMaxFloat>();
        Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
        // Armors
        foreach (CharacterItem equipItem in data.EquipItems)
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
        foreach (CharacterBuff buff in data.Buffs)
        {
            result = GameDataHelpers.CombineDamages(result, buff.GetIncreaseDamages());
        }
        return result;
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetPassiveSkillIncreaseDamages(this ICharacterData data)
    {
        if (data == null)
            return new Dictionary<DamageElement, MinMaxFloat>();
        return data.GetSkills(true).GetPassiveSkillIncreaseDamages();
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetPassiveSkillIncreaseDamages(this Dictionary<BaseSkill, short> skills)
    {
        if (skills == null)
            return new Dictionary<DamageElement, MinMaxFloat>();
        Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
        foreach (KeyValuePair<BaseSkill, short> skill in skills)
        {
            if (skill.Key == null || !skill.Key.IsPassive() || skill.Value <= 0)
                continue;
            result = GameDataHelpers.CombineDamages(result, skill.Key.GetBuff().GetIncreaseDamages(skill.Value));
        }
        return result;
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this ICharacterData data, bool sumWithEquipments, bool sumWithBuffs, Dictionary<BaseSkill, short> skills)
    {
        Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
        if (sumWithEquipments)
            result = GameDataHelpers.CombineDamages(result, data.GetEquipmentIncreaseDamages());
        if (sumWithBuffs)
            result = GameDataHelpers.CombineDamages(result, data.GetBuffIncreaseDamages());
        if (skills != null)
            result = GameDataHelpers.CombineDamages(result, skills.GetPassiveSkillIncreaseDamages());
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
        foreach (CharacterItem equipItem in data.EquipItems)
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
        foreach (CharacterBuff buff in data.Buffs)
        {
            result += buff.GetIncreaseStats();
            result += GameDataHelpers.GetStatsFromAttributes(buff.GetIncreaseAttributes());
            // Increase with rates
            result += baseStats * buff.GetIncreaseStatsRate();
            result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), buff.GetIncreaseAttributesRate()));
        }
        return result;
    }
    public static CharacterStats GetPassiveSkillStats(this ICharacterData data, CharacterStats baseStats, Dictionary<Attribute, float> baseAttributes)
    {
        if (data == null)
            return new CharacterStats();
        return data.GetSkills(true).GetPassiveSkillStats(baseStats, baseAttributes);
    }

    public static CharacterStats GetPassiveSkillStats(this Dictionary<BaseSkill, short> skills, CharacterStats baseStats, Dictionary<Attribute, float> baseAttributes)
    {
        if (skills == null)
            return new CharacterStats();
        CharacterStats result = new CharacterStats();
        foreach (KeyValuePair<BaseSkill, short> skill in skills)
        {
            if (skill.Key == null || !skill.Key.IsPassive() || skill.Value <= 0)
                continue;
            result += skill.Key.GetBuff().GetIncreaseStats(skill.Value);
            result += GameDataHelpers.GetStatsFromAttributes(skill.Key.GetBuff().GetIncreaseAttributes(skill.Value));
            // Increase with rates
            result += baseStats * skill.Key.GetBuff().GetIncreaseStatsRate(skill.Value);
            result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), skill.Key.GetBuff().GetIncreaseAttributesRate(skill.Value)));
        }
        return result;
    }

    public static CharacterStats GetStats(this ICharacterData data, bool sumWithEquipments, bool sumWithBuffs, Dictionary<BaseSkill, short> skills)
    {
        CharacterStats result = new CharacterStats();
        result += data.GetCharacterStats();
        if (sumWithEquipments || sumWithBuffs || skills != null)
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
            if (skills != null)
                result += skills.GetPassiveSkillStats(baseStats, baseAttributes);
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

    public static void FillEmptySlots(this ICharacterData data, bool recacheStats = false)
    {
        if (recacheStats)
            data.MarkToMakeCaches();
        data.NonEquipItems.FillEmptySlots(GameInstance.Singleton.IsLimitInventorySlot, data.GetCaches().LimitItemSlot);
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
        BaseItem itemData;
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

        short maxStack = itemData.MaxStack;
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
        return data.GetCaches().TotalItemSlot + unEquipCount > data.GetCaches().LimitItemSlot;
    }

    public static bool IncreasingItemsWillOverwhelming(this ICharacterData data, int dataId, short amount)
    {
        return data.NonEquipItems.IncreasingItemsWillOverwhelming(
            dataId,
            amount,
            true,
            data.GetCaches().LimitItemWeight,
            data.GetCaches().TotalItemWeight,
            GameInstance.Singleton.IsLimitInventorySlot,
            data.GetCaches().LimitItemSlot);
    }

    public static bool IncreasingItemsWillOverwhelming(this ICharacterData data, IEnumerable<ItemAmount> increasingItems)
    {
        return data.NonEquipItems.IncreasingItemsWillOverwhelming(
            increasingItems,
            true,
            data.GetCaches().LimitItemWeight,
            data.GetCaches().TotalItemWeight,
            GameInstance.Singleton.IsLimitInventorySlot,
            data.GetCaches().LimitItemSlot);
    }

    public static bool IncreasingItemsWillOverwhelming(this ICharacterData data, IEnumerable<CharacterItem> increasingItems)
    {
        return data.NonEquipItems.IncreasingItemsWillOverwhelming(
            increasingItems,
            true,
            data.GetCaches().LimitItemWeight,
            data.GetCaches().TotalItemWeight,
            GameInstance.Singleton.IsLimitInventorySlot,
            data.GetCaches().LimitItemSlot);
    }

    public static bool IncreasingItemsWillOverwhelming(this IList<CharacterItem> itemList, IEnumerable<ItemAmount> increasingItems, bool isLimitWeight, float weightLimit, float totalItemWeight, bool isLimitSlot, short slotLimit)
    {
        if (itemList == null || increasingItems == null)
            return false;
        List<CharacterItem> simulatingItemList = new List<CharacterItem>(itemList);
        foreach (ItemAmount receiveItem in increasingItems)
        {
            if (receiveItem.item == null || receiveItem.amount <= 0) continue;
            if (simulatingItemList.IncreasingItemsWillOverwhelming(
                receiveItem.item.DataId,
                receiveItem.amount,
                isLimitWeight,
                weightLimit,
                totalItemWeight,
                isLimitSlot,
                slotLimit))
            {
                // Overwhelming
                return true;
            }
            else
            {
                // Add item to temp list to check it will overwhelming or not later
                simulatingItemList.AddOrSetItems(CharacterItem.Create(receiveItem.item, 1, receiveItem.amount));
            }
        }
        return false;
    }

    public static bool IncreasingItemsWillOverwhelming(this IList<CharacterItem> itemList, IEnumerable<CharacterItem> increasingItems, bool isLimitWeight, float weightLimit, float totalItemWeight, bool isLimitSlot, short slotLimit)
    {
        if (itemList == null || increasingItems == null)
            return false;
        List<CharacterItem> simulatingItemList = new List<CharacterItem>(itemList);
        foreach (CharacterItem receiveItem in increasingItems)
        {
            if (receiveItem.IsEmptySlot()) continue;
            if (simulatingItemList.IncreasingItemsWillOverwhelming(
                receiveItem.dataId,
                receiveItem.amount,
                isLimitWeight,
                weightLimit,
                totalItemWeight,
                isLimitSlot,
                slotLimit))
            {
                // Overwhelming
                return true;
            }
            else
            {
                // Add item to temp list to check it will overwhelming or not later
                simulatingItemList.AddOrSetItems(CharacterItem.Create(receiveItem.dataId, receiveItem.level, receiveItem.amount));
            }
        }
        return false;
    }
    #endregion

    #region Increase Items
    public static bool IncreaseItems(this IList<CharacterItem> itemList, CharacterItem increasingItem)
    {
        // If item not valid
        if (increasingItem.IsEmptySlot()) return false;

        BaseItem itemData = increasingItem.GetItem();
        short amount = increasingItem.amount;

        short maxStack = itemData.MaxStack;
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
            else if (tempNonEquipItem.dataId == increasingItem.dataId)
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
                tempNewItem = increasingItem.Clone(true);
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
            tempNewItem = increasingItem.Clone(true);
            short addAmount;
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

    public static void IncreaseItems(this IList<CharacterItem> itemList, IEnumerable<ItemAmount> increasingItems, System.Action<int, short, short> onIncrease = null)
    {
        foreach (ItemAmount increasingItem in increasingItems)
        {
            if (increasingItem.item == null || increasingItem.amount <= 0) continue;
            itemList.IncreaseItems(CharacterItem.Create(increasingItem.item.DataId, 1, increasingItem.amount));
            if (onIncrease != null)
                onIncrease.Invoke(increasingItem.item.DataId, 1, increasingItem.amount);
        }
    }

    public static void IncreaseItems(this IList<CharacterItem> itemList, IEnumerable<CharacterItem> increasingItems, System.Action<int, short, short> onIncrease = null)
    {
        foreach (CharacterItem increasingItem in increasingItems)
        {
            if (increasingItem.IsEmptySlot()) continue;
            itemList.IncreaseItems(increasingItem.Clone());
            if (onIncrease != null)
                onIncrease.Invoke(increasingItem.dataId, increasingItem.level, increasingItem.amount);
        }
    }

    public static bool IncreaseItems(this ICharacterData data, CharacterItem increasingItem, System.Action<int, short, short> onIncrease = null)
    {
        if (data.NonEquipItems.IncreaseItems(increasingItem))
        {
            if (onIncrease != null)
                onIncrease.Invoke(increasingItem.dataId, increasingItem.level, increasingItem.amount);
            return true;
        }
        return false;
    }

    public static void IncreaseItems(this ICharacterData data, IEnumerable<ItemAmount> increasingItems, System.Action<int, short, short> onIncrease = null)
    {
        foreach (ItemAmount increasingItem in increasingItems)
        {
            if (increasingItem.item == null || increasingItem.amount <= 0) continue;
            data.NonEquipItems.IncreaseItems(CharacterItem.Create(increasingItem.item.DataId, 1, increasingItem.amount));
            if (onIncrease != null)
                onIncrease.Invoke(increasingItem.item.DataId, 1, increasingItem.amount);
        }
    }

    public static void IncreaseItems(this ICharacterData data, IEnumerable<CharacterItem> increasingItems, System.Action<int, short, short> onIncrease = null)
    {
        foreach (CharacterItem increasingItem in increasingItems)
        {
            if (increasingItem.IsEmptySlot()) continue;
            data.NonEquipItems.IncreaseItems(increasingItem.Clone());
            if (onIncrease != null)
                onIncrease.Invoke(increasingItem.dataId, increasingItem.level, increasingItem.amount);
        }
    }
    #endregion

    #region Decrease Items
    public static bool DecreaseItems(this IList<CharacterItem> itemList, int dataId, short amount, bool isLimitInventorySlot, out Dictionary<int, short> decreaseItems)
    {
        decreaseItems = new Dictionary<int, short>();
        Dictionary<int, short> decreasingItemIndexes = new Dictionary<int, short>();
        short tempDecresingAmount;
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
            decreaseItems.Add(decreasingItem.Key, decreasingItem.Value);
            itemList.DecreaseItemsByIndex(decreasingItem.Key, decreasingItem.Value, isLimitInventorySlot);
        }
        return true;
    }

    public static bool DecreaseItems(this ICharacterData data, int dataId, short amount, bool isLimitInventorySlot, out Dictionary<int, short> decreaseItems)
    {
        return data.NonEquipItems.DecreaseItems(dataId, amount, isLimitInventorySlot, out decreaseItems);
    }

    public static bool DecreaseItems(this ICharacterData data, int dataId, short amount, bool isLimitInventorySlot)
    {
        return DecreaseItems(data, dataId, amount, isLimitInventorySlot, out _);
    }
    #endregion

    #region Decrease Ammos
    public static bool DecreaseAmmos(this ICharacterData data, AmmoType ammoType, short amount, out Dictionary<CharacterItem, short> decreaseItems)
    {
        decreaseItems = new Dictionary<CharacterItem, short>();
        Dictionary<int, short> decreasingItemIndexes = new Dictionary<int, short>();
        CharacterItem nonEquipItem;
        short tempDecresingAmount;
        for (int i = data.NonEquipItems.Count - 1; i >= 0; --i)
        {
            nonEquipItem = data.NonEquipItems[i];
            if (nonEquipItem.GetAmmoItem() != null && nonEquipItem.GetAmmoItem().AmmoType == ammoType)
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
        return DecreaseAmmos(data, ammoType, amount, out _);
    }
    #endregion

    #region Decrease Items By Index
    public static bool DecreaseItemsByIndex(this IList<CharacterItem> itemList, int index, short amount, bool isLimitInventorySlot)
    {
        if (index < 0 || index >= itemList.Count)
            return false;
        CharacterItem nonEquipItem = itemList[index];
        if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
            return false;
        if (nonEquipItem.amount - amount == 0)
        {
            if (isLimitInventorySlot)
                itemList[index] = CharacterItem.Empty;
            else
                itemList.RemoveAt(index);
        }
        else
        {
            nonEquipItem.amount -= amount;
            itemList[index] = nonEquipItem;
        }
        return true;
    }

    public static bool DecreaseItemsByIndex(this ICharacterData data, int index, short amount)
    {
        if (data.NonEquipItems.DecreaseItemsByIndex(index, amount, GameInstance.Singleton.IsLimitInventorySlot))
            return true;
        return false;
    }
    #endregion

    public static List<CharacterItem> Clone(this IList<CharacterItem> itemList)
    {
        List<CharacterItem> result = new List<CharacterItem>();
        foreach (CharacterItem item in itemList)
        {
            result.Add(item.Clone());
        }
        return result;
    }

    public static bool HasOneInNonEquipItems(this ICharacterData data, int dataId)
    {
        if (data != null && data.NonEquipItems.Count > 0)
        {
            IList<CharacterItem> nonEquipItems = data.NonEquipItems;
            foreach (CharacterItem nonEquipItem in nonEquipItems)
            {
                if (nonEquipItem.dataId == dataId && nonEquipItem.amount > 0)
                    return true;
            }
        }
        return false;
    }

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
            IAmmoItem ammoItem;
            IList<CharacterItem> nonEquipItems = data.NonEquipItems;
            foreach (CharacterItem nonEquipItem in nonEquipItems)
            {
                ammoItem = nonEquipItem.GetAmmoItem();
                if (ammoItem != null && ammoType == ammoItem.AmmoType)
                    count += nonEquipItem.amount;
            }
        }
        return count;
    }

    public static CharacterItem GetAvailableWeapon(this ICharacterData data, ref bool isLeftHand)
    {
        IWeaponItem rightWeaponItem = data.EquipWeapons.GetRightHandWeaponItem();
        IWeaponItem leftWeaponItem = data.EquipWeapons.GetLeftHandWeaponItem();
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
        return CharacterItem.Create(GameInstance.Singleton.DefaultWeaponItem.DataId);
    }

    public static DamageInfo GetWeaponDamageInfo(this ICharacterData data, ref bool isLeftHand)
    {
        if (data is BaseMonsterCharacterEntity)
        {
            BaseMonsterCharacterEntity monsterCharacterEntity = data as BaseMonsterCharacterEntity;
            isLeftHand = false;
            return monsterCharacterEntity.CharacterDatabase.DamageInfo;
        }
        else
        {
            return data.GetAvailableWeapon(ref isLeftHand).GetWeaponItem().WeaponType.DamageInfo;
        }
    }

    public static Transform GetWeaponDamageTransform(this BaseCharacterEntity characterEntity, ref bool isLeftHand)
    {
        if (characterEntity is BaseMonsterCharacterEntity)
        {
            BaseMonsterCharacterEntity monsterCharacterEntity = characterEntity as BaseMonsterCharacterEntity;
            isLeftHand = false;
            return monsterCharacterEntity.CharacterDatabase.DamageInfo.GetDamageTransform(characterEntity, isLeftHand);
        }
        else
        {
            return characterEntity.GetAvailableWeapon(ref isLeftHand).GetWeaponItem().WeaponType.DamageInfo.GetDamageTransform(characterEntity, isLeftHand);
        }
    }

    public static KeyValuePair<DamageElement, MinMaxFloat> GetWeaponDamage(this ICharacterData data, ref bool isLeftHand)
    {
        if (data is BaseMonsterCharacterEntity)
        {
            BaseMonsterCharacterEntity monsterCharacterEntity = data as BaseMonsterCharacterEntity;
            isLeftHand = false;
            return GameDataHelpers.MakeDamage(monsterCharacterEntity.CharacterDatabase.DamageAmount, monsterCharacterEntity.Level, 1f, 0f);
        }
        else
        {
            return data.GetAvailableWeapon(ref isLeftHand).GetDamageAmount(data);
        }
    }

    public static int IndexOfAttribute(this ICharacterData data, int dataId)
    {
        for (int i = 0; i < data.Attributes.Count; ++i)
        {
            if (data.Attributes[i].dataId == dataId)
                return i;
        }
        return -1;
    }

    public static int IndexOfSkill(this ICharacterData data, int dataId)
    {
        for (int i = 0; i < data.Skills.Count; ++i)
        {
            if (data.Skills[i].dataId == dataId)
                return i;
        }
        return -1;
    }

    public static int IndexOfSkillUsage(this ICharacterData data, int dataId, SkillUsageType type)
    {
        for (int i = 0; i < data.SkillUsages.Count; ++i)
        {
            if (data.SkillUsages[i].dataId == dataId && data.SkillUsages[i].type == type)
                return i;
        }
        return -1;
    }

    public static int IndexOfBuff(this ICharacterData data, int dataId, BuffType type)
    {
        for (int i = 0; i < data.Buffs.Count; ++i)
        {
            if (data.Buffs[i].dataId == dataId && data.Buffs[i].type == type)
                return i;
        }
        return -1;
    }

    public static int IndexOfEquipItem(this ICharacterData data, int dataId)
    {
        for (int i = 0; i < data.EquipItems.Count; ++i)
        {
            if (data.EquipItems[i].dataId == dataId)
                return i;
        }
        return -1;
    }

    public static int IndexOfEquipItem(this ICharacterData data, string id)
    {
        for (int i = 0; i < data.EquipItems.Count; ++i)
        {
            if (!string.IsNullOrEmpty(data.EquipItems[i].id) && data.EquipItems[i].id.Equals(id))
                return i;
        }
        return -1;
    }

    public static int IndexOfEquipItemByEquipPosition(this ICharacterData data, string equipPosition, byte equipSlotIndex)
    {
        if (string.IsNullOrEmpty(equipPosition))
            return -1;

        for (int i = 0; i < data.EquipItems.Count; ++i)
        {
            if (data.EquipItems[i].GetEquipmentItem() == null)
                continue;

            if (data.EquipItems[i].equipSlotIndex == equipSlotIndex &&
                equipPosition.Equals(data.EquipItems[i].GetArmorItem().EquipPosition))
                return i;
        }
        return -1;
    }

    public static bool FindItemById(
        this ICharacterData data,
        string id)
    {
        return data.FindItemById(id, out _, out _, out _, out _);
    }

    public static bool FindItemById(
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
            return true;
        }

        itemIndex = data.IndexOfEquipItem(id);
        if (itemIndex >= 0)
        {
            characterItem = data.EquipItems[itemIndex];
            inventoryType = InventoryType.EquipItems;
            return true;
        }

        return false;
    }

    public static bool IsEquipped(
        this ICharacterData data,
        string id,
        out InventoryType inventoryType,
        out int itemIndex,
        out byte equipWeaponSet,
        out CharacterItem characterItem)
    {
        if (data.FindItemById(id, out inventoryType, out itemIndex, out equipWeaponSet, out characterItem))
        {
            return inventoryType == InventoryType.EquipItems ||
                inventoryType == InventoryType.EquipWeaponRight ||
                inventoryType == InventoryType.EquipWeaponLeft;
        }
        return false;
    }

    public static void AddOrSetNonEquipItems(this ICharacterData data, CharacterItem characterItem)
    {
        data.AddOrSetNonEquipItems(characterItem, out _);
    }

    public static void AddOrSetNonEquipItems(this ICharacterData data, CharacterItem characterItem, out int index)
    {
        data.NonEquipItems.AddOrSetItems(characterItem, out index);
    }

    public static void AddOrSetItems(this IList<CharacterItem> itemList, CharacterItem characterItem)
    {
        itemList.AddOrSetItems(characterItem, out _);
    }

    public static void AddOrSetItems(this IList<CharacterItem> itemList, CharacterItem characterItem, out int index)
    {
        index = IndexOfEmptyItemSlot(itemList);
        if (index >= 0)
        {
            // Insert to empty slot
            itemList[index] = characterItem;
        }
        else
        {
            // Add to last index
            itemList.Add(characterItem);
            index = itemList.Count - 1;
        }
    }

    public static int IndexOfEmptyNonEquipItemSlot(this ICharacterData data)
    {
        return data.NonEquipItems.IndexOfEmptyItemSlot();
    }

    public static int IndexOfEmptyItemSlot(this IList<CharacterItem> list)
    {
        for (int i = 0; i < list.Count; ++i)
        {
            if (list[i].IsEmptySlot())
                return i;
        }
        return -1;
    }

    public static int IndexOfNonEquipItem(this ICharacterData data, int dataId)
    {
        for (int i = 0; i < data.NonEquipItems.Count; ++i)
        {
            if (data.NonEquipItems[i].dataId == dataId)
                return i;
        }
        return -1;
    }

    public static int IndexOfNonEquipItem(this ICharacterData data, string id)
    {
        for (int i = 0; i < data.NonEquipItems.Count; ++i)
        {
            if (!string.IsNullOrEmpty(data.NonEquipItems[i].id) && data.NonEquipItems[i].id.Equals(id))
                return i;
        }
        return -1;
    }

    public static int IndexOfSummon(this ICharacterData data, uint objectId)
    {
        for (int i = 0; i < data.Summons.Count; ++i)
        {
            if (data.Summons[i].objectId == objectId)
                return i;
        }
        return -1;
    }

    public static int IndexOfSummon(this ICharacterData data, SummonType type)
    {
        for (int i = 0; i < data.Summons.Count; ++i)
        {
            if (data.Summons[i].type == type)
                return i;
        }
        return -1;
    }

    public static int IndexOfSummon(this ICharacterData data, int dataId, SummonType type)
    {
        for (int i = 0; i < data.Summons.Count; ++i)
        {
            if (data.Summons[i].dataId == dataId && data.Summons[i].type == type)
                return i;
        }
        return -1;
    }

    public static int IndexOfAmmoItem(this ICharacterData data, AmmoType ammoType)
    {
        for (int i = 0; i < data.NonEquipItems.Count; ++i)
        {
            if (data.NonEquipItems[i].GetAmmoItem() != null && data.NonEquipItems[i].GetAmmoItem().AmmoType == ammoType)
                return i;
        }
        return -1;
    }

    public static void GetEquipmentSetBonus(this ICharacterData data,
        ref CharacterStats bonusStats,
        Dictionary<Attribute, float> bonusAttributes,
        Dictionary<DamageElement, float> bonusResistances,
        Dictionary<DamageElement, float> bonusArmors,
        Dictionary<DamageElement, MinMaxFloat> bonusDamages,
        Dictionary<BaseSkill, short> bonusSkills,
        Dictionary<EquipmentSet, int> equipmentSets,
        bool combine)
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

        IEquipmentItem tempEquipmentItem;
        // Armor equipment set
        foreach (CharacterItem equipItem in data.EquipItems)
        {
            tempEquipmentItem = equipItem.GetEquipmentItem();
            if (tempEquipmentItem != null && tempEquipmentItem.EquipmentSet != null)
            {
                if (equipmentSets.ContainsKey(tempEquipmentItem.EquipmentSet))
                    ++equipmentSets[tempEquipmentItem.EquipmentSet];
                else
                    equipmentSets.Add(tempEquipmentItem.EquipmentSet, 0);
            }
        }
        // Weapon equipment set
        tempEquipmentItem = data.EquipWeapons.GetRightHandEquipmentItem();
        // Right hand equipment set
        if (tempEquipmentItem != null && tempEquipmentItem.EquipmentSet != null)
        {
            if (equipmentSets.ContainsKey(tempEquipmentItem.EquipmentSet))
                ++equipmentSets[tempEquipmentItem.EquipmentSet];
            else
                equipmentSets.Add(tempEquipmentItem.EquipmentSet, 0);
        }
        tempEquipmentItem = data.EquipWeapons.GetLeftHandEquipmentItem();
        // Left hand equipment set
        if (tempEquipmentItem != null && tempEquipmentItem.EquipmentSet != null)
        {
            if (equipmentSets.ContainsKey(tempEquipmentItem.EquipmentSet))
                ++equipmentSets[tempEquipmentItem.EquipmentSet];
            else
                equipmentSets.Add(tempEquipmentItem.EquipmentSet, 0);
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
        out float resultAtkSpeed,
        out float resultMoveSpeed,
        bool combine)
    {
        if (!combine)
        {
            resultStats = new CharacterStats();
            resultSkills.Clear();
            resultAttributes.Clear();
            resultResistances.Clear();
            resultArmors.Clear();
            resultIncreaseDamages.Clear();
            resultEquipmentSets.Clear();
        }
        // Set results values
        resultSkills = GameDataHelpers.CombineSkills(resultSkills, data.GetSkills(true));
        resultAttributes = GameDataHelpers.CombineAttributes(resultAttributes, data.GetAttributes(true, true, resultSkills));
        resultResistances = GameDataHelpers.CombineResistances(resultResistances, data.GetResistances(true, true, resultSkills));
        resultArmors = GameDataHelpers.CombineArmors(resultArmors, data.GetArmors(true, true, resultSkills));
        resultIncreaseDamages = GameDataHelpers.CombineDamages(resultIncreaseDamages, data.GetIncreaseDamages(true, true, resultSkills));
        resultStats = resultStats + data.GetStats(true, true, resultSkills);
        // Prepare equipment set bonus
        data.GetEquipmentSetBonus(ref resultStats, resultAttributes, resultResistances, resultArmors, resultIncreaseDamages, resultSkills, resultEquipmentSets, true);
        // Separated stats results
        resultMaxHp = (int)resultStats.hp;
        resultMaxMp = (int)resultStats.mp;
        resultMaxStamina = (int)resultStats.stamina;
        resultMaxFood = (int)resultStats.food;
        resultMaxWater = (int)resultStats.water;
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
