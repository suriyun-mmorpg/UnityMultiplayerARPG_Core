using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public class LegacyCharacterUseSkillComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterUseSkillComponent
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

        protected int simulatingHitIndex = 0;
        protected int simulatingTriggerLength = 0;

        public override void EntityUpdate()
        {
            // Update casting skill count down, will show gage at clients
            if (CastingSkillCountDown > 0)
                CastingSkillCountDown -= Time.unscaledDeltaTime;
        }

        protected virtual void SetUseSkillActionStates(AnimActionType animActionType, int animActionDataId, BaseSkill usingSkill, short usingSkillLevel)
        {
            Entity.ClearActionStates();
            Entity.AnimActionType = animActionType;
            Entity.AnimActionDataId = animActionDataId;
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

        public bool CallServerUseSkill(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            RPC(ServerUseSkill, BaseCharacterEntity.ACTION_TO_SERVER_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, dataId, isLeftHand, targetObjectId, aimPosition);
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
        protected void ServerUseSkill(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
#if !CLIENT_BUILD
            if (!Entity.CanUseSkill())
                return;

            BaseSkill skill;
            short skillLevel;
            if (!GameInstance.Skills.TryGetValue(dataId, out skill) ||
                !Entity.GetCaches().Skills.TryGetValue(skill, out skillLevel))
                return;

            // Validate mp amount, skill level, 
            if (!skill.CanUse(Entity, skillLevel, isLeftHand, targetObjectId, out _))
                return;

            // Prepare requires data and get skill data
            AnimActionType animActionType;
            int animatonDataId;
            CharacterItem weapon;
            Entity.GetUsingSkillData(
                skill,
                ref isLeftHand,
                out animActionType,
                out animatonDataId,
                out weapon);

            // Prepare requires data and get animation data
            int animationIndex;
            Entity.GetRandomAnimationData(
                animActionType,
                animatonDataId,
                Random.Range(int.MinValue, int.MaxValue),
                out animationIndex,
                out _,
                out _,
                out _);

            // Start use skill routine
            IsUsingSkill = true;

            // Play animations
            CallAllPlayUseSkillAnimation(isLeftHand, (byte)animationIndex, skill.DataId, skillLevel, targetObjectId, aimPosition);
#endif
        }

        public bool CallAllPlayUseSkillAnimation(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, uint targetObjectId, AimPosition aimPosition)
        {
            RPC(AllPlayUseSkillAnimation, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand, animationIndex, skillDataId, skillLevel, targetObjectId, aimPosition);
            return true;
        }

        [AllRpc]
        protected void AllPlayUseSkillAnimation(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, uint targetObjectId, AimPosition aimPosition)
        {
            BaseSkill skill;
            if (GameInstance.Skills.TryGetValue(skillDataId, out skill) && skillLevel > 0)
            {
                UseSkillRoutine(isLeftHand, animationIndex, skill, skillLevel, targetObjectId, aimPosition).Forget();
            }
            else
            {
                Entity.ClearActionStates();
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
                Entity.CharacterModel.StopActionAnimation();
                Entity.CharacterModel.StopSkillCastAnimation();
                Entity.CharacterModel.StopWeaponChargeAnimation();
            }
            if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
            {
                Entity.FpsModel.StopActionAnimation();
                Entity.FpsModel.StopSkillCastAnimation();
                Entity.FpsModel.StopWeaponChargeAnimation();
            }
        }

        protected async UniTaskVoid UseSkillRoutine(bool isLeftHand, byte animationIndex, BaseSkill skill, short skillLevel, uint targetObjectId, AimPosition skillAimPosition)
        {
            // Prepare cancellation
            CancellationTokenSource skillCancellationTokenSource = new CancellationTokenSource();
            skillCancellationTokenSources.Add(skillCancellationTokenSource);

            // Prepare requires data and get skill data
            AnimActionType animActionType;
            int animActionDataId;
            CharacterItem weapon;
            Entity.GetUsingSkillData(
                skill,
                ref isLeftHand,
                out animActionType,
                out animActionDataId,
                out weapon);

            // Prepare requires data and get animation data
            float animSpeedRate;
            float[] triggerDurations;
            float totalDuration;
            Entity.GetAnimationData(
                animActionType,
                animActionDataId,
                animationIndex,
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

            // Prepare requires data and get damages data
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = skill.GetAttackDamages(Entity, skillLevel, isLeftHand);

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileUsingSkill = skill.moveSpeedRateWhileUsingSkill;

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            animSpeedRate *= Entity.GetAnimSpeedRate(Entity.AnimActionType);

            // Set doing action data
            IsCastingSkillCanBeInterrupted = skill.canBeInterruptedWhileCasting;
            IsCastingSkillInterrupted = false;

            // Get cast duration. Then if cast duration more than 0, it will play cast skill animation.
            CastingSkillDuration = CastingSkillCountDown = skill.GetCastDuration(skillLevel);

            // Get simulate seed for simulation validating
            byte simulateSeed = (byte)Random.Range(byte.MinValue, byte.MaxValue);
            simulatingHitIndex = 0;
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
                    Entity.CharacterModel.PlayActionAnimation(Entity.AnimActionType, Entity.AnimActionDataId, animationIndex, animSpeedRate);
                }
                if (Entity.PassengingVehicleEntity != null && Entity.PassengingVehicleEntity.Entity.Model &&
                    Entity.PassengingVehicleEntity.Entity.Model.gameObject.activeSelf &&
                    Entity.PassengingVehicleEntity.Entity.Model is BaseCharacterModel)
                {
                    // Vehicle model
                    (Entity.PassengingVehicleEntity.Entity.Model as BaseCharacterModel).PlayActionAnimation(Entity.AnimActionType, Entity.AnimActionDataId, animationIndex, animSpeedRate);
                }
                if (IsClient)
                {
                    if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                    {
                        // FPS model
                        Entity.FpsModel.PlayActionAnimation(Entity.AnimActionType, Entity.AnimActionDataId, animationIndex, animSpeedRate);
                    }
                }

                float remainsDuration = totalDuration;
                float tempTriggerDuration;
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
                            (Entity.AnimActionType == AnimActionType.AttackRightHand ||
                            Entity.AnimActionType == AnimActionType.AttackLeftHand))
                            AudioManager.PlaySfxClipAtAudioSource(weaponItem.LaunchClip, Entity.CharacterModel.GenericAudioSource);
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
                    if (IsServer)
                    {
                        int useSkillSeed = unchecked(simulateSeed + (hitIndex * 16));
                        skill.ApplySkill(Entity, skillLevel, isLeftHand, weapon, hitIndex, damageAmounts, targetObjectId, aimPosition, useSkillSeed, null);
                        SimulateLaunchDamageEntityData simulateData = new SimulateLaunchDamageEntityData();
                        if (isLeftHand)
                            simulateData.state |= SimulateLaunchDamageEntityState.IsLeftHand;
                        simulateData.state |= SimulateLaunchDamageEntityState.IsSkill;
                        simulateData.randomSeed = simulateSeed;
                        simulateData.skillDataId = skill.DataId;
                        simulateData.skillLevel = skillLevel;
                        simulateData.targetObjectId = targetObjectId;
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
            catch
            {
                // Catch the cancellation
            }
            finally
            {
                skillCancellationTokenSource.Dispose();
                skillCancellationTokenSources.Remove(skillCancellationTokenSource);
                // Clear simulate validating data
                simulatingTriggerLength = 0;
            }
            // Clear action states at clients and server
            Entity.ClearActionStates();
            LastUseSkillEndTime = Time.unscaledTime;
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

        public bool CallServerUseSkillItem(short index, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            RPC(ServerUseSkillItem, index, isLeftHand, targetObjectId, aimPosition);
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
        protected void ServerUseSkillItem(short itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
#if !CLIENT_BUILD
            if (!Entity.CanUseItem() || !Entity.CanUseSkill())
                return;

            if (itemIndex >= Entity.NonEquipItems.Count)
                return;

            CharacterItem characterItem = Entity.NonEquipItems[itemIndex];
            if (characterItem.IsLock())
                return;

            ISkillItem item = characterItem.GetSkillItem();
            if (!Entity.CanUseItem() || !Entity.CanUseSkill() || item == null || item.UsingSkill == null)
                return;

            // Validate mp amount, skill level
            if (!item.UsingSkill.CanUse(Entity, item.UsingSkillLevel, isLeftHand, targetObjectId, out _, true))
                return;

            // Prepare requires data and get skill data
            AnimActionType animActionType;
            int animActionDataId;
            CharacterItem weapon;
            Entity.GetUsingSkillData(
                item.UsingSkill,
                ref isLeftHand,
                out animActionType,
                out animActionDataId,
                out weapon);

            // Prepare requires data and get animation data
            int animationIndex;
            Entity.GetRandomAnimationData(
                animActionType,
                animActionDataId,
                Random.Range(int.MinValue, int.MaxValue),
                out animationIndex,
                out _,
                out _,
                out _);

            // Validate skill item
            if (!Entity.DecreaseItemsByIndex(itemIndex, 1))
                return;
            Entity.FillEmptySlots();

            // Start use skill routine
            IsUsingSkill = true;

            // Play animations
            CallAllPlayUseSkillAnimation(isLeftHand, (byte)animationIndex, item.UsingSkill.DataId, item.UsingSkillLevel, targetObjectId, aimPosition);
#endif
        }

        public bool CallAllSimulateLaunchDamageEntity(SimulateLaunchDamageEntityData data)
        {
            RPC(AllSimulateLaunchDamageEntity, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, data);
            return true;
        }

        [AllRpc]
        protected void AllSimulateLaunchDamageEntity(SimulateLaunchDamageEntityData data)
        {
            if (IsServer)
                return;
            if (simulatingHitIndex >= simulatingTriggerLength)
                return;
            simulatingHitIndex++;
            bool isLeftHand = data.state.HasFlag(SimulateLaunchDamageEntityState.IsLeftHand);
            if (data.state.HasFlag(SimulateLaunchDamageEntityState.IsSkill))
            {
                BaseSkill skill = data.GetSkill();
                if (skill != null)
                {
                    CharacterItem weapon = Entity.GetAvailableWeapon(ref isLeftHand);
                    Dictionary<DamageElement, MinMaxFloat> damageAmounts = skill.GetAttackDamages(Entity, data.skillLevel, isLeftHand);
                    int useSkillSeed = unchecked(data.randomSeed + (simulatingHitIndex * 16));
                    skill.ApplySkill(Entity, data.skillLevel, isLeftHand, weapon, simulatingHitIndex, damageAmounts, data.targetObjectId, data.aimPosition, useSkillSeed, null);
                }
            }
        }

        public void UseSkill(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            CallServerUseSkill(dataId, isLeftHand, targetObjectId, aimPosition);
        }

        public void UseSkillItem(short itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition)
        {
            CallServerUseSkillItem(itemIndex, isLeftHand, targetObjectId, aimPosition);
        }
    }
}
