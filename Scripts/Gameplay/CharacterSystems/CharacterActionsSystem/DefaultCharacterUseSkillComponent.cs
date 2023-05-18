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
            public bool IsInterrupted;
            public int SimulateSeed;
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
        public BaseSkill UsingSkill { get; protected set; }
        public int UsingSkillLevel { get; protected set; }
        public bool IsUsingSkill { get; protected set; }
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

        protected virtual void SetUseSkillActionStates(AnimActionType animActionType, int animActionDataId, BaseSkill usingSkill, int usingSkillLevel)
        {
            ClearUseSkillStates();
            AnimActionType = animActionType;
            AnimActionDataId = animActionDataId;
            UsingSkill = usingSkill;
            UsingSkillLevel = usingSkillLevel;
            IsUsingSkill = true;
        }

        public virtual void ClearUseSkillStates()
        {
            UsingSkill = null;
            UsingSkillLevel = 0;
            IsUsingSkill = false;
        }

        public void InterruptCastingSkill()
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

        protected void AddOrUpdateSkillUsage(SkillUsageType type, int dataId, int skillLevel)
        {
            int index = Entity.IndexOfSkillUsage(dataId, type);
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

        protected async UniTaskVoid UseSkillRoutine(int simulateSeed, bool isLeftHand, BaseSkill skill, int skillLevel, uint targetObjectId, AimPosition skillAimPosition, int? itemDataId)
        {
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
            SetUseSkillActionStates(animActionType, animActionDataId, skill, skillLevel);

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
                        Entity.CharacterModel.InstantiateEffect(skill.SkillCastEffect);
                    if (fpsModelAvailable)
                        Entity.FpsModel.InstantiateEffect(skill.SkillCastEffect);
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
                        SimulateActionTriggerData simulateData = new SimulateActionTriggerData();
                        if (isLeftHand)
                            simulateData.state |= SimulateActionTriggerState.IsLeftHand;
                        simulateData.state |= SimulateActionTriggerState.IsSkill;
                        simulateData.simulateSeed = simulateSeed;
                        simulateData.triggerIndex = triggerIndex;
                        simulateData.targetObjectId = targetObjectId;
                        simulateData.skillDataId = skill.DataId;
                        simulateData.skillLevel = skillLevel;
                        simulateData.aimPosition = aimPosition;
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

        public void CancelSkill()
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
            bool isLeftHand = data.state.HasFlag(SimulateActionTriggerState.IsLeftHand);
            BaseSkill skill = data.GetSkill();
            if (skill == null)
                return;
            CharacterItem weapon = Entity.GetAvailableWeapon(ref isLeftHand);
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = skill.GetAttackDamages(Entity, data.skillLevel, isLeftHand);
            ApplySkillUsing(skill, data.skillLevel, isLeftHand, weapon, data.simulateSeed, data.triggerIndex, damageAmounts, data.targetObjectId, data.aimPosition);
        }

        public void UseSkill(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (!IsServer && IsOwnerClient)
            {
                // Validate skill
                if (!Entity.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out BaseSkill skill, out int skillLevel, out _))
                    return;
                // Get simulate seed for simulation validating
                int simulateSeed = Random.Range(int.MinValue, int.MaxValue);
                // Prepare state data which will be sent to server
                _clientState = new UseSkillState()
                {
                    SimulateSeed = simulateSeed,
                    Skill = skill,
                    SkillLevel = skillLevel,
                    IsLeftHand = isLeftHand,
                    TargetObjectId = targetObjectId,
                    AimPosition = aimPosition,
                };
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Get simulate seed for simulation validating
                int simulateSeed = Random.Range(int.MinValue, int.MaxValue);
                // Use skill immediately at server
                ProceedUseSkillStateAtServer(simulateSeed, dataId, isLeftHand, targetObjectId, aimPosition);
            }
        }

        protected void ProceedUseSkillStateAtServer(int simulateSeed, int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!_manager.IsAcceptNewAction())
                return;
            // Speed hack avoidance
            if (Time.unscaledTime - LastUseSkillEndTime < -0.05f)
                return;
            // Validate skill
            if (!Entity.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out BaseSkill skill, out int skillLevel, out _))
                return;
            _manager.ActionAccepted();
            // Prepare state data which will be sent to clients
            _serverState = new UseSkillState()
            {
                SimulateSeed = simulateSeed,
                Skill = skill,
                SkillLevel = skillLevel,
                IsLeftHand = isLeftHand,
                TargetObjectId = targetObjectId,
                AimPosition = aimPosition,
            };
