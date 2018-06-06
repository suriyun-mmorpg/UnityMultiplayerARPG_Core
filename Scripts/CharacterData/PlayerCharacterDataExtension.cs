using LiteNetLib.Utils;
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
        to.CurrentStamina = from.CurrentStamina;
        to.CurrentFood = from.CurrentFood;
        to.CurrentWater = from.CurrentWater;
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
        to.Quests = from.Quests;
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
                !database.CacheSkillLevels.ContainsKey(skillId) ||
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
        var skillLevels = database.skillLevels;
        foreach (var skillLevel in skillLevels)
        {
            if (skillLevel.skill != null && validSkillIds.Contains(skillLevel.skill.Id))
                continue;
            var characterSkill = new CharacterSkill();
            characterSkill.skillId = skillLevel.skill.Id;
            characterSkill.level = skillLevel.level;
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
        var skillLevels = playerCharacter.skillLevels;
        foreach (var skillLevel in skillLevels)
        {
            if (skillLevel.skill == null)
                continue;
            var characterSkill = new CharacterSkill();
            characterSkill.skillId = skillLevel.skill.Id;
            characterSkill.level = skillLevel.level;
            character.Skills.Add(characterSkill);
        }
        // Right hand & left hand items
        var rightHandEquipItem = playerCharacter.rightHandEquipItem;
        var leftHandEquipItem = playerCharacter.leftHandEquipItem;
        var equipWeapons = new EquipWeapons();
        // Right hand equipped item
        if (rightHandEquipItem != null)
        {
            var newItem = CharacterItem.Create(rightHandEquipItem);
            equipWeapons.rightHand = newItem;
        }
        // Left hand equipped item
        if (leftHandEquipItem != null)
        {
            var newItem = CharacterItem.Create(leftHandEquipItem);
            equipWeapons.leftHand = newItem;
        }
        character.EquipWeapons = equipWeapons;
        // Armors
        var armorItems = playerCharacter.armorItems;
        foreach (var armorItem in armorItems)
        {
            if (armorItem == null)
                continue;
            var newItem = CharacterItem.Create(armorItem);
            character.EquipItems.Add(newItem);
        }
        // General data
        character.DatabaseId = database.Id;
        character.CharacterName = characterName;
        character.Level = 1;
        var stats = character.GetStats();
        character.CurrentHp = (int)stats.hp;
        character.CurrentMp = (int)stats.mp;
        character.CurrentStamina = (int)stats.stamina;
        character.CurrentFood = (int)stats.food;
        character.CurrentWater = (int)stats.water;
        character.Gold = gameInstance.startGold;
        // Inventory
        var startItems = gameInstance.startItems;
        foreach (var startItem in startItems)
        {
            if (startItem.item == null || startItem.amount <= 0)
                continue;
            var amount = startItem.amount;
            if (amount > startItem.item.maxStack)
                amount = startItem.item.maxStack;
            var newItem = CharacterItem.Create(startItem.item, 1, amount);
            character.NonEquipItems.Add(newItem);
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

    public static void SerializeCharacterData<T>(this T characterData, NetDataWriter writer) where T : IPlayerCharacterData
    {
        writer.Put(characterData.Id);
        writer.Put(characterData.DatabaseId);
        writer.Put(characterData.CharacterName);
        writer.Put(characterData.Level);
        writer.Put(characterData.Exp);
        writer.Put(characterData.CurrentHp);
        writer.Put(characterData.CurrentMp);
        writer.Put(characterData.CurrentStamina);
        writer.Put(characterData.CurrentFood);
        writer.Put(characterData.CurrentWater);
        writer.Put(characterData.StatPoint);
        writer.Put(characterData.SkillPoint);
        writer.Put(characterData.Gold);
        writer.Put(characterData.CurrentMapName);
        writer.Put(characterData.CurrentPosition.x);
        writer.Put(characterData.CurrentPosition.y);
        writer.Put(characterData.CurrentPosition.z);
        writer.Put(characterData.RespawnMapName);
        writer.Put(characterData.RespawnPosition.x);
        writer.Put(characterData.RespawnPosition.y);
        writer.Put(characterData.RespawnPosition.z);
        writer.Put(characterData.LastUpdate);
        writer.Put(characterData.Attributes.Count);
        foreach (var entry in characterData.Attributes)
        {
            writer.Put(entry.attributeId);
            writer.Put(entry.amount);
        }
        writer.Put(characterData.Buffs.Count);
        foreach (var entry in characterData.Buffs)
        {
            writer.Put(entry.id);
            writer.Put(entry.characterId);
            writer.Put(entry.dataId);
            writer.Put((byte)entry.type);
            writer.Put(entry.level);
            writer.Put(entry.buffRemainsDuration);
        }
        writer.Put(characterData.Skills.Count);
        foreach (var entry in characterData.Skills)
        {
            writer.Put(entry.skillId);
            writer.Put(entry.level);
            writer.Put(entry.coolDownRemainsDuration);
        }
        writer.Put(characterData.EquipItems.Count);
        foreach (var entry in characterData.EquipItems)
        {
            writer.Put(entry.id);
            writer.Put(entry.itemId);
            writer.Put(entry.level);
            writer.Put(entry.amount);
        }
        writer.Put(characterData.NonEquipItems.Count);
        foreach (var entry in characterData.NonEquipItems)
        {
            writer.Put(entry.id);
            writer.Put(entry.itemId);
            writer.Put(entry.level);
            writer.Put(entry.amount);
        }
        writer.Put(characterData.Hotkeys.Count);
        foreach (var entry in characterData.Hotkeys)
        {
            writer.Put(entry.hotkeyId);
            writer.Put((byte)entry.type);
            writer.Put(entry.dataId);
        }
        writer.Put(characterData.Quests.Count);
        foreach (var entry in characterData.Quests)
        {
            writer.Put(entry.questId);
            writer.Put(entry.isComplete);
            var killedMonsters = entry.killedMonsters;
            var killMonsterCount = killedMonsters == null ? 0 : killedMonsters.Count;
            writer.Put(killMonsterCount);
            if (killMonsterCount > 0)
            {
                foreach (var killedMonster in killedMonsters)
                {
                    writer.Put(killedMonster.Key);
                    writer.Put(killedMonster.Value);
                }
            }
        }
        var rightHand = characterData.EquipWeapons.rightHand;
        writer.Put(rightHand.id);
        writer.Put(rightHand.itemId);
        writer.Put(rightHand.level);
        writer.Put(rightHand.amount);
        var leftHand = characterData.EquipWeapons.leftHand;
        writer.Put(leftHand.id);
        writer.Put(leftHand.itemId);
        writer.Put(leftHand.level);
        writer.Put(leftHand.amount);
    }

    public static T DeserializeCharacterData<T>(this T characterData, NetDataReader reader) where T : IPlayerCharacterData
    {
        var tempCharacterData = new PlayerCharacterData();
        tempCharacterData.Id = reader.GetString();
        tempCharacterData.DatabaseId = reader.GetString();
        tempCharacterData.CharacterName = reader.GetString();
        tempCharacterData.Level = reader.GetInt();
        tempCharacterData.Exp = reader.GetInt();
        tempCharacterData.CurrentHp = reader.GetInt();
        tempCharacterData.CurrentMp = reader.GetInt();
        tempCharacterData.CurrentStamina = reader.GetInt();
        tempCharacterData.CurrentFood = reader.GetInt();
        tempCharacterData.CurrentWater = reader.GetInt();
        tempCharacterData.StatPoint = reader.GetInt();
        tempCharacterData.SkillPoint = reader.GetInt();
        tempCharacterData.Gold = reader.GetInt();
        tempCharacterData.CurrentMapName = reader.GetString();
        tempCharacterData.CurrentPosition = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        tempCharacterData.RespawnMapName = reader.GetString();
        tempCharacterData.RespawnPosition = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        tempCharacterData.LastUpdate = reader.GetInt();
        var count = 0;
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterAttribute();
            entry.attributeId = reader.GetString();
            entry.amount = reader.GetInt();
            tempCharacterData.Attributes.Add(entry);
        }
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterBuff();
            entry.id = reader.GetString();
            entry.characterId = reader.GetString();
            entry.dataId = reader.GetString();
            entry.type = (BuffType)reader.GetByte();
            entry.level = reader.GetInt();
            entry.buffRemainsDuration = reader.GetFloat();
            tempCharacterData.Buffs.Add(entry);
        }
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterSkill();
            entry.skillId = reader.GetString();
            entry.level = reader.GetInt();
            entry.coolDownRemainsDuration = reader.GetFloat();
            tempCharacterData.Skills.Add(entry);
        }
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterItem();
            entry.id = reader.GetString();
            entry.itemId = reader.GetString();
            entry.level = reader.GetInt();
            entry.amount = reader.GetInt();
            tempCharacterData.EquipItems.Add(entry);
        }
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterItem();
            entry.id = reader.GetString();
            entry.itemId = reader.GetString();
            entry.level = reader.GetInt();
            entry.amount = reader.GetInt();
            tempCharacterData.NonEquipItems.Add(entry);
        }
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterHotkey();
            entry.hotkeyId = reader.GetString();
            entry.type = (HotkeyType)reader.GetByte();
            entry.dataId = reader.GetString();
            tempCharacterData.Hotkeys.Add(entry);
        }
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterQuest();
            entry.questId = reader.GetString();
            entry.isComplete = reader.GetBool();
            var killMonsterCount = reader.GetInt();
            entry.killedMonsters = new Dictionary<string, int>();
            for (var j = 0; j < killMonsterCount; ++j)
            {
                entry.killedMonsters.Add(reader.GetString(), reader.GetInt());
            }
            tempCharacterData.Quests.Add(entry);
        }

        var rightWeapon = new CharacterItem();
        rightWeapon.id = reader.GetString();
        rightWeapon.itemId = reader.GetString();
        rightWeapon.level = reader.GetInt();
        rightWeapon.amount = reader.GetInt();

        var leftWeapon = new CharacterItem();
        leftWeapon.id = reader.GetString();
        leftWeapon.itemId = reader.GetString();
        leftWeapon.level = reader.GetInt();
        leftWeapon.amount = reader.GetInt();

        var equipWeapons = new EquipWeapons();
        equipWeapons.rightHand = rightWeapon;
        equipWeapons.leftHand = leftWeapon;
        tempCharacterData.EquipWeapons = equipWeapons;

        tempCharacterData.ValidateCharacterData();
        tempCharacterData.CloneTo(characterData);
        return characterData;
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
