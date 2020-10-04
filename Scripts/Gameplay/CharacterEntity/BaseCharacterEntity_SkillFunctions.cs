using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        protected CancellationTokenSource skillCancellationTokenSource;
        public bool IsCastingSkillCanBeInterrupted { get; protected set; }
        public bool IsCastingSkillInterrupted { get; protected set; }
        public float CastingSkillDuration { get; protected set; }
        public float CastingSkillCountDown { get; protected set; }

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
            WeaponType weaponType = weaponItem.WeaponType;
            // Get activate animation type which defined at character model
            SkillActivateAnimationType useSkillActivateAnimationType = CharacterModel.UseSkillActivateAnimationType(skill);
            // Prepare animation
            if (useSkillActivateAnimationType == SkillActivateAnimationType.UseAttackAnimation && skill.IsAttack())
            {
                // Assign data id
                animationDataId = weaponType.DataId;
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

        protected void InterruptCastingSkill()
        {
            if (IsCastingSkillCanBeInterrupted && !IsCastingSkillInterrupted)
            {
                IsCastingSkillInterrupted = true;
                CallAllOnSkillCastingInterrupt();
            }
        }

        /// <summary>
        /// Is function will be called at server to order character to use skill
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="isLeftHand"></param>
        [ServerRpc]
        protected void ServerUseSkill(int dataId, bool isLeftHand)
        {
#if !CLIENT_BUILD
            UseSkillFunction(dataId, isLeftHand, null);
#endif
        }

        /// <summary>
        /// Is function will be called at server to order character to use skill
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="aimPosition"></param>
        [ServerRpc]
        protected void ServerUseSkillWithAimPosition(int dataId, bool isLeftHand, Vector3 aimPosition)
        {
#if !CLIENT_BUILD
            UseSkillFunction(dataId, isLeftHand, aimPosition);
#endif
        }

        protected virtual void UseSkillFunction(int dataId, bool isLeftHand, Vector3? aimPosition)
        {
            if (!CanUseSkill())
                return;

            BaseSkill skill;
            short skillLevel;
            if (!GameInstance.Skills.TryGetValue(dataId, out skill) ||
                !this.GetCaches().Skills.TryGetValue(skill, out skillLevel))
                return;

            // Validate mp amount, skill level, 
            GameMessage.Type gameMessageType;
            if (!skill.CanUse(this, skillLevel, isLeftHand, out gameMessageType))
                return;

            // Prepare requires data and get skill data
            AnimActionType animActionType;
            int animatonDataId;
            CharacterItem weapon;
            GetUsingSkillData(
                skill,
                ref isLeftHand,
                out animActionType,
                out animatonDataId,
                out weapon);

            // Validate ammo
            if (skill.IsAttack() && !ValidateAmmo(weapon))
                return;

            // Prepare requires data and get animation data
            int animationIndex;
            float animSpeedRate;
            float[] triggerDurations;
            float totalDuration;
            GetRandomAnimationData(
                animActionType,
                animatonDataId,
                out animationIndex,
                out animSpeedRate,
                out triggerDurations,
                out totalDuration);

            // Start use skill routine
            IsAttackingOrUsingSkill = true;

            // Play animations
            if (!aimPosition.HasValue)
                CallAllPlaySkillAnimation(isLeftHand, (byte)animationIndex, skill.DataId, skillLevel);
            else
                CallAllPlaySkillAnimationWithAimPosition(isLeftHand, (byte)animationIndex, skill.DataId, skillLevel, aimPosition.Value);
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

        /// <summary>
        /// This will be called at clients to stop playing skill casting
        /// </summary>
        [AllRpc]
        protected virtual void AllOnSkillCastingInterrupt()
        {
            IsCastingSkillInterrupted = true;
            IsAttackingOrUsingSkill = false;
            CastingSkillDuration = CastingSkillCountDown = 0;
            CancelSkill();
            if (CharacterModel && CharacterModel.gameObject.activeSelf)
            {
                CharacterModel.StopActionAnimation();
                CharacterModel.StopSkillCastAnimation();
            }
            if (FpsModel && FpsModel.gameObject.activeSelf)
            {
                FpsModel.StopActionAnimation();
                FpsModel.StopSkillCastAnimation();
            }
        }

        protected async UniTaskVoid UseSkillRoutine(bool isLeftHand, byte animationIndex, BaseSkill skill, short skillLevel, Vector3? skillAimPosition)
        {
            // Skill animation still playing, skip it
            if (skillCancellationTokenSource != null)
                return;
            // Prepare cancellation
            skillCancellationTokenSource = new CancellationTokenSource();

            // Prepare requires data and get skill data
            AnimActionType animActionType;
            int animActionDataId;
            CharacterItem weapon;
            GetUsingSkillData(
                skill,
                ref isLeftHand,
                out animActionType,
                out animActionDataId,
                out weapon);

            // Prepare requires data and get animation data
            float animSpeedRate;
            float[] triggerDurations;
            float totalDuration;
            GetAnimationData(
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
                newSkillUsage.Use(this, skillLevel);
                skillUsages.Add(newSkillUsage);
            }

            // Prepare requires data and get damages data
            DamageInfo damageInfo = this.GetWeaponDamageInfo(ref isLeftHand);
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = skill.GetAttackDamages(this, skillLevel, isLeftHand);

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileAttackOrUseSkill = GetMoveSpeedRateWhileAttackOrUseSkill(AnimActionType, skill);

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            animSpeedRate *= GetAnimSpeedRate(AnimActionType);

            // Set doing action data
            IsCastingSkillCanBeInterrupted = skill.canBeInterruptedWhileCasting;
            IsCastingSkillInterrupted = false;

            // Get cast duration. Then if cast duration more than 0, it will play cast skill animation.
            CastingSkillDuration = CastingSkillCountDown = skill.GetCastDuration(skillLevel);
            try
            {
                if (CastingSkillDuration > 0f)
                {
                    // Tell clients that character is casting
                    if (IsClient)
                    {
                        if (CharacterModel && CharacterModel.gameObject.activeSelf)
                        {
                            // Play special effect
                            CharacterModel.InstantiateEffect(skill.GetSkillCastEffect());
                            // Play casting animation
                            CharacterModel.PlaySkillCastClip(skill.DataId, CastingSkillDuration);
                        }
                        if (FpsModel && FpsModel.gameObject.activeSelf)
                        {
                            // Play special effect
                            FpsModel.InstantiateEffect(skill.GetSkillCastEffect());
                            // Play casting animation
                            FpsModel.PlaySkillCastClip(skill.DataId, CastingSkillDuration);
                        }
                    }
                    // Wait until end of cast duration
                    await UniTask.Delay((int)(CastingSkillDuration * 1000f), true, PlayerLoopTiming.Update, skillCancellationTokenSource.Token);
                }

                // Animations will plays on clients only
                if (IsClient)
                {
                    if (CharacterModel && CharacterModel.gameObject.activeSelf)
                        CharacterModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                    if (FpsModel && FpsModel.gameObject.activeSelf)
                        FpsModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                }

                float remainsDuration = totalDuration;
                float tempTriggerDuration;
                for (int hitIndex = 0; hitIndex < triggerDurations.Length; ++hitIndex)
                {
                    // Play special effects after trigger duration
                    tempTriggerDuration = totalDuration * triggerDurations[hitIndex];
                    remainsDuration -= tempTriggerDuration;
                    await UniTask.Delay((int)(tempTriggerDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, skillCancellationTokenSource.Token);

                    // Special effects will plays on clients only
                    if (IsClient)
                    {
                        if (CharacterModel && CharacterModel.gameObject.activeSelf)
                            CharacterModel.PlayWeaponLaunchEffect(AnimActionType);
                        if (FpsModel && FpsModel.gameObject.activeSelf)
                            FpsModel.PlayWeaponLaunchEffect(AnimActionType);
                    }

                    // Get aim position by character's forward
                    Vector3 aimPosition = GetDefaultAttackAimPosition(damageInfo, isLeftHand);
                    if (skill.HasCustomAimControls() && skillAimPosition.HasValue)
                        aimPosition = skillAimPosition.Value;
                    else if (HasAimPosition)
                        aimPosition = AimPosition;

                    // Trigger skill event
                    if (onUseSkillRoutine != null)
                        onUseSkillRoutine.Invoke(skill, skillLevel, isLeftHand, weapon, hitIndex, damageAmounts, aimPosition);

                    // Apply skill buffs, summons and attack damages
                    skill.ApplySkill(this, skillLevel, isLeftHand, weapon, hitIndex, damageAmounts, aimPosition);

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
                skillCancellationTokenSource = null;
            }
            // Clear action states at clients and server
            ClearActionStates();
        }

        protected void CancelSkill()
        {
            if (skillCancellationTokenSource != null &&
                !skillCancellationTokenSource.IsCancellationRequested)
                skillCancellationTokenSource.Cancel();
        }
    }
}
