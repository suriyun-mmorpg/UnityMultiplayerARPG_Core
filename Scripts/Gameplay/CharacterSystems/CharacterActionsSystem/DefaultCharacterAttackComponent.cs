using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultCharacterAttackComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterAttackComponent
    {
        protected List<CancellationTokenSource> attackCancellationTokenSources = new List<CancellationTokenSource>();
        public bool IsAttacking { get; protected set; }
        public float MoveSpeedRateWhileAttacking { get; protected set; }

        protected virtual void SetAttackActionStates(AnimActionType animActionType, int animActionDataId)
        {
            Entity.ClearActionStates();
            Entity.AnimActionType = animActionType;
            Entity.AnimActionDataId = animActionDataId;
            IsAttacking = true;
        }

        public virtual void ClearAttackStates()
        {
            IsAttacking = false;
        }

        public bool CallAllPlayAttackAnimation(bool isLeftHand, byte animationIndex)
        {
            RPC(AllPlayAttackAnimation, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand, animationIndex);
            return true;
        }

        [AllRpc]
        protected void AllPlayAttackAnimation(bool isLeftHand, byte animationIndex)
        {
            if (IsOwnerClientOrOwnedByServer)
                return;
            AttackRoutine(isLeftHand, animationIndex).Forget();
        }

        protected async UniTaskVoid AttackRoutine(bool isLeftHand, byte animationIndex)
        {
            // Prepare cancellation
            CancellationTokenSource attackCancellationTokenSource = new CancellationTokenSource();
            attackCancellationTokenSources.Add(attackCancellationTokenSource);

            // Prepare required data and get weapon data
            AnimActionType animActionType;
            int animActionDataId;
            CharacterItem weapon;
            Entity.GetAttackingData(
                ref isLeftHand,
                out animActionType,
                out animActionDataId,
                out weapon);

            // Prepare required data and get animation data
            float animSpeedRate;
            float[] triggerDurations;
            float totalDuration;
            Entity.GetAnimationData(
                animActionType,
                animActionDataId,
                animationIndex,
                out animSpeedRate,
                out triggerDurations,
                out totalDuration);

            // Set doing action state at clients and server
            SetAttackActionStates(animActionType, animActionDataId);

            // Prepare required data and get damages data
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            DamageInfo damageInfo = Entity.GetWeaponDamageInfo(weaponItem);
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = Entity.GetWeaponDamagesWithBuffs(weapon);

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileAttacking = Entity.GetMoveSpeedRateWhileAttacking(weaponItem);

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            animSpeedRate *= Entity.GetAnimSpeedRate(Entity.AnimActionType);
            try
            {
                // Animations will plays on clients only
                // Play animation
                if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                    Entity.CharacterModel.PlayActionAnimation(Entity.AnimActionType, Entity.AnimActionDataId, animationIndex, animSpeedRate);
                if (IsClient)
                {
                    if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                        Entity.FpsModel.PlayActionAnimation(Entity.AnimActionType, Entity.AnimActionDataId, animationIndex, animSpeedRate);
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
                        if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                            Entity.CharacterModel.PlayWeaponLaunchEffect(Entity.AnimActionType);
                        if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                            Entity.FpsModel.PlayWeaponLaunchEffect(Entity.AnimActionType);
                        // Play launch sfx
                        if (Entity.AnimActionType == AnimActionType.AttackRightHand ||
                            Entity.AnimActionType == AnimActionType.AttackLeftHand)
                            AudioManager.PlaySfxClipAtAudioSource(weaponItem.LaunchClip, Entity.CharacterModel.GenericAudioSource);
                    }

                    // Call on attack to extend attack functionality while attacking
                    bool overrideDefaultAttack = false;
                    foreach (KeyValuePair<BaseSkill, short> skillLevel in Entity.GetCaches().Skills)
                    {
                        if (skillLevel.Value <= 0)
                            continue;
                        if (skillLevel.Key.OnAttack(Entity, skillLevel.Value, isLeftHand, weapon, hitIndex, damageAmounts, Entity.AimPosition))
                            overrideDefaultAttack = true;
                    }

                    // Skip attack function when applied skills (buffs) will override default attack functionality
                    if (!overrideDefaultAttack)
                    {
                        // Trigger attack event
                        Entity.OnAttackRoutine(isLeftHand, weapon, hitIndex, damageInfo, damageAmounts, Entity.AimPosition);

                        // Apply attack damages
                        if (IsOwnerClientOrOwnedByServer)
                        {
                            int randomSeed = Random.Range(0, 255);
                            long time = BaseGameNetworkManager.Singleton.ServerTimestamp;
                            ApplyAttack(isLeftHand, weapon, damageInfo, damageAmounts, Entity.AimPosition, randomSeed, time);
                            SimulateLaunchDamageEntityData simulateData = new SimulateLaunchDamageEntityData();
                            if (isLeftHand)
                                simulateData.state |= SimulateLaunchDamageEntityState.IsLeftHand;
                            simulateData.randomSeed = (byte)randomSeed;
                            simulateData.aimPosition = Entity.AimPosition;
                            simulateData.time = time;
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
                    EquipWeapons equipWeapons = Entity.EquipWeapons;
                    if (isLeftHand)
                        equipWeapons.leftHand = CharacterItem.Empty;
                    else
                        equipWeapons.rightHand = CharacterItem.Empty;
                    Entity.EquipWeapons = equipWeapons;
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
                attackCancellationTokenSources.Remove(attackCancellationTokenSource);
            }
            // Clear action states at clients and server
            Entity.ClearActionStates();
        }

        protected virtual void ApplyAttack(bool isLeftHand, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, AimPosition aimPosition, int randomSeed, long? time)
        {
            if (IsServer)
            {
                // Increase damage with ammo damage
                Dictionary<DamageElement, MinMaxFloat> increaseDamages;
                Entity.DecreaseAmmos(weapon, isLeftHand, 1, out increaseDamages);
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

            // Fire
            Vector3 stagger;
            for (int i = 0; i < fireSpread + 1; ++i)
            {
                stagger = new Vector3();
                randomSeed = unchecked(randomSeed += (i + 1) * 4);
                stagger.x = GenericUtils.RandomFloat(randomSeed, -fireStagger.x, fireStagger.x);
                randomSeed = unchecked(randomSeed += (i + 1) * 4);
                stagger.y = GenericUtils.RandomFloat(randomSeed, -fireStagger.y, fireStagger.y);
                damageInfo.LaunchDamageEntity(
                    Entity,
                    isLeftHand,
                    weapon,
                    damageAmounts,
                    null,
                    0,
                    randomSeed,
                    aimPosition,
                    stagger,
                    out _,
                    time);
            }
        }

        public bool CallAllSimulateLaunchDamageEntity(SimulateLaunchDamageEntityData data)
        {
            RPC(AllSimulateLaunchDamageEntity, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, data);
            return true;
        }

        [AllRpc]
        protected void AllSimulateLaunchDamageEntity(SimulateLaunchDamageEntityData data)
        {
            if (IsOwnerClientOrOwnedByServer)
                return;

            bool isLeftHand = data.state.HasFlag(SimulateLaunchDamageEntityState.IsLeftHand);
            if (!data.state.HasFlag(SimulateLaunchDamageEntityState.IsSkill))
            {
                CharacterItem weapon = Entity.GetAvailableWeapon(ref isLeftHand);
                DamageInfo damageInfo = Entity.GetWeaponDamageInfo(weapon.GetWeaponItem());
                Dictionary<DamageElement, MinMaxFloat> damageAmounts = Entity.GetWeaponDamagesWithBuffs(weapon);
                ApplyAttack(isLeftHand, weapon, damageInfo, damageAmounts, data.aimPosition, data.randomSeed, data.time);
            }
        }

        public void CancelAttack()
        {
            for (int i = attackCancellationTokenSources.Count - 1; i >= 0; --i)
            {
                if (!attackCancellationTokenSources[i].IsCancellationRequested)
                    attackCancellationTokenSources[i].Cancel();
                attackCancellationTokenSources.RemoveAt(i);
            }
        }

        public void Attack(bool isLeftHand)
        {
            // Prepare required data and get weapon data
            AnimActionType animActionType;
            int animaActionDataId;
            Entity.GetAttackingData(
                ref isLeftHand,
                out animActionType,
                out animaActionDataId,
                out _);

            // Prepare required data and get animation data
            int animationIndex;
            Entity.GetRandomAnimationData(
                animActionType,
                animaActionDataId,
                out animationIndex,
                out _,
                out _,
                out _);

            // Start attack routine
            IsAttacking = true;

            // Play attack animation at owning client immediately
            AttackRoutine(isLeftHand, (byte)animationIndex).Forget();

            // Broadcast attack animation playing
            CallAllPlayAttackAnimation(isLeftHand, (byte)animationIndex);
        }
    }
}
