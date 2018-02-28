using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class CharacterData : ICharacterData
{
    public string id;
    public string characterName;
    public string prototypeId;
    public int level;
    public int exp;
    public int currentHp;
    public int currentMp;
    public int statPoint;
    public int skillPoint;
    public int gold;
    public string currentMapName;
    public Vector3 currentPosition;
    public string respawnMapName;
    public Vector3 respawnPosition;
    public int lastUpdate;
    public List<CharacterAttributeLevel> attributeLevels = new List<CharacterAttributeLevel>();
    public List<CharacterSkillLevel> skillLevels = new List<CharacterSkillLevel>();
    public List<CharacterItem> equipItems = new List<CharacterItem>();
    public List<CharacterItem> nonEquipItems = new List<CharacterItem>();

    public string Id { get { return id; } set { id = value; } }
    public string CharacterName { get { return characterName; } set { characterName = value; } }
    public string PrototypeId { get { return prototypeId; } set { prototypeId = value; } }
    public int Level { get { return level; } set { level = value; } }
    public int Exp { get { return exp; } set { exp = value; } }
    public int CurrentHp { get { return currentHp; } set { currentHp = value; } }
    public int CurrentMp { get { return currentMp; } set { currentMp = value; } }
    public int StatPoint { get { return statPoint; } set { statPoint = value; } }
    public int SkillPoint { get { return skillPoint; } set { skillPoint = value; } }
    public int Gold { get { return gold; } set { gold = value; } }
    public string CurrentMapName { get { return currentMapName; } set { currentMapName = value; } }
    public Vector3 CurrentPosition { get { return currentPosition; } set { currentPosition = value; } }
    public string RespawnMapName { get { return respawnMapName; } set { respawnMapName = value; } }
    public Vector3 RespawnPosition { get { return respawnPosition; } set { respawnPosition = value; } }
    public int LastUpdate { get { return lastUpdate; } set { lastUpdate = value; } }
    public IList<CharacterAttributeLevel> AttributeLevels
    {
        get { return attributeLevels; }
        set
        {
            attributeLevels = new List<CharacterAttributeLevel>();
            attributeLevels.AddRange(value);
        }
    }
    public IList<CharacterSkillLevel> SkillLevels
    {
        get { return skillLevels; }
        set
        {
            skillLevels = new List<CharacterSkillLevel>();
            skillLevels.AddRange(value);
        }
    }
    public IList<CharacterItem> EquipItems
    {
        get { return equipItems; }
        set
        {
            equipItems = new List<CharacterItem>();
            equipItems.AddRange(value);
        }
    }
    public IList<CharacterItem> NonEquipItems
    {
        get { return nonEquipItems; }
        set
        {
            nonEquipItems = new List<CharacterItem>();
            nonEquipItems.AddRange(value);
        }
    }
}

public class CharacterDataLastUpdateComparer : IComparer<CharacterData>
{
    private int sortMultiplier = 1;
    public CharacterDataLastUpdateComparer Asc()
    {
        sortMultiplier = 1;
        return this;
    }

    public CharacterDataLastUpdateComparer Desc()
    {
        sortMultiplier = -1;
        return this;
    }

    public int Compare(CharacterData x, CharacterData y)
    {
        return x.LastUpdate.CompareTo(y.LastUpdate) * sortMultiplier;
    }
}

