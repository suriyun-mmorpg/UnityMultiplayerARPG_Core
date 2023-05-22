using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using LiteNetLibManager;
using LiteNetLib.Utils;
using LiteNetLib;
using UnityEngine.Profiling;
using Cysharp.Text;
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
        public const float RESPAWN_GROUNDED_CHECK_DURATION = 1f;
        public const float RESPAWN_INVINCIBLE_DURATION = 1f;
        public const float FIND_ENTITY_DISTANCE_BUFFER = 1f;
        public const int FRAMES_BEFORE_SET_EQUIP_MODEL = 1;

        protected struct SyncListRecachingState
        {
            public static readonly SyncListRecachingState Empty = new SyncListRecachingState();
            public bool isRecaching;
            public LiteNetLibSyncList.Operation operation;
            public int index;
        }

        [Category("Relative GameObjects/Transforms")]
        [Tooltip("When character attack with melee weapon, it will cast sphere from this transform to detect hit objects")]
        [SerializeField]
        private Transform meleeDamageTransform;
        public Transform MeleeDamageTransform
        {
            get { return meleeDamageTransform; }
            set { meleeDamageTransform = value; }
        }

        [Tooltip("When character attack with range weapon, it will spawn missile damage entity from this transform")]
        [SerializeField]
        private Transform missileDamageTransform;
        public Transform MissileDamageTransform
        {
            get { return missileDamageTransform; }
            set { missileDamageTransform = value; }
        }

        [Tooltip("Character UI will instantiates to this transform")]
        [SerializeField]
        [FormerlySerializedAs("characterUITransform")]
        private Transform characterUiTransform;
        public Transform CharacterUiTransform
        {
            get { return characterUiTransform; }
            set { characterUiTransform = value; }
        }

        [Tooltip("Mini Map UI will instantiates to this transform")]
        [SerializeField]
        [FormerlySerializedAs("miniMapUITransform")]
        private Transform miniMapUiTransform;
        public Transform MiniMapUiTransform
        {
            get { return miniMapUiTransform; }
            set { miniMapUiTransform = value; }
        }

        [Tooltip("Chat bubble will instantiates to this transform")]
        [SerializeField]
        private Transform chatBubbleTransform;
        public Transform ChatBubbleTransform
        {
            get { return chatBubbleTransform; }
            set { chatBubbleTransform = value; }
        }

#if UNITY_EDITOR
        [Category(200, "Debugging", false)]
        [FormerlySerializedAs("debugFovColor")]
        public Color debugDamageLaunchingColor = new Color(0, 1, 0, 0.04f);
        public Vector3? debugDamageLaunchingPosition;
        public Vector3? debugDamageLaunchingDirection;
        public Quaternion? debugDamageLaunchingRotation;
        public bool? debugDamageLaunchingIsLeftHand;
