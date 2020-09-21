using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        protected readonly List<CancellationTokenSource> attackCancellationTokenSources = new List<CancellationTokenSource>();
        protected readonly List<CancellationTokenSource> reloadCancellationTokenSources = new List<CancellationTokenSource>();
        /// <summary>
        /// This will be `TRUE` if it's allowing to change aim position instead of using default aim position (character's forward)
        /// So it should be `TRUE` while player's controller is shooter controller
        /// </summary>
        public virtual bool HasAimPosition { get; set; }

        /// <summary>
        /// This will be used if `HasAimPosition` is `TRUE` to change default aim position to this value
        /// </summary>
        public virtual Vector3 AimPosition { get; set; }

        public Vector3 GetDefaultAttackAimPosition(bool isLeftHand)
        {
            return GetDefaultAttackAimPosition(this.GetWeaponDamageInfo(ref isLeftHand), isLeftHand);
        }

        public Vector3 GetDefaultAttackAimPosition(DamageInfo damageInfo, bool isLeftHand)
        {
            // No aim position set, set aim position to forward direction
            BaseGameEntity targetEntity = GetTargetEntity();
            if (targetEntity)
            {
                if (targetEntity is DamageableEntity)
                {
                    return (targetEntity as DamageableEntity).OpponentAimTransform.position;
                }
                else
                {
                    return targetEntity.CacheTransform.position;
                }
            }
            return damageInfo.GetDamageTransform(this, isLeftHand).position + CacheTransform.forward;
        }

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

        public Dictionary<DamageElement, MinMaxFloat> GetWeaponDamages(ref bool isLeftHand)
        {
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Calculate all damages
            damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, this.GetWeaponDamage(ref isLeftHand));
            // Sum damage with buffs
            damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, this.GetCaches().IncreaseDamages);

            return damageAmounts;
        }

        public bool ValidateAmmo(CharacterItem weapon, short amount = 1)
        {
            // Avoid null data
            if (weapon == null)
                return true;

            IWeaponItem weaponItem = weapon.GetWeaponItem();
            if (weaponItem.WeaponType.RequireAmmoType != null)
            {
                if (weaponItem.AmmoCapacity <= 0)
                {
                    // Ammo capacity is 0 so reduce ammo from inventory
                    if (this.CountAmmos(weaponItem.WeaponType.RequireAmmoType) < amount)
                        return false;
                }
                else
                {
                    // Ammo capacity more than 0 reduce loaded ammo
                    if (weapon.ammo < amount)
                        return false;
                }
            }
            return true;
        }

        public void ReduceAmmo(CharacterItem weapon, bool isLeftHand, out Dictionary<DamageElement, MinMaxFloat> increaseDamges, short amount = 1)
        {
            increaseDamges = null;
            // Avoid null data
            if (weapon == null)
                return;

            IWeaponItem weaponItem = weapon.GetWeaponItem();
            if (weaponItem.AmmoCapacity <= 0)
            {
                // Ammo capacity is 0 so reduce ammo from inventory
                Dictionary<CharacterItem, short> decreaseAmmoItems;
                if (this.DecreaseAmmos(weaponItem.WeaponType.RequireAmmoType, amount, out decreaseAmmoItems))
                {
                    this.FillEmptySlots();
                    CharacterItem ammoCharacterItem = decreaseAmmoItems.FirstOrDefault().Key;
                    IAmmoItem ammoItem = ammoCharacterItem.GetAmmoItem();
                    if (ammoItem != null)
                        increaseDamges = ammoItem.GetIncreaseDamages(ammoCharacterItem.level);
                }
            }
            else
            {
                // Ammo capacity >= `amount` reduce loaded ammo
                if (weapon.ammo >= amount)
                {
                    weapon.ammo -= amount;
                    EquipWeapons equipWeapons = EquipWeapons;
                    if (isLeftHand)
                        equipWeapons.leftHand = weapon;
                    else
                        equipWeapons.rightHand = weapon;
                    EquipWeapons = equipWeapons;
                }
            }
        }

        protected virtual void ServerReload(bool isLeftHand)
        {
            if (!CanAttack())
                return;

            CharacterItem reloadingWeapon = isLeftHand ? EquipWeapons.leftHand : EquipWeapons.rightHand;

            if (reloadingWeapon.IsEmptySlot())
                return;

            IWeaponItem reloadingWeaponItem = reloadingWeapon.GetWeaponItem();
            if (reloadingWeaponItem == null ||
                reloadingWeaponItem.WeaponType == null ||
                reloadingWeaponItem.WeaponType.RequireAmmoType == null ||
                reloadingWeaponItem.AmmoCapacity <= 0 ||
                reloadingWeapon.ammo >= reloadingWeaponItem.AmmoCapacity)
                return;

            // Prepare reload data
            short reloadingAmmoAmount = (short)(reloadingWeaponItem.AmmoCapacity - reloadingWeapon.ammo);
            int inventoryAmount = this.CountAmmos(reloadingWeaponItem.WeaponType.RequireAmmoType);
            if (inventoryAmount < reloadingAmmoAmount)
                reloadingAmmoAmount = (short)inventoryAmount;

            if (reloadingAmmoAmount <= 0)
                return;

            // Start reload routine
            IsAttackingOrUsingSkill = true;

            // Play animations
            RequestPlayReloadAnimation(isLeftHand, reloadingAmmoAmount);
        }

        protected async UniTaskVoid ReloadRoutine(bool isLeftHand, short reloadingAmmoAmount)
        {
            // Set doing action state at clients and server
            SetReloadActionStates(isLeftHand ? AnimActionType.ReloadLeftHand : AnimActionType.ReloadRightHand, reloadingAmmoAmount);
            CharacterItem reloadingWeapon = isLeftHand ? EquipWeapons.leftHand : EquipWeapons.rightHand;
            IWeaponItem reloadingWeaponItem = reloadingWeapon.GetWeaponItem();

            // Prepare requires data and get animation data
            float animSpeedRate;
            float[] triggerDurations;
            float totalDuration;
            if (!isLeftHand)
                CharacterModel.GetRightHandReloadAnimation(reloadingWeaponItem.WeaponType.DataId, out animSpeedRate, out triggerDurations, out totalDuration);
            else
                CharacterModel.GetLeftHandReloadAnimation(reloadingWeaponItem.WeaponType.DataId, out animSpeedRate, out triggerDurations, out totalDuration);

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileAttackOrUseSkill = GetMoveSpeedRateWhileAttackOrUseSkill(AnimActionType, null);

            // Prepare cancellation
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            reloadCancellationTokenSources.Add(cancellationTokenSource);
            try
            {
                // Animations will plays on clients only
                if (IsClient)
                {
                    // Play animation
                    CharacterModel.PlayActionAnimation(AnimActionType, reloadingWeaponItem.WeaponType.DataId, 0);
                    if (FpsModel && FpsModel.gameObject.activeSelf)
                        FpsModel.PlayActionAnimation(AnimActionType, reloadingWeaponItem.WeaponType.DataId, 0);
                }

                for (int i = 0; i < triggerDurations.Length; ++i)
                {
                    // Wait until triggger before reload ammo
                    await UniTask.Delay((int)(triggerDurations[i] / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, cancellationTokenSource.Token);

                    // Prepare data
                    EquipWeapons equipWeapons = EquipWeapons;
                    Dictionary<CharacterItem, short> decreaseItems;
                    if (IsServer && this.DecreaseAmmos(reloadingWeaponItem.WeaponType.RequireAmmoType, ReloadingAmmoAmount, out decreaseItems))
                    {
                        this.FillEmptySlots();
                        reloadingWeapon.ammo += ReloadingAmmoAmount;
                        if (isLeftHand)
                            equipWeapons.leftHand = reloadingWeapon;
                        else
                            equipWeapons.rightHand = reloadingWeapon;
                        EquipWeapons = equipWeapons;
                    }
                    await UniTask.Delay((int)((totalDuration - triggerDurations[i]) / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, cancellationTokenSource.Token);
                }
            }
            catch
            {
                // Catch the cancellation
            }
            finally
            {
                reloadCancellationTokenSources.Remove(cancellationTokenSource);
                cancellationTokenSource.Dispose();
            }
            // Clear action states at clients and server
            ClearActionStates();
        }

        /// <summary>
        /// Is function will be called at server to order character to attack
        /// </summary>
        protected virtual void ServerAttack(bool isLeftHand)
        {
            if (!CanAttack())
                return;

            // Prepare requires data and get weapon data
            AnimActionType animActionType;
            int animaActionDataId;
            CharacterItem weapon;
            GetAttackingData(
                ref isLeftHand,
                out animActionType,
                out animaActionDataId,
                out weapon);

            // Prepare requires data and get animation data
            int animationIndex;
            float animSpeedRate;
            float[] triggerDurations;
            float totalDuration;
            GetRandomAnimationData(
                animActionType,
                animaActionDataId,
                out animationIndex,
                out animSpeedRate,
                out triggerDurations,
                out totalDuration);

            // Validate ammo
            if (!ValidateAmmo(weapon))
                return;

            // Start attack routine
            IsAttackingOrUsingSkill = true;

            // Play animations
            RequestPlayAttackAnimation(isLeftHand, (byte)animationIndex);
        }

        protected async UniTaskVoid AttackRoutine(bool isLeftHand, byte animationIndex)
        {
            // Prepare requires data and get weapon data
            AnimActionType animActionType;
            int animActionDataId;
            CharacterItem weapon;
            GetAttackingData(
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
            SetAttackActionStates(animActionType, animActionDataId);

            // Prepare requires data and get damages data
            DamageInfo damageInfo = this.GetWeaponDamageInfo(ref isLeftHand);
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = GetWeaponDamages(ref isLeftHand);

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileAttackOrUseSkill = GetMoveSpeedRateWhileAttackOrUseSkill(AnimActionType, null);

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            animSpeedRate *= GetAnimSpeedRate(AnimActionType);

            // Prepare cancellation
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            attackCancellationTokenSources.Add(cancellationTokenSource);
            try
            {
                // Animations will plays on clients only
                if (IsClient)
                {
                    // Play animation
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
                    await UniTask.Delay((int)(tempTriggerDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, cancellationTokenSource.Token);

                    // Special effects will plays on clients only
                    if (IsClient)
                    {
                        // Play weapon launch special effects
                        CharacterModel.PlayWeaponLaunchEffect(AnimActionType);
                        if (FpsModel && FpsModel.gameObject.activeSelf)
                            FpsModel.PlayWeaponLaunchEffect(AnimActionType);
                    }

                    // Get aim position by character's forward
                    Vector3 aimPosition = GetDefaultAttackAimPosition(damageInfo, isLeftHand);
                    if (HasAimPosition)
                        aimPosition = AimPosition;

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

                    if (remainsDuration <= 0f)
                    {
                        // Stop trigger animations loop
                        break;
                    }
                }

                if (remainsDuration > 0f)
                {
                    // Wait until animation ends to stop actions
                    await UniTask.Delay((int)(remainsDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, cancellationTokenSource.Token);
                }
            }
            catch
            {
                // Catch the cancellation
            }
            finally
            {
                attackCancellationTokenSources.Remove(cancellationTokenSource);
                cancellationTokenSource.Dispose();
            }
            // Clear action states at clients and server
            ClearActionStates();
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
                fireSpread = weapon.GetWeaponItem().FireSpread;
                fireStagger = weapon.GetWeaponItem().FireStagger;
            }

            Vector3 stagger;
            for (int i = 0; i < fireSpread + 1; ++i)
            {
                stagger = new Vector3(Random.Range(-fireStagger.x, fireStagger.x), Random.Range(-fireStagger.y, fireStagger.y));
                damageInfo.LaunchDamageEntity(
                    this,
                    isLeftHand,
                    weapon,
                    damageAmounts,
                    null,
                    0,
                    aimPosition,
                    stagger);
            }
        }

        protected void CancelReload()
        {
            if (reloadCancellationTokenSources.Count > 0)
            {
                List<CancellationTokenSource> cancellationSources = new List<CancellationTokenSource>(reloadCancellationTokenSources);
                foreach (CancellationTokenSource cancellationSource in cancellationSources)
                {
                    if (!cancellationSource.IsCancellationRequested)
                        cancellationSource.Cancel();
                }
            }
        }

        protected void CancelAttack()
        {
            if (attackCancellationTokenSources.Count > 0)
            {
                List<CancellationTokenSource> cancellationSources = new List<CancellationTokenSource>(attackCancellationTokenSources);
                foreach (CancellationTokenSource cancellationSource in cancellationSources)
                {
                    if (!cancellationSource.IsCancellationRequested)
                        cancellationSource.Cancel();
                }
            }
        }
    }
}
