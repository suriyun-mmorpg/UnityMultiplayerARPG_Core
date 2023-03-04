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

        public static T CloneTo<T>(this IPlayerCharacterData from, T to,
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
            to.CurrentPositionX = from.CurrentPositionX;
            to.CurrentPositionY = from.CurrentPositionY;
            to.CurrentPositionZ = from.CurrentPositionZ;
            to.CurrentRotationX = from.CurrentRotationX;
            to.CurrentRotationY = from.CurrentRotationY;
            to.CurrentRotationZ = from.CurrentRotationZ;
            to.RespawnMapName = from.RespawnMapName;
            to.RespawnPositionX = from.RespawnPositionX;
            to.RespawnPositionY = from.RespawnPositionY;
            to.RespawnPositionZ = from.RespawnPositionZ;
            to.MountDataId = from.MountDataId;
            to.IconDataId = from.IconDataId;
            to.FrameDataId = from.FrameDataId;
            to.TitleDataId = from.TitleDataId;
            to.LastDeadTime = from.LastDeadTime;
            to.UnmuteTime = from.UnmuteTime;
            to.LastUpdate = from.LastUpdate;
            if (withEquipWeapons)
                to.SelectableWeaponSets = from.SelectableWeaponSets.Clone();
            if (withAttributes)
                to.Attributes = from.Attributes.Clone();
            if (withSkills)
                to.Skills = from.Skills.Clone();
            if (withSkillUsages)
                to.SkillUsages = from.SkillUsages.Clone();
            if (withBuffs)
                to.Buffs = from.Buffs.Clone();
            if (withEquipItems)
                to.EquipItems = from.EquipItems.Clone();
            if (withNonEquipItems)
                to.NonEquipItems = from.NonEquipItems.Clone();
            if (withSummons)
                to.Summons = from.Summons.Clone();
            if (withHotkeys)
                to.Hotkeys = from.Hotkeys.Clone();
            if (withQuests)
                to.Quests = from.Quests.Clone();
            if (withCurrencies)
                to.Currencies = from.Currencies.Clone();
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
                character.Attributes.Add(CharacterAttribute.Create(attribute.DataId, 0));
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
                character.Skills.Add(CharacterSkill.Create(skill.DataId, 0));
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

        public static T SetNewPlayerCharacterData<T>(this T character, string characterName, int dataId, int entityId, int factionId) where T : IPlayerCharacterData
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
                character.Attributes.Add(CharacterAttribute.Create(attribute.DataId, 0));
            }
            foreach (BaseSkill skill in playerCharacter.CacheSkillLevels.Keys)
            {
                character.Skills.Add(CharacterSkill.Create(skill.DataId, 0));
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
                int amount = startItem.amount;
                while (amount > 0)
                {
                    int addAmount = amount;
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
            character.FactionId = factionId;
            // Start Map
            BaseMapInfo startMap;
            Vector3 startPosition;
            Vector3 startRotation;
            playerCharacter.GetStartMapAndTransform(character, out startMap, out startPosition, out startRotation);
            character.CurrentMapName = startMap.Id;
            character.CurrentPositionX = startPosition.x;
            character.CurrentPositionY = startPosition.y;
            character.CurrentPositionZ = startPosition.z;
            character.CurrentRotationX = startRotation.x;
            character.CurrentRotationY = startRotation.y;
            character.CurrentRotationZ = startRotation.z;
            character.RespawnMapName = startMap.Id;
            character.RespawnPositionX = startPosition.x;
            character.RespawnPositionY = startPosition.y;
            character.RespawnPositionZ = startPosition.z;
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
            writer.PutPackedInt(characterData.Level);
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
                writer.Put(characterData.CurrentPositionX);
                writer.Put(characterData.CurrentPositionY);
                writer.Put(characterData.CurrentPositionZ);
                writer.Put(characterData.CurrentRotationX);
                writer.Put(characterData.CurrentRotationY);
                writer.Put(characterData.CurrentRotationZ);
            }
            writer.Put(characterData.RespawnMapName);
            if (withTransforms)
            {
                writer.Put(characterData.RespawnPositionX);
                writer.Put(characterData.RespawnPositionY);
                writer.Put(characterData.RespawnPositionZ);
            }
            writer.PutPackedInt(characterData.MountDataId);
            writer.PutPackedInt(characterData.IconDataId);
            writer.PutPackedInt(characterData.FrameDataId);
            writer.PutPackedInt(characterData.TitleDataId);
            writer.PutPackedLong(characterData.LastDeadTime);
            writer.PutPackedLong(characterData.UnmuteTime);
            writer.PutPackedLong(characterData.LastUpdate);
            // Attributes
            if (withAttributes)
            {
                writer.PutPackedInt(characterData.Attributes.Count);
                foreach (CharacterAttribute entry in characterData.Attributes)
                {
                    writer.Put(entry);
                }
            }
            // Buffs
            if (withBuffs)
            {
                writer.PutPackedInt(characterData.Buffs.Count);
                foreach (CharacterBuff entry in characterData.Buffs)
                {
                    writer.Put(entry);
                }
            }
            // Skills
            if (withSkills)
            {
                writer.PutPackedInt(characterData.Skills.Count);
                foreach (CharacterSkill entry in characterData.Skills)
                {
                    writer.Put(entry);
                }
            }
            // Skill Usages
            if (withSkillUsages)
            {
                writer.PutPackedInt(characterData.SkillUsages.Count);
                foreach (CharacterSkillUsage entry in characterData.SkillUsages)
                {
                    writer.Put(entry);
                }
            }
            // Summons
            if (withSummons)
            {
                writer.PutPackedInt(characterData.Summons.Count);
                foreach (CharacterSummon entry in characterData.Summons)
                {
                    writer.Put(entry);
                }
            }
            // Equip Items
            if (withEquipItems)
            {
                writer.PutPackedInt(characterData.EquipItems.Count);
                foreach (CharacterItem entry in characterData.EquipItems)
                {
                    writer.Put(entry);
                }
            }
            // Non Equip Items
            if (withNonEquipItems)
            {
                writer.PutPackedInt(characterData.NonEquipItems.Count);
                foreach (CharacterItem entry in characterData.NonEquipItems)
                {
                    writer.Put(entry);
                }
            }
            // Hotkeys
            if (withHotkeys)
            {
                writer.PutPackedInt(characterData.Hotkeys.Count);
                foreach (CharacterHotkey entry in characterData.Hotkeys)
                {
                    writer.Put(entry);
                }
            }
            // Quests
            if (withQuests)
            {
                writer.PutPackedInt(characterData.Quests.Count);
                foreach (CharacterQuest entry in characterData.Quests)
                {
                    writer.Put(entry);
                }
            }
            // Currencies
            if (withCurrencies)
            {
                writer.PutPackedInt(characterData.Currencies.Count);
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
                writer.PutPackedInt(characterData.SelectableWeaponSets.Count);
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
            characterData.Level = reader.GetPackedInt();
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
                characterData.CurrentPositionX = reader.GetFloat();
                characterData.CurrentPositionY = reader.GetFloat();
                characterData.CurrentPositionZ = reader.GetFloat();
                characterData.CurrentRotationX = reader.GetFloat();
                characterData.CurrentRotationY = reader.GetFloat();
                characterData.CurrentRotationZ = reader.GetFloat();
            }
            characterData.RespawnMapName = reader.GetString();
            if (withTransforms)
            {
                characterData.RespawnPositionX = reader.GetFloat();
                characterData.RespawnPositionY = reader.GetFloat();
                characterData.RespawnPositionZ = reader.GetFloat();
            }
            characterData.MountDataId = reader.GetPackedInt();
            characterData.IconDataId = reader.GetPackedInt();
            characterData.FrameDataId = reader.GetPackedInt();
            characterData.TitleDataId = reader.GetPackedInt();
            characterData.LastDeadTime = reader.GetPackedLong();
            characterData.UnmuteTime = reader.GetPackedLong();
            characterData.LastUpdate = reader.GetPackedLong();
            int count;
            // Attributes
            if (withAttributes)
            {
                count = reader.GetPackedInt();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Attributes.Add(reader.Get<CharacterAttribute>());
                }
            }
            // Buffs
            if (withBuffs)
            {
                count = reader.GetPackedInt();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Buffs.Add(reader.Get<CharacterBuff>());
                }
            }
            // Skills
            if (withSkills)
            {
                count = reader.GetPackedInt();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Skills.Add(reader.Get<CharacterSkill>());
                }
            }
            // Skill Usages
            if (withSkillUsages)
            {
                count = reader.GetPackedInt();
                for (int i = 0; i < count; ++i)
                {
                    characterData.SkillUsages.Add(reader.Get<CharacterSkillUsage>());
                }
            }
            // Summons
            if (withSummons)
            {
                count = reader.GetPackedInt();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Summons.Add(reader.Get<CharacterSummon>());
                }
            }
            // Equip Items
            if (withEquipItems)
            {
                count = reader.GetPackedInt();
                for (int i = 0; i < count; ++i)
                {
                    characterData.EquipItems.Add(reader.Get<CharacterItem>());
                }
            }
            // Non Equip Items
            if (withNonEquipItems)
            {
                count = reader.GetPackedInt();
                for (int i = 0; i < count; ++i)
                {
                    characterData.NonEquipItems.Add(reader.Get<CharacterItem>());
                }
            }
            // Hotkeys
            if (withHotkeys)
            {
                count = reader.GetPackedInt();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Hotkeys.Add(reader.Get<CharacterHotkey>());
                }
            }
            // Quests
            if (withQuests)
            {
                count = reader.GetPackedInt();
                for (int i = 0; i < count; ++i)
                {
                    characterData.Quests.Add(reader.Get<CharacterQuest>());
                }
            }
            // Currencies
            if (withCurrencies)
            {
                count = reader.GetPackedInt();
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
                count = reader.GetPackedInt();
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

        public static bool AddAttribute(this IPlayerCharacterData characterData, out UITextKeys gameMessage, int dataId, int amount = 1, int itemIndex = -1)
        {
            if (!GameInstance.Attributes.TryGetValue(dataId, out Attribute attribute))
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ATTRIBUTE_DATA;
                return false;
            }

            CharacterAttribute characterAtttribute;
            int index = characterData.IndexOfAttribute(dataId);
            if (index < 0)
            {
                characterAtttribute = CharacterAttribute.Create(attribute, 0);
                if (!attribute.CanIncreaseAmount(characterData, characterAtttribute.amount + amount - 1, out gameMessage, itemIndex < 0))
                    return false;
                if (itemIndex >= 0)
                {
                    if (characterData.DecreaseItemsByIndex(itemIndex, 1, false))
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
                if (!attribute.CanIncreaseAmount(characterData, characterAtttribute.amount + amount - 1, out gameMessage, itemIndex < 0))
                    return false;
                if (itemIndex >= 0)
                {
                    if (characterData.DecreaseItemsByIndex(itemIndex, 1, false))
                        characterData.FillEmptySlots();
                    else
                        return false;
                }
                characterAtttribute.amount += amount;
                characterData.Attributes[index] = characterAtttribute;
            }
            return true;
        }

        public static bool ResetAttributes(this IPlayerCharacterData characterData, int itemIndex = -1)
        {
            if (itemIndex >= 0)
            {
                if (characterData.DecreaseItemsByIndex(itemIndex, 1, false))
                    characterData.FillEmptySlots();
                else
                    return false;
            }

            int countStatPoint = 0;
            Attribute attribute;
            CharacterAttribute characterAttribute;
            for (int i = characterData.Attributes.Count - 1; i >= 0 ; --i)
            {
                characterAttribute = characterData.Attributes[i];
                attribute = characterAttribute.GetAttribute();
                if (attribute.cannotReset)
                    continue;
                countStatPoint += characterAttribute.amount;
                characterData.Attributes.RemoveAt(i);
            }
            characterData.StatPoint += countStatPoint;
            return true;
        }

        public static bool AddSkill(this IPlayerCharacterData characterData, out UITextKeys gameMessageType, int dataId, int level = 1, int itemIndex = -1)
        {
            if (!GameInstance.Skills.TryGetValue(dataId, out BaseSkill skill))
            {
                gameMessageType = UITextKeys.UI_ERROR_INVALID_SKILL_DATA;
                return false;
            }

            CharacterSkill characterSkill;
            int index = characterData.IndexOfSkill(dataId);
            if (index < 0)
            {
                characterSkill = CharacterSkill.Create(skill, 0);
                if (!skill.CanLevelUp(characterData, (characterSkill.level + level - 1), out gameMessageType, itemIndex < 0, itemIndex < 0))
                    return false;
                if (itemIndex >= 0)
                {
                    if (characterData.DecreaseItemsByIndex(itemIndex, 1, false))
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
                if (!skill.CanLevelUp(characterData, (characterSkill.level + level - 1), out gameMessageType, itemIndex < 0, itemIndex < 0))
                    return false;
                if (itemIndex >= 0)
                {
                    if (characterData.DecreaseItemsByIndex(itemIndex, 1, false))
                        characterData.FillEmptySlots();
                    else
                        return false;
                }
                characterSkill.level += level;
                characterData.Skills[index] = characterSkill;
            }
            return true;
        }

        public static bool ResetSkills(this IPlayerCharacterData characterData, int itemIndex = -1)
        {
            if (itemIndex >= 0)
            {
                if (characterData.DecreaseItemsByIndex(itemIndex, 1, false))
                    characterData.FillEmptySlots();
                else
                    return false;
            }

            int countSkillPoint = 0;
            BaseSkill skill;
            CharacterSkill characterSkill;
            for (int i = characterData.Skills.Count - 1; i >= 0 ; --i)
            {
                characterSkill = characterData.Skills[i];
                skill = characterSkill.GetSkill();
                if (skill.cannotReset)
                    continue;
                for (int j = 0; j < characterSkill.level; ++j)
                {
                    countSkillPoint += Mathf.CeilToInt(skill.GetRequireCharacterSkillPoint(j));
                }
                characterData.Skills.RemoveAt(i);
            }
            characterData.SkillPoint += countSkillPoint;
            return true;
        }

        public static Dictionary<Currency, int> GetCurrencies(this IPlayerCharacterData data)
        {
            if (data == null)
                return new Dictionary<Currency, int>();
            Dictionary<Currency, int> result = new Dictionary<Currency, int>();
            foreach (CharacterCurrency characterCurrency in data.Currencies)
            {
                Currency key = characterCurrency.GetCurrency();
                int value = characterCurrency.amount;
                if (key == null)
                    continue;
                if (!result.ContainsKey(key))
                    result[key] = value;
                else
                    result[key] += value;
            }

            return result;
        }

        public static void IncreaseCurrencies(this IPlayerCharacterData character, Dictionary<Currency, int> currencyAmounts, float multiplier = 1)
        {
            if (currencyAmounts == null)
                return;
            foreach (KeyValuePair<Currency, int> currencyAmount in currencyAmounts)
            {
                character.IncreaseCurrency(currencyAmount.Key, Mathf.CeilToInt(currencyAmount.Value * multiplier));
            }
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

        public static void DecreaseCurrencies(this IPlayerCharacterData character, Dictionary<Currency, int> currencyAmounts, float multiplier = 1)
        {
            if (currencyAmounts == null)
                return;
            foreach (KeyValuePair<Currency, int> currencyAmount in currencyAmounts)
            {
                character.DecreaseCurrency(currencyAmount.Key, Mathf.CeilToInt(currencyAmount.Value * multiplier));
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

        public static void DecreaseCurrencies(this IPlayerCharacterData character, IEnumerable<CharacterCurrency> currencies, float multiplier = 1)
        {
            if (currencies == null)
                return;
            foreach (CharacterCurrency currency in currencies)
            {
                character.DecreaseCurrency(currency.GetCurrency(), Mathf.CeilToInt(currency.amount * multiplier));
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

        public static bool HasEnoughCurrencyAmounts(this IPlayerCharacterData data, Dictionary<Currency, int> requiredCurrencyAmounts, out UITextKeys gameMessage, out Dictionary<Currency, int> currentCurrencyAmounts, float multiplier = 1)
        {
            gameMessage = UITextKeys.NONE;
            currentCurrencyAmounts = data.GetCurrencies();
            foreach (Currency requireCurrency in requiredCurrencyAmounts.Keys)
            {
                if (!currentCurrencyAmounts.ContainsKey(requireCurrency) ||
                    currentCurrencyAmounts[requireCurrency] < Mathf.CeilToInt(requiredCurrencyAmounts[requireCurrency] * multiplier))
                {
                    gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_CURRENCY_AMOUNTS;
                    return false;
                }
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
