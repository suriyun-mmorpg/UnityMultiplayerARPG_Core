using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultCharacterReloadComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterReloadComponent
    {
        public const float DEFAULT_TOTAL_DURATION = 2f;
        public const float DEFAULT_TRIGGER_DURATION = 1f;
        public const float DEFAULT_STATE_SETUP_DELAY = 1f;
        protected List<CancellationTokenSource> reloadCancellationTokenSources = new List<CancellationTokenSource>();
        public short ReloadingAmmoAmount { get; protected set; }
        public bool IsReloading { get; protected set; }
        public float LastReloadEndTime { get; protected set; }
        public float MoveSpeedRateWhileReloading { get; protected set; }
        public MovementRestriction MovementRestrictionWhileReloading { get; protected set; }
        protected float totalDuration;
        public float ReloadTotalDuration { get { return totalDuration; } set { totalDuration = value; } }
        protected float[] triggerDurations;
        public float[] ReloadTriggerDurations { get { return triggerDurations; } set { triggerDurations = value; } }
        public AnimActionType AnimActionType { get; protected set; }

        protected bool sendingClientReload;
        protected bool sendingServerReload;
        protected bool sendingIsLeftHand;
        protected short sendingReloadingAmmoAmount;

        protected virtual void SetReloadActionStates(AnimActionType animActionType, short reloadingAmmoAmount)
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

        protected async UniTaskVoid ReloadRoutine(bool isLeftHand, short reloadingAmmoAmount)
        {
            // Prepare cancellation
            CancellationTokenSource reloadCancellationTokenSource = new CancellationTokenSource();
            reloadCancellationTokenSources.Add(reloadCancellationTokenSource);

            // Prepare requires data and get weapon data
            AnimActionType animActionType;
            int animActionDataId;
            CharacterItem weapon;
            Entity.GetReloadingData(
                ref isLeftHand,
                out animActionType,
                out animActionDataId,
                out weapon);

            // Prepare requires data and get animation data
            float animSpeedRate;
            Entity.GetAnimationData(
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
            MoveSpeedRateWhileReloading = Entity.GetMoveSpeedRateWhileReloading(weaponItem);
            MovementRestrictionWhileReloading = Entity.GetMovementRestrictionWhileReloading(weaponItem);

            // Last attack end time
            float remainsDuration = DEFAULT_TOTAL_DURATION;
            LastReloadEndTime = Time.unscaledTime + DEFAULT_TOTAL_DURATION;
            if (totalDuration >= 0f)
            {
                remainsDuration = totalDuration;
                LastReloadEndTime = Time.unscaledTime + (totalDuration / animSpeedRate);
            }

            try
            {
                // Play animation
                if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                {
                    // TPS model
                    Entity.CharacterModel.PlayActionAnimation(AnimActionType, animActionDataId, 0);
                }
                if (Entity.PassengingVehicleModel && Entity.PassengingVehicleModel is BaseCharacterModel)
                {
                    // Vehicle model
                    (Entity.PassengingVehicleModel as BaseCharacterModel).PlayActionAnimation(AnimActionType, animActionDataId, 0);
                }
                if (IsClient)
                {
                    if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                    {
                        // FPS model
                        Entity.FpsModel.PlayActionAnimation(AnimActionType, animActionDataId, 0);
                    }
                }

                // Special effects will plays on clients only
                if (IsClient)
                {
                    // Play weapon reload special effects
                    if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                        Entity.CharacterModel.PlayEquippedWeaponReload(isLeftHand);
                    if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                        Entity.FpsModel.PlayEquippedWeaponReload(isLeftHand);
                    // Play reload sfx
                    AudioClipWithVolumeSettings audioClip = weaponItem.ReloadClip;
                    if (audioClip != null)
                        AudioManager.PlaySfxClipAtAudioSource(audioClip.audioClip, Entity.CharacterModel.GenericAudioSource, audioClip.GetRandomedVolume());
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
                        LastReloadEndTime = Time.unscaledTime + (totalDuration / animSpeedRate);
                    }
                }

                float tempTriggerDuration;
                for (int i = 0; i < triggerDurations.Length; ++i)
                {
                    // Wait until triggger before reload ammo
                    tempTriggerDuration = triggerDurations[i];
                    remainsDuration -= tempTriggerDuration;
                    await UniTask.Delay((int)(tempTriggerDuration / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, reloadCancellationTokenSource.Token);

                    // Special effects will plays on clients only
                    if (IsClient)
                    {
                        // Play weapon reload special effects
                        if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                            Entity.CharacterModel.PlayEquippedWeaponReloaded(isLeftHand);
                        if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                            Entity.FpsModel.PlayEquippedWeaponReloaded(isLeftHand);
                        // Play reload sfx
                        AudioClipWithVolumeSettings audioClip = weaponItem.ReloadedClip;
                        if (audioClip != null)
                            AudioManager.PlaySfxClipAtAudioSource(audioClip.audioClip, Entity.CharacterModel.GenericAudioSource, audioClip.GetRandomedVolume());
                    }

                    // Reload / Fill ammo
                    short triggerReloadAmmoAmount = (short)(ReloadingAmmoAmount / triggerDurations.Length);
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
                reloadCancellationTokenSources.Remove(reloadCancellationTokenSource);
            }
            // Clear action states at clients and server
            ClearReloadStates();
        }

        public void CancelReload()
        {
            for (int i = reloadCancellationTokenSources.Count - 1; i >= 0; --i)
            {
                if (!reloadCancellationTokenSources[i].IsCancellationRequested)
                    reloadCancellationTokenSources[i].Cancel();
                reloadCancellationTokenSources.RemoveAt(i);
            }
        }

        public void Reload(bool isLeftHand)
        {
            if (!IsServer && IsOwnerClient)
            {
                sendingClientReload = true;
                sendingIsLeftHand = isLeftHand;
                ReloadRoutine(isLeftHand, 0).Forget();
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                ProceedReloadStateAtServer(isLeftHand);
            }
        }

        public bool WriteClientReloadState(NetDataWriter writer)
        {
            if (sendingClientReload)
            {
                writer.Put(sendingIsLeftHand);
                sendingClientReload = false;
                return true;
            }
            return false;
        }

        public bool WriteServerReloadState(NetDataWriter writer)
        {
            if (sendingServerReload)
            {
                writer.Put(sendingIsLeftHand);
                writer.PutPackedShort(sendingReloadingAmmoAmount);
                sendingServerReload = false;
                return true;
            }
            return false;
        }

        public void ReadClientReloadStateAtServer(NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            ProceedReloadStateAtServer(isLeftHand);
        }

        private void ProceedReloadStateAtServer(bool isLeftHand)
        {
#if UNITY_EDITOR || UNITY_SERVER
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
            short reloadingAmmoAmount = (short)(reloadingWeaponItem.AmmoCapacity - reloadingWeapon.ammo);
            int inventoryAmount = Entity.CountAmmos(reloadingWeaponItem.WeaponType.RequireAmmoType);
            if (inventoryAmount < reloadingAmmoAmount)
                reloadingAmmoAmount = (short)inventoryAmount;
            if (reloadingAmmoAmount <= 0)
                return;
            // Set reload state
            IsReloading = true;
            // Play animation at server immediately
            ReloadRoutine(isLeftHand, reloadingAmmoAmount).Forget();
            // Tell clients to play animation later
            sendingServerReload = true;
            sendingIsLeftHand = isLeftHand;
            sendingReloadingAmmoAmount = reloadingAmmoAmount;
#endif
        }

        public void ReadServerReloadStateAtClient(NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            short reloadingAmmoAmount = reader.GetPackedShort();
            if (IsOwnerClientOrOwnedByServer)
            {
                // Don't play reload animation again (it already played in `Reload` function)
                return;
            }
            // Play reload animation at client
            ReloadRoutine(isLeftHand, reloadingAmmoAmount).Forget();
        }
    }
}