public static class CharacterDataExtension
{
    public static T CloneTo<T>(this ICharacterData from, T to) where T : ICharacterData
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
        to.CurrentMapName = from.CurrentMapName;
        to.CurrentPosition = from.CurrentPosition;
        to.RespawnMapName = from.RespawnMapName;
        to.RespawnPosition = from.RespawnPosition;
        to.LastUpdate = from.LastUpdate;
        to.AttributeLevels = from.AttributeLevels;
        to.SkillLevels = from.SkillLevels;
        to.EquipItems = from.EquipItems;
        to.NonEquipItems = from.NonEquipItems;
        return to;
    }

    public static T SetNewCharacterData<T>(this T character, string characterName, string prototypeId) where T : ICharacterData
    {
        var gameInstance = GameInstance.Singleton;
        character.CharacterName = characterName;
        character.PrototypeId = prototypeId;
        character.Level = 1;
        foreach (var baseAttribute in character.GetClass().baseAttributes)
        {
            var attributeLevel = new CharacterAttributeLevel();
            attributeLevel.attributeId = baseAttribute.attribute.Id;
            attributeLevel.amount = baseAttribute.amount;
            character.AttributeLevels.Add(attributeLevel);
        }
        character.CurrentHp = character.GetMaxHp();
        character.CurrentMp = character.GetMaxMp();
        character.Gold = gameInstance.startGold;

        if (gameInstance.startMap != null)
        {
            character.CurrentMapName = gameInstance.startMap.sceneName;
            character.RespawnMapName = gameInstance.startMap.sceneName;
            character.CurrentPosition = gameInstance.startPosition;
            character.RespawnPosition = gameInstance.startPosition;
        }
        return character;
    }

    public static CharacterPrototype GetPrototype(this ICharacterData data)
    {
        return GameInstance.CharacterPrototypes[data.PrototypeId];
    }

    public static CharacterClass GetClass(this ICharacterData data)
    {
        return data.GetPrototype().characterClass;
    }

    public static CharacterModel InstantiateModel(this ICharacterData data, Transform parent)
    {
        var prototype = data.GetPrototype();
        var result = Object.Instantiate(prototype.characterModel, parent);
        result.gameObject.SetActive(true);
        result.transform.localPosition = Vector3.zero;
        return result;
    }

    public static int GetNextLevelExp(this ICharacterData data)
    {
        var level = data.Level;
        var expTree = GameInstance.Singleton.expTree;
        if (level > expTree.Count)
            return 0;
        return expTree[level - 1];
    }

    #region Stats calculation, make saperate stats for buffs calculation
    public static CharacterStats GetStats(this ICharacterData data)
    {
        var id = data.Id;
        var level = data.Level;
        var characterClass = data.GetClass();
        var attributeLevels = data.AttributeLevels;
        var equipItems = data.EquipItems;
        var result = characterClass.baseStats;
        result += characterClass.statsIncreaseEachLevel * level;
        foreach (var attributeLevel in attributeLevels)
        {
            if (attributeLevel.Attribute == null)
            {
                Debug.LogError("Attribute: " + attributeLevel.attributeId + " owned by " + id + " is invalid data");
                continue;
            }
            result += attributeLevel.Attribute.statsIncreaseEachLevel * attributeLevel.amount;
        }
        foreach (var equipment in equipItems)
        {
            if (equipment.EquipmentItem == null)
            {
                Debug.LogError("Item: " + equipment.id + " owned by " + id + " is not equipment");
                continue;
            }
            result += equipment.EquipmentItem.baseStats;
            result += equipment.EquipmentItem.statsIncreaseEachLevel * equipment.level;
        }
        return result;
    }

    public static CharacterStatsPercentage GetStatsPercentage(this ICharacterData data)
    {
        var id = data.Id;
        var level = data.Level;
        var characterClass = data.GetClass();
        var attributeLevels = data.AttributeLevels;
        var equipItems = data.EquipItems;
        var result = characterClass.statsPercentageIncreaseEachLevel * level;
        foreach (var attributeLevel in attributeLevels)
        {
            if (attributeLevel.Attribute == null)
            {
                Debug.LogError("Attribute: " + attributeLevel.attributeId + " owned by " + id + " is invalid data");
                continue;
            }
            result += attributeLevel.Attribute.statsPercentageIncreaseEachLevel * attributeLevel.amount;
        }
        foreach (var equipment in equipItems)
        {
            if (equipment.EquipmentItem == null)
            {
                Debug.LogError("Item: " + equipment.id + " owned by " + id + " is not equipment");
                continue;
            }
            result += equipment.EquipmentItem.statsPercentageIncreaseEachLevel * equipment.level;
        }
        return result;
    }

    public static CharacterStats GetStatsWithoutBuffs(this ICharacterData data)
    {
        return data.GetStats() + data.GetStatsPercentage();
    }
    #endregion

    public static int GetMaxHp(this ICharacterData data)
    {
        return (int)data.GetStatsWithoutBuffs().hp;
    }

    public static int GetMaxMp(this ICharacterData data)
    {
        return (int)data.GetStatsWithoutBuffs().mp;
    }

    public static void SavePersistentCharacterData<T>(this T characterData) where T : ICharacterData
    {
        var savingData = new CharacterData();
        characterData.CloneTo(savingData);
        savingData.LastUpdate = (int)(System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond);
        var binaryFormatter = new BinaryFormatter();
        var surrogateSelector = new SurrogateSelector();
        surrogateSelector.AddAllUnitySurrogate();
        binaryFormatter.SurrogateSelector = surrogateSelector;
        var path = Application.persistentDataPath + "/" + savingData.Id + ".sav";
        var file = File.Open(path, FileMode.OpenOrCreate);
        binaryFormatter.Serialize(file, savingData);
        file.Close();
        Debug.Log("Character Saved: " + path);
    }


    public static T LoadPersistentCharacterDataById<T>(this T characterData, string id) where T : ICharacterData
    {
        return LoadPersistentCharacterData(characterData, Application.persistentDataPath + "/" + id + ".sav");
    }

    public static T LoadPersistentCharacterData<T>(this T characterData, string path) where T : ICharacterData
    {
        if (File.Exists(path))
        {
            var binaryFormatter = new BinaryFormatter();
            var surrogateSelector = new SurrogateSelector();
            surrogateSelector.AddAllUnitySurrogate();
            binaryFormatter.SurrogateSelector = surrogateSelector;
            var file = File.Open(path, FileMode.Open);
            CharacterData loadedData = (CharacterData)binaryFormatter.Deserialize(file);
            file.Close();
            loadedData.CloneTo(characterData);
        }
        return characterData;
    }

    public static List<CharacterData> LoadAllPersistentCharacterData()
    {
        var result = new List<CharacterData>();
        var files = Directory.GetFiles(Application.persistentDataPath, "*.sav");
        foreach (var file in files)
        {
            var characterData = new CharacterData();
            result.Add(characterData.LoadPersistentCharacterData(file));
        }
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

    public static void DeletePersistentCharacterData<T>(this T characterData) where T:ICharacterData
    {
        if (characterData == null)
        {
            Debug.LogWarning("Cannot delete character: character data is empty");
            return;
        }
        DeletePersistentCharacterData(characterData.Id);
    }
}
