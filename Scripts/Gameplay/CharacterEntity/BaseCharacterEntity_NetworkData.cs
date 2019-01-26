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
        protected SyncFieldEquipWeapons equipWeapons = new SyncFieldEquipWeapons();
        [SerializeField]
        protected SyncFieldBool isHidding = new SyncFieldBool();
        [Header("Sync Lists")]
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
        public System.Action<EquipWeapons> onEquipWeaponsChange;
        public System.Action<bool> onIsHiddingChange;
        // List
        public System.Action<LiteNetLibSyncList.Operation, int> onAttributesOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onSkillsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onSkillUsagesOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onBuffsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onEquipItemsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onNonEquipItemsOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onSummonsOperation;
        #endregion

        #region Fields/Interface implementation
        public virtual string Id { get { return id.Value; } set { id.Value = value; } }
        public virtual int DataId { get { return database.DataId; } set { database = GameInstance.AllCharacters[value]; } }
        public virtual int EntityId { get { return Identity.HashAssetId; } set { } }
        public virtual string CharacterName { get { return syncTitle.Value; } set { syncTitle.Value = value; } }
        public virtual short Level { get { return level.Value; } set { level.Value = value; } }
        public virtual int Exp { get { return exp.Value; } set { exp.Value = value; } }
        public virtual int CurrentMp { get { return currentMp.Value; } set { currentMp.Value = value; } }
        public virtual int CurrentStamina { get { return currentStamina.Value; } set { currentStamina.Value = value; } }
        public virtual int CurrentFood { get { return currentFood.Value; } set { currentFood.Value = value; } }
        public virtual int CurrentWater { get { return currentWater.Value; } set { currentWater.Value = value; } }
        public virtual EquipWeapons EquipWeapons { get { return equipWeapons.Value; } set { equipWeapons.Value = value; } }
        public virtual bool IsHidding { get { return isHidding.Value; } set { isHidding.Value = value; } }
        public override string Title { get { return CharacterName; } set { } }

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
                for (int i = 0; i < value.Count; ++i)
                {
                    CharacterItem entry = value[i];
                    Item armorItem = entry.GetArmorItem();
                    if (entry.IsValid() && armorItem != null && !equipItemIndexes.ContainsKey(armorItem.EquipPosition))
                    {
                        equipItemIndexes.Add(armorItem.EquipPosition, i);
                        equipItems.Add(entry);
                    }
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
        protected virtual void OnIdChange(string id)
        {
            if (onIdChange != null)
                onIdChange.Invoke(id);
        }

        /// <summary>
        /// Override this to do stuffs when character name changes
        /// </summary>
        /// <param name="characterName"></param>
        protected virtual void OnCharacterNameChange(string characterName)
        {
            if (onCharacterNameChange != null)
                onCharacterNameChange.Invoke(characterName);
        }

        /// <summary>
        /// Override this to do stuffs when level changes
        /// </summary>
        /// <param name="level"></param>
        protected virtual void OnLevelChange(short level)
        {
            isRecaching = true;

            if (onLevelChange != null)
                onLevelChange.Invoke(level);
        }

        /// <summary>
        /// Override this to do stuffs when exp changes
        /// </summary>
        /// <param name="exp"></param>
        protected virtual void OnExpChange(int exp)
        {
            if (onExpChange != null)
                onExpChange.Invoke(exp);
        }

        /// <summary>
        /// Override this to do stuffs when current hp changes
        /// </summary>
        /// <param name="currentHp"></param>
        protected virtual void OnCurrentHpChange(int currentHp)
        {
            if (onCurrentHpChange != null)
                onCurrentHpChange.Invoke(currentHp);
        }

        /// <summary>
        /// Override this to do stuffs when current mp changes
        /// </summary>
        /// <param name="currentMp"></param>
        protected virtual void OnCurrentMpChange(int currentMp)
        {
            if (onCurrentMpChange != null)
                onCurrentMpChange.Invoke(currentMp);
        }

        /// <summary>
        /// Override this to do stuffs when current food changes
        /// </summary>
        /// <param name="currentFood"></param>
        protected virtual void OnCurrentFoodChange(int currentFood)
        {
            if (onCurrentFoodChange != null)
                onCurrentFoodChange.Invoke(currentFood);
        }

        /// <summary>
        /// Override this to do stuffs when current water changes
        /// </summary>
        /// <param name="currentWater"></param>
        protected virtual void OnCurrentWaterChange(int currentWater)
        {
            if (onCurrentWaterChange != null)
                onCurrentWaterChange.Invoke(currentWater);
        }

        /// <summary>
        /// Override this to do stuffs when equip weapons changes
        /// </summary>
        /// <param name="equipWeapons"></param>
        protected virtual void OnEquipWeaponsChange(EquipWeapons equipWeapons)
        {
            if (CharacterModel != null)
                CharacterModel.SetEquipWeapons(equipWeapons);

            if (onEquipWeaponsChange != null)
                onEquipWeaponsChange.Invoke(equipWeapons);
        }

        /// <summary>
        /// Override this to do stuffs when hidding state changes
        /// </summary>
        /// <param name="isHidding"></param>
        protected virtual void OnIsHiddingChange(bool isHidding)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = !isHidding;
            }
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = !isHidding;
            }

            if (onIsHiddingChange != null)
                onIsHiddingChange.Invoke(isHidding);
        }
        #endregion

        #region Net functions operation callback
        /// <summary>
        /// Override this to do stuffs when attributes changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            isRecaching = true;

            if (onAttributesOperation != null)
                onAttributesOperation.Invoke(operation, index);
        }

        /// <summary>
        /// Override this to do stuffs when skills changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            isRecaching = true;

            if (onSkillsOperation != null)
                onSkillsOperation.Invoke(operation, index);
        }

        /// <summary>
        /// Override this to do stuffs when skill usages changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnSkillUsagesOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            isRecaching = true;

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
                    if (skillIndex >= 0 && onSkillsOperation != null)
                        onSkillsOperation(operation, skillIndex);
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
            isRecaching = true;

            if (CharacterModel != null)
                CharacterModel.SetBuffs(buffs);

            if (onBuffsOperation != null)
                onBuffsOperation.Invoke(operation, index);
        }

        /// <summary>
        /// Override this to do stuffs when equip items changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            isRecaching = true;

            if (CharacterModel != null)
                CharacterModel.SetEquipItems(equipItems);

            if (onEquipItemsOperation != null)
                onEquipItemsOperation.Invoke(operation, index);
        }

        /// <summary>
        /// Override this to do stuffs when non equip items changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            isRecaching = true;

            if (onNonEquipItemsOperation != null)
                onNonEquipItemsOperation.Invoke(operation, index);
        }

        /// <summary>
        /// Override this to do stuffs when summons changes
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="index"></param>
        protected virtual void OnSummonsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            isRecaching = true;

            if (onSummonsOperation != null)
                onSummonsOperation.Invoke(operation, index);
        }
        #endregion
    }
}
