using LiteNetLib.Utils;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MultiplayerARPG
{
    public static partial class PlayerCharacterDataExtensions
    {
        public static System.Type ClassType { get; private set; }

        static PlayerCharacterDataExtensions()
        {
            ClassType = typeof(PlayerCharacterDataExtensions);
        }

        public static T CloneTo<T>(this IPlayerCharacterData from, T to) where T : IPlayerCharacterData
        {
            to.Id = from.Id;
            to.DataId = from.DataId;
            to.EntityId = from.EntityId;
            to.UserId = from.UserId;
            to.FactionId = from.FactionId;
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
            to.UserGold = from.UserGold;
            to.UserCash = from.UserCash;
            to.PartyId = from.PartyId;
            to.GuildId = from.GuildId;
            to.GuildRole = from.GuildRole;
            to.SharedGuildExp = from.SharedGuildExp;
            to.EquipWeaponSet = from.EquipWeaponSet;
            to.CurrentMapName = from.CurrentMapName;
            to.CurrentPosition = from.CurrentPosition;
            to.CurrentRotation = from.CurrentRotation;
            to.RespawnMapName = from.RespawnMapName;
            to.RespawnPosition = from.RespawnPosition;
            to.MountDataId = from.MountDataId;
            to.LastDeadTime = from.LastDeadTime;
            to.UnmuteTime = from.UnmuteTime;
            to.LastUpdate = from.LastUpdate;
            to.SelectableWeaponSets = from.SelectableWeaponSets.Clone();
            to.Attributes = from.Attributes.Clone();
            to.Buffs = from.Buffs.Clone();
            to.Hotkeys = from.Hotkeys.Clone();
            to.Quests = from.Quests.Clone();
            to.Currencies = from.Currencies.Clone();
            to.EquipItems = from.EquipItems.Clone();
            to.NonEquipItems = from.NonEquipItems.Clone();
            to.Skills = from.Skills.Clone();
            to.SkillUsages = from.SkillUsages.Clone();
            to.Summons = from.Summons.Clone();
            DevExtUtils.InvokeStaticDevExtMethods(ClassType, "CloneTo", from, to);
            return to;
        }

        public static T ValidateCharacterData<T>(this T character) where T : IPlayerCharacterData
        {
            PlayerCharacter database;
            if (!GameInstance.PlayerCharacters.TryGetValue(character.DataId, out database))
                return character;
            // Validating character attributes
            int returningStatPoint = 0;
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
            foreach (Attribute attribute in GameInstance.Attributes.Values)
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
            int returningSkillPoint = 0;
            HashSet<int> validSkillIds = new HashSet<int>();
            IList<CharacterSkill> characterSkills = character.Skills;
            for (int i = characterSkills.Count - 1; i >= 0; --i)
            {
                CharacterSkill characterSkill = characterSkills[i];
                BaseSkill skill = characterSkill.GetSkill();
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
            foreach (BaseSkill skill in database.CacheSkillLevels.Keys)
            {
                // This skill is valid, so not have to add it
                if (validSkillIds.Contains(skill.DataId))
                    continue;
                CharacterSkill characterSkill = new CharacterSkill();
                characterSkill.dataId = skill.DataId;
                characterSkill.level = 0;
                character.Skills.Add(characterSkill);
            }
            // Validating character equip weapons
            List<CharacterItem> returningItems = new List<CharacterItem>();
            EquipWeapons equipWeapons = character.EquipWeapons;
            CharacterItem rightHand = equipWeapons.rightHand;
            CharacterItem leftHand = equipWeapons.leftHand;
            if (rightHand.GetEquipmentItem() == null)
            {
                if (rightHand.NotEmptySlot())
                    returningItems.Add(rightHand);
                equipWeapons.rightHand = CharacterItem.Empty;
            }
            if (leftHand.GetEquipmentItem() == null)
            {
                if (leftHand.NotEmptySlot())
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
                    if (equipItem.NotEmptySlot())
                        returningItems.Add(equipItem);
                    character.EquipItems.RemoveAt(i);
                }
            }
            // Return items to non equip items
            foreach (CharacterItem returningItem in returningItems)
            {
                if (returningItem.NotEmptySlot())
                    character.AddOrSetNonEquipItems(returningItem);
            }
            character.FillEmptySlots();
            DevExtUtils.InvokeStaticDevExtMethods(ClassType, "ValidateCharacterData", character);
            return character;
        }

        public static T SetNewPlayerCharacterData<T>(this T character, string characterName, int dataId, int entityId) where T : IPlayerCharacterData
        {
            GameInstance gameInstance = GameInstance.Singleton;
            PlayerCharacter playerCharacter;
            if (!GameInstance.PlayerCharacters.TryGetValue(dataId, out playerCharacter))
                return character;
            // General data
            character.DataId = dataId;
            character.EntityId = entityId;
            character.CharacterName = characterName;
            character.Level = 1;
            // Attributes
            foreach (Attribute attribute in GameInstance.Attributes.Values)
            {
                CharacterAttribute characterAttribute = new CharacterAttribute();
                characterAttribute.dataId = attribute.DataId;
                characterAttribute.amount = 0;
                character.Attributes.Add(characterAttribute);
            }
            foreach (BaseSkill skill in playerCharacter.CacheSkillLevels.Keys)
            {
                CharacterSkill characterSkill = new CharacterSkill();
                characterSkill.dataId = skill.DataId;
                characterSkill.level = 0;
                character.Skills.Add(characterSkill);
            }
            // Prepare weapon sets
            character.FillWeaponSetsIfNeeded(character.EquipWeaponSet);
            // Right hand & left hand items
            BaseItem rightHandEquipItem = playerCharacter.RightHandEquipItem;
            BaseItem leftHandEquipItem = playerCharacter.LeftHandEquipItem;
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
            BaseItem[] armorItems = playerCharacter.ArmorItems;
            foreach (BaseItem armorItem in armorItems)
            {
                if (armorItem == null)
                    continue;
                CharacterItem newItem = CharacterItem.Create(armorItem);
                character.EquipItems.Add(newItem);
            }
            // Start items
            List<ItemAmount> startItems = new List<ItemAmount>();
            startItems.AddRange(gameInstance.newCharacterSetting != null ? gameInstance.newCharacterSetting.startItems : gameInstance.startItems);
            startItems.AddRange(playerCharacter.StartItems);
            foreach (ItemAmount startItem in startItems)
            {
                if (startItem.item == null || startItem.amount <= 0)
                    continue;
                short amount = startItem.amount;
                while (amount > 0)
                {
                    short addAmount = amount;
                    if (addAmount > startItem.item.MaxStack)
                        addAmount = startItem.item.MaxStack;
                    if (!character.IncreasingItemsWillOverwhelming(startItem.item.DataId, addAmount))
                        character.AddOrSetNonEquipItems(CharacterItem.Create(startItem.item, 1, addAmount));
                    amount -= addAmount;
                }
            }
            character.FillEmptySlots();
            // Set start stats
            CharacterStats stats = character.GetCaches().Stats;
            character.CurrentHp = (int)stats.hp;
            character.CurrentMp = (int)stats.mp;
            character.CurrentStamina = (int)stats.stamina;
            character.CurrentFood = (int)stats.food;
            character.CurrentWater = (int)stats.water;
            character.Gold = gameInstance.newCharacterSetting != null ? gameInstance.newCharacterSetting.startGold : gameInstance.startGold;
            // Start Map
            BaseMapInfo startMap;
            Vector3 startPosition;
            Vector3 startRotation;
            playerCharacter.GetStartMapAndTransform(character, out startMap, out startPosition, out startRotation);
            character.CurrentMapName = startMap.Id;
            character.CurrentPosition = startPosition;
            character.CurrentRotation = startRotation;
            character.RespawnMapName = startMap.Id;
            character.RespawnPosition = startPosition;
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
            CharacterCurrencySerializationSurrogate currencySS = new CharacterCurrencySerializationSurrogate();
            CharacterSkillSerializationSurrogate skillSS = new CharacterSkillSerializationSurrogate();
            CharacterSkillUsageSerializationSurrogate skillUsageSS = new CharacterSkillUsageSerializationSurrogate();
            CharacterSummonSerializationSurrogate summonSS = new CharacterSummonSerializationSurrogate();
            surrogateSelector.AddSurrogate(typeof(PlayerCharacterData), new StreamingContext(StreamingContextStates.All), playerCharacterDataSS);
            surrogateSelector.AddSurrogate(typeof(CharacterAttribute), new StreamingContext(StreamingContextStates.All), attributeSS);
            surrogateSelector.AddSurrogate(typeof(CharacterBuff), new StreamingContext(StreamingContextStates.All), buffSS);
            surrogateSelector.AddSurrogate(typeof(CharacterHotkey), new StreamingContext(StreamingContextStates.All), hotkeySS);
            surrogateSelector.AddSurrogate(typeof(CharacterItem), new StreamingContext(StreamingContextStates.All), itemSS);
            surrogateSelector.AddSurrogate(typeof(CharacterQuest), new StreamingContext(StreamingContextStates.All), questSS);
            surrogateSelector.AddSurrogate(typeof(CharacterCurrency), new StreamingContext(StreamingContextStates.All), currencySS);
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
            savingData.LastUpdate = System.DateTimeOffset.Now.ToUnixTimeSeconds();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            surrogateSelector.AddAllUnitySurrogate();
            surrogateSelector.AddAllCharacterRelatesDataSurrogate();
            binaryFormatter.SurrogateSelector = surrogateSelector;
            binaryFormatter.Binder = new PlayerCharacterDataTypeBinder();
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
                binaryFormatter.Binder = new PlayerCharacterDataTypeBinder();
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
            PlayerCharacterData characterData;
            foreach (string file in files)
            {
                // If filename is empty or this is not character save, skip it
                if (file.Length <= 4 || file.Contains("_world_") || file.Contains("_storage") || file.Contains("_summon_buffs"))
                    continue;
                characterData = new PlayerCharacterData();
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

        public static void SerializeCharacterData<T>(this T characterData, NetDataWriter writer,
            bool withTransforms = true,
            bool withEquipWeapons = true,
            bool withAttributes = true,
            bool withSkills = true,
            bool withSkillUsages = true,
            bool withBuffs = true,
            bool withEquipItems = true,
            bool withNonEquipItems = true,
            bool withSummons = true,
            bool withHotkeys = true,
            bool withQuests = true,
            bool withCurrencies = true) where T : IPlayerCharacterData
        {
            writer.Put(characterData.Id);
            writer.PutPackedInt(characterData.DataId);
            writer.PutPackedInt(characterData.EntityId);
            writer.Put(characterData.UserId);
            writer.PutPackedInt(characterData.FactionId);
            writer.Put(characterData.CharacterName);
            writer.PutPackedShort(characterData.Level);
            writer.PutPackedInt(characterData.Exp);
            writer.PutPackedInt(characterData.CurrentHp);
            writer.PutPackedInt(characterData.CurrentMp);
            writer.PutPackedInt(characterData.CurrentStamina);
            writer.PutPackedInt(characterData.CurrentFood);
            writer.PutPackedInt(characterData.CurrentWater);
            writer.Put(characterData.StatPoint);
            writer.Put(characterData.SkillPoint);
            writer.PutPackedInt(characterData.Gold);
            writer.PutPackedInt(characterData.UserGold);
            writer.PutPackedInt(characterData.UserCash);
            writer.PutPackedInt(characterData.PartyId);
            writer.PutPackedInt(characterData.GuildId);
            writer.Put(characterData.GuildRole);
            writer.PutPackedInt(characterData.SharedGuildExp);
            writer.Put(characterData.CurrentMapName);
            if (withTransforms)
            {
                writer.Put(characterData.CurrentPosition.x);
                writer.Put(characterData.CurrentPosition.y);
                writer.Put(characterData.CurrentPosition.z);
                writer.Put(characterData.CurrentRotation.x);
                writer.Put(characterData.CurrentRotation.y);
                writer.Put(characterData.CurrentRotation.z);
            }
            writer.Put(characterData.RespawnMapName);
            if (withTransforms)
            {
                writer.Put(characterData.RespawnPosition.x);
                writer.Put(characterData.RespawnPosition.y);
                writer.Put(characterData.RespawnPosition.z);
            }
            writer.PutPackedInt(characterData.MountDataId);
            writer.PutPackedLong(characterData.LastDeadTime);
            writer.PutPackedLong(characterData.UnmuteTime);
            writer.PutPackedLong(characterData.LastUpdate);
            // Attributes
            if (withAttributes)
            {
                writer.Put((byte)characterData.Attributes.Count);
                foreach (CharacterAttribute entry in characterData.Attributes)
                {
                    writer.Put(entry);
                }
            }
            // Buffs
            if (withBuffs)
            {
                writer.Put((byte)characterData.Buffs.Count);
                foreach (CharacterBuff entry in characterData.Buffs)
                {
                    writer.Put(entry);
                }
            }
            // Skills
            if (withSkills)
            {
                writer.Put((short)characterData.Skills.Count);
                foreach (CharacterSkill entry in characterData.Skills)
                {
                    writer.Put(entry);
                }
            }
            // Skill Usages
            if (withSkillUsages)
            {
                writer.Put((byte)characterData.SkillUsages.Count);
                foreach (CharacterSkillUsage entry in characterData.SkillUsages)
                {
                    writer.Put(entry);
                }
            }
            // Summons
            if (withSummons)
            {
                writer.Put((byte)characterData.Summons.Count);
                foreach (CharacterSummon entry in characterData.Summons)
                {
                    writer.Put(entry);
                }
            }
            // Equip Items
            if (withEquipItems)
            {
                writer.Put((byte)characterData.EquipItems.Count);
                foreach (CharacterItem entry in characterData.EquipItems)
                {
                    writer.Put(entry);
                }
            }
            // Non Equip Items
            if (withNonEquipItems)
            {
                writer.Put((short)characterData.NonEquipItems.Count);
                foreach (CharacterItem entry in characterData.NonEquipItems)
                {
                    writer.Put(entry);
                }
            }
            // Hotkeys
            if (withHotkeys)
            {
                writer.Put((byte)characterData.Hotkeys.Count);
                foreach (CharacterHotkey entry in characterData.Hotkeys)
                {
                    writer.Put(entry);
                }
            }
            // Quests
            if (withQuests)
            {
                writer.Put((short)characterData.Quests.Count);
                foreach (CharacterQuest entry in characterData.Quests)
                {
                    writer.Put(entry);
                }
            }
            // Currencies
            if (withCurrencies)
            {
                writer.Put((byte)characterData.Currencies.Count);
                foreach (CharacterCurrency entry in characterData.Currencies)
                {
                    writer.Put(entry);
                }
            }
            // Equip weapon set
            writer.Put(characterData.EquipWeaponSet);
            // Selectable weapon sets
            if (withEquipWeapons)
            {
                writer.Put((byte)characterData.SelectableWeaponSets.Count);
                foreach (EquipWeapons entry in characterData.SelectableWeaponSets)
                {
                    writer.Put(entry);
                }
            }
            DevExtUtils.InvokeStaticDevExtMethods(ClassType, "SerializeCharacterData", characterData, writer);
        }

        public static PlayerCharacterData DeserializeCharacterData(this NetDataReader reader)
        {
            return new PlayerCharacterData().DeserializeCharacterData(reader);
        }

        public static void DeserializeCharacterData(this NetDataReader reader, ref PlayerCharacterData characterData)
        {
            characterData = reader.DeserializeCharacterData();
        }

        public static T DeserializeCharacterData<T>(this T characterData, NetDataReader reader,
            bool withTransforms = true,
            bool withEquipWeapons = true,
            bool withAttributes = true,
            bool withSkills = true,
            bool withSkillUsages = true,
            bool withBuffs = true,
            bool withEquipItems = true,
            bool withNonEquipItems = true,
            bool withSummons = true,
            bool withHotkeys = true,
            bool withQuests = true,
            bool withCurrencies = true) where T : IPlayerCharacterData
        {
            characterData.Id = reader.GetString();
            characterData.DataId = reader.GetPackedInt();
            characterData.EntityId = reader.GetPackedInt();
            characterData.UserId = reader.GetString();
            characterData.FactionId = reader.GetPackedInt();
            characterData.CharacterName = reader.GetString();
            characterData.Level = reader.GetPackedShort();
            characterData.Exp = reader.GetPackedInt();
            characterData.CurrentHp = reader.GetPackedInt();
            characterData.CurrentMp = reader.GetPackedInt();
            characterData.CurrentStamina = reader.GetPackedInt();
            characterData.CurrentFood = reader.GetPackedInt();
            characterData.CurrentWater = reader.GetPackedInt();
            characterData.StatPoint = reader.GetFloat();
            characterData.SkillPoint = reader.GetFloat();
            characterData.Gold = reader.GetPackedInt();
            characterData.UserGold = reader.GetPackedInt();
            characterData.UserCash = reader.GetPackedInt();
            characterData.PartyId = reader.GetPackedInt();
            characterData.GuildId = reader.GetPackedInt();
            characterData.GuildRole = reader.GetByte();
            characterData.SharedGuildExp = reader.GetPackedInt();
            characterData.CurrentMapName = reader.GetString();
            if (withTransforms)
            {
                characterData.CurrentPosition = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
                characterData.CurrentRotation = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            }
            characterData.RespawnMapName = reader.GetString();
            if (withTransforms)
            {
                characterData.RespawnPosition = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            }
            characterData.MountDataId = reader.GetPackedInt();
            characterData.LastDeadTime = reader.GetPackedLong();
            characterData.UnmuteTime = reader.GetPackedLong();
            characterData.LastUpdate = reader.GetPackedLong();
            int count;
            // Attributes
            if (withAttributes)
            {
                count = reader.GetByte();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Attributes.Add(reader.Get<CharacterAttribute>());
                }
            }
            // Buffs
            if (withBuffs)
            {
                count = reader.GetByte();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Buffs.Add(reader.Get<CharacterBuff>());
                }
            }
            // Skills
            if (withSkills)
            {
                count = reader.GetShort();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Skills.Add(reader.Get<CharacterSkill>());
                }
            }
            // Skill Usages
            if (withSkillUsages)
            {
                count = reader.GetByte();
                for (int i = 0; i < count; ++i)
                {
                    characterData.SkillUsages.Add(reader.Get<CharacterSkillUsage>());
                }
            }
            // Summons
            if (withSummons)
            {
                count = reader.GetByte();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Summons.Add(reader.Get<CharacterSummon>());
                }
            }
            // Equip Items
            if (withEquipItems)
            {
                count = reader.GetByte();
                for (int i = 0; i < count; ++i)
                {
                    characterData.EquipItems.Add(reader.Get<CharacterItem>());
                }
            }
            // Non Equip Items
            if (withNonEquipItems)
            {
                count = reader.GetShort();
                for (int i = 0; i < count; ++i)
                {
                    characterData.NonEquipItems.Add(reader.Get<CharacterItem>());
                }
            }
            // Hotkeys
            if (withHotkeys)
            {
                count = reader.GetByte();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Hotkeys.Add(reader.Get<CharacterHotkey>());
                }
            }
            // Quests
            if (withQuests)
            {
                count = reader.GetShort();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Quests.Add(reader.Get<CharacterQuest>());
                }
            }
            // Currencies
            if (withCurrencies)
            {
                count = reader.GetByte();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Currencies.Add(reader.Get<CharacterCurrency>());
                }
            }
            // Equip weapon set
            characterData.EquipWeaponSet = reader.GetByte();
            // Selectable weapon sets
            if (withEquipWeapons)
            {
                count = reader.GetByte();
                for (int i = 0; i < count; ++i)
                {
                    characterData.SelectableWeaponSets.Add(reader.Get<EquipWeapons>());
                }
            }
            DevExtUtils.InvokeStaticDevExtMethods(ClassType, "DeserializeCharacterData", characterData, reader);
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

        public static int IndexOfCurrency(this IPlayerCharacterData data, int dataId)
        {
            IList<CharacterCurrency> list = data.Currencies;
            CharacterCurrency tempCurrency;
            int index = -1;
            for (int i = 0; i < list.Count; ++i)
            {
                tempCurrency = list[i];
                if (tempCurrency.dataId == dataId)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public static bool AddAttribute(this IPlayerCharacterData characterData, out UITextKeys gameMessage, int dataId, short amount = 1, short itemIndex = -1)
        {
            gameMessage = UITextKeys.NONE;
            Attribute attribute;
            if (!GameInstance.Attributes.TryGetValue(dataId, out attribute))
                return false;

            CharacterAttribute characterAtttribute;
            int index = characterData.IndexOfAttribute(dataId);
            if (index < 0)
            {
                characterAtttribute = CharacterAttribute.Create(attribute, 0);
                if (!attribute.CanIncreaseAmount(characterData, (short)(characterAtttribute.amount + amount - 1), out gameMessage, itemIndex < 0))
                    return false;
                if (itemIndex >= 0)
                {
                    if (characterData.DecreaseItemsByIndex(itemIndex, 1))
                        characterData.FillEmptySlots();
                    else
                        return false;
                }
                characterAtttribute.amount += amount;
                characterData.Attributes.Add(characterAtttribute);
            }
            else
            {
                characterAtttribute = characterData.Attributes[index];
                if (!attribute.CanIncreaseAmount(characterData, (short)(characterAtttribute.amount + amount - 1), out gameMessage, itemIndex < 0))
                    return false;
                if (itemIndex >= 0)
                {
                    if (characterData.DecreaseItemsByIndex(itemIndex, 1))
                        characterData.FillEmptySlots();
                    else
                        return false;
                }
                characterAtttribute.amount += amount;
                characterData.Attributes[index] = characterAtttribute;
            }
            return true;
        }

        public static bool ResetAttributes(this IPlayerCharacterData characterData, short itemIndex = -1)
        {
            if (itemIndex >= 0)
            {
                if (characterData.DecreaseItemsByIndex(itemIndex, 1))
                    characterData.FillEmptySlots();
                else
                    return false;
            }

            int countStatPoint = 0;
            CharacterAttribute characterAttribute;
            for (int i = 0; i < characterData.Attributes.Count; ++i)
            {
                characterAttribute = characterData.Attributes[i];
                countStatPoint += characterAttribute.amount;
            }
            characterData.Attributes.Clear();
            characterData.StatPoint += countStatPoint;
            return true;
        }

        public static bool AddSkill(this IPlayerCharacterData characterData, out UITextKeys gameMessageType, int dataId, short level = 1, short itemIndex = -1)
        {
            gameMessageType = UITextKeys.NONE;
            BaseSkill skill;
            if (!GameInstance.Skills.TryGetValue(dataId, out skill))
            {
                gameMessageType = UITextKeys.UI_ERROR_INVALID_SKILL_DATA;
                return false;
            }

            CharacterSkill characterSkill;
            int index = characterData.IndexOfSkill(dataId);
            if (index < 0)
            {
                characterSkill = CharacterSkill.Create(skill, 0);
                if (!skill.CanLevelUp(characterData, (short)(characterSkill.level + level - 1), out gameMessageType, itemIndex < 0, itemIndex < 0))
                    return false;
                if (itemIndex >= 0)
                {
                    if (characterData.DecreaseItemsByIndex(itemIndex, 1))
                        characterData.FillEmptySlots();
                    else
                        return false;
                }
                characterSkill.level += level;
                characterData.Skills.Add(characterSkill);
            }
            else
            {
                characterSkill = characterData.Skills[index];
                if (!skill.CanLevelUp(characterData, (short)(characterSkill.level + level - 1), out gameMessageType, itemIndex < 0, itemIndex < 0))
                    return false;
                if (itemIndex >= 0)
                {
                    if (characterData.DecreaseItemsByIndex(itemIndex, 1))
                        characterData.FillEmptySlots();
                    else
                        return false;
                }
                characterSkill.level += level;
                characterData.Skills[index] = characterSkill;
            }
            return true;
        }

        public static bool ResetSkills(this IPlayerCharacterData characterData, short itemIndex = -1)
        {
            if (itemIndex >= 0)
            {
                if (characterData.DecreaseItemsByIndex(itemIndex, 1))
                    characterData.FillEmptySlots();
                else
                    return false;
            }

            short countSkillPoint = 0;
            CharacterSkill characterSkill;
            for (int i = 0; i < characterData.Skills.Count; ++i)
            {
                characterSkill = characterData.Skills[i];
                countSkillPoint += characterSkill.level;
            }
            characterData.Skills.Clear();
            characterData.SkillPoint += countSkillPoint;
            return true;
        }

        public static void IncreaseCurrencies(this IPlayerCharacterData character, IEnumerable<CurrencyAmount> currencyAmounts, float multiplier = 1)
        {
            if (currencyAmounts == null)
                return;
            foreach (CurrencyAmount currencyAmount in currencyAmounts)
            {
                character.IncreaseCurrency(currencyAmount.currency, Mathf.CeilToInt(currencyAmount.amount * multiplier));
            }
        }

        public static void IncreaseCurrencies(this IPlayerCharacterData character, IEnumerable<CharacterCurrency> currencies, float multiplier = 1)
        {
            if (currencies == null)
                return;
            foreach (CharacterCurrency currency in currencies)
            {
                character.IncreaseCurrency(currency.GetCurrency(), Mathf.CeilToInt(currency.amount * multiplier));
            }
        }

        public static void IncreaseCurrency(this IPlayerCharacterData character, Currency currency, int amount)
        {
            if (currency == null) return;
            int indexOfCurrency = character.IndexOfCurrency(currency.DataId);
            if (indexOfCurrency >= 0)
            {
                CharacterCurrency characterCurrency = character.Currencies[indexOfCurrency];
                characterCurrency.amount += amount;
                character.Currencies[indexOfCurrency] = characterCurrency;
            }
            else
            {
                character.Currencies.Add(CharacterCurrency.Create(currency, amount));
            }
        }

        public static void DecreaseCurrencies(this IPlayerCharacterData character, IEnumerable<CurrencyAmount> currencyAmounts, float multiplier = 1)
        {
            if (currencyAmounts == null)
                return;
            foreach (CurrencyAmount currencyAmount in currencyAmounts)
            {
                character.DecreaseCurrency(currencyAmount.currency, Mathf.CeilToInt(currencyAmount.amount * multiplier));
            }
        }

        public static void DecreaseCurrency(this IPlayerCharacterData character, Currency currency, int amount)
        {
            if (currency == null) return;
            int indexOfCurrency = character.IndexOfCurrency(currency.DataId);
            if (indexOfCurrency >= 0)
            {
                CharacterCurrency characterCurrency = character.Currencies[indexOfCurrency];
                characterCurrency.amount -= amount;
                character.Currencies[indexOfCurrency] = characterCurrency;
            }
            else
            {
                character.Currencies.Add(CharacterCurrency.Create(currency, -amount));
            }
        }

        public static bool HasEnoughCurrencies(this IPlayerCharacterData character, IEnumerable<CurrencyAmount> currencyAmounts, float multiplier = 1)
        {
            if (currencyAmounts == null)
                return true;
            foreach (CurrencyAmount currencyAmount in currencyAmounts)
            {
                if (currencyAmount.currency == null ||
                    currencyAmount.amount == 0)
                    continue;
                int indexOfCurrency = character.IndexOfCurrency(currencyAmount.currency.DataId);
                int checkAmount = Mathf.CeilToInt(currencyAmount.amount * multiplier);
                if (indexOfCurrency < 0 || character.Currencies[indexOfCurrency].amount < checkAmount)
                    return false;
            }
            return true;
        }

        public static void ClearParty(this IPlayerCharacterData character)
        {
            character.PartyId = 0;
        }

        public static void ClearGuild(this IPlayerCharacterData character)
        {
            character.GuildId = 0;
            character.GuildRole = 0;
            character.SharedGuildExp = 0;
        }

        public static bool IsMuting(this IPlayerCharacterData character)
        {
            return character.UnmuteTime > 0 && character.UnmuteTime > (BaseGameNetworkManager.Singleton.ServerTimestamp / 1000);
        }
    }
}
