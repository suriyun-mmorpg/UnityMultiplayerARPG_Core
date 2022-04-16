using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using LiteNetLibManager;
using LiteNetLib.Utils;
using LiteNetLib;
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
        public const float RESPAWN_INVINCIBLE_DURATION = 1f;
        public const float FIND_ENTITY_DISTANCE_BUFFER = 1f;

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
        public ICharacterAttackComponent AttackComponent { get; protected set; }
        public ICharacterUseSkillComponent UseSkillComponent { get; protected set; }
        public ICharacterReloadComponent ReloadComponent { get; protected set; }
        public ICharacterChargeComponent ChargeComponent { get; protected set; }
        public bool IsAttacking { get { return AttackComponent.IsAttacking; } }
        public float LastAttackEndTime { get { return AttackComponent.LastAttackEndTime; } }
        public float MoveSpeedRateWhileAttacking { get { return AttackComponent.MoveSpeedRateWhileAttacking; } }
        public bool IsUsingSkill { get { return UseSkillComponent.IsUsingSkill; } }
        public float LastUseSkillEndTime { get { return UseSkillComponent.LastUseSkillEndTime; } }
        public float MoveSpeedRateWhileUsingSkill { get { return UseSkillComponent.MoveSpeedRateWhileUsingSkill; } }
        public BaseSkill UsingSkill { get { return UseSkillComponent.UsingSkill; } }
        public short UsingSkillLevel { get { return UseSkillComponent.UsingSkillLevel; } }
        public bool IsCastingSkillCanBeInterrupted { get { return UseSkillComponent.IsCastingSkillCanBeInterrupted; } }
        public bool IsCastingSkillInterrupted { get { return UseSkillComponent.IsCastingSkillInterrupted; } }
        public float CastingSkillDuration { get { return UseSkillComponent.CastingSkillDuration; } }
        public float CastingSkillCountDown { get { return UseSkillComponent.CastingSkillCountDown; } }
        public short ReloadingAmmoAmount { get { return ReloadComponent.ReloadingAmmoAmount; } }
        public bool IsReloading { get { return ReloadComponent.IsReloading; } }
        public float MoveSpeedRateWhileReloading { get { return ReloadComponent.MoveSpeedRateWhileReloading; } }
        public bool IsCharging { get { return ChargeComponent.IsCharging; } }
        public float MoveSpeedRateWhileCharging { get { return ChargeComponent.MoveSpeedRateWhileCharging; } }
        public float RespawnGroundedCheckCountDown { get; protected set; }
        public float RespawnInvincibleCountDown { get; protected set; }

        protected float lastMountTime;
        protected float lastUseItemTime;
        protected float lastActionTime;
        protected float pushGameMessageCountDown;
        protected readonly Queue<UITextKeys> pushingGameMessages = new Queue<UITextKeys>();
        #endregion

        public IPhysicFunctions AttackPhysicFunctions { get; protected set; }
        public IPhysicFunctions FindPhysicFunctions { get; protected set; }

        public override bool IsImmune { get { return base.IsImmune || RespawnInvincibleCountDown > 0f; } set { base.IsImmune = value; } }
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
                meleeDamageTransform = CacheTransform;
            if (missileDamageTransform == null)
                missileDamageTransform = MeleeDamageTransform;
            if (characterUiTransform == null)
                characterUiTransform = CacheTransform;
            if (miniMapUiTransform == null)
                miniMapUiTransform = CacheTransform;
            ModelManager = gameObject.GetOrAddComponent<CharacterModelManager>();
            AttackComponent = gameObject.GetOrAddComponent<ICharacterAttackComponent, DefaultCharacterAttackComponent>();
            UseSkillComponent = gameObject.GetOrAddComponent<ICharacterUseSkillComponent, DefaultCharacterUseSkillComponent>();
            ReloadComponent = gameObject.GetOrAddComponent<ICharacterReloadComponent, DefaultCharacterReloadComponent>();
            ChargeComponent = gameObject.GetOrAddComponent<ICharacterChargeComponent, DefaultCharacterChargeComponent>();
            gameObject.GetOrAddComponent<CharacterRecoveryComponent>();
            gameObject.GetOrAddComponent<CharacterSkillAndBuffComponent>();
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
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
            isRecaching = true;
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
            MakeCaches();
            float deltaTime = Time.deltaTime;

            if (IsServer && CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
                // Ground check / ground damage will be calculated at server while dimension type is 3d only
                if (!lastGrounded && MovementState.Has(MovementState.IsGrounded))
                {
                    // Apply fall damage when not passenging vehicle
                    CurrentGameplayRule.ApplyFallDamage(this, lastGroundedPosition);
                }
                lastGrounded = MovementState.Has(MovementState.IsGrounded);
                if (lastGrounded)
                    lastGroundedPosition = CacheTransform.position;
            }

            bool tempEnableMovement = PassengingVehicleEntity == null;
            if (RespawnGroundedCheckCountDown > 0f)
            {
                // Character won't receive fall damage
                RespawnGroundedCheckCountDown -= deltaTime;
            }
            else
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

            if (RespawnInvincibleCountDown > 0f)
            {
                // Character won't receive damage
                RespawnInvincibleCountDown -= deltaTime;
            }

            // Clear data when character dead
            if (this.IsDead())
                ExitVehicle();

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
            // Set character model hide state
            ModelManager.SetIsHide(CharacterModelManager.HIDE_SETTER_ENTITY, IsHide());
            // Update model animations
            // Update is dead state
            CharacterModel.SetIsDead(this.IsDead());
            // Update move speed multiplier
            CharacterModel.SetMoveAnimationSpeedMultiplier(MoveAnimationSpeedMultiplier);
            // Update movement animation
            CharacterModel.SetMovementState(MovementState, ExtraMovementState, Direction2D, this.GetCaches().FreezeAnimation);
            // Update FPS model
            if (IsClient && FpsModel && FpsModel.gameObject.activeSelf)
            {
                // Update is dead state
                FpsModel.SetIsDead(this.IsDead());
                // Update move speed multiplier
                FpsModel.SetMoveAnimationSpeedMultiplier(MoveAnimationSpeedMultiplier);
                // Update movement animation
                FpsModel.SetMovementState(MovementState, ExtraMovementState, Direction2D, this.GetCaches().FreezeAnimation);
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
            if (ActiveMovement != null && ActiveMovement.WriteClientState(EntityStateDataWriter, out shouldSendReliably))
                inputState |= CharacterInputState.IsMoving;
            // Set input state and send to clients
            if (inputState != CharacterInputState.None)
            {
                TransportHandler.WritePacket(EntityStateMessageWriter, GameNetworkingConsts.EntityState);
                EntityStateMessageWriter.PutPackedUShort((ushort)inputState);
                EntityStateMessageWriter.Put(EntityStateDataWriter.Data, 0, EntityStateDataWriter.Length);
                ClientSendMessage(CLIENT_STATE_DATA_CHANNEL, (shouldSendReliably || (ushort)inputState > 1 << 0) ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Sequenced, EntityStateMessageWriter);
            }
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
            if (ActiveMovement != null && ActiveMovement.WriteServerState(EntityStateDataWriter, out shouldSendReliably))
                inputState |= CharacterInputState.IsMoving;
            // Set input state and send to clients
            if (inputState != CharacterInputState.None)
            {
                TransportHandler.WritePacket(EntityStateMessageWriter, GameNetworkingConsts.EntityState);
                EntityStateMessageWriter.PutPackedUInt(ObjectId);
                EntityStateMessageWriter.PutPackedUShort((ushort)inputState);
                EntityStateMessageWriter.Put(EntityStateDataWriter.Data, 0, EntityStateDataWriter.Length);
                ServerSendMessageToSubscribers(SERVER_STATE_DATA_CHANNEL, (shouldSendReliably || (ushort)inputState > 1 << 0) ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Sequenced, EntityStateMessageWriter);
            }
        }

        public override void ReadClientStateAtServer(NetDataReader reader)
        {
            CharacterInputState inputState = (CharacterInputState)reader.GetPackedUShort();
            // Actions
            if (inputState.Has(CharacterInputState.IsAttacking))
                AttackComponent.ReadClientAttackStateAtServer(reader);
            if (inputState.Has(CharacterInputState.IsUsingSkillInterrupted))
                UseSkillComponent.ReadClientUseSkillInterruptedStateAtServer(reader);
            if (inputState.Has(CharacterInputState.IsUsingSkillItem))
                UseSkillComponent.ReadClientUseSkillItemStateAtServer(reader);
            if (inputState.Has(CharacterInputState.IsUsingSkill))
                UseSkillComponent.ReadClientUseSkillStateAtServer(reader);
            if (inputState.Has(CharacterInputState.IsReloading))
                ReloadComponent.ReadClientReloadStateAtServer(reader);
            if (inputState.Has(CharacterInputState.IsChargeStopping))
                ChargeComponent.ReadClientStopChargeStateAtServer(reader);
            if (inputState.Has(CharacterInputState.IsChargeStarting))
                ChargeComponent.ReadClientStartChargeStateAtServer(reader);
            // Movement
            if (inputState.Has(CharacterInputState.IsMoving) && ActiveMovement != null)
                ActiveMovement.ReadClientStateAtServer(reader);
        }

        public override void ReadServerStateAtClient(NetDataReader reader)
        {
            CharacterInputState inputState = (CharacterInputState)reader.GetPackedUShort();
            // Actions
            if (inputState.Has(CharacterInputState.IsAttacking))
                AttackComponent.ReadServerAttackStateAtClient(reader);
            if (inputState.Has(CharacterInputState.IsUsingSkillInterrupted))
                UseSkillComponent.ReadServerUseSkillInterruptedStateAtClient(reader);
            if (inputState.Has(CharacterInputState.IsUsingSkillItem))
                UseSkillComponent.ReadServerUseSkillItemStateAtClient(reader);
            if (inputState.Has(CharacterInputState.IsUsingSkill))
                UseSkillComponent.ReadServerUseSkillStateAtClient(reader);
            if (inputState.Has(CharacterInputState.IsReloading))
                ReloadComponent.ReadServerReloadStateAtClient(reader);
            if (inputState.Has(CharacterInputState.IsChargeStopping))
                ChargeComponent.ReadServerStopChargeStateAtClient(reader);
            if (inputState.Has(CharacterInputState.IsChargeStarting))
                ChargeComponent.ReadServerStartChargeStateAtClient(reader);
            // Movement
            if (inputState.Has(CharacterInputState.IsMoving) && ActiveMovement != null)
                ActiveMovement.ReadServerStateAtClient(reader);
        }

        protected override void OnTeleport(Vector3 position, Quaternion rotation)
        {
            base.OnTeleport(position, rotation);
            // Clear target entity when teleport
            SetTargetEntity(null);
        }

        public override void PlayJumpAnimation()
        {
            base.PlayJumpAnimation();
            if (IsClient && FpsModel && FpsModel.gameObject.activeSelf)
            {
                if (FpsModel is IJumppableModel)
                    (FpsModel as IJumppableModel).PlayJumpAnimation();
            }
        }

        public override void PlayPickupAnimation()
        {
            base.PlayPickupAnimation();
            if (IsClient && FpsModel && FpsModel.gameObject.activeSelf)
            {
                if (FpsModel is IPickupableModel)
                    (FpsModel as IPickupableModel).PlayPickupAnimation();
            }
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
        public bool ValidateAttack(bool isLeftHand)
        {
            if (!CanAttack())
                return false;

            if (!UpdateLastActionTime())
                return false;

            CharacterItem weapon = this.GetAvailableWeapon(ref isLeftHand);
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            if (!ValidateAmmo(weapon, 1))
            {
                QueueGameMessage(UITextKeys.UI_ERROR_NO_AMMO);
                if (weaponItem != null)
                    AudioManager.PlaySfxClipAtAudioSource(weaponItem.EmptyClip, CharacterModel.GenericAudioSource);
                return false;
            }
            return true;
        }

        public bool ValidateUseSkill(int dataId, bool isLeftHand, uint targetObjectId)
        {
            if (!CanUseSkill())
                return false;

            if (!UpdateLastActionTime())
                return false;

            UITextKeys gameMessage;
            if (!this.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out _, out _, out gameMessage))
            {
                if (gameMessage != UITextKeys.NONE)
                    QueueGameMessage(gameMessage);
                return false;
            }

            return true;
        }

        public bool ValidateUseSkillItem(short index, bool isLeftHand, uint targetObjectId)
        {
            if (!CanUseItem())
                return false;

            if (!UpdateLastActionTime())
                return false;

            UITextKeys gameMessage;
            if (!this.ValidateSkillItemToUse(index, isLeftHand, targetObjectId, out _, out _, out gameMessage))
            {
                if (gameMessage != UITextKeys.NONE)
                    QueueGameMessage(gameMessage);
                return false;
            }

            return true;
        }

        public bool ValidateReload(bool isLeftHand)
        {
            if (!CanDoActions())
                return false;
            if (!isLeftHand && EquipWeapons.rightHand.IsAmmoFull())
                return false;
            if (isLeftHand && EquipWeapons.leftHand.IsAmmoFull())
                return false;
            return true;
        }

        public bool Attack(bool isLeftHand)
        {
            if (!IsOwnerClientOrOwnedByServer)
                return false;
            if (ValidateAttack(isLeftHand))
            {
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
                UseSkillComponent.UseSkill(dataId, isLeftHand, targetObjectId, aimPosition);
                return true;
            }
            return false;
        }

        public bool UseSkillItem(short itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (!IsOwnerClientOrOwnedByServer)
                return false;
            if (ValidateUseSkillItem(itemIndex, isLeftHand, targetObjectId))
            {
                UseSkillComponent.UseSkillItem(itemIndex, isLeftHand, targetObjectId, aimPosition);
                return true;
            }
            return false;
        }

        public void InterruptCastingSkill()
        {
            UseSkillComponent.InterruptCastingSkill();
        }

        public bool StartCharge(bool isLeftHand)
        {
            if (!IsOwnerClientOrOwnedByServer)
                return false;
            if (ValidateAttack(isLeftHand))
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
            ChargeComponent.StopCharge();
            return true;
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
            if (time - lastActionTime < ACTION_DELAY)
                return false;
            lastActionTime = time;
            return true;
        }

        public bool CanDoNextAction()
        {
            return Time.unscaledTime - lastActionTime >= ACTION_DELAY;
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

        public AimPosition GetAttackAimPosition(DamageInfo damageInfo, bool isLeftHand)
        {
            Vector3 position = damageInfo.GetDamageTransform(this, isLeftHand).position;
            Vector3 direction = CacheTransform.forward;
            BaseGameEntity targetEntity = GetTargetEntity();
            if (targetEntity && targetEntity != Entity)
            {
                if (targetEntity is DamageableEntity)
                {
                    return GetAttackAimPosition(position, (targetEntity as DamageableEntity).OpponentAimTransform.position);
                }
                else
                {
                    return GetAttackAimPosition(position, targetEntity.CacheTransform.position);
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
            animActionType = !isLeftHand ? AnimActionType.ReloadRightHand : AnimActionType.ReloadLeftHand;
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
            animActionType = !isLeftHand ? AnimActionType.AttackRightHand : AnimActionType.AttackLeftHand;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetWeaponDamagesWithBuffs(CharacterItem weapon)
        {
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Calculate all damages
            damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, this.GetWeaponDamages(weapon));
            // Sum damage with buffs
            damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, this.GetCaches().IncreaseDamages);

            return damageAmounts;
        }

        public bool ValidateAmmo(CharacterItem weapon, short amount, bool validIfNoRequireAmmoType = true)
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

        public bool DecreaseAmmos(CharacterItem weapon, bool isLeftHand, short amount, out Dictionary<DamageElement, MinMaxFloat> increaseDamages, bool validIfNoRequireAmmoType = true)
        {
            increaseDamages = null;

            // Avoid null data
            if (weapon == null)
                return validIfNoRequireAmmoType;

            IWeaponItem weaponItem = weapon.GetWeaponItem();
            if (weaponItem.WeaponType.RequireAmmoType != null)
            {
                if (weaponItem.AmmoCapacity <= 0)
                {
                    // Ammo capacity is 0 so reduce ammo from inventory
                    if (this.DecreaseAmmos(weaponItem.WeaponType.RequireAmmoType, amount, out increaseDamages))
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

        public virtual bool CanDoActions()
        {
            return !this.IsDead() && !IsAttacking && !IsUsingSkill && !IsReloading && !IsPlayingActionAnimation();
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

            if (this.GetCaches().IsOverweight)
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
            if (this.GetCaches().DisallowMove)
                return false;
            return true;
        }

        public override sealed bool CanSprint()
        {
            if (!MovementState.Has(MovementState.IsGrounded) || MovementState.Has(MovementState.IsUnderWater))
                return false;
            return CurrentStamina > 0;
        }

        public override sealed bool CanCrouch()
        {
            if (!MovementState.Has(MovementState.IsGrounded) || MovementState.Has(MovementState.IsUnderWater))
                return false;
            return true;
        }

        public override sealed bool CanCrawl()
        {
            if (!MovementState.Has(MovementState.IsGrounded) || MovementState.Has(MovementState.IsUnderWater))
                return false;
            return true;
        }

        public override sealed bool IsHide()
        {
            return this.GetCaches().IsHide;
        }

        public virtual bool CanAttack()
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

        public virtual bool CanUseSkill()
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

        public virtual bool CanUseItem()
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

        public List<T> FindCharacters<T>(Vector3 origin, float distance, bool findForAlive, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            List<T> result = new List<T>();
            int tempOverlapSize = FindPhysicFunctions.OverlapObjects(origin, distance, CurrentGameInstance.playerLayer.Mask | CurrentGameInstance.monsterLayer.Mask);
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

        public List<T> FindCharacters<T>(float distance, bool findForAlive, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            return FindCharacters<T>(CacheTransform.position, distance, findForAlive, findForAlly, findForEnemy, findForNeutral, findInFov, fov);
        }

        public List<T> FindAliveCharacters<T>(Vector3 origin, float distance, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            return FindCharacters<T>(origin, distance, true, findForAlly, findForEnemy, findForNeutral, findInFov, fov);
        }

        public List<T> FindAliveCharacters<T>(float distance, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            return FindAliveCharacters<T>(CacheTransform.position, distance, findForAlly, findForEnemy, findForNeutral, findInFov, fov);
        }

        public T FindNearestCharacter<T>(Vector3 origin, float distance, bool findForAliveOnly, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            int tempOverlapSize = FindPhysicFunctions.OverlapObjects(origin, distance, CurrentGameInstance.playerLayer.Mask | CurrentGameInstance.monsterLayer.Mask);
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

        public T FindNearestAliveCharacter<T>(Vector3 origin, float distance, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            return FindNearestCharacter<T>(origin, distance, true, findForAlly, findForEnemy, findForNeutral, findInFov, fov);
        }

        public T FindNearestAliveCharacter<T>(float distance, bool findForAlly, bool findForEnemy, bool findForNeutral, bool findInFov = false, float fov = 0)
            where T : BaseCharacterEntity
        {
            return FindNearestAliveCharacter<T>(CacheTransform.position, distance, findForAlly, findForEnemy, findForNeutral, findInFov, fov);
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

        public virtual void NotifyEnemySpottedToAllies(BaseCharacterEntity enemy)
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
