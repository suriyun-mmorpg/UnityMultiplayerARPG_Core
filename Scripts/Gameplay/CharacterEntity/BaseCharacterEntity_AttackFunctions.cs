using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public event AttackRoutineDelegate onAttackRoutine;

        public virtual void GetAttackingData(
            out AnimActionType animActionType,
            out int dataId,
            out int animationIndex,
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
            isLeftHand = false;
            weapon = null;
            triggerDuration = 0f;
            totalDuration = 0f;
            damageInfo = null;
            allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Prepare weapon data
            weapon = this.GetRandomedWeapon(out isLeftHand);
            Item weaponItem = weapon.GetWeaponItem();
            WeaponType weaponType = weaponItem.WeaponType;
            // Assign data id
            dataId = weaponType.DataId;
            // Assign animation action type
            animActionType = !isLeftHand ? AnimActionType.AttackRightHand : AnimActionType.AttackLeftHand;
            // Random animation
            if (!isLeftHand)
                CharacterModel.GetRandomRightHandAttackAnimation(dataId, out animationIndex, out triggerDuration, out totalDuration);
            else
                CharacterModel.GetRandomLeftHandAttackAnimation(dataId, out animationIndex, out triggerDuration, out totalDuration);
            // Assign damage data
            damageInfo = weaponType.damageInfo;
            // Calculate all damages
            allDamageAmounts = GameDataHelpers.CombineDamages(
                allDamageAmounts,
                weaponItem.GetDamageAmount(weapon.level, weapon.GetEquipmentBonusRate(), this));
            allDamageAmounts = GameDataHelpers.CombineDamages(
                allDamageAmounts,
                CacheIncreaseDamages);
        }

        /// <summary>
        /// Is function will be called at server to order character to attack
        /// </summary>
        protected virtual void NetFuncAttack(bool hasAimPosition, Vector3 aimPosition)
        {
            if (!CanAttack())
                return;

            // Prepare requires data
            AnimActionType animActionType;
            int weaponTypeDataId;
            int animationIndex;
            bool isLeftHand;
            CharacterItem weapon;
            float triggerDuration;
            float totalDuration;
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

            GetAttackingData(
                out animActionType,
                out weaponTypeDataId,
                out animationIndex,
                out isLeftHand,
                out weapon,
                out triggerDuration,
                out totalDuration,
                out damageInfo,
                out allDamageAmounts);

            // Reduce ammo amount
            if (weapon != null)
            {
                WeaponType weaponType = weapon.GetWeaponItem().WeaponType;
                if (weaponType.requireAmmoType != null)
                {
                    Dictionary<CharacterItem, short> decreaseAmmoItems;
                    if (!this.DecreaseAmmos(weaponType.requireAmmoType, 1, out decreaseAmmoItems))
                        return;
                    KeyValuePair<CharacterItem, short> firstEntry = decreaseAmmoItems.FirstOrDefault();
                    CharacterItem ammoCharacterItem = firstEntry.Key;
                    Item ammoItem = ammoCharacterItem.GetItem();
                    if (ammoItem != null && firstEntry.Value > 0)
                        allDamageAmounts = GameDataHelpers.CombineDamages(allDamageAmounts, ammoItem.GetIncreaseDamages(ammoCharacterItem.level, ammoCharacterItem.GetEquipmentBonusRate()));
                }
            }

            // Call on attack to extend attack functionality while attacking
            bool overrideDefaultAttack = false;
            foreach (CharacterSkill characterSkill in Skills)
            {
                if (characterSkill.level > 0)
                {
                    if (characterSkill.GetSkill().OnAttack(this, characterSkill.level, triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition))
                        overrideDefaultAttack = true;
                }
            }

            // Quit function when on attack will override default attack functionality
            if (overrideDefaultAttack)
                return;

            // Start attack routine
            isAttackingOrUsingSkill = true;
            StartCoroutine(AttackRoutine(animActionType, weaponTypeDataId, animationIndex, triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition));
        }

        private IEnumerator AttackRoutine(
            AnimActionType animActionType,
            int weaponTypeDataId,
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
            if (onAttackRoutine != null)
                onAttackRoutine.Invoke(animActionType, weaponTypeDataId, animationIndex, triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition);

            // Play animation on clients
            RequestPlayActionAnimation(animActionType, weaponTypeDataId, (byte)animationIndex);

            yield return new WaitForSecondsRealtime(triggerDuration);
            LaunchDamageEntity(isLeftHand, weapon, damageInfo, allDamageAmounts, CharacterBuff.Empty, 0, hasAimPosition, aimPosition);
            yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
            isAttackingOrUsingSkill = false;
        }
    }
}
