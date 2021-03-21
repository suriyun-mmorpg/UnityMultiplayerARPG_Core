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
        public const float FIND_ENTITY_DISTANCE_BUFFER = 1f;

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
        [Header("Character Attack Debugging")]
        public Color debugFovColor = new Color(0, 1, 0, 0.04f);
        public Vector3? debugDamagePosition;
        public Vector3? debugDamageDirection;
        public Quaternion? debugDamageRotation;
#endif

        [Header("Generic Character Profile")]
        [SerializeField]
        private CharacterRace race;
        public CharacterRace Race
        {
            get { return race; }
        }

        #region Protected data
        public UICharacterEntity UICharacterEntity { get; protected set; }
        public BaseSkill UsingSkill { get; protected set; }
        public short UsingSkillLevel { get; protected set; }
        public AnimActionType AnimActionType { get; protected set; }
        public int AnimActionDataId { get; protected set; }
        public short ReloadingAmmoAmount { get; protected set; }
        public bool IsReloading { get; protected set; }
        public float MoveSpeedRateWhileReloading { get; protected set; }
        public bool IsCharging { get; protected set; }
        public float MoveSpeedRateWhileCharging { get; protected set; }
        public bool IsAttackingOrUsingSkill { get; protected set; }
        public float MoveSpeedRateWhileAttackingOrUsingSkill { get; protected set; }
        public float RespawnGroundedCheckCountDown { get; protected set; }
        protected float lastMountTime;
        protected float lastActionTime;
        protected float pushGameMessageCountDown;
        protected readonly Queue<UITextKeys> pushingGameMessages = new Queue<UITextKeys>();
        #endregion

        public IPhysicFunctions AttackPhysicFunctions { get; protected set; }
        public IPhysicFunctions FindPhysicFunctions { get; protected set; }

        public override sealed int MaxHp { get { return this.GetCaches().MaxHp; } }
        public int MaxMp { get { return this.GetCaches().MaxMp; } }
        public int MaxStamina { get { return this.GetCaches().MaxStamina; } }
        public int MaxFood { get { return this.GetCaches().MaxFood; } }
        public int MaxWater { get { return this.GetCaches().MaxWater; } }
        public override sealed float MoveAnimationSpeedMultiplier { get { return this.GetCaches().BaseMoveSpeed > 0f ? GetMoveSpeed(MovementState, ExtraMovementState.None) / this.GetCaches().BaseMoveSpeed : 1f; } }
        public override sealed bool MuteFootstepSound { get { return this.GetCaches().MuteFootstepSound; } }
        public abstract int DataId { get; set; }

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
            ModelManager = gameObject.GetOrAddComponent<CharacterModelManager>();
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.layer = CurrentGameInstance.characterLayer;
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
                AttackPhysicFunctions = new PhysicFunctions(512);
                FindPhysicFunctions = new PhysicFunctions(512);
            }
            else
            {
                AttackPhysicFunctions = new PhysicFunctions2D(512);
                FindPhysicFunctions = new PhysicFunctions2D(512);
            }
            ClearActionStates();
            isRecaching = true;
        }

        protected virtual void SetAttackActionStates(AnimActionType animActionType, int animActionDataId)
        {
            ClearActionStates();
            AnimActionType = animActionType;
            AnimActionDataId = animActionDataId;
            IsAttackingOrUsingSkill = true;
        }

        protected virtual void SetUseSkillActionStates(AnimActionType animActionType, int animActionDataId, BaseSkill usingSkill, short usingSkillLevel)
        {
            ClearActionStates();
            AnimActionType = animActionType;
            AnimActionDataId = animActionDataId;
            UsingSkill = usingSkill;
            UsingSkillLevel = usingSkillLevel;
            IsAttackingOrUsingSkill = true;
        }

        protected virtual void SetReloadActionStates(AnimActionType animActionType, short reloadingAmmoAmount)
        {
            ClearActionStates();
            AnimActionType = animActionType;
            ReloadingAmmoAmount = reloadingAmmoAmount;
            IsReloading = true;
        }

        protected virtual void ClearActionStates()
        {
            AnimActionType = AnimActionType.None;
            AnimActionDataId = 0;
            UsingSkill = null;
            UsingSkillLevel = 0;
            ReloadingAmmoAmount = 0;
            IsAttackingOrUsingSkill = false;
            IsReloading = false;
            IsCharging = false;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;
            ModelManager = gameObject.GetOrAddComponent<CharacterModelManager>();
            if (model != ModelManager.ActiveModel)
            {
                model = ModelManager.ActiveModel;
                EditorUtility.SetDirty(this);
            }
#endif
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (debugDamagePosition.HasValue &&
                debugDamageDirection.HasValue &&
                debugDamageRotation.HasValue)
            {
                float atkHalfFov = GetAttackFov(false) * 0.5f;
                float atkDist = GetAttackDistance(false);
                Handles.color = debugFovColor;
                Handles.DrawSolidArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.up, debugDamageRotation.Value * Vector3.forward, -atkHalfFov, atkDist);
                Handles.DrawSolidArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.up, debugDamageRotation.Value * Vector3.forward, atkHalfFov, atkDist);
                Handles.DrawSolidArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.right, debugDamageRotation.Value * Vector3.forward, -atkHalfFov, atkDist);
                Handles.DrawSolidArc(debugDamagePosition.Value, debugDamageRotation.Value * Vector3.right, debugDamageRotation.Value * Vector3.forward, atkHalfFov, atkDist);

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

                Gizmos.color = Color.red;
                Gizmos.DrawRay(debugDamagePosition.Value, debugDamageDirection.Value * atkDist);
            }
        }
