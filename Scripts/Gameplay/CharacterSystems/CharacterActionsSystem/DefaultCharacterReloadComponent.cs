using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;

namespace MultiplayerARPG
{
    public class DefaultCharacterReloadComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterReloadComponent
    {

        protected List<CancellationTokenSource> reloadCancellationTokenSources = new List<CancellationTokenSource>();
        public short ReloadingAmmoAmount { get; protected set; }
        public bool IsReloading { get; protected set; }
        public float MoveSpeedRateWhileReloading { get; protected set; }
        public AnimActionType AnimActionType { get; protected set; }

        public bool CallAllPlayReloadAnimation(bool isLeftHand, short reloadingAmmoAmount)
        {
            if (Entity.IsDead())
                return false;
            RPC(AllPlayReloadAnimation, BaseCharacterEntity.ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand, reloadingAmmoAmount);
            return true;
        }

        [AllRpc]
        protected void AllPlayReloadAnimation(bool isLeftHand, short reloadingAmmoAmount)
        {
            ReloadRoutine(isLeftHand, reloadingAmmoAmount).Forget();
        }

        public bool CallServerReload(bool isLeftHand)
        {
            RPC(ServerReload, isLeftHand);
            return true;
        }

        [ServerRpc]
        protected virtual void ServerReload(bool isLeftHand)
        {
#if !CLIENT_BUILD
            if (!Entity.CanDoActions())
                return;

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

            // Start reload routine
            IsReloading = true;

            // Play animations
            CallAllPlayReloadAnimation(isLeftHand, reloadingAmmoAmount);
#endif
        }

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
            float[] triggerDurations;
            float totalDuration;
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
            try
            {
                // Play animation
                if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                {
                    // TPS model
                    Entity.CharacterModel.PlayActionAnimation(AnimActionType, animActionDataId, 0);
                }
                if (Entity.PassengingVehicleEntity != null && Entity.PassengingVehicleEntity.Entity.Model &&
                    Entity.PassengingVehicleEntity.Entity.Model.gameObject.activeSelf &&
                    Entity.PassengingVehicleEntity.Entity.Model is BaseCharacterModel)
                {
                    // Vehicle model
                    (Entity.PassengingVehicleEntity.Entity.Model as BaseCharacterModel).PlayActionAnimation(AnimActionType, animActionDataId, 0);
                }
                if (IsClient)
                {
                    if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                    {
                        // FPS model
                        Entity.FpsModel.PlayActionAnimation(AnimActionType, animActionDataId, 0);
                    }
                }

                // Animations will plays on clients only
                if (IsClient)
                {
                    // Play weapon reload special effects
                    if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                        Entity.CharacterModel.PlayEquippedWeaponReload(isLeftHand);
                    if (Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                        Entity.FpsModel.PlayEquippedWeaponReload(isLeftHand);
                    // Play reload sfx
                    if (AnimActionType == AnimActionType.ReloadRightHand ||
                        AnimActionType == AnimActionType.ReloadLeftHand)
                    {
                        AudioManager.PlaySfxClipAtAudioSource(weaponItem.ReloadClip, Entity.CharacterModel.GenericAudioSource);
                    }
                }

                for (int i = 0; i < triggerDurations.Length; ++i)
                {
                    // Wait until triggger before reload ammo
                    await UniTask.Delay((int)(triggerDurations[i] / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, reloadCancellationTokenSource.Token);

                    // Prepare data
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
                    await UniTask.Delay((int)((totalDuration - triggerDurations[i]) / animSpeedRate * 1000f), true, PlayerLoopTiming.Update, reloadCancellationTokenSource.Token);
                }
            }
            catch
            {
                // Catch the cancellation
            }
            finally
            {
                // Clear action states at clients and server
                if (!reloadCancellationTokenSource.IsCancellationRequested)
                {
                    ClearReloadStates();
                }
                reloadCancellationTokenSource.Dispose();
                reloadCancellationTokenSources.Remove(reloadCancellationTokenSource);
            }
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
            CallServerReload(isLeftHand);
        }
    }
}
