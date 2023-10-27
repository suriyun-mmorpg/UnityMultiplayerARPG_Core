using Cysharp.Threading.Tasks;
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

        protected readonly List<CancellationTokenSource> _reloadCancellationTokenSources = new List<CancellationTokenSource>();
        public int ReloadingAmmoAmount { get; protected set; }
        public bool IsReloading { get; protected set; }
        public float LastReloadEndTime { get; protected set; }
        protected bool _skipMovementValidation;
        public bool LastReloadSkipMovementValidation { get { return _skipMovementValidation; } set { _skipMovementValidation = value; } }
        public float MoveSpeedRateWhileReloading { get; protected set; }
        public MovementRestriction MovementRestrictionWhileReloading { get; protected set; }
        protected float _totalDuration;
        public float ReloadTotalDuration { get { return _totalDuration; } set { _totalDuration = value; } }
        protected float[] _triggerDurations;
        public float[] ReloadTriggerDurations { get { return _triggerDurations; } set { _triggerDurations = value; } }
        public AnimActionType AnimActionType { get; protected set; }

        protected CharacterActionComponentManager _manager;

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

        protected virtual async UniTaskVoid ReloadRoutine(bool isLeftHand, int reloadingAmmoAmount)
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
                out _totalDuration,
                out _skipMovementValidation);

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
                    bool hasAmmoType = weaponItem.WeaponType.AmmoType != null;
                    bool hasAmmoItems = weaponItem.AmmoItems != null && weaponItem.AmmoItems.Length > 0;
                    if (IsServer)
                    {
                        if (hasAmmoType)
                        {
                            if (Entity.DecreaseAmmos(weaponItem.WeaponType.AmmoType, triggerReloadAmmoAmount, out _))
                            {
                                Entity.FillEmptySlots();
                                weapon.ammo += triggerReloadAmmoAmount;
                                if (isLeftHand)
                                    equipWeapons.leftHand = weapon;
                                else
                                    equipWeapons.rightHand = weapon;
                                Entity.EquipWeapons = equipWeapons;
                            }
                        }
                        else if (hasAmmoItems)
                        {
                            for (int indexOfAmmoItem = 0; indexOfAmmoItem < weaponItem.AmmoItems.Length; ++indexOfAmmoItem)
                            {
                                int countCurrentReloadAmmo = Entity.CountNonEquipItems(weaponItem.AmmoItems[indexOfAmmoItem].DataId);
                                if (countCurrentReloadAmmo >= triggerReloadAmmoAmount)
                                    countCurrentReloadAmmo = triggerReloadAmmoAmount;
                                if (Entity.DecreaseItems(weaponItem.AmmoItems[indexOfAmmoItem].DataId, countCurrentReloadAmmo))
                                {
                                    Entity.FillEmptySlots();
                                    weapon.ammo += countCurrentReloadAmmo;
                                    if (isLeftHand)
                                        equipWeapons.leftHand = weapon;
                                    else
                                        equipWeapons.rightHand = weapon;
                                    Entity.EquipWeapons = equipWeapons;
                                    break;
                                }
                            }
                        }
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
                RPC(CmdReload, isLeftHand);
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
#if UNITY_EDITOR || UNITY_SERVER
            if (!_manager.IsAcceptNewAction())
                return;
            // Speed hack avoidance
            if (Time.unscaledTime - LastReloadEndTime < -0.2f)
                return;
            // Get weapon to reload
            CharacterItem reloadingWeapon = isLeftHand ? Entity.EquipWeapons.leftHand : Entity.EquipWeapons.rightHand;
            if (reloadingWeapon.IsEmptySlot())
                return;
            IWeaponItem reloadingWeaponItem = reloadingWeapon.GetWeaponItem();
            if (reloadingWeaponItem == null || reloadingWeaponItem.AmmoCapacity <= 0 || reloadingWeapon.ammo >= reloadingWeaponItem.AmmoCapacity)
                return;
            bool hasAmmoType = reloadingWeaponItem.WeaponType.AmmoType != null;
            bool hasAmmoItems = reloadingWeaponItem.AmmoItems != null && reloadingWeaponItem.AmmoItems.Length > 0;
            if (!hasAmmoType && !hasAmmoItems)
                return;
            // Prepare reload data
            int reloadingAmmoAmount = reloadingWeaponItem.AmmoCapacity - reloadingWeapon.ammo;
            int inventoryAmount = 0;
            if (hasAmmoType)
            {
                inventoryAmount = Entity.CountAmmos(reloadingWeaponItem.WeaponType.AmmoType);
            }
            else if (hasAmmoItems)
            {
                for (int indexOfAmmoItem = 0; indexOfAmmoItem < reloadingWeaponItem.AmmoItems.Length; ++indexOfAmmoItem)
                {
                    inventoryAmount += Entity.CountNonEquipItems(reloadingWeaponItem.AmmoItems[indexOfAmmoItem].DataId);
                    if (inventoryAmount >= reloadingAmmoAmount)
                    {
                        inventoryAmount = reloadingAmmoAmount;
                        break;
                    }
                }
            }

            if (inventoryAmount < reloadingAmmoAmount)
                reloadingAmmoAmount = inventoryAmount;

            if (reloadingAmmoAmount <= 0)
                return;

            _manager.ActionAccepted();
            ReloadRoutine(isLeftHand, reloadingAmmoAmount).Forget();
            RPC(RpcReload, isLeftHand, reloadingAmmoAmount);
#endif
        }

        [AllRpc]
        protected void RpcReload(bool isLeftHand, int reloadingAmmoAmount)
        {
            if (IsServer || IsOwnerClient)
            {
                // Don't play reloading animation again
                return;
            }
            ReloadRoutine(isLeftHand, reloadingAmmoAmount).Forget();
        }
    }
}
