using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public class LegacyCharacterAttackComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterAttackComponent
    {
        protected List<CancellationTokenSource> attackCancellationTokenSources = new List<CancellationTokenSource>();
        public bool IsAttacking { get; protected set; }
        public float LastAttackEndTime { get; protected set; }
        public float MoveSpeedRateWhileAttacking { get; protected set; }
        public AnimActionType AnimActionType { get; protected set; }
        public int AnimActionDataId { get; protected set; }

        [SerializeField]
        [Range(0.00825f, 0.1f)]
        private float clientSendAimPositionInterval = 0.05f;

        private float clientSendAimPositionCountDown;

        public override void EntityFixedUpdate()
        {
            base.EntityFixedUpdate();
            if (IsOwnerClient && !Entity.IsDead())
            {
                clientSendAimPositionCountDown -= Time.unscaledDeltaTime;
                if (clientSendAimPositionCountDown <= 0f)
                {
                    ClientSendPacket(BaseCharacterEntity.ACTION_TO_SERVER_DATA_CHANNEL, DeliveryMethod.Sequenced, GameNetworkingConsts.SetAimPosition, (writer) =>
                    {
                        writer.PutValue(Entity.AimPosition);
                    });
                    clientSendAimPositionCountDown = clientSendAimPositionInterval;
                }
            }
        }

        protected virtual void SetAttackActionStates(AnimActionType animActionType, int animActionDataId)
        {
            ClearAttackStates();
            AnimActionType = animActionType;
            AnimActionDataId = animActionDataId;
            IsAttacking = true;
        }

        public virtual void ClearAttackStates()
        {
            IsAttacking = false;
        }

        public bool CallServerAttack(bool isLeftHand)
        {
            RPC(ServerAttack, BaseCharacterEntity.ACTION_TO_SERVER_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand);
            return true;
        }

        /// <summary>
        /// Is function will be called at server to order character to attack
        /// </summary>
        [ServerRpc]
        protected virtual void ServerAttack(bool isLeftHand)
        {
#if !CLIENT_BUILD
            if (!Entity.CanAttack())
                return;

            // Prepare requires data and get weapon data
            AnimActionType animActionType;
            int animaActionDataId;
            CharacterItem weapon;
            Entity.GetAttackingData(
                ref isLeftHand,
                out animActionType,
                out animaActionDataId,
                out weapon);

            // Prepare requires data and get animation data
            int animationIndex;
            float animSpeedRate;
            float[] triggerDurations;
            float totalDuration;
            Entity.GetRandomAnimationData(
                animActionType,
                animaActionDataId,
                Random.Range(int.MinValue, int.MaxValue),
                out animationIndex,
                out animSpeedRate,
                out triggerDurations,
                out totalDuration);

            // Start attack routine
            IsAttacking = true;

            // Play animations
            CallAllPlayAttackAnimation(isLeftHand, (byte)animationIndex);
#endif
        }

        public bool CallAllPlayAttackAnimation(bool isLeftHand, byte animationIndex)
        {
            RPC(AllPlayAttackAnimation, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand, animationIndex);
            return true;
        }

        [AllRpc]
        protected void AllPlayAttackAnimation(bool isLeftHand, byte animationIndex)
        {
            AttackRoutine(isLeftHand, animationIndex).Forget();
        }

        protected async UniTaskVoid AttackRoutine(bool isLeftHand, byte animationIndex)
        {
            // Prepare cancellation
            CancellationTokenSource attackCancellationTokenSource = new CancellationTokenSource();
            attackCancellationTokenSources.Add(attackCancellationTokenSource);

            // Prepare requires data and get weapon data
            AnimActionType animActionType;
            int animActionDataId;
            CharacterItem weapon;
            Entity.GetAttackingData(
                ref isLeftHand,
                out animActionType,
                out animActionDataId,
                out weapon);

            // Prepare requires data and get animation data
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

            // Prepare requires data and get damages data
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            DamageInfo damageInfo = Entity.GetWeaponDamageInfo(weaponItem);
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = Entity.GetWeaponDamagesWithBuffs(weapon);

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileAttacking = Entity.GetMoveSpeedRateWhileAttacking(weaponItem);

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            animSpeedRate *= Entity.GetAnimSpeedRate(AnimActionType);
            try
            {
                // Play action animation
                if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                {
                    // TPS model
                    Entity.CharacterModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                }
                if (Entity.PassengingVehicleEntity != null && Entity.PassengingVehicleEntity.Entity.Model &&
                    Entity.PassengingVehicleEntity.Entity.Model.gameObject.activeSelf &&
                    Entity.PassengingVehicleEntity.Entity.Model is BaseCharacterModel)
                {
                    // Vehicle model
                    (Entity.PassengingVehicleEntity.Entity.Model as BaseCharacterModel).PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                }
                if (IsClient)
                {
                    if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                    {
                        // FPS model
                        Entity.FpsModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                    }
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
                            Entity.CharacterModel.PlayEquippedWeaponLaunch(isLeftHand);
                        if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                            Entity.FpsModel.PlayEquippedWeaponLaunch(isLeftHand);
                        // Play launch sfx
                        if (AnimActionType == AnimActionType.AttackRightHand ||
                            AnimActionType == AnimActionType.AttackLeftHand)
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
                        if (IsServer)
                        {
                            int randomSeed = Random.Range(0, 255);
                            ApplyAttack(isLeftHand, weapon, damageInfo, damageAmounts, Entity.AimPosition, randomSeed);
                            SimulateLaunchDamageEntityData simulateData = new SimulateLaunchDamageEntityData();
                            if (isLeftHand)
                                simulateData.state |= SimulateLaunchDamageEntityState.IsLeftHand;
                            simulateData.randomSeed = (byte)randomSeed;
                            simulateData.aimPosition = Entity.AimPosition;
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
                // Clear action states at clients and server
                if (!attackCancellationTokenSource.IsCancellationRequested)
                {
                    ClearAttackStates();
                    LastAttackEndTime = Time.unscaledTime;
                }
                attackCancellationTokenSource.Dispose();
                attackCancellationTokenSources.Remove(attackCancellationTokenSource);
            }
        }

        protected virtual void ApplyAttack(bool isLeftHand, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, AimPosition aimPosition, int randomSeed)
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
            System.Random random = new System.Random(randomSeed);
            Vector3 stagger;
            for (int i = 0; i < fireSpread + 1; ++i)
            {
                stagger = new Vector3();
                stagger.x = GenericUtils.RandomFloat(random.Next(), -fireStagger.x, fireStagger.x);
                stagger.y = GenericUtils.RandomFloat(random.Next(), -fireStagger.y, fireStagger.y);
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
                    null);
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
            if (IsServer)
                return;

            bool isLeftHand = data.state.HasFlag(SimulateLaunchDamageEntityState.IsLeftHand);
            if (!data.state.HasFlag(SimulateLaunchDamageEntityState.IsSkill))
            {
                CharacterItem weapon = Entity.GetAvailableWeapon(ref isLeftHand);
                DamageInfo damageInfo = Entity.GetWeaponDamageInfo(weapon.GetWeaponItem());
                Dictionary<DamageElement, MinMaxFloat> damageAmounts = Entity.GetWeaponDamagesWithBuffs(weapon);
                ApplyAttack(isLeftHand, weapon, damageInfo, damageAmounts, data.aimPosition, data.randomSeed);
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
            CallServerAttack(isLeftHand);
        }
    }
}
