using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterActionComponentManager))]
    public class DefaultCharacterReloadComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterReloadComponent
    {
        public const float DEFAULT_TOTAL_DURATION = 2f;
        public const float DEFAULT_TRIGGER_DURATION = 1f;
        public const float DEFAULT_STATE_SETUP_DELAY = 1f;

        protected struct ReloadState
        {
            public bool IsLeftHand;
            public int ReloadingAmmoAmount;
        }

        protected readonly List<CancellationTokenSource> _reloadCancellationTokenSources = new List<CancellationTokenSource>();
        public int ReloadingAmmoAmount { get; protected set; }
        public bool IsReloading { get; protected set; }
        public float LastReloadEndTime { get; protected set; }
        public float MoveSpeedRateWhileReloading { get; protected set; }
        public MovementRestriction MovementRestrictionWhileReloading { get; protected set; }
        protected float _totalDuration;
        public float ReloadTotalDuration { get { return _totalDuration; } set { _totalDuration = value; } }
        protected float[] _triggerDurations;
        public float[] ReloadTriggerDurations { get { return _triggerDurations; } set { _triggerDurations = value; } }
        public AnimActionType AnimActionType { get; protected set; }

        protected CharacterActionComponentManager _manager;
        // Network data sending
        protected ReloadState? _clientState;
        protected ReloadState? _serverState;

        public override void EntityStart()
        {
            _manager = GetComponent<CharacterActionComponentManager>();
        }

        protected virtual void SetReloadActionStates(AnimActionType animActionType, int reloadingAmmoAmount)
        {
            ClearReloadStates();
            AnimActionType = animActionType;
            ReloadingAmmoAmount = reloadingAmmoAmount;
            IsReloading = true;
        }

        public virtual void ClearReloadStates()
        {
            ReloadingAmmoAmount = 0;
            IsReloading = false;
        }

        protected async UniTaskVoid ReloadRoutine(bool isLeftHand, int reloadingAmmoAmount)
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
            SetReloadActionStates(animActionType, reloadingAmmoAmount);

            // Prepare requires data and get damages data
            IWeaponItem weaponItem = weapon.GetWeaponItem();

            // Calculate move speed rate while doing action at clients and server
            MoveSpeedRateWhileReloading = Entity.GetMoveSpeedRateWhileReloading(weaponItem);
            MovementRestrictionWhileReloading = Entity.GetMovementRestrictionWhileReloading(weaponItem);

            // Last attack end time
            float remainsDuration = DEFAULT_TOTAL_DURATION;
            LastReloadEndTime = Time.unscaledTime + DEFAULT_TOTAL_DURATION;
            if (_totalDuration >= 0f)
            {
                remainsDuration = _totalDuration;
                LastReloadEndTime = Time.unscaledTime + (_totalDuration / animSpeedRate);
            }

            try
            {
                bool tpsModelAvailable = Entity.CharacterModel != null && Entity.CharacterModel.gameObject.activeSelf;
                BaseCharacterModel vehicleModel = Entity.PassengingVehicleModel as BaseCharacterModel;
                bool vehicleModelAvailable = vehicleModel != null;
                bool fpsModelAvailable = IsClient && Entity.FpsModel != null && Entity.FpsModel.gameObject.activeSelf;

                // Play animation
                if (tpsModelAvailable)
                    Entity.CharacterModel.PlayActionAnimation(AnimActionType, animActionDataId, 0);
                if (vehicleModelAvailable)
                    vehicleModel.PlayActionAnimation(AnimActionType, animActionDataId, 0);
                if (fpsModelAvailable)
                    Entity.FpsModel.PlayActionAnimation(AnimActionType, animActionDataId, 0);

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
                if (_triggerDurations == null || _triggerDurations.Length == 0 || _totalDuration < 0f)
                {
                    // Wait some components to setup proper `attackTriggerDurations` and `attackTotalDuration` within `DEFAULT_STATE_SETUP_DELAY`
                    float setupDelayCountDown = DEFAULT_STATE_SETUP_DELAY;
                    do
                    {
                        await UniTask.Yield();
                        setupDelayCountDown -= Time.unscaledDeltaTime;
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
                        LastReloadEndTime = Time.unscaledTime + (_totalDuration / animSpeedRate);
                    }
                }

                float tempTriggerDuration;
                for (int i = 0; i < _triggerDurations.Length; ++i)
                {
                    // Wait until triggger before reload ammo
                    tempTriggerDuration = _triggerDurations[i];
                    remainsDuration -= tempTriggerDuration;
                    await UniTask.Delay((int)(tempTriggerDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, reloadCancellationTokenSource.Token);

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

                    // Reload / Fill ammo
                    int triggerReloadAmmoAmount = ReloadingAmmoAmount / _triggerDurations.Length;
                    EquipWeapons equipWeapons = Entity.EquipWeapons;
                    if (IsServer && Entity.DecreaseAmmos(weaponItem.WeaponType.RequireAmmoType, triggerReloadAmmoAmount, out _))
                    {
                        Entity.FillEmptySlots();
                        weapon.ammo += triggerReloadAmmoAmount;
                        if (isLeftHand)
                            equipWeapons.leftHand = weapon;
                        else
                            equipWeapons.rightHand = weapon;
                        Entity.EquipWeapons = equipWeapons;
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
                    await UniTask.Delay((int)(remainsDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, reloadCancellationTokenSource.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Catch the cancellation
                LastReloadEndTime = Time.unscaledTime;
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
            }
            // Clear action states at clients and server
            ClearReloadStates();
        }

        public void CancelReload()
        {
            for (int i = _reloadCancellationTokenSources.Count - 1; i >= 0; --i)
            {
                if (!_reloadCancellationTokenSources[i].IsCancellationRequested)
                    _reloadCancellationTokenSources[i].Cancel();
                _reloadCancellationTokenSources.RemoveAt(i);
            }
        }

        public void Reload(bool isLeftHand)
        {
            if (!IsServer && IsOwnerClient)
            {
                // Prepare state data which will be sent to server
                _clientState = new ReloadState()
                {
                    IsLeftHand = isLeftHand,
                };
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Reload immediately at server
                ProceedReloadStateAtServer(isLeftHand);
            }
        }

        private void ProceedReloadStateAtServer(bool isLeftHand)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!_manager.IsAcceptNewAction())
                return;
            // Speed hack avoidance
            if (Time.unscaledTime - LastReloadEndTime < -0.05f)
                return;
            // Get weapon to reload
            CharacterItem reloadingWeapon = isLeftHand ? Entity.EquipWeapons.leftHand : Entity.EquipWeapons.rightHand;
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
            int reloadingAmmoAmount = reloadingWeaponItem.AmmoCapacity - reloadingWeapon.ammo;
            int inventoryAmount = Entity.CountAmmos(reloadingWeaponItem.WeaponType.RequireAmmoType);
            if (inventoryAmount < reloadingAmmoAmount)
                reloadingAmmoAmount = inventoryAmount;
            if (reloadingAmmoAmount <= 0)
                return;
            _manager.ActionAccepted();
            // Prepare state data which will be sent to clients
            _serverState = new ReloadState()
            {
                IsLeftHand = isLeftHand,
                ReloadingAmmoAmount = reloadingAmmoAmount,
            };
#endif
        }

        public bool WriteClientReloadState(NetDataWriter writer)
        {
            if (_clientState.HasValue)
            {
                // Simulate reloading at client
                ReloadRoutine(_clientState.Value.IsLeftHand, 0).Forget();
                // Send input to server
                writer.Put(_clientState.Value.IsLeftHand);
                // Clear Input
                _clientState = null;
                return true;
            }
            return false;
        }

        public bool WriteServerReloadState(NetDataWriter writer)
        {
            if (_serverState.HasValue)
            {
                // Simulate reloading at server
                ReloadRoutine(_serverState.Value.IsLeftHand, _serverState.Value.ReloadingAmmoAmount).Forget();
                // Send input to client
                writer.Put(_serverState.Value.IsLeftHand);
                writer.PutPackedInt(_serverState.Value.ReloadingAmmoAmount);
                // Clear Input
                _serverState = null;
                return true;
            }
            return false;
        }

        public void ReadClientReloadStateAtServer(NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            ProceedReloadStateAtServer(isLeftHand);
        }

        public void ReadServerReloadStateAtClient(NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            int reloadingAmmoAmount = reader.GetPackedInt();
            if (IsOwnerClientOrOwnedByServer)
            {
                // Don't play reload animation again (it already done in `Reload` function)
                return;
            }
            // Play reload animation at client
            ReloadRoutine(isLeftHand, reloadingAmmoAmount).Forget();
        }
    }
}
