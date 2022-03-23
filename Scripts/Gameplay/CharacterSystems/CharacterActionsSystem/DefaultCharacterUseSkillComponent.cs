using Cysharp.Threading.Tasks;
using LiteNetLib;
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

        protected readonly Dictionary<int, SimulatingHit> SimulatingHits = new Dictionary<int, SimulatingHit>();

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

        public bool CallServerUseSkill(byte simulateSeed, int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            RPC(ServerUseSkill, BaseCharacterEntity.ACTION_TO_SERVER_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, simulateSeed, dataId, isLeftHand, targetObjectId, aimPosition);
            return true;
        }

        /// <summary>
        /// Is function will be called at server to order character to use skill
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="targetObjectId"></param>
        /// <param name="aimPosition"></param>
        [ServerRpc]
        protected void ServerUseSkill(byte simulateSeed, int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
#if !CLIENT_BUILD
            // Speed hack avoidance
            if (Time.unscaledTime - LastUseSkillEndTime < -0.05f)
            {
                return;
            }

            // Validate skill
            BaseSkill skill;
            short skillLevel;
            if (!Entity.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out skill, out skillLevel, out _))
            {
                return;
            }

            // Start use skill routine
            IsUsingSkill = true;

            // Play animations
            CallAllPlayUseSkillAnimation(simulateSeed, isLeftHand, skill.DataId, skillLevel, targetObjectId, aimPosition);
