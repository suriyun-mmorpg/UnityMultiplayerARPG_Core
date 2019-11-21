using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public event UseSkillRoutineDelegate onUseSkillRoutine;

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
            Item weaponItem = weapon.GetWeaponItem();
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
                animActionType = !isLeftHand ? AnimActionType.SkillRightHand : AnimActionType.AttackLeftHand;
            }
        }

        protected void InterruptCastingSkill()
        {
            if (isCastingSkillCanBeInterrupted && !isCastingSkillInterrupted)
            {
                isCastingSkillInterrupted = true;
                RequestSkillCastingInterrupted();
            }
        }

        /// <summary>
        /// Is function will be called at server to order character to use skill
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="aimPosition"></param>
        protected virtual void NetFuncUseSkill(int dataId, bool isLeftHand, Vector3 aimPosition)
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
            float[] triggerDurations;
            float totalDuration;
            GetRandomAnimationData(
                animActionType,
                animatonDataId,
                out animationIndex,
                out triggerDurations,
                out totalDuration);
            
            // Start use skill routine
            isAttackingOrUsingSkill = true;

            // Play animations
            RequestPlaySkillAnimation(isLeftHand, (byte)animationIndex, skill.DataId, skillLevel, aimPosition);
        }

        /// <summary>
        /// This will be called at clients to stop playing skill casting
        /// </summary>
        protected virtual void NetFuncSkillCastingInterrupted()
        {
            isAttackingOrUsingSkill = false;
            castingSkillDuration = castingSkillCountDown = 0;
            if (CharacterModel != null)
            {
                CharacterModel.StopActionAnimation();
                CharacterModel.StopSkillCastAnimation();
            }
        }

        protected IEnumerator UseSkillRoutine(bool isLeftHand, byte animationIndex, BaseSkill skill, short skillLevel, Vector3 aimPosition)
        {
            // Update skill usage states at server only
            if (IsServer)
            {
                CharacterSkillUsage newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.Skill, skill.DataId);
                newSkillUsage.Use(this, skillLevel);
                skillUsages.Add(newSkillUsage);
            }

            // Prepare requires data and get skill data
            int animationDataId;
            CharacterItem weapon;
            GetUsingSkillData(
                skill,
                ref isLeftHand,
                out animActionType,
                out animationDataId,
                out weapon);

            // Prepare requires data and get animation data
            float[] triggerDurations;
            float totalDuration;
            GetAnimationData(
                animActionType,
                animationDataId,
                animationIndex,
                out triggerDurations,
                out totalDuration);

            // Prepare requires data and get damages data
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = skill.GetAttackDamages(this, skillLevel, isLeftHand);

            // Set doing action state at clients and server
            isAttackingOrUsingSkill = true;

            // Calculate move speed rate while doing action at clients and server
            moveSpeedRateWhileAttackOrUseSkill = GetMoveSpeedRateWhileAttackOrUseSkill(animActionType, skill);

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            float playSpeedMultiplier = GetAnimSpeedRate(animActionType);
            
            // Set doing action data
            isCastingSkillCanBeInterrupted = skill.canBeInterruptedWhileCasting;
            isCastingSkillInterrupted = false;
            // Get cast duration. Then if cast duration more than 0, it will play cast skill animation.
            castingSkillDuration = castingSkillCountDown = skill.GetCastDuration(skillLevel);
            if (castingSkillDuration > 0f)
            {
                // Tell clients that character is casting
                // Play special effect
                if (IsClient)
                    Model.InstantiateEffect(skill.castEffects.effects);
                // Play casting animation
                if (IsClient)
                    CharacterModel.PlaySkillCastClip(skill.DataId, castingSkillDuration);
                // Wait until end of cast duration
                yield return new WaitForSeconds(castingSkillDuration);
            }

            // Continue skill activating action or not?
            if (!isCastingSkillInterrupted || !isCastingSkillCanBeInterrupted)
            {
                // Animations will plays on clients only
                if (IsClient)
                    CharacterModel.PlayActionAnimation(animActionType, animationDataId, animationIndex, playSpeedMultiplier);

                float remainsDuration = totalDuration;
                float tempTriggerDuration;
                for (int hitIndex = 0; hitIndex < triggerDurations.Length && remainsDuration > 0f; ++hitIndex)
                {
                    // Play special effects after trigger duration
                    tempTriggerDuration = totalDuration * triggerDurations[hitIndex];
                    remainsDuration -= tempTriggerDuration;
                    yield return new WaitForSecondsRealtime(tempTriggerDuration / playSpeedMultiplier);

                    // Special effects will plays on clients only
                    if (IsClient)
                        CharacterModel.PlayWeaponLaunchEffect(animActionType);

                    // Trigger skill event
                    if (onUseSkillRoutine != null)
                        onUseSkillRoutine.Invoke(skill, skillLevel, isLeftHand, weapon, hitIndex, damageAmounts, aimPosition);

                    // Apply skill buffs, summons and attack damages
                    skill.ApplySkill(this, skillLevel, isLeftHand, weapon, hitIndex, damageAmounts, aimPosition);
                }

                if (remainsDuration > 0f)
                {
                    // Wait until animation ends to stop actions
                    yield return new WaitForSecondsRealtime(remainsDuration / playSpeedMultiplier);
                }
            }

            // Set doing action state to none at clients and server
            animActionType = AnimActionType.None;
            isAttackingOrUsingSkill = false;
        }
    }
}
