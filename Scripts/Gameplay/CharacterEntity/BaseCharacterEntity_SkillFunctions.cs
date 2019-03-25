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
            CharacterSkill characterSkill,
            out AnimActionType animActionType,
            out int dataId,
            out int animationIndex,
            out SkillAttackType skillAttackType,
            out bool isLeftHand,
            out CharacterItem weapon,
            out float triggerDuration,
            out float totalDuration,
            out DamageInfo damageInfo,
            out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            // Initialize data
            animActionType = AnimActionType.None;
            dataId = 0;
            animationIndex = 0;
            skillAttackType = SkillAttackType.None;
            isLeftHand = false;
            weapon = null;
            triggerDuration = 0f;
            totalDuration = 0f;
            damageInfo = null;
            allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Prepare skill data
            Skill skill = characterSkill.GetSkill();
            if (skill == null)
                return;
            // Prepare weapon data
            skillAttackType = skill.skillAttackType;
            weapon = this.GetRandomedWeapon(out isLeftHand);
            Item weaponItem = weapon.GetWeaponItem();
            WeaponType weaponType = weaponItem.WeaponType;
            bool hasSkillAnimation = CharacterModel.HasSkillAnimations(skill);
            // Prepare animation
            if (!hasSkillAnimation && skillAttackType != SkillAttackType.None)
            {
                // If there is no cast animations
                // Assign data id
                dataId = weaponType.DataId;
                // Assign animation action type
                animActionType = !isLeftHand ? AnimActionType.AttackRightHand : AnimActionType.AttackLeftHand;
                // Random animation
                if (!isLeftHand)
                    CharacterModel.GetRandomRightHandAttackAnimation(dataId, out animationIndex, out triggerDuration, out totalDuration);
                else
                    CharacterModel.GetRandomLeftHandAttackAnimation(dataId, out animationIndex, out triggerDuration, out totalDuration);
            }
            else if (hasSkillAnimation)
            {
                // Assign data id
                dataId = skill.DataId;
                // Assign animation action type
                animActionType = AnimActionType.Skill;
                // Random animation
                CharacterModel.GetSkillActivateAnimation(dataId, out triggerDuration, out totalDuration);
            }
            // If it is attack skill
            if (skillAttackType != SkillAttackType.None)
            {
                switch (skillAttackType)
                {
                    case SkillAttackType.Normal:
                        // Assign damage data
                        damageInfo = skill.damageInfo;
                        // Calculate all damages
                        allDamageAmounts = weaponItem.GetDamageAmountWithInflictions(weapon.level, weapon.GetEquipmentBonusRate(), this, skill.GetWeaponDamageInflictions(characterSkill.level));
                        // Sum damage with additional damage amounts
                        allDamageAmounts = GameDataHelpers.CombineDamages(
                            allDamageAmounts,
                            skill.GetDamageAmount(characterSkill.level, this));
                        // Sum damage with skill damage
                        allDamageAmounts = GameDataHelpers.CombineDamages(
                            allDamageAmounts,
                            skill.GetAdditionalDamageAmounts(characterSkill.level));
                        break;
                    case SkillAttackType.BasedOnWeapon:
                        // Assign damage data
                        damageInfo = weaponType.damageInfo;
                        // Calculate all damages
                        allDamageAmounts = weaponItem.GetDamageAmountWithInflictions(weapon.level, weapon.GetEquipmentBonusRate(), this, skill.GetWeaponDamageInflictions(characterSkill.level));
                        // Sum damage with additional damage amounts
                        allDamageAmounts = GameDataHelpers.CombineDamages(
                            allDamageAmounts,
                            skill.GetAdditionalDamageAmounts(characterSkill.level));
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
        protected virtual void NetFuncUseSkill(int skillOrWeaponTypeDataId, bool hasAimPosition, Vector3 aimPosition)
        {
            if (!CanUseSkill())
                return;

            int index = this.IndexOfSkill(skillOrWeaponTypeDataId);
            if (index < 0)
                return;

            CharacterSkill characterSkill = skills[index];
            if (!characterSkill.CanUse(this))
                return;

            // Prepare requires data
            AnimActionType animActionType;
            int animationIndex;
            SkillAttackType skillAttackType;
            bool isLeftHand;
            CharacterItem weapon;
            float triggerDuration;
            float totalDuration;
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

            GetUsingSkillData(
                characterSkill,
                out animActionType,
                out skillOrWeaponTypeDataId,
                out animationIndex,
                out skillAttackType,
                out isLeftHand,
                out weapon,
                out triggerDuration,
                out totalDuration,
                out damageInfo,
                out allDamageAmounts);

            if (weapon != null)
            {
                WeaponType weaponType = weapon.GetWeaponItem().WeaponType;
                // Reduce ammo amount
                if (skillAttackType != SkillAttackType.None && weaponType.requireAmmoType != null)
                {
                    Dictionary<CharacterItem, short> decreaseItems;
                    if (!this.DecreaseAmmos(weaponType.requireAmmoType, 1, out decreaseItems))
                        return;
                    KeyValuePair<CharacterItem, short> firstEntry = decreaseItems.FirstOrDefault();
                    CharacterItem characterItem = firstEntry.Key;
                    Item item = characterItem.GetItem();
                    if (item != null && firstEntry.Value > 0)
                        allDamageAmounts = GameDataHelpers.CombineDamages(allDamageAmounts, item.GetIncreaseDamages(characterItem.level, characterItem.GetEquipmentBonusRate()));
                }
            }

            Skill skill = characterSkill.GetSkill();

            // Call on cast skill to extend skill functionality while casting skills
            // Quit function when on cast skill will override default cast skill functionality
            if (skill.OnCastSkill(this, characterSkill.level, triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition))
                return;

            // Start use skill routine
            isAttackingOrUsingSkill = true;
            StartCoroutine(UseSkillRoutine(animActionType, skillOrWeaponTypeDataId, animationIndex, characterSkill, triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition));
        }

        private IEnumerator UseSkillRoutine(
            AnimActionType animActionType,
            int skillOrWeaponTypeDataId,
            int animationIndex,
            CharacterSkill characterSkill,
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
                onUseSkillRoutine.Invoke(animActionType, skillOrWeaponTypeDataId, animationIndex, characterSkill, triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition);

            Skill skill = characterSkill.GetSkill();
            float castDuration = skill.GetCastDuration(characterSkill.level);

            // Set doing action data
            isCastingSkillCanBeInterrupted = skill.canBeInterruptedWhileCasting;
            isCastingSkillInterrupted = false;

            if (castDuration > 0f)
            {
                // Play casting effects on clients
                RequestPlayEffect(skill.castEffects.Id);

                // Tell clients that character is casting
                RequestSkillCasting(characterSkill.dataId, castDuration);

                yield return new WaitForSecondsRealtime(castDuration);
            }

            // If skill casting not interrupted, continue doing action
            if (!isCastingSkillInterrupted || !isCastingSkillCanBeInterrupted)
            {
                // Play animation on clients
                RequestPlayActionAnimation(animActionType, skillOrWeaponTypeDataId, (byte)animationIndex);

                // Update skill usage states
                CharacterSkillUsage newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.Skill, characterSkill.dataId);
                newSkillUsage.Use(this, characterSkill.level);
                skillUsages.Add(newSkillUsage);

                yield return new WaitForSecondsRealtime(triggerDuration);
                ApplySkill(characterSkill, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition);
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

        protected virtual void ApplySkill(CharacterSkill characterSkill, bool isLeftHand, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, bool hasAimPosition, Vector3 aimPosition)
        {
            Skill skill = characterSkill.GetSkill();

            // Quit function when on apply skill will override default apply skill functionality
            if (skill.OnApplySkill(this, characterSkill.level, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition))
                return;

            switch (skill.skillType)
            {
                case SkillType.Active:
                    ApplySkillBuff(skill, characterSkill.level);
                    ApplySkillSummon(skill, characterSkill.level);
                    if (skill.skillAttackType != SkillAttackType.None)
                    {
                        CharacterBuff debuff = CharacterBuff.Empty;
                        if (skill.isDebuff)
                            debuff = CharacterBuff.Create(BuffType.SkillDebuff, skill.DataId, characterSkill.level);
                        LaunchDamageEntity(isLeftHand, weapon, damageInfo, allDamageAmounts, debuff, skill.hitEffects.Id, hasAimPosition, aimPosition);
                    }
                    break;
            }
        }
    }
}
