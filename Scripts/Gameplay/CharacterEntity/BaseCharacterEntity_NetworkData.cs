using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        #region Sync data
        [Header("Sync Fields")]
        [SerializeField]
        protected SyncFieldString id = new SyncFieldString();
        [SerializeField]
        protected SyncFieldShort level = new SyncFieldShort();
        [SerializeField]
        protected SyncFieldInt exp = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt currentMp = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt currentStamina = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt currentFood = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldInt currentWater = new SyncFieldInt();
        [SerializeField]
        protected SyncFieldByte equipWeaponSet = new SyncFieldByte();
        [SerializeField]
        protected SyncFieldByte pitch = new SyncFieldByte();
        [Header("Sync Lists")]
        [SerializeField]
        protected SyncListEquipWeapons selectableWeaponSets = new SyncListEquipWeapons();
        [SerializeField]
        protected SyncListCharacterAttribute attributes = new SyncListCharacterAttribute();
        [SerializeField]
        protected SyncListCharacterSkill skills = new SyncListCharacterSkill();
        [SerializeField]
        protected SyncListCharacterSkillUsage skillUsages = new SyncListCharacterSkillUsage();
        [SerializeField]
        protected SyncListCharacterBuff buffs = new SyncListCharacterBuff();
        [SerializeField]
        protected SyncListCharacterItem equipItems = new SyncListCharacterItem();
        [SerializeField]
        protected SyncListCharacterItem nonEquipItems = new SyncListCharacterItem();
        [SerializeField]
        protected SyncListCharacterSummon summons = new SyncListCharacterSummon();
        #endregion

        #region Sync data actions
        public System.Action<string> onIdChange;
        public System.Action<string> onCharacterNameChange;
        public System.Action<short> onLevelChange;
        public System.Action<int> onExpChange;
        public System.Action<int> onCurrentHpChange;
        public System.Action<int> onCurrentMpChange;
        public System.Action<int> onCurrentFoodChange;
        public System.Action<int> onCurrentWaterChange;
        public System.Action<byte> onEquipWeaponSetChange;
        public System.Action<byte> onPitchChange;
        public System.Action<bool> onIsHiddingChange;
        // List
        public System.Action<LiteNetLibSyncList.Operation, int> onSelectableWeaponSetsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onAttributesOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onSkillsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onSkillUsagesOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onBuffsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onEquipItemsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onNonEquipItemsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onSummonsOperation;
        #endregion

        #region Fields/Interface implementation
        public string Id { get { return id.Value; } set { id.Value = value; } }
        public int EntityId { get { return Identity.HashAssetId; } set { } }
        public string CharacterName { get { return syncTitle.Value; } set { syncTitle.Value = value; } }
        public short Level { get { return level.Value; } set { level.Value = value; } }
        public int Exp { get { return exp.Value; } set { exp.Value = value; } }
        public int CurrentMp { get { return currentMp.Value; } set { currentMp.Value = value; } }
        public int CurrentStamina { get { return currentStamina.Value; } set { currentStamina.Value = value; } }
        public int CurrentFood { get { return currentFood.Value; } set { currentFood.Value = value; } }
        public int CurrentWater { get { return currentWater.Value; } set { currentWater.Value = value; } }
        public EquipWeapons EquipWeapons
        {
            get
            {
                this.FillWeaponSetsIfNeeded(EquipWeaponSet);
                return SelectableWeaponSets[EquipWeaponSet];
            }
            set
            {
                this.FillWeaponSetsIfNeeded(EquipWeaponSet);
                SelectableWeaponSets[EquipWeaponSet] = value;
            }
        }
        public byte EquipWeaponSet { get { return equipWeaponSet.Value; } set { equipWeaponSet.Value = value; } }
        public float Pitch { get { return (float)pitch.Value * 0.01f * 360f; } set { pitch.Value = (byte)(value / 360f * 100); } }

        public IList<EquipWeapons> SelectableWeaponSets
        {
            get { return selectableWeaponSets; }
            set
            {
                selectableWeaponSets.Clear();
                foreach (EquipWeapons entry in value)
                    selectableWeaponSets.Add(entry);
            }
        }

        public IList<CharacterAttribute> Attributes
        {
            get { return attributes; }
            set
            {
                attributes.Clear();
                foreach (CharacterAttribute entry in value)
                    attributes.Add(entry);
            }
        }

        public IList<CharacterSkill> Skills
        {
            get { return skills; }
            set
            {
                skills.Clear();
                foreach (CharacterSkill entry in value)
                    skills.Add(entry);
            }
        }

        public IList<CharacterSkillUsage> SkillUsages
        {
            get { return skillUsages; }
            set
            {
                skillUsages.Clear();
                foreach (CharacterSkillUsage entry in value)
                    skillUsages.Add(entry);
            }
        }

        public IList<CharacterBuff> Buffs
        {
            get { return buffs; }
            set
            {
                buffs.Clear();
                foreach (CharacterBuff entry in value)
                    buffs.Add(entry);
            }
        }

        public IList<CharacterItem> EquipItems
        {
            get { return equipItems; }
            set
            {
                equipItemIndexes.Clear();
                equipItems.Clear();
                CharacterItem tempEquipItem;
                Item tempArmor;
                string tempEquipPosition;
                for (int i = 0; i < value.Count; ++i)
                {
                    tempEquipItem = value[i];
                    tempArmor = tempEquipItem.GetArmorItem();
                    if (tempEquipItem.IsEmptySlot() || tempArmor == null)
                        continue;

                    tempEquipPosition = GetEquipPosition(tempArmor.EquipPosition, tempEquipItem.equipSlotIndex);
                    if (equipItemIndexes.ContainsKey(tempEquipPosition))
                        continue;

                    equipItemIndexes[tempEquipPosition] = i;
                    equipItems.Add(tempEquipItem);
                }
            }
        }

        public IList<CharacterItem> NonEquipItems
        {
            get { return nonEquipItems; }
            set
            {
                nonEquipItems.Clear();
                foreach (CharacterItem entry in value)
                    nonEquipItems.Add(entry);
            }
        }

        public IList<CharacterSummon> Summons
        {
            get { return summons; }
            set
            {
                summons.Clear();
                foreach (CharacterSummon entry in value)
                    summons.Add(entry);
            }
        }
        #endregion

        #region Sync data changes callback
        /// <summary>
        /// Override this to do stuffs when id changes
        /// </summary>
        /// <param name="id"></param>
        protected virtual void OnIdChange(bool isInitial, string id)
        {
            if (onIdChange != null)
                onIdChange.Invoke(id);
        }

        /// <summary>
        /// Override this to do stuffs when character name changes
        /// </summary>
        /// <param name="characterName"></param>
        protected virtual void OnCharacterNameChange(bool isInitial, string characterName)
        {
            if (onCharacterNameChange != null)
                onCharacterNameChange.Invoke(characterName);
        }

        /// <summary>
        /// Override this to do stuffs when level changes
        /// </summary>
        /// <param name="level"></param>
        protected virtual void OnLevelChange(bool isInitial, short level)
        {
            isRecaching = true;

            if (onLevelChange != null)
                onLevelChange.Invoke(level);
        }

        /// <summary>
        /// Override this to do stuffs when exp changes
        /// </summary>
        /// <param name="exp"></param>
        protected virtual void OnExpChange(bool isInitial, int exp)
        {
            if (onExpChange != null)
                onExpChange.Invoke(exp);
        }

        /// <summary>
        /// Override this to do stuffs when current hp changes
        /// </summary>
        /// <param name="currentHp"></param>
        protected virtual void OnCurrentHpChange(bool isInitial, int currentHp)
        {
            if (onCurrentHpChange != null)
                onCurrentHpChange.Invoke(currentHp);
        }

        /// <summary>
        /// Override this to do stuffs when current mp changes
        /// </summary>
        /// <param name="currentMp"></param>
        protected virtual void OnCurrentMpChange(bool isInitial, int currentMp)
        {
            if (onCurrentMpChange != null)
                onCurrentMpChange.Invoke(currentMp);
        }

        /// <summary>
        /// Override this to do stuffs when current food changes
        /// </summary>
        /// <param name="currentFood"></param>
        protected virtual void OnCurrentFoodChange(bool isInitial, int currentFood)
        {
            if (onCurrentFoodChange != null)
                onCurrentFoodChange.Invoke(currentFood);
        }

        /// <summary>
        /// Override this to do stuffs when current water changes
        /// </summary>
        /// <param name="currentWater"></param>
        protected virtual void OnCurrentWaterChange(bool isInitial, int currentWater)
        {
            if (onCurrentWaterChange != null)
                onCurrentWaterChange.Invoke(currentWater);
        }

        /// <summary>
        /// Override this to do stuffs when equip weapon set changes
        /// </summary>
        /// <param name="equipWeaponSet"></param>
        protected virtual void OnEquipWeaponSetChange(bool isInitial, byte equipWeaponSet)
        {
            CharacterModel.SetEquipWeapons(EquipWeapons);
            if (FpsModel != null)
                FpsModel.SetEquipWeapons(EquipWeapons);

            if (onEquipWeaponSetChange != null)
                onEquipWeaponSetChange.Invoke(equipWeaponSet);
        }

        /// <summary>
        /// Override this to do stuffs when pitch changes
        /// </summary>
        /// <param name="pitch"></param>
        protected virtual void OnPitchChange(bool isInitial, byte pitch)
        {
            if (onPitchChange != null)
                onPitchChange.Invoke(pitch);
        }
        #endregion

        #region Net functions operation callback
        /// <summary>
        /// Override this to do stuffs when equip weapons changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnSelectableWeaponSetsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            selectableWeaponSetsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };

            CharacterModel.SetEquipWeapons(EquipWeapons);
            if (FpsModel != null)
                FpsModel.SetEquipWeapons(EquipWeapons);
        }

        /// <summary>
        /// Override this to do stuffs when attributes changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            attributesRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }

        /// <summary>
        /// Override this to do stuffs when skills changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            skillsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }

        /// <summary>
        /// Override this to do stuffs when skill usages changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnSkillUsagesOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (onSkillUsagesOperation != null)
                onSkillUsagesOperation.Invoke(operation, index);

            // Call update skill operations to update uis
            switch (operation)
            {
                case LiteNetLibSyncList.Operation.Add:
                case LiteNetLibSyncList.Operation.Insert:
                case LiteNetLibSyncList.Operation.Set:
                case LiteNetLibSyncList.Operation.Dirty:
                    int skillIndex = this.IndexOfSkill(SkillUsages[index].dataId);
                    if (skillIndex >= 0)
                    {
                        skillsRecachingState = new SyncListRecachingState()
                        {
                            isRecaching = true,
                            operation = operation,
                            index = skillIndex
                        };
                    }
                    break;
            }
        }

        /// <summary>
        /// Override this to do stuffs when buffs changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            CharacterModel.SetBuffs(buffs);
            if (FpsModel != null)
                FpsModel.SetBuffs(buffs);

            buffsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }

        /// <summary>
        /// Override this to do stuffs when equip items changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            CharacterModel.SetEquipItems(equipItems);
            if (FpsModel != null)
                FpsModel.SetEquipItems(equipItems);

            equipItemsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }

        /// <summary>
        /// Override this to do stuffs when non equip items changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            nonEquipItemsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }

        /// <summary>
        /// Override this to do stuffs when summons changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnSummonsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            summonsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }
        #endregion
    }
}
