using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterRecoveryComponent))]
    [RequireComponent(typeof(CharacterSkillAndBuffComponent))]
    [DisallowMultipleComponent]
    public abstract partial class BaseCharacterEntity : DamageableEntity, ICharacterData, IAttackerEntity
    {
        public const float ACTION_COMMAND_DELAY = 0.2f;
        public const int OVERLAP_COLLIDER_SIZE_FOR_ATTACK = 256;
        public const int OVERLAP_COLLIDER_SIZE_FOR_FIND = 32;
        
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

#if UNITY_EDITOR
        [Header("Character Attack Debug")]
        public Vector3? debugDamagePosition;
        public Quaternion? debugDamageRotation;
        public Color debugFovColor = new Color(0, 1, 0, 0.04f);
#endif

        #region Protected data
        protected UICharacterEntity uiCharacterEntity;
        protected BaseGameEntity targetEntity;
        protected readonly Dictionary<string, int> equipItemIndexes = new Dictionary<string, int>();
        protected AnimActionType animActionType;
        /// <summary>
        /// This variable will be TRUE when cache data have to re-cache
        /// </summary>
        public bool isRecaching { get; protected set; }
        public bool isAttackingOrUsingSkill { get; protected set; }
        public bool isCastingSkillCanBeInterrupted { get; protected set; }
        public bool isCastingSkillInterrupted { get; protected set; }
        public float castingSkillDuration { get; protected set; }
        public float castingSkillCountDown { get; protected set; }
        public float moveSpeedRateWhileAttackOrUseSkill { get; protected set; }
        #endregion

        #region Temp data
        protected Collider[] overlapColliders_ForAttackFunctions = new Collider[OVERLAP_COLLIDER_SIZE_FOR_ATTACK];
        protected Collider2D[] overlapColliders2D_ForAttackFunctions = new Collider2D[OVERLAP_COLLIDER_SIZE_FOR_ATTACK];
        protected Collider[] overlapColliders_ForFindFunctions = new Collider[OVERLAP_COLLIDER_SIZE_FOR_FIND];
        protected Collider2D[] overlapColliders2D_ForFindFunctions = new Collider2D[OVERLAP_COLLIDER_SIZE_FOR_FIND];
        protected GameObject tempGameObject;
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
        public bool CacheDisallowMove { get; protected set; }
        public bool CacheDisallowAttack { get; protected set; }
        public bool CacheDisallowUseSkill { get; protected set; }
        public bool CacheDisallowUseItem { get; protected set; }
        #endregion

        public override int MaxHp { get { return CacheMaxHp; } }
        public float MoveAnimationSpeedMultiplier { get { return gameplayRule.GetMoveSpeed(this) / CacheBaseMoveSpeed; } }
        public virtual bool IsGrounded { get; protected set; }
        public virtual bool IsJumping { get; protected set; }
        public abstract int DataId { get; set; }

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
            MigrateRemoveCharacterAnimationComponent();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
#if UNITY_EDITOR
            if (MigrateTransforms())
                EditorUtility.SetDirty(this);
            if (MigrateRemoveCharacterAnimationComponent())
                EditorUtility.SetDirty(gameObject);
#endif
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (debugDamagePosition.HasValue && debugDamageRotation.HasValue)
            {
                float atkHalfFov = GetAttackFov() * 0.5f;
                float atkDist = GetAttackDistance();
                Handles.color = debugFovColor;
                Handles.DrawSolidArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.up, debugDamageRotation.Value * Vector3.forward, -atkHalfFov, atkDist);
                Handles.DrawSolidArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.up, debugDamageRotation.Value * Vector3.forward, atkHalfFov, atkDist);
                Handles.DrawSolidArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.right, debugDamageRotation.Value * Vector3.forward, -atkHalfFov, atkDist);
                Handles.DrawSolidArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.right, debugDamageRotation.Value * Vector3.forward, atkHalfFov, atkDist);

                Handles.color = new Color(1, 0, 0, debugFovColor.a);
                Handles.DrawSolidArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.up, debugDamageRotation.Value * Vector3.forward, -atkHalfFov, 0);
                Handles.DrawSolidArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.up, debugDamageRotation.Value * Vector3.forward, atkHalfFov, 0);
                Handles.DrawSolidArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.right, debugDamageRotation.Value * Vector3.forward, -atkHalfFov, 0);
                Handles.DrawSolidArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.right, debugDamageRotation.Value * Vector3.forward, atkHalfFov, 0);

                Handles.color = new Color(debugFovColor.r, debugFovColor.g, debugFovColor.b);
                Handles.DrawWireArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.up, debugDamageRotation.Value * Vector3.forward, -atkHalfFov, atkDist);
                Handles.DrawWireArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.up, debugDamageRotation.Value * Vector3.forward, atkHalfFov, atkDist);
                Handles.DrawWireArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.up, debugDamageRotation.Value * Vector3.forward, -atkHalfFov, 0);
                Handles.DrawWireArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.up, debugDamageRotation.Value * Vector3.forward, atkHalfFov, 0);

                Handles.DrawWireArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.right, debugDamageRotation.Value * Vector3.forward, -atkHalfFov, 0);
                Handles.DrawWireArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.right, debugDamageRotation.Value * Vector3.forward, atkHalfFov, 0);
                Handles.DrawWireArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.right, debugDamageRotation.Value * Vector3.forward, -atkHalfFov, atkDist);
                Handles.DrawWireArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.right, debugDamageRotation.Value * Vector3.forward, atkHalfFov, atkDist);
            }
        }
