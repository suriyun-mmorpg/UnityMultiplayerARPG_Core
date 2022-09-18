using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultCharacterUseSkillComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterUseSkillComponent
    {
        protected List<CancellationTokenSource> skillCancellationTokenSources = new List<CancellationTokenSource>();
        public BaseSkill UsingSkill { get; protected set; }
        public short UsingSkillLevel { get; protected set; }
        public bool IsUsingSkill { get; protected set; }
        public float LastUseSkillEndTime { get; protected set; }
        public bool IsCastingSkillCanBeInterrupted { get; protected set; }
        public bool IsCastingSkillInterrupted { get; protected set; }
        public float CastingSkillDuration { get; protected set; }
        public float CastingSkillCountDown { get; protected set; }
        public float MoveSpeedRateWhileUsingSkill { get; protected set; }
        public AnimActionType AnimActionType { get; protected set; }
        public int AnimActionDataId { get; protected set; }

        protected readonly Dictionary<int, SimulatingActionTriggerHistory> SimulatingActionTriggerHistories = new Dictionary<int, SimulatingActionTriggerHistory>();
        protected readonly Dictionary<int, List<SimulateActionTriggerData>> SimlatingActionTriggerDataList = new Dictionary<int, List<SimulateActionTriggerData>>();
        protected bool sendingClientUseSkill;
        protected bool sendingClientUseSkillItem;
        protected bool sendingClientUseSkillInterrupted;
        protected bool sendingServerUseSkill;
        protected bool sendingServerUseSkillInterrupted;
        protected byte sendingSeed;
        protected int sendingSkillDataId;
        protected short sendingSkillLevel;
        protected short sendingItemIndex;
        protected bool sendingIsLeftHand;
        protected uint sendingTargetObjectId;
        protected AimPosition sendingAimPosition;

        public override void EntityUpdate()
        {
            // Update casting skill count down, will show gage at clients
            if (CastingSkillCountDown > 0)
                CastingSkillCountDown -= Time.unscaledDeltaTime;
        }

        protected virtual void SetUseSkillActionStates(AnimActionType animActionType, int animActionDataId, BaseSkill usingSkill, short usingSkillLevel)
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
                sendingClientUseSkillInterrupted = true;
                return;
            }
            if (IsCastingSkillCanBeInterrupted && !IsCastingSkillInterrupted)
            {
                IsCastingSkillInterrupted = true;
                sendingServerUseSkillInterrupted = true;
            }
        }

        protected async UniTaskVoid UseSkillRoutine(byte simulateSeed, bool isLeftHand, BaseSkill skill, short skillLevel, uint targetObjectId, AimPosition skillAimPosition)
        {
            // Prepare cancellation
            CancellationTokenSource skillCancellationTokenSource = new CancellationTokenSource();
            skillCancellationTokenSources.Add(skillCancellationTokenSource);

            // Prepare required data and get skill data
            AnimActionType animActionType;
            int animActionDataId;
            CharacterItem weapon;
            Entity.GetUsingSkillData(
                skill,
                ref isLeftHand,
                out animActionType,
                out animActionDataId,
                out weapon);

            // Prepare required data and get animation data
            int animationIndex;
            float animSpeedRate;
            float[] triggerDurations;
            float totalDuration;
            Entity.GetRandomAnimationData(
                animActionType,
                animActionDataId,
                simulateSeed,
                out animationIndex,
                out animSpeedRate,
                out triggerDurations,
                out totalDuration);

            // Set doing action state at clients and server
            SetUseSkillActionStates(animActionType, animActionDataId, skill, skillLevel);

            // Update skill usage states at server only
            if (IsServer)
            {
                CharacterSkillUsage newSkillUsage;
                int skillUsageIndex = Entity.IndexOfSkillUsage(skill.DataId, SkillUsageType.Skill);
                if (skillUsageIndex >= 0)
                {
                    newSkillUsage = Entity.SkillUsages[skillUsageIndex];
                    newSkillUsage.Use(Entity, skillLevel);
                    Entity.SkillUsages[skillUsageIndex] = newSkillUsage;
                }
                else
                {
                    newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.Skill, skill.DataId);
                    newSkillUsage.Use(Entity, skillLevel);
                    Entity.SkillUsages.Add(newSkillUsage);
                }
            }

            // Prepare required data and get damages data
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = skill.GetAttackDamages(Entity, skillLevel, isLeftHand);

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileUsingSkill = skill.moveSpeedRateWhileUsingSkill;

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            animSpeedRate *= Entity.GetAnimSpeedRate(AnimActionType);

            // Set doing action data
            IsCastingSkillCanBeInterrupted = skill.canBeInterruptedWhileCasting;
            IsCastingSkillInterrupted = false;

            // Get cast duration. Then if cast duration more than 0, it will play cast skill animation.
            CastingSkillDuration = CastingSkillCountDown = skill.GetCastDuration(skillLevel);

            // Last use skill end time
            LastUseSkillEndTime = Time.unscaledTime + (totalDuration / animSpeedRate);

            try
            {
                // Play special effect
                if (IsClient)
                {
                    if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                        Entity.CharacterModel.InstantiateEffect(skill.SkillCastEffect);
                    if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                        Entity.FpsModel.InstantiateEffect(skill.SkillCastEffect);
                }

                if (CastingSkillDuration > 0f)
                {
                    // Play cast animation
                    if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                    {
                        // TPS model
                        Entity.CharacterModel.PlaySkillCastClip(skill.DataId, CastingSkillDuration);
                    }
                    if (Entity.PassengingVehicleModel && Entity.PassengingVehicleModel is BaseCharacterModel)
                    {
                        // Vehicle model
                        (Entity.PassengingVehicleModel as BaseCharacterModel).PlaySkillCastClip(skill.DataId, CastingSkillDuration);
                    }
                    if (IsClient)
                    {
                        if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                        {
                            // FPS model
                            Entity.FpsModel.PlaySkillCastClip(skill.DataId, CastingSkillDuration);
                        }
                    }
                    // Wait until end of cast duration
                    await UniTask.Delay((int)(CastingSkillDuration * 1000f), true, PlayerLoopTiming.Update, skillCancellationTokenSource.Token);
                }

                // Play action animation
                if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                {
                    // TPS model
                    Entity.CharacterModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                }
                if (Entity.PassengingVehicleModel && Entity.PassengingVehicleModel is BaseCharacterModel)
                {
                    // Vehicle model
                    (Entity.PassengingVehicleModel as BaseCharacterModel).PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                }
                if (IsClient)
                {
                    if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                    {
                        // FPS model
                        Entity.FpsModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                    }
                }

                float remainsDuration = totalDuration;
                float tempTriggerDuration;
                SimulatingActionTriggerHistories[simulateSeed] = new SimulatingActionTriggerHistory(triggerDurations.Length);
                if (SimlatingActionTriggerDataList.ContainsKey(simulateSeed))
                {
                    foreach (SimulateActionTriggerData data in SimlatingActionTriggerDataList[simulateSeed])
                    {
                        ProceedSimulateActionTrigger(data);
                    }
                }
                SimlatingActionTriggerDataList.Clear();
                for (int hitIndex = 0; hitIndex < triggerDurations.Length; ++hitIndex)
                {
                    // Play special effects after trigger duration
                    tempTriggerDuration = triggerDurations[hitIndex];
                    remainsDuration -= tempTriggerDuration;
                    await UniTask.Delay((int)(tempTriggerDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, skillCancellationTokenSource.Token);

                    // Special effects will plays on clients only
                    if (IsClient)
                    {
                        // Play weapon launch special effects
                        if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                            Entity.CharacterModel.PlayEquippedWeaponLaunch(isLeftHand);
                        if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                            Entity.FpsModel.PlayEquippedWeaponLaunch(isLeftHand);
                        // Play launch sfx
                        if (weaponItem != null &&
                            (AnimActionType == AnimActionType.AttackRightHand ||
                            AnimActionType == AnimActionType.AttackLeftHand))
                        {
                            AudioManager.PlaySfxClipAtAudioSource(weaponItem.LaunchClip, Entity.CharacterModel.GenericAudioSource);
                        }
                    }

                    // Get aim position by character's forward
                    AimPosition aimPosition;
                    if (skill.HasCustomAimControls() && skillAimPosition.type == AimPositionType.Position)
                        aimPosition = skillAimPosition;
                    else
                        aimPosition = Entity.AimPosition;

                    // Trigger skill event
                    Entity.OnUseSkillRoutine(skill, skillLevel, isLeftHand, weapon, hitIndex, damageAmounts, targetObjectId, aimPosition);

                    // Apply skill buffs, summons and attack damages
                    if (IsOwnerClientOrOwnedByServer)
                    {
                        int applySeed = unchecked(simulateSeed + (hitIndex * 16));
                        skill.ApplySkill(Entity, skillLevel, isLeftHand, weapon, hitIndex, damageAmounts, targetObjectId, aimPosition, applySeed);
                        SimulateActionTriggerData simulateData = new SimulateActionTriggerData();
                        if (isLeftHand)
                            simulateData.state |= SimulateActionTriggerState.IsLeftHand;
                        simulateData.state |= SimulateActionTriggerState.IsSkill;
                        simulateData.randomSeed = simulateSeed;
                        simulateData.targetObjectId = targetObjectId;
                        simulateData.skillDataId = skill.DataId;
                        simulateData.skillLevel = skillLevel;
                        simulateData.aimPosition = aimPosition;
                        RPC(AllSimulateActionTrigger, BaseGameEntity.SERVER_STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, simulateData);
                    }

                    if (remainsDuration <= 0f)
                    {
                        // Stop trigger animations loop
                        break;
                    }
                }

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
                skillCancellationTokenSources.Remove(skillCancellationTokenSource);
            }
            // Clear action states at clients and server
            ClearUseSkillStates();
        }

        public void CancelSkill()
        {
            for (int i = skillCancellationTokenSources.Count - 1; i >= 0; --i)
            {
                if (!skillCancellationTokenSources[i].IsCancellationRequested)
                    skillCancellationTokenSources[i].Cancel();
                skillCancellationTokenSources.RemoveAt(i);
            }
        }

        [AllRpc]
        protected void AllSimulateActionTrigger(SimulateActionTriggerData data)
        {
            if (IsOwnerClientOrOwnedByServer)
                return;
            if (!ProceedSimulateActionTrigger(data))
            {
                if (!SimlatingActionTriggerDataList.ContainsKey(data.randomSeed))
                    SimlatingActionTriggerDataList[data.randomSeed] = new List<SimulateActionTriggerData>();
                SimlatingActionTriggerDataList[data.randomSeed].Add(data);
            }
        }

        protected bool ProceedSimulateActionTrigger(SimulateActionTriggerData data)
        {
            SimulatingActionTriggerHistory simulatingHit;
            if (!SimulatingActionTriggerHistories.TryGetValue(data.randomSeed, out simulatingHit) || simulatingHit.TriggeredIndex >= simulatingHit.TriggerLength)
                return false;
            int hitIndex = SimulatingActionTriggerHistories[data.randomSeed].TriggeredIndex;
            int applySeed = unchecked(data.randomSeed + (hitIndex * 16));
            hitIndex++;
            simulatingHit.TriggeredIndex = hitIndex;
            SimulatingActionTriggerHistories[data.randomSeed] = simulatingHit;
            bool isLeftHand = data.state.HasFlag(SimulateActionTriggerState.IsLeftHand);
            if (data.state.HasFlag(SimulateActionTriggerState.IsSkill))
            {
                BaseSkill skill = data.GetSkill();
                if (skill != null)
                {
                    CharacterItem weapon = Entity.GetAvailableWeapon(ref isLeftHand);
                    Dictionary<DamageElement, MinMaxFloat> damageAmounts = skill.GetAttackDamages(Entity, data.skillLevel, isLeftHand);
                    skill.ApplySkill(Entity, data.skillLevel, isLeftHand, weapon, hitIndex, damageAmounts, data.targetObjectId, data.aimPosition, applySeed);
                }
            }
            return true;
        }

        public void UseSkill(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (!IsServer && IsOwnerClient)
            {
                // Validate skill
                BaseSkill skill;
                short skillLevel;
                if (!Entity.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out skill, out skillLevel, out _))
                    return;
                // Get simulate seed for simulation validating
                byte simulateSeed = (byte)Random.Range(byte.MinValue, byte.MaxValue);
                // Set use skill state
                IsUsingSkill = true;
                // Simulate skill using at client immediately
                UseSkillRoutine(simulateSeed, isLeftHand, skill, skillLevel, targetObjectId, aimPosition).Forget();
                // Tell server that this client use skill
                sendingClientUseSkill = true;
                sendingSeed = simulateSeed;
                sendingSkillDataId = dataId;
                sendingIsLeftHand = isLeftHand;
                sendingTargetObjectId = targetObjectId;
                sendingAimPosition = aimPosition;
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Get simulate seed for simulation validating
                byte simulateSeed = (byte)Random.Range(byte.MinValue, byte.MaxValue);
                // Use skill immediately at server
                ProceedUseSkillStateAtServer(simulateSeed, dataId, isLeftHand, targetObjectId, aimPosition);
            }
        }

        public void UseSkillItem(short itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (!IsServer && IsOwnerClient)
            {
                // Validate skill
                BaseSkill skill;
                short skillLevel;
                if (!Entity.ValidateSkillItemToUse(itemIndex, isLeftHand, targetObjectId, out skill, out skillLevel, out _))
                    return;
                // Get simulate seed for simulation validating
                byte simulateSeed = (byte)Random.Range(byte.MinValue, byte.MaxValue);
                // Set use skill state
                IsUsingSkill = true;
                // Simulate skill using at client immediately
                UseSkillRoutine(simulateSeed, isLeftHand, skill, skillLevel, targetObjectId, aimPosition).Forget();
                // Tell server that this client use skill
                sendingClientUseSkillItem = true;
                sendingSeed = simulateSeed;
                sendingItemIndex = itemIndex;
                sendingIsLeftHand = isLeftHand;
                sendingTargetObjectId = targetObjectId;
                sendingAimPosition = aimPosition;
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Get simulate seed for simulation validating
                byte simulateSeed = (byte)Random.Range(byte.MinValue, byte.MaxValue);
                // Use skill immediately at server
                ProceedUseSkillItemStateAtServer(simulateSeed, itemIndex, isLeftHand, targetObjectId, aimPosition);
            }
        }

        public bool WriteClientUseSkillState(NetDataWriter writer)
        {
            if (sendingClientUseSkill)
            {
                writer.Put(sendingSeed);
                writer.PutPackedInt(sendingSkillDataId);
                writer.Put(sendingIsLeftHand);
                writer.PutPackedUInt(sendingTargetObjectId);
                writer.Put(sendingAimPosition);
                sendingClientUseSkill = false;
                return true;
            }
            return false;
        }

        public bool WriteServerUseSkillState(NetDataWriter writer)
        {
            if (sendingServerUseSkill)
            {
                writer.Put(sendingSeed);
                writer.PutPackedInt(sendingSkillDataId);
                writer.PutPackedShort(sendingSkillLevel);
                writer.Put(sendingIsLeftHand);
                writer.PutPackedUInt(sendingTargetObjectId);
                writer.Put(sendingAimPosition);
                sendingServerUseSkill = false;
                return true;
            }
            return false;
        }

        public bool WriteClientUseSkillItemState(NetDataWriter writer)
        {
            if (sendingClientUseSkillItem)
            {
                writer.Put(sendingSeed);
                writer.PutPackedShort(sendingItemIndex);
                writer.Put(sendingIsLeftHand);
                writer.PutPackedUInt(sendingTargetObjectId);
                writer.Put(sendingAimPosition);
                sendingClientUseSkillItem = false;
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
            if (sendingClientUseSkillInterrupted)
            {
                sendingClientUseSkillInterrupted = false;
                return true;
            }
            return false;
        }

        public bool WriteServerUseSkillInterruptedState(NetDataWriter writer)
        {
            if (sendingServerUseSkillInterrupted)
            {
                sendingServerUseSkillInterrupted = false;
                return true;
            }
            return false;
        }

        public void ReadClientUseSkillStateAtServer(NetDataReader reader)
        {
            byte simulateSeed = reader.GetByte();
            int dataId = reader.GetPackedInt();
            bool isLeftHand = reader.GetBool();
            uint targetObjectId = reader.GetPackedUInt();
            AimPosition aimPosition = reader.Get<AimPosition>();
            ProceedUseSkillStateAtServer(simulateSeed, dataId, isLeftHand, targetObjectId, aimPosition);
        }

        protected void ProceedUseSkillStateAtServer(byte simulateSeed, int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
#if UNITY_EDITOR || UNITY_SERVER || !MMO_BUILD
            // Speed hack avoidance
            if (Time.unscaledTime - LastUseSkillEndTime < -0.05f)
                return;
            // Validate skill
            BaseSkill skill;
            short skillLevel;
            if (!Entity.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out skill, out skillLevel, out _))
                return;
            // Set use skill state
            IsUsingSkill = true;
            // Play animation at server immediately
            UseSkillRoutine(simulateSeed, isLeftHand, skill, skillLevel, targetObjectId, aimPosition).Forget();
            // Tell clients to play animation later
            sendingServerUseSkill = true;
            sendingSeed = simulateSeed;
            sendingSkillDataId = dataId;
            sendingSkillLevel = skillLevel;
            sendingIsLeftHand = isLeftHand;
            sendingTargetObjectId = targetObjectId;
            sendingAimPosition = aimPosition;
#endif
        }

        public void ReadServerUseSkillStateAtClient(NetDataReader reader)
        {
            byte simulateSeed = reader.GetByte();
            int skillDataId = reader.GetPackedInt();
            short skillLevel = reader.GetPackedShort();
            bool isLeftHand = reader.GetBool();
            uint targetObjectId = reader.GetPackedUInt();
            AimPosition aimPosition = reader.Get<AimPosition>();
            if (IsOwnerClientOrOwnedByServer)
            {
                // Don't play use skill animation again (it already played in `UseSkill` and `UseSkillItem` function)
                return;
            }
            BaseSkill skill;
            if (!GameInstance.Skills.TryGetValue(skillDataId, out skill) && skillLevel > 0)
                ClearUseSkillStates();
            Entity.AttackComponent.CancelAttack();
            UseSkillRoutine(simulateSeed, isLeftHand, skill, skillLevel, targetObjectId, aimPosition).Forget();
        }

        public void ReadClientUseSkillItemStateAtServer(NetDataReader reader)
        {
            byte simulateSeed = reader.GetByte();
            short itemIndex = reader.GetPackedShort();
            bool isLeftHand = reader.GetBool();
            uint targetObjectId = reader.GetPackedUInt();
            AimPosition aimPosition = reader.Get<AimPosition>();
            ProceedUseSkillItemStateAtServer(simulateSeed, itemIndex, isLeftHand, targetObjectId, aimPosition);
        }

        protected void ProceedUseSkillItemStateAtServer(byte simulateSeed, short itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
#if UNITY_EDITOR || UNITY_SERVER || !MMO_BUILD
            // Speed hack avoidance
            if (Time.unscaledTime - LastUseSkillEndTime < -0.05f)
                return;
            // Validate skill item
            BaseSkill skill;
            short skillLevel;
            if (!Entity.ValidateSkillItemToUse(itemIndex, isLeftHand, targetObjectId, out skill, out skillLevel, out _))
                return;
            if (!Entity.DecreaseItemsByIndex(itemIndex, 1))
                return;
            Entity.FillEmptySlots();
            // Set use skill state
            IsUsingSkill = true;
            // Play animation at server immediately
            UseSkillRoutine(simulateSeed, isLeftHand, skill, skillLevel, targetObjectId, aimPosition).Forget();
            // Tell clients to play animation later
            sendingServerUseSkill = true;
            sendingSeed = simulateSeed;
            sendingSkillDataId = skill.DataId;
            sendingSkillLevel = skillLevel;
            sendingIsLeftHand = isLeftHand;
            sendingTargetObjectId = targetObjectId;
            sendingAimPosition = aimPosition;
#endif
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
            if (Entity.PassengingVehicleModel && Entity.PassengingVehicleModel is BaseCharacterModel)
            {
                // Vehicle model
                (Entity.PassengingVehicleModel as BaseCharacterModel).StopActionAnimation();
                (Entity.PassengingVehicleModel as BaseCharacterModel).StopSkillCastAnimation();
                (Entity.PassengingVehicleModel as BaseCharacterModel).StopWeaponChargeAnimation();
            }
            if (IsClient)
            {
                if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                {
                    // FPS model
                    Entity.FpsModel.StopActionAnimation();
                    Entity.FpsModel.StopSkillCastAnimation();
                    Entity.FpsModel.StopWeaponChargeAnimation();
                }
            }
        }
    }
}
