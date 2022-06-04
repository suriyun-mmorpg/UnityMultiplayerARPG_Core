using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        #region Sync data
        [Category("Sync Fields")]
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
        [SerializeField]
        protected SyncFieldUInt targetEntityId = new SyncFieldUInt();

        [Category(101, "Sync Lists", false)]
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

        #region Fields/Interface implementation
        public string Id { get { return id.Value; } set { id.Value = value; } }
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
                if (EquipWeaponSet < SelectableWeaponSets.Count)
                    return SelectableWeaponSets[EquipWeaponSet];
                return new EquipWeapons();
            }
            set
            {
                this.FillWeaponSetsIfNeeded(EquipWeaponSet);
                SelectableWeaponSets[EquipWeaponSet] = value;
            }
        }
        public byte EquipWeaponSet { get { return equipWeaponSet.Value; } set { equipWeaponSet.Value = value; } }
        public float Pitch
        {
            get
            {
                return (float)pitch.Value * 0.01f * 360f;
            }
            set
            {
                pitch.Value = (byte)(value / 360f * 100);
            }
        }
        public AimPosition AimPosition { get; set; }

        public IList<EquipWeapons> SelectableWeaponSets
        {
            get { return selectableWeaponSets; }
            set
            {
                selectableWeaponSets.Clear();
                selectableWeaponSets.AddRange(value);
            }
        }

        public IList<CharacterAttribute> Attributes
        {
            get { return attributes; }
            set
            {
                attributes.Clear();
                attributes.AddRange(value);
            }
        }

        public IList<CharacterSkill> Skills
        {
            get { return skills; }
            set
            {
                skills.Clear();
                skills.AddRange(value);
            }
        }

        public IList<CharacterSkillUsage> SkillUsages
        {
            get { return skillUsages; }
            set
            {
                skillUsages.Clear();
                skillUsages.AddRange(value);
            }
        }

        public IList<CharacterBuff> Buffs
        {
            get { return buffs; }
            set
            {
                buffs.Clear();
                buffs.AddRange(value);
            }
        }

        public IList<CharacterItem> EquipItems
        {
            get { return equipItems; }
            set
            {
                // Validate items
                HashSet<string> equipPositions = new HashSet<string>();
                IArmorItem tempArmor;
                string tempEquipPosition;
                for (int i = value.Count - 1; i >= 0; --i)
                {
                    // Remove empty slot
                    if (value[i].IsEmptySlot())
                    {
                        value.RemoveAt(i);
                        continue;
                    }
                    // Remove non-armor item
                    tempArmor = value[i].GetArmorItem();
                    if (tempArmor == null)
                    {
                        value.RemoveAt(i);
                        continue;
                    }
                    tempEquipPosition = GetEquipPosition(tempArmor.GetEquipPosition(), value[i].equipSlotIndex);
                    if (equipPositions.Contains(tempEquipPosition))
                    {
                        value.RemoveAt(i);
                        continue;
                    }
                    equipPositions.Add(tempEquipPosition);
                }
                equipItems.Clear();
                equipItems.AddRange(value);
            }
        }

        public IList<CharacterItem> NonEquipItems
        {
            get { return nonEquipItems; }
            set
            {
                nonEquipItems.Clear();
                nonEquipItems.AddRange(value);
            }
        }

        public IList<CharacterSummon> Summons
        {
            get { return summons; }
            set
            {
                summons.Clear();
                summons.AddRange(value);
            }
        }
        #endregion

        #region Network setup functions
        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            id.deliveryMethod = DeliveryMethod.ReliableOrdered;
            id.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            level.deliveryMethod = DeliveryMethod.ReliableOrdered;
            level.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            exp.deliveryMethod = DeliveryMethod.ReliableOrdered;
            exp.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            isImmune.deliveryMethod = DeliveryMethod.ReliableOrdered;
            isImmune.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentHp.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currentHp.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentMp.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currentMp.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentFood.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currentFood.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentWater.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currentWater.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            equipWeaponSet.deliveryMethod = DeliveryMethod.ReliableOrdered;
            equipWeaponSet.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            pitch.deliveryMethod = DeliveryMethod.Sequenced;
            pitch.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            targetEntityId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            targetEntityId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;

            selectableWeaponSets.forOwnerOnly = false;
            attributes.forOwnerOnly = false;
            skills.forOwnerOnly = false;
            skillUsages.forOwnerOnly = true;
            buffs.forOwnerOnly = false;
            equipItems.forOwnerOnly = false;
            nonEquipItems.forOwnerOnly = true;
            summons.forOwnerOnly = true;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // On data changes events
            id.onChange += OnIdChange;
            syncTitle.onChange += OnCharacterNameChange;
            level.onChange += OnLevelChange;
            exp.onChange += OnExpChange;
            isImmune.onChange += OnIsImmuneChange;
            currentHp.onChange += OnCurrentHpChange;
            currentMp.onChange += OnCurrentMpChange;
            currentFood.onChange += OnCurrentFoodChange;
            currentWater.onChange += OnCurrentWaterChange;
            equipWeaponSet.onChange += OnEquipWeaponSetChange;
            pitch.onChange += OnPitchChange;
            targetEntityId.onChange += OnTargetEntityIdChange;
            // On list changes events
            selectableWeaponSets.onOperation += OnSelectableWeaponSetsOperation;
            attributes.onOperation += OnAttributesOperation;
            skills.onOperation += OnSkillsOperation;
            skillUsages.onOperation += OnSkillUsagesOperation;
            buffs.onOperation += OnBuffsOperation;
            equipItems.onOperation += OnEquipItemsOperation;
            nonEquipItems.onOperation += OnNonEquipItemsOperation;
            summons.onOperation += OnSummonsOperation;
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            // On data changes events
            id.onChange -= OnIdChange;
            syncTitle.onChange -= OnCharacterNameChange;
            level.onChange -= OnLevelChange;
            exp.onChange -= OnExpChange;
            isImmune.onChange -= OnIsImmuneChange;
            currentHp.onChange -= OnCurrentHpChange;
            currentMp.onChange -= OnCurrentMpChange;
            currentFood.onChange -= OnCurrentFoodChange;
            currentWater.onChange -= OnCurrentWaterChange;
            equipWeaponSet.onChange -= OnEquipWeaponSetChange;
            pitch.onChange -= OnPitchChange;
            targetEntityId.onChange -= OnTargetEntityIdChange;
            // On list changes events
            selectableWeaponSets.onOperation -= OnSelectableWeaponSetsOperation;
            attributes.onOperation -= OnAttributesOperation;
            skills.onOperation -= OnSkillsOperation;
            skillUsages.onOperation -= OnSkillUsagesOperation;
            buffs.onOperation -= OnBuffsOperation;
            equipItems.onOperation -= OnEquipItemsOperation;
            nonEquipItems.onOperation -= OnNonEquipItemsOperation;
            summons.onOperation -= OnSummonsOperation;

            if (UICharacterEntity != null)
                Destroy(UICharacterEntity.gameObject);
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
        /// Override this to do stuffs when is immune changes
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="isImmune"></param>
        protected virtual void OnIsImmuneChange(bool isInitial, bool isImmune)
        {
            if (onIsImmuneChange != null)
                onIsImmuneChange.Invoke(isImmune);
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
            SetEquipWeaponsModels();
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

        /// <summary>
        /// Override this to do stuffs when target entity id changes
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="targetEntityId"></param>
        protected virtual void OnTargetEntityIdChange(bool isInitial, uint targetEntityId)
        {
            if (onTargetEntityIdChange != null)
                onTargetEntityIdChange.Invoke(targetEntityId);
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
            SetEquipWeaponsModels();
            selectableWeaponSetsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
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
                case LiteNetLibSyncList.Operation.AddInitial:
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
            // Update model's buffs effects
            CharacterModel.SetBuffs(buffs);
            if (FpsModel)
                FpsModel.SetBuffs(buffs);

            switch (operation)
            {
                case LiteNetLibSyncList.Operation.Add:
                case LiteNetLibSyncList.Operation.AddInitial:
                case LiteNetLibSyncList.Operation.Insert:
                    // Check last buff to update disallow status
                    if (buffs.Count > 0 && buffs[buffs.Count - 1].GetBuff().disallowMove)
                        StopMove();
                    break;
            }

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
            SetEquipItemsModels();
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

        #region Other functions
        protected void SetEquipWeaponsModels()
        {
            CharacterModel.SetEquipWeapons(EquipWeapons);
            if (FpsModel)
                FpsModel.SetEquipWeapons(EquipWeapons);
        }

        protected void SetEquipItemsModels()
        {
            CharacterModel.SetEquipItems(EquipItems);
            if (FpsModel)
                FpsModel.SetEquipItems(EquipItems);
        }
        #endregion
    }
}
