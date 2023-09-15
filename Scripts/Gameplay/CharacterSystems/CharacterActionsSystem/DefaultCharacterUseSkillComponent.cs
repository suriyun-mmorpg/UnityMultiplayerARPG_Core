using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterActionComponentManager))]
    public class DefaultCharacterUseSkillComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterUseSkillComponent
    {
        public const float DEFAULT_TOTAL_DURATION = 2f;
        public const float DEFAULT_TRIGGER_DURATION = 1f;
        public const float DEFAULT_STATE_SETUP_DELAY = 1f;

        protected struct UseSkillState
        {
            public long SimulateSeed;
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
        public bool IsCastingSkillCanBeInterrupted { get; protected set; }
        public bool IsCastingSkillInterrupted { get; protected set; }
        public float CastingSkillDuration { get; protected set; }
        public float CastingSkillCountDown { get; protected set; }
        public float MoveSpeedRateWhileUsingSkill { get; protected set; }
        public MovementRestriction MovementRestrictionWhileUsingSkill { get; protected set; }
        protected float totalDuration;
        public float UseSkillTotalDuration { get { return totalDuration; } set { totalDuration = value; } }
        protected float[] _triggerDurations;
        public float[] UseSkillTriggerDurations { get { return _triggerDurations; } set { _triggerDurations = value; } }
        public AnimActionType AnimActionType { get; protected set; }
        public int AnimActionDataId { get; protected set; }
        public IHitRegistrationManager HitRegistrationManager { get { return BaseGameNetworkManager.Singleton.HitRegistrationManager; } }

        protected CharacterActionComponentManager _manager;
        protected float _lastAcceptedTime;
        // Network data sending
        protected UseSkillState? _clientState;
        protected UseSkillState? _serverState;
        protected UseSkillState? _simulateState;

        public override void EntityStart()
        {
            _manager = GetComponent<CharacterActionComponentManager>();
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

        public virtual void InterruptCastingSkill()
        {
            if (!IsServer)
            {
                _clientState = new UseSkillState()
                {
                    IsInterrupted = true,
                };
                return;
            }
            if (IsCastingSkillCanBeInterrupted && !IsCastingSkillInterrupted)
            {
                IsCastingSkillInterrupted = true;
                _serverState = new UseSkillState()
                {
                    IsInterrupted = true,
                };
            }
        }

        protected virtual void AddOrUpdateSkillUsage(SkillUsageType type, int dataId, int skillLevel)
        {
            int index = Entity.IndexOfSkillUsage(type, dataId);
            if (index >= 0)
            {
                CharacterSkillUsage newSkillUsage = Entity.SkillUsages[index];
                newSkillUsage.Use(Entity, skillLevel);
                Entity.SkillUsages[index] = newSkillUsage;
            }
            else
            {
                CharacterSkillUsage newSkillUsage = CharacterSkillUsage.Create(type, dataId);
                newSkillUsage.Use(Entity, skillLevel);
                Entity.SkillUsages.Add(newSkillUsage);
            }
        }

        protected virtual async UniTaskVoid UseSkillRoutine(long peerTimestamp, UseSkillState simulateState)
        {
            int simulateSeed = (int)peerTimestamp;
            bool isLeftHand = simulateState.IsLeftHand;
            BaseSkill skill = simulateState.Skill;
            int skillLevel = simulateState.SkillLevel;
            uint targetObjectId = simulateState.TargetObjectId;
            AimPosition skillAimPosition = simulateState.AimPosition;
            int? itemDataId = simulateState.ItemDataId;
            simulateState.SimulateSeed = simulateSeed;

            // Prepare required data and get skill data
            Entity.GetUsingSkillData(
                skill,
                ref isLeftHand,
                out AnimActionType animActionType,
                out int animActionDataId,
                out CharacterItem weapon);

            // Prepare required data and get animation data
            Entity.GetRandomAnimationData(
                animActionType,
                animActionDataId,
                simulateSeed,
                out int animationIndex,
                out float animSpeedRate,
                out _triggerDurations,
                out totalDuration);

            // Set doing action state at clients and server
            SetUseSkillActionStates(animActionType, animActionDataId, simulateState);

            if (IsServer)
            {
                // Update skill usage states at server only
                if (itemDataId.HasValue)
                {
                    AddOrUpdateSkillUsage(SkillUsageType.UsableItem, itemDataId.Value, skillLevel);
                }
                else
                {
                    AddOrUpdateSkillUsage(SkillUsageType.Skill, skill.DataId, skillLevel);
                }
                // Do something with buffs when use skill
                Entity.SkillAndBuffComponent.OnUseSkill();
            }

            // Prepare required data and get damages data
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = skill.GetAttackDamages(Entity, skillLevel, isLeftHand);

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

            // Last use skill end time
            float remainsDuration = DEFAULT_TOTAL_DURATION;
            LastUseSkillEndTime = Time.unscaledTime + CastingSkillDuration + DEFAULT_TOTAL_DURATION;
            if (totalDuration >= 0f)
            {
                remainsDuration = totalDuration;
                LastUseSkillEndTime = Time.unscaledTime + CastingSkillDuration + (totalDuration / animSpeedRate);
            }

            // Prepare cancellation
            CancellationTokenSource skillCancellationTokenSource = new CancellationTokenSource();
            _skillCancellationTokenSources.Add(skillCancellationTokenSource);

            try
            {
                bool tpsModelAvailable = Entity.CharacterModel != null && Entity.CharacterModel.gameObject.activeSelf;
                BaseCharacterModel vehicleModel = Entity.PassengingVehicleModel as BaseCharacterModel;
                bool vehicleModelAvailable = vehicleModel != null;
                bool fpsModelAvailable = IsClient && Entity.FpsModel != null && Entity.FpsModel.gameObject.activeSelf;

                // Play special effect
                if (IsClient)
                {
                    if (tpsModelAvailable)
                        Entity.CharacterModel.InstantiateEffect(skill.SkillCastEffects);
                    if (fpsModelAvailable)
                        Entity.FpsModel.InstantiateEffect(skill.SkillCastEffects);
                }

                if (CastingSkillDuration > 0f)
                {
                    // Play cast animation
                    if (tpsModelAvailable)
                        Entity.CharacterModel.PlaySkillCastClip(skill.DataId, CastingSkillDuration);
                    if (vehicleModelAvailable)
                        vehicleModel.PlaySkillCastClip(skill.DataId, CastingSkillDuration);
                    if (fpsModelAvailable)
                        Entity.FpsModel.PlaySkillCastClip(skill.DataId, CastingSkillDuration);
                    // Wait until end of cast duration
                    await UniTask.Delay((int)(CastingSkillDuration * 1000f), true, PlayerLoopTiming.Update, skillCancellationTokenSource.Token);
                }

                // Play special effect
                if (IsClient)
                {
                    if (tpsModelAvailable)
                        Entity.CharacterModel.InstantiateEffect(skill.SkillActivateEffects);
                    if (fpsModelAvailable)
                        Entity.FpsModel.InstantiateEffect(skill.SkillActivateEffects);
                }

                // Play action animation
                if (tpsModelAvailable)
                    Entity.CharacterModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                if (vehicleModelAvailable)
                    vehicleModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                if (fpsModelAvailable)
                    Entity.FpsModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);

                // Try setup state data (maybe by animation clip events or state machine behaviours), if it was not set up
                if (_triggerDurations == null || _triggerDurations.Length == 0 || totalDuration < 0f)
                {
                    // Wait some components to setup proper `useSkillTriggerDurations` and `useSkillTotalDuration` within `DEFAULT_STATE_SETUP_DELAY`
                    float setupDelayCountDown = DEFAULT_STATE_SETUP_DELAY;
                    do
                    {
                        await UniTask.Yield();
                        setupDelayCountDown -= Time.unscaledDeltaTime;
                    } while (setupDelayCountDown > 0 && (_triggerDurations == null || _triggerDurations.Length == 0 || totalDuration < 0f));
                    if (setupDelayCountDown <= 0f)
                    {
                        // Can't setup properly, so try to setup manually to make it still workable
                        remainsDuration = DEFAULT_TOTAL_DURATION - DEFAULT_STATE_SETUP_DELAY;
                        _triggerDurations = new float[1]
                        {
                            DEFAULT_TRIGGER_DURATION,
                        };
                    }
                    else
                    {
                        // Can setup, so set proper `remainsDuration` and `LastUseSkillEndTime` value
                        remainsDuration = totalDuration;
                        LastUseSkillEndTime = Time.unscaledTime + (totalDuration / animSpeedRate);
                    }
                }


                // Prepare hit register validation, it will be used later when receive attack start/end events from clients
                if (IsServer && !IsOwnerClientOrOwnedByServer && skill.IsAttack && skill.TryGetDamageInfo(Entity, isLeftHand, out DamageInfo damageInfo))
                    HitRegistrationManager.PrepareHitRegValidatation(Entity, simulateSeed, _triggerDurations, 0, damageInfo, damageAmounts, weapon, skill, skillLevel);

                float tempTriggerDuration;
                for (byte triggerIndex = 0; triggerIndex < _triggerDurations.Length; ++triggerIndex)
                {
                    // Play special effects after trigger duration
                    tempTriggerDuration = _triggerDurations[triggerIndex];
                    remainsDuration -= tempTriggerDuration;
                    await UniTask.Delay((int)(tempTriggerDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, skillCancellationTokenSource.Token);

                    // Special effects will plays on clients only
                    if (IsClient && (AnimActionType == AnimActionType.AttackRightHand || AnimActionType == AnimActionType.AttackLeftHand))
                    {
                        // Play weapon launch special effects
                        if (tpsModelAvailable)
                            Entity.CharacterModel.PlayEquippedWeaponLaunch(isLeftHand);
                        if (fpsModelAvailable)
                            Entity.FpsModel.PlayEquippedWeaponLaunch(isLeftHand);
                        // Play launch sfx
                        AudioClipWithVolumeSettings audioClip = weaponItem.LaunchClip;
                        if (audioClip != null)
                            AudioManager.PlaySfxClipAtAudioSource(audioClip.audioClip, Entity.CharacterModel.GenericAudioSource, audioClip.GetRandomedVolume());
                    }

                    // Get aim position by character's forward
                    AimPosition aimPosition;
                    if (skill.HasCustomAimControls() && skillAimPosition.type == AimPositionType.Position)
                        aimPosition = skillAimPosition;
                    else
                        aimPosition = Entity.AimPosition;

                    // Trigger skill event
                    Entity.OnUseSkillRoutine(skill, skillLevel, isLeftHand, weapon, simulateSeed, triggerIndex, damageAmounts, targetObjectId, aimPosition);

                    // Apply skill buffs, summons and attack damages
                    if (IsOwnerClientOrOwnedByServer)
                    {
                        // Simulate action at non-owner clients
                        SimulateActionTriggerData simulateData = new SimulateActionTriggerData()
                        {
                            simulateSeed = simulateSeed,
                            triggerIndex = triggerIndex,
                            aimPosition = aimPosition,
                        };
                        RPC(AllSimulateActionTrigger, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, simulateData);
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
                    await UniTask.Delay((int)(remainsDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, skillCancellationTokenSource.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Catch the cancellation
                LastUseSkillEndTime = Time.unscaledTime;
            }
            catch (System.Exception ex)
            {
                // Other errors
                Logging.LogException(LogTag, ex);
            }
            finally
            {
                skillCancellationTokenSource.Dispose();
                _skillCancellationTokenSources.Remove(skillCancellationTokenSource);
            }
            // Clear action states at clients and server
            ClearUseSkillStates();
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

        protected virtual void ApplySkillUsing(BaseSkill skill, int skillLevel, bool isLeftHand, CharacterItem weapon, int simulateSeed, byte triggerIndex, Dictionary<DamageElement, MinMaxFloat> damageAmounts, uint targetObjectId, AimPosition aimPosition)
        {
            skill.ApplySkill(
                Entity,
                skillLevel,
                isLeftHand,
                weapon,
                simulateSeed,
                triggerIndex,
                damageAmounts,
                targetObjectId,
                aimPosition,
                OnAttackOriginPrepared,
                OnAttackHit);
        }

        protected virtual void OnAttackOriginPrepared(int simulateSeed, byte triggerIndex, byte spreadIndex, Vector3 position, Vector3 direction, Quaternion rotation)
        {
            if (!IsServer || IsOwnerClientOrOwnedByServer)
                return;
            HitRegistrationManager.PrepareHitRegOrigin(Entity, simulateSeed, triggerIndex, spreadIndex, position, direction);
        }

        protected virtual void OnAttackHit(int simulateSeed, byte triggerIndex, byte spreadIndex, uint objectId, byte hitboxIndex, Vector3 hitPoint)
        {
            if (IsServer || !IsOwnerClient)
                return;
            HitRegistrationManager.PrepareToRegister(simulateSeed, triggerIndex, spreadIndex, objectId, hitboxIndex, hitPoint);
        }

        [AllRpc]
        protected void AllSimulateActionTrigger(SimulateActionTriggerData data)
        {
            if (IsOwnerClientOrOwnedByServer)
                return;
            if (!_simulateState.HasValue)
                return;
            if (data.simulateSeed != _simulateState.Value.SimulateSeed)
                return;
            bool isLeftHand = _simulateState.Value.IsLeftHand;
            BaseSkill skill = _simulateState.Value.Skill;
            int skillLevel = _simulateState.Value.SkillLevel;
            uint targetObjectId = _simulateState.Value.TargetObjectId;
            CharacterItem weapon = Entity.GetAvailableWeapon(ref isLeftHand);
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = skill.GetAttackDamages(Entity, skillLevel, isLeftHand);
            ApplySkillUsing(skill, skillLevel, isLeftHand, weapon, data.simulateSeed, data.triggerIndex, damageAmounts, targetObjectId, data.aimPosition);
        }

        public virtual void UseSkill(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (!IsServer && IsOwnerClient)
            {
                // Validate skill
                if (!Entity.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out BaseSkill skill, out int skillLevel, out _))
                    return;
                // Prepare state data which will be sent to server
                _clientState = new UseSkillState()
                {
                    Skill = skill,
                    SkillLevel = skillLevel,
                    IsLeftHand = isLeftHand,
                    TargetObjectId = targetObjectId,
                    AimPosition = aimPosition,
                };
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Use skill immediately at server
                ProceedUseSkillStateAtServer(dataId, isLeftHand, targetObjectId, aimPosition);
            }
        }

        protected virtual void ProceedUseSkillStateAtServer(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!_manager.IsAcceptNewAction())
                return;
            // Speed hack avoidance
            if (Time.unscaledTime - LastUseSkillEndTime < -0.2f)
                return;
            // Validate skill
            if (!Entity.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out BaseSkill skill, out int skillLevel, out _))
                return;
            _manager.ActionAccepted();
            // Prepare state data which will be sent to clients
            _serverState = new UseSkillState()
            {
                Skill = skill,
                SkillLevel = skillLevel,
                IsLeftHand = isLeftHand,
                TargetObjectId = targetObjectId,
                AimPosition = aimPosition,
            };
#endif
        }

        public virtual void UseSkillItem(int itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (!IsServer && IsOwnerClient)
            {
                // Validate skill
                if (!Entity.ValidateSkillItemToUse(itemIndex, isLeftHand, targetObjectId, out ISkillItem skillItem, out BaseSkill skill, out int skillLevel, out UITextKeys gameMessage))
                {
                    ClientGenericActions.ClientReceiveGameMessage(gameMessage);
                    return;
                }
                // Update using time
                Entity.LastUseItemTime = Time.unscaledTime;
                // Prepare state data which will be sent to server
                _clientState = new UseSkillState()
                {
                    UseItem = true,
                    ItemIndex = itemIndex,
                    ItemDataId = skillItem.DataId,
                    Skill = skill,
                    SkillLevel = skillLevel,
                    IsLeftHand = isLeftHand,
                    TargetObjectId = targetObjectId,
                    AimPosition = aimPosition,
                };
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Use skill immediately at server
                ProceedUseSkillItemStateAtServer(itemIndex, isLeftHand, targetObjectId, aimPosition);
            }
        }

        protected virtual void ProceedUseSkillItemStateAtServer(int itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!_manager.IsAcceptNewAction())
                return;
            // Speed hack avoidance
            if (Time.unscaledTime - LastUseSkillEndTime < -0.2f)
                return;
            // Validate skill item
            if (!Entity.ValidateSkillItemToUse(itemIndex, isLeftHand, targetObjectId, out ISkillItem skillItem, out BaseSkill skill, out int skillLevel, out _))
                return;
            _manager.ActionAccepted();
            // Prepare state data which will be sent to clients
            _serverState = new UseSkillState()
            {
                ItemDataId = skillItem.DataId,
                Skill = skill,
                SkillLevel = skillLevel,
                IsLeftHand = isLeftHand,
                TargetObjectId = targetObjectId,
                AimPosition = aimPosition,
            };
#endif
        }

        public virtual bool WriteClientUseSkillState(long writeTimestamp, NetDataWriter writer)
        {
            if (_clientState.HasValue && !_clientState.Value.IsInterrupted && !_clientState.Value.UseItem)
            {
                // Simulate skill using at client
                UseSkillRoutine(writeTimestamp, _clientState.Value).Forget();
                // Send input to server
                writer.Put(_clientState.Value.IsLeftHand);
                writer.PutPackedInt(_clientState.Value.Skill.DataId);
                writer.PutPackedUInt(_clientState.Value.TargetObjectId);
                writer.Put(_clientState.Value.AimPosition);
                // Clear Input
                _clientState = null;
                return true;
            }
            return false;
        }

        public virtual bool WriteServerUseSkillState(long writeTimestamp, NetDataWriter writer)
        {
            if (_serverState.HasValue && !_serverState.Value.IsInterrupted && !_serverState.Value.UseItem)
            {
                // Simulate skill using at server
                UseSkillRoutine(writeTimestamp, _serverState.Value).Forget();
                // Send input to client
                writer.Put(_serverState.Value.IsLeftHand);
                writer.PutPackedInt(_serverState.Value.Skill.DataId);
                writer.PutPackedInt(_serverState.Value.SkillLevel);
                writer.PutPackedUInt(_serverState.Value.TargetObjectId);
                writer.Put(_serverState.Value.AimPosition);
                // Clear Input
                _serverState = null;
                return true;
            }
            return false;
        }

        public virtual bool WriteClientUseSkillItemState(long writeTimestamp, NetDataWriter writer)
        {
            if (_clientState.HasValue && !_clientState.Value.IsInterrupted && _clientState.Value.UseItem)
            {
                // Simulate skill using at client
                UseSkillRoutine(writeTimestamp, _clientState.Value).Forget();
                // Send input to server
                writer.Put(_clientState.Value.IsLeftHand);
                writer.PutPackedInt(_clientState.Value.ItemIndex);
                writer.PutPackedUInt(_clientState.Value.TargetObjectId);
                writer.Put(_clientState.Value.AimPosition);
                // Clear Input
                _clientState = null;
                return true;
            }
            return false;
        }

        public virtual bool WriteServerUseSkillItemState(long writeTimestamp, NetDataWriter writer)
        {
            // It's the same behaviour with `use skill` (just play animation at clients)
            // So just send `use skill` state (see `ReadClientUseSkillItemStateAtServer` function)
            return false;
        }

        public virtual bool WriteClientUseSkillInterruptedState(long writeTimestamp, NetDataWriter writer)
        {
            if (_clientState.HasValue && _clientState.Value.IsInterrupted)
            {
                // Simulate skill interrupting at client
                _clientState = null;
                return true;
            }
            return false;
        }

        public virtual bool WriteServerUseSkillInterruptedState(long writeTimestamp, NetDataWriter writer)
        {
            if (_serverState.HasValue && _serverState.Value.IsInterrupted)
            {
                // Simulate skill interrupting at server
                ProceedUseSkillInterruptedState();
                _serverState = null;
                return true;
            }
            return false;
        }

        public virtual void ReadClientUseSkillStateAtServer(long peerTimestamp, NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            int dataId = reader.GetPackedInt();
            uint targetObjectId = reader.GetPackedUInt();
            AimPosition aimPosition = reader.Get<AimPosition>();
            ProceedUseSkillStateAtServer(dataId, isLeftHand, targetObjectId, aimPosition);
        }

        public virtual void ReadServerUseSkillStateAtClient(long peerTimestamp, NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            int skillDataId = reader.GetPackedInt();
            int skillLevel = reader.GetPackedInt();
            uint targetObjectId = reader.GetPackedUInt();
            AimPosition aimPosition = reader.Get<AimPosition>();
            if (IsServer || IsOwnerClient)
            {
                // Don't play skill using animation again (it already done in `WriteClientUseSkillItemState` and `WriteServerUseSkillState` function)
                return;
            }
            if (!GameInstance.Skills.TryGetValue(skillDataId, out BaseSkill skill))
            {
                // No skill existed, so it can't simulate
                return;
            }
            ClearUseSkillStates();
            Entity.AttackComponent.CancelAttack();
            UseSkillState simulateState = new UseSkillState()
            {
                IsLeftHand = isLeftHand,
                Skill = skill,
                SkillLevel = skillLevel,
                TargetObjectId = targetObjectId,
                AimPosition = aimPosition,
            };
            UseSkillRoutine(peerTimestamp, simulateState).Forget();
        }

        public virtual void ReadClientUseSkillItemStateAtServer(long peerTimestamp, NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            int itemIndex = reader.GetPackedInt();
            uint targetObjectId = reader.GetPackedUInt();
            AimPosition aimPosition = reader.Get<AimPosition>();
            ProceedUseSkillItemStateAtServer(itemIndex, isLeftHand, targetObjectId, aimPosition);
        }

        public virtual void ReadServerUseSkillItemStateAtClient(long peerTimestamp, NetDataReader reader)
        {
            // See `ReadServerUseSkillStateAtClient`
        }

        public void ReadClientUseSkillInterruptedStateAtServer(long peerTimestamp, NetDataReader reader)
        {
            // TODO: Verify interrupt request
            // Broadcast interrupting to all clients
            InterruptCastingSkill();
        }

        public void ReadServerUseSkillInterruptedStateAtClient(long peerTimestamp, NetDataReader reader)
        {
            if (IsServer)
            {
                // Don't interrupt using skill again (it already done in `WriteServerUseSkillInterruptedState` function)
                return;
            }
            ProceedUseSkillInterruptedState();
        }

        protected virtual void ProceedUseSkillInterruptedState()
        {
            IsCastingSkillInterrupted = true;
            CastingSkillDuration = CastingSkillCountDown = 0;
            CancelSkill();
            if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
            {
                // TPS model
                Entity.CharacterModel.StopActionAnimation();
                Entity.CharacterModel.StopSkillCastAnimation();
                Entity.CharacterModel.StopWeaponChargeAnimation();
            }
            if (Entity.PassengingVehicleModel && Entity.PassengingVehicleModel is BaseCharacterModel vehicleModel)
            {
                // Vehicle model
                vehicleModel.StopActionAnimation();
                vehicleModel.StopSkillCastAnimation();
                vehicleModel.StopWeaponChargeAnimation();
            }
            if (IsClient && Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
            {
                // FPS model
                Entity.FpsModel.StopActionAnimation();
                Entity.FpsModel.StopSkillCastAnimation();
                Entity.FpsModel.StopWeaponChargeAnimation();
            }
        }
    }
}
