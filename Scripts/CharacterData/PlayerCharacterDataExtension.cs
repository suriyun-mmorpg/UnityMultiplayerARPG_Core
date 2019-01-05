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
        to.Summons = new List<CharacterSummon>(from.Summons);
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "CloneTo", from, to);
        return to;
    }

    public static T ValidateCharacterData<T>(this T character) where T : IPlayerCharacterData
    {
        GameInstance gameInstance = GameInstance.Singleton;
        PlayerCharacter database;
        if (!GameInstance.PlayerCharacters.TryGetValue(character.DataId, out database))
            return character;
        // Validating character attributes
        short returningStatPoint = 0;
        HashSet<int> validAttributeIds = new HashSet<int>();
        IList<CharacterAttribute> characterAttributes = character.Attributes;
        for (int i = characterAttributes.Count - 1; i >= 0; --i)
        {
            CharacterAttribute characterAttribute = characterAttributes[i];
            int attributeDataId = characterAttribute.dataId;
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
        Dictionary<int, Attribute>.ValueCollection attributes = GameInstance.Attributes.Values;
        foreach (Attribute attribute in attributes)
        {
            // This attribute is valid, so not have to add it
            if (validAttributeIds.Contains(attribute.DataId))
                continue;
            CharacterAttribute characterAttribute = new CharacterAttribute();
            characterAttribute.dataId = attribute.DataId;
            characterAttribute.amount = 0;
            character.Attributes.Add(characterAttribute);
        }
        // Validating character skills
        short returningSkillPoint = 0;
        HashSet<int> validSkillIds = new HashSet<int>();
        IList<CharacterSkill> characterSkills = character.Skills;
        for (int i = characterSkills.Count - 1; i >= 0; --i)
        {
            CharacterSkill characterSkill = characterSkills[i];
            Skill skill = characterSkill.GetSkill();
            // If skill is invalid or this character database does not have skill
            if (characterSkill.GetSkill() == null ||
                !database.CacheSkillLevels.ContainsKey(skill) ||
                validSkillIds.Contains(skill.DataId))
            {
                returningSkillPoint += characterSkill.level;
                character.Skills.RemoveAt(i);
            }
            else
                validSkillIds.Add(skill.DataId);
        }
        character.SkillPoint += returningSkillPoint;
        // Add character's skills
        Dictionary<Skill, short> skillLevels = database.CacheSkillLevels;
        foreach (KeyValuePair<Skill, short> skillLevel in skillLevels)
        {
            Skill skill = skillLevel.Key;
            // This skill is valid, so not have to add it
            if (validSkillIds.Contains(skill.DataId))
                continue;
            CharacterSkill characterSkill = new CharacterSkill();
            characterSkill.dataId = skill.DataId;
            characterSkill.level = skillLevel.Value;
            character.Skills.Add(characterSkill);
        }
        // Validating character equip weapons
        List<CharacterItem> returningItems = new List<CharacterItem>();
        EquipWeapons equipWeapons = character.EquipWeapons;
        CharacterItem rightHand = equipWeapons.rightHand;
        CharacterItem leftHand = equipWeapons.leftHand;
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
        IList<CharacterItem> equipItems = character.EquipItems;
        for (int i = equipItems.Count - 1; i >= 0; --i)
        {
            CharacterItem equipItem = equipItems[i];
            // If equipment is invalid
            if (equipItem.GetEquipmentItem() == null)
            {
                if (equipItem.IsValid())
                    returningItems.Add(equipItem);
                character.EquipItems.RemoveAt(i);
            }
        }
        // Return items to non equip items
        foreach (CharacterItem returningItem in returningItems)
        {
            character.NonEquipItems.Add(returningItem);
        }
        // Validating character non equip items
        IList<CharacterItem> nonEquipItems = character.NonEquipItems;
        for (int i = nonEquipItems.Count - 1; i >= 0; --i)
        {
            CharacterItem nonEquipItem = nonEquipItems[i];
            // If equipment is invalid
            if (!nonEquipItem.IsValid())
                character.NonEquipItems.RemoveAt(i);
        }
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "ValidateCharacterData", character);
        return character;
    }

    public static T SetNewPlayerCharacterData<T>(this T character, string characterName, int dataId, int entityId) where T : IPlayerCharacterData
    {
        GameInstance gameInstance = GameInstance.Singleton;
        PlayerCharacter database;
        if (!GameInstance.PlayerCharacters.TryGetValue(dataId, out database))
            return character;
        // Player character database
        PlayerCharacter playerCharacter = database as PlayerCharacter;
        // Attributes
        Dictionary<int, Attribute>.ValueCollection attributes = GameInstance.Attributes.Values;
        foreach (Attribute attribute in attributes)
        {
            CharacterAttribute characterAttribute = new CharacterAttribute();
            characterAttribute.dataId = attribute.DataId;
            characterAttribute.amount = 0;
            character.Attributes.Add(characterAttribute);
        }
        Dictionary<Skill, short> skillLevels = playerCharacter.CacheSkillLevels;
        foreach (KeyValuePair<Skill, short> skillLevel in skillLevels)
        {
            CharacterSkill characterSkill = new CharacterSkill();
            characterSkill.dataId = skillLevel.Key.DataId;
            characterSkill.level = skillLevel.Value;
            character.Skills.Add(characterSkill);
        }
        // Right hand & left hand items
        Item rightHandEquipItem = playerCharacter.rightHandEquipItem;
        Item leftHandEquipItem = playerCharacter.leftHandEquipItem;
        EquipWeapons equipWeapons = new EquipWeapons();
        // Right hand equipped item
        if (rightHandEquipItem != null)
        {
            CharacterItem newItem = CharacterItem.Create(rightHandEquipItem);
            equipWeapons.rightHand = newItem;
        }
        // Left hand equipped item
        if (leftHandEquipItem != null)
        {
            CharacterItem newItem = CharacterItem.Create(leftHandEquipItem);
            equipWeapons.leftHand = newItem;
        }
        character.EquipWeapons = equipWeapons;
        // Armors
        Item[] armorItems = playerCharacter.armorItems;
        foreach (Item armorItem in armorItems)
        {
            if (armorItem == null)
                continue;
            CharacterItem newItem = CharacterItem.Create(armorItem);
            character.EquipItems.Add(newItem);
        }
        // General data
        character.DataId = dataId;
        character.EntityId = entityId;
        character.CharacterName = characterName;
        character.Level = 1;
        CharacterStats stats = character.GetStats();
        character.CurrentHp = (int)stats.hp;
        character.CurrentMp = (int)stats.mp;
        character.CurrentStamina = (int)stats.stamina;
        character.CurrentFood = (int)stats.food;
        character.CurrentWater = (int)stats.water;
        character.Gold = gameInstance.startGold;
        // Inventory
        ItemAmount[] startItems = gameInstance.startItems;
        foreach (ItemAmount startItem in startItems)
        {
            if (startItem.item == null || startItem.amount <= 0)
                continue;
            short amount = startItem.amount;
            if (amount > startItem.item.maxStack)
                amount = startItem.item.maxStack;
            CharacterItem newItem = CharacterItem.Create(startItem.item, 1, amount);
            character.NonEquipItems.Add(newItem);
        }
        // Position
        MapInfo startMap = playerCharacter.StartMap;
        character.CurrentMapName = startMap.scene.SceneName;
        character.RespawnMapName = startMap.scene.SceneName;
        character.CurrentPosition = startMap.startPosition;
        character.RespawnPosition = startMap.startPosition;
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "SetNewCharacterData", character, characterName, dataId, entityId);
        return character;
    }

    public static void AddAllCharacterRelatesDataSurrogate(this SurrogateSelector surrogateSelector)
    {
        PlayerCharacterSerializationSurrogate playerCharacterDataSS = new PlayerCharacterSerializationSurrogate();
        CharacterAttributeSerializationSurrogate attributeSS = new CharacterAttributeSerializationSurrogate();
        CharacterBuffSerializationSurrogate buffSS = new CharacterBuffSerializationSurrogate();
        CharacterHotkeySerializationSurrogate hotkeySS = new CharacterHotkeySerializationSurrogate();
        CharacterItemSerializationSurrogate itemSS = new CharacterItemSerializationSurrogate();
        CharacterQuestSerializationSurrogate questSS = new CharacterQuestSerializationSurrogate();
        CharacterSkillSerializationSurrogate skillSS = new CharacterSkillSerializationSurrogate();
        CharacterSkillUsageSerializationSurrogate skillUsageSS = new CharacterSkillUsageSerializationSurrogate();
        CharacterSummonSerializationSurrogate summonSS = new CharacterSummonSerializationSurrogate();
        surrogateSelector.AddSurrogate(typeof(PlayerCharacterData), new StreamingContext(StreamingContextStates.All), playerCharacterDataSS);
        surrogateSelector.AddSurrogate(typeof(CharacterAttribute), new StreamingContext(StreamingContextStates.All), attributeSS);
        surrogateSelector.AddSurrogate(typeof(CharacterBuff), new StreamingContext(StreamingContextStates.All), buffSS);
        surrogateSelector.AddSurrogate(typeof(CharacterHotkey), new StreamingContext(StreamingContextStates.All), hotkeySS);
        surrogateSelector.AddSurrogate(typeof(CharacterItem), new StreamingContext(StreamingContextStates.All), itemSS);
        surrogateSelector.AddSurrogate(typeof(CharacterQuest), new StreamingContext(StreamingContextStates.All), questSS);
        surrogateSelector.AddSurrogate(typeof(CharacterSkill), new StreamingContext(StreamingContextStates.All), skillSS);
        surrogateSelector.AddSurrogate(typeof(CharacterSkillUsage), new StreamingContext(StreamingContextStates.All), skillUsageSS);
        surrogateSelector.AddSurrogate(typeof(CharacterSummon), new StreamingContext(StreamingContextStates.All), summonSS);
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "AddAllCharacterRelatesDataSurrogate", surrogateSelector);
    }

    public static void SavePersistentCharacterData<T>(this T characterData) where T : IPlayerCharacterData
    {
        PlayerCharacterData savingData = new PlayerCharacterData();
        characterData.CloneTo(savingData);
        if (string.IsNullOrEmpty(savingData.Id))
            return;
        savingData.LastUpdate = (int)(System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        SurrogateSelector surrogateSelector = new SurrogateSelector();
        surrogateSelector.AddAllUnitySurrogate();
        surrogateSelector.AddAllCharacterRelatesDataSurrogate();
        binaryFormatter.SurrogateSelector = surrogateSelector;
        string path = Application.persistentDataPath + "/" + savingData.Id + ".sav";
        Debug.Log("Character Saving to: " + path);
        FileStream file = File.Open(path, FileMode.OpenOrCreate);
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
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            surrogateSelector.AddAllUnitySurrogate();
            surrogateSelector.AddAllCharacterRelatesDataSurrogate();
            binaryFormatter.SurrogateSelector = surrogateSelector;
            FileStream file = File.Open(path, FileMode.Open);
            PlayerCharacterData loadedData = (PlayerCharacterData)binaryFormatter.Deserialize(file);
            file.Close();
            loadedData.CloneTo(characterData);
        }
        return characterData;
    }

    public static List<PlayerCharacterData> LoadAllPersistentCharacterData()
    {
        List<PlayerCharacterData> result = new List<PlayerCharacterData>();
        string path = Application.persistentDataPath;
        string[] files = Directory.GetFiles(path, "*.sav");
        Debug.Log("Characters loading from: " + path);
        foreach (string file in files)
        {
            // If filename is empty or this is not character save, skip it
            if (file.Length <= 4 || file.Contains("_world_"))
                continue;
            PlayerCharacterData characterData = new PlayerCharacterData();
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
        foreach (CharacterAttribute entry in characterData.Attributes)
        {
            entry.Serialize(writer);
        }
        writer.Put((byte)characterData.Buffs.Count);
        foreach (CharacterBuff entry in characterData.Buffs)
        {
            entry.Serialize(writer);
        }
        writer.Put((byte)characterData.Skills.Count);
        foreach (CharacterSkill entry in characterData.Skills)
        {
            entry.Serialize(writer);
        }
        writer.Put((byte)characterData.SkillUsages.Count);
        foreach (CharacterSkillUsage entry in characterData.SkillUsages)
        {
            entry.Serialize(writer);
        }
        writer.Put((byte)characterData.Summons.Count);
        foreach (CharacterSummon entry in characterData.Summons)
        {
            entry.Serialize(writer);
        }
        writer.Put((byte)characterData.EquipItems.Count);
        foreach (CharacterItem entry in characterData.EquipItems)
        {
            entry.Serialize(writer);
        }
        writer.Put((short)characterData.NonEquipItems.Count);
        foreach (CharacterItem entry in characterData.NonEquipItems)
        {
            entry.Serialize(writer);
        }
        writer.Put((byte)characterData.Hotkeys.Count);
        foreach (CharacterHotkey entry in characterData.Hotkeys)
        {
            entry.Serialize(writer);
        }
        writer.Put((byte)characterData.Quests.Count);
        foreach (CharacterQuest entry in characterData.Quests)
        {
            entry.Serialize(writer);
        }
        characterData.EquipWeapons.Serialize(writer);
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "SerializeCharacterData", characterData, writer);
    }

    public static T DeserializeCharacterData<T>(this T characterData, NetDataReader reader) where T : IPlayerCharacterData
    {
        PlayerCharacterData tempCharacterData = new PlayerCharacterData();
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
        for (int i = 0; i < count; ++i)
        {
            CharacterAttribute entry = new CharacterAttribute();
            entry.Deserialize(reader);
            tempCharacterData.Attributes.Add(entry);
        }
        count = reader.GetByte();
        for (int i = 0; i < count; ++i)
        {
            CharacterBuff entry = new CharacterBuff();
            entry.Deserialize(reader);
            tempCharacterData.Buffs.Add(entry);
        }
        count = reader.GetByte();
        for (int i = 0; i < count; ++i)
        {
            CharacterSkill entry = new CharacterSkill();
            entry.Deserialize(reader);
            tempCharacterData.Skills.Add(entry);
        }
        count = reader.GetByte();
        for (int i = 0; i < count; ++i)
        {
            CharacterSkillUsage entry = new CharacterSkillUsage();
            entry.Deserialize(reader);
            tempCharacterData.SkillUsages.Add(entry);
        }
        count = reader.GetByte();
        for (int i = 0; i < count; ++i)
        {
            CharacterSummon entry = new CharacterSummon();
            entry.Deserialize(reader);
            tempCharacterData.Summons.Add(entry);
        }
        count = reader.GetByte();
        for (int i = 0; i < count; ++i)
        {
            CharacterItem entry = new CharacterItem();
            entry.Deserialize(reader);
            tempCharacterData.EquipItems.Add(entry);
        }
        count = reader.GetShort();
        for (int i = 0; i < count; ++i)
        {
            CharacterItem entry = new CharacterItem();
            entry.Deserialize(reader);
            tempCharacterData.NonEquipItems.Add(entry);
        }
        count = reader.GetByte();
        for (int i = 0; i < count; ++i)
        {
            CharacterHotkey entry = new CharacterHotkey();
            entry.Deserialize(reader);
            tempCharacterData.Hotkeys.Add(entry);
        }
        count = reader.GetByte();
        for (int i = 0; i < count; ++i)
        {
            CharacterQuest entry = new CharacterQuest();
            entry.Deserialize(reader);
            tempCharacterData.Quests.Add(entry);
        }
        EquipWeapons equipWeapons = new EquipWeapons();
        equipWeapons.Deserialize(reader);
        tempCharacterData.EquipWeapons = equipWeapons;
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "DeserializeCharacterData", characterData, reader);

        tempCharacterData.ValidateCharacterData();
        tempCharacterData.CloneTo(characterData);
        return characterData;
    }

    public static int IndexOfHotkey(this IPlayerCharacterData data, string hotkeyId)
    {
        IList<CharacterHotkey> list = data.Hotkeys;
        CharacterHotkey tempHotkey;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
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
        IList<CharacterQuest> list = data.Quests;
        CharacterQuest tempQuest;
        int index = -1;
        for (int i = 0; i < list.Count; ++i)
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
