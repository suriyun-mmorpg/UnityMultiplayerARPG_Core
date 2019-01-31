using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterAnimationComponent))]
    [RequireComponent(typeof(CharacterRecoveryComponent))]
    [RequireComponent(typeof(CharacterSkillAndBuffComponent))]
    [DisallowMultipleComponent]
    public abstract partial class BaseCharacterEntity : DamageableEntity, ICharacterData, IAttackerEntity
    {
        public const float ACTION_COMMAND_DELAY = 0.2f;
        public const int OVERLAP_COLLIDER_SIZE = 32;
        
        [HideInInspector, System.NonSerialized]
        // This will be TRUE when this character enter to safe area
        public bool isInSafeArea;

        #region Serialize data
        [HideInInspector]
        // TODO: This will be made as private variable later
        public BaseCharacter database;
        [HideInInspector]
        // TODO: This will be removed later
        public Transform uiElementTransform;
        [HideInInspector]
        // TODO: This will be removed later
        public Transform miniMapElementContainer;

        [Header("Character Settings")]
        [Tooltip("When character attack with melee weapon, it will cast sphere from this transform to detect hit objects")]
        public Transform meleeDamageTransform;
        [Tooltip("When character attack with range weapon, it will spawn missile damage entity from this transform")]
        public Transform missileDamageTransform;
        [Tooltip("Character UI will instantiates to this transform")]
        public Transform characterUITransform;
        [Tooltip("Mini Map UI will instantiates to this transform")]
        public Transform miniMapUITransform;
        #endregion

        #region Protected data
        protected UICharacterEntity uiCharacterEntity;
        protected BaseGameEntity targetEntity;
        protected readonly Dictionary<string, int> equipItemIndexes = new Dictionary<string, int>();
        protected AnimActionType animActionType;
        protected bool isAttackingOrUsingSkill;
        /// <summary>
        /// This variable will be TRUE when cache data have to re-cache
        /// </summary>
        public bool isRecaching { get; protected set; }
        /// <summary>
        /// This variable will be TRUE when player hold sprint key
        /// </summary>
        public bool isSprinting { get; protected set; }
        #endregion

        #region Temp data
        protected Collider[] overlapColliders = new Collider[OVERLAP_COLLIDER_SIZE];
        protected Collider2D[] overlapColliders2D = new Collider2D[OVERLAP_COLLIDER_SIZE];
        protected int tempOverlapSize;
        protected int tempLoopCounter;
        protected GameObject tempGameObject;
        protected Transform rightHandMissileDamageTransform;
        protected Transform leftHandMissileDamageTransform;
        protected bool hasAimPosition;
        protected Vector3 aimPosition;
        #endregion

        #region Caches Data
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
        #endregion

        public override int MaxHp { get { return CacheMaxHp; } }
        public float MoveAnimationSpeedMultiplier { get { return gameplayRule.GetMoveSpeed(this) / CacheBaseMoveSpeed; } }
        public abstract int DataId { get; set; }
        public abstract BaseCharacter Database { get; }

        private BaseCharacterModel characterModel;
        public BaseCharacterModel CharacterModel
        {
            get
            {
                if (characterModel == null)
                    characterModel = GetComponent<BaseCharacterModel>();
                return characterModel;
            }
        }

        public Transform MeleeDamageTransform
        {
            get
            {
                if (meleeDamageTransform == null)
                    meleeDamageTransform = CacheTransform;
                return meleeDamageTransform;
            }
        }

        public Transform MissileDamageTransform
        {
            get
            {
                if (missileDamageTransform == null)
                    missileDamageTransform = CacheTransform;
                return missileDamageTransform;
            }
        }

        public Transform CharacterUITransform
        {
            get
            {
                if (characterUITransform == null)
                    characterUITransform = CacheTransform;
                return characterUITransform;
            }
        }

        public Transform MiniMapUITransform
        {
            get
            {
                if (miniMapUITransform == null)
                    miniMapUITransform = CacheTransform;
                return miniMapUITransform;
            }
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.layer = gameInstance.characterLayer;
            animActionType = AnimActionType.None;
            isRecaching = true;
            MigrateTransforms();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
#if UNITY_EDITOR
            if (MigrateTransforms())
                EditorUtility.SetDirty(this);
#endif
        }

        private bool MigrateTransforms()
        {
            bool hasChanges = false;
            if (uiElementTransform != null)
            {
                characterUITransform = uiElementTransform;
                uiElementTransform = null;
                hasChanges = true;
            }
            if (miniMapElementContainer != null)
            {
                miniMapUITransform = miniMapElementContainer;
                miniMapElementContainer = null;
                hasChanges = true;
            }
            return hasChanges;
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            Profiler.BeginSample("BaseCharacterEntity - Update");
            MakeCaches();
            if (IsDead())
                animActionType = AnimActionType.None;
            Profiler.EndSample();
        }

        public virtual void ValidateRecovery()
        {
            if (!IsServer)
                return;

            // Validate Hp
            if (CurrentHp < 0)
                CurrentHp = 0;
            if (CurrentHp > CacheMaxHp)
                CurrentHp = CacheMaxHp;
            // Validate Mp
            if (CurrentMp < 0)
                CurrentMp = 0;
            if (CurrentMp > CacheMaxMp)
                CurrentMp = CacheMaxMp;
            // Validate Stamina
            if (CurrentStamina < 0)
                CurrentStamina = 0;
            if (CurrentStamina > CacheMaxStamina)
                CurrentStamina = CacheMaxStamina;
            // Validate Food
            if (CurrentFood < 0)
                CurrentFood = 0;
            if (CurrentFood > CacheMaxFood)
                CurrentFood = CacheMaxFood;
            // Validate Water
            if (CurrentWater < 0)
                CurrentWater = 0;
            if (CurrentWater > CacheMaxWater)
                CurrentWater = CacheMaxWater;

            if (IsDead())
                Killed(null);
        }

        #region Inventory helpers
        public bool IncreasingItemsWillOverwhelming(int dataId, short amount)
        {
            Item itemData;
            // If item not valid
            if (amount <= 0 || !GameInstance.Items.TryGetValue(dataId, out itemData))
                return false;

            float weight = itemData.weight;
            // If overwhelming
            if (CacheTotalItemWeight + (amount * weight) > CacheStats.weightLimit)
                return true;

            return false;
        }

        public bool CanEquipItem(CharacterItem equippingItem, InventoryType inventoryType, int oldEquipIndex, out GameMessage.Type gameMessageType, out bool shouldUnequipRightHand, out bool shouldUnequipLeftHand)
        {
            gameMessageType = GameMessage.Type.None;
            shouldUnequipRightHand = false;
            shouldUnequipLeftHand = false;

            Item equipmentItem = equippingItem.GetEquipmentItem();
            if (equipmentItem == null)
            {
                gameMessageType = GameMessage.Type.CannotEquip;
                return false;
            }

            if (!equippingItem.CanEquip(this))
            {
                gameMessageType = GameMessage.Type.LevelOrAttributeNotEnough;
                return false;
            }
            
            EquipWeapons tempEquipWeapons = EquipWeapons;
            Item rightHandWeapon = tempEquipWeapons.rightHand.GetWeaponItem();
            Item leftHandWeapon = tempEquipWeapons.leftHand.GetWeaponItem();
            Item leftHandShield = tempEquipWeapons.leftHand.GetShieldItem();

            WeaponItemEquipType rightHandEquipType;
            bool hasRightHandItem = rightHandWeapon.TryGetWeaponItemEquipType(out rightHandEquipType);
            WeaponItemEquipType leftHandEquipType;
            bool hasLeftHandItem = leftHandShield != null || leftHandWeapon.TryGetWeaponItemEquipType(out leftHandEquipType);

            // Equipping item is weapon
            Item weaponItem = equippingItem.GetWeaponItem();
            if (weaponItem != null)
            {
                switch (weaponItem.EquipType)
                {
                    case WeaponItemEquipType.OneHand:
                        // If weapon is one hand its equip position must be right hand
                        if (inventoryType != InventoryType.EquipWeaponRight)
                        {
                            gameMessageType = GameMessage.Type.InvalidEquipPositionRightHand;
                            return false;
                        }
                        // One hand can equip with shield only 
                        // if there are weapons on left hand it should unequip
                        if (hasRightHandItem)
                            shouldUnequipRightHand = true;
                        if (hasLeftHandItem)
                            shouldUnequipLeftHand = true;
                        break;
                    case WeaponItemEquipType.OneHandCanDual:
                        // If weapon is one hand can dual its equip position must be right or left hand
                        if (inventoryType != InventoryType.EquipWeaponRight &&
                            inventoryType != InventoryType.EquipWeaponLeft)
                        {
                            gameMessageType = GameMessage.Type.InvalidEquipPositionRightHandOrLeftHand;
                            return false;
                        }
                        if (inventoryType == InventoryType.EquipWeaponRight && hasRightHandItem)
                        {
                            shouldUnequipRightHand = true;
                        }
                        // Unequip item if right hand weapon is one hand or two hand when equipping at left hand
                        if (inventoryType == InventoryType.EquipWeaponLeft && hasLeftHandItem)
                        {
                            if (rightHandEquipType == WeaponItemEquipType.OneHand ||
                                rightHandEquipType == WeaponItemEquipType.TwoHand)
                                shouldUnequipLeftHand = true;
                        }
                        break;
                    case WeaponItemEquipType.TwoHand:
                        // If weapon is one hand its equip position must be right hand
                        if (inventoryType != InventoryType.EquipWeaponRight)
                        {
                            gameMessageType = GameMessage.Type.InvalidEquipPositionRightHand;
                            return false;
                        }
                        // Unequip both left and right hand
                        if (hasRightHandItem)
                            shouldUnequipRightHand = true;
                        if (hasLeftHandItem)
                            shouldUnequipLeftHand = true;
                        break;
                }
                return true;
            }
            // Equipping item is shield
            Item shieldItem = equippingItem.GetShieldItem();
            if (shieldItem != null)
            {
                if (inventoryType != InventoryType.EquipWeaponLeft)
                {
                    gameMessageType = GameMessage.Type.InvalidEquipPositionLeftHand;
                    return false;
                }
                if (hasRightHandItem && rightHandEquipType == WeaponItemEquipType.TwoHand)
                    shouldUnequipRightHand = true;
                if (hasLeftHandItem)
                    shouldUnequipLeftHand = true;
                return true;
            }
            // Equipping item is armor
            Item armorItem = equippingItem.GetArmorItem();
            if (armorItem != null)
            {
                if (oldEquipIndex >= 0 && !armorItem.EquipPosition.Equals(EquipItems[oldEquipIndex].GetArmorItem().EquipPosition))
                {
                    gameMessageType = GameMessage.Type.InvalidEquipPositionArmor;
                    return false;
                }
                return true;
            }
            return false;
        }

        public override void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            if (!IsServer || IsDead() || !CanReceiveDamageFrom(attacker))
                return;

            base.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
            BaseCharacterEntity attackerCharacter = attacker as BaseCharacterEntity;

            // Notify enemy spotted when received damage from enemy
            NotifyEnemySpottedToAllies(attackerCharacter);

            // Notify enemy spotted when damage taken to enemy
            attackerCharacter.NotifyEnemySpottedToAllies(this);

            // Calculate chance to hit
            float hitChance = gameInstance.GameplayRule.GetHitChance(attackerCharacter, this);
            // If miss, return don't calculate damages
            if (Random.value > hitChance)
            {
                ReceivedDamage(attackerCharacter, CombatAmountType.Miss, 0);
                return;
            }

            // Calculate damages
            float calculatingTotalDamage = 0f;
            if (allDamageAmounts.Count > 0)
            {
                foreach (KeyValuePair<DamageElement, MinMaxFloat> allDamageAmount in allDamageAmounts)
                {
                    DamageElement damageElement = allDamageAmount.Key;
                    MinMaxFloat damageAmount = allDamageAmount.Value;
                    // Set hit effect by damage element
                    if (hitEffectsId == 0 && damageElement != gameInstance.DefaultDamageElement)
                        hitEffectsId = damageElement.hitEffects.Id;
                    float receivingDamage = damageElement.GetDamageReducedByResistance(this, damageAmount.Random());
                    if (receivingDamage > 0f)
                        calculatingTotalDamage += receivingDamage;
                }
            }

            // Play hit effect
            if (hitEffectsId == 0)
                hitEffectsId = gameInstance.DefaultHitEffects.Id;
            if (hitEffectsId > 0)
                RequestPlayEffect(hitEffectsId);

            // Calculate chance to critical
            float criticalChance = gameInstance.GameplayRule.GetCriticalChance(attackerCharacter, this);
            bool isCritical = Random.value <= criticalChance;
            // If critical occurs
            if (isCritical)
                calculatingTotalDamage = gameInstance.GameplayRule.GetCriticalDamage(attackerCharacter, this, calculatingTotalDamage);

            // Calculate chance to block
            float blockChance = gameInstance.GameplayRule.GetBlockChance(attackerCharacter, this);
            bool isBlocked = Random.value <= blockChance;
            // If block occurs
            if (isBlocked)
                calculatingTotalDamage = gameInstance.GameplayRule.GetBlockDamage(attackerCharacter, this, calculatingTotalDamage);

            // Apply damages
            int totalDamage = (int)calculatingTotalDamage;
            CurrentHp -= totalDamage;

            if (isBlocked)
                ReceivedDamage(attackerCharacter, CombatAmountType.BlockedDamage, totalDamage);
            else if (isCritical)
                ReceivedDamage(attackerCharacter, CombatAmountType.CriticalDamage, totalDamage);
            else
                ReceivedDamage(attackerCharacter, CombatAmountType.NormalDamage, totalDamage);

            if (CharacterModel != null)
                CharacterModel.PlayHurtAnimation();

            // If current hp <= 0, character dead
            if (IsDead())
            {
                // Call killed function, this should be called only once when dead
                Killed(attackerCharacter);
            }
            else
            {
                // Apply debuff if character is not dead
                if (!debuff.IsEmpty())
                    ApplyBuff(debuff.dataId, debuff.type, debuff.level);
            }
        }
        #endregion

        #region Keys indexes update functions
        protected void UpdateEquipItemIndexes()
        {
            equipItemIndexes.Clear();
            for (int i = 0; i < equipItems.Count; ++i)
            {
                CharacterItem entry = equipItems[i];
                Item armorItem = entry.GetArmorItem();
                if (entry.IsValid() && armorItem != null && !equipItemIndexes.ContainsKey(armorItem.EquipPosition))
                    equipItemIndexes.Add(armorItem.EquipPosition, i);
            }
        }
        #endregion

        #region Target Entity Getter/Setter
        public virtual void SetTargetEntity(BaseGameEntity entity)
        {
            targetEntity = entity;
        }

        public virtual BaseGameEntity GetTargetEntity()
        {
            return targetEntity;
        }

        public bool TryGetTargetEntity<T>(out T entity) where T : class
        {
            entity = null;
            if (targetEntity == null)
                return false;
            entity = targetEntity as T;
            return entity != null;
        }
        #endregion

        #region Buffs / Weapons / Damage
        public void ApplyBuff(int dataId, BuffType type, short level)
        {
            if (IsDead() || !IsServer)
                return;

            int buffIndex = this.IndexOfBuff(dataId, type);
            if (buffIndex >= 0)
                buffs.RemoveAt(buffIndex);

            CharacterBuff newBuff = CharacterBuff.Create(type, dataId, level);
            newBuff.Apply();
            buffs.Add(newBuff);

            float duration = newBuff.GetDuration();
            int recoveryHp = duration <= 0f ? newBuff.GetBuffRecoveryHp() : 0;
            if (recoveryHp != 0)
            {
                CurrentHp += recoveryHp;
                RequestCombatAmount(CombatAmountType.HpRecovery, recoveryHp);
            }
            int recoveryMp = duration <= 0f ? newBuff.GetBuffRecoveryMp() : 0;
            if (recoveryMp != 0)
            {
                CurrentMp += recoveryMp;
                RequestCombatAmount(CombatAmountType.HpRecovery, recoveryMp);
            }
            int recoveryStamina = duration <= 0f ? newBuff.GetBuffRecoveryStamina() : 0;
            if (recoveryStamina != 0)
            {
                CurrentStamina += recoveryStamina;
                RequestCombatAmount(CombatAmountType.HpRecovery, recoveryStamina);
            }
            int recoveryFood = duration <= 0f ? newBuff.GetBuffRecoveryFood() : 0;
            if (recoveryFood != 0)
            {
                CurrentFood += recoveryFood;
                RequestCombatAmount(CombatAmountType.FoodRecovery, recoveryFood);
            }
            int recoveryWater = duration <= 0f ? newBuff.GetBuffRecoveryWater() : 0;
            if (recoveryWater != 0)
            {
                CurrentWater += recoveryWater;
                RequestCombatAmount(CombatAmountType.WaterRecovery, recoveryWater);
            }
            ValidateRecovery();
        }

        protected void ApplyPotionBuff(Item item, short level)
        {
            if (IsDead() || !IsServer || item == null || level <= 0)
                return;
            ApplyBuff(item.DataId, BuffType.PotionBuff, level);
        }

        protected virtual void ApplySkillBuff(Skill skill, short level)
        {
            if (IsDead() || !IsServer || skill == null || level <= 0)
                return;
            List<BaseCharacterEntity> tempCharacters;
            switch (skill.skillBuffType)
            {
                case SkillBuffType.BuffToUser:
                    ApplyBuff(skill.DataId, BuffType.SkillBuff, level);
                    break;
                case SkillBuffType.BuffToNearbyAllies:
                    tempCharacters = FindAliveCharacters<BaseCharacterEntity>(skill.buffDistance.GetAmount(level), true, false, false);
                    foreach (BaseCharacterEntity character in tempCharacters)
                    {
                        character.ApplyBuff(skill.DataId, BuffType.SkillBuff, level);
                    }
                    ApplyBuff(skill.DataId, BuffType.SkillBuff, level);
                    break;
                case SkillBuffType.BuffToNearbyCharacters:
                    tempCharacters = FindAliveCharacters<BaseCharacterEntity>(skill.buffDistance.GetAmount(level), true, false, true);
                    foreach (BaseCharacterEntity character in tempCharacters)
                    {
                        character.ApplyBuff(skill.DataId, BuffType.SkillBuff, level);
                    }
                    ApplyBuff(skill.DataId, BuffType.SkillBuff, level);
                    break;
            }
        }

        protected virtual void ApplyGuildSkillBuff(GuildSkill guildSkill, short level)
        {
            if (IsDead() || !IsServer || guildSkill == null || level <= 0)
                return;
            ApplyBuff(guildSkill.DataId, BuffType.GuildSkillBuff, level);
        }

        protected virtual void ApplyItemPetSummon(Item item, short level, int exp)
        {
            if (IsDead() || !IsServer || item == null || level <= 0)
                return;
            // Clear all summoned pets
            CharacterSummon tempSummon;
            for (int i = 0; i < Summons.Count; ++i)
            {
                tempSummon = summons[i];
                if (tempSummon.type != SummonType.Pet)
                    continue;
                summons.RemoveAt(i);
                tempSummon.UnSummon(this);
            }
            // Summon new pet
            CharacterSummon newSummon = CharacterSummon.Create(SummonType.Pet, item.DataId);
            newSummon.Summon(this, level, 0f, exp);
            summons.Add(newSummon);
        }

        protected virtual void ApplySkillSummon(Skill skill, short level)
        {
            if (IsDead() || !IsServer || skill == null || level <= 0)
                return;
            int i = 0;
            int amountEachTime = skill.summon.amountEachTime.GetAmount(level);
            for (i = 0; i < amountEachTime; ++i)
            {
                CharacterSummon newSummon = CharacterSummon.Create(SummonType.Skill, skill.DataId);
                newSummon.Summon(this, skill.summon.level.GetAmount(level), skill.summon.duration.GetAmount(level));
                summons.Add(newSummon);
            }
            int count = 0;
            for (i = 0; i < summons.Count; ++i)
            {
                if (summons[i].dataId == skill.DataId)
                    ++count;
            }
            int maxStack = skill.summon.maxStack.GetAmount(level);
            int unSummonAmount = count > maxStack ? count - maxStack : 0;
            CharacterSummon tempSummon;
            for (i = unSummonAmount; i > 0; --i)
            {
                int summonIndex = this.IndexOfSummon(skill.DataId, SummonType.Skill);
                tempSummon = summons[summonIndex];
                if (summonIndex >= 0)
                {
                    summons.RemoveAt(summonIndex);
                    tempSummon.UnSummon(this);
                }
            }
        }

        protected virtual void ApplySkill(CharacterSkill characterSkill, bool hasAimPosition, Vector3 aimPosition, SkillAttackType skillAttackType, bool isLeftHand, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            Skill skill = characterSkill.GetSkill();
            switch (skill.skillType)
            {
                case SkillType.Active:
                    ApplySkillBuff(skill, characterSkill.level);
                    ApplySkillSummon(skill, characterSkill.level);
                    if (skillAttackType != SkillAttackType.None)
                    {
                        CharacterBuff debuff = CharacterBuff.Empty;
                        if (skill.isDebuff)
                            debuff = CharacterBuff.Create(BuffType.SkillDebuff, skill.DataId, characterSkill.level);
                        LaunchDamageEntity(hasAimPosition, aimPosition, isLeftHand, weapon, damageInfo, allDamageAmounts, debuff, skill.hitEffects.Id);
                    }
                    break;
            }
        }

        public virtual void GetAttackingData(
            out AnimActionType animActionType,
            out int dataId,
            out int animationIndex,
            out bool isLeftHand,
            out CharacterItem weapon,
            out float triggerDuration,
            out float totalDuration,
            out DamageInfo damageInfo,
            out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            // Initialize data
            animActionType = AnimActionType.None;
            dataId = 0;
            animationIndex = 0;
            isLeftHand = false;
            weapon = null;
            triggerDuration = 0f;
            totalDuration = 0f;
            damageInfo = null;
            allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Prepare weapon data
            weapon = this.GetRandomedWeapon(out isLeftHand);
            Item weaponItem = weapon.GetWeaponItem();
            WeaponType weaponType = weaponItem.WeaponType;
            // Assign data id
            dataId = weaponType.DataId;
            // Assign animation action type
            animActionType = !isLeftHand ? AnimActionType.AttackRightHand : AnimActionType.AttackLeftHand;
            // Random animation
            if (!isLeftHand)
                CharacterModel.GetRandomRightHandAttackAnimation(dataId, out animationIndex, out triggerDuration, out totalDuration);
            else
                CharacterModel.GetRandomLeftHandAttackAnimation(dataId, out animationIndex, out triggerDuration, out totalDuration);
            // Assign damage data
            damageInfo = weaponType.damageInfo;
            // Calculate all damages
            allDamageAmounts = GameDataHelpers.CombineDamages(
                allDamageAmounts,
                weaponItem.GetDamageAmount(weapon.level, weapon.GetEquipmentBonusRate(), this));
            allDamageAmounts = GameDataHelpers.CombineDamages(
                allDamageAmounts,
                CacheIncreaseDamages);
        }

        public virtual void GetUsingSkillData(
            CharacterSkill characterSkill,
            out AnimActionType animActionType,
            out int dataId,
            out int animationIndex,
            out SkillAttackType skillAttackType,
            out bool isLeftHand,
            out CharacterItem weapon,
            out float triggerDuration,
            out float totalDuration,
            out DamageInfo damageInfo,
            out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            // Initialize data
            animActionType = AnimActionType.None;
            dataId = 0;
            animationIndex = 0;
            skillAttackType = SkillAttackType.None;
            isLeftHand = false;
            weapon = null;
            triggerDuration = 0f;
            totalDuration = 0f;
            damageInfo = null;
            allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Prepare skill data
            Skill skill = characterSkill.GetSkill();
            if (skill == null)
                return;
            // Prepare weapon data
            skillAttackType = skill.skillAttackType;
            weapon = this.GetRandomedWeapon(out isLeftHand);
            Item weaponItem = weapon.GetWeaponItem();
            WeaponType weaponType = weaponItem.WeaponType;
            bool hasSkillCastAnimation = CharacterModel.HasSkillCastAnimations(skill);
            // Prepare animation
            if (!hasSkillCastAnimation && skillAttackType != SkillAttackType.None)
            {
                // If there is no cast animations
                // Assign data id
                dataId = weaponType.DataId;
                // Assign animation action type
                animActionType = !isLeftHand ? AnimActionType.AttackRightHand : AnimActionType.AttackLeftHand;
                // Random animation
                if (!isLeftHand)
                    CharacterModel.GetRandomRightHandAttackAnimation(dataId, out animationIndex, out triggerDuration, out totalDuration);
                else
                    CharacterModel.GetRandomLeftHandAttackAnimation(dataId, out animationIndex, out triggerDuration, out totalDuration);
            }
            else if (hasSkillCastAnimation)
            {
                // Assign data id
                dataId = skill.DataId;
                // Assign animation action type
                animActionType = AnimActionType.Skill;
                // Random animation
                CharacterModel.GetRandomSkillCastAnimation(dataId, out animationIndex, out triggerDuration, out totalDuration);
            }
            // If it is attack skill
            if (skillAttackType != SkillAttackType.None)
            {
                switch (skillAttackType)
                {
                    case SkillAttackType.Normal:
                        // Assign damage data
                        damageInfo = skill.damageInfo;
                        // Calculate all damages
                        allDamageAmounts = weaponItem.GetDamageAmountWithInflictions(weapon.level, weapon.GetEquipmentBonusRate(), this, skill.GetWeaponDamageInflictions(characterSkill.level));
                        // Sum damage with additional damage amounts
                        allDamageAmounts = GameDataHelpers.CombineDamages(
                            allDamageAmounts,
                            skill.GetDamageAmount(characterSkill.level, this));
                        // Sum damage with skill damage
                        allDamageAmounts = GameDataHelpers.CombineDamages(
                            allDamageAmounts,
                            skill.GetAdditionalDamageAmounts(characterSkill.level));
                        break;
                    case SkillAttackType.BasedOnWeapon:
                        // Assign damage data
                        damageInfo = weaponType.damageInfo;
                        // Calculate all damages
                        allDamageAmounts = weaponItem.GetDamageAmountWithInflictions(weapon.level, weapon.GetEquipmentBonusRate(), this, skill.GetWeaponDamageInflictions(characterSkill.level));
                        // Sum damage with additional damage amounts
                        allDamageAmounts = GameDataHelpers.CombineDamages(
                            allDamageAmounts,
                            skill.GetAdditionalDamageAmounts(characterSkill.level));
                        break;
                }
                allDamageAmounts = GameDataHelpers.CombineDamages(
                    allDamageAmounts,
                    CacheIncreaseDamages);
            }
        }

        public virtual float GetAttackDistance()
        {
            // Finding minimum distance of equipped weapons
            // For example, if right hand attack distance is 1m and left hand attack distance is 0.7m
            // it will return 0.7m. if no equipped weapons, it will return default weapon attack distance
            float minDistance = float.MaxValue;
            DamageInfo tempDamageInfo;
            float tempDistance = 0f;
            CharacterItem rightHand = EquipWeapons.rightHand;
            CharacterItem leftHand = EquipWeapons.leftHand;
            Item rightHandWeapon = rightHand.GetWeaponItem();
            Item leftHandWeapon = leftHand.GetWeaponItem();
            if (rightHandWeapon != null)
            {
                tempDamageInfo = rightHandWeapon.WeaponType.damageInfo;
                tempDistance = tempDamageInfo.GetDistance();
                if (minDistance > tempDistance)
                    minDistance = tempDistance;
            }
            if (leftHandWeapon != null)
            {
                tempDamageInfo = leftHandWeapon.WeaponType.damageInfo;
                tempDistance = tempDamageInfo.GetDistance();
                if (minDistance > tempDistance)
                    minDistance = tempDistance;
            }
            if (rightHandWeapon == null && leftHandWeapon == null)
            {
                tempDamageInfo = gameInstance.DefaultWeaponItem.WeaponType.damageInfo;
                tempDistance = tempDamageInfo.GetDistance();
                minDistance = tempDistance;
            }
            return minDistance;
        }

        public virtual float GetAttackFov()
        {
            float minFov = float.MaxValue;
            DamageInfo tempDamageInfo;
            float tempFov = 0f;
            CharacterItem rightHand = EquipWeapons.rightHand;
            CharacterItem leftHand = EquipWeapons.leftHand;
            Item rightHandWeapon = rightHand.GetWeaponItem();
            Item leftHandWeapon = leftHand.GetWeaponItem();
            if (rightHandWeapon != null)
            {
                tempDamageInfo = rightHandWeapon.WeaponType.damageInfo;
                tempFov = tempDamageInfo.GetFov();
                if (minFov > tempFov)
                    minFov = tempFov;
            }
            if (leftHandWeapon != null)
            {
                tempDamageInfo = leftHandWeapon.WeaponType.damageInfo;
                tempFov = tempDamageInfo.GetFov();
                if (minFov > tempFov)
                    minFov = tempFov;
            }
            if (rightHandWeapon == null && leftHandWeapon == null)
            {
                tempDamageInfo = gameInstance.DefaultWeaponItem.WeaponType.damageInfo;
                tempFov = tempDamageInfo.GetFov();
                minFov = tempFov;
            }
            return minFov;
        }

        public virtual float GetSkillAttackDistance(Skill skill)
        {
            if (skill == null || !skill.IsAttack())
                return 0f;
            if (skill.skillAttackType == SkillAttackType.Normal)
                return skill.damageInfo.GetDistance();
            else
                return GetAttackDistance();
        }

        public virtual float GetSkillAttackFov(Skill skill)
        {
            if (skill == null || !skill.IsAttack())
                return 0f;
            if (skill.skillAttackType == SkillAttackType.Normal)
                return skill.damageInfo.GetFov();
            else
                return GetAttackFov();
        }

        public virtual void LaunchDamageEntity(
            bool hasAimPosition,
            Vector3 aimPosition,
            bool isLeftHand,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            CharacterBuff debuff,
            uint hitEffectsId)
        {
            if (!IsServer)
                return;

            IDamageableEntity tempDamageableEntity = null;
            Vector3 damagePosition;
            Quaternion damageRotation;
            GetDamagePositionAndRotation(damageInfo.damageType, hasAimPosition, aimPosition, isLeftHand, out damagePosition, out damageRotation);
            switch (damageInfo.damageType)
            {
                case DamageType.Melee:
                    if (damageInfo.hitOnlySelectedTarget)
                    {
                        if (!TryGetTargetEntity(out tempDamageableEntity))
                        {
                            tempOverlapSize = OverlapObjects(damagePosition, damageInfo.hitDistance, gameInstance.GetDamageableLayerMask());
                            if (tempOverlapSize == 0)
                                return;
                            // Target entity not set, use overlapped object as target
                            tempGameObject = GetOverlapObject(0);
                            tempDamageableEntity = tempGameObject.GetComponent<DamageableEntity>();
                        }
                        // Target receive damage
                        if (tempDamageableEntity != null && !tempDamageableEntity.IsDead() &&
                            (!(tempDamageableEntity is BaseCharacterEntity) || (BaseCharacterEntity)tempDamageableEntity != this) &&
                            IsPositionInFov(damageInfo.hitFov, tempDamageableEntity.transform.position))
                        {
                            // Pass all receive damage condition, then apply damages
                            tempDamageableEntity.ReceiveDamage(this, weapon, allDamageAmounts, debuff, hitEffectsId);
                        }
                    }
                    else
                    {
                        tempOverlapSize = OverlapObjects(damagePosition, damageInfo.hitDistance, gameInstance.GetDamageableLayerMask());
                        if (tempOverlapSize == 0)
                            return;
                        for (tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
                        {
                            tempGameObject = GetOverlapObject(tempLoopCounter);
                            tempDamageableEntity = tempGameObject.GetComponent<DamageableEntity>();
                            // Target receive damage
                            if (tempDamageableEntity != null && !tempDamageableEntity.IsDead() &&
                                (!(tempDamageableEntity is BaseCharacterEntity) || (BaseCharacterEntity)tempDamageableEntity != this) &&
                                IsPositionInFov(damageInfo.hitFov, tempDamageableEntity.transform.position))
                            {
                                // Pass all receive damage condition, then apply damages
                                tempDamageableEntity.ReceiveDamage(this, weapon, allDamageAmounts, debuff, hitEffectsId);
                            }
                        }
                    }
                    break;
                case DamageType.Missile:
                    if (damageInfo.missileDamageEntity != null)
                    {
                        MissileDamageEntity missileDamageEntity = Manager.Assets.NetworkSpawn(damageInfo.missileDamageEntity.Identity, damagePosition, damageRotation).GetComponent<MissileDamageEntity>();
                        if (damageInfo.hitOnlySelectedTarget)
                        {
                            if (!TryGetTargetEntity(out tempDamageableEntity))
                                tempDamageableEntity = null;
                        }
                        missileDamageEntity.SetupDamage(this, weapon, allDamageAmounts, debuff, hitEffectsId, damageInfo.missileDistance, damageInfo.missileSpeed, tempDamageableEntity);
                    }
                    break;
            }
        }
        #endregion

        public int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            if (gameInstance.DimensionType == DimensionType.Dimension2D)
                return Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D, layerMask);
            return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders, layerMask);
        }

        public GameObject GetOverlapObject(int index)
        {
            if (gameInstance.DimensionType == DimensionType.Dimension2D)
                return overlapColliders2D[index].gameObject;
            return overlapColliders[index].gameObject;
        }

        public virtual bool IsPositionInFov(float fov, Vector3 position)
        {
            float halfFov = fov * 0.5f;
            float angle = Vector3.Angle((CacheTransform.position - position).normalized, CacheTransform.forward);
            // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
            return (angle < 180 + halfFov && angle > 180 - halfFov);
        }

        protected virtual void GetDamagePositionAndRotation(DamageType damageType, bool hasAimPosition, Vector3 aimPosition, bool isLeftHand, out Vector3 position, out Quaternion rotation)
        {
            position = CacheTransform.position;
            switch (damageType)
            {
                case DamageType.Melee:
                    position = MeleeDamageTransform.position;
                    break;
                case DamageType.Missile:
                    if (rightHandMissileDamageTransform != null && !isLeftHand)
                    {
                        // Use position from right hand weapon missile damage transform
                        position = rightHandMissileDamageTransform.position;
                    }
                    else if (leftHandMissileDamageTransform != null && isLeftHand)
                    {
                        // Use position from left hand weapon missile damage transform
                        position = leftHandMissileDamageTransform.position;
                    }
                    else
                    {
                        // Use position from default missile damage transform
                        position = MissileDamageTransform.position;
                    }
                    break;
            }
            rotation = Quaternion.LookRotation(CacheTransform.forward);
            if (hasAimPosition)
                rotation = Quaternion.LookRotation(aimPosition);
        }

        public override void ReceivedDamage(IAttackerEntity attacker, CombatAmountType combatAmountType, int damage)
        {
            base.ReceivedDamage(attacker, combatAmountType, damage);
            if (attacker is BaseCharacterEntity)
                gameInstance.GameplayRule.OnCharacterReceivedDamage(attacker as BaseCharacterEntity, this, combatAmountType, damage);
        }

        public virtual void Killed(BaseCharacterEntity lastAttacker)
        {
            StopAllCoroutines();
            buffs.Clear();
            skillUsages.Clear();
            // Send OnDead to owner player only
            RequestOnDead();
        }

        public virtual void Respawn()
        {
            if (!IsServer || !IsDead())
                return;
            CurrentHp = CacheMaxHp;
            CurrentMp = CacheMaxMp;
            CurrentStamina = CacheMaxStamina;
            CurrentFood = CacheMaxFood;
            CurrentWater = CacheMaxWater;
            // Send OnRespawn to owner player only
            RequestOnRespawn();
        }

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
            if (!isRecaching)
                return;
            this.GetAllStats(
                out cacheStats,
                out cacheAttributes,
                out cacheSkills,
                out cacheResistances,
                out cacheIncreaseDamages,
                out cacheEquipmentSets,
                out cacheMaxHp,
                out cacheMaxMp,
                out cacheMaxStamina,
                out cacheMaxFood,
                out cacheMaxWater,
                out cacheTotalItemWeight,
                out cacheAtkSpeed,
                out cacheMoveSpeed);
            if (Database != null)
                CacheBaseMoveSpeed = Database.stats.baseStats.moveSpeed;
            isRecaching = false;
        }

        public virtual void InstantiateUI(UICharacterEntity prefab)
        {
            if (prefab == null)
                return;
            if (uiCharacterEntity != null)
                Destroy(uiCharacterEntity.gameObject);
            uiCharacterEntity = Instantiate(prefab, CharacterUITransform);
            uiCharacterEntity.transform.localPosition = Vector3.zero;
            uiCharacterEntity.Data = this;
        }

        public virtual bool IsPlayingActionAnimation()
        {
            return animActionType == AnimActionType.AttackRightHand || animActionType == AnimActionType.AttackLeftHand || animActionType == AnimActionType.Skill;
        }

        public virtual bool CanMoveOrDoActions()
        {
            return !IsDead() && !IsPlayingActionAnimation();
        }

        public virtual void RewardExp(int exp, RewardGivenType rewardGivenType)
        {
            if (!IsServer)
                return;
            if (!gameInstance.GameplayRule.IncreaseExp(this, exp))
                return;
            // Send OnLevelUp to owner player only
            RequestOnLevelUp();
        }

        public List<T> FindCharacters<T>(float distance, bool findForAliveOnly, bool findForAlly, bool findForEnemy, bool findForNeutral)
            where T : BaseCharacterEntity
        {
            List<T> result = new List<T>();
            tempOverlapSize = OverlapObjects(CacheTransform.position, distance, gameInstance.characterLayer.Mask);
            if (tempOverlapSize == 0)
                return null;
            T tempEntity;
            for (tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
            {
                tempGameObject = GetOverlapObject(tempLoopCounter);
                tempEntity = tempGameObject.GetComponent<T>();
                if (!IsCharacterWhichLookingFor(tempEntity, findForAliveOnly, findForAlly, findForEnemy, findForNeutral))
                    continue;
                result.Add(tempEntity);
            }
            return result;
        }

        public List<T> FindAliveCharacters<T>(float distance, bool findForAlly, bool findForEnemy, bool findForNeutral)
            where T : BaseCharacterEntity
        {
            return FindCharacters<T>(distance, true, findForAlly, findForEnemy, findForNeutral);
        }

        public T FindNearestCharacter<T>(float distance, bool findForAliveOnly, bool findForAlly, bool findForEnemy, bool findForNeutral)
            where T : BaseCharacterEntity
        {
            tempOverlapSize = OverlapObjects(CacheTransform.position, distance, gameInstance.characterLayer.Mask);
            if (tempOverlapSize == 0)
                return null;
            float tempDistance;
            T tempEntity;
            float nearestDistance = float.MaxValue;
            T nearestEntity = null;
            for (tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
            {
                tempGameObject = GetOverlapObject(tempLoopCounter);
                tempEntity = tempGameObject.GetComponent<T>();
                if (!IsCharacterWhichLookingFor(tempEntity, findForAliveOnly, findForAlly, findForEnemy, findForNeutral))
                    continue;
                tempDistance = Vector3.Distance(CacheTransform.position, tempEntity.CacheTransform.position);
                if (tempDistance < nearestDistance)
                {
                    nearestDistance = tempDistance;
                    nearestEntity = tempEntity;
                }
            }
            return nearestEntity;
        }

        public T FindNearestAliveCharacter<T>(float distance, bool findForAlly, bool findForEnemy, bool findForNeutral)
            where T : BaseCharacterEntity
        {
            return FindNearestCharacter<T>(distance, true, findForAlly, findForEnemy, findForNeutral);
        }

        private bool IsCharacterWhichLookingFor(BaseCharacterEntity characterEntity, bool findForAliveOnly, bool findForAlly, bool findForEnemy, bool findForNeutral)
        {
            if (characterEntity == null || characterEntity == this)
                return false;
            if (findForAliveOnly && characterEntity.IsDead())
                return false;
            return (findForAlly && characterEntity.IsAlly(this)) ||
                (findForEnemy && characterEntity.IsEnemy(this)) ||
                (findForNeutral && characterEntity.IsNeutral(this));
        }

        private void NotifyEnemySpottedToAllies(BaseCharacterEntity enemy)
        {
            // Warn that this character received damage to nearby characters
            List<BaseCharacterEntity> foundCharacters = FindAliveCharacters<BaseCharacterEntity>(gameInstance.enemySpottedNotifyDistance, true, false, false);
            foreach (BaseCharacterEntity foundCharacter in foundCharacters)
            {
                foundCharacter.NotifyEnemySpotted(this, enemy);
            }
        }

        public virtual Vector3 GetSummonPosition()
        {
            if (gameInstance.DimensionType == DimensionType.Dimension2D)
                return CacheTransform.position + new Vector3(Random.Range(gameInstance.minSummonDistance, gameInstance.maxSummonDistance) * GenericUtils.GetNegativePositive(), Random.Range(gameInstance.minSummonDistance, gameInstance.maxSummonDistance) * GenericUtils.GetNegativePositive(), 0f);
            return CacheTransform.position + new Vector3(Random.Range(gameInstance.minSummonDistance, gameInstance.maxSummonDistance) * GenericUtils.GetNegativePositive(), 0f, Random.Range(gameInstance.minSummonDistance, gameInstance.maxSummonDistance) * GenericUtils.GetNegativePositive());
        }

        public virtual Quaternion GetSummonRotation()
        {
            if (gameInstance.DimensionType == DimensionType.Dimension2D)
                return Quaternion.identity;
            return CacheTransform.rotation;
        }

        public abstract void NotifyEnemySpotted(BaseCharacterEntity ally, BaseCharacterEntity attacker);
        public abstract bool IsAlly(BaseCharacterEntity characterEntity);
        public abstract bool IsEnemy(BaseCharacterEntity characterEntity);
        public bool IsNeutral(BaseCharacterEntity characterEntity)
        {
            return !IsAlly(characterEntity) && !IsEnemy(characterEntity);
        }
    }
}
