using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterActionComponentManager))]
    public class DefaultCharacterAttackComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterAttackComponent
    {
        public const float DEFAULT_TOTAL_DURATION = 2f;
        public const float DEFAULT_TRIGGER_DURATION = 1f;
        public const float DEFAULT_STATE_SETUP_DELAY = 1f;

        protected struct AttackState
        {
            public int SimulateSeed;
            public bool IsLeftHand;
        }

        protected readonly List<CancellationTokenSource> _attackCancellationTokenSources = new List<CancellationTokenSource>();
        public bool IsAttacking
        {
            get
            {
                return _simulateState.HasValue;
            }
        }
        public float LastAttackEndTime { get; protected set; }
        public float MoveSpeedRateWhileAttacking { get; protected set; }
        public MovementRestriction MovementRestrictionWhileAttacking { get; protected set; }
        protected float _totalDuration;
        public float AttackTotalDuration { get { return _totalDuration; } set { _totalDuration = value; } }
        protected float[] _triggerDurations;
        public float[] AttackTriggerDurations { get { return _triggerDurations; } set { _triggerDurations = value; } }
        public AnimActionType AnimActionType { get; protected set; }
        public int AnimActionDataId { get; protected set; }
        public IHitRegistrationManager HitRegistrationManager { get { return BaseGameNetworkManager.Singleton.HitRegistrationManager; } }

        public bool doNotRandomAnimation;
        public float animationResetDelay = 2f;

        protected CharacterActionComponentManager _manager;
        protected int _lastAttackAnimationIndex = 0;
        protected int _lastAttackDataId = 0;
        // Network data sending
        protected int _clientNextSeed;
        protected AttackState? _clientState;
        protected AttackState? _serverState;
        protected AttackState? _simulateState;

        public override void EntityStart()
        {
            _manager = GetComponent<CharacterActionComponentManager>();
        }

        protected virtual void SetAttackActionStates(AnimActionType animActionType, int animActionDataId, AttackState simulateState)
        {
            ClearAttackStates();
            AnimActionType = animActionType;
            AnimActionDataId = animActionDataId;
            _simulateState = simulateState;
        }

        public virtual void ClearAttackStates()
        {
            _simulateState = null;
        }

        protected virtual async UniTaskVoid AttackRoutine(long peerTimestamp, AttackState simulateState)
        {
            int simulateSeed = (int)peerTimestamp;
            bool isLeftHand = simulateState.IsLeftHand;
            if (simulateState.SimulateSeed == 0)
                simulateState.SimulateSeed = simulateSeed;
            else
                simulateSeed = simulateState.SimulateSeed;

            // Prepare time
            float time = Time.unscaledTime;
            float deltaTime = Time.unscaledDeltaTime;

            // Prepare required data and get weapon data
            Entity.GetAttackingData(
                ref isLeftHand,
                out AnimActionType animActionType,
                out int animActionDataId,
                out CharacterItem weapon);

            // Get playing animation index
            int randomMax = 1;
            switch (animActionType)
            {
                case AnimActionType.AttackLeftHand:
                    randomMax = Entity.CharacterModel.GetLeftHandAttackRandomMax(animActionDataId);
                    break;
                case AnimActionType.AttackRightHand:
                    randomMax = Entity.CharacterModel.GetRightHandAttackRandomMax(animActionDataId);
                    break;
            }
            if (time - LastAttackEndTime > animationResetDelay || _lastAttackDataId != animActionDataId)
                _lastAttackAnimationIndex = 0;
            int animationIndex = _lastAttackAnimationIndex++;
            if (!doNotRandomAnimation)
                animationIndex = GenericUtils.RandomInt(simulateSeed, 0, randomMax);
            if (_lastAttackAnimationIndex >= randomMax)
                _lastAttackAnimationIndex = 0;
            _lastAttackDataId = animActionDataId;

            // Prepare required data and get animation data
            Entity.GetAnimationData(
                animActionType,
                animActionDataId,
                animationIndex,
                out float animSpeedRate,
                out _triggerDurations,
                out _totalDuration);

            // Set doing action state at clients and server
            SetAttackActionStates(animActionType, animActionDataId, simulateState);

            // Prepare required data and get damages data
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            DamageInfo damageInfo = Entity.GetWeaponDamageInfo(weaponItem);
            Dictionary<DamageElement, MinMaxFloat> damageAmounts;
            if (isLeftHand && Entity.GetCaches().LeftHandDamages != null)
                damageAmounts = new Dictionary<DamageElement, MinMaxFloat>(Entity.GetCaches().LeftHandDamages);
            else
                damageAmounts = new Dictionary<DamageElement, MinMaxFloat>(Entity.GetCaches().RightHandDamages);


            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileAttacking = Entity.GetMoveSpeedRateWhileAttacking(weaponItem);
            MovementRestrictionWhileAttacking = Entity.GetMovementRestrictionWhileAttacking(weaponItem);

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            animSpeedRate *= Entity.GetAnimSpeedRate(AnimActionType);

            // Last attack end time
            float remainsDuration = DEFAULT_TOTAL_DURATION;
            LastAttackEndTime = time + DEFAULT_TOTAL_DURATION;
            if (_totalDuration >= 0f)
            {
                remainsDuration = _totalDuration;
                LastAttackEndTime = time + (_totalDuration / animSpeedRate);
            }

            if (IsServer)
            {
                // Do something with buffs when attack
                Entity.SkillAndBuffComponent.OnAttack();
            }

            // Prepare cancellation
            CancellationTokenSource attackCancellationTokenSource = new CancellationTokenSource();
            _attackCancellationTokenSources.Add(attackCancellationTokenSource);

            try
            {
                bool tpsModelAvailable = Entity.CharacterModel != null && Entity.CharacterModel.gameObject.activeSelf;
                BaseCharacterModel vehicleModel = Entity.PassengingVehicleModel as BaseCharacterModel;
                bool vehicleModelAvailable = vehicleModel != null;
                bool fpsModelAvailable = IsClient && Entity.FpsModel != null && Entity.FpsModel.gameObject.activeSelf;

                // Play action animation
                if (tpsModelAvailable)
                    Entity.CharacterModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                if (vehicleModelAvailable)
                    vehicleModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);
                if (fpsModelAvailable)
                    Entity.FpsModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, animSpeedRate);

                // Try setup state data (maybe by animation clip events or state machine behaviours), if it was not set up
                if (_triggerDurations == null || _triggerDurations.Length == 0 || _totalDuration < 0f)
                {
                    // Wait some components to setup proper `attackTriggerDurations` and `attackTotalDuration` within `DEFAULT_STATE_SETUP_DELAY`
                    float setupDelayCountDown = DEFAULT_STATE_SETUP_DELAY;
                    do
                    {
                        await UniTask.Yield();
                        setupDelayCountDown -= deltaTime;
                    } while (setupDelayCountDown > 0 && (_triggerDurations == null || _triggerDurations.Length == 0 || _totalDuration < 0f));
                    if (setupDelayCountDown <= 0f)
                    {
                        // Can't setup properly, so try to setup manually to make it still workable
                        remainsDuration = DEFAULT_TOTAL_DURATION - DEFAULT_STATE_SETUP_DELAY;
                        _triggerDurations = new float[1]
                        {
                            DEFAULT_TRIGGER_DURATION,
                        };
                    }
                    else
                    {
                        // Can setup, so set proper `remainsDuration` and `LastAttackEndTime` value
                        remainsDuration = _totalDuration;
                        LastAttackEndTime = time + (_totalDuration / animSpeedRate);
                    }
                }

                // Prepare hit register validation, it will be used later when receive attack start/end events from clients
                if (IsServer && !IsOwnerClientOrOwnedByServer)
                    HitRegistrationManager.PrepareHitRegValidatation(Entity, simulateSeed, _triggerDurations, weaponItem.FireSpread, damageInfo, damageAmounts, weapon, null, 0);

                float tempTriggerDuration;
                for (byte triggerIndex = 0; triggerIndex < _triggerDurations.Length; ++triggerIndex)
                {
                    // Wait until triggger before play special effects
                    tempTriggerDuration = _triggerDurations[triggerIndex];
                    remainsDuration -= tempTriggerDuration;
                    await UniTask.Delay((int)(tempTriggerDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, attackCancellationTokenSource.Token);

                    // Special effects will plays on clients only
                    if (IsClient)
                    {
                        // Play weapon launch special effects
                        if (tpsModelAvailable)
                            Entity.CharacterModel.PlayEquippedWeaponLaunch(isLeftHand);
                        if (fpsModelAvailable)
                            Entity.FpsModel.PlayEquippedWeaponLaunch(isLeftHand);
                        // Play launch sfx
                        AudioClipWithVolumeSettings audioClip = weaponItem.LaunchClip;
                        if (audioClip != null)
                            AudioManager.PlaySfxClipAtAudioSource(audioClip.audioClip, Entity.CharacterModel.GenericAudioSource, audioClip.GetRandomedVolume());
                    }

                    // Get aim position by character's forward
                    AimPosition aimPosition = Entity.AimPosition;

                    // Call on attack to extend attack functionality while attacking
                    bool overrideDefaultAttack = false;
                    foreach (KeyValuePair<BaseSkill, int> skillLevel in Entity.CachedData.Skills)
                    {
                        if (skillLevel.Value <= 0)
                            continue;
                        if (skillLevel.Key.OnAttack(Entity, skillLevel.Value, isLeftHand, weapon, simulateSeed, triggerIndex, damageAmounts, aimPosition))
                            overrideDefaultAttack = true;
                    }

                    // Skip attack function when applied skills (buffs) will override default attack functionality
                    if (!overrideDefaultAttack)
                    {

                        // Trigger attack event
                        Entity.OnAttackRoutine(isLeftHand, weapon, simulateSeed, triggerIndex, damageInfo, damageAmounts, aimPosition);

                        // Apply attack damages
                        if (IsOwnerClientOrOwnedByServer)
                        {
                            // Simulate action at non-owner clients
                            SimulateActionTriggerData simulateData = new SimulateActionTriggerData()
                            {
                                simulateSeed = simulateSeed,
                                triggerIndex = triggerIndex,
                                aimPosition = aimPosition,
                            };
                            RPC(AllSimulateActionTrigger, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, simulateData);
                            ApplyAttack(isLeftHand, weapon, simulateSeed, triggerIndex, damageInfo, damageAmounts, aimPosition);
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
                _attackCancellationTokenSources.Remove(attackCancellationTokenSource);
            }
            // Clear action states at clients and server
            ClearAttackStates();
        }

        protected virtual void ApplyAttack(bool isLeftHand, CharacterItem weapon, int simulateSeed, byte triggerIndex, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, AimPosition aimPosition)
        {
            if (IsServer)
            {
                // Increase damage with ammo damage
                Entity.DecreaseAmmos(weapon, isLeftHand, 1, out Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts);
                damageAmounts = HitRegistrationManager.IncreasePreparedDamageAmounts(Entity, simulateSeed, increaseDamageAmounts);
            }

            byte fireSpread = 0;
            Vector3 fireStagger = Vector3.zero;
            if (weapon != null && weapon.GetWeaponItem() != null)
            {
                // For monsters, their weapon can be null so have to avoid null exception
                fireSpread = weapon.GetWeaponItem().FireSpread;
                fireStagger = weapon.GetWeaponItem().FireStagger;
            }

            for (byte spreadIndex = 0; spreadIndex < fireSpread + 1; ++spreadIndex)
            {
                damageInfo.LaunchDamageEntity(
                    Entity,
                    isLeftHand,
                    weapon,
                    simulateSeed,
                    triggerIndex,
                    spreadIndex,
                    fireStagger,
                    damageAmounts,
                    null,
                    0,
                    aimPosition,
                    OnAttackOriginPrepared,
                    OnAttackHit);
            }
        }

        protected virtual void OnAttackOriginPrepared(int simulateSeed, byte triggerIndex, byte spreadIndex, Vector3 position, Vector3 direction, Quaternion rotation)
        {
            if (!IsServer || IsOwnerClientOrOwnedByServer)
                return;
            HitRegistrationManager.PrepareHitRegOrigin(Entity, simulateSeed, triggerIndex, spreadIndex, position, direction);
        }

        protected virtual void OnAttackHit(int simulateSeed, byte triggerIndex, byte spreadIndex, uint objectId, byte hitboxIndex, Vector3 hitPoint)
        {
            if (IsServer || !IsOwnerClient)
                return;
            HitRegistrationManager.PrepareToRegister(simulateSeed, triggerIndex, spreadIndex, objectId, hitboxIndex, hitPoint);
        }

        [AllRpc]
        protected void AllSimulateActionTrigger(SimulateActionTriggerData data)
        {
            if (IsOwnerClientOrOwnedByServer)
                return;
            if (!_simulateState.HasValue)
                return;
            if (data.simulateSeed != _simulateState.Value.SimulateSeed)
                return;
            bool isLeftHand = _simulateState.Value.IsLeftHand;
            CharacterItem weapon = Entity.GetAvailableWeapon(ref isLeftHand);
            DamageInfo damageInfo = Entity.GetWeaponDamageInfo(weapon.GetWeaponItem());
            Dictionary<DamageElement, MinMaxFloat> damageAmounts;
            if (isLeftHand && Entity.GetCaches().LeftHandDamages != null)
                damageAmounts = new Dictionary<DamageElement, MinMaxFloat>(Entity.GetCaches().LeftHandDamages);
            else
                damageAmounts = new Dictionary<DamageElement, MinMaxFloat>(Entity.GetCaches().RightHandDamages);
            ApplyAttack(isLeftHand, weapon, data.simulateSeed, data.triggerIndex, damageInfo, damageAmounts, data.aimPosition);
        }

        public virtual void CancelAttack()
        {
            for (int i = _attackCancellationTokenSources.Count - 1; i >= 0; --i)
            {
                if (!_attackCancellationTokenSources[i].IsCancellationRequested)
                    _attackCancellationTokenSources[i].Cancel();
                _attackCancellationTokenSources.RemoveAt(i);
            }
        }

        public virtual void Attack(bool isLeftHand)
        {
            if (!IsServer && IsOwnerClient)
            {
                // Prepare state data which will be sent to server
                _clientState = new AttackState()
                {
                    IsLeftHand = isLeftHand,
                };
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Attack immediately at server
                ProceedAttackStateAtServer(0, isLeftHand);
            }
        }

        protected virtual void ProceedAttackStateAtServer(long peerTimestamp, bool isLeftHand)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!_manager.IsAcceptNewAction())
                return;
            // Speed hack avoidance
            if (Time.unscaledTime - LastAttackEndTime < -0.2f)
                return;
            _manager.ActionAccepted();
            // Prepare state data which will be sent to clients
            _serverState = new AttackState()
            {
                SimulateSeed = (int)peerTimestamp,
                IsLeftHand = isLeftHand,
            };
#endif
        }

        public virtual bool WriteClientAttackState(long writeTimestamp, NetDataWriter writer)
        {
            if (_clientState.HasValue)
            {
                // Simulate attacking at client
                AttackRoutine(writeTimestamp, _clientState.Value).Forget();
                // Send input to server
                writer.Put(_clientState.Value.IsLeftHand);
                // Clear Input
                _clientState = null;
                return true;
            }
            return false;
        }

        public virtual bool WriteServerAttackState(long writeTimestamp, NetDataWriter writer)
        {
            if (_serverState.HasValue)
            {
                // Simulate attacking at server
                AttackRoutine(writeTimestamp, _serverState.Value).Forget();
                // Send input to client
                writer.Put(_serverState.Value.IsLeftHand);
                // Clear Input
                _serverState = null;
                return true;
            }
            return false;
        }

        public virtual void ReadClientAttackStateAtServer(long peerTimestamp, NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            ProceedAttackStateAtServer(peerTimestamp, isLeftHand);
        }

        public virtual void ReadServerAttackStateAtClient(long peerTimestamp, NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            if (IsServer || IsOwnerClient)
            {
                // Don't play attacking animation again (it already done in `WriteServerAttackState` function)
                return;
            }
            AttackState simulateState = new AttackState()
            {
                IsLeftHand = isLeftHand,
            };
            AttackRoutine(peerTimestamp, simulateState).Forget();
        }
    }
}
