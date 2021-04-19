using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
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
        public bool IsCastingSkillCanBeInterrupted { get; protected set; }
        public bool IsCastingSkillInterrupted { get; protected set; }
        public float CastingSkillDuration { get; protected set; }
        public float CastingSkillCountDown { get; protected set; }
        public float MoveSpeedRateWhileUsingSkill { get; protected set; }

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

        public bool ValidateRequestUseSKill(int dataId, bool isLeftHand)
        {
            if (!Entity.CanUseSkill())
                return false;

            if (!Entity.UpdateLastActionTime())
                return false;

            BaseSkill skill;
            short skillLevel;
            if (!GameInstance.Skills.TryGetValue(dataId, out skill) ||
                !Entity.GetCaches().Skills.TryGetValue(skill, out skillLevel))
                return false;

            UITextKeys gameMessage;
            if (!skill.CanUse(Entity, skillLevel, isLeftHand, out gameMessage))
            {
                Entity.QueueGameMessage(gameMessage);
                return false;
            }
            return true;
        }

        public bool CallServerUseSkill(int dataId, bool isLeftHand, AimPosition aimPosition)
        {
            if (!ValidateRequestUseSKill(dataId, isLeftHand))
                return false;
            RPC(ServerUseSkill, BaseCharacterEntity.ACTION_TO_SERVER_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, dataId, isLeftHand, aimPosition);
            return true;
        }

        /// <summary>
        /// Is function will be called at server to order character to use skill
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="aimPosition"></param>
        [ServerRpc]
        protected void ServerUseSkill(int dataId, bool isLeftHand, AimPosition aimPosition)
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
            if (!skill.CanUse(Entity, skillLevel, isLeftHand, out _))
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

            // Validate ammo
            if (skill.IsAttack() && !Entity.ValidateAmmo(weapon))
                return;

            // Prepare requires data and get animation data
            int animationIndex;
            Entity.GetRandomAnimationData(
                animActionType,
                animatonDataId,
                out animationIndex,
                out _,
                out _,
                out _);

            // Start use skill routine
            IsUsingSkill = true;

            // Play animations
            CallAllPlayUseSkillAnimation(isLeftHand, (byte)animationIndex, skill.DataId, skillLevel, aimPosition);
