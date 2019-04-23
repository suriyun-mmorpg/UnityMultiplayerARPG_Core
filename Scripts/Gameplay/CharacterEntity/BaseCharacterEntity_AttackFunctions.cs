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
            bool isLeftHand,
            out AnimActionType animActionType,
            out int dataId,
            out int animationIndex,
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
            weapon = this.GetAvailableWeapon(isLeftHand);
            triggerDuration = 0f;
            totalDuration = 0f;
            damageInfo = null;
            allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Prepare weapon data
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

        protected virtual void NetFuncReload(bool isLeftHand)
        {
            if (!CanAttack())
                return;

            CharacterItem weapon;

            if (isLeftHand)
                weapon = EquipWeapons.leftHand;
            else
                weapon = EquipWeapons.rightHand;

            if (weapon.IsEmpty())
                return;

            Item weaponItem = weapon.GetWeaponItem();

            if (weaponItem != null &&
                weaponItem.WeaponType != null &&
                weaponItem.WeaponType.requireAmmoType != null &&
                weaponItem.WeaponType.ammoCapacity > 0 &&
                weapon.ammo < weaponItem.WeaponType.ammoCapacity)
            {
                // Prepare reload data
                AnimActionType animActionType = isLeftHand ? AnimActionType.ReloadLeftHand : AnimActionType.ReloadRightHand;
                int weaponTypeDataId = weaponItem.WeaponType.DataId;
                float triggerDuration = 0f;
                float totalDuration = 0f;
                if (!isLeftHand)
                    CharacterModel.GetRightHandReloadAnimation(weaponTypeDataId, out triggerDuration, out totalDuration);
                else
                    CharacterModel.GetLeftHandReloadAnimation(weaponTypeDataId, out triggerDuration, out totalDuration);

                int reloadingAmount = weaponItem.WeaponType.ammoCapacity - weapon.ammo;
                int inventoryAmount = this.CountAmmos(weaponItem.WeaponType.requireAmmoType);
                if (inventoryAmount < reloadingAmount)
                    reloadingAmount = inventoryAmount;

                if (reloadingAmount > 0)
                {
                    // Start reload routine
                    isAttackingOrUsingSkill = true;
                    StartCoroutine(ReloadRoutine(animActionType, weaponTypeDataId, triggerDuration, totalDuration, isLeftHand, weapon, (short)reloadingAmount));
                }
            }
        }
        
        private IEnumerator ReloadRoutine(
            AnimActionType animActionType,
            int weaponTypeDataId,
            float triggerDuration,
            float totalDuration,
            bool isLeftHand,
            CharacterItem weapon,
            short reloadingAmount)
        {
            // Play animation on clients
            RequestPlayActionAnimation(animActionType, weaponTypeDataId, 0);

            yield return new WaitForSecondsRealtime(triggerDuration);

            // Prepare data
            EquipWeapons equipWeapons = EquipWeapons;
            Dictionary<CharacterItem, short> decreaseItems;
            if (this.DecreaseAmmos(weapon.GetWeaponItem().WeaponType.requireAmmoType, reloadingAmount, out decreaseItems))
            {
                weapon.ammo += reloadingAmount;
                if (isLeftHand)
                    equipWeapons.leftHand = weapon;
                else
                    equipWeapons.rightHand = weapon;
                EquipWeapons = equipWeapons;
            }

            yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
            isAttackingOrUsingSkill = false;
        }

        /// <summary>
        /// Is function will be called at server to order character to attack
        /// </summary>
        protected virtual void NetFuncAttack(bool isLeftHand, bool hasAimPosition, Vector3 aimPosition)
        {
            if (!CanAttack())
                return;

            // Prepare requires data
            AnimActionType animActionType;
            int weaponTypeDataId;
            int animationIndex;
            CharacterItem weapon;
            float triggerDuration;
            float totalDuration;
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

            GetAttackingData(
                isLeftHand,
                out animActionType,
                out weaponTypeDataId,
                out animationIndex,
                out weapon,
                out triggerDuration,
                out totalDuration,
                out damageInfo,
                out allDamageAmounts);

            // Validate ammo
            if (weapon != null)
            {
                // For monsters, their weapon can be null so have to avoid null exception
                Item weaponItem = weapon.GetWeaponItem();
                WeaponType weaponType = weaponItem.WeaponType;
                if (weaponType.requireAmmoType != null)
                {
                    if (weaponType.ammoCapacity <= 0)
                    {
                        // Ammo capacity is 0 so reduce ammo from inventory
                        if (this.CountAmmos(weaponType.requireAmmoType) == 0)
                        {
                            // TODO: send no ammo message
                            return;
                        }
                    }
                    else
                    {
                        // Ammo capacity more than 0 reduce loaded ammo
                        if (weapon.ammo <= 0)
                        {
                            // TODO: send no ammo message
                            return;
                        }
                    }
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
                onAttackRoutine.Invoke(animActionType, weaponTypeDataId, animationIndex, triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts);

            // Play animation on clients
            RequestPlayActionAnimation(animActionType, weaponTypeDataId, (byte)animationIndex);

            yield return new WaitForSecondsRealtime(triggerDuration);

            // Reduce ammo amount
            if (weapon != null)
            {
                // For monsters, their weapon can be null so have to avoid null exception
                Item weaponItem = weapon.GetWeaponItem();
                WeaponType weaponType = weaponItem.WeaponType;
                if (weaponType.requireAmmoType != null)
                {
                    if (weaponType.ammoCapacity <= 0)
                    {
                        // Ammo capacity is 0 so reduce ammo from inventory
                        Dictionary<CharacterItem, short> decreaseAmmoItems;
                        if (this.DecreaseAmmos(weaponType.requireAmmoType, 1, out decreaseAmmoItems))
                        {
                            KeyValuePair<CharacterItem, short> firstEntry = decreaseAmmoItems.FirstOrDefault();
                            CharacterItem ammoCharacterItem = firstEntry.Key;
                            Item ammoItem = ammoCharacterItem.GetItem();
                            if (ammoItem != null && firstEntry.Value > 0)
                                allDamageAmounts = GameDataHelpers.CombineDamages(allDamageAmounts, ammoItem.GetIncreaseDamages(ammoCharacterItem.level, ammoCharacterItem.GetEquipmentBonusRate()));
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
            }

            // If no aim position set with attack function get aim position which set from client-controller if existed
            if (!hasAimPosition && HasAimPosition)
            {
                hasAimPosition = true;
                aimPosition = AimPosition;
            }

            byte fireSpread = 0;
            Vector3 fireStagger = Vector3.zero;
            if (weapon != null && weapon.GetWeaponItem() != null)
            {
                // For monsters, their weapon can be null so have to avoid null exception
                fireSpread = weapon.GetWeaponItem().WeaponType.fireSpread;
                fireStagger = weapon.GetWeaponItem().WeaponType.fireStagger;
            }

            Vector3 stagger;
            for (int i = 0; i < fireSpread + 1; ++i)
            {
                stagger = new Vector3(Random.Range(-fireStagger.x, fireStagger.x), Random.Range(-fireStagger.y, fireStagger.y));
                LaunchDamageEntity(
                    isLeftHand,
                    weapon,
                    damageInfo,
                    allDamageAmounts,
                    CharacterBuff.Empty,
                    0,
                    hasAimPosition,
                    aimPosition,
                    stagger);
            }
            yield return new WaitForSecondsRealtime(totalDuration - triggerDuration);
            isAttackingOrUsingSkill = false;
        }
    }
}
