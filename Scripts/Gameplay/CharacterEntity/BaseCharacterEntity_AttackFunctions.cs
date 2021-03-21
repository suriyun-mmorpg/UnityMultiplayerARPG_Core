using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        protected CancellationTokenSource reloadCancellationTokenSource;
        protected CancellationTokenSource attackCancellationTokenSource;

        public Vector3 GetDefaultAttackAimPosition(ref bool isLeftHand)
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
            return damageInfo.GetDamageTransform(this, isLeftHand).position + CacheTransform.forward * damageInfo.GetDistance();
        }

        public virtual void GetReloadingData(
            ref bool isLeftHand,
            out AnimActionType animActionType,
            out int animationDataId,
            out CharacterItem weapon)
        {
            weapon = this.GetAvailableWeapon(ref isLeftHand);
            // Assign data id
            animationDataId = weapon.GetWeaponItem().WeaponType.DataId;
            // Assign animation action type
            animActionType = !isLeftHand ? AnimActionType.ReloadRightHand : AnimActionType.ReloadLeftHand;
        }

        public virtual void GetAttackingData(
            ref bool isLeftHand,
            out AnimActionType animActionType,
            out int animationDataId,
            out CharacterItem weapon)
        {
            weapon = this.GetAvailableWeapon(ref isLeftHand);
            // Assign data id
            animationDataId = weapon.GetWeaponItem().WeaponType.DataId;
            // Assign animation action type
            animActionType = !isLeftHand ? AnimActionType.AttackRightHand : AnimActionType.AttackLeftHand;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetWeaponDamagesWithBuffs(CharacterItem weapon)
        {
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            // Calculate all damages
            damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, this.GetWeaponDamages(weapon));
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

        public void ReduceAmmo(CharacterItem weapon, bool isLeftHand, out IAmmoItem ammoItem, out short ammoLevel, short amount = 1)
        {
            ammoItem = null;
            ammoLevel = 1;

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
                    ammoItem = ammoCharacterItem.GetAmmoItem();
                    ammoLevel = ammoCharacterItem.level;
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

        [ServerRpc]
        protected virtual void ServerReload(bool isLeftHand)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
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
            CallAllPlayReloadAnimation(isLeftHand, reloadingAmmoAmount);
#endif
        }

        protected async UniTaskVoid ReloadRoutine(bool isLeftHand, short reloadingAmmoAmount)
        {
            // Reload animation still playing, skip it
            if (reloadCancellationTokenSource != null)
                return;
            // Prepare cancellation
            reloadCancellationTokenSource = new CancellationTokenSource();

            // Prepare requires data and get weapon data
            AnimActionType animActionType;
            int animActionDataId;
            CharacterItem weapon;
            GetReloadingData(
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
                0,
                out animSpeedRate,
                out triggerDurations,
                out totalDuration);

            // Set doing action state at clients and server
            SetReloadActionStates(animActionType, reloadingAmmoAmount);

            // Prepare requires data and get damages data
            IWeaponItem weaponItem = weapon.GetWeaponItem();

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileReloading = this.GetMoveSpeedRateWhileReloading(weaponItem);
            try
            {
                // Animations will plays on clients only
                if (IsClient)
                {
                    // Play animation
                    if (CharacterModel && CharacterModel.gameObject.activeSelf)
                        CharacterModel.PlayActionAnimation(AnimActionType, animActionDataId, 0);
                    if (FpsModel && FpsModel.gameObject.activeSelf)
                        FpsModel.PlayActionAnimation(AnimActionType, animActionDataId, 0);
                    // Play reload sfx
                    if (AnimActionType == AnimActionType.ReloadRightHand ||
                        AnimActionType == AnimActionType.ReloadLeftHand)
                        AudioManager.PlaySfxClipAtAudioSource(weaponItem.ReloadClip, CharacterModel.GenericAudioSource);
                }

                for (int i = 0; i < triggerDurations.Length; ++i)
                {
                    // Wait until triggger before reload ammo
                    await UniTask.Delay((int)(triggerDurations[i] / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, reloadCancellationTokenSource.Token);

                    // Prepare data
                    short triggerReloadAmmoAmount = (short)(ReloadingAmmoAmount / triggerDurations.Length);
                    EquipWeapons equipWeapons = EquipWeapons;
                    Dictionary<CharacterItem, short> decreaseItems;
                    if (IsServer && this.DecreaseAmmos(weaponItem.WeaponType.RequireAmmoType, triggerReloadAmmoAmount, out decreaseItems))
                    {
                        this.FillEmptySlots();
                        weapon.ammo += triggerReloadAmmoAmount;
                        if (isLeftHand)
                            equipWeapons.leftHand = weapon;
                        else
                            equipWeapons.rightHand = weapon;
                        EquipWeapons = equipWeapons;
                    }
                    await UniTask.Delay((int)((totalDuration - triggerDurations[i]) / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, reloadCancellationTokenSource.Token);
                }
            }
            catch
            {
                // Catch the cancellation
            }
            finally
            {
                reloadCancellationTokenSource.Dispose();
                reloadCancellationTokenSource = null;
            }
            // Clear action states at clients and server
            ClearActionStates();
        }

        /// <summary>
        /// Is function will be called at server to order character to start weapon charging
        /// </summary>
        [ServerRpc]
        protected virtual void ServerStartWeaponCharge(bool isLeftHand)
        {
            // TODO: May have charge power which increase attack damage
            CallAllPlayChargeAnimation(isLeftHand);
        }

        /// <summary>
        /// Is function will be called at server to order character to stop weapon charging
        /// </summary>
        [ServerRpc]
        protected virtual void ServerStopWeaponCharge()
        {
            // TODO: If there is charge power, stop it. But there is no charge power yet so just stop playing animation
            CallAllStopChargeAnimation();
        }

        /// <summary>
        /// Is function will be called at server to order character to attack
        /// </summary>
        [ServerRpc]
        protected virtual void ServerAttack(bool isLeftHand)
        {
#if !CLIENT_BUILD
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
            CallAllPlayAttackAnimation(isLeftHand, (byte)animationIndex);
#endif
        }

        protected async UniTaskVoid AttackRoutine(bool isLeftHand, byte animationIndex)
        {
            // Attack animation still playing, skip it
            if (attackCancellationTokenSource != null)
                return;
            // Prepare cancellation
            attackCancellationTokenSource = new CancellationTokenSource();

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
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            DamageInfo damageInfo = this.GetWeaponDamageInfo(weaponItem);
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = GetWeaponDamagesWithBuffs(weapon);

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileAttackingOrUsingSkill = this.GetMoveSpeedRateWhileAttacking(weaponItem);

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            animSpeedRate *= GetAnimSpeedRate(AnimActionType);
            try
            {
                // Animations will plays on clients only
                if (IsClient)
                {
                    // Play animation
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
                    tempTriggerDuration = triggerDurations[hitIndex];
                    remainsDuration -= tempTriggerDuration;
                    await UniTask.Delay((int)(tempTriggerDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, attackCancellationTokenSource.Token);

                    // Special effects will plays on clients only
                    if (IsClient)
                    {
                        // Play weapon launch special effects
                        if (CharacterModel && CharacterModel.gameObject.activeSelf)
                            CharacterModel.PlayWeaponLaunchEffect(AnimActionType);
                        if (FpsModel && FpsModel.gameObject.activeSelf)
                            FpsModel.PlayWeaponLaunchEffect(AnimActionType);
                        // Play launch sfx
                        if (AnimActionType == AnimActionType.AttackRightHand ||
                            AnimActionType == AnimActionType.AttackLeftHand)
                            AudioManager.PlaySfxClipAtAudioSource(weaponItem.LaunchClip, CharacterModel.GenericAudioSource);
                    }

                    // Call on attack to extend attack functionality while attacking
                    bool overrideDefaultAttack = false;
                    foreach (KeyValuePair<BaseSkill, short> skillLevel in this.GetCaches().Skills)
                    {
                        if (skillLevel.Value <= 0)
                            continue;
                        if (skillLevel.Key.OnAttack(this, skillLevel.Value, isLeftHand, weapon, hitIndex, damageAmounts, AimPosition))
                            overrideDefaultAttack = true;
                    }

                    // Skip attack function when applied skills (buffs) will override default attack functionality
                    if (!overrideDefaultAttack)
                    {
                        // Trigger attack event
                        if (onAttackRoutine != null)
                            onAttackRoutine.Invoke(isLeftHand, weapon, hitIndex, damageInfo, damageAmounts, AimPosition);

                        // Apply attack damages
                        if (IsServer)
                        {
                            int randomSeed = Random.Range(0, 255);
                            ApplyAttack(isLeftHand, weapon, damageInfo, damageAmounts, AimPosition, randomSeed);
                            SimulateLaunchDamageEntityData simulateData = new SimulateLaunchDamageEntityData();
                            if (isLeftHand)
                                simulateData.state |= SimulateLaunchDamageEntityState.IsLeftHand;
                            simulateData.randomSeed = (byte)randomSeed;
                            simulateData.aimPosition = AimPosition;
                            CallAllSimulateLaunchDamageEntity(simulateData);
                        }
                    }

                    if (remainsDuration <= 0f)
                    {
                        // Stop trigger animations loop
                        break;
                    }
                }

                if (IsServer && weaponItem.DestroyImmediatelyAfterFired)
                {
                    EquipWeapons equipWeapons = EquipWeapons;
                    if (isLeftHand)
                        equipWeapons.leftHand = CharacterItem.Empty;
                    else
                        equipWeapons.rightHand = CharacterItem.Empty;
                    EquipWeapons = equipWeapons;
                }

                if (remainsDuration > 0f)
                {
                    // Wait until animation ends to stop actions
                    await UniTask.Delay((int)(remainsDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, attackCancellationTokenSource.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Catch the cancellation
            }
            catch (System.Exception ex)
            {
                // Other errors
                Logging.LogException(LogTag, ex);
            }
            finally
            {
                attackCancellationTokenSource.Dispose();
                attackCancellationTokenSource = null;
            }
            // Clear action states at clients and server
            ClearActionStates();
        }

        protected virtual void ApplyAttack(bool isLeftHand, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, Vector3 aimPosition, int randomSeed)
        {
            if (IsServer)
            {
                // Increase damage with ammo damage
                IAmmoItem ammoItem;
                short ammoLevel;
                ReduceAmmo(weapon, isLeftHand, out ammoItem, out ammoLevel);
                if (ammoItem != null)
                    damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, ammoItem.GetIncreaseDamages(ammoLevel));
            }

            byte fireSpread = 0;
            Vector3 fireStagger = Vector3.zero;
            if (weapon != null && weapon.GetWeaponItem() != null)
            {
                // For monsters, their weapon can be null so have to avoid null exception
                fireSpread = weapon.GetWeaponItem().FireSpread;
                fireStagger = weapon.GetWeaponItem().FireStagger;
            }

            // Random fire stagger seed while it's not simulating
            randomSeed = Random.Range(0, 255);

            // Fire
            Vector3 stagger;
            for (int i = 0; i < fireSpread + 1; ++i)
            {
                Random.InitState(unchecked(randomSeed++));
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

        [AllRpc]
        protected void AllSimulateLaunchDamageEntity(SimulateLaunchDamageEntityData data)
        {
            if (IsServer)
                return;

            bool isLeftHand = data.state.HasFlag(SimulateLaunchDamageEntityState.IsLeftHand);
            if (data.state.HasFlag(SimulateLaunchDamageEntityState.IsSkill))
            {
                BaseSkill skill = data.GetSkill();
                if (skill != null)
                {
                    CharacterItem weapon = this.GetAvailableWeapon(ref isLeftHand);
                    Dictionary<DamageElement, MinMaxFloat> damageAmounts = skill.GetAttackDamages(this, data.skillLevel, isLeftHand);
                    skill.ApplySkill(this, data.skillLevel, isLeftHand, weapon, data.hitIndex, damageAmounts, data.aimPosition);
                }
            }
            else
            {
                CharacterItem weapon = this.GetAvailableWeapon(ref isLeftHand);
                DamageInfo damageInfo = this.GetWeaponDamageInfo(weapon.GetWeaponItem());
                Dictionary<DamageElement, MinMaxFloat> damageAmounts = GetWeaponDamagesWithBuffs(weapon);
                ApplyAttack(isLeftHand, weapon, damageInfo, damageAmounts, data.aimPosition, data.randomSeed);
            }
        }

        protected void CancelReload()
        {
            if (reloadCancellationTokenSource != null &&
                !reloadCancellationTokenSource.IsCancellationRequested)
                reloadCancellationTokenSource.Cancel();
        }

        protected void CancelAttack()
        {
            if (attackCancellationTokenSource != null &&
                !attackCancellationTokenSource.IsCancellationRequested)
                attackCancellationTokenSource.Cancel();
        }
    }
}