#endif

        [Category(5, "Character Settings")]
        [SerializeField]
        private CharacterRace race;
        public CharacterRace Race
        {
            get { return race; }
            set { race = value; }
        }

        #region Protected data
        public UICharacterEntity UICharacterEntity { get; protected set; }
        public UIChatMessage UIChatBubble { get; protected set; }
        public ICharacterAttackComponent AttackComponent { get; protected set; }
        public ICharacterUseSkillComponent UseSkillComponent { get; protected set; }
        public ICharacterReloadComponent ReloadComponent { get; protected set; }
        public ICharacterChargeComponent ChargeComponent { get; protected set; }
        public CharacterRecoveryComponent RecoveryComponent { get; protected set; }
        public CharacterSkillAndBuffComponent SkillAndBuffComponent { get; protected set; }
        public bool IsAttacking { get { return AttackComponent.IsAttacking; } }
        public float LastAttackEndTime { get { return AttackComponent.LastAttackEndTime; } }
        public float MoveSpeedRateWhileAttacking { get { return AttackComponent.MoveSpeedRateWhileAttacking; } }
        public MovementRestriction MovementRestrictionWhileAttacking { get { return AttackComponent.MovementRestrictionWhileAttacking; } }
        public float AttackTotalDuration { get { return AttackComponent.AttackTotalDuration; } set { AttackComponent.AttackTotalDuration = value; } }
        public float[] AttackTriggerDurations { get { return AttackComponent.AttackTriggerDurations; } set { AttackComponent.AttackTriggerDurations = value; } }
        public bool IsUsingSkill { get { return UseSkillComponent.IsUsingSkill; } }
        public float LastUseSkillEndTime { get { return UseSkillComponent.LastUseSkillEndTime; } }
        public float MoveSpeedRateWhileUsingSkill { get { return UseSkillComponent.MoveSpeedRateWhileUsingSkill; } }
        public MovementRestriction MovementRestrictionWhileUsingSkill { get { return UseSkillComponent.MovementRestrictionWhileUsingSkill; } }
        public float UseSkillTotalDuration { get { return UseSkillComponent.UseSkillTotalDuration; } set { UseSkillComponent.UseSkillTotalDuration = value; } }
        public float[] UseSkillTriggerDurations { get { return UseSkillComponent.UseSkillTriggerDurations; } set { UseSkillComponent.UseSkillTriggerDurations = value; } }
        public BaseSkill UsingSkill { get { return UseSkillComponent.UsingSkill; } }
        public int UsingSkillLevel { get { return UseSkillComponent.UsingSkillLevel; } }
        public bool IsCastingSkillCanBeInterrupted { get { return UseSkillComponent.IsCastingSkillCanBeInterrupted; } }
        public bool IsCastingSkillInterrupted { get { return UseSkillComponent.IsCastingSkillInterrupted; } }
        public float CastingSkillDuration { get { return UseSkillComponent.CastingSkillDuration; } }
        public float CastingSkillCountDown { get { return UseSkillComponent.CastingSkillCountDown; } }
        public int ReloadingAmmoAmount { get { return ReloadComponent.ReloadingAmmoAmount; } }
        public bool IsReloading { get { return ReloadComponent.IsReloading; } }
        public float LastReloadEndTime { get { return ReloadComponent.LastReloadEndTime; } }
        public float MoveSpeedRateWhileReloading { get { return ReloadComponent.MoveSpeedRateWhileReloading; } }
        public MovementRestriction MovementRestrictionWhileReloading { get { return ReloadComponent.MovementRestrictionWhileReloading; } }
        public float ReloadTotalDuration { get { return ReloadComponent.ReloadTotalDuration; } set { ReloadComponent.ReloadTotalDuration = value; } }
        public float[] ReloadTriggerDurations { get { return ReloadComponent.ReloadTriggerDurations; } set { ReloadComponent.ReloadTriggerDurations = value; } }
        public bool IsCharging { get { return ChargeComponent.IsCharging; } }
        public bool WillDoActionWhenStopCharging { get { return ChargeComponent.WillDoActionWhenStopCharging; } }
        public float MoveSpeedRateWhileCharging { get { return ChargeComponent.MoveSpeedRateWhileCharging; } }
        public MovementRestriction MovementRestrictionWhileCharging { get { return ChargeComponent.MovementRestrictionWhileCharging; } }
        public float RespawnGroundedCheckCountDown { get; protected set; }
        public float RespawnInvincibleCountDown { get; protected set; }
        public float LastUseItemTime { get; set; }

        protected int _countDownToSetEquipWeaponsModels = FRAMES_BEFORE_SET_EQUIP_MODEL;
        protected int _countDownToSetEquipItemsModels = FRAMES_BEFORE_SET_EQUIP_MODEL;
        protected float _lastMountTime;
        protected float _lastActionTime;
        protected bool _lastGrounded;
        protected Vector3 _lastGroundedPosition;
        #endregion

        public IPhysicFunctions AttackPhysicFunctions { get; protected set; }
        public IPhysicFunctions FindPhysicFunctions { get; protected set; }

        public override bool IsImmune { get { return base.IsImmune || RespawnInvincibleCountDown > 0f; } set { base.IsImmune = value; } }
        public override sealed int MaxHp { get { return CachedData.MaxHp; } }
        public int MaxMp { get { return CachedData.MaxMp; } }
        public int MaxStamina { get { return CachedData.MaxStamina; } }
        public int MaxFood { get { return CachedData.MaxFood; } }
        public int MaxWater { get { return CachedData.MaxWater; } }
        public override sealed float MoveAnimationSpeedMultiplier { get { return CachedData.BaseMoveSpeed > 0f ? GetMoveSpeed(MovementState, ExtraMovementState.None) / CachedData.BaseMoveSpeed : 1f; } }
        public override sealed bool MuteFootstepSound { get { return CachedData.MuteFootstepSound; } }
        public abstract int DataId { get; set; }

        public CharacterModelManager ModelManager { get; private set; }

        public override GameEntityModel Model
        {
            get { return ModelManager.ActiveTpsModel; }
        }

        public BaseCharacterModel CharacterModel
        {
            get { return ModelManager.ActiveTpsModel; }
        }

        public BaseCharacterModel FpsModel
        {
            get { return ModelManager.ActiveFpsModel; }
        }

        public override void InitialRequiredComponents()
        {
            base.InitialRequiredComponents();
            // Cache components
            if (meleeDamageTransform == null)
                meleeDamageTransform = EntityTransform;
            if (missileDamageTransform == null)
                missileDamageTransform = MeleeDamageTransform;
            if (characterUiTransform == null)
                characterUiTransform = EntityTransform;
            if (miniMapUiTransform == null)
                miniMapUiTransform = EntityTransform;
            if (chatBubbleTransform == null)
                chatBubbleTransform = EntityTransform;
            ModelManager = gameObject.GetOrAddComponent<CharacterModelManager>();
            AttackComponent = gameObject.GetOrAddComponent<ICharacterAttackComponent, DefaultCharacterAttackComponent>();
            UseSkillComponent = gameObject.GetOrAddComponent<ICharacterUseSkillComponent, DefaultCharacterUseSkillComponent>();
            ReloadComponent = gameObject.GetOrAddComponent<ICharacterReloadComponent, DefaultCharacterReloadComponent>();
            ChargeComponent = gameObject.GetOrAddComponent<ICharacterChargeComponent, DefaultCharacterChargeComponent>();
            RecoveryComponent = gameObject.GetOrAddComponent<CharacterRecoveryComponent>();
            SkillAndBuffComponent = gameObject.GetOrAddComponent<CharacterSkillAndBuffComponent>();
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            ForceMakeCaches();
            _lastGrounded = false;
            _lastGroundedPosition = EntityTransform.position;
        }

        protected override void EntityStart()
        {
            base.EntityStart();
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
                AttackPhysicFunctions = new PhysicFunctions(64);
                FindPhysicFunctions = new PhysicFunctions(IsOwnerClient ? 128 : 32);
            }
            else
            {
                AttackPhysicFunctions = new PhysicFunctions2D(64);
                FindPhysicFunctions = new PhysicFunctions2D(IsOwnerClient ? 128 : 32);
            }
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (debugDamageLaunchingPosition.HasValue &&
                debugDamageLaunchingDirection.HasValue &&
                debugDamageLaunchingRotation.HasValue &&
                debugDamageLaunchingIsLeftHand.HasValue)
            {
                float atkHalfFov = GetAttackFov(debugDamageLaunchingIsLeftHand.Value) * 0.5f;
                float atkDist = GetAttackDistance(debugDamageLaunchingIsLeftHand.Value);
                Handles.color = debugDamageLaunchingColor;
                Handles.DrawSolidArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.up, debugDamageLaunchingRotation.Value * Vector3.forward, -atkHalfFov, atkDist);
                Handles.DrawSolidArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.up, debugDamageLaunchingRotation.Value * Vector3.forward, atkHalfFov, atkDist);
                Handles.DrawSolidArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.right, debugDamageLaunchingRotation.Value * Vector3.forward, -atkHalfFov, atkDist);
                Handles.DrawSolidArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.right, debugDamageLaunchingRotation.Value * Vector3.forward, atkHalfFov, atkDist);

                Handles.DrawSolidArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.up, debugDamageLaunchingRotation.Value * Vector3.forward, -atkHalfFov, 0);
                Handles.DrawSolidArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.up, debugDamageLaunchingRotation.Value * Vector3.forward, atkHalfFov, 0);
                Handles.DrawSolidArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.right, debugDamageLaunchingRotation.Value * Vector3.forward, -atkHalfFov, 0);
                Handles.DrawSolidArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.right, debugDamageLaunchingRotation.Value * Vector3.forward, atkHalfFov, 0);

                Handles.color = new Color(debugDamageLaunchingColor.r, debugDamageLaunchingColor.g, debugDamageLaunchingColor.b);
                Handles.DrawWireArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.up, debugDamageLaunchingRotation.Value * Vector3.forward, -atkHalfFov, atkDist);
                Handles.DrawWireArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.up, debugDamageLaunchingRotation.Value * Vector3.forward, atkHalfFov, atkDist);
                Handles.DrawWireArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.up, debugDamageLaunchingRotation.Value * Vector3.forward, -atkHalfFov, 0);
                Handles.DrawWireArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.up, debugDamageLaunchingRotation.Value * Vector3.forward, atkHalfFov, 0);

                Handles.DrawWireArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.right, debugDamageLaunchingRotation.Value * Vector3.forward, -atkHalfFov, 0);
                Handles.DrawWireArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.right, debugDamageLaunchingRotation.Value * Vector3.forward, atkHalfFov, 0);
                Handles.DrawWireArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.right, debugDamageLaunchingRotation.Value * Vector3.forward, -atkHalfFov, atkDist);
                Handles.DrawWireArc(debugDamageLaunchingPosition.Value, debugDamageLaunchingRotation.Value * Vector3.right, debugDamageLaunchingRotation.Value * Vector3.forward, atkHalfFov, atkDist);

                Gizmos.color = Color.red;
                Gizmos.DrawRay(debugDamageLaunchingPosition.Value, debugDamageLaunchingDirection.Value * atkDist);
            }
        }