#endif
        }

        public void UseSkillItem(int itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
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
                // Get simulate seed for simulation validating
                int simulateSeed = Random.Range(int.MinValue, int.MaxValue);
                // Prepare state data which will be sent to server
                _clientState = new UseSkillState()
                {
                    SimulateSeed = simulateSeed,
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
                // Get simulate seed for simulation validating
                int simulateSeed = Random.Range(int.MinValue, int.MaxValue);
                // Use skill immediately at server
                ProceedUseSkillItemStateAtServer(simulateSeed, itemIndex, isLeftHand, targetObjectId, aimPosition);
            }
        }

        protected void ProceedUseSkillItemStateAtServer(int simulateSeed, int itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!_manager.IsAcceptNewAction())
                return;
            // Speed hack avoidance
            if (Time.unscaledTime - LastUseSkillEndTime < -0.05f)
                return;
            // Validate skill item
            if (!Entity.ValidateSkillItemToUse(itemIndex, isLeftHand, targetObjectId, out ISkillItem skillItem, out BaseSkill skill, out int skillLevel, out _))
                return;
            _manager.ActionAccepted();
            // Prepare state data which will be sent to clients
            _serverState = new UseSkillState()
            {
                SimulateSeed = simulateSeed,
                ItemDataId = skillItem.DataId,
                Skill = skill,
                SkillLevel = skillLevel,
                IsLeftHand = isLeftHand,
                TargetObjectId = targetObjectId,
                AimPosition = aimPosition,
            };
