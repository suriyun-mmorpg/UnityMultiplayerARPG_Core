using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterActionComponentManager))]
    public class DefaultCharacterUseSkillComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterUseSkillComponent
    {
        protected struct UseSkillState
        {
            public int SimulateSeed;
            public bool IsInterrupted;
            public bool UseItem;
            public int ItemIndex;
            public int? ItemDataId;
            public BaseSkill Skill;
            public int SkillLevel;
            public bool IsLeftHand;
            public uint TargetObjectId;
            public AimPosition AimPosition;
        }

        protected readonly List<CancellationTokenSource> _skillCancellationTokenSources = new List<CancellationTokenSource>();
        public BaseSkill UsingSkill
        {
            get
            {
                if (!IsUsingSkill)
                    return null;
                return _simulateState.Value.Skill;
            }
        }
        public int UsingSkillLevel
        {
            get
            {
                if (!IsUsingSkill)
                    return 0;
                return _simulateState.Value.SkillLevel;
            }
        }
        public bool IsUsingSkill
        {
            get
            {
                return _simulateState.HasValue;
            }
        }
        public float LastUseSkillEndTime { get; protected set; }
        protected bool _skipMovementValidation;
        public bool IsSkipMovementValidationWhileUsingSkill { get { return _skipMovementValidation; } protected set { _skipMovementValidation = value; } }
        protected bool _shouldUseRootMotion;
        public bool IsUseRootMotionWhileUsingSkill { get { return _shouldUseRootMotion; } protected set { _shouldUseRootMotion = value; } }
        public bool IsCastingSkillCanBeInterrupted { get; protected set; }
        public bool IsCastingSkillInterrupted { get; protected set; }
        public float CastingSkillDuration { get; protected set; }
        public float CastingSkillCountDown { get; protected set; }
        public float MoveSpeedRateWhileUsingSkill { get; protected set; }
        public MovementRestriction MovementRestrictionWhileUsingSkill { get; protected set; }
        public System.Action OnCastSkillStart { get; set; }
        public System.Action OnCastSkillEnd { get; set; }
        public System.Action OnUseSkillStart { get; set; }
        public System.Action<int> OnUseSkillTrigger { get; set; }
        public System.Action OnUseSkillEnd { get; set; }
        public System.Action OnSkillInterupted { get; set; }
        public AnimActionType AnimActionType { get; protected set; }
        public int AnimActionDataId { get; protected set; }
        public IHitRegistrationManager HitRegistrationManager { get { return BaseGameNetworkManager.Singleton.HitRegistrationManager; } }

        protected CharacterActionComponentManager _manager;
        // Network data sending
        protected UseSkillState? _simulateState;
        // Logging data
        bool _entityIsPlayer = false;
        BasePlayerCharacterEntity _playerCharacterEntity = null;

        public override void EntityStart()
        {
            _manager = GetComponent<CharacterActionComponentManager>();
            if (Entity is BasePlayerCharacterEntity)
            {
                _entityIsPlayer = true;
                _playerCharacterEntity = Entity as BasePlayerCharacterEntity;
            }
        }

        public override void EntityOnDestroy()
        {
            CancelSkill();
            ClearUseSkillStates();
            _manager = null;
            _entityIsPlayer = false;
            _playerCharacterEntity = null;
        }

        public override void EntityUpdate()
        {
            // Update casting skill count down, will show gage at clients
            if (CastingSkillCountDown > 0)
                CastingSkillCountDown -= Time.unscaledDeltaTime;
        }

        protected virtual void SetUseSkillActionStates(AnimActionType animActionType, int animActionDataId, UseSkillState simulateState)
        {
            ClearUseSkillStates();
            AnimActionType = animActionType;
            AnimActionDataId = animActionDataId;
            _simulateState = simulateState;
        }

        public virtual void ClearUseSkillStates()
        {
            _simulateState = null;
        }

        protected virtual async UniTaskVoid UseSkillRoutine(long peerTimestamp, UseSkillState simulateState)
        {
            int simulateSeed = GetSimulateSeed(peerTimestamp);
            bool isLeftHand = simulateState.IsLeftHand;
            BaseSkill skill = simulateState.Skill;
            int skillLevel = simulateState.SkillLevel;
            uint targetObjectId = simulateState.TargetObjectId;
            AimPosition skillAimPosition = simulateState.AimPosition;
            int? itemDataId = simulateState.ItemDataId;
            if (simulateState.SimulateSeed == 0)
                simulateState.SimulateSeed = simulateSeed;
            else
                simulateSeed = simulateState.SimulateSeed;

            // Prepare required data and get skill data
            Entity.GetUsingSkillData(
                skill,
                ref isLeftHand,
                out AnimActionType animActionType,
                out int animActionDataId,
                out CharacterItem weapon,
                out DamageInfo damageInfo);

            // Prepare required data and get animation data
            Entity.GetRandomAnimationData(
                animActionType,
                animActionDataId,
                simulateSeed,
                out int animationIndex,
                out float animSpeedRate,
                out float[] triggerDurations,
                out float totalDuration);

            // Set doing action state at clients and server
            SetUseSkillActionStates(animActionType, animActionDataId, simulateState);

            if (IsServer)
            {
                // Update skill usage states at server only
                if (itemDataId.HasValue)
                {
                    Entity.AddOrUpdateSkillUsage(SkillUsageType.UsableItem, itemDataId.Value, skillLevel);
                }
                else
                {
                    Entity.AddOrUpdateSkillUsage(SkillUsageType.Skill, skill.DataId, skillLevel);
                }
                // Do something with buffs when use skill
                Entity.SkillAndBuffComponent.OnUseSkill();
            }

            // Prepare required data and get damages data
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            Dictionary<DamageElement, MinMaxFloat> baseDamageAmounts = skill.GetAttackDamages(Entity, skillLevel, isLeftHand);

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileUsingSkill = skill.moveSpeedRateWhileUsingSkill;
            MovementRestrictionWhileUsingSkill = skill.movementRestrictionWhileUsingSkill;

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            animSpeedRate *= Entity.GetAnimSpeedRate(AnimActionType);

            // Set doing action data
            IsCastingSkillCanBeInterrupted = skill.canBeInterruptedWhileCasting;
            IsCastingSkillInterrupted = false;

            // Get cast duration. Then if cast duration more than 0, it will play cast skill animation.
            CastingSkillDuration = CastingSkillCountDown = skill.GetCastDuration(skillLevel);

            // Prepare cancellation
            CancellationTokenSource skillCancellationTokenSource = new CancellationTokenSource();
            _skillCancellationTokenSources.Add(skillCancellationTokenSource);

            try
            {
                BaseCharacterModel tpsModel = Entity.CharacterModel;
                bool tpsModelAvailable = tpsModel != null && tpsModel.gameObject.activeSelf;
                BaseCharacterModel vehicleModel = Entity.PassengingVehicleModel as BaseCharacterModel;
                bool vehicleModelAvailable = vehicleModel != null;
                bool overridePassengerActionAnimations = vehicleModelAvailable && Entity.PassengingVehicleSeat.overridePassengerActionAnimations;
                BaseCharacterModel fpsModel = Entity.FpsModel;
                bool fpsModelAvailable = IsClient && fpsModel != null && fpsModel.gameObject.activeSelf;

                // Prepare end time
                LastUseSkillEndTime = CharacterActionComponentManager.PrepareActionEndTime(totalDuration, animSpeedRate, CastingSkillDuration);

                // Play special effect
                if (IsClient)
                {
                    if (!overridePassengerActionAnimations)
                    {
                        if (tpsModelAvailable)
                        {
                            tpsModel.InstantiateEffect(skill.SkillCastEffects);
                            tpsModel.InstantiateEffect(skill.AddressableSkillCastEffects).Forget();
                        }
                        if (fpsModelAvailable)
                        {
                            fpsModel.InstantiateEffect(skill.SkillCastEffects);
                            fpsModel.InstantiateEffect(skill.AddressableSkillCastEffects).Forget();
                        }
                    }
                    else if (vehicleModelAvailable)
                    {
                        vehicleModel.InstantiateEffect(skill.SkillCastEffects);
                        vehicleModel.InstantiateEffect(skill.AddressableSkillCastEffects).Forget();
                    }
                }

                if (CastingSkillDuration > 0f)
                {
                    OnCastSkillStart?.Invoke();
                    // Play cast animation
                    if (vehicleModelAvailable)
                        vehicleModel.PlaySkillCastClip(skill.DataId, CastingSkillDuration, out _skipMovementValidation, out _shouldUseRootMotion);
                    if (!overridePassengerActionAnimations)
                    {
                        if (tpsModelAvailable)
                            tpsModel.PlaySkillCastClip(skill.DataId, CastingSkillDuration, out _skipMovementValidation, out _shouldUseRootMotion);
                        if (fpsModelAvailable)
                            fpsModel.PlaySkillCastClip(skill.DataId, CastingSkillDuration, out _, out _);
                    }
                    // Wait until end of cast duration
                    await UniTask.Delay((int)(CastingSkillDuration * 1000f), true, PlayerLoopTiming.FixedUpdate, skillCancellationTokenSource.Token);
                    OnCastSkillEnd?.Invoke();
                }

                // Play special effect
                if (IsClient)
                {
                    if (vehicleModelAvailable)
                    {
                        vehicleModel.InstantiateEffect(skill.SkillActivateEffects);
                        vehicleModel.InstantiateEffect(skill.AddressableSkillActivateEffects).Forget();
                    }
                    if (!overridePassengerActionAnimations)
                    {
                        if (tpsModelAvailable)
                        {
                            tpsModel.InstantiateEffect(skill.SkillActivateEffects);
                            tpsModel.InstantiateEffect(skill.AddressableSkillActivateEffects).Forget();
                        }
                        if (fpsModelAvailable)
                        {
                            fpsModel.InstantiateEffect(skill.SkillActivateEffects);
                            fpsModel.InstantiateEffect(skill.AddressableSkillActivateEffects).Forget();
                        }
                    }
                }

                // Play action animation
                if (vehicleModelAvailable)
                    vehicleModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, out _skipMovementValidation, out _shouldUseRootMotion, animSpeedRate);
                if (!overridePassengerActionAnimations)
                {
                    if (tpsModelAvailable)
                        tpsModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, out _skipMovementValidation, out _shouldUseRootMotion, animSpeedRate);
                    if (fpsModelAvailable)
                        fpsModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, out _, out _, animSpeedRate);
                }

                // Try setup state data (maybe by animation clip events or state machine behaviours), if it was not set up
                float remainsDuration = totalDuration;
                await _manager.PrepareActionDurations(triggerDurations, totalDuration, 0f, animSpeedRate, skillCancellationTokenSource.Token,
                    (__triggerDurations, __totalDuration, __remainsDuration, __endTime) =>
                    {
                        triggerDurations = __triggerDurations;
                        totalDuration = __totalDuration;
                        remainsDuration = __remainsDuration;
                        LastUseSkillEndTime = __endTime;
                    });

                // Prepare damage amounts
                List<Dictionary<DamageElement, MinMaxFloat>> damageAmounts = skill.PrepareDamageAmounts(Entity, isLeftHand, baseDamageAmounts, triggerDurations.Length);

                // Prepare hit register validation, it will be used later when receive attack start/end events from clients
                if ((IsServer && !IsOwnerClient) || !IsOwnedByServer)
                    HitRegistrationManager.PrepareHitRegValidation(Entity, simulateSeed, triggerDurations, 0, damageInfo, damageAmounts, isLeftHand, weapon, skill, skillLevel);
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogUseSkillStart(_playerCharacterEntity, simulateSeed, triggerDurations, weaponItem.FireSpreadAmount, isLeftHand, weapon, skill, skillLevel);
                OnUseSkillStart?.Invoke();

                for (byte triggerIndex = 0; triggerIndex < triggerDurations.Length; ++triggerIndex)
                {
                    // Play special effects after trigger duration
                    float tempTriggerDuration = triggerDurations[triggerIndex];
                    remainsDuration -= tempTriggerDuration;
                    await UniTask.Delay((int)(tempTriggerDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.FixedUpdate, skillCancellationTokenSource.Token);

                    // Special effects will plays on clients only
                    if (IsClient && (AnimActionType == AnimActionType.AttackRightHand || AnimActionType == AnimActionType.AttackLeftHand))
                    {
                        // Play weapon launch special effects
                        if (tpsModelAvailable)
                            tpsModel.PlayEquippedWeaponLaunch(isLeftHand);
                        if (fpsModelAvailable)
                            fpsModel.PlayEquippedWeaponLaunch(isLeftHand);
                        // Play launch sfx
                        weaponItem.LaunchClip?.Play(tpsModel.GenericAudioSource);
                    }

                    // Get aim position by character's forward
                    AimPosition aimPosition;
                    if (skill.HasCustomAimControls() && skillAimPosition.type == AimPositionType.Position)
                        aimPosition = skillAimPosition;
                    else
                        aimPosition = Entity.AimPosition;

                    // Trigger skill event
                    OnUseSkillTrigger?.Invoke(triggerIndex);
                    Entity.OnUseSkillRoutine(skill, skillLevel, isLeftHand, weapon, simulateSeed, triggerIndex, damageAmounts, targetObjectId, aimPosition);

                    // Apply skill buffs, summons and attack damages
                    if (IsServer)
                    {
                        if (!skill.DecreaseResources(Entity, weapon, isLeftHand, out _))
                            continue;
                        if (!IsOwnerClient && !IsOwnedByServer)
                            continue;
                        RPC(RpcSimulateActionTrigger, BaseGameEntity.ACTION_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, new SimulateActionTriggerData()
                        {
                            simulateSeed = simulateSeed,
                            triggerIndex = triggerIndex,
                            targetObjectId = targetObjectId,
                            aimPosition = aimPosition,
                        });
                        ApplySkillUsing(skill, skillLevel, isLeftHand, weapon, simulateSeed, triggerIndex, damageAmounts, targetObjectId, aimPosition);
                    }
                    else if (IsOwnerClient)
                    {
                        RPC(CmdSimulateActionTrigger, BaseGameEntity.ACTION_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, new SimulateActionTriggerData()
                        {
                            simulateSeed = simulateSeed,
                            triggerIndex = triggerIndex,
                            targetObjectId = targetObjectId,
                            aimPosition = aimPosition,
                        });
                        ApplySkillUsing(skill, skillLevel, isLeftHand, weapon, simulateSeed, triggerIndex, damageAmounts, targetObjectId, aimPosition);
                    }

                    if (remainsDuration <= 0f)
                    {
                        // Stop trigger animations loop
                        break;
                    }
                }

                // Decrease items
                if (IsServer && itemDataId.HasValue && Entity.DecreaseItems(itemDataId.Value, 1))
                    Entity.FillEmptySlots();

                if (remainsDuration > 0f)
                {
                    // Wait until animation ends to stop actions
                    await UniTask.Delay((int)(remainsDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.FixedUpdate, skillCancellationTokenSource.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Catch the cancellation
                OnSkillInterupted?.Invoke();
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogUseSkillInterrupt(_playerCharacterEntity, simulateSeed);
            }
            catch (System.Exception ex)
            {
                // Other errors
                Debug.LogError("Error occuring in `DefaultCharacterUseSkillComponent` -> `UseSkillRoutine`, " + this);
                Debug.LogException(ex);
            }
            finally
            {
                LastUseSkillEndTime = Time.unscaledTime;
                skillCancellationTokenSource.Dispose();
                _skillCancellationTokenSources.Remove(skillCancellationTokenSource);
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogUseSkillEnd(_playerCharacterEntity, simulateSeed);
                OnUseSkillEnd?.Invoke();
            }
            // Clear action states at clients and server
            ClearUseSkillStates();
        }

        [ServerRpc]
        protected void CmdSimulateActionTrigger(SimulateActionTriggerData data)
        {
            HitValidateData validateData = HitRegistrationManager.GetHitValidateData(Entity, data.simulateSeed);
            if (validateData == null || validateData.Skill == null)
            {
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogUseSkillTriggerFail(_playerCharacterEntity, data.simulateSeed, data.triggerIndex, ActionTriggerFailReasons.NoValidateData);
                return;
            }
            if (data.triggerIndex >= validateData.DamageAmounts.Count)
            {
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogUseSkillTriggerFail(_playerCharacterEntity, data.simulateSeed, data.triggerIndex, ActionTriggerFailReasons.NotEnoughResources);
                return;
            }
            RPC(RpcSimulateActionTrigger, BaseGameEntity.ACTION_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, data);
            ApplySkillUsing(validateData.Skill, validateData.SkillLevel, validateData.IsLeftHand, validateData.Weapon, data.simulateSeed, data.triggerIndex, validateData.DamageAmounts, data.targetObjectId, data.aimPosition);
            if (_entityIsPlayer && IsServer)
                GameInstance.ServerLogHandlers.LogUseSkillTrigger(_playerCharacterEntity, data.simulateSeed, data.triggerIndex);
        }

        [AllRpc]
        protected void RpcSimulateActionTrigger(SimulateActionTriggerData data)
        {
            if (IsServer)
                return;
            if (IsOwnerClientOrOwnedByServer)
                return;
            HitValidateData validateData = HitRegistrationManager.GetHitValidateData(Entity, data.simulateSeed);
            if (validateData == null || validateData.Skill == null)
                return;
            ApplySkillUsing(validateData.Skill, validateData.SkillLevel, validateData.IsLeftHand, validateData.Weapon, data.simulateSeed, data.triggerIndex, validateData.DamageAmounts, data.targetObjectId, data.aimPosition);
        }

        protected virtual void ApplySkillUsing(BaseSkill skill, int skillLevel, bool isLeftHand, CharacterItem weapon, int simulateSeed, byte triggerIndex, List<Dictionary<DamageElement, MinMaxFloat>> damageAmounts, uint targetObjectId, AimPosition aimPosition)
        {
            if (triggerIndex >= damageAmounts.Count)
            {
                // No damage applied (may not have enough ammo)
                return;
            }

            skill.ApplySkill(
                Entity,
                skillLevel,
                isLeftHand,
                weapon,
                simulateSeed,
                triggerIndex,
                damageAmounts,
                targetObjectId,
                aimPosition);
        }

        public virtual void UseSkill(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            long timestamp = Manager.ServerTimestamp;
            if (!IsServer && IsOwnerClient)
            {
                if (!Entity.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out BaseSkill skill, out int skillLevel, out UITextKeys gameMessage))
                {
                    ClientGenericActions.ClientReceiveGameMessage(gameMessage);
                    return;
                }
                ProceedUseSkill(timestamp, skill, skillLevel, isLeftHand, targetObjectId, aimPosition);
                RPC(CmdUseSkill, BaseGameEntity.ACTION_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, timestamp, dataId, isLeftHand, targetObjectId, aimPosition);
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                ProceedCmdUseSkill(timestamp, dataId, isLeftHand, targetObjectId, aimPosition);
            }
        }

        [ServerRpc]
        protected void CmdUseSkill(long peerTimestamp, int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            ProceedCmdUseSkill(peerTimestamp, dataId, isLeftHand, targetObjectId, aimPosition);
        }

        protected void ProceedCmdUseSkill(long peerTimestamp, int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (!Entity.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out BaseSkill skill, out int skillLevel, out _))
                return;
            ProceedUseSkill(peerTimestamp, skill, skillLevel, isLeftHand, targetObjectId, aimPosition);
            RPC(RpcUseSkill, BaseGameEntity.ACTION_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, peerTimestamp, dataId, skillLevel, isLeftHand, targetObjectId, aimPosition);
        }

        [AllRpc]
        protected void RpcUseSkill(long peerTimestamp, int dataId, int skillLevel, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (IsServer || IsOwnerClient)
            {
                // Don't play attacking animation again
                return;
            }
            if (!GameInstance.Skills.TryGetValue(dataId, out BaseSkill skill))
                return;
            ProceedUseSkill(peerTimestamp, skill, skillLevel, isLeftHand, targetObjectId, aimPosition);
        }

        protected void ProceedUseSkill(long peerTimestamp, BaseSkill skill, int skillLevel, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            UseSkillState simulateState = new UseSkillState()
            {
                SimulateSeed = GetSimulateSeed(peerTimestamp),
                Skill = skill,
                SkillLevel = skillLevel,
                IsLeftHand = isLeftHand,
                TargetObjectId = targetObjectId,
                AimPosition = aimPosition,
            };
            UseSkillRoutine(peerTimestamp, simulateState).Forget();
        }

        public virtual void UseSkillItem(int itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            long timestamp = Manager.ServerTimestamp;
            if (!IsServer && IsOwnerClient)
            {
                if (!Entity.ValidateSkillItemToUse(itemIndex, isLeftHand, targetObjectId, out ISkillItem skillItem, out BaseSkill skill, out int skillLevel, out UITextKeys gameMessage))
                {
                    ClientGenericActions.ClientReceiveGameMessage(gameMessage);
                    return;
                }
                Entity.LastUseItemTime = Time.unscaledTime;
                ProceedUseSkillItem(timestamp, skillItem, skill, skillLevel, isLeftHand, targetObjectId, aimPosition);
                RPC(CmdUseSkillItem, BaseGameEntity.ACTION_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, timestamp, itemIndex, isLeftHand, targetObjectId, aimPosition);
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                ProceedCmdUseSkillItem(timestamp, itemIndex, isLeftHand, targetObjectId, aimPosition);
            }
        }

        [ServerRpc]
        protected void CmdUseSkillItem(long peerTimestamp, int itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            ProceedCmdUseSkillItem(peerTimestamp, itemIndex, isLeftHand, targetObjectId, aimPosition);
        }

        protected void ProceedCmdUseSkillItem(long peerTimestamp, int itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (!Entity.ValidateSkillItemToUse(itemIndex, isLeftHand, targetObjectId, out ISkillItem skillItem, out BaseSkill skill, out int skillLevel, out _))
                return;
            ProceedUseSkillItem(peerTimestamp, skillItem, skill, skillLevel, isLeftHand, targetObjectId, aimPosition);
            RPC(RpcUseSkillItem, BaseGameEntity.ACTION_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, peerTimestamp, skillItem.DataId, isLeftHand, targetObjectId, aimPosition);
        }

        [AllRpc]
        protected void RpcUseSkillItem(long peerTimestamp, int itemDataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (IsServer || IsOwnerClient)
            {
                // Don't play attacking animation again
                return;
            }
            if (!GameInstance.Items.TryGetValue(itemDataId, out BaseItem item) || !(item is ISkillItem skillItem))
                return;
            ProceedUseSkillItem(peerTimestamp, skillItem, skillItem.SkillData, skillItem.SkillLevel, isLeftHand, targetObjectId, aimPosition);
        }

        protected void ProceedUseSkillItem(long peerTimestamp, ISkillItem skillItem, BaseSkill skill, int skillLevel, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            UseSkillState simulateState = new UseSkillState()
            {
                SimulateSeed = GetSimulateSeed(peerTimestamp),
                ItemDataId = skillItem.DataId,
                Skill = skill,
                SkillLevel = skillLevel,
                IsLeftHand = isLeftHand,
                TargetObjectId = targetObjectId,
                AimPosition = aimPosition,
            };
            UseSkillRoutine(peerTimestamp, simulateState).Forget();
        }

        public virtual void InterruptCastingSkill()
        {
            if (!IsServer)
            {
                RPC(CmdInterruptCastingSkill, BaseGameEntity.ACTION_DATA_CHANNEL, DeliveryMethod.ReliableOrdered);
                return;
            }
            if (IsCastingSkillCanBeInterrupted && !IsCastingSkillInterrupted)
            {
                IsCastingSkillInterrupted = true;
                ProceedInterruptCastingSkill();
                RPC(RpcInterruptCastingSkill, BaseGameEntity.ACTION_DATA_CHANNEL, DeliveryMethod.ReliableOrdered);
            }
        }

        [ServerRpc]
        protected void CmdInterruptCastingSkill()
        {
            InterruptCastingSkill();
        }

        [AllRpc]
        protected void RpcInterruptCastingSkill()
        {
            if (IsServer)
            {
                // Don't interrupt using skill again
                return;
            }
            ProceedInterruptCastingSkill();
        }

        protected virtual void ProceedInterruptCastingSkill()
        {
            IsCastingSkillInterrupted = true;
            CastingSkillDuration = CastingSkillCountDown = 0;
            CancelSkill();

            BaseCharacterModel tpsModel = Entity.CharacterModel;
            bool tpsModelAvailable = tpsModel != null && tpsModel.gameObject.activeSelf;
            BaseCharacterModel vehicleModel = Entity.PassengingVehicleModel as BaseCharacterModel;
            bool vehicleModelAvailable = vehicleModel != null;
            BaseCharacterModel fpsModel = Entity.FpsModel;
            bool fpsModelAvailable = IsClient && fpsModel != null && fpsModel.gameObject.activeSelf;

            if (tpsModelAvailable)
            {
                // TPS model
                tpsModel.StopActionAnimation();
                tpsModel.StopSkillCastAnimation();
                tpsModel.StopWeaponChargeAnimation();
            }
            if (vehicleModelAvailable)
            {
                // Vehicle model
                vehicleModel.StopActionAnimation();
                vehicleModel.StopSkillCastAnimation();
                vehicleModel.StopWeaponChargeAnimation();
            }
            if (fpsModelAvailable)
            {
                // FPS model
                fpsModel.StopActionAnimation();
                fpsModel.StopSkillCastAnimation();
                fpsModel.StopWeaponChargeAnimation();
            }
        }

        public virtual void CancelSkill()
        {
            for (int i = _skillCancellationTokenSources.Count - 1; i >= 0; --i)
            {
                if (!_skillCancellationTokenSources[i].IsCancellationRequested)
                    _skillCancellationTokenSources[i].Cancel();
                _skillCancellationTokenSources.RemoveAt(i);
            }
        }

        private int GetSimulateSeed(long timestamp)
        {
            return (int)(timestamp % 16384);
        }
    }
}
