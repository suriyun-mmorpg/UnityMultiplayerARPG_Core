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
        to.CharacterName = from.CharacterName;
        to.PrototypeId = from.PrototypeId;
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

    public static T SetNewCharacterData<T>(this T character, string characterName, string prototypeId) where T : IPlayerCharacterData
    {
        var gameInstance = GameInstance.Singleton;
        character.CharacterName = characterName;
        character.PrototypeId = prototypeId;
        character.Level = 1;
        foreach (var baseAttribute in character.GetClass().baseAttributes)
        {
            var characterAttribute = new CharacterAttribute();
            characterAttribute.attributeId = baseAttribute.attribute.Id;
            characterAttribute.amount = baseAttribute.amount;
            character.Attributes.Add(characterAttribute);
        }
        character.CurrentHp = character.GetMaxHp();
        character.CurrentMp = character.GetMaxMp();
        character.Gold = gameInstance.startGold;

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

        character.CurrentMapName = gameInstance.startSceneName;
        character.RespawnMapName = gameInstance.startSceneName;
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
