using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    public enum AnimActionType : byte
    {
        None,
        Generic,
        Attack,
        Skill,
    }

    [RequireComponent(typeof(CharacterAnimationComponent))]
    [RequireComponent(typeof(CharacterRecoveryComponent))]
    [RequireComponent(typeof(CharacterSkillAndBuffComponent))]
    [RequireComponent(typeof(CharacterModel))]
    public abstract partial class BaseCharacterEntity : DamageableNetworkEntity, ICharacterData
    {
        public const float ACTION_COMMAND_DELAY = 0.2f;
        [HideInInspector]
        public bool isInSafeArea;

        #region Serialize data
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
        protected BaseCharacter database;
        protected RpgNetworkEntity targetEntity;
        protected readonly Dictionary<string, int> equipItemIndexes = new Dictionary<string, int>();
        protected AnimActionType animActionType;
        protected float lastActionCommandReceivedTime;
        public bool isRecaching { get; protected set; }
        public bool isSprinting { get; protected set; }
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

        private CharacterModel characterModel;
        public CharacterModel CharacterModel
        {
            get
            {
                if (characterModel == null)
                    characterModel = GetComponent<CharacterModel>();
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
            gameObject.layer = gameInstance.characterLayer;
            animActionType = AnimActionType.None;
            isRecaching = true;
        }

        protected override void EntityStart()
        {
            base.EntityStart();
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

        public override void ReceiveDamage(BaseCharacterEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            if (!IsServer || IsDead() || !CanReceiveDamageFrom(attacker))
                return;

            base.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
            // Calculate chance to hit
            var hitChance = gameInstance.GameplayRule.GetHitChance(attacker, this);
            // If miss, return don't calculate damages
            if (Random.value > hitChance)
            {
                ReceivedDamage(attacker, CombatAmountType.Miss, 0);
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
                    if (hitEffectsId < 0 && damageElement != gameInstance.DefaultDamageElement)
                        hitEffectsId = damageElement.hitEffects.Id;
                    var receivingDamage = damageElement.GetDamageReducedByResistance(this, damageAmount.Random());
                    if (receivingDamage > 0f)
                        calculatingTotalDamage += receivingDamage;
                }
            }
            // Play hit effect
            if (hitEffectsId <= 0)
                hitEffectsId = gameInstance.defaultHitEffects.Id;
            if (hitEffectsId > 0)
                RequestPlayEffect(hitEffectsId);
            // Calculate chance to critical
            var criticalChance = gameInstance.GameplayRule.GetCriticalChance(attacker, this);
            var isCritical = Random.value <= criticalChance;
            // If critical occurs
            if (isCritical)
                calculatingTotalDamage = gameInstance.GameplayRule.GetCriticalDamage(attacker, this, calculatingTotalDamage);
            // Calculate chance to block
            var blockChance = gameInstance.GameplayRule.GetBlockChance(attacker, this);
            var isBlocked = Random.value <= blockChance;
            // If block occurs
            if (isBlocked)
                calculatingTotalDamage = gameInstance.GameplayRule.GetBlockDamage(attacker, this, calculatingTotalDamage);
            // Apply damages
            var totalDamage = (int)calculatingTotalDamage;
            CurrentHp -= totalDamage;

            if (isBlocked)
                ReceivedDamage(attacker, CombatAmountType.BlockedDamage, totalDamage);
            else if (isCritical)
                ReceivedDamage(attacker, CombatAmountType.CriticalDamage, totalDamage);
            else
                ReceivedDamage(attacker, CombatAmountType.NormalDamage, totalDamage);

            if (CharacterModel != null)
                CharacterModel.PlayHurtAnimation();

            // If current hp <= 0, character dead
            if (IsDead())
                Killed(attacker);
            else if (!debuff.IsEmpty())
                ApplyBuff(debuff.characterId, debuff.dataId, debuff.type, debuff.level);
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
        public virtual void SetTargetEntity(RpgNetworkEntity entity)
        {
            targetEntity = entity;
        }

        public bool TryGetTargetEntity<T>(out T entity) where T : RpgNetworkEntity
        {
            entity = null;
            if (targetEntity == null)
                return false;
            entity = targetEntity as T;
            return entity != null;
        }
        #endregion

        #region Buffs / Weapons / Damage
        protected void ApplyBuff(string characterId, int dataId, BuffType type, short level)
        {
            if (IsDead() || !IsServer)
                return;

            var buffIndex = this.IndexOfBuff(characterId, dataId, type);
            if (buffIndex >= 0)
                buffs.RemoveAt(buffIndex);

            var newBuff = CharacterBuff.Create(characterId, type, dataId, level);
            newBuff.Added();
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

        protected void ApplyPotionBuff(CharacterItem characterItem)
        {
            var item = characterItem.GetPotionItem();
            if (item == null)
                return;
            ApplyBuff(Id, item.DataId, BuffType.PotionBuff, characterItem.level);
        }

        protected virtual void ApplySkillBuff(CharacterSkill characterSkill)
        {
            var skill = characterSkill.GetSkill();
            if (skill == null)
                return;
            if (skill.skillBuffType == SkillBuffType.BuffToUser)
                ApplyBuff(Id, skill.DataId, BuffType.SkillBuff, characterSkill.level);
        }

        protected virtual void ApplySkill(CharacterSkill characterSkill, Vector3 position, bool isAttack, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            var skill = characterSkill.GetSkill();
            switch (skill.skillType)
            {
                case SkillType.Active:
                    ApplySkillBuff(characterSkill);
                    if (isAttack)
                    {
                        CharacterBuff debuff = CharacterBuff.Empty;
                        if (skill.isDebuff)
                            debuff = CharacterBuff.Create(Id, BuffType.SkillDebuff, skill.DataId, characterSkill.level);
                        LaunchDamageEntity(position, weapon, damageInfo, allDamageAmounts, debuff, skill.hitEffects.Id);
                    }
                    break;
            }
        }

        public virtual void GetActionAnimationDurations(ActionAnimation anim, out float triggerDuration, out float totalDuration)
        {
            triggerDuration = 0f;
            totalDuration = 0f;
            AnimationClip animClip;
            float animTriggerDuration;
            float animExtraDuration;
            AudioClip animAudioClip;
            if (anim.GetData(CharacterModel, out animClip, out animTriggerDuration, out animExtraDuration, out animAudioClip))
            {
                triggerDuration = animTriggerDuration / CacheAtkSpeed;
                totalDuration = (animClip.length + animExtraDuration) / CacheAtkSpeed;
            }
        }

        public virtual void GetAttackingData(
            out CharacterItem weapon,
            out uint actionId,
            out float triggerDuration,
            out float totalDuration,
            out DamageInfo damageInfo,
            out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            // Initialize data
            weapon = null;
            actionId = 0;
            triggerDuration = 0f;
            totalDuration = 0f;
            damageInfo = null;
            allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Prepare weapon data
            var isLeftHand = false;
            weapon = this.GetRandomedWeapon(out isLeftHand);
            var weaponItem = weapon.GetWeaponItem();
            var weaponType = weaponItem.WeaponType;
            // Assign damage data
            damageInfo = weaponType.damageInfo;
            // Random animation
            var animArray = !isLeftHand ? weaponType.rightHandAttackAnimations : weaponType.leftHandAttackAnimations;
            var animLength = animArray.Length;
            if (animLength > 0)
            {
                var anim = animArray[Random.Range(0, animLength)];
                // Assign animation data
                actionId = anim.Id;
                GetActionAnimationDurations(anim, out triggerDuration, out totalDuration);
            }
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
            out CharacterItem weapon,
            out uint actionId,
            out float triggerDuration,
            out float totalDuration,
            out bool isAttack,
            out DamageInfo damageInfo,
            out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            // Initialize data
            weapon = null;
            isAttack = false;
            actionId = 0;
            triggerDuration = 0f;
            totalDuration = 0f;
            damageInfo = null;
            allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Prepare skill data
            var skill = characterSkill.GetSkill();
            if (skill == null)
                return;
            isAttack = skill.IsAttack();
            // Prepare weapon data
            var isLeftHand = false;
            weapon = this.GetRandomedWeapon(out isLeftHand);
            var weaponItem = weapon.GetWeaponItem();
            var weaponType = weaponItem.WeaponType;
            // Prepare animation
            if ((skill.castAnimations == null || skill.castAnimations.Length == 0) && isAttack)
            {
                // If there is no cast animations
                // Random attack animation
                var animArray = !isLeftHand ? weaponType.rightHandAttackAnimations : weaponType.leftHandAttackAnimations;
                var animLength = animArray.Length;
                if (animLength > 0)
                {
                    var anim = animArray[Random.Range(0, animLength)];
                    // Assign animation data
                    actionId = anim.Id;
                    GetActionAnimationDurations(anim, out triggerDuration, out totalDuration);
                }
            }
            else if (skill.castAnimations != null && skill.castAnimations.Length > 0)
            {
                // Random animation
                var animArray = skill.castAnimations;
                var animLength = animArray.Length;
                var anim = animArray[Random.Range(0, animLength)];
                // Assign animation data
                actionId = anim.Id;
                GetActionAnimationDurations(anim, out triggerDuration, out totalDuration);
            }
            if (isAttack)
            {
                switch (skill.skillAttackType)
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
            Vector3 position,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            CharacterBuff debuff,
            uint hitEffectsId)
        {
            if (!IsServer)
                return;

            Transform damageTransform = GetDamageTransform(damageInfo.damageType);
            switch (damageInfo.damageType)
            {
                case DamageType.Melee:
                    var halfFov = damageInfo.hitFov * 0.5f;
                    var hits = Physics.OverlapSphere(damageTransform.position, damageInfo.hitDistance, gameInstance.GetDamageableLayerMask());
                    foreach (var hit in hits)
                    {
                        var damageableEntity = hit.GetComponent<DamageableNetworkEntity>();
                        // Try to find damageable entity by building object materials
                        if (damageableEntity == null)
                        {
                            var buildingMaterial = hit.GetComponent<BuildingMaterial>();
                            if (buildingMaterial != null && buildingMaterial.buildingEntity != null)
                                damageableEntity = buildingMaterial.buildingEntity;
                        }
                        if (damageableEntity == null || damageableEntity == this || damageableEntity.IsDead())
                            continue;
                        var targetDir = (CacheTransform.position - damageableEntity.CacheTransform.position).normalized;
                        var angle = Vector3.Angle(targetDir, CacheTransform.forward);
                        // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
                        if (angle < 180 + halfFov && angle > 180 - halfFov)
                            damageableEntity.ReceiveDamage(this, weapon, allDamageAmounts, debuff, hitEffectsId);
                    }
                    break;
                case DamageType.Missile:
                    if (damageInfo.missileDamageEntity != null)
                    {
                        var missileDamageIdentity = Manager.Assets.NetworkSpawn(damageInfo.missileDamageEntity.Identity, damageTransform.position, damageTransform.rotation);
                        var missileDamageEntity = missileDamageIdentity.GetComponent<MissileDamageEntity>();
                        missileDamageEntity.SetupDamage(this, weapon, allDamageAmounts, debuff, hitEffectsId, damageInfo.missileDistance, damageInfo.missileSpeed);
                    }
                    break;
            }
        }
        #endregion

        protected virtual Transform GetDamageTransform(DamageType damageType)
        {
            if (CharacterModel != null)
            {
                switch (damageType)
                {
                    case DamageType.Melee:
                        return MeleeDamageTransform;
                    case DamageType.Missile:
                        return MissileDamageTransform;
                }
            }
            return CacheTransform;
        }

        public override void ReceivedDamage(BaseCharacterEntity attacker, CombatAmountType combatAmountType, int damage)
        {
            base.ReceivedDamage(attacker, combatAmountType, damage);
            gameInstance.GameplayRule.OnCharacterReceivedDamage(attacker, this, combatAmountType, damage);
        }

        public virtual void Killed(BaseCharacterEntity lastAttacker)
        {
            StopAllCoroutines();
            buffs.Clear();
            var count = skills.Count;
            for (var i = 0; i < count; ++i)
            {
                var skill = skills[i];
                skill.coolDownRemainsDuration = 0;
                skills.Dirty(i);
            }
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
            return animActionType == AnimActionType.Attack || animActionType == AnimActionType.Skill;
        }

        public virtual bool CanMoveOrDoActions()
        {
            return !IsDead() && !IsPlayingActionAnimation();
        }

        public virtual void IncreaseExp(int exp)
        {
            if (!IsServer)
                return;
            if (!gameInstance.GameplayRule.IncreaseExp(this, exp))
                return;
            // Send OnLevelUp to owner player only
            RequestOnLevelUp();
        }

        public T FindNearestAliveCharacter<T>(float distance, bool findForAlly, bool findForEnemy) where T : BaseCharacterEntity
        {
            T result = null;
            var colliders = Physics.OverlapSphere(CacheTransform.position, distance, gameInstance.characterLayer.Mask);
            if (colliders != null && colliders.Length > 0)
            {
                float tempDistance;
                T tempEntity;
                float nearestDistance = float.MaxValue;
                T nearestEntity = null;
                foreach (var collider in colliders)
                {
                    tempEntity = collider.GetComponent<T>();
                    if (tempEntity == null || tempEntity == this || tempEntity.IsDead())
                        continue;
                    if (findForAlly && !tempEntity.IsAlly(this))
                        continue;
                    if (findForEnemy && !tempEntity.IsEnemy(this))
                        continue;
                    tempDistance = Vector3.Distance(CacheTransform.position, tempEntity.CacheTransform.position);
                    if (tempDistance < nearestDistance)
                    {
                        nearestDistance = tempDistance;
                        nearestEntity = tempEntity;
                    }
                }
                result = nearestEntity;
            }
            return result;
        }

        public abstract bool CanReceiveDamageFrom(BaseCharacterEntity characterEntity);
        public abstract bool IsAlly(BaseCharacterEntity characterEntity);
        public abstract bool IsEnemy(BaseCharacterEntity characterEntity);
    }
}
