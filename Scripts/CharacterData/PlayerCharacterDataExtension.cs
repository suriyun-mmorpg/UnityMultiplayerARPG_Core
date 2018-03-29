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
        return to;
    }

    public static T SetNewCharacterData<T>(this T character, string characterName, string databaseId) where T : IPlayerCharacterData
    {
        var gameInstance = GameInstance.Singleton;
        PlayerCharacterDatabase database;
        if (!GameInstance.PlayerCharacterDatabases.TryGetValue(databaseId, out database))
            return character;
        // Player character database
        var playerCharacterDatabase = database as PlayerCharacterDatabase;
        // Attributes
        var baseAttributes = playerCharacterDatabase.baseAttributes;
        foreach (var baseAttribute in baseAttributes)
        {
            var characterAttribute = new CharacterAttribute();
            characterAttribute.attributeId = baseAttribute.attribute.Id;
            characterAttribute.amount = baseAttribute.amount;
            character.Attributes.Add(characterAttribute);
        }
        // Right hand & left hand items
        var rightHandEquipItem = playerCharacterDatabase.rightHandEquipItem;
        var leftHandEquipItem = playerCharacterDatabase.leftHandEquipItem;
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
        var armorItems = playerCharacterDatabase.armorItems;
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
}
