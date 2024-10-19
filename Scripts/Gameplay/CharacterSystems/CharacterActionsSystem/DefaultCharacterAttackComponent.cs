using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterActionComponentManager))]
    public class DefaultCharacterAttackComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterAttackComponent, ICharacterActionComponentPreparation
    {
        public const float RATE_OF_FIRE_BASE = 60f;

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
        protected bool _skipMovementValidation;
        public bool IsSkipMovementValidationWhileAttacking { get { return _skipMovementValidation; } set { _skipMovementValidation = value; } }
        protected bool _shouldUseRootMotion;
        public bool IsUseRootMotionWhileAttacking { get { return _shouldUseRootMotion; } protected set { _shouldUseRootMotion = value; } }
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
        protected float _remainsDurationWithoutSpeedRate = 0f;
        // Network data sending
        protected AttackState? _simulateState;
        // Logging data
        bool _entityIsPlayer = false;
        BasePlayerCharacterEntity _playerCharacterEntity = null;

        public override void EntityStart()
        {
            _manager = GetComponent<CharacterActionComponentManager>();
            if (Entity is BasePlayerCharacterEntity)
            {
                _entityIsPlayer = true;
                _playerCharacterEntity = Entity as BasePlayerCharacterEntity;
            }
        }

        public override void EntityOnDestroy()
        {
            CancelAttack();
            ClearAttackStates();
            _manager = null;
            _entityIsPlayer = false;
            _playerCharacterEntity = null;
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

        public void OnPrepareActionDurations(float[] triggerDurations, float totalDuration, float remainsDurationWithoutSpeedRate, float endTime)
        {
            _triggerDurations = triggerDurations;
            _totalDuration = totalDuration;
            _remainsDurationWithoutSpeedRate = remainsDurationWithoutSpeedRate;
            LastAttackEndTime = endTime;
        }

        protected virtual async UniTaskVoid AttackRoutine(long peerTimestamp, AttackState simulateState)
        {
            int simulateSeed = GetSimulateSeed(peerTimestamp);
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
                out CharacterItem weapon,
                out DamageInfo damageInfo);

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
            Dictionary<DamageElement, MinMaxFloat> baseDamageAmounts;
            if (isLeftHand && Entity.CachedData.LeftHandDamages != null)
                baseDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>(Entity.CachedData.LeftHandDamages);
            else
                baseDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>(Entity.CachedData.RightHandDamages);

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileAttacking = Entity.GetMoveSpeedRateWhileAttacking(weaponItem);
            MovementRestrictionWhileAttacking = Entity.GetMovementRestrictionWhileAttacking(weaponItem);

            // Get play speed multiplier will use it to play animation faster or slower based on attack speed stats
            animSpeedRate *= Entity.GetAnimSpeedRate(AnimActionType);

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

                // Prepare end time
                LastAttackEndTime = CharacterActionComponentManager.PrepareActionDefaultEndTime(_totalDuration, animSpeedRate);

                // Play action animation
                if (weaponItem.DoRecoilingAsAttackAnimation)
                {

                    _totalDuration = Entity.CharacterModel.CacheAttackRecoiler?.DefaultRecoilDuration ?? 1f;
                    _triggerDurations = new float[] { 0f };
                    if (tpsModelAvailable)
                        Entity.CharacterModel.CacheAttackRecoiler?.PlayRecoiling();
                    if (vehicleModelAvailable)
                        vehicleModel.CacheAttackRecoiler?.PlayRecoiling();
                    if (fpsModelAvailable)
                        Entity.FpsModel.CacheAttackRecoiler?.PlayRecoiling();
                }
                else
                {
                    if (tpsModelAvailable)
                        Entity.CharacterModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, out _skipMovementValidation, out _shouldUseRootMotion, animSpeedRate);
                    if (vehicleModelAvailable)
                        vehicleModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, out _skipMovementValidation, out _shouldUseRootMotion, animSpeedRate);
                    if (fpsModelAvailable)
                        Entity.FpsModel.PlayActionAnimation(AnimActionType, AnimActionDataId, animationIndex, out _, out _, animSpeedRate);
                }

                if (weaponItem.RateOfFire > 0)
                {
                    _totalDuration = RATE_OF_FIRE_BASE / weaponItem.RateOfFire;
                    _triggerDurations = new float[] { 0f };
                }

                // Try setup state data (maybe by animation clip events or state machine behaviours), if it was not set up
                await _manager.PrepareActionDurations(this, _triggerDurations, _totalDuration, 0f, animSpeedRate, attackCancellationTokenSource.Token);

                // Prepare damage amounts
                List<Dictionary<DamageElement, MinMaxFloat>> damageAmounts = Entity.PrepareDamageAmounts(weapon, isLeftHand, baseDamageAmounts, _triggerDurations.Length, 1);

                // Prepare hit register validation, it will be used later when receive attack start/end events from clients
                if ((IsServer && !IsOwnerClient) || !IsOwnedByServer)
                    HitRegistrationManager.PrepareHitRegValidation(Entity, simulateSeed, _triggerDurations, weaponItem.FireSpreadAmount, damageInfo, damageAmounts, isLeftHand, weapon, null, 0);
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogAttackStart(_playerCharacterEntity, simulateSeed, _triggerDurations, weaponItem.FireSpreadAmount, isLeftHand, weapon);

                float tempTriggerDuration;
                for (byte triggerIndex = 0; triggerIndex < _triggerDurations.Length; ++triggerIndex)
                {
                    // Wait until triggger before play special effects
                    tempTriggerDuration = _triggerDurations[triggerIndex];
                    _remainsDurationWithoutSpeedRate -= tempTriggerDuration;
                    await UniTask.Delay((int)(tempTriggerDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.FixedUpdate, attackCancellationTokenSource.Token);

                    // Special effects will plays on clients only
                    if (IsClient)
                    {
                        // Play weapon launch special effects
                        if (tpsModelAvailable)
                            Entity.CharacterModel.PlayEquippedWeaponLaunch(isLeftHand);
                        if (fpsModelAvailable)
                            Entity.FpsModel.PlayEquippedWeaponLaunch(isLeftHand);
                        // Play launch sfx
                        AudioClipWithVolumeSettings launchClip = weaponItem.LaunchClip;
                        if (Entity.GetCaches().TryGetWeaponAbility(isLeftHand, LaunchSfxWeaponAbility.KEY, out BaseWeaponAbility ability) && ability is LaunchSfxWeaponAbility launchSfxAbility)
                            launchClip = launchSfxAbility.LaunchClip;
                        launchClip?.Play(Entity.CharacterModel.GenericAudioSource);
                    }

                    await UniTask.Yield(attackCancellationTokenSource.Token);
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
                        if (IsServer)
                        {
                            if (!Entity.DecreaseAmmos(weapon, isLeftHand, 1, out _))
                                continue;
                            if (!IsOwnerClient && !IsOwnedByServer)
                                continue;
                            RPC(RpcSimulateActionTrigger, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, new SimulateActionTriggerData()
                            {
                                simulateSeed = simulateSeed,
                                triggerIndex = triggerIndex,
                                aimPosition = aimPosition,
                            });
                            ApplyAttack(isLeftHand, weapon, simulateSeed, triggerIndex, damageInfo, damageAmounts, aimPosition);
                        }
                        else if (IsOwnerClient)
                        {
                            RPC(CmdSimulateActionTrigger, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, new SimulateActionTriggerData()
                            {
                                simulateSeed = simulateSeed,
                                triggerIndex = triggerIndex,
                                aimPosition = aimPosition,
                            });
                            ApplyAttack(isLeftHand, weapon, simulateSeed, triggerIndex, damageInfo, damageAmounts, aimPosition);
                        }
                    }

                    if (_remainsDurationWithoutSpeedRate <= 0f)
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

                if (_remainsDurationWithoutSpeedRate > 0f)
                {
                    // Wait until animation ends to stop actions
                    await UniTask.Delay((int)(_remainsDurationWithoutSpeedRate / animSpeedRate * 1000f), true, PlayerLoopTiming.FixedUpdate, attackCancellationTokenSource.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Catch the cancellation
                LastAttackEndTime = Time.unscaledTime;
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogAttackInterrupt(_playerCharacterEntity, simulateSeed);
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
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogAttackEnd(_playerCharacterEntity, simulateSeed);
            }
            await UniTask.Yield();
            // Clear action states at clients and server
            ClearAttackStates();
        }

        [ServerRpc]
        protected void CmdSimulateActionTrigger(SimulateActionTriggerData data)
        {
            HitValidateData validateData = HitRegistrationManager.GetHitValidateData(Entity, data.simulateSeed);
            if (validateData == null)
            {
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogAttackTriggerFail(_playerCharacterEntity, data.simulateSeed, data.triggerIndex, ActionTriggerFailReasons.NoValidateData);
                return;
            }
            if (data.triggerIndex >= validateData.DamageAmounts.Count)
            {
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogAttackTriggerFail(_playerCharacterEntity, data.simulateSeed, data.triggerIndex, ActionTriggerFailReasons.NotEnoughResources);
                return;
            }
            RPC(RpcSimulateActionTrigger, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, data);
            ApplyAttack(validateData.IsLeftHand, validateData.Weapon, data.simulateSeed, data.triggerIndex, validateData.DamageInfo, validateData.DamageAmounts, data.aimPosition);
            if (_entityIsPlayer && IsServer)
                GameInstance.ServerLogHandlers.LogAttackTrigger(_playerCharacterEntity, data.simulateSeed, data.triggerIndex);
        }

        [AllRpc]
        protected void RpcSimulateActionTrigger(SimulateActionTriggerData data)
        {
            if (IsServer)
                return;
            if (IsOwnerClientOrOwnedByServer)
                return;
            HitValidateData validateData = HitRegistrationManager.GetHitValidateData(Entity, data.simulateSeed);
            if (validateData == null)
                return;
            ApplyAttack(validateData.IsLeftHand, validateData.Weapon, data.simulateSeed, data.triggerIndex, validateData.DamageInfo, validateData.DamageAmounts, data.aimPosition);
        }

        protected virtual async void ApplyAttack(bool isLeftHand, CharacterItem weapon, int simulateSeed, byte triggerIndex, DamageInfo damageInfo, List<Dictionary<DamageElement, MinMaxFloat>> damageAmounts, AimPosition aimPosition)
        {
            if (triggerIndex >= damageAmounts.Count)
            {
                // No damage applied (may not have enough ammo)
                return;
            }

            byte fireSpreadAmount = 0;
            Vector3 fireSpreadRange = Vector3.zero;
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            if (weaponItem != null)
            {
                // For monsters, their weapon can be null so have to avoid null exception
                fireSpreadAmount = weaponItem.FireSpreadAmount;
                fireSpreadRange = weaponItem.FireSpreadRange;
            }
            // Make sure it won't increase damage to the wrong collction
            for (byte spreadIndex = 0; spreadIndex < fireSpreadAmount + 1; ++spreadIndex)
            {
                await damageInfo.LaunchDamageEntity(
                    Entity,
                    isLeftHand,
                    weapon,
                    simulateSeed,
                    triggerIndex,
                    spreadIndex,
                    fireSpreadRange,
                    damageAmounts,
                    null,
                    0,
                    aimPosition);
            }
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
            long timestamp = Manager.Timestamp;
            if (!IsServer && IsOwnerClient)
            {
                ProceedAttack(timestamp, isLeftHand);
                RPC(CmdAttack, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, timestamp, isLeftHand);
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                PreceedCmdAttack(timestamp, isLeftHand);
            }
        }

        [ServerRpc]
        protected void CmdAttack(long peerTimestamp, bool isLeftHand)
        {
            PreceedCmdAttack(peerTimestamp, isLeftHand);
        }

        protected void PreceedCmdAttack(long peerTimestamp, bool isLeftHand)
        {
            if (!_manager.IsAcceptNewAction())
                return;
            _manager.ActionAccepted();
            ProceedAttack(peerTimestamp, isLeftHand);
            RPC(RpcAttack, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, peerTimestamp, isLeftHand);
        }

        [AllRpc]
        protected void RpcAttack(long peerTimestamp, bool isLeftHand)
        {
            if (IsServer || IsOwnerClient)
            {
                // Don't play attacking animation again
                return;
            }
            ProceedAttack(peerTimestamp, isLeftHand);
        }

        protected void ProceedAttack(long peerTimestamp, bool isLeftHand)
        {
            AttackState simulateState = new AttackState()
            {
                SimulateSeed = GetSimulateSeed(peerTimestamp),
                IsLeftHand = isLeftHand,
            };
            AttackRoutine(peerTimestamp, simulateState).Forget();
        }

        private int GetSimulateSeed(long timestamp)
        {
            return (int)(timestamp % 16384);
        }
    }
}
