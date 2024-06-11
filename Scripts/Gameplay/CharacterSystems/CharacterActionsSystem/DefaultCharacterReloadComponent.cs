using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterActionComponentManager))]
    public class DefaultCharacterReloadComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterReloadComponent, ICharacterActionComponentPreparation
    {
        protected readonly List<CancellationTokenSource> _reloadCancellationTokenSources = new List<CancellationTokenSource>();
        public int ReloadingAmmoDataId { get; protected set; }
        public int ReloadingAmmoAmount { get; protected set; }
        public bool IsReloading { get; protected set; }
        public float LastReloadEndTime { get; protected set; }
        protected bool _skipMovementValidation;
        public bool IsSkipMovementValidationWhileReloading { get { return _skipMovementValidation; } set { _skipMovementValidation = value; } }
        protected bool _shouldUseRootMotion;
        public bool IsUseRootMotionWhileReloading { get { return _shouldUseRootMotion; } protected set { _shouldUseRootMotion = value; } }
        public float MoveSpeedRateWhileReloading { get; protected set; }
        public MovementRestriction MovementRestrictionWhileReloading { get; protected set; }
        protected float _totalDuration;
        public float ReloadTotalDuration { get { return _totalDuration; } set { _totalDuration = value; } }
        protected float[] _triggerDurations;
        public float[] ReloadTriggerDurations { get { return _triggerDurations; } set { _triggerDurations = value; } }
        public AnimActionType AnimActionType { get; protected set; }

        protected CharacterActionComponentManager _manager;
        protected float _remainsDurationWithoutSpeedRate = 0f;
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
            CancelReload();
            ClearReloadStates();
            _manager = null;
            _entityIsPlayer = false;
            _playerCharacterEntity = null;
        }

        protected virtual void SetReloadActionStates(AnimActionType animActionType, int reloadingAmmoDataId, int reloadingAmmoAmount)
        {
            ClearReloadStates();
            AnimActionType = animActionType;
            ReloadingAmmoDataId = reloadingAmmoDataId;
            ReloadingAmmoAmount = reloadingAmmoAmount;
            IsReloading = true;
        }

        public virtual void ClearReloadStates()
        {
            ReloadingAmmoAmount = 0;
            IsReloading = false;
        }

        public void OnPrepareActionDurations(float[] triggerDurations, float totalDuration, float remainsDurationWithoutSpeedRate, float endTime)
        {
            _triggerDurations = triggerDurations;
            _totalDuration = totalDuration;
            _remainsDurationWithoutSpeedRate = remainsDurationWithoutSpeedRate;
            LastReloadEndTime = endTime;
        }

        protected virtual async UniTaskVoid ReloadRoutine(bool isLeftHand, int reloadingAmmoDataId, int reloadingAmmoAmount)
        {
            // Prepare cancellation
            CancellationTokenSource reloadCancellationTokenSource = new CancellationTokenSource();
            _reloadCancellationTokenSources.Add(reloadCancellationTokenSource);

            // Prepare requires data and get weapon data
            Entity.GetReloadingData(
                ref isLeftHand,
                out AnimActionType animActionType,
                out int animActionDataId,
                out CharacterItem weapon);

            // Prepare requires data and get animation data
            Entity.GetAnimationData(
                animActionType,
                animActionDataId,
                0,
                out float animSpeedRate,
                out _triggerDurations,
                out _totalDuration);

            // Set doing action state at clients and server
            SetReloadActionStates(animActionType, reloadingAmmoDataId, reloadingAmmoAmount);

            // Prepare requires data and get damages data
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            if (weaponItem.ReloadDuration > 0)
                _totalDuration = weaponItem.ReloadDuration;

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileReloading = Entity.GetMoveSpeedRateWhileReloading(weaponItem);
            MovementRestrictionWhileReloading = Entity.GetMovementRestrictionWhileReloading(weaponItem);

            try
            {
                bool tpsModelAvailable = Entity.CharacterModel != null && Entity.CharacterModel.gameObject.activeSelf;
                BaseCharacterModel vehicleModel = Entity.PassengingVehicleModel as BaseCharacterModel;
                bool vehicleModelAvailable = vehicleModel != null;
                bool fpsModelAvailable = IsClient && Entity.FpsModel != null && Entity.FpsModel.gameObject.activeSelf;

                // Prepare end time
                LastReloadEndTime = CharacterActionComponentManager.PrepareActionDefaultEndTime(_totalDuration, animSpeedRate);

                // Play animation
                if (tpsModelAvailable)
                    Entity.CharacterModel.PlayActionAnimation(AnimActionType, animActionDataId, 0, out _skipMovementValidation, out _shouldUseRootMotion);
                if (vehicleModelAvailable)
                    vehicleModel.PlayActionAnimation(AnimActionType, animActionDataId, 0, out _skipMovementValidation, out _shouldUseRootMotion);
                if (fpsModelAvailable)
                    Entity.FpsModel.PlayActionAnimation(AnimActionType, animActionDataId, 0, out _, out _);

                // Special effects will plays on clients only
                if (IsClient)
                {
                    // Play weapon reload special effects
                    if (tpsModelAvailable)
                        Entity.CharacterModel.PlayEquippedWeaponReload(isLeftHand);
                    if (fpsModelAvailable)
                        Entity.FpsModel.PlayEquippedWeaponReload(isLeftHand);
                    // Play reload sfx
                    AudioClipWithVolumeSettings audioClip = weaponItem.ReloadClip;
                    if (audioClip != null)
                        AudioManager.PlaySfxClipAtAudioSource(audioClip.audioClip, Entity.CharacterModel.GenericAudioSource, audioClip.GetRandomedVolume());
                }

                // Try setup state data (maybe by animation clip events or state machine behaviours), if it was not set up
                await _manager.PrepareActionDurations(this, _triggerDurations, _totalDuration, 0f, animSpeedRate, reloadCancellationTokenSource.Token);

                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogReloadStart(_playerCharacterEntity, _triggerDurations);

                bool reloaded = false;
                float tempTriggerDuration;
                for (byte triggerIndex = 0; triggerIndex < _triggerDurations.Length; ++triggerIndex)
                {
                    // Wait until triggger before reload ammo
                    tempTriggerDuration = _triggerDurations[triggerIndex];
                    _remainsDurationWithoutSpeedRate -= tempTriggerDuration;
                    await UniTask.Delay((int)(tempTriggerDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.FixedUpdate, reloadCancellationTokenSource.Token);

                    // Special effects will plays on clients only
                    if (IsClient)
                    {
                        // Play weapon reload special effects
                        if (tpsModelAvailable)
                            Entity.CharacterModel.PlayEquippedWeaponReloaded(isLeftHand);
                        if (fpsModelAvailable)
                            Entity.FpsModel.PlayEquippedWeaponReloaded(isLeftHand);

                        // Play reload sfx
                        AudioClipWithVolumeSettings audioClip = weaponItem.ReloadedClip;
                        if (audioClip != null)
                            AudioManager.PlaySfxClipAtAudioSource(audioClip.audioClip, Entity.CharacterModel.GenericAudioSource, audioClip.GetRandomedVolume());
                    }

                    await UniTask.Yield(reloadCancellationTokenSource.Token);

                    // Reload / Fill ammo
                    if (!reloaded)
                    {
                        reloaded = true;
                        ActionTrigger(reloadingAmmoDataId, reloadingAmmoAmount, triggerIndex, isLeftHand, weapon);
                    }

                    if (_remainsDurationWithoutSpeedRate <= 0f)
                    {
                        // Stop trigger animations loop
                        break;
                    }
                }

                if (_remainsDurationWithoutSpeedRate > 0f)
                {
                    // Wait until animation ends to stop actions
                    await UniTask.Delay((int)(_remainsDurationWithoutSpeedRate / animSpeedRate * 1000f), true, PlayerLoopTiming.FixedUpdate, reloadCancellationTokenSource.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Catch the cancellation
                LastReloadEndTime = Time.unscaledTime;
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogReloadInterrupt(_playerCharacterEntity);
            }
            catch (System.Exception ex)
            {
                // Other errors
                Logging.LogException(LogTag, ex);
            }
            finally
            {
                reloadCancellationTokenSource.Dispose();
                _reloadCancellationTokenSources.Remove(reloadCancellationTokenSource);
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogReloadEnd(_playerCharacterEntity);
            }
            // Clear action states at clients and server
            ClearReloadStates();
        }

        protected virtual void ActionTrigger(int reloadingAmmoDataId, int reloadingAmmoAmount, byte triggerIndex, bool isLeftHand, CharacterItem weapon)
        {
            if (!IsServer)
                return;
            if (!Entity.DecreaseItems(reloadingAmmoDataId, reloadingAmmoAmount))
            {
                if (_entityIsPlayer && IsServer)
                    GameInstance.ServerLogHandlers.LogReloadTriggerFail(_playerCharacterEntity, triggerIndex, ActionTriggerFailReasons.NotEnoughResources);
                return;
            }
            if (weapon.ammo > 0 && weapon.ammoDataId != reloadingAmmoDataId)
            {
                // If ammo that stored in the weapon is difference
                // Then it will return ammo in the weapon, and replace amount with the new one
                Entity.IncreaseItems(CharacterItem.Create(weapon.ammoDataId, 1, weapon.ammo));
                weapon.ammo = 0;
            }
            Entity.FillEmptySlots();
            weapon.ammoDataId = reloadingAmmoDataId;
            weapon.ammo += reloadingAmmoAmount;
            EquipWeapons equipWeapons = Entity.EquipWeapons;
            if (isLeftHand)
                equipWeapons.leftHand = weapon;
            else
                equipWeapons.rightHand = weapon;
            Entity.EquipWeapons = equipWeapons;
            if (_entityIsPlayer && IsServer)
                GameInstance.ServerLogHandlers.LogReloadTrigger(_playerCharacterEntity, triggerIndex);
        }

        public virtual void CancelReload()
        {
            for (int i = _reloadCancellationTokenSources.Count - 1; i >= 0; --i)
            {
                if (!_reloadCancellationTokenSources[i].IsCancellationRequested)
                    _reloadCancellationTokenSources[i].Cancel();
                _reloadCancellationTokenSources.RemoveAt(i);
            }
        }

        public virtual void Reload(bool isLeftHand)
        {
            if (!IsServer && IsOwnerClient)
            {
                RPC(CmdReload, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand);
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Reload immediately at server
                ProceedCmdReload(isLeftHand);
            }
        }

        [ServerRpc]
        protected void CmdReload(bool isLeftHand)
        {
            ProceedCmdReload(isLeftHand);
        }

        protected void ProceedCmdReload(bool isLeftHand)
        {
#if UNITY_EDITOR || !EXCLUDE_SERVER_CODES
            if (!_manager.IsAcceptNewAction())
                return;

            CharacterItem reloadingWeapon = isLeftHand ? Entity.EquipWeapons.leftHand : Entity.EquipWeapons.rightHand;
            if (reloadingWeapon.IsEmptySlot())
            {
                // Invalid item data
                return;
            }
            IWeaponItem reloadingWeaponItem = reloadingWeapon.GetWeaponItem();
            if (reloadingWeaponItem == null || reloadingWeaponItem.AmmoCapacity <= 0)
            {
                // This is not an items that have something like gun's magazine, it might be bow or crossbow :P
                return;
            }
            bool hasAmmoType = reloadingWeaponItem.WeaponType.AmmoType != null;
            bool hasAmmoItems = reloadingWeaponItem.AmmoItemIds.Count > 0;
            if (!hasAmmoType && !hasAmmoItems)
            {
                // This is not an items that have something like gun's magazine, it might be bow or crossbow :P
                return;
            }

            if (reloadingWeapon.IsAmmoFull())
            {
                // Full, don't reload
                return;
            }

            // Prepare reload data
            int reloadingAmmoDataId = 0;
            int inventoryAmount = 0;
            if (hasAmmoItems)
            {
                // Looking for items in inventory
                CharacterItem tempCharacterItem;
                for (int i = 0; i < Entity.NonEquipItems.Count; ++i)
                {
                    tempCharacterItem = Entity.NonEquipItems[i];
                    if (tempCharacterItem.IsEmptySlot())
                        continue;
                    if (!reloadingWeaponItem.AmmoItemIds.Contains(tempCharacterItem.dataId))
                        continue;
                    if (reloadingAmmoDataId == 0)
                        reloadingAmmoDataId = tempCharacterItem.dataId;
                    if (reloadingAmmoDataId == tempCharacterItem.dataId)
                        inventoryAmount += tempCharacterItem.amount;
                }
            }
            if (hasAmmoType && inventoryAmount <= 0)
            {
                inventoryAmount = Entity.CountAmmos(reloadingWeaponItem.WeaponType.AmmoType, out reloadingAmmoDataId);
            }

            int ammoCapacity = reloadingWeaponItem.AmmoCapacity;
            if (GameInstance.Items.TryGetValue(reloadingAmmoDataId, out BaseItem tempItem) &&
                tempItem.OverrideAmmoCapacity > 0)
            {
                // Override capacity by the item
                ammoCapacity = tempItem.OverrideAmmoCapacity;
            }

            int reloadingAmmoAmount = 0;
            if (reloadingWeapon.ammoDataId != reloadingAmmoDataId)
            {
                // If ammo that stored in the weapon is difference
                // Then it will return ammo in the weapon, and replace amount with the new one
                reloadingAmmoAmount = ammoCapacity;
            }
            else
            {
                reloadingAmmoAmount = ammoCapacity - reloadingWeapon.ammo;
            }

            if (inventoryAmount < reloadingAmmoAmount)
            {
                // Ammo in inventory less than reloading amount, so use amount of ammo in inventory
                reloadingAmmoAmount = inventoryAmount;
            }

            if (reloadingAmmoAmount <= 0)
            {
                // No ammo to reload
                return;
            }

            _manager.ActionAccepted();
            ReloadRoutine(isLeftHand, reloadingAmmoDataId, reloadingAmmoAmount).Forget();
            RPC(RpcReload, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand, reloadingAmmoDataId, reloadingAmmoAmount);
#endif
        }

        [AllRpc]
        protected void RpcReload(bool isLeftHand, int reloadingAmmoDataId, int reloadingAmmoAmount)
        {
            if (IsServer || IsOwnerClient)
            {
                // Don't play reloading animation again
                return;
            }
            ReloadRoutine(isLeftHand, reloadingAmmoDataId, reloadingAmmoAmount).Forget();
        }
    }
}
