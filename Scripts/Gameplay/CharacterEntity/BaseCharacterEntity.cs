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
    public abstract partial class BaseCharacterEntity : DamageableEntity, ICharacterData
    {
        public const float ACTION_DELAY = 0.1f;
        public const float COMBATANT_MESSAGE_DELAY = 1f;
        public const float RESPAWN_GROUNDED_CHECK_DURATION = 1f;
        public const float MOUNT_DELAY = 1f;

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
        protected readonly Dictionary<string, int> equipItemIndexes = new Dictionary<string, int>();
        protected AnimActionType animActionType;
        protected short reloadingAmmoAmount;
        public bool IsHideOrDead { get { return this.GetCaches().IsHide || IsDead(); } }
        public bool IsAttackingOrUsingSkill { get; protected set; }
        public float MoveSpeedRateWhileAttackOrUseSkill { get; protected set; }
        public float RespawnGroundedCheckCountDown { get; protected set; }
        protected float lastMountTime;
        protected float lastActionTime;
        protected float lastCombatantErrorTime;
        protected readonly Dictionary<int, float> requestUseSkillErrorTime = new Dictionary<int, float>();
        #endregion

        #region Temp data
        protected Collider[] overlapColliders_ForAttackFunctions;
        protected Collider2D[] overlapColliders2D_ForAttackFunctions;
        protected RaycastHit[] raycasts_ForAttackFunctions;
        protected RaycastHit2D[] raycasts2D_ForAttackFunctions;
        protected Collider[] overlapColliders_ForFindFunctions;
        protected Collider2D[] overlapColliders2D_ForFindFunctions;
        #endregion

        public override sealed int MaxHp { get { return this.GetCaches().MaxHp; } }
        public int MaxMp { get { return this.GetCaches().MaxMp; } }
        public int MaxStamina { get { return this.GetCaches().MaxStamina; } }
        public int MaxFood { get { return this.GetCaches().MaxFood; } }
        public int MaxWater { get { return this.GetCaches().MaxWater; } }
        public override sealed float MoveAnimationSpeedMultiplier { get { return this.GetCaches().BaseMoveSpeed > 0f ? GetMoveSpeed(MovementState, ExtraMovementState.None) / this.GetCaches().BaseMoveSpeed : 1f; } }
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
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
            {
                overlapColliders2D_ForAttackFunctions = new Collider2D[512];
                raycasts2D_ForAttackFunctions = new RaycastHit2D[512];
                overlapColliders2D_ForFindFunctions = new Collider2D[512];
            }
            else
            {
                overlapColliders_ForAttackFunctions = new Collider[512];
                raycasts_ForAttackFunctions = new RaycastHit[512];
                overlapColliders_ForFindFunctions = new Collider[512];
            }
            animActionType = AnimActionType.None;
            isRecaching = true;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;
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
            if (IsServer && CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
                // Ground check / ground damage will be calculated at server while dimension type is 3d only
                if (!lastGrounded && MovementState.HasFlag(MovementState.IsGrounded))
                {
                    // Apply fall damage when not passenging vehicle
                    CurrentGameplayRule.ApplyFallDamage(this, lastGroundedPosition);
                }
                lastGrounded = MovementState.HasFlag(MovementState.IsGrounded);
                if (lastGrounded)
                    lastGroundedPosition = CacheTransform.position;
            }

            bool tempEnableMovement = PassengingVehicleEntity == null;
            if (RespawnGroundedCheckCountDown <= 0)
            {
                // Killing character when it fall below dead Y
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D &&
                    CurrentMapInfo != null && CacheTransform.position.y <= CurrentMapInfo.DeadY)
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
                CharacterModel.SetMovementState(MovementState, ExtraMovementState, Direction2D);
                // Update FPS model
                if (FpsModel && FpsModel.gameObject.activeSelf)
                {
                    // Update is dead state
                    FpsModel.SetIsDead(IsDead());
                    // Update move speed multiplier
                    FpsModel.SetMoveAnimationSpeedMultiplier(MoveAnimationSpeedMultiplier);
                    // Update movement animation
                    FpsModel.SetMovementState(MovementState, ExtraMovementState, Direction2D);
                }
            }
        }

        protected override void OnTeleport(Vector3 position)
        {
            base.OnTeleport(position);
            // Clear target entity when teleport
            SetTargetEntity(null);
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
            CurrentGameManager.SendNotifyRewardExp(ConnectionId, reward.exp);
            if (!CurrentGameplayRule.RewardExp(this, reward, multiplier, rewardGivenType))
                return;
            // Send OnLevelUp to owner player only
            RequestOnLevelUp();
        }

        public void RewardCurrencies(Reward reward, float multiplier, RewardGivenType rewardGivenType)
        {
            if (!IsServer)
                return;
            CurrentGameManager.SendNotifyRewardGold(ConnectionId, reward.gold);
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

            if (!equippingItem.GetEquipmentItem().CanEquip(this, equippingItem.level, out gameMessageType))
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
            IWeaponItem equippingWeaponItem = equippingItem.GetWeaponItem();
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
            IShieldItem equippingShieldItem = equippingItem.GetShieldItem();
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

        public bool CanEquipItem(CharacterItem equippingItem, byte equipSlotIndex, out GameMessage.Type gameMessageType, out int unEquippingIndex)
        {
            gameMessageType = GameMessage.Type.None;
            unEquippingIndex = -1;

            if (equippingItem.GetArmorItem() == null)
            {
                gameMessageType = GameMessage.Type.CannotEquip;
                return false;
            }

            if (!equippingItem.GetEquipmentItem().CanEquip(this, equippingItem.level, out gameMessageType))
                return false;

            // Equipping item is armor
            IArmorItem equippingArmorItem = equippingItem.GetArmorItem();
            if (equippingArmorItem != null)
            {
                unEquippingIndex = this.IndexOfEquipItemByEquipPosition(equippingArmorItem.EquipPosition, equipSlotIndex);
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
            IArmorItem tempArmor;
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
        public void SetTargetEntity(BaseGameEntity entity)
        {
            if (entity == null)
            {
                targetEntityId.Value = 0;
                return;
            }
            targetEntityId.Value = entity.ObjectId;
        }

        public BaseGameEntity GetTargetEntity()
        {
            BaseGameEntity entity;
            if (targetEntityId.Value == 0 || !Manager.Assets.TryGetSpawnedObject(targetEntityId.Value, out entity))
                return null;
            return entity;
        }

        public bool TryGetTargetEntity<T>(out T entity) where T : class
        {
            entity = null;
            if (GetTargetEntity() == null)
                return false;
            entity = GetTargetEntity() as T;
            return entity != null;
        }
        #endregion

        #region Weapons / Damage
        public virtual CrosshairSetting GetCrosshairSetting()
        {
            IWeaponItem rightWeaponItem = EquipWeapons.GetRightHandWeaponItem();
            IWeaponItem leftWeaponItem = EquipWeapons.GetLeftHandWeaponItem();
            if (rightWeaponItem != null && leftWeaponItem != null)
            {
                // Create new crosshair setting based on weapons
                return new CrosshairSetting()
                {
                    hidden = rightWeaponItem.CrosshairSetting.hidden || leftWeaponItem.CrosshairSetting.hidden,
                    expandPerFrameWhileMoving = (rightWeaponItem.CrosshairSetting.expandPerFrameWhileMoving + leftWeaponItem.CrosshairSetting.expandPerFrameWhileMoving) / 2f,
                    expandPerFrameWhileAttacking = (rightWeaponItem.CrosshairSetting.expandPerFrameWhileAttacking + leftWeaponItem.CrosshairSetting.expandPerFrameWhileAttacking) / 2f,
                    shrinkPerFrame = (rightWeaponItem.CrosshairSetting.shrinkPerFrame + leftWeaponItem.CrosshairSetting.shrinkPerFrame) / 2f,
                    minSpread = (rightWeaponItem.CrosshairSetting.minSpread + leftWeaponItem.CrosshairSetting.minSpread) / 2f,
                    maxSpread = (rightWeaponItem.CrosshairSetting.maxSpread + leftWeaponItem.CrosshairSetting.maxSpread) / 2f
                };
            }
            if (rightWeaponItem != null)
                return rightWeaponItem.CrosshairSetting;
            if (leftWeaponItem != null)
                return leftWeaponItem.CrosshairSetting;
            return CurrentGameInstance.DefaultWeaponItem.CrosshairSetting;
        }

        public virtual float GetAttackDistance(bool isLeftHand)
        {
            IWeaponItem rightWeaponItem = EquipWeapons.GetRightHandWeaponItem();
            IWeaponItem leftWeaponItem = EquipWeapons.GetLeftHandWeaponItem();
            if (!isLeftHand)
            {
                if (rightWeaponItem != null)
                    return rightWeaponItem.WeaponType.DamageInfo.GetDistance() + StoppingDistance;
                if (rightWeaponItem == null && leftWeaponItem != null)
                    return leftWeaponItem.WeaponType.DamageInfo.GetDistance() + StoppingDistance;
            }
            else
            {
                if (leftWeaponItem != null)
                    return leftWeaponItem.WeaponType.DamageInfo.GetDistance() + StoppingDistance;
                if (leftWeaponItem == null && rightWeaponItem != null)
                    return rightWeaponItem.WeaponType.DamageInfo.GetDistance() + StoppingDistance;
            }
            return CurrentGameInstance.DefaultWeaponItem.WeaponType.DamageInfo.GetDistance() + StoppingDistance;
        }

        public virtual float GetAttackFov(bool isLeftHand)
        {
            IWeaponItem rightWeaponItem = EquipWeapons.GetRightHandWeaponItem();
            IWeaponItem leftWeaponItem = EquipWeapons.GetLeftHandWeaponItem();
            if (!isLeftHand)
            {
                if (rightWeaponItem != null)
                    return rightWeaponItem.WeaponType.DamageInfo.GetFov();
                if (rightWeaponItem == null && leftWeaponItem != null)
                    return leftWeaponItem.WeaponType.DamageInfo.GetFov();
            }
            else
            {
                if (leftWeaponItem != null)
                    return leftWeaponItem.WeaponType.DamageInfo.GetFov();
                if (leftWeaponItem == null && rightWeaponItem != null)
                    return rightWeaponItem.WeaponType.DamageInfo.GetFov();
            }
            return CurrentGameInstance.DefaultWeaponItem.WeaponType.DamageInfo.GetFov();
        }

#if UNITY_EDITOR
        public void SetDebugDamage(Vector3 damagePosition, Quaternion damageRotation)
        {
            debugDamagePosition = damagePosition;
            debugDamageRotation = damageRotation;
        }
#endif
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

        protected float GetMoveSpeed(MovementState movementState, ExtraMovementState extraMovementState)
        {
            float moveSpeed = this.GetCaches().MoveSpeed;

            if (IsAttackingOrUsingSkill)
                moveSpeed *= MoveSpeedRateWhileAttackOrUseSkill;

            if (movementState.HasFlag(MovementState.IsUnderWater))
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
            return GetMoveSpeed(MovementState, ExtraMovementState);
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
            if (!MovementState.HasFlag(MovementState.IsGrounded) || MovementState.HasFlag(MovementState.IsUnderWater))
                return false;
            return CurrentStamina > 0;
        }

        public override sealed bool CanCrouch()
        {
            if (!MovementState.HasFlag(MovementState.IsGrounded) || MovementState.HasFlag(MovementState.IsUnderWater))
                return false;
            return true;
        }

        public override sealed bool CanCrawl()
        {
            if (!MovementState.HasFlag(MovementState.IsGrounded) || MovementState.HasFlag(MovementState.IsUnderWater))
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

        public bool GetRaycastObjectIsTrigger_ForAttackFunctions(int index)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return raycasts2D_ForAttackFunctions[index].collider.isTrigger;
            return raycasts_ForAttackFunctions[index].collider.isTrigger;
        }

        public int OverlapObjects_ForAttackFunctions(Vector3 position, float distance, int layerMask)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return PhysicUtils.SortedOverlapCircleNonAlloc(position, distance, overlapColliders2D_ForAttackFunctions, layerMask);
            return PhysicUtils.SortedOverlapSphereNonAlloc(position, distance, overlapColliders_ForAttackFunctions, layerMask);
        }

        public GameObject GetOverlapObject_ForAttackFunctions(int index)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return overlapColliders2D_ForAttackFunctions[index].gameObject;
            return overlapColliders_ForAttackFunctions[index].gameObject;
        }

        public bool GetOverlapObjectIsTrigger_ForAttackFunctions(int index)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return overlapColliders2D_ForAttackFunctions[index].isTrigger;
            return overlapColliders_ForAttackFunctions[index].isTrigger;
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

        public bool GetOverlapObjectIsTrigger_ForFindFunctions(int index)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return overlapColliders2D_ForFindFunctions[index].isTrigger;
            return overlapColliders_ForFindFunctions[index].isTrigger;
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
            Vector2 targetDir = position - CacheTransform.position;
            targetDir.Normalize();
            float angle = Vector2.Angle(targetDir, Direction2D);
            // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
            return angle < fov * 0.5f;
        }

        protected bool IsPositionInFov3D(float fov, Vector3 position, Vector3 forward)
        {
            // This is unsigned angle, so angle found from this function is 0 - 180
            // if position forward from character this value will be 180
            // so just find for angle > 180 - halfFov
            Vector3 targetDir = position - CacheTransform.position;
            targetDir.y = 0;
            forward.y = 0;
            targetDir.Normalize();
            forward.Normalize();
            return Vector3.Angle(targetDir, forward) < fov * 0.5f;
        }

        public List<T> FindDamageableEntities<T>(float distance, int layerMask, bool findForAlive, bool findInFov = false, float fov = 0)
            where T : class, IDamageableEntity
        {
            List<T> result = new List<T>();
            int tempOverlapSize = OverlapObjects_ForFindFunctions(CacheTransform.position, distance, layerMask);
            if (tempOverlapSize == 0)
                return result;
            GameObject tempGameObject;
            IDamageableEntity tempBaseEntity;
            T tempEntity;
            for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
            {
                tempGameObject = GetOverlapObject_ForFindFunctions(tempLoopCounter);
                tempBaseEntity = tempGameObject.GetComponent<IDamageableEntity>();
                if (tempBaseEntity == null)
                    continue;
                tempEntity = tempBaseEntity.Entity as T;
                if (tempEntity == null)
                    continue;
                if (findForAlive && tempEntity.IsDead())
                    continue;
                if (findInFov && !IsPositionInFov(fov, tempEntity.GetTransform().position))
                    continue;
                if (result.Contains(tempEntity))
                    continue;
                result.Add(tempEntity);
            }
            return result;
        }

        public List<T> FindAliveDamageableEntities<T>(float distance, int layerMask, bool findInFov = false, float fov = 0)
            where T : class, IDamageableEntity
        {
            return FindDamageableEntities<T>(distance, layerMask, true, findInFov, fov);
        }

        public List<T> FindCharacters<T>(float distance, bool findForAlive, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            List<T> result = new List<T>();
            int tempOverlapSize = OverlapObjects_ForFindFunctions(CacheTransform.position, distance, CurrentGameInstance.characterLayer.Mask);
            if (tempOverlapSize == 0)
                return result;
            GameObject tempGameObject;
            IDamageableEntity tempBaseEntity;
            T tempEntity;
            for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
            {
                tempGameObject = GetOverlapObject_ForFindFunctions(tempLoopCounter);
                tempBaseEntity = tempGameObject.GetComponent<IDamageableEntity>();
                if (tempBaseEntity == null)
                    continue;
                tempEntity = tempBaseEntity.Entity as T;
                if (!IsCharacterWhichLookingFor(tempEntity, findForAlive, findForAlly, findForEnemy, findForNeutral, findInFov, fov))
                    continue;
                if (result.Contains(tempEntity))
                    continue;
                result.Add(tempEntity);
            }
            return result;
        }

        public List<T> FindAliveCharacters<T>(float distance, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            return FindCharacters<T>(distance, true, findForAlly, findForEnemy, findForNeutral, findInFov, fov);
        }

        public T FindNearestCharacter<T>(float distance, bool findForAliveOnly, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            int tempOverlapSize = OverlapObjects_ForFindFunctions(CacheTransform.position, distance, CurrentGameInstance.characterLayer.Mask);
            if (tempOverlapSize == 0)
                return null;
            GameObject tempGameObject;
            float tempDistance;
            IDamageableEntity tempBaseEntity;
            T tempEntity;
            float nearestDistance = float.MaxValue;
            T nearestEntity = null;
            for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
            {
                tempGameObject = GetOverlapObject_ForFindFunctions(tempLoopCounter);
                tempBaseEntity = tempGameObject.GetComponent<IDamageableEntity>();
                if (tempBaseEntity == null)
                    continue;
                tempEntity = tempBaseEntity.Entity as T;
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

        public virtual float GetMoveSpeedRateWhileAttackOrUseSkill(AnimActionType animActionType, BaseSkill skill)
        {
            if (skill != null)
                return skill.moveSpeedRateWhileUsingSkill;
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    if (EquipWeapons.GetRightHandWeaponItem() != null)
                        return EquipWeapons.GetRightHandWeaponItem().MoveSpeedRateWhileAttacking;
                    return CurrentGameInstance.DefaultWeaponItem.MoveSpeedRateWhileAttacking;
                case AnimActionType.AttackLeftHand:
                    if (EquipWeapons.GetLeftHandWeaponItem() != null)
                        return EquipWeapons.GetLeftHandWeaponItem().MoveSpeedRateWhileAttacking;
                    return CurrentGameInstance.DefaultWeaponItem.MoveSpeedRateWhileAttacking;
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

        public bool IsAlly(BaseCharacterEntity targetCharacter)
        {
            if (CurrentMapInfo == null)
                return false;
            return CurrentMapInfo.IsAlly(this, targetCharacter);
        }

        public bool IsEnemy(BaseCharacterEntity targetCharacter)
        {
            if (CurrentMapInfo == null)
                return false;
            return CurrentMapInfo.IsEnemy(this, targetCharacter);
        }

        public abstract void NotifyEnemySpotted(BaseCharacterEntity ally, BaseCharacterEntity attacker);
    }
}
