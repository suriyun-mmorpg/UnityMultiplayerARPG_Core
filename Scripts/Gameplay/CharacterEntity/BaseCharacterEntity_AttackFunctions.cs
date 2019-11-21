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
            ref bool isLeftHand,
            out AnimActionType animActionType,
            out int animationDataId,
            out CharacterItem weapon)
        {
            // Initialize data
            animActionType = AnimActionType.None;
            animationDataId = 0;
            weapon = this.GetAvailableWeapon(ref isLeftHand);
            // Assign data id
            animationDataId = weapon.GetWeaponItem().WeaponType.DataId;
            // Assign animation action type
            animActionType = !isLeftHand ? AnimActionType.AttackRightHand : AnimActionType.AttackLeftHand;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetWeaponDamageAmounts(ref bool isLeftHand)
        {
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Calculate all damages
            damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, this.GetWeaponDamage(ref isLeftHand));
            // Sum damage with buffs
            damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, this.GetCaches().IncreaseDamages);

            return damageAmounts;
        }

        public bool ValidateAmmo(CharacterItem weapon)
        {
            // Avoid null data
            if (weapon == null)
                return true;

            Item weaponItem = weapon.GetWeaponItem();
            if (weaponItem.WeaponType.requireAmmoType != null)
            {
                if (weaponItem.ammoCapacity <= 0)
                {
                    // Ammo capacity is 0 so reduce ammo from inventory
                    if (this.CountAmmos(weaponItem.WeaponType.requireAmmoType) == 0)
                        return false;
                }
                else
                {
                    // Ammo capacity more than 0 reduce loaded ammo
                    if (weapon.ammo <= 0)
                        return false;
                }
            }
            return true;
        }

        public void ReduceAmmo(CharacterItem weapon, bool isLeftHand, out Dictionary<DamageElement, MinMaxFloat> increaseDamges)
        {
            increaseDamges = null;
            // Avoid null data
            if (weapon == null)
                return;

            Item weaponItem = weapon.GetWeaponItem();
            if (weaponItem.ammoCapacity <= 0)
            {
                // Ammo capacity is 0 so reduce ammo from inventory
                Dictionary<CharacterItem, short> decreaseAmmoItems;
                if (this.DecreaseAmmos(weaponItem.WeaponType.requireAmmoType, 1, out decreaseAmmoItems))
                {
                    KeyValuePair<CharacterItem, short> firstEntry = decreaseAmmoItems.FirstOrDefault();
                    CharacterItem ammoCharacterItem = firstEntry.Key;
                    Item ammoItem = ammoCharacterItem.GetItem();
                    if (ammoItem != null && firstEntry.Value > 0)
                    {
                        // Ammo level always 1
                        increaseDamges = ammoItem.GetIncreaseDamages(1);
                    }
                }
            }
            else
            {
                // Ammo capacity more than 0 reduce loaded ammo
                if (weapon.ammo > 0)
                {
                    weapon.ammo--;
                    EquipWeapons equipWeapons = EquipWeapons;
                    if (isLeftHand)
                        equipWeapons.leftHand = weapon;
                    else
                        equipWeapons.rightHand = weapon;
                    EquipWeapons = equipWeapons;
                }
            }
        }

        protected void NetFuncReload(bool isLeftHand)
        {
            if (!CanAttack())
                return;
            
            CharacterItem reloadingWeapon = isLeftHand ? EquipWeapons.leftHand : EquipWeapons.rightHand;

            if (reloadingWeapon.IsEmptySlot())
                return;

            Item reloadingWeaponItem = reloadingWeapon.GetWeaponItem();
            if (reloadingWeaponItem == null ||
                reloadingWeaponItem.WeaponType == null ||
                reloadingWeaponItem.WeaponType.requireAmmoType == null ||
                reloadingWeaponItem.ammoCapacity <= 0 ||
                reloadingWeapon.ammo >= reloadingWeaponItem.ammoCapacity)
                return;

            // Prepare reload data
            reloadingAmmoAmount = (short)(reloadingWeaponItem.ammoCapacity - reloadingWeapon.ammo);
            int inventoryAmount = this.CountAmmos(reloadingWeaponItem.WeaponType.requireAmmoType);
            if (inventoryAmount < reloadingAmmoAmount)
                reloadingAmmoAmount = (short)inventoryAmount;

            if (reloadingAmmoAmount <= 0)
                return;

            // Start reload routine
            isAttackingOrUsingSkill = true;

            // Play animations
            RequestPlayReloadAnimation(isLeftHand);
        }
    
        protected IEnumerator ReloadRoutine(bool isLeftHand)
        {
            animActionType = isLeftHand ? AnimActionType.ReloadLeftHand : AnimActionType.ReloadRightHand;
            CharacterItem reloadingWeapon = isLeftHand ? EquipWeapons.leftHand : EquipWeapons.rightHand;
            Item reloadingWeaponItem = reloadingWeapon.GetWeaponItem();

            // Prepare requires data and get animation data
            float[] triggerDurations;
            float totalDuration;
            if (!isLeftHand)
                CharacterModel.GetRightHandReloadAnimation(reloadingWeaponItem.WeaponType.DataId, out triggerDurations, out totalDuration);
            else
                CharacterModel.GetLeftHandReloadAnimation(reloadingWeaponItem.WeaponType.DataId, out triggerDurations, out totalDuration);

            // Set doing action state at clients and server
            isAttackingOrUsingSkill = true;

            // Calculate move speed rate while doing action at clients and server
            moveSpeedRateWhileAttackOrUseSkill = GetMoveSpeedRateWhileAttackOrUseSkill(animActionType, null);

            // Animations will plays on clients only
            if (IsClient)
            {
                // Play animation
                CharacterModel.PlayActionAnimation(animActionType, reloadingWeaponItem.WeaponType.DataId, 0);
            }

            for (int i = 0; i < triggerDurations.Length; ++i)
            {
                // Wait until triggger before reload ammo
                yield return new WaitForSecondsRealtime(triggerDurations[i]);

                // Prepare data
                EquipWeapons equipWeapons = EquipWeapons;
                Dictionary<CharacterItem, short> decreaseItems;
                if (IsServer && this.DecreaseAmmos(reloadingWeaponItem.WeaponType.requireAmmoType, reloadingAmmoAmount, out decreaseItems))
                {
                    reloadingWeapon.ammo += reloadingAmmoAmount;
                    if (isLeftHand)
                        equipWeapons.leftHand = reloadingWeapon;
                    else
                        equipWeapons.rightHand = reloadingWeapon;
                    EquipWeapons = equipWeapons;
                }
                yield return new WaitForSecondsRealtime(totalDuration - triggerDurations[i]);
            }
            animActionType = AnimActionType.None;
            isAttackingOrUsingSkill = false;
        }

        /// <summary>
        /// Is function will be called at server to order character to attack
        /// </summary>
        protected virtual void NetFuncAttack(bool isLeftHand, Vector3 aimPosition)
        {
            if (!CanAttack())
                return;

            // Prepare requires data and get weapon data
            AnimActionType animActionType;
            int animationDataId;
            CharacterItem weapon;
            GetAttackingData(
                ref isLeftHand,
                out animActionType,
                out animationDataId,
                out weapon);

            // Prepare requires data and get animation data
            int animationIndex;
            float[] triggerDurations;
            float totalDuration;
            GetRandomAnimationData(
                animActionType,
                animationDataId,
                out animationIndex,
                out triggerDurations,
                out totalDuration);

            // Validate ammo
            if (!ValidateAmmo(weapon))
                return;
            
            // Start attack routine
            isAttackingOrUsingSkill = true;

            // Play animations
            RequestPlayAttackAnimation(isLeftHand, (byte)animationIndex, aimPosition);
        }

        protected IEnumerator AttackRoutine(bool isLeftHand, byte animationIndex, Vector3 aimPosition)
        {
            // Prepare requires data and get weapon data
            int animationDataId;
            CharacterItem weapon;
            GetAttackingData(
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
            DamageInfo damageInfo = this.GetWeaponDamageInfo(ref isLeftHand);
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = GetWeaponDamageAmounts(ref isLeftHand);

            // Set doing action state at clients and server
            isAttackingOrUsingSkill = true;

            // Calculate move speed rate while doing action at clients and server
            moveSpeedRateWhileAttackOrUseSkill = GetMoveSpeedRateWhileAttackOrUseSkill(animActionType, null);

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            float playSpeedMultiplier = GetAnimSpeedRate(animActionType);

            // Animations will plays on clients only
            if (IsClient)
            {
                // Play animation
                CharacterModel.PlayActionAnimation(animActionType, animationDataId, animationIndex, playSpeedMultiplier);
            }

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
                {
                    // Play weapon launch special effects
                    CharacterModel.PlayWeaponLaunchEffect(animActionType);
                }

                // Call on attack to extend attack functionality while attacking
                bool overrideDefaultAttack = false;
                foreach (CharacterSkill characterSkill in Skills)
                {
                    if (characterSkill.level <= 0)
                        continue;
                    if (characterSkill.GetSkill().OnAttack(this, characterSkill.level, isLeftHand, weapon, hitIndex, damageAmounts, aimPosition))
                        overrideDefaultAttack = true;
                }

                // Skip attack function when applied skills (buffs) will override default attack functionality
                if (!overrideDefaultAttack)
                {
                    // Trigger attack event
                    if (onAttackRoutine != null)
                        onAttackRoutine.Invoke(isLeftHand, weapon, hitIndex, damageAmounts, aimPosition);

                    // Apply attack damages
                    ApplyAttack(
                        isLeftHand,
                        weapon,
                        damageInfo,
                        damageAmounts,
                        aimPosition);
                }
            }

            if (remainsDuration > 0f)
            {
                // Wait until animation ends to stop actions
                yield return new WaitForSecondsRealtime(remainsDuration / playSpeedMultiplier);
            }

            // Set doing action state to none at clients and server
            animActionType = AnimActionType.None;
            isAttackingOrUsingSkill = false;
        }

        protected virtual void ApplyAttack(bool isLeftHand, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, Vector3 aimPosition)
        {
            // Increase damage with ammo damage
            if (IsServer)
            {
                Dictionary<DamageElement, MinMaxFloat> increaseDamages;
                ReduceAmmo(weapon, isLeftHand, out increaseDamages);
                if (increaseDamages != null)
                    damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, increaseDamages);
            }

            byte fireSpread = 0;
            Vector3 fireStagger = Vector3.zero;
            if (weapon != null && weapon.GetWeaponItem() != null)
            {
                // For monsters, their weapon can be null so have to avoid null exception
                fireSpread = weapon.GetWeaponItem().fireSpread;
                fireStagger = weapon.GetWeaponItem().fireStagger;
            }

            Vector3 stagger;
            for (int i = 0; i < fireSpread + 1; ++i)
            {
                stagger = new Vector3(Random.Range(-fireStagger.x, fireStagger.x), Random.Range(-fireStagger.y, fireStagger.y));
                LaunchDamageEntity(
                    isLeftHand,
                    weapon,
                    damageInfo,
                    damageAmounts,
                    null,
                    0,
                    aimPosition,
                    stagger);
            }
        }
    }
}
