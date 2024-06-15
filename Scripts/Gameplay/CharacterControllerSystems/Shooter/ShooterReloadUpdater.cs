using UnityEngine;

namespace MultiplayerARPG
{
    public class ShooterReloadUpdater : MonoBehaviour
    {
        public ShooterPlayerCharacterController Controller { get; set; }
        public BasePlayerCharacterEntity PlayingCharacterEntity => Controller.PlayingCharacterEntity;
        public bool IsReloading { get; protected set; }
        protected int? _reloadedDataIdR = null;
        protected int? _reloadedDataIdL = null;

        public virtual void Reload()
        {
            if (PlayingCharacterEntity.IsDead())
                return;
            if (IsReloading)
                return;
            IsReloading = true;
            _reloadedDataIdR = null;
            _reloadedDataIdL = null;
            if (Controller.WeaponAbility != null &&
                Controller.WeaponAbility.ShouldDeactivateOnReload)
            {
                Controller.WeaponAbility.ForceDeactivated();
                Controller.WeaponAbilityState = WeaponAbilityState.Deactivated;
            }
        }

        public virtual void Interrupt()
        {
            IsReloading = false;
        }

        protected virtual void Update()
        {
            if (PlayingCharacterEntity.IsDead())
                IsReloading = false;
            if (!IsReloading)
                return;
            // Wait until animation end
            if (PlayingCharacterEntity.IsPlayingActionAnimation())
                return;
            bool continueReloadingR = false;
            bool continueReloadingL = false;
            // Reload right-hand weapon
            if (!PlayingCharacterEntity.EquipWeapons.rightHand.IsAmmoFull() &&
                PlayingCharacterEntity.EquipWeapons.rightHand.HasAmmoToReload(PlayingCharacterEntity, out int reloadingAmmoDataIdR, out _) &&
                (!_reloadedDataIdR.HasValue || _reloadedDataIdR.Value == reloadingAmmoDataIdR))
            {
                _reloadedDataIdR = reloadingAmmoDataIdR;
                continueReloadingR = true;
                IWeaponItem weaponItem = PlayingCharacterEntity.EquipWeapons.GetRightHandWeaponItem();
                if (!PlayingCharacterEntity.Reload(false))
                    continueReloadingR = false;
                else if (weaponItem == null || weaponItem.MaxAmmoEachReload <= 0)
                    continueReloadingR = false;
            }
            // Reload left-hand weapon
            if (!PlayingCharacterEntity.EquipWeapons.leftHand.IsAmmoFull() &&
                PlayingCharacterEntity.EquipWeapons.leftHand.HasAmmoToReload(PlayingCharacterEntity, out int reloadingAmmoDataIdL, out _) &&
                (!_reloadedDataIdL.HasValue || _reloadedDataIdL.Value == reloadingAmmoDataIdL))
            {
                _reloadedDataIdL = reloadingAmmoDataIdL;
                continueReloadingL = true;
                IWeaponItem weaponItem = PlayingCharacterEntity.EquipWeapons.GetLeftHandWeaponItem();
                if (!PlayingCharacterEntity.Reload(true))
                    continueReloadingL = false;
                else if (weaponItem == null || weaponItem.MaxAmmoEachReload <= 0)
                    continueReloadingL = false;
            }
            if (!continueReloadingR && !continueReloadingL)
                IsReloading = false;
        }
    }
}