#endif
        }

        public bool CallAllPlayUseSkillAnimation(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, AimPosition aimPosition)
        {
            if (Entity.IsDead())
                return false;
            RPC(AllPlayUseSkillAnimation, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand, animationIndex, skillDataId, skillLevel, aimPosition);
            return true;
        }

        [AllRpc]
        protected void AllPlayUseSkillAnimation(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, AimPosition aimPosition)
        {
            BaseSkill skill;
            if (GameInstance.Skills.TryGetValue(skillDataId, out skill) && skillLevel > 0)
            {
                UseSkillRoutine(isLeftHand, animationIndex, skill, skillLevel, aimPosition).Forget();
            }
            else
            {
                Entity.ClearActionStates();
            }
        }

        public void InterruptCastingSkill()
        {
            if (IsCastingSkillCanBeInterrupted && !IsCastingSkillInterrupted)
            {
                IsCastingSkillInterrupted = true;
                CallAllOnSkillCastingInterrupt();
            }
        }

        public bool CallServerSkillCastingInterrupt()
        {
            if (Entity.IsDead())
                return false;
            RPC(ServerSkillCastingInterrupt, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered);
            return true;
        }

        /// <summary>
        /// This will be called at server by owner client to stop playing skill casting
        /// </summary>
        [ServerRpc]
        protected virtual void ServerSkillCastingInterrupt()
        {
#if !CLIENT_BUILD
            InterruptCastingSkill();
#endif
        }

        public bool CallAllOnSkillCastingInterrupt()
        {
            if (Entity.IsDead())
                return false;
            RPC(AllOnSkillCastingInterrupt, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered);
            return true;
        }

        /// <summary>
        /// This will be called at clients to stop playing skill casting
        /// </summary>
        [AllRpc]
        protected virtual void AllOnSkillCastingInterrupt()
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

        protected async UniTaskVoid UseSkillRoutine(bool isLeftHand, byte animationIndex, BaseSkill skill, short skillLevel, AimPosition skillAimPosition)
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
                CharacterSkillUsage newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.Skill, skill.DataId);
                newSkillUsage.Use(Entity, skillLevel);
                Entity.SkillUsages.Add(newSkillUsage);
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
            try
            {
                // Play special effect
                if (IsClient)
                {
                    if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                        Entity.CharacterModel.InstantiateEffect(skill.GetSkillCastEffect());
                    if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                        Entity.FpsModel.InstantiateEffect(skill.GetSkillCastEffect());
                }

                if (CastingSkillDuration > 0f)
                {
                    // Play special effect
                    if (IsClient)
                    {
                        if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                            Entity.CharacterModel.PlaySkillCastClip(skill.DataId, CastingSkillDuration);
                        if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                            Entity.FpsModel.PlaySkillCastClip(skill.DataId, CastingSkillDuration);
                    }
                    // Wait until end of cast duration
                    await UniTask.Delay((int)(CastingSkillDuration * 1000f), true, PlayerLoopTiming.Update, skillCancellationTokenSource.Token);
                }

                // Animations will plays on clients only
                if (IsClient)
                {
                    if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                        Entity.CharacterModel.PlayActionAnimation(Entity.AnimActionType, Entity.AnimActionDataId, animationIndex, animSpeedRate);
                    if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                        Entity.FpsModel.PlayActionAnimation(Entity.AnimActionType, Entity.AnimActionDataId, animationIndex, animSpeedRate);
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
                            Entity.CharacterModel.PlayWeaponLaunchEffect(Entity.AnimActionType);
                        if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                            Entity.FpsModel.PlayWeaponLaunchEffect(Entity.AnimActionType);
                        // Play launch sfx
                        if (weaponItem != null &&
                            (Entity.AnimActionType == AnimActionType.AttackRightHand ||
                            Entity.AnimActionType == AnimActionType.AttackLeftHand))
                            AudioManager.PlaySfxClipAtAudioSource(weaponItem.LaunchClip, Entity.CharacterModel.GenericAudioSource);
                    }

                    // Get aim position by character's forward
                    Vector3 aimPosition;
                    if (skill.HasCustomAimControls() && skillAimPosition.hasValue)
                        aimPosition = skillAimPosition.value;
                    else
                        aimPosition = Entity.AimPosition;

                    // Trigger skill event
                    Entity.OnUseSkillRoutine(skill, skillLevel, isLeftHand, weapon, hitIndex, damageAmounts, aimPosition);

                    // Apply skill buffs, summons and attack damages
                    if (IsServer)
                    {
                        skill.ApplySkill(Entity, skillLevel, isLeftHand, weapon, hitIndex, damageAmounts, aimPosition);
                        SimulateLaunchDamageEntityData simulateData = new SimulateLaunchDamageEntityData();
                        if (isLeftHand)
                            simulateData.state |= SimulateLaunchDamageEntityState.IsLeftHand;
                        simulateData.state |= SimulateLaunchDamageEntityState.IsSkill;
                        simulateData.skillDataId = skill.DataId;
                        simulateData.skillLevel = skillLevel;
                        simulateData.hitIndex = hitIndex;
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
            }
            // Clear action states at clients and server
            Entity.ClearActionStates();
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

        public bool ValidateRequestUseSkillItem(short index, bool isLeftHand)
        {
            if (!Entity.CanUseItem())
                return false;

            if (!Entity.UpdateLastActionTime())
                return false;

            if (index >= Entity.NonEquipItems.Count)
                return false;

            if (Entity.NonEquipItems[index].IsLock())
                return false;

            ISkillItem item = Entity.NonEquipItems[index].GetSkillItem();
            if (item == null)
                return false;

            BaseSkill skill = item.UsingSkill;
            short skillLevel = item.UsingSkillLevel;
            if (skill == null)
                return false;

            UITextKeys gameMessage;
            if (!skill.CanUse(Entity, skillLevel, isLeftHand, out gameMessage, true))
            {
                Entity.QueueGameMessage(gameMessage);
                return false;
            }

            return true;
        }

        public bool CallServerUseSkillItem(short index, bool isLeftHand, AimPosition aimPosition)
        {
            if (!ValidateRequestUseSkillItem(index, isLeftHand))
                return false;
            RPC(ServerUseSkillItem, index, isLeftHand, aimPosition);
            return true;
        }

        /// <summary>
        /// This function will be called at server to order character to use item
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="aimPosition"></param>
        [ServerRpc]
        protected void ServerUseSkillItem(short itemIndex, bool isLeftHand, AimPosition aimPosition)
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
            if (!item.UsingSkill.CanUse(Entity, item.UsingSkillLevel, isLeftHand, out _, true))
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

            // Validate ammo
            if (item.UsingSkill.IsAttack() && !Entity.ValidateAmmo(weapon))
                return;

            // Prepare requires data and get animation data
            int animationIndex;
            Entity.GetRandomAnimationData(
                animActionType,
                animActionDataId,
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
            CallAllPlayUseSkillAnimation(isLeftHand, (byte)animationIndex, item.UsingSkill.DataId, item.UsingSkillLevel, aimPosition);
#endif
        }

        public bool CallAllSimulateLaunchDamageEntity(SimulateLaunchDamageEntityData data)
        {
            if (Entity.IsDead())
                return false;
            RPC(AllSimulateLaunchDamageEntity, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, data);
            return true;
        }

        [AllRpc]
        protected void AllSimulateLaunchDamageEntity(SimulateLaunchDamageEntityData data)
        {
            Entity.AttackComponent.SimulateLaunchDamageEntity(data);
        }
    }
}
