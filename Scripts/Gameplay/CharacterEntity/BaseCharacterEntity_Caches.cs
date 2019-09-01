using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        // States
        /// <summary>
        /// This variable will be TRUE when cache data have to re-cache
        /// </summary>
        public bool IsRecaching
        {
            get
            {
                return isRecaching ||
                    selectableWeaponSetsRecachingState.isRecaching ||
                    attributesRecachingState.isRecaching ||
                    skillsRecachingState.isRecaching ||
                    buffsRecachingState.isRecaching ||
                    equipItemsRecachingState.isRecaching ||
                    nonEquipItemsRecachingState.isRecaching ||
                    summonsRecachingState.isRecaching;
            }
        }
        protected bool isRecaching;
        protected SyncListRecachingState selectableWeaponSetsRecachingState;
        protected SyncListRecachingState attributesRecachingState;
        protected SyncListRecachingState skillsRecachingState;
        protected SyncListRecachingState buffsRecachingState;
        protected SyncListRecachingState equipItemsRecachingState;
        protected SyncListRecachingState nonEquipItemsRecachingState;
        protected SyncListRecachingState summonsRecachingState;
        // Data
        protected CharacterStats cacheStats;
        protected Dictionary<Attribute, short> cacheAttributes;
        protected Dictionary<Skill, short> cacheSkills;
        protected Dictionary<DamageElement, float> cacheResistances;
        protected Dictionary<DamageElement, MinMaxFloat> cacheIncreaseDamages;
        protected Dictionary<EquipmentSet, int> cacheEquipmentSets;
        protected int cacheMaxHp;
        protected int cacheMaxMp;
        protected int cacheMaxStamina;
        protected int cacheMaxFood;
        protected int cacheMaxWater;
        protected float cacheTotalItemWeight;
        protected float cacheAtkSpeed;
        protected float cacheMoveSpeed;
        public CharacterStats CacheStats { get { return cacheStats; } }
        public Dictionary<Attribute, short> CacheAttributes { get { return cacheAttributes; } }
        public Dictionary<Skill, short> CacheSkills { get { return cacheSkills; } }
        public Dictionary<DamageElement, float> CacheResistances { get { return cacheResistances; } }
        public Dictionary<DamageElement, MinMaxFloat> CacheIncreaseDamages { get { return cacheIncreaseDamages; } }
        public Dictionary<EquipmentSet, int> CacheEquipmentSets { get { return cacheEquipmentSets; } }
        public int CacheMaxHp { get { return cacheMaxHp; } }
        public int CacheMaxMp { get { return cacheMaxMp; } }
        public int CacheMaxStamina { get { return cacheMaxStamina; } }
        public int CacheMaxFood { get { return cacheMaxFood; } }
        public int CacheMaxWater { get { return cacheMaxWater; } }
        public float CacheTotalItemWeight { get { return cacheTotalItemWeight; } }
        public float CacheAtkSpeed { get { return cacheAtkSpeed; } }
        public float CacheMoveSpeed { get { return cacheMoveSpeed; } }
        public float CacheBaseMoveSpeed { get; protected set; }
        public bool CacheDisallowMove { get; protected set; }
        public bool CacheDisallowAttack { get; protected set; }
        public bool CacheDisallowUseSkill { get; protected set; }
        public bool CacheDisallowUseItem { get; protected set; }

        /// <summary>
        /// Make caches for character stats / attributes / skills / resistances / increase damages and so on immdediately
        /// </summary>
        public void ForceMakeCaches()
        {
            isRecaching = true;
            MakeCaches();
        }

        /// <summary>
        /// Make caches for character stats / attributes / skills / resistances / increase damages and so on when update calls
        /// </summary>
        protected virtual void MakeCaches()
        {
            if (!IsRecaching)
                return;

            if (cacheAttributes == null)
                cacheAttributes = new Dictionary<Attribute, short>();
            if (cacheResistances == null)
                cacheResistances = new Dictionary<DamageElement, float>();
            if (cacheIncreaseDamages == null)
                cacheIncreaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
            if (cacheSkills == null)
                cacheSkills = new Dictionary<Skill, short>();
            if (cacheEquipmentSets == null)
                cacheEquipmentSets = new Dictionary<EquipmentSet, int>();

            this.GetAllStats(
                out cacheStats,
                cacheAttributes,
                cacheResistances,
                cacheIncreaseDamages,
                cacheSkills,
                cacheEquipmentSets,
                out cacheMaxHp,
                out cacheMaxMp,
                out cacheMaxStamina,
                out cacheMaxFood,
                out cacheMaxWater,
                out cacheTotalItemWeight,
                out cacheAtkSpeed,
                out cacheMoveSpeed);

            if (this.GetDatabase() != null)
                CacheBaseMoveSpeed = this.GetDatabase().Stats.baseStats.moveSpeed;

            CacheDisallowMove = false;
            CacheDisallowAttack = false;
            CacheDisallowUseSkill = false;
            CacheDisallowUseItem = false;
            Buff tempBuff;
            foreach (CharacterBuff characterBuff in Buffs)
            {
                tempBuff = characterBuff.GetBuff();
                if (tempBuff.disallowMove)
                    CacheDisallowMove = true;
                if (tempBuff.disallowAttack)
                    CacheDisallowAttack = true;
                if (tempBuff.disallowUseSkill)
                    CacheDisallowUseSkill = true;
                if (tempBuff.disallowUseItem)
                    CacheDisallowUseItem = true;
                if (CacheDisallowMove &&
                    CacheDisallowAttack &&
                    CacheDisallowUseSkill &&
                    CacheDisallowUseItem)
                    break;
            }

            if (selectableWeaponSetsRecachingState.isRecaching)
            {
                if (onSelectableWeaponSetsOperation != null)
                    onSelectableWeaponSetsOperation.Invoke(selectableWeaponSetsRecachingState.operation, selectableWeaponSetsRecachingState.index);
                selectableWeaponSetsRecachingState = SyncListRecachingState.Empty;
            }

            if (attributesRecachingState.isRecaching)
            {
                if (onAttributesOperation != null)
                    onAttributesOperation.Invoke(attributesRecachingState.operation, attributesRecachingState.index);
                attributesRecachingState = SyncListRecachingState.Empty;
            }

            if (skillsRecachingState.isRecaching)
            {
                if (onSkillsOperation != null)
                    onSkillsOperation.Invoke(skillsRecachingState.operation, skillsRecachingState.index);
                skillsRecachingState = SyncListRecachingState.Empty;
            }

            if (buffsRecachingState.isRecaching)
            {
                if (onBuffsOperation != null)
                    onBuffsOperation.Invoke(buffsRecachingState.operation, buffsRecachingState.index);
                buffsRecachingState = SyncListRecachingState.Empty;
            }

            if (equipItemsRecachingState.isRecaching)
            {
                if (onEquipItemsOperation != null)
                    onEquipItemsOperation.Invoke(equipItemsRecachingState.operation, equipItemsRecachingState.index);
                equipItemsRecachingState = SyncListRecachingState.Empty;
            }

            if (nonEquipItemsRecachingState.isRecaching)
            {
                if (onNonEquipItemsOperation != null)
                    onNonEquipItemsOperation.Invoke(nonEquipItemsRecachingState.operation, nonEquipItemsRecachingState.index);
                nonEquipItemsRecachingState = SyncListRecachingState.Empty;
            }

            if (summonsRecachingState.isRecaching)
            {
                if (onSummonsOperation != null)
                    onSummonsOperation.Invoke(summonsRecachingState.operation, summonsRecachingState.index);
                summonsRecachingState = SyncListRecachingState.Empty;
            }

            isRecaching = false;
        }
    }
}
