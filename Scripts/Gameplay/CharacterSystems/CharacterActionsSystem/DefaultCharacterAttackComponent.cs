using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultCharacterAttackComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterAttackComponent
    {
        public const float DEFAULT_TOTAL_DURATION = 2f;
        public const float DEFAULT_TRIGGER_DURATION = 1f;
        public const float DEFAULT_STATE_SETUP_DELAY = 1f;
        protected List<CancellationTokenSource> attackCancellationTokenSources = new List<CancellationTokenSource>();
        public bool IsAttacking { get; protected set; }
        public float LastAttackEndTime { get; protected set; }
        public float MoveSpeedRateWhileAttacking { get; protected set; }
        public MovementRestriction MovementRestrictionWhileAttacking { get; protected set; }
        protected float totalDuration;
        public float AttackTotalDuration { get { return totalDuration; } set { totalDuration = value; } }
        protected float[] triggerDurations;
        public float[] AttackTriggerDurations { get { return triggerDurations; } set { triggerDurations = value; } }
        public AnimActionType AnimActionType { get; protected set; }
        public int AnimActionDataId { get; protected set; }

        protected readonly Dictionary<int, SimulatingActionTriggerHistory> SimulatingActionTriggerHistories = new Dictionary<int, SimulatingActionTriggerHistory>();
        protected readonly Dictionary<int, List<SimulateActionTriggerData>> SimlatingActionTriggerDataList = new Dictionary<int, List<SimulateActionTriggerData>>();
        protected bool sendingClientAttack;
        protected bool sendingServerAttack;
        protected byte sendingSeed;
        protected bool sendingIsLeftHand;

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

        protected async UniTaskVoid AttackRoutine(byte simulateSeed, bool isLeftHand)
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
            int animationIndex;
            float animSpeedRate;
            Entity.GetRandomAnimationData(
                animActionType,
                animActionDataId,
                simulateSeed,
                out animationIndex,
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
            MovementRestrictionWhileAttacking = Entity.GetMovementRestrictionWhileAttacking(weaponItem);

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            animSpeedRate *= Entity.GetAnimSpeedRate(AnimActionType);

            // Last attack end time
            float remainsDuration = DEFAULT_TOTAL_DURATION;
            LastAttackEndTime = Time.unscaledTime + DEFAULT_TOTAL_DURATION;
            if (totalDuration >= 0f)
            {
                remainsDuration = totalDuration;
                LastAttackEndTime = Time.unscaledTime + (totalDuration / animSpeedRate);
            }

            if (IsServer)
            {
                // Do something with buffs when attack
                Entity.SkillAndBuffComponent.OnAttack();
            }

            try
            {
                // Play action animation
                if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                {
                    // TPS model
                    Entity.CharacterModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                }
                if (Entity.PassengingVehicleModel && Entity.PassengingVehicleModel is BaseCharacterModel)
                {
                    // Vehicle model
                    (Entity.PassengingVehicleModel as BaseCharacterModel).PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                }
                if (IsClient)
                {
                    if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                    {
                        // FPS model
                        Entity.FpsModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                    }
                }

                // Try setup state data (maybe by animation clip events or state machine behaviours), if it was not set up
                if (triggerDurations == null || triggerDurations.Length == 0 || totalDuration < 0f)
                {
                    // Wait some components to setup proper `attackTriggerDurations` and `attackTotalDuration` within `DEFAULT_STATE_SETUP_DELAY`
                    float setupDelayCountDown = DEFAULT_STATE_SETUP_DELAY;
                    do
                    {
                        await UniTask.Yield();
                        setupDelayCountDown -= Time.unscaledDeltaTime;
                    } while (setupDelayCountDown > 0 && (triggerDurations == null || triggerDurations.Length == 0 || totalDuration < 0f));
                    if (setupDelayCountDown <= 0f)
                    {
                        // Can't setup properly, so try to setup manually to make it still workable
                        remainsDuration = DEFAULT_TOTAL_DURATION - DEFAULT_STATE_SETUP_DELAY;
                        triggerDurations = new float[1]
                        {
                        DEFAULT_TRIGGER_DURATION,
                        };
                    }
                    else
                    {
                        // Can setup, so set proper `remainsDuration` and `LastAttackEndTime` value
                        remainsDuration = totalDuration;
                        LastAttackEndTime = Time.unscaledTime + (totalDuration / animSpeedRate);
                    }
                }

                SimulatingActionTriggerHistories[simulateSeed] = new SimulatingActionTriggerHistory(triggerDurations.Length);
                if (SimlatingActionTriggerDataList.ContainsKey(simulateSeed))
                {
                    foreach (SimulateActionTriggerData data in SimlatingActionTriggerDataList[simulateSeed])
                    {
                        ProceedSimulateActionTrigger(data);
                    }
                }
                SimlatingActionTriggerDataList.Clear();

                float tempTriggerDuration;
                for (int hitIndex = 0; hitIndex < triggerDurations.Length; ++hitIndex)
                {
                    // Wait until triggger before play special effects
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
                        AudioClipWithVolumeSettings audioClip = weaponItem.LaunchClip;
                        if (audioClip != null)
                            AudioManager.PlaySfxClipAtAudioSource(audioClip.audioClip, Entity.CharacterModel.GenericAudioSource, audioClip.GetRandomedVolume());
                    }

                    // Call on attack to extend attack functionality while attacking
                    bool overrideDefaultAttack = false;
                    foreach (KeyValuePair<BaseSkill, int> skillLevel in Entity.GetCaches().Skills)
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
                            int applySeed = unchecked(simulateSeed + (hitIndex * 16));
                            ApplyAttack(isLeftHand, weapon, damageInfo, damageAmounts, Entity.AimPosition, applySeed);
                            SimulateActionTriggerData simulateData = new SimulateActionTriggerData();
                            if (isLeftHand)
                                simulateData.state |= SimulateActionTriggerState.IsLeftHand;
                            simulateData.randomSeed = simulateSeed;
                            simulateData.aimPosition = Entity.AimPosition;
                            RPC(AllSimulateActionTrigger, BaseGameEntity.SERVER_STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, simulateData);
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
                LastAttackEndTime = Time.unscaledTime;
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
            ClearAttackStates();
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

            // Prepare hit reg validatation, hit reg will be made from client later
            if (IsServer && !IsOwnerClient && !IsOwnedByServer)
                BaseGameNetworkManager.Singleton.HitRegistrationManager.PrepareHitRegValidatation(damageInfo, randomSeed, fireSpread, Entity, damageAmounts, weapon, null, 0);

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
                    out _);
            }
        }

        [AllRpc]
        protected void AllSimulateActionTrigger(SimulateActionTriggerData data)
        {
            if (IsOwnerClientOrOwnedByServer)
                return;
            if (!ProceedSimulateActionTrigger(data))
            {
                if (!SimlatingActionTriggerDataList.ContainsKey(data.randomSeed))
                    SimlatingActionTriggerDataList[data.randomSeed] = new List<SimulateActionTriggerData>();
                SimlatingActionTriggerDataList[data.randomSeed].Add(data);
            }
        }

        protected bool ProceedSimulateActionTrigger(SimulateActionTriggerData data)
        {
            SimulatingActionTriggerHistory history;
            if (!SimulatingActionTriggerHistories.TryGetValue(data.randomSeed, out history) || history.TriggeredIndex >= history.TriggerLength)
                return false;
            int hitIndex = SimulatingActionTriggerHistories[data.randomSeed].TriggeredIndex;
            int applySeed = unchecked(data.randomSeed + (hitIndex * 16));
            hitIndex++;
            history.TriggeredIndex = hitIndex;
            SimulatingActionTriggerHistories[data.randomSeed] = history;
            bool isLeftHand = data.state.HasFlag(SimulateActionTriggerState.IsLeftHand);
            if (!data.state.HasFlag(SimulateActionTriggerState.IsSkill))
            {
                CharacterItem weapon = Entity.GetAvailableWeapon(ref isLeftHand);
                DamageInfo damageInfo = Entity.GetWeaponDamageInfo(weapon.GetWeaponItem());
                Dictionary<DamageElement, MinMaxFloat> damageAmounts = Entity.GetWeaponDamagesWithBuffs(weapon);
                ApplyAttack(isLeftHand, weapon, damageInfo, damageAmounts, data.aimPosition, applySeed);
            }
            return true;
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
            if (!IsServer && IsOwnerClient)
            {
                // Get simulate seed for simulation validating
                byte simulateSeed = (byte)Random.Range(byte.MinValue, byte.MaxValue);
                // Set attack state
                IsAttacking = true;
                // Simulate attacking at client immediately
                AttackRoutine(simulateSeed, isLeftHand).Forget();
                // Tell server that this client attack
                sendingClientAttack = true;
                sendingSeed = simulateSeed;
                sendingIsLeftHand = isLeftHand;
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Get simulate seed for simulation validating
                byte simulateSeed = (byte)Random.Range(byte.MinValue, byte.MaxValue);
                // Attack immediately at server
                ProceedAttackStateAtServer(simulateSeed, isLeftHand);
            }
        }

        public bool WriteClientAttackState(NetDataWriter writer)
        {
            if (sendingClientAttack)
            {
                writer.Put(sendingSeed);
                writer.Put(sendingIsLeftHand);
                sendingClientAttack = false;
                return true;
            }
            return false;
        }

        public bool WriteServerAttackState(NetDataWriter writer)
        {
            if (sendingServerAttack)
            {
                writer.Put(sendingSeed);
                writer.Put(sendingIsLeftHand);
                sendingServerAttack = false;
                return true;
            }
            return false;
        }

        public void ReadClientAttackStateAtServer(NetDataReader reader)
        {
            byte simulateSeed = reader.GetByte();
            bool isLeftHand = reader.GetBool();
            ProceedAttackStateAtServer(simulateSeed, isLeftHand);
        }

        protected void ProceedAttackStateAtServer(byte simulateSeed, bool isLeftHand)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Speed hack avoidance
            if (Time.unscaledTime - LastAttackEndTime < -0.05f)
                return;
            // Set attack state
            IsAttacking = true;
            // Play attack animation at server immediately
            AttackRoutine(simulateSeed, isLeftHand).Forget();
            // Tell clients to play animation later
            sendingServerAttack = true;
            sendingSeed = simulateSeed;
            sendingIsLeftHand = isLeftHand;
#endif
        }

        public void ReadServerAttackStateAtClient(NetDataReader reader)
        {
            byte simulateSeed = reader.GetByte();
            bool isLeftHand = reader.GetBool();
            if (IsOwnerClientOrOwnedByServer)
            {
                // Don't play attack animation again (it already played in `Attack` function)
                return;
            }
            // Play attack animation at client
            AttackRoutine(simulateSeed, isLeftHand).Forget();
        }
    }
}
