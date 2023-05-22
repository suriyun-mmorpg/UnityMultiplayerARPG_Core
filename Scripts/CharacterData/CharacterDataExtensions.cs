using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static partial class CharacterDataExtensions
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
                Logging.LogWarning($"[GetDatabase] Cannot find character database with id: {data.DataId}");
                return null;
            }

            return database;
        }

        public static BaseCharacterEntity GetEntityPrefab(this ICharacterData data)
        {
            BaseCharacterEntity entityPrefab;
            if (!GameInstance.CharacterEntities.TryGetValue(data.EntityId, out entityPrefab))
            {
                Logging.LogWarning($"[GetEntityPrefab] Cannot find character entity with id: {data.EntityId}");
                return null;
            }
            return entityPrefab;
        }

        public static BaseCharacterModel InstantiateModel(this ICharacterData data, Transform parent)
        {
            BaseCharacterEntity entityPrefab = data.GetEntityPrefab();
            if (entityPrefab == null)
            {
                Logging.LogWarning($"[InstantiateModel] Cannot find character entity with id: {data.EntityId}");
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
            result.gameObject.SetLayerRecursively(GameInstance.Singleton.playerLayer, true);
            result.gameObject.SetActive(true);
            result.transform.localPosition = Vector3.zero;
            return result.CharacterModel;
        }

        public static int GetNextLevelExp(this ICharacterData data)
        {
            int level = data.Level;
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

        public static int GetTotalItemSlot(this IList<CharacterItem> itemList)
        {
            int result = 0;
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
                int value = characterAttribute.amount;
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
                result = GameDataHelpers.CombineAttributes(result, equipItem.GetBuff().GetIncreaseAttributes());
                result = GameDataHelpers.CombineAttributes(result, equipItem.GetSocketsIncreaseAttributes());
                // Increase by rate
                result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), equipItem.GetBuff().GetIncreaseAttributesRate()));
                result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), equipItem.GetSocketsIncreaseAttributesRate()));
            }
            // Increase attributes from right hand equipment
            if (data.EquipWeapons.NotEmptyRightHandSlot())
            {
                result = GameDataHelpers.CombineAttributes(result, data.EquipWeapons.rightHand.GetBuff().GetIncreaseAttributes());
                result = GameDataHelpers.CombineAttributes(result, data.EquipWeapons.rightHand.GetSocketsIncreaseAttributes());
                // Increase by rate
                result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.rightHand.GetBuff().GetIncreaseAttributesRate()));
                result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.rightHand.GetSocketsIncreaseAttributesRate()));
            }
            // Increase attributes from left hand equipment
            if (data.EquipWeapons.NotEmptyLeftHandSlot())
            {
                result = GameDataHelpers.CombineAttributes(result, data.EquipWeapons.leftHand.GetBuff().GetIncreaseAttributes());
                result = GameDataHelpers.CombineAttributes(result, data.EquipWeapons.leftHand.GetSocketsIncreaseAttributes());
                // Increase by rate
                result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.leftHand.GetBuff().GetIncreaseAttributesRate()));
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
                result = GameDataHelpers.CombineAttributes(result, buff.GetBuff().GetIncreaseAttributes());
                // Increase with rates
                result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), buff.GetBuff().GetIncreaseAttributesRate()));
            }
            foreach (CharacterSummon summon in data.Summons)
            {
                result = GameDataHelpers.CombineAttributes(result, summon.GetBuff().GetIncreaseAttributes());
                // Increase with rates
                result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), summon.GetBuff().GetIncreaseAttributesRate()));
            }
            if (data.PassengingVehicleEntity != null)
            {
                result = GameDataHelpers.CombineAttributes(result, data.PassengingVehicleEntity.GetBuff().GetIncreaseAttributes());
                // Increase with rates
                result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.PassengingVehicleEntity.GetBuff().GetIncreaseAttributesRate()));
            }
            if (GameInstance.PlayerTitles.TryGetValue(data.TitleDataId, out PlayerTitle title))
            {
                result = GameDataHelpers.CombineAttributes(result, title.Buff.GetIncreaseAttributes(1));
                // Increase with rates
                result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), title.Buff.GetIncreaseAttributesRate(1)));
            }
            return result;
        }

        public static Dictionary<Attribute, float> GetPassiveSkillAttributes(this ICharacterData data, Dictionary<Attribute, float> baseAttributes)
        {
            if (data == null)
                return new Dictionary<Attribute, float>();
            return data.GetSkills(true).GetPassiveSkillAttributes(baseAttributes);
        }

        public static Dictionary<Attribute, float> GetPassiveSkillAttributes(this Dictionary<BaseSkill, int> skills, Dictionary<Attribute, float> baseAttributes)
        {
            if (skills == null)
                return new Dictionary<Attribute, float>();
            Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
            foreach (KeyValuePair<BaseSkill, int> skill in skills)
            {
                if (skill.Key == null || !skill.Key.IsPassive || skill.Value <= 0)
                    continue;
                result = GameDataHelpers.CombineAttributes(result, skill.Key.Buff.GetIncreaseAttributes(skill.Value));
                // Increase with rates
                result = GameDataHelpers.CombineAttributes(result, GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), skill.Key.Buff.GetIncreaseAttributesRate(skill.Value)));
            }
            return result;
        }

        public static Dictionary<Attribute, float> GetAttributes(this ICharacterData data, bool sumWithEquipments, bool sumWithBuffs, Dictionary<BaseSkill, int> skills)
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

        public static Dictionary<BaseSkill, int> GetCharacterSkills(this ICharacterData data)
        {
            if (data == null || data.GetDatabase() == null)
                return new Dictionary<BaseSkill, int>();
            // Make dictionary of skills which set in `PlayerCharacter` or `MonsterCharacter`
            Dictionary<BaseSkill, int> result = new Dictionary<BaseSkill, int>(data.GetDatabase().CacheSkillLevels);
            // Combine with skills that character learnt
            BaseSkill learnedSkill;
            int learnedSkillLevel;
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

        public static Dictionary<BaseSkill, int> GetEquipmentSkills(this ICharacterData data)
        {
            if (data == null)
                return new Dictionary<BaseSkill, int>();
            Dictionary<BaseSkill, int> result = new Dictionary<BaseSkill, int>();
            // Armors
            foreach (CharacterItem equipItem in data.EquipItems)
            {
                if (equipItem.IsEmptySlot()) continue;
                result = GameDataHelpers.CombineSkills(result, equipItem.GetBuff().GetIncreaseSkills());
                result = GameDataHelpers.CombineSkills(result, equipItem.GetSocketsIncreaseSkills());
            }
            // Right hand equipment
            if (data.EquipWeapons.NotEmptyRightHandSlot())
            {
                result = GameDataHelpers.CombineSkills(result, data.EquipWeapons.rightHand.GetBuff().GetIncreaseSkills());
                result = GameDataHelpers.CombineSkills(result, data.EquipWeapons.rightHand.GetSocketsIncreaseSkills());
            }
            // Left hand equipment
            if (data.EquipWeapons.NotEmptyLeftHandSlot())
            {
                result = GameDataHelpers.CombineSkills(result, data.EquipWeapons.leftHand.GetBuff().GetIncreaseSkills());
                result = GameDataHelpers.CombineSkills(result, data.EquipWeapons.leftHand.GetSocketsIncreaseSkills());
            }
            return result;
        }

        public static Dictionary<BaseSkill, int> GetSkills(this ICharacterData data, bool sumWithEquipments)
        {
            Dictionary<BaseSkill, int> result = data.GetCharacterSkills();
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
                result = GameDataHelpers.CombineResistances(result, equipItem.GetBuff().GetIncreaseResistances());
                result = GameDataHelpers.CombineResistances(result, equipItem.GetSocketsIncreaseResistances());
            }
            // Right hand equipment
            if (data.EquipWeapons.NotEmptyRightHandSlot())
            {
                result = GameDataHelpers.CombineResistances(result, data.EquipWeapons.rightHand.GetBuff().GetIncreaseResistances());
                result = GameDataHelpers.CombineResistances(result, data.EquipWeapons.rightHand.GetSocketsIncreaseResistances());
            }
            // Left hand equipment
            if (data.EquipWeapons.NotEmptyLeftHandSlot())
            {
                result = GameDataHelpers.CombineResistances(result, data.EquipWeapons.leftHand.GetBuff().GetIncreaseResistances());
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
                result = GameDataHelpers.CombineResistances(result, buff.GetBuff().GetIncreaseResistances());
            }
            foreach (CharacterSummon summon in data.Summons)
            {
                result = GameDataHelpers.CombineResistances(result, summon.GetBuff().GetIncreaseResistances());
            }
            if (data.PassengingVehicleEntity != null)
            {
                result = GameDataHelpers.CombineResistances(result, data.PassengingVehicleEntity.GetBuff().GetIncreaseResistances());
            }
            if (GameInstance.PlayerTitles.TryGetValue(data.TitleDataId, out PlayerTitle title))
            {
                result = GameDataHelpers.CombineResistances(result, title.Buff.GetIncreaseResistances(1));
            }
            return result;
        }

        public static Dictionary<DamageElement, float> GetPassiveSkillResistances(this ICharacterData data)
        {
            if (data == null)
                return new Dictionary<DamageElement, float>();
            return data.GetSkills(true).GetPassiveSkillResistances();
        }

        public static Dictionary<DamageElement, float> GetPassiveSkillResistances(this Dictionary<BaseSkill, int> skills)
        {
            if (skills == null)
                return new Dictionary<DamageElement, float>();
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            foreach (KeyValuePair<BaseSkill, int> skill in skills)
            {
                if (skill.Key == null || !skill.Key.IsPassive || skill.Value <= 0)
                    continue;
                result = GameDataHelpers.CombineResistances(result, skill.Key.Buff.GetIncreaseResistances(skill.Value));
            }
            return result;
        }

        public static Dictionary<DamageElement, float> GetAttributeResistances(this Dictionary<Attribute, float> attributes)
        {
            if (attributes == null)
                return new Dictionary<DamageElement, float>();
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            foreach (KeyValuePair<Attribute, float> attribute in attributes)
            {
                if (attribute.Key == null || attribute.Value <= 0)
                    continue;
                result = GameDataHelpers.CombineResistances(result, attribute.Key.GetIncreaseResistances(attribute.Value));
            }
            return result;
        }

        public static Dictionary<DamageElement, float> GetResistances(this ICharacterData data, bool sumWithEquipments, bool sumWithBuffs, Dictionary<Attribute, float> attributes, Dictionary<BaseSkill, int> skills)
        {
            Dictionary<DamageElement, float> result = data.GetCharacterResistances();
            if (sumWithEquipments)
                result = GameDataHelpers.CombineResistances(result, data.GetEquipmentResistances());
            if (sumWithBuffs)
                result = GameDataHelpers.CombineResistances(result, data.GetBuffResistances());
            if (attributes != null)
                result = GameDataHelpers.CombineResistances(result, attributes.GetAttributeResistances());
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
                result = GameDataHelpers.CombineArmors(result, equipItem.GetBuff().GetIncreaseArmors());
                result = GameDataHelpers.CombineArmors(result, equipItem.GetSocketsIncreaseArmors());
            }
            // Right hand equipment
            if (data.EquipWeapons.NotEmptyRightHandSlot())
            {
                if (data.EquipWeapons.rightHand.GetDefendItem() != null)
                    result = GameDataHelpers.CombineArmors(result, data.EquipWeapons.rightHand.GetArmorAmount());
                result = GameDataHelpers.CombineArmors(result, data.EquipWeapons.rightHand.GetBuff().GetIncreaseArmors());
                result = GameDataHelpers.CombineArmors(result, data.EquipWeapons.rightHand.GetSocketsIncreaseArmors());
            }
            // Left hand equipment
            if (data.EquipWeapons.NotEmptyLeftHandSlot())
            {
                if (data.EquipWeapons.leftHand.GetDefendItem() != null)
                    result = GameDataHelpers.CombineArmors(result, data.EquipWeapons.leftHand.GetArmorAmount());
                result = GameDataHelpers.CombineArmors(result, data.EquipWeapons.leftHand.GetBuff().GetIncreaseArmors());
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
                result = GameDataHelpers.CombineArmors(result, buff.GetBuff().GetIncreaseArmors());
            }
            foreach (CharacterSummon summon in data.Summons)
            {
                result = GameDataHelpers.CombineArmors(result, summon.GetBuff().GetIncreaseArmors());
            }
            if (data.PassengingVehicleEntity != null)
            {
                result = GameDataHelpers.CombineArmors(result, data.PassengingVehicleEntity.GetBuff().GetIncreaseArmors());
            }
            if (GameInstance.PlayerTitles.TryGetValue(data.TitleDataId, out PlayerTitle title))
            {
                result = GameDataHelpers.CombineArmors(result, title.Buff.GetIncreaseArmors(1));
            }
            return result;
        }

        public static Dictionary<DamageElement, float> GetPassiveSkillArmors(this ICharacterData data)
        {
            if (data == null)
                return new Dictionary<DamageElement, float>();
            return data.GetPassiveSkillArmors();
        }

        public static Dictionary<DamageElement, float> GetPassiveSkillArmors(this Dictionary<BaseSkill, int> skills)
        {
            if (skills == null)
                return new Dictionary<DamageElement, float>();
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            foreach (KeyValuePair<BaseSkill, int> skill in skills)
            {
                if (skill.Key == null || !skill.Key.IsPassive || skill.Value <= 0)
                    continue;
                result = GameDataHelpers.CombineArmors(result, skill.Key.Buff.GetIncreaseArmors(skill.Value));
            }
            return result;
        }

        public static Dictionary<DamageElement, float> GetAttributeArmors(this Dictionary<Attribute, float> attributes)
        {
            if (attributes == null)
                return new Dictionary<DamageElement, float>();
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            foreach (KeyValuePair<Attribute, float> attribute in attributes)
            {
                if (attribute.Key == null || attribute.Value <= 0)
                    continue;
                result = GameDataHelpers.CombineArmors(result, attribute.Key.GetIncreaseArmors(attribute.Value));
            }
            return result;
        }

        public static Dictionary<DamageElement, float> GetArmors(this ICharacterData data, bool sumWithEquipments, bool sumWithBuffs, Dictionary<Attribute, float> attributes, Dictionary<BaseSkill, int> skills)
        {
            Dictionary<DamageElement, float> result = data.GetCharacterArmors();
            if (sumWithEquipments)
                result = GameDataHelpers.CombineArmors(result, data.GetEquipmentArmors());
            if (sumWithBuffs)
                result = GameDataHelpers.CombineArmors(result, data.GetBuffArmors());
            if (attributes != null)
                result = GameDataHelpers.CombineArmors(result, attributes.GetAttributeArmors());
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
                result = GameDataHelpers.CombineDamages(result, equipItem.GetBuff().GetIncreaseDamages());
                result = GameDataHelpers.CombineDamages(result, equipItem.GetSocketsIncreaseDamages());
            }
            // Right hand equipment
            if (data.EquipWeapons.NotEmptyRightHandSlot())
            {
                result = GameDataHelpers.CombineDamages(result, data.EquipWeapons.rightHand.GetBuff().GetIncreaseDamages());
                result = GameDataHelpers.CombineDamages(result, data.EquipWeapons.rightHand.GetSocketsIncreaseDamages());
            }
            // Left hand equipment
            if (data.EquipWeapons.NotEmptyLeftHandSlot())
            {
                result = GameDataHelpers.CombineDamages(result, data.EquipWeapons.leftHand.GetBuff().GetIncreaseDamages());
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
                result = GameDataHelpers.CombineDamages(result, buff.GetBuff().GetIncreaseDamages());
            }
            foreach (CharacterSummon summon in data.Summons)
            {
                result = GameDataHelpers.CombineDamages(result, summon.GetBuff().GetIncreaseDamages());
            }
            if (data.PassengingVehicleEntity != null)
            {
                result = GameDataHelpers.CombineDamages(result, data.PassengingVehicleEntity.GetBuff().GetIncreaseDamages());
            }
            if (GameInstance.PlayerTitles.TryGetValue(data.TitleDataId, out PlayerTitle title))
            {
                result = GameDataHelpers.CombineDamages(result, title.Buff.GetIncreaseDamages(1));
            }
            return result;
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetPassiveSkillIncreaseDamages(this ICharacterData data)
        {
            if (data == null)
                return new Dictionary<DamageElement, MinMaxFloat>();
            return data.GetSkills(true).GetPassiveSkillIncreaseDamages();
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetPassiveSkillIncreaseDamages(this Dictionary<BaseSkill, int> skills)
        {
            if (skills == null)
                return new Dictionary<DamageElement, MinMaxFloat>();
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            foreach (KeyValuePair<BaseSkill, int> skill in skills)
            {
                if (skill.Key == null || !skill.Key.IsPassive || skill.Value <= 0)
                    continue;
                result = GameDataHelpers.CombineDamages(result, skill.Key.Buff.GetIncreaseDamages(skill.Value));
            }
            return result;
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetAttributeIncreaseDamages(this Dictionary<Attribute, float> attributes)
        {
            if (attributes == null)
                return new Dictionary<DamageElement, MinMaxFloat>();
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            foreach (KeyValuePair<Attribute, float> attribute in attributes)
            {
                if (attribute.Key == null || attribute.Value <= 0)
                    continue;
                result = GameDataHelpers.CombineDamages(result, attribute.Key.GetIncreaseDamages(attribute.Value));
            }
            return result;
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this ICharacterData data, bool sumWithEquipments, bool sumWithBuffs, Dictionary<Attribute, float> attributes, Dictionary<BaseSkill, int> skills)
        {
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            if (sumWithEquipments)
                result = GameDataHelpers.CombineDamages(result, data.GetEquipmentIncreaseDamages());
            if (sumWithBuffs)
                result = GameDataHelpers.CombineDamages(result, data.GetBuffIncreaseDamages());
            if (attributes != null)
                result = GameDataHelpers.CombineDamages(result, attributes.GetAttributeIncreaseDamages());
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
                result += equipItem.GetBuff().GetIncreaseStats();
                result += equipItem.GetSocketsIncreaseStats();
                result += GameDataHelpers.GetStatsFromAttributes(equipItem.GetBuff().GetIncreaseAttributes());
                result += GameDataHelpers.GetStatsFromAttributes(equipItem.GetSocketsIncreaseAttributes());
                // Increase with rates
                result += baseStats * equipItem.GetBuff().GetIncreaseStatsRate();
                result += baseStats * equipItem.GetSocketsIncreaseStatsRate();
                result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), equipItem.GetBuff().GetIncreaseAttributesRate()));
                result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), equipItem.GetSocketsIncreaseAttributesRate()));
            }
            // Increase stats from right hand equipment
            if (data.EquipWeapons.NotEmptyRightHandSlot())
            {
                result += data.EquipWeapons.rightHand.GetBuff().GetIncreaseStats();
                result += data.EquipWeapons.rightHand.GetSocketsIncreaseStats();
                result += GameDataHelpers.GetStatsFromAttributes(data.EquipWeapons.rightHand.GetBuff().GetIncreaseAttributes());
                result += GameDataHelpers.GetStatsFromAttributes(data.EquipWeapons.rightHand.GetSocketsIncreaseAttributes());
                // Increase with rates
                result += baseStats * data.EquipWeapons.rightHand.GetBuff().GetIncreaseStatsRate();
                result += baseStats * data.EquipWeapons.rightHand.GetSocketsIncreaseStatsRate();
                result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.rightHand.GetBuff().GetIncreaseAttributesRate()));
                result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.rightHand.GetSocketsIncreaseAttributesRate()));
            }
            // Increase stats from left hand equipment
            if (data.EquipWeapons.NotEmptyLeftHandSlot())
            {
                result += data.EquipWeapons.leftHand.GetBuff().GetIncreaseStats();
                result += data.EquipWeapons.leftHand.GetSocketsIncreaseStats();
                result += GameDataHelpers.GetStatsFromAttributes(data.EquipWeapons.leftHand.GetBuff().GetIncreaseAttributes());
                result += GameDataHelpers.GetStatsFromAttributes(data.EquipWeapons.leftHand.GetSocketsIncreaseAttributes());
                // Increase with rates
                result += baseStats * data.EquipWeapons.leftHand.GetBuff().GetIncreaseStatsRate();
                result += baseStats * data.EquipWeapons.leftHand.GetSocketsIncreaseStatsRate();
                result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.EquipWeapons.leftHand.GetBuff().GetIncreaseAttributesRate()));
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
                result += buff.GetBuff().GetIncreaseStats();
                result += GameDataHelpers.GetStatsFromAttributes(buff.GetBuff().GetIncreaseAttributes());
                // Increase with rates
                result += baseStats * buff.GetBuff().GetIncreaseStatsRate();
                result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), buff.GetBuff().GetIncreaseAttributesRate()));
            }
            foreach (CharacterSummon summon in data.Summons)
            {
                result += summon.GetBuff().GetIncreaseStats();
                result += GameDataHelpers.GetStatsFromAttributes(summon.GetBuff().GetIncreaseAttributes());
                // Increase with rates
                result += baseStats * summon.GetBuff().GetIncreaseStatsRate();
                result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), summon.GetBuff().GetIncreaseAttributesRate()));
            }
            if (data.PassengingVehicleEntity != null)
            {
                result += data.PassengingVehicleEntity.GetBuff().GetIncreaseStats();
                result += GameDataHelpers.GetStatsFromAttributes(data.PassengingVehicleEntity.GetBuff().GetIncreaseAttributes());
                // Increase with rates
                result += baseStats * data.PassengingVehicleEntity.GetBuff().GetIncreaseStatsRate();
                result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), data.PassengingVehicleEntity.GetBuff().GetIncreaseAttributesRate()));
            }
            if (GameInstance.PlayerTitles.TryGetValue(data.TitleDataId, out PlayerTitle title))
            {
                result += title.Buff.GetIncreaseStats(1);
                result += GameDataHelpers.GetStatsFromAttributes(title.Buff.GetIncreaseAttributes(1));
                // Increase with rates
                result += baseStats * title.Buff.GetIncreaseStatsRate(1);
                result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), title.Buff.GetIncreaseAttributesRate(1)));
            }
            return result;
        }
        public static CharacterStats GetPassiveSkillStats(this ICharacterData data, CharacterStats baseStats, Dictionary<Attribute, float> baseAttributes)
        {
            if (data == null)
                return new CharacterStats();
            return data.GetSkills(true).GetPassiveSkillStats(baseStats, baseAttributes);
        }

        public static CharacterStats GetPassiveSkillStats(this Dictionary<BaseSkill, int> skills, CharacterStats baseStats, Dictionary<Attribute, float> baseAttributes)
        {
            if (skills == null)
                return new CharacterStats();
            CharacterStats result = new CharacterStats();
            foreach (KeyValuePair<BaseSkill, int> skill in skills)
            {
                if (skill.Key == null || !skill.Key.IsPassive || skill.Value <= 0)
                    continue;
                result += skill.Key.Buff.GetIncreaseStats(skill.Value);
                result += GameDataHelpers.GetStatsFromAttributes(skill.Key.Buff.GetIncreaseAttributes(skill.Value));
                // Increase with rates
                result += baseStats * skill.Key.Buff.GetIncreaseStatsRate(skill.Value);
                result += GameDataHelpers.GetStatsFromAttributes(GameDataHelpers.MultiplyAttributes(new Dictionary<Attribute, float>(baseAttributes), skill.Key.Buff.GetIncreaseAttributesRate(skill.Value)));
            }
            return result;
        }

        public static CharacterStats GetStats(this ICharacterData data, bool sumWithEquipments, bool sumWithBuffs, Dictionary<BaseSkill, int> skills)
        {
            CharacterStats result = new CharacterStats();
            result += data.GetCharacterStats();
            if (sumWithEquipments || sumWithBuffs || skills != null)
            {
                // Prepare base stats and attributes, it will be multiplied with increase stats rate
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
        public static void FillEmptySlots(this IList<CharacterItem> itemList, bool isLimitSlot, int slotLimit)
        {
            int i;
            if (!isLimitSlot || GameInstance.Singleton.doNotFillEmptySlots)
            {
                // If it is not limit slots, don't fill it, and also remove empty slots
                for (i = itemList.Count - 1; i >= 0; --i)
                {
                    if (itemList[i].IsEmpty() || itemList[i].IsEmptySlot())
                        itemList.RemoveAt(i);
                }
                return;
            }

            // Place empty slots
            for (i = 0; i < itemList.Count; ++i)
            {
                if (itemList[i].IsEmpty())
                    itemList[i] = CharacterItem.CreateEmptySlot();
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
            if (data is IGameEntity gameEntity && !gameEntity.Entity.IsServer)
            {
                Logging.LogWarning("[FillWeaponSetsIfNeeded] Client can't fill weapon sets");
                return;
            }
            while (data.SelectableWeaponSets.Count <= equipWeaponSet)
            {
                data.SelectableWeaponSets.Add(new EquipWeapons());
            }
        }
        #endregion

        #region Increasing Items Will Overwhelming
        public static bool UnEquipItemWillOverwhelming(this ICharacterData data, int unEquipCount = 1)
        {
            if (!GameInstance.Singleton.IsLimitInventorySlot)
                return false;
            return data.GetCaches().TotalItemSlot + unEquipCount > data.GetCaches().LimitItemSlot;
        }

        public static bool IncreasingItemsWillOverwhelming(this IList<CharacterItem> itemList, int dataId, int amount, bool isLimitWeight, float weightLimit, float totalItemWeight, bool isLimitSlot, int slotLimit)
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

            int maxStack = itemData.MaxStack;
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
                        amount -= maxStack - tempItem.amount;
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

        public static bool IncreasingItemsWillOverwhelming(this IList<CharacterItem> itemList, IEnumerable<ItemAmount> increasingItems, bool isLimitWeight, float weightLimit, float totalItemWeight, bool isLimitSlot, int slotLimit)
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

        public static bool IncreasingItemsWillOverwhelming(this IList<CharacterItem> itemList, IEnumerable<RewardedItem> increasingItems, bool isLimitWeight, float weightLimit, float totalItemWeight, bool isLimitSlot, int slotLimit)
        {
            if (itemList == null || increasingItems == null)
                return false;
            List<CharacterItem> simulatingItemList = new List<CharacterItem>(itemList);
            foreach (RewardedItem receiveItem in increasingItems)
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
                    simulatingItemList.AddOrSetItems(CharacterItem.Create(receiveItem.item, receiveItem.level, receiveItem.amount, receiveItem.randomSeed));
                }
            }
            return false;
        }

        public static bool IncreasingItemsWillOverwhelming(this IList<CharacterItem> itemList, IEnumerable<CharacterItem> increasingItems, bool isLimitWeight, float weightLimit, float totalItemWeight, bool isLimitSlot, int slotLimit)
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

        public static bool IncreasingItemsWillOverwhelming(this ICharacterData data, int dataId, int amount)
        {
            return data.NonEquipItems.IncreasingItemsWillOverwhelming(
                dataId,
                amount,
                GameInstance.Singleton.IsLimitInventoryWeight,
                data.GetCaches().LimitItemWeight,
                data.GetCaches().TotalItemWeight,
                GameInstance.Singleton.IsLimitInventorySlot,
                data.GetCaches().LimitItemSlot);
        }

        public static bool IncreasingItemsWillOverwhelming(this ICharacterData data, IEnumerable<ItemAmount> increasingItems)
        {
            return data.NonEquipItems.IncreasingItemsWillOverwhelming(
                increasingItems,
                GameInstance.Singleton.IsLimitInventoryWeight,
                data.GetCaches().LimitItemWeight,
                data.GetCaches().TotalItemWeight,
                GameInstance.Singleton.IsLimitInventorySlot,
                data.GetCaches().LimitItemSlot);
        }

        public static bool IncreasingItemsWillOverwhelming(this ICharacterData data, IEnumerable<RewardedItem> increasingItems)
        {
            return data.NonEquipItems.IncreasingItemsWillOverwhelming(
                increasingItems,
                GameInstance.Singleton.IsLimitInventoryWeight,
                data.GetCaches().LimitItemWeight,
                data.GetCaches().TotalItemWeight,
                GameInstance.Singleton.IsLimitInventorySlot,
                data.GetCaches().LimitItemSlot);
        }

        public static bool IncreasingItemsWillOverwhelming(this ICharacterData data, IEnumerable<CharacterItem> increasingItems)
        {
            return data.NonEquipItems.IncreasingItemsWillOverwhelming(
                increasingItems,
                GameInstance.Singleton.IsLimitInventoryWeight,
                data.GetCaches().LimitItemWeight,
                data.GetCaches().TotalItemWeight,
                GameInstance.Singleton.IsLimitInventorySlot,
                data.GetCaches().LimitItemSlot);
        }
        #endregion

        #region Increase Items
        public static bool IncreaseItems(this IList<CharacterItem> itemList, CharacterItem increasingItem)
        {
            // If item not valid
            if (increasingItem.IsEmptySlot()) return false;

            BaseItem itemData = increasingItem.GetItem();
            int amount = increasingItem.amount;

            int maxStack = itemData.MaxStack;
            Dictionary<int, CharacterItem> emptySlots = new Dictionary<int, CharacterItem>();
            Dictionary<int, CharacterItem> changes = new Dictionary<int, CharacterItem>();
            // Loop to all slots to add amount to any slots that item amount not max in stack
            CharacterItem item;
            for (int i = 0; i < itemList.Count; ++i)
            {
                item = itemList[i];
                if (item.IsEmptySlot())
                {
                    // If current entry is not valid, add it to empty list, going to replacing it later
                    emptySlots[i] = item;
                }
                else if (item.dataId == increasingItem.dataId)
                {
                    // If same item id, increase its amount
                    if (item.amount + amount <= maxStack)
                    {
                        item.amount += amount;
                        changes[i] = item;
                        amount = 0;
                        break;
                    }
                    else if (maxStack - item.amount >= 0)
                    {
                        amount -= maxStack - item.amount;
                        item.amount = maxStack;
                        changes[i] = item;
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
                    int addAmount = 0;
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
                int addAmount;
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

        public static void IncreaseItems(this IList<CharacterItem> itemList, IEnumerable<ItemAmount> increasingItems, System.Action<CharacterItem> onIncrease = null)
        {
            CharacterItem increasedItem;
            foreach (ItemAmount increasingItem in increasingItems)
            {
                if (increasingItem.item == null || increasingItem.amount <= 0) continue;
                increasedItem = CharacterItem.Create(increasingItem.item.DataId, 1, increasingItem.amount);
                itemList.IncreaseItems(increasedItem);
                if (onIncrease != null)
                    onIncrease.Invoke(increasedItem);
            }
        }

        public static void IncreaseItems(this IList<CharacterItem> itemList, IEnumerable<RewardedItem> increasingItems, System.Action<CharacterItem> onIncrease = null)
        {
            CharacterItem increasedItem;
            foreach (RewardedItem increasingItem in increasingItems)
            {
                if (increasingItem.item == null || increasingItem.amount <= 0) continue;
                increasedItem = CharacterItem.Create(increasingItem.item.DataId, increasingItem.level, increasingItem.amount, increasingItem.randomSeed);
                itemList.IncreaseItems(increasedItem);
                if (onIncrease != null)
                    onIncrease.Invoke(increasedItem);
            }
        }

        public static void IncreaseItems(this IList<CharacterItem> itemList, IEnumerable<CharacterItem> increasingItems, System.Action<CharacterItem> onIncrease = null)
        {
            CharacterItem increasedItem;
            foreach (CharacterItem increasingItem in increasingItems)
            {
                if (increasingItem.IsEmptySlot()) continue;
                increasedItem = increasingItem.Clone();
                itemList.IncreaseItems(increasedItem);
                if (onIncrease != null)
                    onIncrease.Invoke(increasedItem);
            }
        }

        public static bool IncreaseItems(this ICharacterData data, CharacterItem increasingItem, System.Action<CharacterItem> onIncrease = null)
        {
            if (data.NonEquipItems.IncreaseItems(increasingItem))
            {
                if (onIncrease != null)
                    onIncrease.Invoke(increasingItem);
                return true;
            }
            return false;
        }

        public static void IncreaseItems(this ICharacterData data, IEnumerable<ItemAmount> increasingItems, System.Action<CharacterItem> onIncrease = null)
        {
            CharacterItem increasedItem;
            foreach (ItemAmount increasingItem in increasingItems)
            {
                if (increasingItem.item == null || increasingItem.amount <= 0) continue;
                increasedItem = CharacterItem.Create(increasingItem.item.DataId, 1, increasingItem.amount);
                data.NonEquipItems.IncreaseItems(increasedItem);
                if (onIncrease != null)
                    onIncrease.Invoke(increasedItem);
            }
        }

        public static void IncreaseItems(this ICharacterData data, IEnumerable<RewardedItem> increasingItems, System.Action<CharacterItem> onIncrease = null)
        {
            CharacterItem increasedItem;
            foreach (RewardedItem increasingItem in increasingItems)
            {
                if (increasingItem.item == null || increasingItem.amount <= 0) continue;
                increasedItem = CharacterItem.Create(increasingItem.item.DataId, 1, increasingItem.amount, increasingItem.randomSeed);
                data.NonEquipItems.IncreaseItems(increasedItem);
                if (onIncrease != null)
                    onIncrease.Invoke(increasedItem);
            }
        }

        public static void IncreaseItems(this ICharacterData data, IEnumerable<CharacterItem> increasingItems, System.Action<CharacterItem> onIncrease = null)
        {
            foreach (CharacterItem increasingItem in increasingItems)
            {
                if (increasingItem.IsEmptySlot()) continue;
                data.NonEquipItems.IncreaseItems(increasingItem.Clone());
                if (onIncrease != null)
                    onIncrease.Invoke(increasingItem);
            }
        }
        #endregion

        #region Decrease Items
        public static bool DecreaseItems(this IList<CharacterItem> itemList, int dataId, int amount, bool isLimitInventorySlot, out Dictionary<int, int> decreaseItems)
        {
            decreaseItems = new Dictionary<int, int>();
            Dictionary<int, int> decreasingItemIndexes = new Dictionary<int, int>();
            int tempDecresingAmount;
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
            foreach (KeyValuePair<int, int> decreasingItem in decreasingItemIndexes)
            {
                decreaseItems.Add(decreasingItem.Key, decreasingItem.Value);
                itemList.DecreaseItemsByIndex(decreasingItem.Key, decreasingItem.Value, isLimitInventorySlot, true);
            }
            return true;
        }

        public static bool DecreaseItems(this ICharacterData data, int dataId, int amount, out Dictionary<int, int> decreaseItems)
        {
            if (data.NonEquipItems.DecreaseItems(dataId, amount, GameInstance.Singleton.IsLimitInventorySlot, out decreaseItems))
                return true;
            return false;
        }

        public static bool DecreaseItems(this ICharacterData data, int dataId, int amount)
        {
            return DecreaseItems(data, dataId, amount, out _);
        }

        public static void DecreaseItems(this ICharacterData character, Dictionary<BaseItem, int> itemAmounts, float multiplier = 1)
        {
            if (itemAmounts == null)
                return;
            foreach (KeyValuePair<BaseItem, int> itemAmount in itemAmounts)
            {
                character.DecreaseItems(itemAmount.Key.DataId, Mathf.CeilToInt(itemAmount.Value * multiplier), out _);
            }
        }

        public static void DecreaseItems(this ICharacterData character, IEnumerable<ItemAmount> itemAmounts, float multiplier = 1)
        {
            if (itemAmounts == null)
                return;
            foreach (ItemAmount itemAmount in itemAmounts)
            {
                character.DecreaseItems(itemAmount.item.DataId, Mathf.CeilToInt(itemAmount.amount * multiplier), out _);
            }
        }

        public static void DecreaseItems(this ICharacterData character, IEnumerable<CharacterItem> characterItems, float multiplier = 1)
        {
            if (characterItems == null)
                return;
            foreach (CharacterItem characterItem in characterItems)
            {
                character.DecreaseItems(characterItem.dataId, Mathf.CeilToInt(characterItem.amount * multiplier), out _);
            }
        }
        #endregion

        #region Ammo Functions
        public static bool DecreaseAmmos(this ICharacterData data, AmmoType ammoType, int amount, out Dictionary<DamageElement, MinMaxFloat> increaseDamages, out Dictionary<CharacterItem, int> decreaseItems)
        {
            increaseDamages = null;
            decreaseItems = new Dictionary<CharacterItem, int>();
            if (ammoType == null || amount <= 0)
                return false;
            Dictionary<int, int> decreasingItemIndexes = new Dictionary<int, int>();
            CharacterItem nonEquipItem;
            IAmmoItem ammoItemData;
            int tempDecresingAmount;
            for (int i = data.NonEquipItems.Count - 1; i >= 0; --i)
            {
                nonEquipItem = data.NonEquipItems[i];
                ammoItemData = nonEquipItem.GetAmmoItem();
                if (ammoItemData != null && ammoItemData.AmmoType == ammoType)
                {
                    if (increaseDamages == null)
                        increaseDamages = ammoItemData.GetIncreaseDamages(nonEquipItem.level);
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
            foreach (KeyValuePair<int, int> decreasingItem in decreasingItemIndexes)
            {
                decreaseItems.Add(data.NonEquipItems[decreasingItem.Key], decreasingItem.Value);
                DecreaseItemsByIndex(data, decreasingItem.Key, decreasingItem.Value, true);
            }
            return true;
        }

        public static bool DecreaseAmmos(this ICharacterData data, AmmoType ammoType, int amount, out Dictionary<DamageElement, MinMaxFloat> increaseDamages)
        {
            return DecreaseAmmos(data, ammoType, amount, out increaseDamages, out _);
        }
        #endregion

        #region Decrease Items By Index
        public static bool DecreaseItemsByIndex(this IList<CharacterItem> itemList, int index, int amount, bool isLimitInventorySlot, bool adjustMaxAmount)
        {
            if (index < 0 || index >= itemList.Count)
                return false;
            CharacterItem item = itemList[index];
            if (item.IsEmptySlot())
                return false;
            if (amount > item.amount)
            {
                if (!adjustMaxAmount)
                    return false;
                amount = item.amount;
            }
            if (item.amount - amount == 0)
            {
                if (isLimitInventorySlot)
                    itemList[index] = CharacterItem.Empty;
                else
                    itemList.RemoveAt(index);
            }
            else
            {
                item.amount -= amount;
                itemList[index] = item;
            }
            return true;
        }

        public static bool DecreaseItemsByIndex(this ICharacterData data, int index, int amount, bool adjustMaxAmount)
        {
            if (data.NonEquipItems.DecreaseItemsByIndex(index, amount, GameInstance.Singleton.IsLimitInventorySlot, adjustMaxAmount))
                return true;
            return false;
        }
        #endregion

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
                foreach (CharacterItem nonEquipItem in data.NonEquipItems)
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
            return data.GetCaches().GetAvailableWeapon(ref isLeftHand);
        }

        public static DamageInfo GetWeaponDamageInfo(this ICharacterData data, ref bool isLeftHand)
        {
            if (data is BaseMonsterCharacterEntity monsterCharacterEntity)
            {
                isLeftHand = false;
                return monsterCharacterEntity.CharacterDatabase.DamageInfo;
            }
            return data.GetAvailableWeapon(ref isLeftHand).GetWeaponItem().WeaponType.DamageInfo;
        }

        public static DamageInfo GetWeaponDamageInfo(this ICharacterData data, IWeaponItem weaponItem)
        {
            if (data is BaseMonsterCharacterEntity monsterCharacterEntity)
                return monsterCharacterEntity.CharacterDatabase.DamageInfo;
            return weaponItem.WeaponType.DamageInfo;
        }

        public static KeyValuePair<DamageElement, MinMaxFloat> GetWeaponDamages(this ICharacterData data, ref bool isLeftHand)
        {
            if (data is BaseMonsterCharacterEntity monsterCharacterEntity)
            {
                isLeftHand = false;
                return monsterCharacterEntity.CharacterDatabase.DamageAmount.ToKeyValuePair(monsterCharacterEntity.Level, 1f, 0f);
            }
            return data.GetAvailableWeapon(ref isLeftHand).GetDamageAmount(data);
        }

        public static KeyValuePair<DamageElement, MinMaxFloat> GetWeaponDamages(this ICharacterData data, CharacterItem weapon)
        {
            if (data is BaseMonsterCharacterEntity monsterCharacterEntity)
                return monsterCharacterEntity.CharacterDatabase.DamageAmount.ToKeyValuePair(monsterCharacterEntity.Level, 1f, 0f);
            return weapon.GetDamageAmount(data);
        }

        public static float GetMoveSpeedRateWhileReloading(this ICharacterData data, IWeaponItem weaponItem)
        {
            if (data is BaseMonsterCharacterEntity)
                return 1f;
            return weaponItem.MoveSpeedRateWhileReloading;
        }

        public static MovementRestriction GetMovementRestrictionWhileReloading(this ICharacterData data, IWeaponItem weaponItem)
        {
            if (data is BaseMonsterCharacterEntity)
                return MovementRestriction.None;
            return weaponItem.MovementRestrictionWhileReloading;
        }

        public static float GetMoveSpeedRateWhileCharging(this ICharacterData data, IWeaponItem weaponItem)
        {
            if (data is BaseMonsterCharacterEntity)
                return 1f;
            return weaponItem.MoveSpeedRateWhileCharging;
        }

        public static MovementRestriction GetMovementRestrictionWhileCharging(this ICharacterData data, IWeaponItem weaponItem)
        {
            if (data is BaseMonsterCharacterEntity)
                return MovementRestriction.None;
            return weaponItem.MovementRestrictionWhileCharging;
        }

        public static float GetMoveSpeedRateWhileAttacking(this ICharacterData data, IWeaponItem weaponItem)
        {
            if (data is BaseMonsterCharacterEntity monsterCharacterEntity)
                return monsterCharacterEntity.CharacterDatabase.MoveSpeedRateWhileAttacking;
            return weaponItem.MoveSpeedRateWhileAttacking;
        }

        public static MovementRestriction GetMovementRestrictionWhileAttacking(this ICharacterData data, IWeaponItem weaponItem)
        {
            if (data is BaseMonsterCharacterEntity)
                return MovementRestriction.None;
            return weaponItem.MovementRestrictionWhileAttacking;
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

        public static List<int> IndexesOfBuff(this ICharacterData data, int dataId, BuffType type)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < data.Buffs.Count; ++i)
            {
                if (data.Buffs[i].dataId == dataId && data.Buffs[i].type == type)
                    result.Add(i);
            }
            return result;
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
                    equipPosition.Equals(data.EquipItems[i].GetArmorItem().GetEquipPosition()))
                    return i;
            }
            return -1;
        }

        public static bool HasEnoughAttributeAmounts(this ICharacterData data, Dictionary<Attribute, float> requiredAttributeAmounts, bool sumWithEquipments, bool sumWithBuffs, bool sumWithSkills, out UITextKeys gameMessage, out Dictionary<Attribute, float> currentAttributeAmounts, float multiplier = 1)
        {
            Dictionary<BaseSkill, int> currentSkillLevels = sumWithSkills ? data.GetSkills(sumWithEquipments) : new Dictionary<BaseSkill, int>();
            return data.HasEnoughAttributeAmounts(requiredAttributeAmounts, sumWithEquipments, sumWithBuffs, currentSkillLevels, out gameMessage, out currentAttributeAmounts, multiplier);
        }

        public static bool HasEnoughAttributeAmounts(this ICharacterData data, Dictionary<Attribute, float> requiredAttributeAmounts, bool sumWithEquipments, bool sumWithBuffs, Dictionary<BaseSkill, int> currentSkillLevels, out UITextKeys gameMessage, out Dictionary<Attribute, float> currentAttributeAmounts, float multiplier = 1)
        {
            gameMessage = UITextKeys.NONE;
            currentAttributeAmounts = data.GetAttributes(sumWithEquipments, sumWithBuffs, currentSkillLevels);
            foreach (Attribute requireAttribute in requiredAttributeAmounts.Keys)
            {
                if (!currentAttributeAmounts.ContainsKey(requireAttribute) ||
                    currentAttributeAmounts[requireAttribute] < Mathf.CeilToInt(requiredAttributeAmounts[requireAttribute] * multiplier))
                {
                    gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ATTRIBUTE_AMOUNTS;
                    return false;
                }
            }
            return true;
        }

        public static bool HasEnoughSkillLevels(this ICharacterData data, Dictionary<BaseSkill, int> requiredSkillLevels, bool sumWithEquipments, out UITextKeys gameMessage, out Dictionary<BaseSkill, int> currentSkillLevels, float multiplier = 1)
        {
            gameMessage = UITextKeys.NONE;
            currentSkillLevels = data.GetSkills(sumWithEquipments);
            foreach (BaseSkill requireSkill in requiredSkillLevels.Keys)
            {
                if (!currentSkillLevels.ContainsKey(requireSkill) ||
                    currentSkillLevels[requireSkill] < Mathf.CeilToInt(requiredSkillLevels[requireSkill] * multiplier))
                {
                    gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_SKILL_LEVELS;
                    return false;
                }
            }
            return true;
        }

        public static Dictionary<BaseItem, int> GetNonEquipItems(this ICharacterData data)
        {
            if (data == null)
                return new Dictionary<BaseItem, int>();
            Dictionary<BaseItem, int> result = new Dictionary<BaseItem, int>();
            foreach (CharacterItem characterItem in data.NonEquipItems)
            {
                BaseItem key = characterItem.GetItem();
                int value = characterItem.amount;
                if (key == null)
                    continue;
                if (!result.ContainsKey(key))
                    result[key] = value;
                else
                    result[key] += value;
            }

            return result;
        }

        public static bool HasEnoughNonEquipItemAmounts(this ICharacterData data, Dictionary<BaseItem, int> requiredItemAmounts, out UITextKeys gameMessage, out Dictionary<BaseItem, int> currentItemAmounts, float multiplier = 1)
        {
            gameMessage = UITextKeys.NONE;
            currentItemAmounts = data.GetNonEquipItems();
            foreach (BaseItem requireItem in requiredItemAmounts.Keys)
            {
                if (!currentItemAmounts.ContainsKey(requireItem) ||
                    currentItemAmounts[requireItem] < Mathf.CeilToInt(requiredItemAmounts[requireItem] * multiplier))
                {
                    gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                    return false;
                }
            }
            return true;
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

        public static void AddOrSetNonEquipItems(this ICharacterData data, CharacterItem characterItem, int expectedIndex = -1)
        {
            data.AddOrSetNonEquipItems(characterItem, out _, expectedIndex);
        }

        public static void AddOrSetNonEquipItems(this ICharacterData data, CharacterItem characterItem, out int index, int expectedIndex = -1)
        {
            data.NonEquipItems.AddOrSetItems(characterItem, out index, expectedIndex);
        }

        public static void AddOrSetItems(this IList<CharacterItem> itemList, CharacterItem characterItem, int expectedIndex = -1)
        {
            itemList.AddOrSetItems(characterItem, out _, expectedIndex);
        }

        public static void AddOrSetItems(this IList<CharacterItem> itemList, CharacterItem characterItem, out int index, int expectedIndex = -1)
        {
            index = expectedIndex;
            if (index < 0 || index >= itemList.Count || itemList[index].NotEmptySlot())
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
            Dictionary<BaseSkill, int> bonusSkills,
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
            Dictionary<BaseSkill, int> tempSkills;
            CharacterStats tempIncreaseStats;
            foreach (KeyValuePair<EquipmentSet, int> cacheEquipmentSet in equipmentSets)
            {
                EquipmentBonus[] effects = cacheEquipmentSet.Key.Effects;
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
                        tempSkills = GameDataHelpers.CombineSkills(effects[i].skills, null, 1f);
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
                        bonusSkills = GameDataHelpers.CombineSkills(bonusSkills, tempSkills);
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
            Dictionary<BaseSkill, int> resultSkills,
            Dictionary<EquipmentSet, int> resultEquipmentSets,
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
            // Prepare equipment set bonus
            data.GetEquipmentSetBonus(ref resultStats, resultAttributes, resultResistances, resultArmors, resultIncreaseDamages, resultSkills, resultEquipmentSets, true);
            // Validate max amount
            foreach (Attribute attribute in new List<Attribute>(resultAttributes.Keys))
            {
                if (attribute.maxAmount > 0 && resultAttributes[attribute] > attribute.maxAmount)
                    resultAttributes[attribute] = attribute.maxAmount;
            }
            resultResistances = GameDataHelpers.CombineResistances(resultResistances, data.GetResistances(true, true, resultAttributes, resultSkills));
            resultArmors = GameDataHelpers.CombineArmors(resultArmors, data.GetArmors(true, true, resultAttributes, resultSkills));
            resultIncreaseDamages = GameDataHelpers.CombineDamages(resultIncreaseDamages, data.GetIncreaseDamages(true, true, resultAttributes, resultSkills));
            resultStats = resultStats + data.GetStats(true, true, resultSkills);
        }

        public static void ApplyStatusEffect(this IEnumerable<StatusEffectApplying> statusEffects, int level, EntityInfo applier, CharacterItem weapon, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null || statusEffects == null)
                return;
            foreach (StatusEffectApplying effect in statusEffects)
            {
                if (effect.statusEffect == null) continue;
                target.ApplyBuff(effect.statusEffect.DataId, BuffType.StatusEffect, effect.buffLevel.GetAmount(level), applier, weapon);
            }
        }

        public static Dictionary<int, int> ToAttributeAmountDictionary(this IEnumerable<CharacterAttribute> list)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            foreach (CharacterAttribute entry in list)
            {
                result[entry.dataId] = entry.amount;
            }
            return result;
        }

        public static List<CharacterAttribute> ToCharacterAttributes(this Dictionary<int, int> dict)
        {
            List<CharacterAttribute> result = new List<CharacterAttribute>();
            foreach (KeyValuePair<int, int> entry in dict)
            {
                result.Add(CharacterAttribute.Create(entry.Key, entry.Value));
            }
            return result;
        }

        public static Dictionary<int, int> ToSkillLevelDictionary(this IEnumerable<CharacterSkill> list)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            foreach (CharacterSkill entry in list)
            {
                result[entry.dataId] = entry.level;
            }
            return result;
        }

        public static List<CharacterSkill> ToCharacterSkills(this Dictionary<int, int> dict)
        {
            List<CharacterSkill> result = new List<CharacterSkill>();
            foreach (KeyValuePair<int, int> entry in dict)
            {
                result.Add(CharacterSkill.Create(entry.Key, entry.Value));
            }
            return result;
        }

        public static bool ValidateSkillToUse(this BaseCharacterEntity character, int dataId, bool isLeftHand, uint targetObjectId, out BaseSkill skill, out int skillLevel, out UITextKeys gameMessage)
        {
            skillLevel = 0;
            gameMessage = UITextKeys.NONE;

            if (!GameInstance.Skills.TryGetValue(dataId, out skill) ||
                !character.GetCaches().Skills.TryGetValue(skill, out skillLevel) ||
                !skill.CanUse(character, skillLevel, isLeftHand, targetObjectId, out gameMessage))
            {
                return false;
            }

            return true;
        }

        public static bool ValidateSkillItemToUse(this BaseCharacterEntity character, int itemIndex, bool isLeftHand, uint targetObjectId, out ISkillItem skillItem, out BaseSkill skill, out int skillLevel, out UITextKeys gameMessage)
        {
            skillItem = null;
            skill = null;
            skillLevel = 0;

            if (!ValidateUsableItemToUse(character, itemIndex, out IUsableItem usableItem, out gameMessage))
            {
                return false;
            }

            skillItem = usableItem as ISkillItem;
            if (skillItem == null || skillItem.UsingSkill == null ||
                !skillItem.UsingSkill.CanUse(character, skillItem.UsingSkillLevel, isLeftHand, targetObjectId, out gameMessage, true))
            {
                return false;
            }
            skill = skillItem.UsingSkill;
            skillLevel = skillItem.UsingSkillLevel;

            return true;
        }

        public static bool ValidateUsableItemToUse(this BaseCharacterEntity character, int itemIndex, out IUsableItem usableItem, out UITextKeys gameMessage)
        {
            usableItem = null;
            gameMessage = UITextKeys.NONE;

            if (itemIndex < 0 || itemIndex >= character.NonEquipItems.Count)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                return false;
            }

            if (character.NonEquipItems[itemIndex].IsLocked())
            {
                gameMessage = UITextKeys.UI_ERROR_ITEM_IS_LOCKED;
                return false;
            }

            usableItem = character.NonEquipItems[itemIndex].GetUsableItem();
            if (usableItem == null)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
                return false;
            }

            if (character.IndexOfSkillUsage(character.NonEquipItems[itemIndex].dataId, SkillUsageType.UsableItem) >= 0)
            {
                gameMessage = UITextKeys.UI_ERROR_ITEM_IS_COOLING_DOWN;
                return false;
            }

            return true;
        }
    }
}
