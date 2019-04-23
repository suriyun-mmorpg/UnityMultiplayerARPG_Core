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
            short level,
            bool isLeftHand,
            out AnimActionType animActionType,
            out int skillOrWeaponTypeDataId,
            out int animationIndex,
            out CharacterItem weapon,
            out float triggerDuration,
            out float totalDuration,
            out DamageInfo damageInfo,
            out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            // Initialize data
            animActionType = AnimActionType.None;
            skillOrWeaponTypeDataId = 0;
            animationIndex = 0;
            weapon = this.GetAvailableWeapon(isLeftHand);
            triggerDuration = 0f;
            totalDuration = 0f;
            damageInfo = null;
            allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Prepare skill data
            if (skill == null)
                return;
            // Prepare weapon data
            Item weaponItem = weapon.GetWeaponItem();
            WeaponType weaponType = weaponItem.WeaponType;
            bool hasSkillAnimation = CharacterModel.HasSkillAnimations(skill);
            // Prepare animation
            if (!hasSkillAnimation && skill.skillAttackType != SkillAttackType.None)
            {
                // If there is no cast animations
                // Assign data id
                skillOrWeaponTypeDataId = weaponType.DataId;
                // Assign animation action type
                animActionType = !isLeftHand ? AnimActionType.AttackRightHand : AnimActionType.AttackLeftHand;
                // Random animation
                if (!isLeftHand)
                    CharacterModel.GetRandomRightHandAttackAnimation(skillOrWeaponTypeDataId, out animationIndex, out triggerDuration, out totalDuration);
                else
                    CharacterModel.GetRandomLeftHandAttackAnimation(skillOrWeaponTypeDataId, out animationIndex, out triggerDuration, out totalDuration);
            }
            else if (hasSkillAnimation)
            {
                // Assign data id
                skillOrWeaponTypeDataId = skill.DataId;
                // Assign animation action type
                animActionType = AnimActionType.Skill;
                // Random animation
                CharacterModel.GetSkillActivateAnimation(skillOrWeaponTypeDataId, out triggerDuration, out totalDuration);
            }
            // If it is attack skill
            if (skill.skillAttackType != SkillAttackType.None)
            {
                switch (skill.skillAttackType)
                {
                    case SkillAttackType.Normal:
                        // Assign damage data
                        damageInfo = skill.damageInfo;
                        // Calculate all damages
                        allDamageAmounts = weaponItem.GetDamageAmountWithInflictions(weapon.level, weapon.GetEquipmentBonusRate(), this, skill.GetWeaponDamageInflictions(level));
                        // Sum damage with additional damage amounts
                        allDamageAmounts = GameDataHelpers.CombineDamages(
                            allDamageAmounts,
                            skill.GetDamageAmount(level, this));
                        // Sum damage with skill damage
                        allDamageAmounts = GameDataHelpers.CombineDamages(
                            allDamageAmounts,
                            skill.GetAdditionalDamageAmounts(level));
                        break;
                    case SkillAttackType.BasedOnWeapon:
                        // Assign damage data
                        damageInfo = weaponType.damageInfo;
                        // Calculate all damages
                        allDamageAmounts = weaponItem.GetDamageAmountWithInflictions(weapon.level, weapon.GetEquipmentBonusRate(), this, skill.GetWeaponDamageInflictions(level));
                        // Sum damage with additional damage amounts
                        allDamageAmounts = GameDataHelpers.CombineDamages(
                            allDamageAmounts,
                            skill.GetAdditionalDamageAmounts(level));
                        break;
                }
                allDamageAmounts = GameDataHelpers.CombineDamages(
                    allDamageAmounts,
                    CacheIncreaseDamages);
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
        protected virtual void NetFuncUseSkill(int dataId, bool isLeftHand, bool hasAimPosition, Vector3 aimPosition)
        {
            if (!CanUseSkill())
                return;

            Skill skill;
            short level;
            if (!GameInstance.Skills.TryGetValue(dataId, out skill) ||
                !CacheSkills.TryGetValue(skill, out level))
                return;
            
            // Validate mp amount, skill level, 
            if (!skill.CanUse(this, level))
                return;

            // Prepare requires data
            AnimActionType animActionType;
            int skillOrWeaponTypeDataId;
            int animationIndex;
            CharacterItem weapon;
            float triggerDuration;
            float totalDuration;
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

            GetUsingSkillData(
                skill,
                level,
                isLeftHand,
                out animActionType,
                out skillOrWeaponTypeDataId,
                out animationIndex,
                out weapon,
                out triggerDuration,
                out totalDuration,
                out damageInfo,
                out allDamageAmounts);

            // Validate ammo
            if (skill.skillAttackType != SkillAttackType.None && !ValidateAmmo(weapon))
                return;

            // Call on cast skill to extend skill functionality while casting skills
            // Quit function when on cast skill will override default cast skill functionality
            if (skill.OnCastSkill(this, level, triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition))
                return;

            // Start use skill routine
            isAttackingOrUsingSkill = true;
            StartCoroutine(UseSkillRoutine(skill, level, animActionType, skillOrWeaponTypeDataId, animationIndex, triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition));
        }

        private IEnumerator UseSkillRoutine(
            Skill skill,
            short level,
            AnimActionType animActionType,
            int skillOrWeaponTypeDataId,
            int animationIndex,
            float triggerDuration,
            float totalDuration,
            bool isLeftHand,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            bool hasAimPosition,
            Vector3 aimPosition)
        {
            if (onUseSkillRoutine != null)
                onUseSkillRoutine.Invoke(skill, level, animActionType, skillOrWeaponTypeDataId, animationIndex, triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition);
            
            // Set doing action data
            isCastingSkillCanBeInterrupted = skill.canBeInterruptedWhileCasting;
            isCastingSkillInterrupted = false;

            float castDuration = skill.GetCastDuration(level);
            if (castDuration > 0f)
            {
                // Play casting effects on clients
                RequestPlayEffect(skill.castEffects.Id);

                // Tell clients that character is casting
                RequestSkillCasting(skill.DataId, castDuration);

                yield return new WaitForSecondsRealtime(castDuration);
            }

            // If skill casting not interrupted, continue doing action
            if (!isCastingSkillInterrupted || !isCastingSkillCanBeInterrupted)
            {
                // Play animation on clients
                RequestPlayActionAnimation(animActionType, skillOrWeaponTypeDataId, (byte)animationIndex);

                // Update skill usage states
                CharacterSkillUsage newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.Skill, skill.DataId);
                newSkillUsage.Use(this, level);
                skillUsages.Add(newSkillUsage);

                yield return new WaitForSecondsRealtime(triggerDuration);

                // Reduce ammo amount
                if (skill.skillAttackType != SkillAttackType.None)
                {
                    Dictionary<DamageElement, MinMaxFloat> increaseDamages;
                    ReduceAmmo(weapon, isLeftHand, out increaseDamages);
                    if (increaseDamages != null)
                        allDamageAmounts = GameDataHelpers.CombineDamages(allDamageAmounts, increaseDamages);
                }

                ApplySkill(skill, level, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition);
                yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
            }
            isAttackingOrUsingSkill = false;
        }

        /// <summary>
        /// This will be called at clients to play skill casting state with duration
        /// </summary>
        /// <param name="duration"></param>
        protected virtual void NetFuncSkillCasting(int dataId, float duration)
        {
            if (IsDead())
                return;
            castingSkillDuration = castingSkillCountDown = duration;
            StartCoroutine(SkillCastingRoutine(dataId, duration));
        }

        private IEnumerator SkillCastingRoutine(int dataId, float duration)
        {
            // Set doing action state at clients and server
            isAttackingOrUsingSkill = true;
            // Play casting animation
            if (CharacterModel != null)
                yield return CharacterModel.PlaySkillCastClip(dataId, duration);
        }

        /// <summary>
        /// This will be called at clients to stop playing skill casting
        /// </summary>
        protected virtual void NetFuncSkillCastingInterrupted()
        {
            isAttackingOrUsingSkill = false;
            castingSkillDuration = castingSkillCountDown = 0;
            if (CharacterModel != null)
                CharacterModel.StopActionAnimation();
        }

        protected virtual void ApplySkill(Skill skill, short level, bool isLeftHand, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, bool hasAimPosition, Vector3 aimPosition)
        {
            // Quit function when on apply skill will override default apply skill functionality
            if (skill.OnApplySkill(this, level, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition))
                return;

            switch (skill.skillType)
            {
                case SkillType.Active:
                    ApplySkillBuff(skill, level);
                    ApplySkillSummon(skill, level);
                    if (skill.skillAttackType != SkillAttackType.None)
                    {
                        CharacterBuff debuff = CharacterBuff.Empty;
                        if (skill.isDebuff)
                            debuff = CharacterBuff.Create(BuffType.SkillDebuff, skill.DataId, level);
                        // TODO: some skill type will not able to change aim position by controller
                        if (!hasAimPosition && HasAimPosition)
                        {
                            hasAimPosition = true;
                            aimPosition = AimPosition;
                        }
                        LaunchDamageEntity(isLeftHand, weapon, damageInfo, allDamageAmounts, debuff, skill.hitEffects.Id, hasAimPosition, aimPosition, Vector3.zero);
                    }
                    break;
            }
        }
    }
}
