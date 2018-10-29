using LiteNetLib.Utils;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using MultiplayerARPG;

public static partial class PlayerCharacterDataExtension
{
    private static System.Type classType;
    public static System.Type ClassType
    {
        get
        {
            if (classType == null)
                classType = typeof(PlayerCharacterDataExtension);
            return classType;
        }
    }

    public static T CloneTo<T>(this IPlayerCharacterData from, T to) where T : IPlayerCharacterData
    {
        to.Id = from.Id;
        to.DataId = from.DataId;
        to.EntityId = from.EntityId;
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
        to.PartyId = from.PartyId;
        to.GuildId = from.GuildId;
        to.GuildRole = from.GuildRole;
        to.SharedGuildExp = from.SharedGuildExp;
        to.EquipWeapons = from.EquipWeapons;
        to.CurrentMapName = from.CurrentMapName;
        to.CurrentPosition = from.CurrentPosition;
        to.RespawnMapName = from.RespawnMapName;
        to.RespawnPosition = from.RespawnPosition;
        to.LastUpdate = from.LastUpdate;
        to.Attributes = new List<CharacterAttribute>(from.Attributes);
        to.Buffs = new List<CharacterBuff>(from.Buffs);
        to.Hotkeys = new List<CharacterHotkey>(from.Hotkeys);
        to.Quests = new List<CharacterQuest>(from.Quests);
        to.EquipItems = new List<CharacterItem>(from.EquipItems);
        to.NonEquipItems = new List<CharacterItem>(from.NonEquipItems);
        to.Skills = new List<CharacterSkill>(from.Skills);
        to.SkillUsages = new List<CharacterSkillUsage>(from.SkillUsages);
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "CloneTo", from, to);
        return to;
    }

    public static T ValidateCharacterData<T>(this T character) where T : IPlayerCharacterData
    {
        var gameInstance = GameInstance.Singleton;
        PlayerCharacter database;
        if (!GameInstance.PlayerCharacters.TryGetValue(character.DataId, out database))
            return character;
        // Validating character attributes
        short returningStatPoint = 0;
        var validAttributeIds = new HashSet<int>();
        var characterAttributes = character.Attributes;
        for (var i = characterAttributes.Count - 1; i >= 0; --i)
        {
            var characterAttribute = characterAttributes[i];
            var attributeDataId = characterAttribute.dataId;
            // If attribute is invalid
            if (characterAttribute.GetAttribute() == null ||
                validAttributeIds.Contains(attributeDataId))
            {
                returningStatPoint += characterAttribute.amount;
                character.Attributes.RemoveAt(i);
            }
            else
                validAttributeIds.Add(attributeDataId);
        }
        character.StatPoint += returningStatPoint;
        // Add character's attributes
        var attributes = GameInstance.Attributes.Values;
        foreach (var attribute in attributes)
        {
            // This attribute is valid, so not have to add it
            if (validAttributeIds.Contains(attribute.DataId))
                continue;
            var characterAttribute = new CharacterAttribute();
            characterAttribute.dataId = attribute.DataId;
            characterAttribute.amount = 0;
            character.Attributes.Add(characterAttribute);
        }
        // Validating character skills
        short returningSkillPoint = 0;
        var validSkillIds = new HashSet<int>();
        var characterSkills = character.Skills;
        for (var i = characterSkills.Count - 1; i >= 0; --i)
        {
            var characterSkill = characterSkills[i];
            var skillDataId = characterSkill.dataId;
            // If skill is invalid or this character database does not have skill
            if (characterSkill.GetSkill() == null ||
                !database.CacheSkillLevels.ContainsKey(skillDataId) ||
                validSkillIds.Contains(skillDataId))
            {
                returningSkillPoint += characterSkill.level;
                character.Skills.RemoveAt(i);
            }
            else
                validSkillIds.Add(skillDataId);
        }
        character.SkillPoint += returningSkillPoint;
        // Add character's skills
        var skillLevels = database.skillLevels;
        foreach (var skillLevel in skillLevels)
        {
            // Skip empty skill data
            if (skillLevel.skill == null)
            {
                Debug.LogWarning("[ValidateCharacterData] Character: " + character.CharacterName + "'s Skill data is empty");
                continue;
            }
            // This skill is valid, so not have to add it
            if (validSkillIds.Contains(skillLevel.skill.DataId))
                continue;
            var characterSkill = new CharacterSkill();
            characterSkill.dataId = skillLevel.skill.DataId;
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
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "ValidateCharacterData", character);
        return character;
    }

    public static T SetNewPlayerCharacterData<T>(this T character, string characterName, int dataId, int entityId) where T : IPlayerCharacterData
    {
        var gameInstance = GameInstance.Singleton;
        PlayerCharacter database;
        if (!GameInstance.PlayerCharacters.TryGetValue(dataId, out database))
            return character;
        // Player character database
        var playerCharacter = database as PlayerCharacter;
        // Attributes
        var attributes = GameInstance.Attributes.Values;
        foreach (var attribute in attributes)
        {
            var characterAttribute = new CharacterAttribute();
            characterAttribute.dataId = attribute.DataId;
            characterAttribute.amount = 0;
            character.Attributes.Add(characterAttribute);
        }
        var skillLevels = playerCharacter.skillLevels;
        foreach (var skillLevel in skillLevels)
        {
            if (skillLevel.skill == null)
                continue;
            var characterSkill = new CharacterSkill();
            characterSkill.dataId = skillLevel.skill.DataId;
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
        character.DataId = dataId;
        character.EntityId = entityId;
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
        var startMap = playerCharacter.StartMap;
        character.CurrentMapName = startMap.scene.SceneName;
        character.RespawnMapName = startMap.scene.SceneName;
        character.CurrentPosition = startMap.startPosition;
        character.RespawnPosition = startMap.startPosition;
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "SetNewCharacterData", character, characterName, dataId);
        return character;
    }

    public static void AddAllCharacterRelatesDataSurrogate(this SurrogateSelector surrogateSelector)
    {
        var playerCharacterDataSS = new PlayerCharacterSerializationSurrogate();
        var attributeSS = new CharacterAttributeSerializationSurrogate();
        var buffSS = new CharacterBuffSerializationSurrogate();
        var hotkeySS = new CharacterHotkeySerializationSurrogate();
        var itemSS = new CharacterItemSerializationSurrogate();
        var questSS = new CharacterQuestSerializationSurrogate();
        var skillSS = new CharacterSkillSerializationSurrogate();
        surrogateSelector.AddSurrogate(typeof(PlayerCharacterData), new StreamingContext(StreamingContextStates.All), playerCharacterDataSS);
        surrogateSelector.AddSurrogate(typeof(CharacterAttribute), new StreamingContext(StreamingContextStates.All), attributeSS);
        surrogateSelector.AddSurrogate(typeof(CharacterBuff), new StreamingContext(StreamingContextStates.All), buffSS);
        surrogateSelector.AddSurrogate(typeof(CharacterHotkey), new StreamingContext(StreamingContextStates.All), hotkeySS);
        surrogateSelector.AddSurrogate(typeof(CharacterItem), new StreamingContext(StreamingContextStates.All), itemSS);
        surrogateSelector.AddSurrogate(typeof(CharacterQuest), new StreamingContext(StreamingContextStates.All), questSS);
        surrogateSelector.AddSurrogate(typeof(CharacterSkill), new StreamingContext(StreamingContextStates.All), skillSS);
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "AddAllCharacterRelatesDataSurrogate", surrogateSelector);
    }

    public static void SavePersistentCharacterData<T>(this T characterData) where T : IPlayerCharacterData
    {
        var savingData = new PlayerCharacterData();
        characterData.CloneTo(savingData);
        if (string.IsNullOrEmpty(savingData.Id))
            return;
        savingData.LastUpdate = (int)(System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond);
        var binaryFormatter = new BinaryFormatter();
        var surrogateSelector = new SurrogateSelector();
        surrogateSelector.AddAllUnitySurrogate();
        surrogateSelector.AddAllCharacterRelatesDataSurrogate();
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
            surrogateSelector.AddAllCharacterRelatesDataSurrogate();
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
            // If filename is empty or this is not character save, skip it
            if (file.Length <= 4 || file.Contains("_world_"))
                continue;
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
        writer.Put(characterData.DataId);
        writer.Put(characterData.EntityId);
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
        writer.Put(characterData.PartyId);
        writer.Put(characterData.GuildId);
        writer.Put(characterData.GuildRole);
        writer.Put(characterData.SharedGuildExp);
        writer.Put(characterData.CurrentMapName);
        writer.Put(characterData.CurrentPosition.x);
        writer.Put(characterData.CurrentPosition.y);
        writer.Put(characterData.CurrentPosition.z);
        writer.Put(characterData.RespawnMapName);
        writer.Put(characterData.RespawnPosition.x);
        writer.Put(characterData.RespawnPosition.y);
        writer.Put(characterData.RespawnPosition.z);
        writer.Put(characterData.LastUpdate);
        writer.Put((byte)characterData.Attributes.Count);
        foreach (var entry in characterData.Attributes)
        {
            writer.Put(entry.dataId);
            writer.Put(entry.amount);
        }
        writer.Put((byte)characterData.Buffs.Count);
        foreach (var entry in characterData.Buffs)
        {
            writer.Put(entry.dataId);
            writer.Put((byte)entry.type);
            writer.Put(entry.level);
            writer.Put(entry.buffRemainsDuration);
        }
        writer.Put((byte)characterData.Skills.Count);
        foreach (var entry in characterData.Skills)
        {
            writer.Put(entry.dataId);
            writer.Put(entry.level);
        }
        writer.Put((byte)characterData.SkillUsages.Count);
        foreach (var entry in characterData.SkillUsages)
        {
            writer.Put(entry.dataId);
            writer.Put((byte)entry.type);
            writer.Put(entry.coolDownRemainsDuration);
        }
        writer.Put((byte)characterData.EquipItems.Count);
        foreach (var entry in characterData.EquipItems)
        {
            writer.Put(entry.dataId);
            writer.Put(entry.level);
            writer.Put(entry.amount);
            writer.Put(entry.durability);
        }
        writer.Put((short)characterData.NonEquipItems.Count);
        foreach (var entry in characterData.NonEquipItems)
        {
            writer.Put(entry.dataId);
            writer.Put(entry.level);
            writer.Put(entry.amount);
            writer.Put(entry.durability);
        }
        writer.Put((byte)characterData.Hotkeys.Count);
        foreach (var entry in characterData.Hotkeys)
        {
            writer.Put(entry.hotkeyId);
            writer.Put((byte)entry.type);
            writer.Put(entry.dataId);
        }
        writer.Put((byte)characterData.Quests.Count);
        foreach (var entry in characterData.Quests)
        {
            writer.Put(entry.dataId);
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
        writer.Put(rightHand.dataId);
        writer.Put(rightHand.level);
        writer.Put(rightHand.amount);
        writer.Put(rightHand.durability);
        var leftHand = characterData.EquipWeapons.leftHand;
        writer.Put(leftHand.dataId);
        writer.Put(leftHand.level);
        writer.Put(leftHand.amount);
        writer.Put(leftHand.durability);
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "SerializeCharacterData", characterData, writer);
    }

    public static T DeserializeCharacterData<T>(this T characterData, NetDataReader reader) where T : IPlayerCharacterData
    {
        var tempCharacterData = new PlayerCharacterData();
        tempCharacterData.Id = reader.GetString();
        tempCharacterData.DataId = reader.GetInt();
        tempCharacterData.EntityId = reader.GetInt();
        tempCharacterData.CharacterName = reader.GetString();
        tempCharacterData.Level = reader.GetShort();
        tempCharacterData.Exp = reader.GetInt();
        tempCharacterData.CurrentHp = reader.GetInt();
        tempCharacterData.CurrentMp = reader.GetInt();
        tempCharacterData.CurrentStamina = reader.GetInt();
        tempCharacterData.CurrentFood = reader.GetInt();
        tempCharacterData.CurrentWater = reader.GetInt();
        tempCharacterData.StatPoint = reader.GetShort();
        tempCharacterData.SkillPoint = reader.GetShort();
        tempCharacterData.Gold = reader.GetInt();
        tempCharacterData.PartyId = reader.GetInt();
        tempCharacterData.GuildId = reader.GetInt();
        tempCharacterData.GuildRole = reader.GetByte();
        tempCharacterData.SharedGuildExp = reader.GetInt();
        tempCharacterData.CurrentMapName = reader.GetString();
        tempCharacterData.CurrentPosition = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        tempCharacterData.RespawnMapName = reader.GetString();
        tempCharacterData.RespawnPosition = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        tempCharacterData.LastUpdate = reader.GetInt();
        int count = 0;
        count = reader.GetByte();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterAttribute();
            entry.dataId = reader.GetInt();
            entry.amount = reader.GetShort();
            tempCharacterData.Attributes.Add(entry);
        }
        count = reader.GetByte();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterBuff();
            entry.dataId = reader.GetInt();
            entry.type = (BuffType)reader.GetByte();
            entry.level = reader.GetShort();
            entry.buffRemainsDuration = reader.GetFloat();
            tempCharacterData.Buffs.Add(entry);
        }
        count = reader.GetByte();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterSkill();
            entry.dataId = reader.GetInt();
            entry.level = reader.GetShort();
            tempCharacterData.Skills.Add(entry);
        }
        count = reader.GetByte();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterSkillUsage();
            entry.dataId = reader.GetInt();
            entry.type = (SkillUsageType)reader.GetByte();
            entry.coolDownRemainsDuration = reader.GetFloat();
        }
        count = reader.GetByte();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterItem();
            entry.dataId = reader.GetInt();
            entry.level = reader.GetShort();
            entry.amount = reader.GetShort();
            entry.durability = reader.GetFloat();
            tempCharacterData.EquipItems.Add(entry);
        }
        count = reader.GetShort();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterItem();
            entry.dataId = reader.GetInt();
            entry.level = reader.GetShort();
            entry.amount = reader.GetShort();
            entry.durability = reader.GetFloat();
            tempCharacterData.NonEquipItems.Add(entry);
        }
        count = reader.GetByte();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterHotkey();
            entry.hotkeyId = reader.GetString();
            entry.type = (HotkeyType)reader.GetByte();
            entry.dataId = reader.GetInt();
            tempCharacterData.Hotkeys.Add(entry);
        }
        count = reader.GetByte();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterQuest();
            entry.dataId = reader.GetInt();
            entry.isComplete = reader.GetBool();
            var killMonsterCount = reader.GetInt();
            entry.killedMonsters = new Dictionary<int, int>();
            for (var j = 0; j < killMonsterCount; ++j)
            {
                entry.killedMonsters.Add(reader.GetInt(), reader.GetInt());
            }
            tempCharacterData.Quests.Add(entry);
        }

        var rightWeapon = new CharacterItem();
        rightWeapon.dataId = reader.GetInt();
        rightWeapon.level = reader.GetShort();
        rightWeapon.amount = reader.GetShort();
        rightWeapon.durability = reader.GetFloat();

        var leftWeapon = new CharacterItem();
        leftWeapon.dataId = reader.GetInt();
        leftWeapon.level = reader.GetShort();
        leftWeapon.amount = reader.GetShort();
        leftWeapon.durability = reader.GetFloat();

        var equipWeapons = new EquipWeapons();
        equipWeapons.rightHand = rightWeapon;
        equipWeapons.leftHand = leftWeapon;
        tempCharacterData.EquipWeapons = equipWeapons;

        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "DeserializeCharacterData", characterData, reader);

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

    public static int IndexOfQuest(this IPlayerCharacterData data, int dataId)
    {
        var list = data.Quests;
        CharacterQuest tempQuest;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempQuest = list[i];
            if (tempQuest.dataId == dataId)
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