#endif
        }

        public bool CallServerUseSkillItem(byte simulateSeed, short index, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            RPC(ServerUseSkillItem, simulateSeed, index, isLeftHand, targetObjectId, aimPosition);
            return true;
        }

        /// <summary>
        /// This function will be called at server to order character to use item
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="targetObjectId"></param>
        /// <param name="aimPosition"></param>
        [ServerRpc]
        protected void ServerUseSkillItem(byte simulateSeed, short itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
#if !CLIENT_BUILD
            // Speed hack avoidance
            if (Time.unscaledTime - LastUseSkillEndTime < -0.05f)
            {
                return;
            }

            BaseSkill skill;
            short skillLevel;
            if (!Entity.ValidateSkillItemToUse(itemIndex, isLeftHand, targetObjectId, out skill, out skillLevel, out _))
            {
                return;
            }

            // Validate skill item
            if (!Entity.DecreaseItemsByIndex(itemIndex, 1))
            {
                return;
            }
            Entity.FillEmptySlots();

            // Start use skill routine
            IsUsingSkill = true;

            // Play animations
            CallAllPlayUseSkillAnimation(simulateSeed, isLeftHand, skill.DataId, skillLevel, targetObjectId, aimPosition);
#endif
        }

        public bool CallAllPlayUseSkillAnimation(byte simulateSeed, bool isLeftHand, int skillDataId, short skillLevel, uint targetObjectId, AimPosition aimPosition)
        {
            RPC(AllPlayUseSkillAnimation, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, simulateSeed, isLeftHand, skillDataId, skillLevel, targetObjectId, aimPosition);
            return true;
        }

        [AllRpc]
        protected void AllPlayUseSkillAnimation(byte simulateSeed, bool isLeftHand, int skillDataId, short skillLevel, uint targetObjectId, AimPosition aimPosition)
        {
            if (IsOwnerClientOrOwnedByServer)
                return;
            BaseSkill skill;
            if (GameInstance.Skills.TryGetValue(skillDataId, out skill) && skillLevel > 0)
            {
                Entity.AttackComponent.CancelAttack();
                UseSkillRoutine(simulateSeed, isLeftHand, skill, skillLevel, targetObjectId, aimPosition).Forget();
            }
            else
            {
                ClearUseSkillStates();
            }
        }

        public void InterruptCastingSkill()
        {
            if (!IsServer)
            {
                CallServerInterruptCastingSkill();
                return;
            }
            if (IsCastingSkillCanBeInterrupted && !IsCastingSkillInterrupted)
            {
                IsCastingSkillInterrupted = true;
                CallAllOnInterruptCastingSkill();
            }
        }

        public bool CallServerInterruptCastingSkill()
        {
            RPC(ServerInterruptCastingSkill, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered);
            return true;
        }

        /// <summary>
        /// This will be called at server by owner client to stop playing skill casting
        /// </summary>
        [ServerRpc]
        protected virtual void ServerInterruptCastingSkill()
        {
#if !CLIENT_BUILD
            InterruptCastingSkill();
#endif
        }

        public bool CallAllOnInterruptCastingSkill()
        {
            RPC(AllOnInterruptCastingSkill, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered);
            return true;
        }

        /// <summary>
        /// This will be called at clients to stop playing skill casting
        /// </summary>
        [AllRpc]
        protected virtual void AllOnInterruptCastingSkill()
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
            if (Entity.PassengingVehicleEntity != null && Entity.PassengingVehicleEntity.Entity.Model &&
                Entity.PassengingVehicleEntity.Entity.Model.gameObject.activeSelf &&
                Entity.PassengingVehicleEntity.Entity.Model is BaseCharacterModel)
            {
                // Vehicle model
                (Entity.PassengingVehicleEntity.Entity.Model as BaseCharacterModel).StopActionAnimation();
                (Entity.PassengingVehicleEntity.Entity.Model as BaseCharacterModel).StopSkillCastAnimation();
                (Entity.PassengingVehicleEntity.Entity.Model as BaseCharacterModel).StopWeaponChargeAnimation();
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
                    if (Entity.PassengingVehicleEntity != null && Entity.PassengingVehicleEntity.Entity.Model &&
                        Entity.PassengingVehicleEntity.Entity.Model.gameObject.activeSelf &&
                        Entity.PassengingVehicleEntity.Entity.Model is BaseCharacterModel)
                    {
                        // Vehicle model
                        (Entity.PassengingVehicleEntity.Entity.Model as BaseCharacterModel).PlaySkillCastClip(skill.DataId, CastingSkillDuration);
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
                if (Entity.PassengingVehicleEntity != null && Entity.PassengingVehicleEntity.Entity.Model &&
                    Entity.PassengingVehicleEntity.Entity.Model.gameObject.activeSelf &&
                    Entity.PassengingVehicleEntity.Entity.Model is BaseCharacterModel)
                {
                    // Vehicle model
                    (Entity.PassengingVehicleEntity.Entity.Model as BaseCharacterModel).PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
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
                SimulatingHits[simulateSeed] = new SimulatingHit(triggerDurations.Length);
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
                        int useSkillSeed = unchecked(simulateSeed + (hitIndex * 16));
                        skill.ApplySkill(Entity, skillLevel, isLeftHand, weapon, hitIndex, damageAmounts, targetObjectId, aimPosition, useSkillSeed);
                        SimulateLaunchDamageEntityData simulateData = new SimulateLaunchDamageEntityData();
                        if (isLeftHand)
                            simulateData.state |= SimulateLaunchDamageEntityState.IsLeftHand;
                        simulateData.state |= SimulateLaunchDamageEntityState.IsSkill;
                        simulateData.randomSeed = useSkillSeed;
                        simulateData.targetObjectId = targetObjectId;
                        simulateData.skillDataId = skill.DataId;
                        simulateData.skillLevel = skillLevel;
                        simulateData.aimPosition = aimPosition;
                        CallAllSimulateLaunchDamageEntity(simulateData);
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

        public bool CallAllSimulateLaunchDamageEntity(SimulateLaunchDamageEntityData data)
        {
            RPC(AllSimulateLaunchDamageEntity, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, data);
            return true;
        }

        [AllRpc]
        protected void AllSimulateLaunchDamageEntity(SimulateLaunchDamageEntityData data)
        {
            if (IsOwnerClientOrOwnedByServer)
                return;
            SimulatingHit simulatingHit;
            if (!SimulatingHits.TryGetValue(data.randomSeed, out simulatingHit) || simulatingHit.hitIndex >= simulatingHit.triggerLength)
                return;
            int hitIndex = SimulatingHits[data.randomSeed].hitIndex + 1;
            simulatingHit.hitIndex = hitIndex;
            SimulatingHits[data.randomSeed] = simulatingHit;
            bool isLeftHand = data.state.HasFlag(SimulateLaunchDamageEntityState.IsLeftHand);
            if (data.state.HasFlag(SimulateLaunchDamageEntityState.IsSkill))
            {
                BaseSkill skill = data.GetSkill();
                if (skill != null)
                {
                    CharacterItem weapon = Entity.GetAvailableWeapon(ref isLeftHand);
                    Dictionary<DamageElement, MinMaxFloat> damageAmounts = skill.GetAttackDamages(Entity, data.skillLevel, isLeftHand);
                    int useSkillSeed = unchecked(data.randomSeed + (hitIndex * 16));
                    skill.ApplySkill(Entity, data.skillLevel, isLeftHand, weapon, hitIndex, damageAmounts, data.targetObjectId, data.aimPosition, useSkillSeed);
                }
            }
        }

        public void UseSkill(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            if (Time.unscaledTime - LastUseSkillEndTime < 0f)
                return;

            // Validate skill
            BaseSkill skill;
            short skillLevel;
            if (!Entity.ValidateSkillToUse(dataId, isLeftHand, targetObjectId, out skill, out skillLevel, out _))
                return;

            // Set use skill state
            IsUsingSkill = true;

            // Get simulate seed for simulation validating
            byte simulateSeed = (byte)Random.Range(byte.MinValue, byte.MaxValue);

            // Simulate skill using at client immediately
            UseSkillRoutine(simulateSeed, isLeftHand, skill, skillLevel, targetObjectId, aimPosition).Forget();

            // Tell the server to use skill
            if (!IsServer)
            {
                CallServerUseSkill(simulateSeed, dataId, isLeftHand, targetObjectId, aimPosition);
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                CallAllPlayUseSkillAnimation(simulateSeed, isLeftHand, skill.DataId, skillLevel, targetObjectId, aimPosition);
            }
        }

        public void UseSkillItem(short itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            // Validate skill item
            BaseSkill skill;
            short skillLevel;
            if (!Entity.ValidateSkillItemToUse(itemIndex, isLeftHand, targetObjectId, out skill, out skillLevel, out _))
                return;

            // Set use skill state
            IsUsingSkill = true;

            // Get simulate seed for simulation validating
            byte simulateSeed = (byte)Random.Range(byte.MinValue, byte.MaxValue);

            // Simulate skill using at client immediately
            UseSkillRoutine(simulateSeed, isLeftHand, skill, skillLevel, targetObjectId, aimPosition).Forget();

            // Tell the server to use skill item
            if (!IsServer)
            {
                CallServerUseSkillItem(simulateSeed, itemIndex, isLeftHand, targetObjectId, aimPosition);
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Decrease item immediately
                if (!Entity.DecreaseItemsByIndex(itemIndex, 1))
                {
                    return;
                }
                Entity.FillEmptySlots();
                CallAllPlayUseSkillAnimation(simulateSeed, isLeftHand, skill.DataId, skillLevel, targetObjectId, aimPosition);
            }
        }
    }
}
