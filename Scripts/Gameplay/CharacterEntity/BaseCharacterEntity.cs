using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterAnimationComponent))]
    [RequireComponent(typeof(CharacterRecoveryComponent))]
    [RequireComponent(typeof(CharacterSkillAndBuffComponent))]
    public abstract partial class BaseCharacterEntity : DamageableNetworkEntity, ICharacterData, IAttackerEntity
    {
        public const float ACTION_COMMAND_DELAY = 0.2f;
        public const int OVERLAP_COLLIDER_SIZE = 32;

        [HideInInspector]
        public bool isInSafeArea;

        #region Serialize data
        public BaseCharacter database;
        [Header("Settings")]
        [Tooltip("These objects will be hidden on non owner objects")]
        public GameObject[] ownerObjects;
        [Tooltip("These objects will be hidden on owner objects")]
        public GameObject[] nonOwnerObjects;
        [Header("UI / Damage transform")]
        public Transform meleeDamageTransform;
        public Transform missileDamageTransform;
        public Transform uiElementTransform;
        public Transform miniMapElementContainer;
        #endregion

        #region Protected data
        protected UICharacterEntity uiCharacterEntity;
        protected BaseGameEntity targetEntity;
        protected readonly Dictionary<string, int> equipItemIndexes = new Dictionary<string, int>();
        protected AnimActionType animActionType;
        protected float lastActionCommandReceivedTime;
        public bool isRecaching { get; protected set; }
        public bool isSprinting { get; protected set; }
        #endregion

        #region Temp data
        protected Collider[] overlapColliders = new Collider[OVERLAP_COLLIDER_SIZE];
        protected int overlapSize;
        protected int counter;
        protected GameObject tempGameObject;
        #endregion

        #region Caches Data
        public CharacterStats CacheStats { get; protected set; }
        public Dictionary<Attribute, short> CacheAttributes { get; protected set; }
        public Dictionary<Skill, short> CacheSkills { get; protected set; }
        public Dictionary<DamageElement, float> CacheResistances { get; protected set; }
        public Dictionary<DamageElement, MinMaxFloat> CacheIncreaseDamages { get; protected set; }
        public int CacheMaxHp { get; protected set; }
        public int CacheMaxMp { get; protected set; }
        public int CacheMaxStamina { get; protected set; }
        public int CacheMaxFood { get; protected set; }
        public int CacheMaxWater { get; protected set; }
        public float CacheTotalItemWeight { get; protected set; }
        public float CacheAtkSpeed { get; protected set; }
        public float CacheMoveSpeed { get; protected set; }
        public float CacheBaseMoveSpeed { get; protected set; }
        #endregion

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

        public Transform MiniMapElementContainer
        {
            get
            {
                if (miniMapElementContainer == null)
                    miniMapElementContainer = CacheTransform;
                return miniMapElementContainer;
            }
        }

        public Transform UIElementTransform
        {
            get
            {
                if (uiElementTransform == null)
                    uiElementTransform = CacheTransform;
                return uiElementTransform;
            }
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.layer = GameInstance.characterLayer;
            animActionType = AnimActionType.None;
            isRecaching = true;
        }

        protected override void EntityOnSetOwnerClient()
        {
            base.EntityOnSetOwnerClient();
            foreach (var ownerObject in ownerObjects)
            {
                if (ownerObject == null) continue;
                ownerObject.SetActive(IsOwnerClient);
            }
            foreach (var nonOwnerObject in nonOwnerObjects)
            {
                if (nonOwnerObject == null) continue;
                nonOwnerObject.SetActive(!IsOwnerClient);
            }
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

            var weight = itemData.weight;
            // If overwhelming
            if (CacheTotalItemWeight + (amount * weight) > CacheStats.weightLimit)
                return true;

            return false;
        }

        public bool CanEquipItem(CharacterItem equippingItem, string equipPosition, out string reasonWhyCannot, out HashSet<string> shouldUnequipPositions)
        {
            reasonWhyCannot = "";
            shouldUnequipPositions = new HashSet<string>();

            var equipmentItem = equippingItem.GetEquipmentItem();
            if (equipmentItem == null)
            {
                reasonWhyCannot = "This item is not equipment item";
                return false;
            }

            if (string.IsNullOrEmpty(equipPosition))
            {
                reasonWhyCannot = "Invalid equip position";
                return false;
            }

            if (!equippingItem.CanEquip(this))
            {
                reasonWhyCannot = "Character level or attributes does not meet requirements";
                return false;
            }

            var weaponItem = equippingItem.GetWeaponItem();
            var shieldItem = equippingItem.GetShieldItem();
            var armorItem = equippingItem.GetArmorItem();

            var tempEquipWeapons = EquipWeapons;
            var rightHandWeapon = tempEquipWeapons.rightHand.GetWeaponItem();
            var leftHandWeapon = tempEquipWeapons.leftHand.GetWeaponItem();
            var leftHandShield = tempEquipWeapons.leftHand.GetShieldItem();

            WeaponItemEquipType rightHandEquipType;
            var hasRightHandItem = rightHandWeapon.TryGetWeaponItemEquipType(out rightHandEquipType);
            WeaponItemEquipType leftHandEquipType;
            var hasLeftHandItem = leftHandShield != null || leftHandWeapon.TryGetWeaponItemEquipType(out leftHandEquipType);

            if (weaponItem != null)
            {
                switch (weaponItem.EquipType)
                {
                    case WeaponItemEquipType.OneHand:
                        // If weapon is one hand its equip position must be right hand
                        if (!equipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND))
                        {
                            reasonWhyCannot = "Can equip to right hand only";
                            return false;
                        }
                        // One hand can equip with shield only 
                        // if there are weapons on left hand it should unequip
                        if (hasRightHandItem)
                            shouldUnequipPositions.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
                        if (hasLeftHandItem)
                            shouldUnequipPositions.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND);
                        break;
                    case WeaponItemEquipType.OneHandCanDual:
                        // If weapon is one hand can dual its equip position must be right or left hand
                        if (!equipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) &&
                            !equipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
                        {
                            reasonWhyCannot = "Can equip to right hand or left hand only";
                            return false;
                        }
                        // Unequip item if right hand weapon is one hand or two hand
                        if (hasRightHandItem)
                        {
                            if (rightHandEquipType == WeaponItemEquipType.OneHand ||
                                rightHandEquipType == WeaponItemEquipType.TwoHand)
                                shouldUnequipPositions.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
                        }
                        break;
                    case WeaponItemEquipType.TwoHand:
                        // If weapon is one hand its equip position must be right hand
                        if (!equipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND))
                        {
                            reasonWhyCannot = "Can equip to right hand or left hand only";
                            return false;
                        }
                        // Unequip both left and right hand
                        if (hasRightHandItem)
                            shouldUnequipPositions.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
                        if (hasLeftHandItem)
                            shouldUnequipPositions.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND);
                        break;
                }
            }

            if (shieldItem != null)
            {
                if (!equipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
                {
                    reasonWhyCannot = "Can equip to left hand only";
                    return false;
                }
                if (hasRightHandItem && rightHandEquipType == WeaponItemEquipType.TwoHand)
                    shouldUnequipPositions.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
            }

            if (armorItem != null)
            {
                if (!equipPosition.Equals(armorItem.EquipPosition))
                {
                    reasonWhyCannot = "Can equip to " + armorItem.EquipPosition + " only";
                    return false;
                }
            }
            shouldUnequipPositions.Add(equipPosition);
            return true;
        }

        public override void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            if (!IsServer || IsDead() || !CanReceiveDamageFrom(attacker))
                return;

            base.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
            var attackerCharacter = attacker as BaseCharacterEntity;

            // Notify enemy spotted when received damage from enemy
            NotifyEnemySpottedToAllies(attackerCharacter);

            // Notify enemy spotted when damage taken to enemy
            attackerCharacter.NotifyEnemySpottedToAllies(this);

            // Calculate chance to hit
            var hitChance = GameInstance.GameplayRule.GetHitChance(attackerCharacter, this);
            // If miss, return don't calculate damages
            if (Random.value > hitChance)
            {
                ReceivedDamage(attackerCharacter, CombatAmountType.Miss, 0);
                return;
            }

            // Calculate damages
            var calculatingTotalDamage = 0f;
            if (allDamageAmounts.Count > 0)
            {
                foreach (var allDamageAmount in allDamageAmounts)
                {
                    var damageElement = allDamageAmount.Key;
                    var damageAmount = allDamageAmount.Value;
                    // Set hit effect by damage element
                    if (hitEffectsId == 0 && damageElement != GameInstance.DefaultDamageElement)
                        hitEffectsId = damageElement.hitEffects.Id;
                    var receivingDamage = damageElement.GetDamageReducedByResistance(this, damageAmount.Random());
                    if (receivingDamage > 0f)
                        calculatingTotalDamage += receivingDamage;
                }
            }

            // Play hit effect
            if (hitEffectsId == 0)
                hitEffectsId = GameInstance.DefaultHitEffects.Id;
            if (hitEffectsId > 0)
                RequestPlayEffect(hitEffectsId);

            // Calculate chance to critical
            var criticalChance = GameInstance.GameplayRule.GetCriticalChance(attackerCharacter, this);
            var isCritical = Random.value <= criticalChance;
            // If critical occurs
            if (isCritical)
                calculatingTotalDamage = GameInstance.GameplayRule.GetCriticalDamage(attackerCharacter, this, calculatingTotalDamage);

            // Calculate chance to block
            var blockChance = GameInstance.GameplayRule.GetBlockChance(attackerCharacter, this);
            var isBlocked = Random.value <= blockChance;
            // If block occurs
            if (isBlocked)
                calculatingTotalDamage = GameInstance.GameplayRule.GetBlockDamage(attackerCharacter, this, calculatingTotalDamage);

            // Apply damages
            var totalDamage = (int)calculatingTotalDamage;
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
            for (var i = 0; i < equipItems.Count; ++i)
            {
                var entry = equipItems[i];
                var armorItem = entry.GetArmorItem();
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

            var buffIndex = this.IndexOfBuff(dataId, type);
            if (buffIndex >= 0)
                buffs.RemoveAt(buffIndex);

            var newBuff = CharacterBuff.Create(type, dataId, level);
            newBuff.Apply();
            buffs.Add(newBuff);

            var duration = newBuff.GetDuration();
            var recoveryHp = duration <= 0f ? newBuff.GetBuffRecoveryHp() : 0;
            if (recoveryHp != 0)
            {
                CurrentHp += recoveryHp;
                RequestCombatAmount(CombatAmountType.HpRecovery, recoveryHp);
            }
            var recoveryMp = duration <= 0f ? newBuff.GetBuffRecoveryMp() : 0;
            if (recoveryMp != 0)
            {
                CurrentMp += recoveryMp;
                RequestCombatAmount(CombatAmountType.HpRecovery, recoveryMp);
            }
            var recoveryStamina = duration <= 0f ? newBuff.GetBuffRecoveryStamina() : 0;
            if (recoveryStamina != 0)
            {
                CurrentStamina += recoveryStamina;
                RequestCombatAmount(CombatAmountType.HpRecovery, recoveryStamina);
            }
            var recoveryFood = duration <= 0f ? newBuff.GetBuffRecoveryFood() : 0;
            if (recoveryFood != 0)
            {
                CurrentFood += recoveryFood;
                RequestCombatAmount(CombatAmountType.FoodRecovery, recoveryFood);
            }
            var recoveryWater = duration <= 0f ? newBuff.GetBuffRecoveryWater() : 0;
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
                    foreach (var character in tempCharacters)
                    {
                        character.ApplyBuff(skill.DataId, BuffType.SkillBuff, level);
                    }
                    ApplyBuff(skill.DataId, BuffType.SkillBuff, level);
                    break;
                case SkillBuffType.BuffToNearbyCharacters:
                    tempCharacters = FindAliveCharacters<BaseCharacterEntity>(skill.buffDistance.GetAmount(level), true, false, true);
                    foreach (var character in tempCharacters)
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
            for (var i = 0; i < Summons.Count; ++i)
            {
                tempSummon = summons[i];
                if (tempSummon.type != SummonType.Pet)
                    continue;
                summons.RemoveAt(i);
                tempSummon.UnSummon(this);
            }
            // Summon new pet
            var newSummon = CharacterSummon.Create(SummonType.Pet, item.DataId);
            newSummon.Summon(this, level, 0f, exp);
            summons.Add(newSummon);
        }

        protected virtual void ApplySkillSummon(Skill skill, short level)
        {
            if (IsDead() || !IsServer || skill == null || level <= 0)
                return;
            var i = 0;
            var amountEachTime = skill.summon.amountEachTime.GetAmount(level);
            for (i = 0; i < amountEachTime; ++i)
            {
                var newSummon = CharacterSummon.Create(SummonType.Skill, skill.DataId);
                newSummon.Summon(this, skill.summon.level.GetAmount(level), skill.summon.duration.GetAmount(level));
                summons.Add(newSummon);
            }
            var count = 0;
            for (i = 0; i < summons.Count; ++i)
            {
                if (summons[i].dataId == skill.DataId)
                    ++count;
            }
            var maxStack = skill.summon.maxStack.GetAmount(level);
            var unSummonAmount = count > maxStack ? count - maxStack : 0;
            CharacterSummon tempSummon;
            for (i = unSummonAmount; i > 0; --i)
            {
                var summonIndex = this.IndexOfSummon(skill.DataId, SummonType.Skill);
                tempSummon = summons[summonIndex];
                if (summonIndex >= 0)
                {
                    summons.RemoveAt(summonIndex);
                    tempSummon.UnSummon(this);
                }
            }
        }

        protected virtual void ApplySkill(CharacterSkill characterSkill, Vector3 position, SkillAttackType skillAttackType, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            var skill = characterSkill.GetSkill();
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
                        LaunchDamageEntity(position, weapon, damageInfo, allDamageAmounts, debuff, skill.hitEffects.Id);
                    }
                    break;
            }
        }

        public virtual void GetAttackingData(
            out AnimActionType animActionType,
            out int dataId,
            out int animationIndex,
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
            weapon = null;
            triggerDuration = 0f;
            totalDuration = 0f;
            damageInfo = null;
            allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Prepare weapon data
            bool isLeftHand;
            weapon = this.GetRandomedWeapon(out isLeftHand);
            var weaponItem = weapon.GetWeaponItem();
            var weaponType = weaponItem.WeaponType;
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
            allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(
                allDamageAmounts,
                weaponItem.GetDamageAmount(weapon.level, weapon.GetEquipmentBonusRate(), this));
            allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(
                allDamageAmounts,
                CacheIncreaseDamages);
        }

        public virtual void GetUsingSkillData(
            CharacterSkill characterSkill,
            out AnimActionType animActionType,
            out int dataId,
            out int animationIndex,
            out SkillAttackType skillAttackType,
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
            weapon = null;
            triggerDuration = 0f;
            totalDuration = 0f;
            damageInfo = null;
            allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Prepare skill data
            var skill = characterSkill.GetSkill();
            if (skill == null)
                return;
            // Prepare weapon data
            skillAttackType = skill.skillAttackType;
            bool isLeftHand;
            weapon = this.GetRandomedWeapon(out isLeftHand);
            var weaponItem = weapon.GetWeaponItem();
            var weaponType = weaponItem.WeaponType;
            var hasSkillCastAnimation = CharacterModel.HasSkillCastAnimations(skill);
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
                        allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(
                            allDamageAmounts,
                            skill.GetDamageAmount(characterSkill.level, this));
                        // Sum damage with skill damage
                        allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(
                            allDamageAmounts,
                            skill.GetAdditionalDamageAmounts(characterSkill.level));
                        break;
                    case SkillAttackType.BasedOnWeapon:
                        // Assign damage data
                        damageInfo = weaponType.damageInfo;
                        // Calculate all damages
                        allDamageAmounts = weaponItem.GetDamageAmountWithInflictions(weapon.level, weapon.GetEquipmentBonusRate(), this, skill.GetWeaponDamageInflictions(characterSkill.level));
                        // Sum damage with additional damage amounts
                        allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(
                            allDamageAmounts,
                            skill.GetAdditionalDamageAmounts(characterSkill.level));
                        break;
                }
                allDamageAmounts = GameDataHelpers.CombineDamageAmountsDictionary(
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
            var rightHand = EquipWeapons.rightHand;
            var leftHand = EquipWeapons.leftHand;
            var rightHandWeapon = rightHand.GetWeaponItem();
            var leftHandWeapon = leftHand.GetWeaponItem();
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
                tempDamageInfo = GameInstance.DefaultWeaponItem.WeaponType.damageInfo;
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
            var rightHand = EquipWeapons.rightHand;
            var leftHand = EquipWeapons.leftHand;
            var rightHandWeapon = rightHand.GetWeaponItem();
            var leftHandWeapon = leftHand.GetWeaponItem();
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
                tempDamageInfo = GameInstance.DefaultWeaponItem.WeaponType.damageInfo;
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
            Vector3 position,
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
            GetDamagePositionAndRotation(damageInfo.damageType, out damagePosition, out damageRotation);
            switch (damageInfo.damageType)
            {
                case DamageType.Melee:
                    if (damageInfo.hitOnlySelectedTarget)
                    {
                        if (!TryGetTargetEntity(out tempDamageableEntity))
                        {
                            overlapSize = OverlapObjects(damagePosition, damageInfo.hitDistance, GameInstance.GetDamageableLayerMask());
                            if (overlapSize == 0)
                                return;
                            // Target entity not set, use overlapped object as target
                            tempGameObject = GetOverlapObject(0);
                            tempDamageableEntity = tempGameObject.GetComponent<DamageableNetworkEntity>();
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
                        overlapSize = OverlapObjects(damagePosition, damageInfo.hitDistance, GameInstance.GetDamageableLayerMask());
                        if (overlapSize == 0)
                            return;
                        for (counter = 0; counter < overlapSize; ++counter)
                        {
                            tempGameObject = GetOverlapObject(counter);
                            tempDamageableEntity = tempGameObject.GetComponent<DamageableNetworkEntity>();
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
                        var missileDamageEntity = Manager.Assets.NetworkSpawn(damageInfo.missileDamageEntity.Identity, damagePosition, damageRotation).GetComponent<MissileDamageEntity>();
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

        public virtual int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders, layerMask);
        }

        public virtual GameObject GetOverlapObject(int index)
        {
            return overlapColliders[index].gameObject;
        }

        public virtual bool IsPositionInFov(float fov, Vector3 position)
        {
            var halfFov = fov * 0.5f;
            var angle = Vector3.Angle((CacheTransform.position - position).normalized, CacheTransform.forward);
            // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
            return (angle < 180 + halfFov && angle > 180 - halfFov);
        }

        protected virtual void GetDamagePositionAndRotation(DamageType damageType, out Vector3 position, out Quaternion rotation)
        {
            position = CacheTransform.position;
            rotation = CacheTransform.rotation;
            if (CharacterModel != null)
            {
                switch (damageType)
                {
                    case DamageType.Melee:
                        position = MeleeDamageTransform.position;
                        rotation = MeleeDamageTransform.rotation;
                        break;
                    case DamageType.Missile:
                        position = MissileDamageTransform.position;
                        rotation = MissileDamageTransform.rotation;
                        break;
                }
            }
        }

        public override void ReceivedDamage(IAttackerEntity attacker, CombatAmountType combatAmountType, int damage)
        {
            base.ReceivedDamage(attacker, combatAmountType, damage);
            if (attacker is BaseCharacterEntity)
                GameInstance.GameplayRule.OnCharacterReceivedDamage(attacker as BaseCharacterEntity, this, combatAmountType, damage);
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
            CacheStats = this.GetStats();
            CacheAttributes = this.GetAttributes();
            CacheSkills = this.GetSkills();
            CacheResistances = this.GetResistances();
            CacheIncreaseDamages = this.GetIncreaseDamages();
            CacheMaxHp = (int)CacheStats.hp;
            CacheMaxMp = (int)CacheStats.mp;
            CacheMaxStamina = (int)CacheStats.stamina;
            CacheMaxFood = (int)CacheStats.food;
            CacheMaxWater = (int)CacheStats.water;
            CacheTotalItemWeight = this.GetTotalItemWeight();
            CacheAtkSpeed = CacheStats.atkSpeed;
            CacheMoveSpeed = CacheStats.moveSpeed;
            if (database != null)
                CacheBaseMoveSpeed = database.stats.baseStats.moveSpeed;
            isRecaching = false;
        }

        public virtual void InstantiateUI(UICharacterEntity prefab)
        {
            if (prefab == null)
                return;
            if (uiCharacterEntity != null)
                Destroy(uiCharacterEntity.gameObject);
            uiCharacterEntity = Instantiate(prefab);
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
            if (!GameInstance.GameplayRule.IncreaseExp(this, exp))
                return;
            // Send OnLevelUp to owner player only
            RequestOnLevelUp();
        }

        public List<T> FindAliveCharacters<T>(float distance, bool findForAlly, bool findForEnemy, bool findForNeutral) where T : BaseCharacterEntity
        {
            var result = new List<T>();
            overlapSize = OverlapObjects(CacheTransform.position, distance, GameInstance.characterLayer.Mask);
            if (overlapSize == 0)
                return null;
            T tempEntity;
            for (counter = 0; counter < overlapSize; ++counter)
            {
                tempGameObject = GetOverlapObject(counter);
                tempEntity = tempGameObject.GetComponent<T>();
                if (!IsCharacterWhichLookingFor(tempEntity, findForAlly, findForEnemy, findForNeutral))
                    continue;
                result.Add(tempEntity);
            }
            return result;
        }

        public T FindNearestAliveCharacter<T>(float distance, bool findForAlly, bool findForEnemy, bool findForNeutral) where T : BaseCharacterEntity
        {
            overlapSize = OverlapObjects(CacheTransform.position, distance, GameInstance.characterLayer.Mask);
            if (overlapSize == 0)
                return null;
            float tempDistance;
            T tempEntity;
            float nearestDistance = float.MaxValue;
            T nearestEntity = null;
            for (counter = 0; counter < overlapSize; ++counter)
            {
                tempGameObject = GetOverlapObject(counter);
                tempEntity = tempGameObject.GetComponent<T>();
                if (!IsCharacterWhichLookingFor(tempEntity, findForAlly, findForEnemy, findForNeutral))
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

        private bool IsCharacterWhichLookingFor(BaseCharacterEntity characterEntity, bool findForAlly, bool findForEnemy, bool findForNeutral)
        {
            if (characterEntity == null || characterEntity == this || characterEntity.IsDead())
                return false;
            return (findForAlly && characterEntity.IsAlly(this)) ||
                (findForEnemy && characterEntity.IsEnemy(this)) ||
                (findForNeutral && characterEntity.IsNeutral(this));
        }

        private void NotifyEnemySpottedToAllies(BaseCharacterEntity enemy)
        {
            // Warn that this character received damage to nearby characters
            var foundCharacters = FindAliveCharacters<BaseCharacterEntity>(GameInstance.enemySpottedNotifyDistance, true, false, false);
            foreach (var foundCharacter in foundCharacters)
            {
                foundCharacter.NotifyEnemySpotted(this, enemy);
            }
        }

        public virtual Vector3 GetSummonPosition()
        {
            return CacheTransform.position + new Vector3(Random.Range(GameInstance.minSummonDistance, GameInstance.maxSummonDistance) * GenericUtils.GetNegativePositive(), 0f, Random.Range(GameInstance.minSummonDistance, GameInstance.maxSummonDistance) * GenericUtils.GetNegativePositive());
        }

        public virtual Quaternion GetSummonRotation()
        {
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
