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
        public const float DEFAULT_TOTAL_DURATION = 2f;
        public const float DEFAULT_TRIGGER_DURATION = 1f;
        public const float DEFAULT_STATE_SETUP_DELAY = 1f;

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
        public bool LastUseSkillSkipMovementValidation { get { return _skipMovementValidation; } set { _skipMovementValidation = value; } }
        public bool IsCastingSkillCanBeInterrupted { get; protected set; }
        public bool IsCastingSkillInterrupted { get; protected set; }
        public float CastingSkillDuration { get; protected set; }
        public float CastingSkillCountDown { get; protected set; }
        public float MoveSpeedRateWhileUsingSkill { get; protected set; }
        public MovementRestriction MovementRestrictionWhileUsingSkill { get; protected set; }
        protected float _totalDuration;
        public float UseSkillTotalDuration { get { return _totalDuration; } set { _totalDuration = value; } }
        protected float[] _triggerDurations;
        public float[] UseSkillTriggerDurations { get { return _triggerDurations; } set { _triggerDurations = value; } }
        public AnimActionType AnimActionType { get; protected set; }
        public int AnimActionDataId { get; protected set; }
        public IHitRegistrationManager HitRegistrationManager { get { return BaseGameNetworkManager.Singleton.HitRegistrationManager; } }

        protected CharacterActionComponentManager _manager;
        protected float _lastAcceptedTime;
        // Network data sending
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
                out CharacterItem weapon);

            // Prepare required data and get animation data
            Entity.GetRandomAnimationData(
                animActionType,
                animActionDataId,
                simulateSeed,
                out int animationIndex,
                out float animSpeedRate,
                out _triggerDurations,
                out _totalDuration,
                out _skipMovementValidation);

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
            if (_totalDuration >= 0f)
            {
                remainsDuration = _totalDuration;
                LastUseSkillEndTime = Time.unscaledTime + CastingSkillDuration + (_totalDuration / animSpeedRate);
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
                if (_triggerDurations == null || _triggerDurations.Length == 0 || _totalDuration < 0f)
                {
                    // Wait some components to setup proper `useSkillTriggerDurations` and `useSkillTotalDuration` within `DEFAULT_STATE_SETUP_DELAY`
                    float setupDelayCountDown = DEFAULT_STATE_SETUP_DELAY;
                    do
                    {
                        await UniTask.Yield();
                        setupDelayCountDown -= Time.unscaledDeltaTime;
                    } while (setupDelayCountDown > 0 && (_triggerDurations == null || _triggerDurations.Length == 0 || _totalDuration < 0f));
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
                        remainsDuration = _totalDuration;
                        LastUseSkillEndTime = Time.unscaledTime + (_totalDuration / animSpeedRate);
                    }
                }

                // Prepare hit register validation, it will be used later when receive attack start/end events from clients
                HitRegistrationManager.PrepareHitRegValidation(Entity, simulateSeed, _triggerDurations, 0, skill.GetDamageInfo(Entity, isLeftHand), damageAmounts, isLeftHand, weapon, skill, skillLevel);

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

                    await UniTask.Yield();
                    // Get aim position by character's forward
                    AimPosition aimPosition;
                    if (skill.HasCustomAimControls() && skillAimPosition.type == AimPositionType.Position)
                        aimPosition = skillAimPosition;
                    else
                        aimPosition = Entity.AimPosition;

                    // Trigger skill event
                    Entity.OnUseSkillRoutine(skill, skillLevel, isLeftHand, weapon, simulateSeed, triggerIndex, damageAmounts, targetObjectId, aimPosition);

                    // Apply skill buffs, summons and attack damages
                    if ((IsServer && IsOwnerClient) || IsOwnedByServer)
                    {
                        if (!skill.DecreaseResources(Entity, weapon, isLeftHand, out Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts))
                            break;
                        RPC(RpcSimulateActionTrigger, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, new SimulateActionTriggerData()
                        {
                            simulateSeed = simulateSeed,
                            triggerIndex = triggerIndex,
                            targetObjectId = targetObjectId,
                            aimPosition = aimPosition,
                        });
                        ApplySkillUsing(skill, skillLevel, isLeftHand, weapon, simulateSeed, triggerIndex, damageAmounts, increaseDamageAmounts, targetObjectId, aimPosition);
                    }
                    else if (IsOwnerClient)
                    {
                        Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts = skill.GetIncreaseDamageByResources(Entity, weapon);
                        RPC(CmdSimulateActionTrigger, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, new SimulateActionTriggerData()
                        {
                            simulateSeed = simulateSeed,
                            triggerIndex = triggerIndex,
                            targetObjectId = targetObjectId,
                            aimPosition = aimPosition,
                        });
                        ApplySkillUsing(skill, skillLevel, isLeftHand, weapon, simulateSeed, triggerIndex, damageAmounts, increaseDamageAmounts, targetObjectId, aimPosition);
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
            await UniTask.Yield();
            // Clear action states at clients and server
            ClearUseSkillStates();
        }

        [ServerRpc]
        protected void CmdSimulateActionTrigger(SimulateActionTriggerData data)
        {
            HitValidateData validateData = HitRegistrationManager.GetHitValidateData(Entity, data.simulateSeed);
            if (validateData == null || validateData.Skill == null)
                return;
            if (!validateData.Skill.DecreaseResources(Entity, validateData.Weapon, validateData.IsLeftHand, out Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts))
                return;
            HitRegistrationManager.ConfirmHitRegValidation(Entity, data.simulateSeed, data.triggerIndex, increaseDamageAmounts);
            RPC(RpcSimulateActionTrigger, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, data);
            ApplySkillUsing(validateData.Skill, validateData.SkillLevel, validateData.IsLeftHand, validateData.Weapon, data.simulateSeed, data.triggerIndex, validateData.BaseDamageAmounts, increaseDamageAmounts, data.targetObjectId, data.aimPosition);
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
            Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts = validateData.Skill.GetIncreaseDamageByResources(Entity, validateData.Weapon);
            ApplySkillUsing(validateData.Skill, validateData.SkillLevel, validateData.IsLeftHand, validateData.Weapon, data.simulateSeed, data.triggerIndex, validateData.BaseDamageAmounts, increaseDamageAmounts, data.targetObjectId, data.aimPosition);
        }

        protected virtual void ApplySkillUsing(BaseSkill skill, int skillLevel, bool isLeftHand, CharacterItem weapon, int simulateSeed, byte triggerIndex, Dictionary<DamageElement, MinMaxFloat> damageAmounts, Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts, uint targetObjectId, AimPosition aimPosition)
        {
            // Make sure it won't increase damage to the wrong collction
            damageAmounts = damageAmounts == null ? new Dictionary<DamageElement, MinMaxFloat>() : new Dictionary<DamageElement, MinMaxFloat>(damageAmounts);
            // Increase damage amounts
            if (increaseDamageAmounts != null && increaseDamageAmounts.Count > 0)
                damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, increaseDamageAmounts);

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
            long timestamp = Manager.Timestamp;
            if (!IsServer && IsOwnerClient)
            {
                if (!Entity.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out BaseSkill skill, out int skillLevel, out UITextKeys gameMessage))
                {
                    ClientGenericActions.ClientReceiveGameMessage(gameMessage);
                    return;
                }
                ProceedUseSkill(timestamp, skill, skillLevel, isLeftHand, targetObjectId, aimPosition);
                RPC(CmdUseSkill, timestamp, dataId, isLeftHand, targetObjectId, aimPosition);
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
            if (!_manager.IsAcceptNewAction())
                return;
            // Speed hack avoidance
            if (Time.unscaledTime - LastUseSkillEndTime < -0.2f)
                return;
            if (!Entity.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out BaseSkill skill, out int skillLevel, out _))
                return;
            _manager.ActionAccepted();
            ProceedUseSkill(peerTimestamp, skill, skillLevel, isLeftHand, targetObjectId, aimPosition);
            RPC(RpcUseSkill, peerTimestamp, dataId, skillLevel, isLeftHand, targetObjectId, aimPosition);
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
            long timestamp = Manager.Timestamp;
            if (!IsServer && IsOwnerClient)
            {
                if (!Entity.ValidateSkillItemToUse(itemIndex, isLeftHand, targetObjectId, out ISkillItem skillItem, out BaseSkill skill, out int skillLevel, out UITextKeys gameMessage))
                {
                    ClientGenericActions.ClientReceiveGameMessage(gameMessage);
                    return;
                }
                Entity.LastUseItemTime = Time.unscaledTime;
                ProceedUseSkillItem(timestamp, skillItem, skill, skillLevel, isLeftHand, targetObjectId, aimPosition);
                RPC(CmdUseSkillItem, timestamp, itemIndex, isLeftHand, targetObjectId, aimPosition);
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
            if (!_manager.IsAcceptNewAction())
                return;
            // Speed hack avoidance
            if (Time.unscaledTime - LastUseSkillEndTime < -0.2f)
                return;
            // Validate skill item
            if (!Entity.ValidateSkillItemToUse(itemIndex, isLeftHand, targetObjectId, out ISkillItem skillItem, out BaseSkill skill, out int skillLevel, out _))
                return;
            _manager.ActionAccepted();
            ProceedUseSkillItem(peerTimestamp, skillItem, skill, skillLevel, isLeftHand, targetObjectId, aimPosition);
            RPC(RpcUseSkillItem, peerTimestamp, skillItem.DataId, isLeftHand, targetObjectId, aimPosition);
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
            ProceedUseSkillItem(peerTimestamp, skillItem, skillItem.UsingSkill, skillItem.UsingSkillLevel, isLeftHand, targetObjectId, aimPosition);
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
                RPC(CmdInterruptCastingSkill);
                return;
            }
            if (IsCastingSkillCanBeInterrupted && !IsCastingSkillInterrupted)
            {
                IsCastingSkillInterrupted = true;
                ProceedInterruptCastingSkill();
                RPC(RpcInterruptCastingSkill);
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
