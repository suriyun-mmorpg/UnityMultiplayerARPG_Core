using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class PlayerCharacterDataExtension
{
    public static T CloneTo<T>(this IPlayerCharacterData from, T to) where T : IPlayerCharacterData
    {
        to.Id = from.Id;
        to.DatabaseId = from.DatabaseId;
        to.CharacterName = from.CharacterName;
        to.Level = from.Level;
        to.Exp = from.Exp;
        to.CurrentHp = from.CurrentHp;
        to.CurrentMp = from.CurrentMp;
        to.StatPoint = from.StatPoint;
        to.SkillPoint = from.SkillPoint;
        to.Gold = from.Gold;
        to.EquipWeapons = from.EquipWeapons;
        to.CurrentMapName = from.CurrentMapName;
        to.CurrentPosition = from.CurrentPosition;
        to.RespawnMapName = from.RespawnMapName;
        to.RespawnPosition = from.RespawnPosition;
        to.LastUpdate = from.LastUpdate;
        to.Attributes = from.Attributes;
        to.Skills = from.Skills;
        to.Buffs = from.Buffs;
        to.EquipItems = from.EquipItems;
        to.NonEquipItems = from.NonEquipItems;
        to.Hotkeys = from.Hotkeys;
        return to;
    }

    public static T ValidateCharacterData<T>(this T character) where T : IPlayerCharacterData
    {
        var gameInstance = GameInstance.Singleton;
        PlayerCharacter database;
        if (!GameInstance.PlayerCharacters.TryGetValue(character.DatabaseId, out database))
            return character;
        // Validating character attributes
        var returningStatPoint = 0;
        var validAttributeIds = new HashSet<string>();
        var characterAttributes = character.Attributes;
        for (var i = characterAttributes.Count - 1; i >= 0; --i)
        {
            var characterAttribute = characterAttributes[i];
            var attributeId = characterAttribute.attributeId;
            // If attribute is invalid
            if (string.IsNullOrEmpty(attributeId) || 
                characterAttribute.GetAttribute() == null ||
                validAttributeIds.Contains(attributeId))
            {
                returningStatPoint += characterAttribute.amount;
                character.Attributes.RemoveAt(i);
            }
            else
                validAttributeIds.Add(attributeId);
        }
        character.StatPoint += returningStatPoint;
        // Add character's attributes
        var attributes = GameInstance.Attributes.Values;
        foreach (var attribute in attributes)
        {
            if (validAttributeIds.Contains(attribute.Id))
                continue;
            var characterAttribute = new CharacterAttribute();
            characterAttribute.attributeId = attribute.Id;
            characterAttribute.amount = 0;
            character.Attributes.Add(characterAttribute);
        }
        // Validating character skills
        var returningSkillPoint = 0;
        var validSkillIds = new HashSet<string>();
        var characterSkills = character.Skills;
        for (var i = characterSkills.Count - 1; i >= 0; --i)
        {
            var characterSkill = characterSkills[i];
            var skillId = characterSkill.skillId;
            // If skill is invalid or this character database does not have skill
            if (string.IsNullOrEmpty(skillId) || 
                characterSkill.GetSkill() == null || 
                !database.CacheSkills.ContainsKey(skillId) || 
                validSkillIds.Contains(skillId))
            {
                returningSkillPoint += characterSkill.level;
                character.Skills.RemoveAt(i);
            }
            else
                validSkillIds.Add(skillId);
        }
        character.SkillPoint += returningSkillPoint;
        // Add character's skills
        var skills = database.skills;
        foreach (var skill in skills)
        {
            if (validSkillIds.Contains(skill.Id))
                continue;
            var characterSkill = new CharacterSkill();
            characterSkill.skillId = skill.Id;
            characterSkill.level = 0;
            character.Skills.Add(characterSkill);
        }
        // Validating character equip weapons
        var returningItems = new List<CharacterItem>();
        var equipWeapons = character.EquipWeapons;
        var rightHand = equipWeapons.rightHand;
        var leftHand = equipWeapons.leftHand;
        if (rightHand.GetEquipmentItem() == null)
        {
            if (rightHand.IsValid())
                returningItems.Add(rightHand);
            equipWeapons.rightHand = CharacterItem.Empty;
        }
        if (leftHand.GetEquipmentItem() == null)
        {
            if (leftHand.IsValid())
                returningItems.Add(leftHand);
            equipWeapons.leftHand = CharacterItem.Empty;
        }
        // Validating character equip items
        var equipItems = character.EquipItems;
        for (var i = equipItems.Count - 1; i >= 0; --i)
        {
            var equipItem = equipItems[i];
            // If equipment is invalid
            if (equipItem.GetEquipmentItem() == null)
            {
                if (equipItem.IsValid())
                    returningItems.Add(equipItem);
                character.EquipItems.RemoveAt(i);
            }
        }
        // Return items to non equip items
        foreach (var returningItem in returningItems)
        {
            character.NonEquipItems.Add(returningItem);
        }
        // Validating character non equip items
        var nonEquipItems = character.NonEquipItems;
        for (var i = nonEquipItems.Count - 1; i >= 0; --i)
        {
            var nonEquipItem = nonEquipItems[i];
            // If equipment is invalid
            if (!nonEquipItem.IsValid())
                character.NonEquipItems.RemoveAt(i);
        }
        return character;
    }

    public static T SetNewCharacterData<T>(this T character, string characterName, string databaseId) where T : IPlayerCharacterData
    {
        var gameInstance = GameInstance.Singleton;
        PlayerCharacter database;
        if (!GameInstance.PlayerCharacters.TryGetValue(databaseId, out database))
            return character;
        // Player character database
        var playerCharacter = database as PlayerCharacter;
        // Attributes
        var attributes = GameInstance.Attributes.Values;
        foreach (var attribute in attributes)
        {
            var characterAttribute = new CharacterAttribute();
            characterAttribute.attributeId = attribute.Id;
            characterAttribute.amount = 0;
            character.Attributes.Add(characterAttribute);
        }
        var skills = playerCharacter.skills;
        foreach (var skill in skills)
        {
            var characterSkill = new CharacterSkill();
            characterSkill.skillId = skill.Id;
            characterSkill.level = 0;
            character.Skills.Add(characterSkill);
        }
        // Right hand & left hand items
        var rightHandEquipItem = playerCharacter.rightHandEquipItem;
        var leftHandEquipItem = playerCharacter.leftHandEquipItem;
        var equipWeapons = new EquipWeapons();
        // Right hand equipped item
        if (rightHandEquipItem != null)
        {
            var characterItem = new CharacterItem();
            characterItem.id = System.Guid.NewGuid().ToString();
            characterItem.itemId = rightHandEquipItem.Id;
            characterItem.level = 1;
            characterItem.amount = rightHandEquipItem.maxStack;
            equipWeapons.rightHand = characterItem;
        }
        // Left hand equipped item
        if (leftHandEquipItem != null)
        {
            var characterItem = new CharacterItem();
            characterItem.id = System.Guid.NewGuid().ToString();
            characterItem.itemId = leftHandEquipItem.Id;
            characterItem.level = 1;
            characterItem.amount = leftHandEquipItem.maxStack;
            equipWeapons.leftHand = characterItem;
        }
        character.EquipWeapons = equipWeapons;
        // Armors
        var armorItems = playerCharacter.armorItems;
        foreach (var armorItem in armorItems)
        {
            if (armorItem == null)
                continue;
            var characterItem = new CharacterItem();
            characterItem.id = System.Guid.NewGuid().ToString();
            characterItem.itemId = armorItem.Id;
            characterItem.level = 1;
            characterItem.amount = armorItem.maxStack;
            character.EquipItems.Add(characterItem);
        }
        // General data
        character.DatabaseId = database.Id;
        character.CharacterName = characterName;
        character.Level = 1;
        character.CurrentHp = character.GetMaxHp();
        character.CurrentMp = character.GetMaxMp();
        character.Gold = gameInstance.startGold;
        // Inventory
        var startItems = gameInstance.startItems;
        foreach (var startItem in startItems)
        {
            if (startItem.item == null || startItem.amount <= 0)
                continue;
            var characterItem = new CharacterItem();
            var amount = startItem.amount;
            if (amount > startItem.item.maxStack)
                amount = startItem.item.maxStack;
            characterItem.id = System.Guid.NewGuid().ToString();
            characterItem.itemId = startItem.item.Id;
            characterItem.level = 1;
            characterItem.amount = amount;
            character.NonEquipItems.Add(characterItem);
        }
        // Position
        character.CurrentMapName = gameInstance.startScene;
        character.RespawnMapName = gameInstance.startScene;
        character.CurrentPosition = gameInstance.startPosition;
        character.RespawnPosition = gameInstance.startPosition;
        return character;
    }

    public static void SavePersistentCharacterData<T>(this T characterData) where T : IPlayerCharacterData
    {
        var savingData = new PlayerCharacterData();
        characterData.CloneTo(savingData);
        savingData.LastUpdate = (int)(System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond);
        var binaryFormatter = new BinaryFormatter();
        var surrogateSelector = new SurrogateSelector();
        surrogateSelector.AddAllUnitySurrogate();
        binaryFormatter.SurrogateSelector = surrogateSelector;
        var path = Application.persistentDataPath + "/" + savingData.Id + ".sav";
        Debug.Log("Character Saving to: " + path);
        var file = File.Open(path, FileMode.OpenOrCreate);
        binaryFormatter.Serialize(file, savingData);
        file.Close();
        Debug.Log("Character Saved to: " + path);
    }


    public static T LoadPersistentCharacterDataById<T>(this T characterData, string id) where T : IPlayerCharacterData
    {
        return LoadPersistentCharacterData(characterData, Application.persistentDataPath + "/" + id + ".sav");
    }

    public static T LoadPersistentCharacterData<T>(this T characterData, string path) where T : IPlayerCharacterData
    {
        if (File.Exists(path))
        {
            var binaryFormatter = new BinaryFormatter();
            var surrogateSelector = new SurrogateSelector();
            surrogateSelector.AddAllUnitySurrogate();
            binaryFormatter.SurrogateSelector = surrogateSelector;
            var file = File.Open(path, FileMode.Open);
            PlayerCharacterData loadedData = (PlayerCharacterData)binaryFormatter.Deserialize(file);
            file.Close();
            loadedData.CloneTo(characterData);
        }
        return characterData;
    }

    public static List<PlayerCharacterData> LoadAllPersistentCharacterData()
    {
        var result = new List<PlayerCharacterData>();
        var path = Application.persistentDataPath;
        var files = Directory.GetFiles(path, "*.sav");
        Debug.Log("Characters loading from: " + path);
        foreach (var file in files)
        {
            var characterData = new PlayerCharacterData();
            result.Add(characterData.LoadPersistentCharacterData(file));
        }
        Debug.Log("Characters loaded from: " + path);
        return result;
    }

    public static void DeletePersistentCharacterData(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("Cannot delete character: character id is empty");
            return;
        }
        File.Delete(Application.persistentDataPath + "/" + id + ".sav");
    }

    public static void DeletePersistentCharacterData<T>(this T characterData) where T : IPlayerCharacterData
    {
        if (characterData == null)
        {
            Debug.LogWarning("Cannot delete character: character data is empty");
            return;
        }
        DeletePersistentCharacterData(characterData.Id);
    }

    public static int IndexOfHotkey(this IPlayerCharacterData data, string hotkeyId)
    {
        var list = data.Hotkeys;
        CharacterHotkey tempHotkey;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempHotkey = list[i];
            if (!string.IsNullOrEmpty(tempHotkey.hotkeyId) &&
                tempHotkey.hotkeyId.Equals(hotkeyId))
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static int IndexOfQuest(this IPlayerCharacterData data, string questId)
    {
        var list = data.Quests;
        CharacterQuest tempQuest;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempQuest = list[i];
            if (!string.IsNullOrEmpty(tempQuest.questId) &&
                tempQuest.questId.Equals(questId))
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
