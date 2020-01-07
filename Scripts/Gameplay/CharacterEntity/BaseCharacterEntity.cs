using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterModelManager))]
    [RequireComponent(typeof(CharacterRecoveryComponent))]
    [RequireComponent(typeof(CharacterSkillAndBuffComponent))]
    public abstract partial class BaseCharacterEntity : DamageableEntity, ICharacterData, IGameEntity
    {
        public const float ACTION_DELAY = 0.2f;
        public const float COMBATANT_MESSAGE_DELAY = 1f;
        public const float RESPAWN_GROUNDED_CHECK_DURATION = 1f;
        public const float MOUNT_DELAY = 1f;
        public const int OVERLAP_COLLIDER_SIZE_FOR_ATTACK = 16;
        public const int RAYCAST_SIZE_FOR_ATTACK = 8;
        public const int OVERLAP_COLLIDER_SIZE_FOR_FIND = 32;

        protected struct SyncListRecachingState
        {
            public static readonly SyncListRecachingState Empty = new SyncListRecachingState();
            public bool isRecaching;
            public LiteNetLibSyncList.Operation operation;
            public int index;
        }
        
        [Header("Character Settings")]
        [Tooltip("When character attack with melee weapon, it will cast sphere from this transform to detect hit objects")]
        [SerializeField]
        private Transform meleeDamageTransform;
        public Transform MeleeDamageTransform
        {
            get { return meleeDamageTransform; }
        }

        [Tooltip("When character attack with range weapon, it will spawn missile damage entity from this transform")]
        [SerializeField]
        private Transform missileDamageTransform;
        public Transform MissileDamageTransform
        {
            get { return missileDamageTransform; }
        }

        [Tooltip("Character UI will instantiates to this transform")]
        [SerializeField]
        private Transform characterUITransform;
        public Transform CharacterUITransform
        {
            get { return characterUITransform; }
        }

        [Tooltip("Mini Map UI will instantiates to this transform")]
        [SerializeField]
        private Transform miniMapUITransform;
        public Transform MiniMapUITransform
        {
            get { return miniMapUITransform; }
        }

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
        protected short reloadingAmmoAmount;
        public bool IsAttackingOrUsingSkill { get; protected set; }
        public bool IsCastingSkillCanBeInterrupted { get; protected set; }
        public bool IsCastingSkillInterrupted { get; protected set; }
        public float CastingSkillDuration { get; protected set; }
        public float CastingSkillCountDown { get; protected set; }
        public float MoveSpeedRateWhileAttackOrUseSkill { get; protected set; }
        public float RespawnGroundedCheckCountDown { get; protected set; }
        protected float lastActionTime;
        protected float lastCombatantErrorTime;
        protected float lastMountTime;
        protected readonly Dictionary<int, float> requestUseSkillErrorTime = new Dictionary<int, float>();
        #endregion

        #region Temp data
        protected Collider[] overlapColliders_ForAttackFunctions = new Collider[OVERLAP_COLLIDER_SIZE_FOR_ATTACK];
        protected Collider2D[] overlapColliders2D_ForAttackFunctions = new Collider2D[OVERLAP_COLLIDER_SIZE_FOR_ATTACK];
        protected RaycastHit[] raycasts_ForAttackFunctions = new RaycastHit[RAYCAST_SIZE_FOR_ATTACK];
        protected RaycastHit2D[] raycasts2D_ForAttackFunctions = new RaycastHit2D[RAYCAST_SIZE_FOR_ATTACK];
        protected Collider[] overlapColliders_ForFindFunctions = new Collider[OVERLAP_COLLIDER_SIZE_FOR_FIND];
        protected Collider2D[] overlapColliders2D_ForFindFunctions = new Collider2D[OVERLAP_COLLIDER_SIZE_FOR_FIND];
        protected GameObject tempGameObject;
        #endregion

        public override sealed int MaxHp { get { return this.GetCaches().MaxHp; } }
        public int MaxMp { get { return this.GetCaches().MaxMp; } }
        public int MaxStamina { get { return this.GetCaches().MaxStamina; } }
        public int MaxFood { get { return this.GetCaches().MaxFood; } }
        public int MaxWater { get { return this.GetCaches().MaxWater; } }
        public override sealed float MoveAnimationSpeedMultiplier { get { return GetMoveSpeed(ExtraMovementState.None, false) / this.GetCaches().BaseMoveSpeed; } }
        public override sealed bool MuteFootstepSound { get { return this.GetCaches().MuteFootstepSound; } }
        public abstract int DataId { get; set; }
        public CharacterHitBox[] HitBoxes { get; protected set; }
        
        public CharacterModelManager ModelManager { get; private set; }

        public BaseCharacterModel CharacterModel
        {
            get { return ModelManager.ActiveModel; }
        }

        public BaseCharacterModel FpsModel
        {
            get { return ModelManager.FpsModel; }
        }

        public override void InitialRequiredComponents()
        {
            base.InitialRequiredComponents();
            // Cache components
            if (meleeDamageTransform == null)
                meleeDamageTransform = CacheTransform;
            if (missileDamageTransform == null)
                missileDamageTransform = MeleeDamageTransform;
            if (characterUITransform == null)
                characterUITransform = CacheTransform;
            if (miniMapUITransform == null)
                miniMapUITransform = CacheTransform;
            ModelManager = GetComponent<CharacterModelManager>();
            if (ModelManager == null)
                ModelManager = gameObject.AddComponent<CharacterModelManager>();
            HitBoxes = GetComponentsInChildren<CharacterHitBox>();
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.layer = CurrentGameInstance.characterLayer;
            animActionType = AnimActionType.None;
            isRecaching = true;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
#if UNITY_EDITOR
            ModelManager = GetComponent<CharacterModelManager>();
            if (ModelManager == null)
                ModelManager = gameObject.AddComponent<CharacterModelManager>();
            if (model != ModelManager.ActiveModel)
            {
                model = ModelManager.ActiveModel;
                EditorUtility.SetDirty(this);
            }
#endif
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (debugDamagePosition.HasValue && debugDamageRotation.HasValue)
            {
                float atkHalfFov = GetAttackFov(false) * 0.5f;
                float atkDist = GetAttackDistance(false);
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

        protected override void EntityUpdate()
        {
            MakeCaches();

            if (!lastGrounded && IsGrounded && PassengingVehicleEntity == null)
            {
                // Apply fall damage when not passenging vehicle
                CurrentGameplayRule.ApplyFallDamage(this, lastGroundedPosition);
            }
            lastGrounded = IsGrounded || PassengingVehicleEntity != null;
            if (lastGrounded)
                lastGroundedPosition = CacheTransform.position;

            bool tempEnableMovement = PassengingVehicleEntity == null;
            if (RespawnGroundedCheckCountDown <= 0)
            {
                // Killing character when it fall below dead Y
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D &&
                    BaseGameNetworkManager.CurrentMapInfo != null &&
                    CacheTransform.position.y <= BaseGameNetworkManager.CurrentMapInfo.deadY)
                {
                    if (IsServer && !IsDead())
                    {
                        // Character will dead only when dimension type is 3D
                        CurrentHp = 0;
                        Killed(this);
                    }
                    // Disable movement when character dead
                    tempEnableMovement = false;
                }
            }
            else
            {
                RespawnGroundedCheckCountDown -= Time.deltaTime;
            }

            // Clear data when character dead
            if (IsDead())
            {
                // Clear action states when character dead
                animActionType = AnimActionType.None;
                IsAttackingOrUsingSkill = false;
                InterruptCastingSkill();
                ExitVehicle();
            }

            // Enable movement or not
            if (Movement.Enabled != tempEnableMovement)
            {
                if (!tempEnableMovement)
                    Movement.StopMove();
                // Enable movement while not passenging any vehicle
                Movement.Enabled = tempEnableMovement;
            }

            // Update character model handler based on passenging vehicle
            ModelManager.UpdatePassengingVehicle(PassengingVehicleType, PassengingVehicle.seatIndex);
            // Update current model
            model = ModelManager.ActiveModel;
            // Update casting skill count down, will show gage at clients
            if (CastingSkillCountDown > 0)
            {
                CastingSkillCountDown -= Time.deltaTime;
                if (CastingSkillCountDown < 0)
                    CastingSkillCountDown = 0;
            }
            // Set character model hide state
            ModelManager.SetIsHide(CharacterModelManager.HIDE_SETTER_ENTITY, this.GetCaches().IsHide);
            // Update model animations
            if (IsClient)
            {
                // Update is dead state
                CharacterModel.SetIsDead(IsDead());
                // Update move speed multiplier
                CharacterModel.SetMoveAnimationSpeedMultiplier(MoveAnimationSpeedMultiplier);
                // Update movement animation
                CharacterModel.SetMovementState(MovementState, ExtraMovementState, DirectionType2D, IsUnderWater);
                // Update FPS model
                if (FpsModel != null)
                {
                    // Update is dead state
                    FpsModel.SetIsDead(IsDead());
                    // Update move speed multiplier
                    FpsModel.SetMoveAnimationSpeedMultiplier(MoveAnimationSpeedMultiplier);
                    // Update movement animation
                    FpsModel.SetMovementState(MovementState, ExtraMovementState, DirectionType2D, IsUnderWater);
                }
            }
        }

        #region Relates Objects
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
        public void ValidateRecovery(IGameEntity causer = null)
        {
            if (!IsServer)
                return;

            // Validate Hp
            if (CurrentHp < 0)
                CurrentHp = 0;
            if (CurrentHp > this.GetCaches().MaxHp)
                CurrentHp = this.GetCaches().MaxHp;
            // Validate Mp
            if (CurrentMp < 0)
                CurrentMp = 0;
            if (CurrentMp > this.GetCaches().MaxMp)
                CurrentMp = this.GetCaches().MaxMp;
            // Validate Stamina
            if (CurrentStamina < 0)
                CurrentStamina = 0;
            if (CurrentStamina > this.GetCaches().MaxStamina)
                CurrentStamina = this.GetCaches().MaxStamina;
            // Validate Food
            if (CurrentFood < 0)
                CurrentFood = 0;
            if (CurrentFood > this.GetCaches().MaxFood)
                CurrentFood = this.GetCaches().MaxFood;
            // Validate Water
            if (CurrentWater < 0)
                CurrentWater = 0;
            if (CurrentWater > this.GetCaches().MaxWater)
                CurrentWater = this.GetCaches().MaxWater;

            if (IsDead())
                Killed(causer);
        }

        public void GetDamagePositionAndRotation(DamageType damageType, bool isLeftHand, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Vector3 direction, out Quaternion rotation)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                GetDamagePositionAndRotation2D(damageType, isLeftHand, aimPosition, stagger, out position, out direction, out rotation);
            else
                GetDamagePositionAndRotation3D(damageType, isLeftHand, aimPosition, stagger, out position, out direction, out rotation);
        }

        protected void GetDamagePositionAndRotation2D(DamageType damageType, bool isLeftHand, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Vector3 direction, out Quaternion rotation)
        {
            Transform transform = GetDamageTransform(damageType, isLeftHand);
            position = transform.position;
            direction = Direction2D;
            rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(direction.y, direction.x) * (180 / Mathf.PI)) + 90);
        }

        protected void GetDamagePositionAndRotation3D(DamageType damageType, bool isLeftHand, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Vector3 direction, out Quaternion rotation)
        {
            Transform aimTransform = GetDamageTransform(damageType, isLeftHand);
            position = aimTransform.position;
            Quaternion forwardRotation = Quaternion.LookRotation(aimPosition - position);
            Vector3 forwardStagger = forwardRotation * stagger;
            direction = aimPosition + forwardStagger - position;
            rotation = Quaternion.LookRotation(direction);
        }

        public Transform GetDamageTransform(DamageType damageType, bool isLeftHand)
        {
            Transform transform = null;
            switch (damageType)
            {
                case DamageType.Melee:
                    transform = MeleeDamageTransform;
                    break;
                case DamageType.Missile:
                case DamageType.Raycast:
                    if (ModelManager.IsFps)
                    {
                        if (FpsModel != null)
                        {
                            // Spawn bullets from fps model
                            transform = isLeftHand ? FpsModel.GetLeftHandMissileDamageTransform() : FpsModel.GetRightHandMissileDamageTransform();
                        }
                    }
                    else
                    {
                        // Spawn bullets from tps model
                        transform = isLeftHand ? CharacterModel.GetLeftHandMissileDamageTransform() : CharacterModel.GetRightHandMissileDamageTransform();
                    }

                    if (transform == null)
                    {
                        // Still no missile transform, use default missile transform
                        transform = MissileDamageTransform;
                    }
                    break;
            }
            return transform;
        }

        public virtual void Killed(IGameEntity lastAttacker)
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
            CurrentHp = this.GetCaches().MaxHp;
            CurrentMp = this.GetCaches().MaxMp;
            CurrentStamina = this.GetCaches().MaxStamina;
            CurrentFood = this.GetCaches().MaxFood;
            CurrentWater = this.GetCaches().MaxWater;
            RespawnGroundedCheckCountDown = RESPAWN_GROUNDED_CHECK_DURATION;
            // Send OnRespawn to owner player only
            RequestOnRespawn();
        }

        public void RewardExp(Reward reward, float multiplier, RewardGivenType rewardGivenType)
        {
            if (!IsServer)
                return;
            if (!CurrentGameplayRule.RewardExp(this, reward, multiplier, rewardGivenType))
                return;
            // Send OnLevelUp to owner player only
            RequestOnLevelUp();
        }

        public void RewardCurrencies(Reward reward, float multiplier, RewardGivenType rewardGivenType)
        {
            CurrentGameplayRule.RewardCurrencies(this, reward, multiplier, rewardGivenType);
        }
        #endregion

        #region Inventory Helpers
        public bool CanEquipWeapon(CharacterItem equippingItem, byte equipWeaponSet, bool isLeftHand, out GameMessage.Type gameMessageType, out bool shouldUnequipRightHand, out bool shouldUnequipLeftHand)
        {
            gameMessageType = GameMessage.Type.None;
            shouldUnequipRightHand = false;
            shouldUnequipLeftHand = false;

            if (equippingItem.GetWeaponItem() == null && equippingItem.GetShieldItem() == null)
            {
                gameMessageType = GameMessage.Type.CannotEquip;
                return false;
            }

            if (!equippingItem.GetItem().CanEquip(this, equippingItem.level, out gameMessageType))
                return false;

            this.FillWeaponSetsIfNeeded(equipWeaponSet);
            EquipWeapons tempEquipWeapons = SelectableWeaponSets[equipWeaponSet];

            WeaponItemEquipType rightHandEquipType;
            bool hasRightHandItem =
                tempEquipWeapons.GetRightHandWeaponItem().TryGetWeaponItemEquipType(out rightHandEquipType);
            WeaponItemEquipType leftHandEquipType;
            bool hasLeftHandItem =
                tempEquipWeapons.GetLeftHandWeaponItem().TryGetWeaponItemEquipType(out leftHandEquipType) ||
                tempEquipWeapons.GetLeftHandShieldItem() != null;

            // Equipping item is weapon
            Item equippingWeaponItem = equippingItem.GetWeaponItem();
            if (equippingWeaponItem != null)
            {
                switch (equippingWeaponItem.EquipType)
                {
                    case WeaponItemEquipType.OneHand:
                        // If weapon is one hand its equip position must be right hand
                        if (isLeftHand)
                        {
                            gameMessageType = GameMessage.Type.InvalidEquipPositionRightHand;
                            return false;
                        }
                        // One hand can equip with shield only 
                        // if there are weapons on left hand it should unequip
                        if (hasRightHandItem)
                            shouldUnequipRightHand = true;
                        // Unequip left-hand weapon, don't unequip shield
                        if (hasLeftHandItem && tempEquipWeapons.GetLeftHandWeaponItem() != null)
                            shouldUnequipLeftHand = true;
                        break;
                    case WeaponItemEquipType.OneHandCanDual:
                        // If weapon is one hand can dual its equip position must be right or left hand
                        if (!isLeftHand && hasRightHandItem)
                        {
                            shouldUnequipRightHand = true;
                        }
                        if (isLeftHand && hasLeftHandItem)
                        {
                            shouldUnequipLeftHand = true;
                        }
                        // Unequip item if right hand weapon is one hand or two hand when equipping at left hand
                        if (isLeftHand && hasRightHandItem)
                        {
                            if (rightHandEquipType == WeaponItemEquipType.OneHand ||
                                rightHandEquipType == WeaponItemEquipType.TwoHand)
                                shouldUnequipRightHand = true;
                        }
                        break;
                    case WeaponItemEquipType.TwoHand:
                        // If weapon is one hand its equip position must be right hand
                        if (isLeftHand)
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
            Item equippingShieldItem = equippingItem.GetShieldItem();
            if (equippingShieldItem != null)
            {
                // If it is shield, its equip position must be left hand
                if (!isLeftHand)
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
            gameMessageType = GameMessage.Type.CannotEquip;
            return false;
        }

        public bool CanEquipItem(CharacterItem equippingItem, byte equipSlotIndex, out GameMessage.Type gameMessageType, out short unEquippingIndex)
        {
            gameMessageType = GameMessage.Type.None;
            unEquippingIndex = -1;

            if (equippingItem.GetArmorItem() == null)
            {
                gameMessageType = GameMessage.Type.CannotEquip;
                return false;
            }

            if (!equippingItem.GetItem().CanEquip(this, equippingItem.level, out gameMessageType))
                return false;

            // Equipping item is armor
            Item equippingArmorItem = equippingItem.GetArmorItem();
            if (equippingArmorItem != null)
            {
                unEquippingIndex = (short)this.IndexOfEquipItemByEquipPosition(equippingArmorItem.EquipPosition, equipSlotIndex);
                return true;
            }
            gameMessageType = GameMessage.Type.CannotEquip;
            return false;
        }

        public override void ReceiveDamage(IGameEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel)
        {
            if (!IsServer || IsDead() || !CanReceiveDamageFrom(attacker))
                return;

            if (HitBoxes != null && HitBoxes.Length > 0)
            {
                // Character have hit boxes, let's hit boxes handle damages, so skip receive damage function here
                return;
            }

            ReceiveDamageFunction(attacker, weapon, damageAmounts, skill, skillLevel);
        }

        internal void ReceiveDamageFunction(IGameEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel)
        {
            base.ReceiveDamage(attacker, weapon, damageAmounts, skill, skillLevel);

            BaseCharacterEntity attackerCharacter = null;
            if (attacker != null)
                attackerCharacter = attacker.Entity as BaseCharacterEntity;

            // Notify enemy spotted when received damage from enemy
            NotifyEnemySpottedToAllies(attackerCharacter);

            // Notify enemy spotted when damage taken to enemy
            attackerCharacter.NotifyEnemySpottedToAllies(this);

            bool isCritical = false;
            bool isBlocked = false;
            if (attackerCharacter != null)
            {
                // Calculate chance to critical
                isCritical = Random.value <= CurrentGameInstance.GameplayRule.GetCriticalChance(attackerCharacter, this);

                // If miss, return don't calculate damages
                if (!isCritical && Random.value > CurrentGameInstance.GameplayRule.GetHitChance(attackerCharacter, this))
                {
                    ReceivedDamage(attackerCharacter, CombatAmountType.Miss, 0);
                    return;
                }

                isBlocked = Random.value <= CurrentGameInstance.GameplayRule.GetBlockChance(attackerCharacter, this);
            }

            // Calculate damages
            float calculatingTotalDamage = 0f;
            if (damageAmounts != null && damageAmounts.Count > 0)
            {
                MinMaxFloat damageAmount;
                float tempReceivingDamage;
                foreach (DamageElement damageElement in damageAmounts.Keys)
                {
                    damageAmount = damageAmounts[damageElement];
                    tempReceivingDamage = damageElement.GetDamageReducedByResistance(this, damageAmount.Random());
                    if (tempReceivingDamage > 0f)
                        calculatingTotalDamage += tempReceivingDamage;
                }
            }

            if (attackerCharacter != null)
            {
                // If critical occurs
                if (isCritical)
                    calculatingTotalDamage = CurrentGameInstance.GameplayRule.GetCriticalDamage(attackerCharacter, this, calculatingTotalDamage);

                // If block occurs
                if (isBlocked)
                    calculatingTotalDamage = CurrentGameInstance.GameplayRule.GetBlockDamage(attackerCharacter, this, calculatingTotalDamage);
            }

            // Apply damages
            int totalDamage = (int)calculatingTotalDamage;
            CurrentHp -= totalDamage;

            CombatAmountType combatAmountType = CombatAmountType.NormalDamage;
            if (isBlocked)
                combatAmountType = CombatAmountType.BlockedDamage;
            else if (isCritical)
                combatAmountType = CombatAmountType.CriticalDamage;
            ReceivedDamage(attacker, combatAmountType, totalDamage);

            // Decrease equipment durability
            CurrentGameInstance.GameplayRule.OnCharacterReceivedDamage(attackerCharacter, this, combatAmountType, totalDamage);

            // Interrupt casting skill when receive damage
            InterruptCastingSkill();

            // Only TPS model will plays hit animation
            CharacterModel.PlayHitAnimation();

            // If current hp <= 0, character dead
            if (IsDead())
            {
                // Call killed function, this should be called only once when dead
                ValidateRecovery(attacker);
            }
            else
            {
                // Apply debuff if character is not dead
                if (skill != null && skill.IsDebuff())
                    ApplyBuff(skill.DataId, BuffType.SkillDebuff, skillLevel, attacker);
            }
        }
        #endregion

        #region Keys indexes update functions
        protected void UpdateEquipItemIndexes()
        {
            equipItemIndexes.Clear();
            CharacterItem tempEquipItem;
            Item tempArmor;
            string tempEquipPosition;
            for (int i = 0; i < equipItems.Count; ++i)
            {
                tempEquipItem = equipItems[i];
                tempArmor = tempEquipItem.GetArmorItem();
                if (tempEquipItem.IsEmptySlot() || tempArmor == null)
                    continue;

                tempEquipPosition = GetEquipPosition(tempArmor.EquipPosition, tempEquipItem.equipSlotIndex);
                if (equipItemIndexes.ContainsKey(tempEquipPosition))
                    continue;

                equipItemIndexes[tempEquipPosition] = i;
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
        public virtual CrosshairSetting GetCrosshairSetting()
        {
            Item rightWeaponItem = EquipWeapons.GetRightHandWeaponItem();
            Item leftWeaponItem = EquipWeapons.GetLeftHandWeaponItem();
            if (rightWeaponItem != null && leftWeaponItem != null)
            {
                // Create new crosshair setting based on weapons
                return new CrosshairSetting()
                {
                    hidden = rightWeaponItem.crosshairSetting.hidden || leftWeaponItem.crosshairSetting.hidden,
                    expandPerFrameWhileMoving = (rightWeaponItem.crosshairSetting.expandPerFrameWhileMoving + leftWeaponItem.crosshairSetting.expandPerFrameWhileMoving) / 2f,
                    expandPerFrameWhileAttacking = (rightWeaponItem.crosshairSetting.expandPerFrameWhileAttacking + leftWeaponItem.crosshairSetting.expandPerFrameWhileAttacking) / 2f,
                    shrinkPerFrame = (rightWeaponItem.crosshairSetting.shrinkPerFrame + leftWeaponItem.crosshairSetting.shrinkPerFrame) / 2f,
                    minSpread = (rightWeaponItem.crosshairSetting.minSpread + leftWeaponItem.crosshairSetting.minSpread) / 2f,
                    maxSpread = (rightWeaponItem.crosshairSetting.maxSpread + leftWeaponItem.crosshairSetting.maxSpread) / 2f
                };
            }
            if (rightWeaponItem != null)
                return rightWeaponItem.crosshairSetting;
            if (leftWeaponItem != null)
                return leftWeaponItem.crosshairSetting;
            return CurrentGameInstance.DefaultWeaponItem.crosshairSetting;
        }

        public virtual float GetAttackDistance(bool isLeftHand)
        {
            Item rightWeaponItem = EquipWeapons.GetRightHandWeaponItem();
            Item leftWeaponItem = EquipWeapons.GetLeftHandWeaponItem();
            if (!isLeftHand)
            {
                if (rightWeaponItem != null)
                    return rightWeaponItem.WeaponType.damageInfo.GetDistance();
                if (rightWeaponItem == null && leftWeaponItem != null)
                    return leftWeaponItem.WeaponType.damageInfo.GetDistance();
            }
            else
            {
                if (leftWeaponItem != null)
                    return leftWeaponItem.WeaponType.damageInfo.GetDistance();
                if (leftWeaponItem == null && rightWeaponItem != null)
                    return rightWeaponItem.WeaponType.damageInfo.GetDistance();
            }
            return CurrentGameInstance.DefaultWeaponItem.WeaponType.damageInfo.GetDistance();
        }

        public virtual float GetAttackFov(bool isLeftHand)
        {
            Item rightWeaponItem = EquipWeapons.GetRightHandWeaponItem();
            Item leftWeaponItem = EquipWeapons.GetLeftHandWeaponItem();
            if (!isLeftHand)
            {
                if (rightWeaponItem != null)
                    return rightWeaponItem.WeaponType.damageInfo.GetFov();
                if (rightWeaponItem == null && leftWeaponItem != null)
                    return leftWeaponItem.WeaponType.damageInfo.GetFov();
            }
            else
            {
                if (leftWeaponItem != null)
                    return leftWeaponItem.WeaponType.damageInfo.GetFov();
                if (leftWeaponItem == null && rightWeaponItem != null)
                    return rightWeaponItem.WeaponType.damageInfo.GetFov();
            }
            return CurrentGameInstance.DefaultWeaponItem.WeaponType.damageInfo.GetFov();
        }

        /// <summary>
        /// This function can be called at both client and server
        /// For server it will instantiates damage entities if needed
        /// For client it will instantiates special effects
        /// </summary>
        /// <param name="isLeftHand"></param>
        /// <param name="weapon"></param>
        /// <param name="damageInfo"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="debuff"></param>
        /// <param name="aimPosition"></param>
        /// <param name="stagger"></param>
        public virtual void LaunchDamageEntity(
            bool isLeftHand,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            short skillLevel,
            Vector3 aimPosition,
            Vector3 stagger)
        {
            IDamageableEntity tempDamageableEntity = null;
            Vector3 damagePosition;
            Vector3 damageDirection;
            Quaternion damageRotation;
            GetDamagePositionAndRotation(damageInfo.damageType, isLeftHand, aimPosition, stagger, out damagePosition, out damageDirection, out damageRotation);
#if UNITY_EDITOR
            debugDamagePosition = damagePosition;
            debugDamageRotation = damageRotation;
#endif
            switch (damageInfo.damageType)
            {
                case DamageType.Melee:
                    if (damageInfo.hitOnlySelectedTarget)
                    {
                        IDamageableEntity damageTakenTarget = null;
                        IDamageableEntity selectedTarget = null;
                        bool hasSelectedTarget = TryGetTargetEntity(out selectedTarget);
                        // If hit only selected target, find selected character (only 1 character) to apply damage
                        int tempOverlapSize = OverlapObjects_ForAttackFunctions(damagePosition, damageInfo.hitDistance, CurrentGameInstance.GetDamageableLayerMask());
                        if (tempOverlapSize == 0)
                            return;
                        // Find characters that receiving damages
                        for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
                        {
                            tempGameObject = GetOverlapObject_ForAttackFunctions(tempLoopCounter);
                            tempDamageableEntity = tempGameObject.GetComponent<IDamageableEntity>();
                            if (tempDamageableEntity == null || tempDamageableEntity.Entity == this ||
                                tempDamageableEntity.IsDead() || !tempDamageableEntity.CanReceiveDamageFrom(this) ||
                                !IsPositionInFov(damageInfo.hitFov, tempDamageableEntity.transform.position))
                            {
                                // Entity can't receive damage, so skip it.
                                continue;
                            }

                            // Set damage taken target, this entity will receives damages
                            damageTakenTarget = tempDamageableEntity;

                            if (hasSelectedTarget && selectedTarget.Entity == tempDamageableEntity.Entity)
                            {
                                // This is selected target, so this is character which must receives damages
                                break;
                            }
                        }
                        // Only 1 target will receives damages
                        if (damageTakenTarget != null)
                        {
                            // Pass all receive damage condition, then apply damages
                            if (IsServer)
                                damageTakenTarget.ReceiveDamage(this, weapon, damageAmounts, skill, skillLevel);
                            if (IsClient)
                                damageTakenTarget.PlayHitEffects(damageAmounts.Keys, skill);
                        }
                    }
                    else
                    {
                        // If not hit only selected target, find characters within hit fov to applies damages
                        int tempOverlapSize = OverlapObjects_ForAttackFunctions(damagePosition, damageInfo.hitDistance, CurrentGameInstance.GetDamageableLayerMask());
                        if (tempOverlapSize == 0)
                            return;
                        // Find characters that receiving damages
                        for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
                        {
                            tempGameObject = GetOverlapObject_ForAttackFunctions(tempLoopCounter);
                            tempDamageableEntity = tempGameObject.GetComponent<IDamageableEntity>();
                            if (tempDamageableEntity == null || tempDamageableEntity.Entity == this ||
                                tempDamageableEntity.IsDead() || !tempDamageableEntity.CanReceiveDamageFrom(this) ||
                                !IsPositionInFov(damageInfo.hitFov, tempDamageableEntity.transform.position))
                            {
                                // Entity can't receive damage, so skip it.
                                continue;
                            }
                            // Target receives damages
                            if (IsServer)
                                tempDamageableEntity.ReceiveDamage(this, weapon, damageAmounts, skill, skillLevel);
                            if (IsClient)
                                tempDamageableEntity.PlayHitEffects(damageAmounts.Keys, skill);
                        }
                    }
                    break;
                case DamageType.Missile:
                    // Spawn missile damage entity, it will move to target then apply damage when hit
                    // Instantiates on both client and server (damage applies at server)
                    if (damageInfo.missileDamageEntity != null)
                    {
                        MissileDamageEntity missileDamageEntity = Instantiate(damageInfo.missileDamageEntity, damagePosition, damageRotation);
                        if (damageInfo.hitOnlySelectedTarget)
                        {
                            if (!TryGetTargetEntity(out tempDamageableEntity))
                                tempDamageableEntity = null;
                        }
                        missileDamageEntity.Setup(this, weapon, damageAmounts, skill, skillLevel, damageInfo.missileDistance, damageInfo.missileSpeed, tempDamageableEntity);
                    }
                    break;
                case DamageType.Raycast:
                    float minDistance = damageInfo.missileDistance;
                    // Just raycast to any entity to apply damage
                    int tempRaycastSize = RaycastObjects_ForAttackFunctions(damagePosition, damageDirection, damageInfo.missileDistance, Physics.DefaultRaycastLayers);
                    if (tempRaycastSize > 0)
                    {
                        // Sort index
                        Vector3 point;
                        Vector3 normal;
                        float distance;
                        Transform tempHitTransform;
                        // Find characters that receiving damages
                        for (int tempLoopCounter = 0; tempLoopCounter < tempRaycastSize; ++tempLoopCounter)
                        {
                            tempHitTransform = GetRaycastObject_ForAttackFunctions(tempLoopCounter, out point, out normal, out distance);
                            if (distance < minDistance)
                                minDistance = distance;
                            tempDamageableEntity = tempHitTransform.GetComponent<IDamageableEntity>();
                            if (tempDamageableEntity != null)
                            {
                                if (tempDamageableEntity.Entity == this)
                                    continue;

                                // Target receives damages
                                if (!tempDamageableEntity.IsDead() && tempDamageableEntity.CanReceiveDamageFrom(this))
                                {
                                    if (IsServer)
                                        tempDamageableEntity.ReceiveDamage(this, weapon, damageAmounts, skill, skillLevel);
                                    if (IsClient)
                                        tempDamageableEntity.PlayHitEffects(damageAmounts.Keys, skill);
                                }
                            }
                            else
                            {
                                // Hit wall... so break the loop
                                break;
                            }
                        } // End of for...loop (raycast result)
                    }
                    // Spawn projectile effect, it will move to target but it won't apply damage because it is just effect
                    if (IsClient && damageInfo.projectileEffect != null)
                    {
                        Instantiate(damageInfo.projectileEffect, damagePosition, damageRotation)
                            .Setup(minDistance, damageInfo.missileSpeed);
                    }
                    break;
            }
        }
        #endregion

        #region Allowed abilities
        public virtual bool IsPlayingActionAnimation()
        {
            return animActionType == AnimActionType.AttackRightHand ||
                animActionType == AnimActionType.AttackLeftHand ||
                animActionType == AnimActionType.SkillRightHand ||
                animActionType == AnimActionType.ReloadRightHand ||
                animActionType == AnimActionType.ReloadLeftHand;
        }

        public virtual bool CanDoActions()
        {
            return !IsDead() && !IsPlayingActionAnimation() && !IsAttackingOrUsingSkill;
        }

        public float GetAttackSpeed()
        {
            float atkSpeed = this.GetCaches().AtkSpeed;
            // Minimum attack speed is 0.1
            if (atkSpeed <= 0.1f)
                atkSpeed = 0.1f;
            return atkSpeed;
        }

        protected float GetMoveSpeed(ExtraMovementState extraMovementState, bool isUnderWater)
        {
            float moveSpeed = this.GetCaches().MoveSpeed;

            if (IsAttackingOrUsingSkill)
                moveSpeed *= MoveSpeedRateWhileAttackOrUseSkill;

            if (isUnderWater)
            {
                moveSpeed *= CurrentGameplayRule.GetSwimMoveSpeedRate(this);
            }
            else
            {
                switch (extraMovementState)
                {
                    case ExtraMovementState.IsSprinting:
                        moveSpeed *= CurrentGameplayRule.GetSprintMoveSpeedRate(this);
                        break;
                    case ExtraMovementState.IsCrouching:
                        moveSpeed *= CurrentGameplayRule.GetCrouchMoveSpeedRate(this);
                        break;
                    case ExtraMovementState.IsCrawling:
                        moveSpeed *= CurrentGameplayRule.GetCrawlMoveSpeedRate(this);
                        break;
                }
            }

            return moveSpeed;
        }

        public override float GetMoveSpeed()
        {
            return GetMoveSpeed(ExtraMovementState, IsUnderWater);
        }

        public override sealed bool CanMove()
        {
            if (IsDead())
                return false;
            if (this.GetCaches().DisallowMove)
                return false;
            return true;
        }

        public override sealed bool CanSprint()
        {
            if (IsUnderWater)
                return false;
            return CurrentStamina > 0;
        }

        public override sealed bool CanCrouch()
        {
            if (IsUnderWater)
                return false;
            return true;
        }

        public override sealed bool CanCrawl()
        {
            if (IsUnderWater)
                return false;
            return true;
        }

        public bool CanAttack()
        {
            if (!CanDoActions())
                return false;
            if (this.GetCaches().DisallowAttack)
                return false;
            if (PassengingVehicleEntity != null &&
                !PassengingVehicleSeat.canAttack)
                return false;
            return true;
        }

        public bool CanUseSkill()
        {
            if (!CanDoActions())
                return false;
            if (this.GetCaches().DisallowUseSkill)
                return false;
            if (PassengingVehicleEntity != null &&
                !PassengingVehicleSeat.canUseSkill)
                return false;
            return true;
        }

        public bool CanUseItem()
        {
            if (IsDead())
                return false;
            if (this.GetCaches().DisallowUseItem)
                return false;
            return true;
        }
        #endregion

        #region Data helpers
        private string GetEquipPosition(string equipPositionId, byte equipSlotIndex)
        {
            return equipPositionId + ":" + equipSlotIndex;
        }
        #endregion

        #region Find objects helpers
        public int RaycastObjects_ForAttackFunctions(Vector3 origin, Vector3 direction, float distance, int layerMask)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return PhysicUtils.SortedRaycastNonAlloc2D(origin, direction, raycasts2D_ForAttackFunctions, distance, layerMask);
            return PhysicUtils.SortedRaycastNonAlloc3D(origin, direction, raycasts_ForAttackFunctions, distance, layerMask);
        }

        public Transform GetRaycastObject_ForAttackFunctions(int index, out Vector3 point, out Vector3 normal, out float distance)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
            {
                point = raycasts2D_ForAttackFunctions[index].point;
                normal = raycasts2D_ForAttackFunctions[index].normal;
                distance = raycasts2D_ForAttackFunctions[index].distance;
                return raycasts2D_ForAttackFunctions[index].transform;
            }
            point = raycasts_ForAttackFunctions[index].point;
            normal = raycasts_ForAttackFunctions[index].normal;
            distance = raycasts_ForAttackFunctions[index].distance;
            return raycasts_ForAttackFunctions[index].transform;
        }

        public int OverlapObjects_ForAttackFunctions(Vector3 position, float distance, int layerMask)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D_ForAttackFunctions, layerMask);
            return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders_ForAttackFunctions, layerMask);
        }

        public GameObject GetOverlapObject_ForAttackFunctions(int index)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return overlapColliders2D_ForAttackFunctions[index].gameObject;
            return overlapColliders_ForAttackFunctions[index].gameObject;
        }

        public int OverlapObjects_ForFindFunctions(Vector3 position, float distance, int layerMask)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D_ForFindFunctions, layerMask);
            return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders_ForFindFunctions, layerMask);
        }

        public GameObject GetOverlapObject_ForFindFunctions(int index)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return overlapColliders2D_ForFindFunctions[index].gameObject;
            return overlapColliders_ForFindFunctions[index].gameObject;
        }

        public bool IsPositionInFov(float fov, Vector3 position)
        {
            return IsPositionInFov(fov, position, CacheTransform.forward);
        }

        public bool IsPositionInFov(float fov, Vector3 position, Vector3 forward)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return IsPositionInFov2D(fov, position, forward);
            return IsPositionInFov3D(fov, position, forward);
        }

        protected bool IsPositionInFov2D(float fov, Vector3 position, Vector3 forward)
        {
            float halfFov = fov * 0.5f;
            Vector2 targetDir = (position - CacheTransform.position).normalized;
            float angle = Vector2.Angle(targetDir, Direction2D);
            // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
            return angle < halfFov;
        }

        protected bool IsPositionInFov3D(float fov, Vector3 position, Vector3 forward)
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
            int tempOverlapSize = OverlapObjects_ForFindFunctions(CacheTransform.position, distance, CurrentGameInstance.characterLayer.Mask);
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
            int tempOverlapSize = OverlapObjects_ForFindFunctions(CacheTransform.position, distance, CurrentGameInstance.characterLayer.Mask);
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

        #region Animation helpers
        public void GetRandomAnimationData(
            AnimActionType animActionType,
            int skillOrWeaponTypeDataId,
            out int animationIndex,
            out float[] triggerDurations,
            out float totalDuration)
        {
            animationIndex = 0;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            // Random animation
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    CharacterModel.GetRandomRightHandAttackAnimation(skillOrWeaponTypeDataId, out animationIndex, out triggerDurations, out totalDuration);
                    break;
                case AnimActionType.AttackLeftHand:
                    CharacterModel.GetRandomLeftHandAttackAnimation(skillOrWeaponTypeDataId, out animationIndex, out triggerDurations, out totalDuration);
                    break;
                case AnimActionType.SkillRightHand:
                case AnimActionType.SkillLeftHand:
                    CharacterModel.GetSkillActivateAnimation(skillOrWeaponTypeDataId, out triggerDurations, out totalDuration);
                    break;
            }
        }

        public void GetAnimationData(
            AnimActionType animActionType,
            int skillOrWeaponTypeDataId,
            int animationIndex,
            out float[] triggerDurations,
            out float totalDuration)
        {
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            // Random animation
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    CharacterModel.GetRightHandAttackAnimation(skillOrWeaponTypeDataId, animationIndex, out triggerDurations, out totalDuration);
                    break;
                case AnimActionType.AttackLeftHand:
                    CharacterModel.GetLeftHandAttackAnimation(skillOrWeaponTypeDataId, animationIndex, out triggerDurations, out totalDuration);
                    break;
                case AnimActionType.SkillRightHand:
                case AnimActionType.SkillLeftHand:
                    CharacterModel.GetSkillActivateAnimation(skillOrWeaponTypeDataId, out triggerDurations, out totalDuration);
                    break;
            }
        }

        public float GetAnimSpeedRate(AnimActionType animActionType)
        {
            if (animActionType == AnimActionType.AttackRightHand ||
                animActionType == AnimActionType.AttackLeftHand)
                return GetAttackSpeed();
            return 1f;
        }

        public float GetMoveSpeedRateWhileAttackOrUseSkill(AnimActionType animActionType, BaseSkill skill)
        {
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    if (EquipWeapons.GetRightHandWeaponItem() != null)
                        return EquipWeapons.GetRightHandWeaponItem().moveSpeedRateWhileAttacking;
                    return CurrentGameInstance.DefaultWeaponItem.moveSpeedRateWhileAttacking;
                case AnimActionType.AttackLeftHand:
                    if (EquipWeapons.GetLeftHandWeaponItem() != null)
                        return EquipWeapons.GetLeftHandWeaponItem().moveSpeedRateWhileAttacking;
                    return CurrentGameInstance.DefaultWeaponItem.moveSpeedRateWhileAttacking;
                case AnimActionType.SkillRightHand:
                case AnimActionType.SkillLeftHand:
                    // Calculate move speed rate while doing action at clients and server
                    if (skill != null)
                        return skill.moveSpeedRateWhileUsingSkill;
                    break;
            }
            return 1f;
        }
        #endregion

        protected virtual void NotifyEnemySpottedToAllies(BaseCharacterEntity enemy)
        {
            foreach (CharacterSummon summon in Summons)
            {
                if (summon.CacheEntity == null)
                    continue;
                summon.CacheEntity.NotifyEnemySpotted(this, enemy);
            }
        }

        public virtual Vector3 GetSummonPosition()
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return CacheTransform.position + new Vector3(Random.Range(CurrentGameInstance.minSummonDistance, CurrentGameInstance.maxSummonDistance) * GenericUtils.GetNegativePositive(), Random.Range(CurrentGameInstance.minSummonDistance, CurrentGameInstance.maxSummonDistance) * GenericUtils.GetNegativePositive(), 0f);
            return CacheTransform.position + new Vector3(Random.Range(CurrentGameInstance.minSummonDistance, CurrentGameInstance.maxSummonDistance) * GenericUtils.GetNegativePositive(), 0f, Random.Range(CurrentGameInstance.minSummonDistance, CurrentGameInstance.maxSummonDistance) * GenericUtils.GetNegativePositive());
        }

        public virtual Quaternion GetSummonRotation()
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return Quaternion.identity;
            return CacheTransform.rotation;
        }

        public bool IsNeutral(BaseCharacterEntity characterEntity)
        {
            return !IsAlly(characterEntity) && !IsEnemy(characterEntity);
        }

        public override sealed bool CanReceiveDamageFrom(IGameEntity attacker)
        {
            if (IsInSafeArea)
            {
                // If this character is in safe area it will not receives damages
                return false;
            }

            if (attacker == null || attacker.Entity == null)
            {
                // If attacker is unknow entity, can receive damages
                return true;
            }

            if (attacker.Entity.IsInSafeArea)
            {
                // If attacker is in safe area, it will not receives damages
                return false;
            }

            // If this character is not ally so it is enemy and also can receive damage
            return !IsAlly(attacker.Entity as BaseCharacterEntity);
        }

        public abstract void NotifyEnemySpotted(BaseCharacterEntity ally, BaseCharacterEntity attacker);
        public abstract bool IsAlly(BaseCharacterEntity characterEntity);
        public abstract bool IsEnemy(BaseCharacterEntity characterEntity);
    }
}