#endif

        protected override void EntityUpdate()
        {
            MakeCaches();
            float deltaTime = Time.deltaTime;
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
                    if (IsServer && !this.IsDead())
                    {
                        // Character will dead only when dimension type is 3D
                        CurrentHp = 0;
                        Killed(GetInfo());
                    }
                    // Disable movement when character dead
                    tempEnableMovement = false;
                }
            }
            else
            {
                RespawnGroundedCheckCountDown -= deltaTime;
            }

            // Clear data when character dead
            if (this.IsDead())
            {
                // Clear action states when character dead
                ClearActionStates();
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
                CastingSkillCountDown -= deltaTime;
            // Set character model hide state
            ModelManager.SetIsHide(CharacterModelManager.HIDE_SETTER_ENTITY, this.GetCaches().IsHide);
            // Update model animations
            if (IsClient)
            {
                // Update is dead state
                CharacterModel.SetIsDead(this.IsDead());
                // Update move speed multiplier
                CharacterModel.SetMoveAnimationSpeedMultiplier(MoveAnimationSpeedMultiplier);
                // Update movement animation
                CharacterModel.SetMovementState(MovementState, ExtraMovementState, Direction2D);
                // Update FPS model
                if (FpsModel && FpsModel.gameObject.activeSelf)
                {
                    // Update is dead state
                    FpsModel.SetIsDead(this.IsDead());
                    // Update move speed multiplier
                    FpsModel.SetMoveAnimationSpeedMultiplier(MoveAnimationSpeedMultiplier);
                    // Update movement animation
                    FpsModel.SetMovementState(MovementState, ExtraMovementState, Direction2D);
                }
            }

            if (IsOwnerClient)
            {
                // Pushing combatatnt errors on screen
                if (pushGameMessageCountDown > 0)
                    pushGameMessageCountDown -= deltaTime;
                if (pushGameMessageCountDown <= 0 && pushingGameMessages.Count > 0)
                {
                    pushGameMessageCountDown = COMBATANT_MESSAGE_DELAY;
                    ClientGenericActions.ClientReceiveGameMessage(pushingGameMessages.Dequeue());
                }
            }
        }

        protected override void OnTeleport(Vector3 position, Quaternion rotation)
        {
            base.OnTeleport(position, rotation);
            // Clear target entity when teleport
            SetTargetEntity(null);
        }

        #region Relates Objects
        public virtual void InstantiateUI(UICharacterEntity prefab)
        {
            if (prefab == null)
                return;
            if (UICharacterEntity != null)
                Destroy(UICharacterEntity.gameObject);
            UICharacterEntity = Instantiate(prefab, CharacterUITransform);
            UICharacterEntity.transform.localPosition = Vector3.zero;
            UICharacterEntity.Data = this;
        }
        #endregion

        #region Attack / Receive Damage / Dead / Spawn
        public void ValidateRecovery(EntityInfo causer)
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

            if (this.IsDead())
                Killed(causer);
        }

        public virtual void Killed(EntityInfo lastAttacker)
        {
            StopAllCoroutines();
            buffs.Clear();
            skillUsages.Clear();
            CallAllOnDead();
        }

        public virtual void OnRespawn()
        {
            if (!IsServer)
                return;
            lastGrounded = true;
            lastGroundedPosition = CacheTransform.position;
            RespawnGroundedCheckCountDown = RESPAWN_GROUNDED_CHECK_DURATION;
            CallAllOnRespawn();
        }

        public void RewardExp(Reward reward, float multiplier, RewardGivenType rewardGivenType)
        {
            if (!IsServer)
                return;
            int rewardedExp;
            if (!CurrentGameplayRule.RewardExp(this, reward, multiplier, rewardGivenType, out rewardedExp))
            {
                GameInstance.ServerGameMessageHandlers.NotifyRewardExp(ConnectionId, rewardedExp);
                return;
            }
            GameInstance.ServerGameMessageHandlers.NotifyRewardExp(ConnectionId, rewardedExp);
            CallAllOnLevelUp();
        }

        public void RewardCurrencies(Reward reward, float multiplier, RewardGivenType rewardGivenType)
        {
            if (!IsServer)
                return;
            int rewardedGold;
            CurrentGameplayRule.RewardCurrencies(this, reward, multiplier, rewardGivenType, out rewardedGold);
            GameInstance.ServerGameMessageHandlers.NotifyRewardGold(ConnectionId, rewardedGold);
            if (reward.currencies != null && reward.currencies.Length > 0)
            {
                foreach (CurrencyAmount currency in reward.currencies)
                {
                    if (currency.currency == null || currency.amount == 0)
                        continue;
                    GameInstance.ServerGameMessageHandlers.NotifyRewardCurrency(ConnectionId, currency.currency.DataId, currency.amount);
                }
            }
        }
        #endregion

        #region Helpers
        protected override void ApplyReceiveDamage(Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, out CombatAmountType combatAmountType, out int totalDamage)
        {
            bool isCritical = false;
            bool isBlocked = false;

            BaseCharacterEntity attackerCharacter;
            if (instigator.TryGetEntity(out attackerCharacter))
            {
                // Notify enemy spotted when received damage from enemy
                NotifyEnemySpottedToAllies(attackerCharacter);

                // Notify enemy spotted when damage taken to enemy
                attackerCharacter.NotifyEnemySpottedToAllies(this);

                if (!CurrentGameInstance.GameplayRule.RandomAttackHitOccurs(attackerCharacter, this, out isCritical, out isBlocked))
                {
                    // Don't hit (Miss)
                    combatAmountType = CombatAmountType.Miss;
                    totalDamage = 0;
                    return;
                }
            }

            // Calculate damages
            combatAmountType = CombatAmountType.NormalDamage;
            float calculatingTotalDamage = 0f;
            float calculatingDamage;
            MinMaxFloat damageAmount;
            foreach (DamageElement damageElement in damageAmounts.Keys)
            {
                damageAmount = damageAmounts[damageElement];
                calculatingDamage = damageElement.GetDamageReducedByResistance(this.GetCaches().Resistances, this.GetCaches().Armors, damageAmount.Random());
                if (calculatingDamage > 0f)
                    calculatingTotalDamage += calculatingDamage;
            }

            if (attackerCharacter != null)
            {
                // If critical occurs
                if (isCritical)
                {
                    calculatingTotalDamage = CurrentGameInstance.GameplayRule.GetCriticalDamage(attackerCharacter, this, calculatingTotalDamage);
                    combatAmountType = CombatAmountType.CriticalDamage;
                }
                // If block occurs
                if (isBlocked)
                {
                    calculatingTotalDamage = CurrentGameInstance.GameplayRule.GetBlockDamage(attackerCharacter, this, calculatingTotalDamage);
                    combatAmountType = CombatAmountType.BlockedDamage;
                }
            }

            // Apply damages
            totalDamage = (int)calculatingTotalDamage;
            CurrentHp -= totalDamage;
        }

        public override void ReceivedDamage(Vector3 fromPosition, EntityInfo instigator, CombatAmountType combatAmountType, int damage, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            base.ReceivedDamage(fromPosition, instigator, combatAmountType, damage, weapon, skill, skillLevel);
            BaseCharacterEntity attackerCharacter;
            if (instigator.TryGetEntity(out attackerCharacter))
                CurrentGameInstance.GameplayRule.OnCharacterReceivedDamage(attackerCharacter, this, combatAmountType, damage, weapon, skill, skillLevel);

            if (combatAmountType == CombatAmountType.Miss ||
                combatAmountType == CombatAmountType.None)
                return;

            // Interrupt casting skill when receive damage
            InterruptCastingSkill();

            // Only TPS model will plays hit animation
            CharacterModel.PlayHitAnimation();

            // Do something when character dead
            if (this.IsDead())
            {
                // Cancel actions
                CancelReload();
                CancelAttack();
                CancelSkill();

                // Call killed function, this should be called only once when dead
                ValidateRecovery(instigator);
            }
            else
            {
                // Apply debuff if character is not dead
                if (skill != null && skill.IsDebuff())
                    ApplyBuff(skill.DataId, BuffType.SkillDebuff, skillLevel, instigator);
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
            targetEntityId.UpdateImmediately();
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
        public void SetDebugDamage(Vector3 damagePosition, Vector3 damageDirection, Quaternion damageRotation)
        {
            debugDamagePosition = damagePosition;
            debugDamageDirection = damageDirection;
            debugDamageRotation = damageRotation;
        }
#endif
        #endregion

        #region Allowed abilities
        public virtual bool IsPlayingAttackOrUseSkillAnimation()
        {
            return AnimActionType == AnimActionType.AttackRightHand ||
                AnimActionType == AnimActionType.AttackLeftHand ||
                AnimActionType == AnimActionType.SkillLeftHand ||
                AnimActionType == AnimActionType.SkillRightHand;
        }

        public virtual bool IsPlayingActionAnimation()
        {
            return IsPlayingAttackOrUseSkillAnimation() ||
                AnimActionType == AnimActionType.ReloadRightHand ||
                AnimActionType == AnimActionType.ReloadLeftHand;
        }

        public virtual bool CanDoActions()
        {
            return !this.IsDead() && !IsAttackingOrUsingSkill && !IsReloading && !IsCharging && !IsPlayingActionAnimation();
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
            {
                moveSpeed *= MoveSpeedRateWhileAttackingOrUsingSkill;
            }
            else if (IsReloading)
            {
                moveSpeed *= MoveSpeedRateWhileReloading;
            }
            else if (IsCharging)
            {
                moveSpeed *= MoveSpeedRateWhileCharging;
            }

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
                    case ExtraMovementState.IsWalking:
                        moveSpeed *= CurrentGameplayRule.GetWalkMoveSpeedRate(this);
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
            if (this.IsDead())
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

        public override sealed bool IsHide()
        {
            return this.GetCaches().IsHide;
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
            if (this.IsDead())
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

        public bool IsGameEntityInDistance<T>(T targetEntity, float distance, bool includeUnHittable = true)
            where T : class, IGameEntity
        {
            return FindPhysicFunctions.IsGameEntityInDistance(targetEntity, CacheTransform.position, distance + FIND_ENTITY_DISTANCE_BUFFER, includeUnHittable);
        }

        public List<T> FindGameEntitiesInDistance<T>(float distance, int layerMask)
            where T : class, IGameEntity
        {
            return FindPhysicFunctions.FindGameEntitiesInDistance<T>(CacheTransform.position, distance + FIND_ENTITY_DISTANCE_BUFFER, layerMask);
        }

        public List<T> FindDamageableEntities<T>(float distance, int layerMask, bool findForAlive, bool findInFov = false, float fov = 0)
            where T : class, IDamageableEntity
        {
            List<T> result = new List<T>();
            int tempOverlapSize = FindPhysicFunctions.OverlapObjects(CacheTransform.position, distance, layerMask);
            if (tempOverlapSize == 0)
                return result;
            IDamageableEntity tempBaseEntity;
            T tempEntity;
            for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
            {
                tempBaseEntity = FindPhysicFunctions.GetOverlapObject(tempLoopCounter).GetComponent<IDamageableEntity>();
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
            int tempOverlapSize = FindPhysicFunctions.OverlapObjects(CacheTransform.position, distance, CurrentGameInstance.characterLayer.Mask);
            if (tempOverlapSize == 0)
                return result;
            IDamageableEntity tempBaseEntity;
            T tempEntity;
            for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
            {
                tempBaseEntity = FindPhysicFunctions.GetOverlapObject(tempLoopCounter).GetComponent<IDamageableEntity>();
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
            int tempOverlapSize = FindPhysicFunctions.OverlapObjects(CacheTransform.position, distance, CurrentGameInstance.characterLayer.Mask);
            if (tempOverlapSize == 0)
                return null;
            float tempDistance;
            IDamageableEntity tempBaseEntity;
            T tempEntity;
            float nearestDistance = float.MaxValue;
            T nearestEntity = null;
            for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
            {
                tempBaseEntity = FindPhysicFunctions.GetOverlapObject(tempLoopCounter).GetComponent<IDamageableEntity>();
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
            EntityInfo instigator = GetInfo();
            return (findForAlly && characterEntity.IsAlly(instigator)) ||
                (findForEnemy && characterEntity.IsEnemy(instigator)) ||
                (findForNeutral && characterEntity.IsNeutral(instigator));
        }
        #endregion

        #region Animation helpers
        public void GetRandomAnimationData(
            AnimActionType animActionType,
            int skillOrWeaponTypeDataId,
            out int animationIndex,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            animationIndex = 0;
            animSpeedRate = 1f;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            // Random animation
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    CharacterModel.GetRandomRightHandAttackAnimation(skillOrWeaponTypeDataId, out animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
                    break;
                case AnimActionType.AttackLeftHand:
                    CharacterModel.GetRandomLeftHandAttackAnimation(skillOrWeaponTypeDataId, out animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
                    break;
                case AnimActionType.SkillRightHand:
                case AnimActionType.SkillLeftHand:
                    CharacterModel.GetSkillActivateAnimation(skillOrWeaponTypeDataId, out animSpeedRate, out triggerDurations, out totalDuration);
                    break;
            }
        }

        public void GetAnimationData(
            AnimActionType animActionType,
            int skillOrWeaponTypeDataId,
            int animationIndex,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            animSpeedRate = 1f;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            // Random animation
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    CharacterModel.GetRightHandAttackAnimation(skillOrWeaponTypeDataId, animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
                    break;
                case AnimActionType.AttackLeftHand:
                    CharacterModel.GetLeftHandAttackAnimation(skillOrWeaponTypeDataId, animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
                    break;
                case AnimActionType.SkillRightHand:
                case AnimActionType.SkillLeftHand:
                    CharacterModel.GetSkillActivateAnimation(skillOrWeaponTypeDataId, out animSpeedRate, out triggerDurations, out totalDuration);
                    break;
                case AnimActionType.ReloadRightHand:
                    CharacterModel.GetRightHandReloadAnimation(skillOrWeaponTypeDataId, out animSpeedRate, out triggerDurations, out totalDuration);
                    break;
                case AnimActionType.ReloadLeftHand:
                    CharacterModel.GetLeftHandReloadAnimation(skillOrWeaponTypeDataId, out animSpeedRate, out triggerDurations, out totalDuration);
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

        public bool IsNeutral(EntityInfo instigator)
        {
            return !IsAlly(instigator) && !IsEnemy(instigator);
        }

        public override bool CanReceiveDamageFrom(EntityInfo instigator)
        {
            if (!base.CanReceiveDamageFrom(instigator))
                return false;
            // If this character is not ally so it is enemy and also can receive damage
            return !IsAlly(instigator);
        }

        public bool IsAlly(EntityInfo entityInfo)
        {
            if (CurrentMapInfo == null)
                return false;
            return CurrentMapInfo.IsAlly(this, entityInfo);
        }

        public bool IsEnemy(EntityInfo entityInfo)
        {
            if (CurrentMapInfo == null)
                return false;
            return CurrentMapInfo.IsEnemy(this, entityInfo);
        }

        public void QueueGameMessage(UITextKeys error)
        {
            if (!IsOwnerClient)
                return;
            // Last error must be different
            if (pushingGameMessages.Count > 0 &&
                pushingGameMessages.Peek() == error)
                return;
            // Enqueue error, it will be pushing on screen in Update()
            pushingGameMessages.Enqueue(error);
        }

        public abstract void NotifyEnemySpotted(BaseCharacterEntity ally, BaseCharacterEntity attacker);
    }
}
