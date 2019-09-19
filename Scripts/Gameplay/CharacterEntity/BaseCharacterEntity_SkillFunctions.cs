using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public event UseSkillRoutineDelegate onUseSkillRoutine;

        public virtual void GetUsingSkillData(
            Skill skill,
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
            if (useSkillActivateAnimationType == SkillActivateAnimationType.UseAttackAnimation && skill.skillAttackType != SkillAttackType.None)
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

        public virtual void GetSkillDamages(CharacterItem weapon, Skill skill, short level, out DamageInfo damageInfo, out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            Item weaponItem = weapon.GetWeaponItem();
            damageInfo = weaponItem.WeaponType.damageInfo;
            allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // If it is attack skill
            if (skill.skillAttackType != SkillAttackType.None)
            {
                switch (skill.skillAttackType)
                {
                    case SkillAttackType.Normal:
                        // Assign damage data
                        damageInfo = skill.damageInfo;
                        // Sum damage with skill damage because this skill damages based on itself
                        allDamageAmounts = GameDataHelpers.CombineDamages(
                            allDamageAmounts,
                            skill.GetAdditionalDamageAmounts(level));
                        // Sum damage with additional damage amounts
                        allDamageAmounts = GameDataHelpers.CombineDamages(
                            allDamageAmounts,
                            skill.GetDamageAmount(level, this));
                        break;
                    case SkillAttackType.BasedOnWeapon:
                        // Assign damage data
                        damageInfo = weaponItem.WeaponType.damageInfo;
                        // Calculate all damages
                        allDamageAmounts = weaponItem.GetDamageAmountWithInflictions(
                            weapon.level,
                            weapon.GetEquipmentBonusRate(),
                            this,
                            skill.GetWeaponDamageInflictions(level));
                        // Sum damage with additional damage amounts
                        allDamageAmounts = GameDataHelpers.CombineDamages(
                            allDamageAmounts,
                            skill.GetAdditionalDamageAmounts(level));
                        break;
                }
                // Sum damage with buffs
                allDamageAmounts = GameDataHelpers.CombineDamages(
                    allDamageAmounts,
                    this.GetCaches().IncreaseDamages);
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
        /// <param name="hasAimPosition"></param>
        /// <param name="aimPosition"></param>
        protected virtual void NetFuncUseSkill(int dataId, bool isLeftHand, bool hasAimPosition, Vector3 aimPosition)
        {
            if (!CanUseSkill())
                return;

            Skill skill;
            short skillLevel;
            if (!GameInstance.Skills.TryGetValue(dataId, out skill) ||
                !this.GetCaches().Skills.TryGetValue(skill, out skillLevel))
                return;

            // Validate mp amount, skill level, 
            if (!skill.CanUse(this, skillLevel))
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
            if (skill.skillAttackType != SkillAttackType.None && !ValidateAmmo(weapon))
                return;

            // Prepare requires data and get animation data
            int animationIndex;
            float triggerDuration;
            float totalDuration;
            GetRandomAnimationData(
                animActionType,
                animatonDataId,
                out animationIndex,
                out triggerDuration,
                out totalDuration);

            // TODO: some skill type will not able to change aim position by controller
            if (!hasAimPosition && HasAimPosition)
            {
                hasAimPosition = true;
                aimPosition = AimPosition;
            }
            
            // Start use skill routine
            isAttackingOrUsingSkill = true;

            // Play animations
            if (hasAimPosition)
                RequestPlaySkillAnimation(isLeftHand, (byte)animationIndex, skill.DataId, skillLevel, aimPosition);
            else
                RequestPlaySkillAnimation(isLeftHand, (byte)animationIndex, skill.DataId, skillLevel);
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

        protected IEnumerator UseSkillRoutine(bool isLeftHand, byte animationIndex, Skill skill, short skillLevel, bool hasAimPosition, Vector3 aimPosition)
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
            float triggerDuration;
            float totalDuration;
            GetAnimationData(
                animActionType,
                animationDataId,
                animationIndex,
                out triggerDuration,
                out totalDuration);

            // Prepare requires data and get damages data
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;
            GetSkillDamages(
                weapon,
                skill,
                level,
                out damageInfo,
                out allDamageAmounts);

            // Set doing action state at clients and server
            isAttackingOrUsingSkill = true;

            // Calculate move speed rate while doing action at clients and server
            moveSpeedRateWhileAttackOrUseSkill = GetMoveSpeedRateWhileAttackOrUseSkill(animActionType, null);

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            float playSpeedMultiplier = GetAnimSpeedRate(animActionType);
            
            // Set doing action data
            isCastingSkillCanBeInterrupted = skill.canBeInterruptedWhileCasting;
            isCastingSkillInterrupted = false;
            // Get cast duration. Then if cast duration more than 0, it will play cast skill animation.
            castingSkillDuration = castingSkillCountDown = skill.GetCastDuration(level);
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

                // Play special effects after trigger duration
                yield return new WaitForSecondsRealtime(triggerDuration * playSpeedMultiplier);

                // Special effects will plays on clients only
                if (IsClient)
                    CharacterModel.PlayWeaponLaunchEffect(animActionType);

                if (!hasAimPosition)
                {
                    // No aim position set, set aim position to forward direction
                    Transform damageTransform = GetDamageTransform(damageInfo.damageType, isLeftHand);
                    aimPosition = damageTransform.position + damageTransform.forward;
                }

                // Skip use skill function when using skill will override default skill functionality
                if (!skill.OnApplySkill(this, skillLevel, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition))
                {
                    // Trigger skill event
                    if (onUseSkillRoutine != null)
                        onUseSkillRoutine.Invoke(skill, skillLevel, isLeftHand, weapon, damageInfo, allDamageAmounts, aimPosition);

                    // Apply skill buffs, summons and attack damages
                    ApplySkill(
                        skill,
                        skillLevel,
                        isLeftHand,
                        weapon,
                        damageInfo,
                        allDamageAmounts,
                        hasAimPosition,
                        aimPosition);
                }

                // Wait until animation ends to stop actions
                yield return new WaitForSecondsRealtime((totalDuration - triggerDuration) * playSpeedMultiplier);
            }

            // Set doing action state to none at clients and server
            animActionType = AnimActionType.None;
            isAttackingOrUsingSkill = false;
        }

        protected virtual void ApplySkill(Skill skill, short skillLevel, bool isLeftHand, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, bool hasAimPosition, Vector3 aimPosition)
        {
            // Apply skills only when it's active skill
            if (skill.skillType != SkillType.Active)
                return;

            // Apply buff, summons at server only
            if (IsServer)
            {
                ApplySkillBuff(skill, skillLevel);
                ApplySkillSummon(skill, skillLevel);
                ApplySkillMount(skill, skillLevel);
            }

            // Apply attack skill
            if (skill.skillAttackType != SkillAttackType.None)
            {
                if (IsServer)
                {
                    // Increase damage with ammo damage
                    Dictionary<DamageElement, MinMaxFloat> increaseDamages;
                    ReduceAmmo(weapon, isLeftHand, out increaseDamages);
                    if (increaseDamages != null)
                        allDamageAmounts = GameDataHelpers.CombineDamages(allDamageAmounts, increaseDamages);
                }

                // Apply debuff
                CharacterBuff debuff = CharacterBuff.Empty;
                if (skill.isDebuff)
                    debuff = CharacterBuff.Create(BuffType.SkillDebuff, skill.DataId, skillLevel);

                // Launch damage entity to apply damage to other characters
                LaunchDamageEntity(
                    isLeftHand,
                    weapon,
                    damageInfo,
                    allDamageAmounts,
                    debuff,
                    skill,
                    aimPosition,
                    Vector3.zero);
            }
        }
    }
}