#endif

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

        private bool MigrateRemoveCharacterAnimationComponent()
        {
            bool hasChanges = false;
            CharacterAnimationComponent comp = GetComponent<CharacterAnimationComponent>();
            if (comp != null)
            {
                comp.enabled = false;
                Debug.LogWarning("`CharacterAnimationComponent` component will not be used anymore from v1.40 or above, so developer should remove it from character entities");
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
            {
                // Clear action states when character dead
                animActionType = AnimActionType.None;
                isAttackingOrUsingSkill = false;
                InterruptCastingSkill();
            }
            CharacterModel.UpdateAnimation(IsDead(), MovementState, MoveAnimationSpeedMultiplier);
            if (castingSkillCountDown > 0)
            {
                castingSkillCountDown -= Time.deltaTime;
                if (castingSkillCountDown < 0)
                    castingSkillCountDown = 0;
            }
            Profiler.EndSample();
        }

        #region Caches / Relates Objects
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
                out cacheResistances,
                out cacheIncreaseDamages,
                out cacheSkills,
                out cacheEquipmentSets,
                out cacheMaxHp,
                out cacheMaxMp,
                out cacheMaxStamina,
                out cacheMaxFood,
                out cacheMaxWater,
                out cacheTotalItemWeight,
                out cacheAtkSpeed,
                out cacheMoveSpeed);
            if (this.GetDatabase() != null)
                CacheBaseMoveSpeed = this.GetDatabase().stats.baseStats.moveSpeed;
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
        #endregion

        #region Attack / Receive Damage / Dead / Spawn
        public void ValidateRecovery(BaseCharacterEntity attacker = null)
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
                Killed(attacker);
        }

        protected virtual void GetDamagePositionAndRotation(DamageType damageType, bool isLeftHand, bool hasAimPosition, Vector3 aimPosition, out Vector3 position, out Quaternion rotation)
        {
            position = CacheTransform.position;
            switch (damageType)
            {
                case DamageType.Melee:
                    position = MeleeDamageTransform.position;
                    break;
                case DamageType.Missile:
                    Transform tempMissileDamageTransform = null;
                    if ((tempMissileDamageTransform = CharacterModel.GetRightHandEquipmentEntity()) != null && !isLeftHand)
                    {
                        // Use position from right hand weapon missile damage transform
                        position = tempMissileDamageTransform.position;
                    }
                    else if ((tempMissileDamageTransform = CharacterModel.GetLeftHandEquipmentEntity()) != null && isLeftHand)
                    {
                        // Use position from left hand weapon missile damage transform
                        position = tempMissileDamageTransform.position;
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
                rotation = Quaternion.LookRotation((aimPosition - position).normalized);
        }

        public override void ReceivedDamage(IAttackerEntity attacker, CombatAmountType combatAmountType, int damage)
        {
            base.ReceivedDamage(attacker, combatAmountType, damage);
            InterruptCastingSkill();
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

        public virtual void RewardExp(int exp, RewardGivenType rewardGivenType)
        {
            if (!IsServer)
                return;
            if (!gameInstance.GameplayRule.IncreaseExp(this, exp))
                return;
            // Send OnLevelUp to owner player only
            RequestOnLevelUp();
        }
        #endregion

        #region Inventory Helpers
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
                        if (inventoryType == InventoryType.EquipWeaponLeft && hasLeftHandItem)
                        {
                            shouldUnequipLeftHand = true;
                        }
                        // Unequip item if right hand weapon is one hand or two hand when equipping at left hand
                        if (inventoryType == InventoryType.EquipWeaponLeft && hasRightHandItem)
                        {
                            if (rightHandEquipType == WeaponItemEquipType.OneHand ||
                                rightHandEquipType == WeaponItemEquipType.TwoHand)
                                shouldUnequipRightHand = true;
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
                DamageElement damageElement;
                MinMaxFloat damageAmount;
                float tempReceivingDamage;
                foreach (KeyValuePair<DamageElement, MinMaxFloat> allDamageAmount in allDamageAmounts)
                {
                    damageElement = allDamageAmount.Key;
                    damageAmount = allDamageAmount.Value;
                    // Set hit effect by damage element
                    if (hitEffectsId == 0 && damageElement != gameInstance.DefaultDamageElement)
                        hitEffectsId = damageElement.hitEffects.Id;
                    tempReceivingDamage = damageElement.GetDamageReducedByResistance(this, damageAmount.Random());
                    if (tempReceivingDamage > 0f)
                        calculatingTotalDamage += tempReceivingDamage;
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
                ValidateRecovery(attackerCharacter);
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
                if (entry.NotEmptySlot() && armorItem != null && !equipItemIndexes.ContainsKey(armorItem.EquipPosition))
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

        #region Weapons / Damage
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
            return GetAttackDistance();
        }

        public virtual float GetSkillAttackFov(Skill skill)
        {
            if (skill == null || !skill.IsAttack())
                return 0f;
            if (skill.skillAttackType == SkillAttackType.Normal)
                return skill.damageInfo.GetFov();
            return GetAttackFov();
        }

        public virtual void LaunchDamageEntity(
            bool isLeftHand,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            CharacterBuff debuff,
            uint hitEffectsId,
            bool hasAimPosition,
            Vector3 aimPosition)
        {
            if (!IsServer)
                return;

            IDamageableEntity tempDamageableEntity = null;
            Vector3 damagePosition;
            Quaternion damageRotation;
            GetDamagePositionAndRotation(damageInfo.damageType, isLeftHand, hasAimPosition, aimPosition, out damagePosition, out damageRotation);
#if UNITY_EDITOR
            debugDamagePosition = damagePosition;
            debugDamageRotation = damageRotation;
#endif
            switch (damageInfo.damageType)
            {
                case DamageType.Melee:
                    if (damageInfo.hitOnlySelectedTarget)
                    {
                        if (!TryGetTargetEntity(out tempDamageableEntity))
                        {
                            int tempOverlapSize = OverlapObjects_ForAttackFunctions(damagePosition, damageInfo.hitDistance, gameInstance.GetDamageableLayerMask());
                            if (tempOverlapSize == 0)
                                return;
                            // Target entity not set, use overlapped object as target
                            for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
                            {
                                tempGameObject = GetOverlapObject_ForAttackFunctions(tempLoopCounter);
                                tempDamageableEntity = tempGameObject.GetComponent<IDamageableEntity>();
                                if (tempDamageableEntity != null && (!(tempDamageableEntity is BaseCharacterEntity) || (BaseCharacterEntity)tempDamageableEntity != this))
                                    break;
                            }
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
                        int tempOverlapSize = OverlapObjects_ForAttackFunctions(damagePosition, damageInfo.hitDistance, gameInstance.GetDamageableLayerMask());
                        if (tempOverlapSize == 0)
                            return;
                        // Find characters that receiving damages
                        for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
                        {
                            tempGameObject = GetOverlapObject_ForAttackFunctions(tempLoopCounter);
                            tempDamageableEntity = tempGameObject.GetComponent<IDamageableEntity>();
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
                        GameObject spawnObj = Instantiate(damageInfo.missileDamageEntity.gameObject, damagePosition, damageRotation);
                        MissileDamageEntity missileDamageEntity = spawnObj.GetComponent<MissileDamageEntity>();
                        if (damageInfo.hitOnlySelectedTarget)
                        {
                            if (!TryGetTargetEntity(out tempDamageableEntity))
                                tempDamageableEntity = null;
                        }
                        missileDamageEntity.SetupDamage(this, weapon, allDamageAmounts, debuff, hitEffectsId, damageInfo.missileDistance, damageInfo.missileSpeed, tempDamageableEntity);
                        Manager.Assets.NetworkSpawn(spawnObj);
                    }
                    break;
            }
        }
        #endregion

        #region Allowed abilities
        public virtual bool IsPlayingActionAnimation()
        {
            return animActionType == AnimActionType.AttackRightHand || animActionType == AnimActionType.AttackLeftHand || animActionType == AnimActionType.Skill;
        }

        public virtual bool CanDoActions()
        {
            return !IsDead() && !IsPlayingActionAnimation() && !isAttackingOrUsingSkill;
        }

        public bool CanMove()
        {
            if (IsDead())
                return false;
            if (CacheDisallowMove)
                return false;
            return true;
        }

        public bool CanAttack()
        {
            if (!CanDoActions())
                return false;
            if (CacheDisallowAttack)
                return false;
            return true;
        }

        public bool CanUseSkill()
        {
            if (!CanDoActions())
                return false;
            if (CacheDisallowUseSkill)
                return false;
            return true;
        }

        public bool CanUseItem()
        {
            if (IsDead())
                return false;
            if (CacheDisallowUseItem)
                return false;
            return true;
        }
        #endregion

        #region Find objects helpers
        public int OverlapObjects_ForAttackFunctions(Vector3 position, float distance, int layerMask)
        {
            if (gameInstance.DimensionType == DimensionType.Dimension2D)
                return Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D_ForAttackFunctions, layerMask);
            return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders_ForAttackFunctions, layerMask);
        }

        public GameObject GetOverlapObject_ForAttackFunctions(int index)
        {
            if (gameInstance.DimensionType == DimensionType.Dimension2D)
                return overlapColliders2D_ForAttackFunctions[index].gameObject;
            return overlapColliders_ForAttackFunctions[index].gameObject;
        }

        public int OverlapObjects_ForFindFunctions(Vector3 position, float distance, int layerMask)
        {
            if (gameInstance.DimensionType == DimensionType.Dimension2D)
                return Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D_ForFindFunctions, layerMask);
            return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders_ForFindFunctions, layerMask);
        }

        public GameObject GetOverlapObject_ForFindFunctions(int index)
        {
            if (gameInstance.DimensionType == DimensionType.Dimension2D)
                return overlapColliders2D_ForFindFunctions[index].gameObject;
            return overlapColliders_ForFindFunctions[index].gameObject;
        }

        public bool IsPositionInFov(float fov, Vector3 position)
        {
            return IsPositionInFov(fov, position, CacheTransform.forward);
        }

        public virtual bool IsPositionInFov(float fov, Vector3 position, Vector3 forward)
        {
            float halfFov = fov * 0.5f;
            // This is unsigned angle, so angle found from this function is 0 - 180
            // if position forward from character this value will be 180
            // so just find for angle > 180 - halfFov
            Vector3 targetDir = (position - CacheTransform.position).normalized;
            targetDir.y = 0;
            forward.y = 0;
            targetDir.Normalize();
            forward.Normalize();
            return Vector3.Angle(targetDir, forward) < halfFov;
        }

        public List<T> FindCharacters<T>(float distance, bool findForAliveOnly, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            List<T> result = new List<T>();
            int tempOverlapSize = OverlapObjects_ForFindFunctions(CacheTransform.position, distance, gameInstance.characterLayer.Mask);
            if (tempOverlapSize == 0)
                return null;
            T tempEntity;
            for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
            {
                tempGameObject = GetOverlapObject_ForFindFunctions(tempLoopCounter);
                tempEntity = tempGameObject.GetComponent<T>();
                if (!IsCharacterWhichLookingFor(tempEntity, findForAliveOnly, findForAlly, findForEnemy, findForNeutral, findInFov, fov))
                    continue;
                result.Add(tempEntity);
            }
            return result;
        }

        public List<T> FindAliveCharacters<T>(float distance, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            return FindCharacters<T>(distance, true, findForAlly, findForEnemy, findForNeutral, findInFov, 0);
        }

        public T FindNearestCharacter<T>(float distance, bool findForAliveOnly, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            int tempOverlapSize = OverlapObjects_ForFindFunctions(CacheTransform.position, distance, gameInstance.characterLayer.Mask);
            if (tempOverlapSize == 0)
                return null;
            float tempDistance;
            T tempEntity;
            float nearestDistance = float.MaxValue;
            T nearestEntity = null;
            for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
            {
                tempGameObject = GetOverlapObject_ForFindFunctions(tempLoopCounter);
                tempEntity = tempGameObject.GetComponent<T>();
                if (!IsCharacterWhichLookingFor(tempEntity, findForAliveOnly, findForAlly, findForEnemy, findForNeutral, findInFov, fov))
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

        public T FindNearestAliveCharacter<T>(float distance, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            return FindNearestCharacter<T>(distance, true, findForAlly, findForEnemy, findForNeutral, findInFov, fov);
        }

        private bool IsCharacterWhichLookingFor(BaseCharacterEntity characterEntity, bool findForAlive, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov, float fov)
        {
            if (characterEntity == null || characterEntity == this)
                return false;
            if (findForAlive && characterEntity.IsDead())
                return false;
            if (findInFov && !IsPositionInFov(fov, characterEntity.CacheTransform.position))
                return false;
            return (findForAlly && characterEntity.IsAlly(this)) ||
                (findForEnemy && characterEntity.IsEnemy(this)) ||
                (findForNeutral && characterEntity.IsNeutral(this));
        }
        #endregion

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