#endif

        protected override void EntityUpdate()
        {
            Profiler.BeginSample("BaseCharacterEntity - MakeCaches");
            MakeCaches();
            Profiler.EndSample();
            float deltaTime = Time.unscaledDeltaTime;

            Profiler.BeginSample("BaseCharacterEntity - AddHitBoxesTransformHistory");
            if (IsServer && CurrentGameManager.LagCompensationManager.ShouldStoreHitBoxesTransformHistory)
                AddHitBoxesTransformHistory(CurrentGameManager.ServerTimestamp);
            Profiler.EndSample();

            Profiler.BeginSample("BaseCharacterEntity - ApplyFallDamage");
            if (IsServer && CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
                bool isGrounded = MovementState.Has(MovementState.IsGrounded);
                // Ground check, ground damage will be calculated at server while dimension type is 3d only
                if (!_lastGrounded && isGrounded)
                {
                    // Apply fall damage when falling last frame and grounded this frame
                    CurrentGameplayRule.ApplyFallDamage(this, _lastGroundedPosition);
                }
                // Set last grounded state, it will be used next frame to find
                _lastGrounded = isGrounded;
                if (_lastGrounded)
                {
                    // Set last grounded position, it will be used to calculate fall damage
                    _lastGroundedPosition = EntityTransform.position;
                }
            }
            Profiler.EndSample();

            bool tempEnableMovement = PassengingVehicleEntity == null;
            Profiler.BeginSample("BaseCharacterEntity - UnderDeadYChecking");
            if (RespawnGroundedCheckCountDown > 0f)
            {
                // Character won't receive fall damage
                RespawnGroundedCheckCountDown -= deltaTime;
            }
            else
            {
                // Killing character when it fall below dead Y
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D &&
                    CurrentMapInfo != null && EntityTransform.position.y <= CurrentMapInfo.DeadY)
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
            Profiler.EndSample();

            if (RespawnInvincibleCountDown > 0f)
            {
                // Character won't receive damage
                RespawnInvincibleCountDown -= deltaTime;
            }

            Profiler.BeginSample("BaseCharacterEntity - DeadExitVehicle");
            // Clear data when character dead
            if (this.IsDead())
                ExitVehicle();
            Profiler.EndSample();

            Profiler.BeginSample("BaseCharacterEntity - MovementEnablingUpdate");
            // Enable movement or not
            if (!Movement.IsNull() && Movement.Enabled != tempEnableMovement)
            {
                if (!tempEnableMovement)
                    Movement.StopMove();
                // Enable movement while not passenging any vehicle
                Movement.Enabled = tempEnableMovement;
            }
            Profiler.EndSample();

            Profiler.BeginSample("BaseCharacterEntity - ModelManagerUpdate");
            // Update character model handler based on passenging vehicle
            ModelManager.UpdatePassengingVehicle(PassengingVehicleType, PassengingVehicleSeatIndex);
            // Set character model hide state
            ModelManager.SetIsHide(CharacterModelManager.HIDE_SETTER_ENTITY, IsHide());
            Profiler.EndSample();


            Profiler.BeginSample("BaseCharacterEntity - CharacterModelUpdate");
            // Update model animations
            if (IsClient || GameInstance.Singleton.updateAnimationAtServer)
            {
                // Update is dead state
                CharacterModel.SetIsDead(this.IsDead());
                // Update move speed multiplier
                CharacterModel.SetMoveAnimationSpeedMultiplier(MoveAnimationSpeedMultiplier);
                // Update movement state
                CharacterModel.SetMovementState(MovementState, ExtraMovementState, Direction2D, CachedData.FreezeAnimation);
                // Update animation
                CharacterModel.UpdateAnimation(deltaTime);
            }
            Profiler.EndSample();

            Profiler.BeginSample("BaseCharacterEntity - FPSModelUpdate");
            // Update FPS model
            if (IsOwnerClient && FpsModel != null && FpsModel.gameObject.activeSelf)
            {
                // Update is dead state
                FpsModel.SetIsDead(this.IsDead());
                // Update move speed multiplier
                FpsModel.SetMoveAnimationSpeedMultiplier(MoveAnimationSpeedMultiplier);
                // Update movement state
                FpsModel.SetMovementState(MovementState, ExtraMovementState, Direction2D, CachedData.FreezeAnimation);
                // Update animation
                FpsModel.UpdateAnimation(deltaTime);
            }
            Profiler.EndSample();

            Profiler.BeginSample("BaseCharacterEntity - SetEquipWeaponModels");
            if (_countDownToSetEquipWeaponsModels > 0)
            {
                --_countDownToSetEquipWeaponsModels;
                if (_countDownToSetEquipWeaponsModels <= 0)
                    SetEquipWeaponsModels();
            }
            Profiler.EndSample();

            Profiler.BeginSample("BaseCharacterEntity - SetEquipItemsModels");
            if (_countDownToSetEquipItemsModels > 0)
            {
                --_countDownToSetEquipItemsModels;
                if (_countDownToSetEquipItemsModels <= 0)
                    SetEquipItemsModels();
            }
            Profiler.EndSample();
        }

        public override void SendClientState()
        {
            bool shouldSendReliably = false;
            CharacterInputState inputState = CharacterInputState.None;
            EntityStateDataWriter.Reset();
            // Actions (can do only 1 action)
            if (AttackComponent.WriteClientAttackState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsAttacking;
            else if (UseSkillComponent.WriteClientUseSkillInterruptedState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsUsingSkillInterrupted;
            else if (UseSkillComponent.WriteClientUseSkillItemState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsUsingSkillItem;
            else if (UseSkillComponent.WriteClientUseSkillState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsUsingSkill;
            else if (ReloadComponent.WriteClientReloadState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsReloading;
            else if (ChargeComponent.WriteClientStopChargeState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsChargeStopping;
            else if (ChargeComponent.WriteClientStartChargeState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsChargeStarting;
            // Movement
            if (!Movement.IsNull() && Movement.Enabled && Movement.WriteClientState(EntityStateDataWriter, out shouldSendReliably))
                inputState |= CharacterInputState.IsMoving;
            // Set input state and send to clients
            if (inputState != CharacterInputState.None)
            {
                TransportHandler.WritePacket(EntityStateMessageWriter, GameNetworkingConsts.EntityState);
                EntityStateMessageWriter.PutPackedUInt(ObjectId);
                EntityStateMessageWriter.PutPackedUShort((ushort)inputState);
                EntityStateMessageWriter.Put(EntityStateDataWriter.Data, 0, EntityStateDataWriter.Length);
                ClientSendMessage(STATE_DATA_CHANNEL, (shouldSendReliably || (ushort)inputState > 1 << 0) ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Sequenced, EntityStateMessageWriter);
            }
            CurrentGameManager.HitRegistrationManager.SendHitRegToServer();
        }

        public override void SendServerState()
        {
            bool shouldSendReliably = false;
            CharacterInputState inputState = CharacterInputState.None;
            EntityStateDataWriter.Reset();
            // Actions (can do only 1 action)
            if (AttackComponent.WriteServerAttackState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsAttacking;
            else if (UseSkillComponent.WriteServerUseSkillInterruptedState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsUsingSkillInterrupted;
            else if (UseSkillComponent.WriteServerUseSkillItemState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsUsingSkillItem;
            else if (UseSkillComponent.WriteServerUseSkillState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsUsingSkill;
            else if (ReloadComponent.WriteServerReloadState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsReloading;
            else if (ChargeComponent.WriteServerStopChargeState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsChargeStopping;
            else if (ChargeComponent.WriteServerStartChargeState(EntityStateDataWriter))
                inputState |= CharacterInputState.IsChargeStarting;
            // Movement
            if (!Movement.IsNull() && Movement.Enabled && Movement.WriteServerState(EntityStateDataWriter, out shouldSendReliably))
                inputState |= CharacterInputState.IsMoving;
            // Set input state and send to clients
            if (inputState != CharacterInputState.None)
            {
                TransportHandler.WritePacket(EntityStateMessageWriter, GameNetworkingConsts.EntityState);
                EntityStateMessageWriter.PutPackedUInt(ObjectId);
                EntityStateMessageWriter.PutPackedUShort((ushort)inputState);
                EntityStateMessageWriter.Put(EntityStateDataWriter.Data, 0, EntityStateDataWriter.Length);
                ServerSendMessageToSubscribers(STATE_DATA_CHANNEL, (shouldSendReliably || (ushort)inputState > 1 << 0) ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Sequenced, EntityStateMessageWriter);
            }
        }

        public override void ReadClientStateAtServer(NetDataReader reader)
        {
            CharacterInputState inputState = (CharacterInputState)reader.GetPackedUShort();
            // Actions
            if (inputState.Has(CharacterInputState.IsReloading))
                ReloadComponent.ReadClientReloadStateAtServer(reader);
            if (inputState.Has(CharacterInputState.IsChargeStopping))
                ChargeComponent.ReadClientStopChargeStateAtServer(reader);
            if (inputState.Has(CharacterInputState.IsChargeStarting))
                ChargeComponent.ReadClientStartChargeStateAtServer(reader);
            if (inputState.Has(CharacterInputState.IsUsingSkillInterrupted))
                UseSkillComponent.ReadClientUseSkillInterruptedStateAtServer(reader);
            if (inputState.Has(CharacterInputState.IsAttacking))
                AttackComponent.ReadClientAttackStateAtServer(reader);
            if (inputState.Has(CharacterInputState.IsUsingSkillItem))
                UseSkillComponent.ReadClientUseSkillItemStateAtServer(reader);
            if (inputState.Has(CharacterInputState.IsUsingSkill))
                UseSkillComponent.ReadClientUseSkillStateAtServer(reader);
            // Movement
            if (inputState.Has(CharacterInputState.IsMoving) && Movement != null)
                Movement.ReadClientStateAtServer(reader);
        }

        public override void ReadServerStateAtClient(NetDataReader reader)
        {
            CharacterInputState inputState = (CharacterInputState)reader.GetPackedUShort();
            // Actions
            if (inputState.Has(CharacterInputState.IsReloading))
                ReloadComponent.ReadServerReloadStateAtClient(reader);
            if (inputState.Has(CharacterInputState.IsChargeStopping))
                ChargeComponent.ReadServerStopChargeStateAtClient(reader);
            if (inputState.Has(CharacterInputState.IsChargeStarting))
                ChargeComponent.ReadServerStartChargeStateAtClient(reader);
            if (inputState.Has(CharacterInputState.IsUsingSkillInterrupted))
                UseSkillComponent.ReadServerUseSkillInterruptedStateAtClient(reader);
            if (inputState.Has(CharacterInputState.IsAttacking))
                AttackComponent.ReadServerAttackStateAtClient(reader);
            if (inputState.Has(CharacterInputState.IsUsingSkillItem))
                UseSkillComponent.ReadServerUseSkillItemStateAtClient(reader);
            if (inputState.Has(CharacterInputState.IsUsingSkill))
                UseSkillComponent.ReadServerUseSkillStateAtClient(reader);
            // Movement
            if (inputState.Has(CharacterInputState.IsMoving) && Movement != null)
                Movement.ReadServerStateAtClient(reader);
        }

        protected override void OnTeleport(Vector3 position, Quaternion rotation)
        {
            base.OnTeleport(position, rotation);
            // Clear target entity when teleport
            SetTargetEntity(null);
            // Setup ground check data
            _lastGrounded = true;
            _lastGroundedPosition = position;
        }

        public override void PlayJumpAnimation()
        {
            if (CharacterModel && CharacterModel.gameObject.activeSelf)
                CharacterModel.PlayJumpAnimation();
            if (IsOwnerClient && FpsModel != null && FpsModel.gameObject.activeSelf)
                FpsModel.PlayJumpAnimation();
        }

        public override void PlayPickupAnimation()
        {
            if (CharacterModel && CharacterModel.gameObject.activeSelf)
                CharacterModel.PlayPickupAnimation();
            if (IsOwnerClient && FpsModel != null && FpsModel.gameObject.activeSelf)
                FpsModel.PlayPickupAnimation();
        }

        public override void PlayHitAnimation()
        {
            if (CharacterModel && CharacterModel.gameObject.activeSelf)
                CharacterModel.PlayHitAnimation();
            if (IsOwnerClient && FpsModel != null && FpsModel.gameObject.activeSelf)
                FpsModel.PlayHitAnimation();
        }

        public override void SetModelIsDead(bool isDead)
        {
            if (CharacterModel && CharacterModel.gameObject.activeSelf)
                CharacterModel.SetIsDead(isDead);
            if (IsOwnerClient && FpsModel != null && FpsModel.gameObject.activeSelf)
                FpsModel.SetIsDead(isDead);
        }

        #region Relates Objects
        public virtual void InstantiateUI(UICharacterEntity prefab)
        {
            if (prefab == null)
                return;
            if (UICharacterEntity != null)
                Destroy(UICharacterEntity.gameObject);
            UICharacterEntity = Instantiate(prefab, CharacterUiTransform);
            UICharacterEntity.transform.localPosition = Vector3.zero;
            UICharacterEntity.Data = this;
        }

        public virtual void InstantiateChatBubble(UIChatMessage prefab, ChatMessage chatMessage, float destroyDelay)
        {
            if (prefab == null)
                return;
            if (UIChatBubble != null)
                Destroy(UIChatBubble.gameObject);
            UIChatBubble = Instantiate(prefab, ChatBubbleTransform);
            UIChatBubble.transform.localPosition = Vector3.zero;
            UIChatBubble.Data = chatMessage;
            Destroy(UIChatBubble.gameObject, destroyDelay);
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

        #region Attack / Skill / Weapon / Damage
        public bool ValidateAttack(ref bool isLeftHand, out CharacterItem characterItem)
        {
            characterItem = null;

            if (!CanAttack())
                return false;

            if (!UpdateLastActionTime())
                return false;

            characterItem = this.GetAvailableWeapon(ref isLeftHand);
            IWeaponItem weaponItem = characterItem.GetWeaponItem();

            if (!ValidateAmmo(characterItem, 1))
            {
                if (IsOwnerClient)
                    ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_NO_AMMO);
                AudioClipWithVolumeSettings audioClip = weaponItem.EmptyClip;
                if (audioClip != null)
                    AudioManager.PlaySfxClipAtAudioSource(audioClip.audioClip, CharacterModel.GenericAudioSource, audioClip.GetRandomedVolume());
                return false;
            }

            if (!Entity.MovementState.Has(MovementState.IsGrounded) && weaponItem.AttackRestriction.restrictedWhileAirborne)
                return false;

            if (Entity.ExtraMovementState == ExtraMovementState.IsCrouching && weaponItem.AttackRestriction.restrictedWhileCrouching)
                return false;

            if (Entity.ExtraMovementState == ExtraMovementState.IsCrawling && weaponItem.AttackRestriction.restrictedWhileCrawling)
                return false;

            return true;
        }

        public bool ValidateUseSkill(int dataId, bool isLeftHand, uint targetObjectId)
        {
            if (!CanUseSkill())
                return false;

            if (!UpdateLastActionTime())
                return false;

            if (!this.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out BaseSkill skill, out _, out UITextKeys gameMessage))
            {
                if (IsOwnerClient)
                    ClientGenericActions.ClientReceiveGameMessage(gameMessage);
                return false;
            }

            if (!Entity.MovementState.Has(MovementState.IsGrounded) && skill.useSkillRestriction.restrictedWhileAirborne)
                return false;

            if (Entity.ExtraMovementState == ExtraMovementState.IsCrouching && skill.useSkillRestriction.restrictedWhileCrouching)
                return false;

            if (Entity.ExtraMovementState == ExtraMovementState.IsCrawling && skill.useSkillRestriction.restrictedWhileCrawling)
                return false;

            return true;
        }

        public bool ValidateUseSkillItem(int index, bool isLeftHand, uint targetObjectId)
        {
            if (!CanUseItem())
                return false;

            if (!UpdateLastActionTime())
                return false;

            if (!this.ValidateSkillItemToUse(index, isLeftHand, targetObjectId, out _, out BaseSkill skill, out _, out UITextKeys gameMessage))
            {
                if (IsOwnerClient)
                    ClientGenericActions.ClientReceiveGameMessage(gameMessage);
                return false;
            }

            if (!Entity.MovementState.Has(MovementState.IsGrounded) && skill.useSkillRestriction.restrictedWhileAirborne)
                return false;

            if (Entity.ExtraMovementState == ExtraMovementState.IsCrouching && skill.useSkillRestriction.restrictedWhileCrouching)
                return false;

            if (Entity.ExtraMovementState == ExtraMovementState.IsCrawling && skill.useSkillRestriction.restrictedWhileCrawling)
                return false;

            return true;
        }

        public bool ValidateReload(bool isLeftHand)
        {
            if (!CanDoActions())
                return false;

            CharacterItem characterItem = isLeftHand ? EquipWeapons.leftHand : EquipWeapons.rightHand;
            if (characterItem.IsEmptySlot())
                return false;

            IWeaponItem weaponItem = characterItem.GetWeaponItem();
            if (characterItem.IsAmmoFull() || !characterItem.HasAmmoToReload(this))
                return false;

            if (!Entity.MovementState.Has(MovementState.IsGrounded) && weaponItem.ReloadRestriction.restrictedWhileAirborne)
                return false;

            if (Entity.ExtraMovementState == ExtraMovementState.IsCrouching && weaponItem.ReloadRestriction.restrictedWhileCrouching)
                return false;

            if (Entity.ExtraMovementState == ExtraMovementState.IsCrawling && weaponItem.ReloadRestriction.restrictedWhileCrawling)
                return false;

            return true;
        }

        public bool Attack(ref bool isLeftHand)
        {
            if (!IsOwnerClientOrOwnedByServer)
                return false;
            if (ValidateAttack(ref isLeftHand, out CharacterItem characterItem))
            {
                if (characterItem.GetWeaponItem().FireType == FireType.FireOnRelease && !WillDoActionWhenStopCharging)
                {
                    StopCharge();
                    return false;
                }
                StopCharge();
                AttackComponent.Attack(isLeftHand);
                return true;
            }
            return false;
        }

        public bool UseSkill(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (!IsOwnerClientOrOwnedByServer)
                return false;
            if (ValidateUseSkill(dataId, isLeftHand, targetObjectId))
            {
                StopCharge();
                UseSkillComponent.UseSkill(dataId, isLeftHand, targetObjectId, aimPosition);
                return true;
            }
            return false;
        }

        public bool UseSkillItem(int itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (!IsOwnerClientOrOwnedByServer)
                return false;
            if (ValidateUseSkillItem(itemIndex, isLeftHand, targetObjectId))
            {
                StopCharge();
                UseSkillComponent.UseSkillItem(itemIndex, isLeftHand, targetObjectId, aimPosition);
                return true;
            }
            return false;
        }

        public void InterruptCastingSkill()
        {
            UseSkillComponent.InterruptCastingSkill();
        }

        public bool StartCharge(ref bool isLeftHand)
        {
            if (!IsOwnerClientOrOwnedByServer)
                return false;
            if (ValidateAttack(ref isLeftHand, out CharacterItem item) && item.GetWeaponItem().FireType == FireType.FireOnRelease)
            {
                ChargeComponent.StartCharge(isLeftHand);
                return true;
            }
            return false;
        }

        public bool StopCharge()
        {
            if (!IsOwnerClientOrOwnedByServer)
                return false;
            if (IsCharging)
            {
                ChargeComponent.StopCharge();
                return true;
            }
            return false;
        }

        public bool Reload(bool isLeftHand)
        {
            if (!IsOwnerClientOrOwnedByServer)
                return false;
            if (ValidateReload(isLeftHand))
            {
                ReloadComponent.Reload(isLeftHand);
                return true;
            }
            return false;
        }

        public bool UpdateLastActionTime()
        {
            float time = Time.unscaledTime;
            if (time - _lastActionTime < ACTION_DELAY)
                return false;
            _lastActionTime = time;
            return true;
        }

        public bool CanDoNextAction()
        {
            return Time.unscaledTime - _lastActionTime >= ACTION_DELAY;
        }

        public void ClearActionStates()
        {
            AttackComponent.ClearAttackStates();
            UseSkillComponent.ClearUseSkillStates();
            ReloadComponent.ClearReloadStates();
            ChargeComponent.ClearChargeStates();
        }

        public AimPosition GetAttackAimPosition(ref bool isLeftHand)
        {
            return GetAttackAimPosition(this.GetWeaponDamageInfo(ref isLeftHand), isLeftHand);
        }

        public AimPosition GetAttackAimPosition(ref bool isLeftHand, Vector3 targetPosition)
        {
            return GetAttackAimPosition(this.GetWeaponDamageInfo(ref isLeftHand), isLeftHand, targetPosition);
        }

        public AimPosition GetAttackAimPositionByDirection(ref bool isLeftHand, Vector3 direction, bool aimToTargetIfExisted = true)
        {
            return GetAttackAimPositionByDirection(this.GetWeaponDamageInfo(ref isLeftHand), isLeftHand, direction, aimToTargetIfExisted);
        }

        public AimPosition GetAttackAimPosition(DamageInfo damageInfo, bool isLeftHand, bool aimToTargetIfExisted = true)
        {
            return GetAttackAimPositionByDirection(damageInfo, isLeftHand, EntityTransform.forward, aimToTargetIfExisted);
        }

        public AimPosition GetAttackAimPositionByDirection(DamageInfo damageInfo, bool isLeftHand, Vector3 direction, bool aimToTargetIfExisted = true)
        {
            Vector3 position = damageInfo.GetDamageTransform(this, isLeftHand).position;
            if (aimToTargetIfExisted)
            {
                BaseGameEntity targetEntity = GetTargetEntity();
                if (targetEntity != null && targetEntity != Entity)
                {
                    if (targetEntity is DamageableEntity damageableEntity)
                    {
                        if (!damageableEntity.IsHideOrDead())
                            return GetAttackAimPosition(position, damageableEntity.OpponentAimTransform.position);
                    }
                    else
                    {
                        return GetAttackAimPosition(position, targetEntity.EntityTransform.position);
                    }
                }
            }
            return AimPosition.CreateDirection(position, direction);
        }

        public AimPosition GetAttackAimPosition(DamageInfo damageInfo, bool isLeftHand, Vector3 targetPosition)
        {
            return GetAttackAimPosition(damageInfo.GetDamageTransform(this, isLeftHand).position, targetPosition);
        }

        public AimPosition GetAttackAimPosition(Vector3 position, Vector3 targetPosition)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
                Vector3 direction = (targetPosition - position).normalized;
                return AimPosition.CreateDirection(position, direction);
            }
            return AimPosition.CreatePosition(targetPosition);
        }

        public virtual void GetReloadingData(
            ref bool isLeftHand,
            out AnimActionType animActionType,
            out int animationDataId,
            out CharacterItem weapon)
        {
            weapon = this.GetAvailableWeapon(ref isLeftHand);
            // Assign data id
            animationDataId = weapon.GetWeaponItem().WeaponType.DataId;
            // Assign animation action type
            animActionType = isLeftHand ? AnimActionType.ReloadLeftHand : AnimActionType.ReloadRightHand;
        }

        public virtual void GetAttackingData(
            ref bool isLeftHand,
            out AnimActionType animActionType,
            out int animationDataId,
            out CharacterItem weapon)
        {
            weapon = this.GetAvailableWeapon(ref isLeftHand);
            // Assign data id
            animationDataId = weapon.GetWeaponItem().WeaponType.DataId;
            // Assign animation action type
            animActionType = isLeftHand ? AnimActionType.AttackLeftHand : AnimActionType.AttackRightHand;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetWeaponDamagesWithBuffs(CharacterItem weapon)
        {
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Calculate all damages
            damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, this.GetWeaponDamages(weapon));
            // Sum damage with buffs
            damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, CachedData.IncreaseDamages);

            return damageAmounts;
        }

        public bool ValidateAmmo(CharacterItem weapon, int amount, bool validIfNoRequireAmmoType = true)
        {
            // Avoid null data
            if (weapon == null)
                return validIfNoRequireAmmoType;

            IWeaponItem weaponItem = weapon.GetWeaponItem();
            if (weaponItem.WeaponType.RequireAmmoType != null)
            {
                if (weaponItem.AmmoCapacity <= 0)
                {
                    // Ammo capacity is 0 so reduce ammo from inventory
                    if (this.CountAmmos(weaponItem.WeaponType.RequireAmmoType) < amount)
                        return false;
                }
                else
                {
                    // Ammo capacity more than 0 reduce loaded ammo
                    if (weapon.ammo < amount)
                        return false;
                }
                return true;
            }

            return validIfNoRequireAmmoType;
        }

        public bool DecreaseAmmos(CharacterItem weapon, bool isLeftHand, int amount, out Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts, bool validIfNoRequireAmmoType = true)
        {
            increaseDamageAmounts = null;

            // Avoid null data
            if (weapon == null)
                return validIfNoRequireAmmoType;

            IWeaponItem weaponItem = weapon.GetWeaponItem();
            if (weaponItem.WeaponType.RequireAmmoType != null)
            {
                if (weaponItem.AmmoCapacity <= 0)
                {
                    // Ammo capacity is 0 so reduce ammo from inventory
                    if (this.DecreaseAmmos(weaponItem.WeaponType.RequireAmmoType, amount, out increaseDamageAmounts))
                    {
                        this.FillEmptySlots();
                        return true;
                    }
                    // Not enough ammo
                    return false;
                }
                else
                {
                    // Ammo capacity >= `amount` reduce loaded ammo
                    if (weapon.ammo >= amount)
                    {
                        weapon.ammo -= amount;
                        EquipWeapons equipWeapons = EquipWeapons;
                        if (isLeftHand)
                            equipWeapons.leftHand = weapon;
                        else
                            equipWeapons.rightHand = weapon;
                        EquipWeapons = equipWeapons;
                        return true;
                    }
                    // Not enough ammo
                    return false;
                }
            }
            return validIfNoRequireAmmoType;
        }

        public virtual void GetUsingSkillData(
            BaseSkill skill,
            ref bool isLeftHand,
            out AnimActionType animActionType,
            out int animationDataId,
            out CharacterItem weapon)
        {
            // Initialize data
            animActionType = AnimActionType.None;
            animationDataId = 0;
            weapon = this.GetAvailableWeapon(ref isLeftHand);
            // Prepare skill data
            if (skill == null)
                return;
            // Prepare weapon data
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            // Get activate animation type which defined at character model
            SkillActivateAnimationType useSkillActivateAnimationType = CharacterModel.UseSkillActivateAnimationType(skill);
            // Prepare animation
            if (useSkillActivateAnimationType == SkillActivateAnimationType.UseAttackAnimation && skill.IsAttack)
            {
                // Assign data id
                animationDataId = weaponItem.WeaponType.DataId;
                // Assign animation action type
                animActionType = !isLeftHand ? AnimActionType.AttackRightHand : AnimActionType.AttackLeftHand;
            }
            else if (useSkillActivateAnimationType == SkillActivateAnimationType.UseActivateAnimation)
            {
                // Assign data id
                animationDataId = skill.DataId;
                // Assign animation action type
                animActionType = !isLeftHand ? AnimActionType.SkillRightHand : AnimActionType.SkillLeftHand;
            }
        }

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
                    return rightWeaponItem.WeaponType.DamageInfo.GetDistance();
                if (rightWeaponItem == null && leftWeaponItem != null)
                    return leftWeaponItem.WeaponType.DamageInfo.GetDistance();
            }
            else
            {
                if (leftWeaponItem != null)
                    return leftWeaponItem.WeaponType.DamageInfo.GetDistance();
                if (leftWeaponItem == null && rightWeaponItem != null)
                    return rightWeaponItem.WeaponType.DamageInfo.GetDistance();
            }
            return CurrentGameInstance.DefaultWeaponItem.WeaponType.DamageInfo.GetDistance();
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
        public void SetDebugDamage(Vector3 damagePosition, Vector3 damageDirection, Quaternion damageRotation, bool isLeftHand)
        {
            debugDamageLaunchingPosition = damagePosition;
            debugDamageLaunchingDirection = damageDirection;
            debugDamageLaunchingRotation = damageRotation;
            debugDamageLaunchingIsLeftHand = isLeftHand;
        }
#endif
        #endregion

        #region Allowed abilities
        public virtual bool IsPlayingAttackOrUseSkillAnimation()
        {
            return AttackComponent.IsAttacking || UseSkillComponent.IsUsingSkill;
        }

        public virtual bool IsPlayingReloadAnimation()
        {
            return ReloadComponent.IsReloading;
        }

        public virtual bool IsPlayingActionAnimation()
        {
            return IsPlayingAttackOrUseSkillAnimation() ||
                IsPlayingReloadAnimation();
        }

        public float GetAttackSpeed()
        {
            float atkSpeed = CachedData.AtkSpeed;
            // Minimum attack speed is 0.1
            if (atkSpeed <= 0.1f)
                atkSpeed = 0.1f;
            return atkSpeed;
        }

        protected float GetMoveSpeed(MovementState movementState, ExtraMovementState extraMovementState)
        {
            float moveSpeed = CachedData.MoveSpeed;
            float time = Time.unscaledTime;
            if (IsAttacking || time - LastAttackEndTime < CurrentGameInstance.returnMoveSpeedDelayAfterAction)
            {
                moveSpeed *= MoveSpeedRateWhileAttacking;
            }
            else if (IsUsingSkill || time - LastUseSkillEndTime < CurrentGameInstance.returnMoveSpeedDelayAfterAction)
            {
                moveSpeed *= MoveSpeedRateWhileUsingSkill;
            }
            else if (IsReloading)
            {
                moveSpeed *= MoveSpeedRateWhileReloading;
            }
            else if (IsCharging)
            {
                moveSpeed *= MoveSpeedRateWhileCharging;
            }
            if (movementState.Has(MovementState.IsUnderWater))
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

            if (CachedData.IsOverweight)
                moveSpeed *= CurrentGameplayRule.GetOverweightMoveSpeedRate(this);

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
            if (CachedData.DisallowMove)
                return false;
            return true;
        }

        public override sealed bool CanSprint()
        {
            if (!MovementState.Has(MovementState.IsGrounded) || MovementState.Has(MovementState.IsUnderWater))
                return false;
            if (CachedData.DisallowSprint)
                return false;
            return CurrentStamina > 0;
        }

        public sealed override bool CanWalk()
        {
            if (!MovementState.Has(MovementState.IsGrounded) || MovementState.Has(MovementState.IsUnderWater))
                return false;
            if (CachedData.DisallowWalk)
                return false;
            return true;
        }

        public override sealed bool CanCrouch()
        {
            if (!MovementState.Has(MovementState.IsGrounded) || MovementState.Has(MovementState.IsUnderWater))
                return false;
            if (CachedData.DisallowCrouch)
                return false;
            return true;
        }

        public override sealed bool CanCrawl()
        {
            if (!MovementState.Has(MovementState.IsGrounded) || MovementState.Has(MovementState.IsUnderWater))
                return false;
            if (CachedData.DisallowCrawl)
                return false;
            return true;
        }

        public override bool CanJump()
        {
            if (CachedData.DisallowJump)
            {
                return false;
            }
            if (IsAttacking && MovementRestrictionWhileAttacking.jumpRestricted)
            {
                return false;
            }
            else if (IsUsingSkill && MovementRestrictionWhileUsingSkill.jumpRestricted)
            {
                return false;
            }
            else if (IsReloading && MovementRestrictionWhileReloading.jumpRestricted)
            {
                return false;
            }
            else if (IsCharging && MovementRestrictionWhileCharging.jumpRestricted)
            {
                return false;
            }
            return true;
        }

        public override bool CanTurn()
        {
            if (IsAttacking && MovementRestrictionWhileAttacking.turnRestricted)
            {
                return false;
            }
            else if (IsUsingSkill && MovementRestrictionWhileUsingSkill.turnRestricted)
            {
                return false;
            }
            else if (IsReloading && MovementRestrictionWhileReloading.turnRestricted)
            {
                return false;
            }
            else if (IsCharging && MovementRestrictionWhileCharging.turnRestricted)
            {
                return false;
            }
            return true;
        }

        public override sealed bool IsHide()
        {
            return CachedData.IsHide;
        }
        #endregion

        #region Data helpers
        private string GetEquipPosition(string equipPositionId, byte equipSlotIndex)
        {
            return ZString.Concat(equipPositionId, ':', equipSlotIndex);
        }
        #endregion

        #region Find objects helpers
        public bool IsPositionInFov(float fov, Vector3 position)
        {
            return IsPositionInFov(fov, position, EntityTransform.forward);
        }

        public bool IsPositionInFov(float fov, Vector3 position, Vector3 forward)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                return IsPositionInFov2D(fov, position, forward);
            return IsPositionInFov3D(fov, position, forward);
        }

        protected bool IsPositionInFov2D(float fov, Vector3 position, Vector3 forward)
        {
            Vector2 targetDir = position - EntityTransform.position;
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
            Vector3 targetDir = position - EntityTransform.position;
            targetDir.y = 0;
            forward.y = 0;
            targetDir.Normalize();
            forward.Normalize();
            return Vector3.Angle(targetDir, forward) < fov * 0.5f;
        }

        public bool IsGameEntityInDistance<T>(T targetEntity, float distance, bool includeUnHittable = true)
            where T : class, IGameEntity
        {
            return FindPhysicFunctions.IsGameEntityInDistance(targetEntity, EntityTransform.position, distance + FIND_ENTITY_DISTANCE_BUFFER, includeUnHittable);
        }

        public List<T> FindGameEntitiesInDistance<T>(float distance, int overlayMask)
            where T : class, IGameEntity
        {
            return FindPhysicFunctions.FindGameEntitiesInDistance<T>(EntityTransform.position, distance + FIND_ENTITY_DISTANCE_BUFFER, overlayMask);
        }

        public List<T> FindEntities<T>(Vector3 origin, float distance, bool findForAlive, bool findForAlly, bool findForEnemy, bool findForNeutral, int overlapMask, bool findInFov = false, float fov = 0)
            where T : DamageableEntity
        {
            List<T> result = new List<T>();
            Profiler.BeginSample("Character Entity Components - Find Entities");
            int tempOverlapSize = FindPhysicFunctions.OverlapObjects(origin, distance, overlapMask);
            Profiler.EndSample();
            if (tempOverlapSize == 0)
                return result;
            IDamageableEntity tempBaseEntity;
            T tempEntity;
            for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
            {
                tempBaseEntity = FindPhysicFunctions.GetOverlapObject(tempLoopCounter).GetComponent<IDamageableEntity>();
                if (tempBaseEntity.IsNull())
                    continue;
                tempEntity = tempBaseEntity.Entity as T;
                if (!IsEntityWhichLookingFor(tempEntity, findForAlive, findForAlly, findForEnemy, findForNeutral, findInFov, fov))
                    continue;
                if (result.Contains(tempEntity))
                    continue;
                result.Add(tempEntity);
            }
            return result;
        }

        public List<T> FindEntities<T>(float distance, bool findForAlive, bool findForAlly, bool findForEnemy, bool findForNeutral, int overlapMask, bool findInFov = false, float fov = 0)
            where T : DamageableEntity
        {
            return FindEntities<T>(EntityTransform.position, distance, findForAlive, findForAlly, findForEnemy, findForNeutral, overlapMask, findInFov, fov);
        }

        public List<T> FindAliveEntities<T>(Vector3 origin, float distance, bool findForAlly, bool findForEnemy, bool findForNeutral, int overlapMask, bool findInFov = false, float fov = 0)
            where T : DamageableEntity
        {
            return FindEntities<T>(origin, distance, true, findForAlly, findForEnemy, findForNeutral, overlapMask, findInFov, fov);
        }

        public List<T> FindAliveEntities<T>(float distance, bool findForAlly, bool findForEnemy, bool findForNeutral, int overlapMask, bool findInFov = false, float fov = 0)
            where T : DamageableEntity
        {
            return FindAliveEntities<T>(EntityTransform.position, distance, findForAlly, findForEnemy, findForNeutral, overlapMask, findInFov, fov);
        }

        public T FindNearestEntity<T>(Vector3 origin, float distance, bool findForAliveOnly, bool findForAlly, bool findForEnemy, bool findForNeutral, int overlapMask, bool findInFov = false, float fov = 0)
            where T : DamageableEntity
        {
            Profiler.BeginSample("Character Entity Components - Find Nearest Characters");
            int tempOverlapSize = FindPhysicFunctions.OverlapObjects(origin, distance, overlapMask);
            Profiler.EndSample();
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
                if (tempBaseEntity.IsNull())
                    continue;
                tempEntity = tempBaseEntity.Entity as T;
                if (!IsEntityWhichLookingFor(tempEntity, findForAliveOnly, findForAlly, findForEnemy, findForNeutral, findInFov, fov))
                    continue;
                tempDistance = Vector3.Distance(EntityTransform.position, tempEntity.EntityTransform.position);
                if (tempDistance < nearestDistance)
                {
                    nearestDistance = tempDistance;
                    nearestEntity = tempEntity;
                }
            }
            return nearestEntity;
        }

        public T FindNearestAliveEntity<T>(Vector3 origin, float distance, bool findForAlly, bool findForEnemy, bool findForNeutral, int overlapMask, bool findInFov = false, float fov = 0)
            where T : DamageableEntity
        {
            return FindNearestEntity<T>(origin, distance, true, findForAlly, findForEnemy, findForNeutral, overlapMask, findInFov, fov);
        }

        public T FindNearestAliveEntity<T>(float distance, bool findForAlly, bool findForEnemy, bool findForNeutral, int overlapMask, bool findInFov = false, float fov = 0)
            where T : DamageableEntity
        {
            return FindNearestAliveEntity<T>(EntityTransform.position, distance, findForAlly, findForEnemy, findForNeutral, overlapMask, findInFov, fov);
        }

        private bool IsEntityWhichLookingFor(DamageableEntity entity, bool findForAlive, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov, float fov)
        {
            if (entity == null || entity == this)
                return false;
            if (findForAlive && entity.IsDead())
                return false;
            if (findInFov && !IsPositionInFov(fov, entity.EntityTransform.position))
                return false;
            EntityInfo instigator = GetInfo();
            return (findForAlly && entity.IsAlly(instigator)) ||
                (findForEnemy && entity.IsEnemy(instigator)) ||
                (findForNeutral && entity.IsNeutral(instigator));
        }
        #endregion

        #region Animation helpers
        public void GetRandomAnimationData(
            AnimActionType animActionType,
            int skillOrWeaponTypeDataId,
            int randomSeed,
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
                    CharacterModel.GetRandomRightHandAttackAnimation(skillOrWeaponTypeDataId, randomSeed, out animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
                    break;
                case AnimActionType.AttackLeftHand:
                    CharacterModel.GetRandomLeftHandAttackAnimation(skillOrWeaponTypeDataId, randomSeed, out animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
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

        #region Equip items models setting
        protected void PrepareToSetEquipWeaponsModels()
        {
            _countDownToSetEquipWeaponsModels = FRAMES_BEFORE_SET_EQUIP_MODEL;
        }

        protected void PrepareToSetEquipItemsModels()
        {
            _countDownToSetEquipItemsModels = FRAMES_BEFORE_SET_EQUIP_MODEL;
        }

        protected void SetEquipWeaponsModels()
        {
            CharacterModel.SetEquipWeapons(SelectableWeaponSets, EquipWeaponSet, IsWeaponsSheathed);
            if (IsOwnerClient && FpsModel != null)
                FpsModel.SetEquipWeapons(SelectableWeaponSets, EquipWeaponSet, IsWeaponsSheathed);
        }

        protected void SetEquipItemsModels()
        {
            CharacterModel.SetEquipItems(EquipItems);
            if (IsOwnerClient && FpsModel != null)
                FpsModel.SetEquipItems(EquipItems);
        }
        #endregion

        public virtual void NotifyEnemySpotted(BaseCharacterEntity enemy)
        {
            foreach (CharacterSummon summon in Summons)
            {
                if (summon.CacheEntity == null)
                    continue;
                summon.CacheEntity.NotifyEnemySpottedByAlly(this, enemy);
            }
            if (onNotifyEnemySpotted != null)
                onNotifyEnemySpotted(enemy);
        }

        public virtual void NotifyEnemySpottedByAlly(BaseCharacterEntity ally, BaseCharacterEntity enemy)
        {
            if (onNotifyEnemySpottedByAlly != null)
                onNotifyEnemySpottedByAlly(ally, enemy);
        }
    }
}
