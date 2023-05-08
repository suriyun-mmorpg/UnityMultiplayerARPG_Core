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
        protected SyncFieldInt level = new SyncFieldInt();
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
        protected SyncFieldBool isWeaponsSheathed = new SyncFieldBool();
        [SerializeField]
        protected SyncFieldUShort pitch = new SyncFieldUShort();
        [SerializeField]
        protected SyncFieldAimPosition aimPosition = new SyncFieldAimPosition();
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
        public int Level { get { return level.Value; } set { level.Value = value; } }
        public int Exp { get { return exp.Value; } set { exp.Value = value; } }
        public int CurrentMp { get { return currentMp.Value; } set { currentMp.Value = value; } }
        public int CurrentStamina { get { return currentStamina.Value; } set { currentStamina.Value = value; } }
        public int CurrentFood { get { return currentFood.Value; } set { currentFood.Value = value; } }
        public int CurrentWater { get { return currentWater.Value; } set { currentWater.Value = value; } }
        public virtual int IconDataId { get; set; }
        public virtual int FrameDataId { get; set; }
        public virtual int TitleDataId { get; set; }
        public byte EquipWeaponSet { get { return equipWeaponSet.Value; } set { equipWeaponSet.Value = value; } }
        public bool IsWeaponsSheathed { get { return isWeaponsSheathed.Value; } set { isWeaponsSheathed.Value = value; } }
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
        public float Pitch
        {
            get
            {
                return (float)pitch.Value * 0.0001f * 360f;
            }
            set
            {
                pitch.Value = (ushort)(value / 360f * 10000);
            }
        }
        public AimPosition AimPosition
        {
            get
            {
                return aimPosition.Value;
            }
            set
            {
                aimPosition.Value = value;
            }
        }

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
            isWeaponsSheathed.deliveryMethod = DeliveryMethod.ReliableOrdered;
            isWeaponsSheathed.syncMode = LiteNetLibSyncField.SyncMode.ClientMulticast;
            pitch.deliveryMethod = DeliveryMethod.Sequenced;
            pitch.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            aimPosition.deliveryMethod = DeliveryMethod.Sequenced;
            aimPosition.syncMode = LiteNetLibSyncField.SyncMode.ClientMulticast;
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
            // On data changed events
            id.onChange += OnIdChange;
            syncTitle.onChange += OnCharacterNameChange;
            level.onChange += OnLevelChange;
            exp.onChange += OnExpChange;
            isImmune.onChange += OnIsImmuneChange;
            currentMp.onChange += OnCurrentMpChange;
            currentFood.onChange += OnCurrentFoodChange;
            currentWater.onChange += OnCurrentWaterChange;
            equipWeaponSet.onChange += OnEquipWeaponSetChange;
            isWeaponsSheathed.onChange += OnIsWeaponsSheathedChange;
            pitch.onChange += OnPitchChange;
            aimPosition.onChange += OnAimPositionChange;
            targetEntityId.onChange += OnTargetEntityIdChange;
            // On list changed events
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
            // On data changed events
            id.onChange -= OnIdChange;
            syncTitle.onChange -= OnCharacterNameChange;
            level.onChange -= OnLevelChange;
            exp.onChange -= OnExpChange;
            isImmune.onChange -= OnIsImmuneChange;
            currentMp.onChange -= OnCurrentMpChange;
            currentFood.onChange -= OnCurrentFoodChange;
            currentWater.onChange -= OnCurrentWaterChange;
            equipWeaponSet.onChange -= OnEquipWeaponSetChange;
            isWeaponsSheathed.onChange -= OnIsWeaponsSheathedChange;
            pitch.onChange -= OnPitchChange;
            aimPosition.onChange -= OnAimPositionChange;
            targetEntityId.onChange -= OnTargetEntityIdChange;
            // On list changed events
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

        #region Sync data changed callback
        /// <summary>
        /// This will be called when id changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="id"></param>
        private void OnIdChange(bool isInitial, string id)
        {
            if (onIdChange != null)
                onIdChange.Invoke(id);
        }

        /// <summary>
        /// This will be called when character name changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="characterName"></param>
        private void OnCharacterNameChange(bool isInitial, string characterName)
        {
            if (onCharacterNameChange != null)
                onCharacterNameChange.Invoke(characterName);
        }

        /// <summary>
        /// This will be called when level changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="level"></param>
        private void OnLevelChange(bool isInitial, int level)
        {
            _isRecaching = true;
            if (onLevelChange != null)
                onLevelChange.Invoke(level);
        }

        /// <summary>
        /// This will be called when exp changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="exp"></param>
        private void OnExpChange(bool isInitial, int exp)
        {
            if (onExpChange != null)
                onExpChange.Invoke(exp);
        }

        /// <summary>
        /// This will be called when is immune changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="isImmune"></param>
        private void OnIsImmuneChange(bool isInitial, bool isImmune)
        {
            if (onIsImmuneChange != null)
                onIsImmuneChange.Invoke(isImmune);
        }

        /// <summary>
        /// This will be called when current mp changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="currentMp"></param>
        private void OnCurrentMpChange(bool isInitial, int currentMp)
        {
            if (onCurrentMpChange != null)
                onCurrentMpChange.Invoke(currentMp);
        }

        /// <summary>
        /// This will be called when current food changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="currentFood"></param>
        private void OnCurrentFoodChange(bool isInitial, int currentFood)
        {
            if (onCurrentFoodChange != null)
                onCurrentFoodChange.Invoke(currentFood);
        }

        /// <summary>
        /// This will be called when current water changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="currentWater"></param>
        private void OnCurrentWaterChange(bool isInitial, int currentWater)
        {
            if (onCurrentWaterChange != null)
                onCurrentWaterChange.Invoke(currentWater);
        }

        /// <summary>
        /// This will be called when equip weapon set changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="equipWeaponSet"></param>
        protected virtual void OnEquipWeaponSetChange(bool isInitial, byte equipWeaponSet)
        {
            PrepareToSetEquipWeaponsModels();
            _isRecaching = true;
            if (onEquipWeaponSetChange != null)
                onEquipWeaponSetChange.Invoke(equipWeaponSet);
        }

        /// <summary>
        /// This will be called when is weapons sheathed changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="isWeaponsSheathed"></param>
        protected virtual void OnIsWeaponsSheathedChange(bool isInitial, bool isWeaponsSheathed)
        {
            PrepareToSetEquipWeaponsModels();
            _isRecaching = true;
            if (onIsWeaponsSheathedChange != null)
                onIsWeaponsSheathedChange.Invoke(isWeaponsSheathed);
        }

        /// <summary>
        /// This will be called when pitch changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="pitch"></param>
        private void OnPitchChange(bool isInitial, ushort pitch)
        {
            if (onPitchChange != null)
                onPitchChange.Invoke(pitch);
        }

        /// <summary>
        /// This will be called when aim position changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="aimPosition"></param>
        private void OnAimPositionChange(bool isInitial, AimPosition aimPosition)
        {
            if (onAimPositionChange != null)
                onAimPositionChange.Invoke(aimPosition);
        }

        /// <summary>
        /// This will be called when target entity id changed
        /// </summary>
        /// <param name="isInitial"></param>
        /// <param name="targetEntityId"></param>
        private void OnTargetEntityIdChange(bool isInitial, uint targetEntityId)
        {
            if (onTargetEntityIdChange != null)
                onTargetEntityIdChange.Invoke(targetEntityId);
        }
        #endregion

        #region Net functions operation callback
        /// <summary>
        /// This will be called when equip weapons changed
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        private void OnSelectableWeaponSetsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            PrepareToSetEquipWeaponsModels();
            _selectableWeaponSetsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }

        /// <summary>
        /// This will be called when attributes changed
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        private void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            _attributesRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }

        /// <summary>
        /// This will be called when skills changed
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        private void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            _skillsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }

        /// <summary>
        /// This will be called when skill usages changed
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        private void OnSkillUsagesOperation(LiteNetLibSyncList.Operation operation, int index)
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
                        _skillsRecachingState = new SyncListRecachingState()
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
        /// This will be called when buffs changed
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        private void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
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
                    if (buffs.Count > 0 && buffs[buffs.Count - 1].GetBuff().GetBuff().disallowMove)
                        StopMove();
                    break;
            }

            _buffsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }

        /// <summary>
        /// This will be called when equip items changed
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        private void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            PrepareToSetEquipItemsModels();
            _equipItemsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }

        /// <summary>
        /// This will be called when non equip items changed
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            _nonEquipItemsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }

        /// <summary>
        /// This will be called when summons changed
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        private void OnSummonsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            _summonsRecachingState = new SyncListRecachingState()
            {
                isRecaching = true,
                operation = operation,
                index = index
            };
        }
        #endregion
    }
}