#endif
        }

        public bool WriteClientUseSkillState(NetDataWriter writer)
        {
            if (_clientState.HasValue && !_clientState.Value.IsInterrupted && !_clientState.Value.UseItem)
            {
                // Simulate skill using at client
                UseSkillRoutine(_clientState.Value.SimulateSeed, _clientState.Value.IsLeftHand, _clientState.Value.Skill, _clientState.Value.SkillLevel, _clientState.Value.TargetObjectId, _clientState.Value.AimPosition, _clientState.Value.ItemDataId).Forget();
                // Send input to server
                writer.PutPackedInt(_clientState.Value.SimulateSeed);
                writer.PutPackedInt(_clientState.Value.Skill.DataId);
                writer.Put(_clientState.Value.IsLeftHand);
                writer.PutPackedUInt(_clientState.Value.TargetObjectId);
                writer.Put(_clientState.Value.AimPosition);
                // Clear Input
                _clientState = null;
                return true;
            }
            return false;
        }

        public bool WriteServerUseSkillState(NetDataWriter writer)
        {
            if (_serverState.HasValue && !_serverState.Value.IsInterrupted && !_serverState.Value.UseItem)
            {
                // Simulate skill using at server
                UseSkillRoutine(_serverState.Value.SimulateSeed, _serverState.Value.IsLeftHand, _serverState.Value.Skill, _serverState.Value.SkillLevel, _serverState.Value.TargetObjectId, _serverState.Value.AimPosition, _serverState.Value.ItemDataId).Forget();
                // Send input to client
                writer.PutPackedInt(_serverState.Value.SimulateSeed);
                writer.PutPackedInt(_serverState.Value.Skill.DataId);
                writer.PutPackedInt(_serverState.Value.SkillLevel);
                writer.Put(_serverState.Value.IsLeftHand);
                writer.PutPackedUInt(_serverState.Value.TargetObjectId);
                writer.Put(_serverState.Value.AimPosition);
                // Clear Input
                _serverState = null;
                return true;
            }
            return false;
        }

        public bool WriteClientUseSkillItemState(NetDataWriter writer)
        {
            if (_clientState.HasValue && !_clientState.Value.IsInterrupted && _clientState.Value.UseItem)
            {
                // Simulate skill using at client
                UseSkillRoutine(_clientState.Value.SimulateSeed, _clientState.Value.IsLeftHand, _clientState.Value.Skill, _clientState.Value.SkillLevel, _clientState.Value.TargetObjectId, _clientState.Value.AimPosition, _clientState.Value.ItemDataId).Forget();
                // Send input to server
                writer.PutPackedInt(_clientState.Value.SimulateSeed);
                writer.PutPackedInt(_clientState.Value.ItemIndex);
                writer.Put(_clientState.Value.IsLeftHand);
                writer.PutPackedUInt(_clientState.Value.TargetObjectId);
                writer.Put(_clientState.Value.AimPosition);
                // Clear Input
                _clientState = null;
                return true;
            }
            return false;
        }

        public bool WriteServerUseSkillItemState(NetDataWriter writer)
        {
            // It's the same behaviour with `use skill` (just play animation at clients)
            // So just send `use skill` state (see `ReadClientUseSkillItemStateAtServer` function)
            return false;
        }

        public bool WriteClientUseSkillInterruptedState(NetDataWriter writer)
        {
            if (_clientState.HasValue && _clientState.Value.IsInterrupted)
            {
                _clientState = null;
                return true;
            }
            return false;
        }

        public bool WriteServerUseSkillInterruptedState(NetDataWriter writer)
        {
            if (_serverState.HasValue && _serverState.Value.IsInterrupted)
            {
                _serverState = null;
                return true;
            }
            return false;
        }

        public void ReadClientUseSkillStateAtServer(NetDataReader reader)
        {
            int simulateSeed = reader.GetPackedInt();
            int dataId = reader.GetPackedInt();
            bool isLeftHand = reader.GetBool();
            uint targetObjectId = reader.GetPackedUInt();
            AimPosition aimPosition = reader.Get<AimPosition>();
            ProceedUseSkillStateAtServer(simulateSeed, dataId, isLeftHand, targetObjectId, aimPosition);
        }

        public void ReadServerUseSkillStateAtClient(NetDataReader reader)
        {
            int simulateSeed = reader.GetPackedInt();
            int skillDataId = reader.GetPackedInt();
            int skillLevel = reader.GetPackedInt();
            bool isLeftHand = reader.GetBool();
            uint targetObjectId = reader.GetPackedUInt();
            AimPosition aimPosition = reader.Get<AimPosition>();
            if (IsOwnerClientOrOwnedByServer)
            {
                // Don't play use skill animation again (it already played in `UseSkill` and `UseSkillItem` function)
                return;
            }
            if (GameInstance.Skills.TryGetValue(skillDataId, out BaseSkill skill) && skillLevel > 0)
                ClearUseSkillStates();
            Entity.AttackComponent.CancelAttack();
            UseSkillRoutine(simulateSeed, isLeftHand, skill, skillLevel, targetObjectId, aimPosition, null).Forget();
        }

        public void ReadClientUseSkillItemStateAtServer(NetDataReader reader)
        {
            int simulateSeed = reader.GetPackedInt();
            int itemIndex = reader.GetPackedInt();
            bool isLeftHand = reader.GetBool();
            uint targetObjectId = reader.GetPackedUInt();
            AimPosition aimPosition = reader.Get<AimPosition>();
            ProceedUseSkillItemStateAtServer(simulateSeed, itemIndex, isLeftHand, targetObjectId, aimPosition);
        }

        public void ReadServerUseSkillItemStateAtClient(NetDataReader reader)
        {
            // See `ReadServerUseSkillStateAtClient`
        }

        public void ReadClientUseSkillInterruptedStateAtServer(NetDataReader reader)
        {
            ProceedUseSkillInterruptedState();
        }

        public void ReadServerUseSkillInterruptedStateAtClient(NetDataReader reader)
        {
            ProceedUseSkillInterruptedState();
        }

        protected void ProceedUseSkillInterruptedState()
        {
            IsCastingSkillInterrupted = true;
            IsUsingSkill = false;
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
