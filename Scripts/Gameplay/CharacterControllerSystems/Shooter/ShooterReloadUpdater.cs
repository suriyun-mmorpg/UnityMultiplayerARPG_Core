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
            if (PlayingCharacterEntity.IsPlayingActionAnimation())
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
                PlayingCharacterEntity.EquipWeapons.rightHand.HasAmmoToReload(PlayingCharacterEntity, out int reloadingAmmoDataIdR, out int ammoAmountR) &&
                (!_reloadedDataIdR.HasValue || _reloadedDataIdR.Value == reloadingAmmoDataIdR))
            {
                _reloadedDataIdR = reloadingAmmoDataIdR;
                continueReloadingR = ProceedReloading(false, PlayingCharacterEntity.EquipWeapons.GetRightHandWeaponItem(), ammoAmountR);
            }
            // Reload left-hand weapon
            if (!PlayingCharacterEntity.EquipWeapons.leftHand.IsAmmoFull() &&
                PlayingCharacterEntity.EquipWeapons.leftHand.HasAmmoToReload(PlayingCharacterEntity, out int reloadingAmmoDataIdL, out int ammoAmountL) &&
                (!_reloadedDataIdL.HasValue || _reloadedDataIdL.Value == reloadingAmmoDataIdL))
            {
                _reloadedDataIdL = reloadingAmmoDataIdL;
                continueReloadingL = ProceedReloading(true, PlayingCharacterEntity.EquipWeapons.GetLeftHandWeaponItem(), ammoAmountL);
            }
            if (!continueReloadingR && !continueReloadingL)
                IsReloading = false;
        }

        /// <summary>
        /// Return `TRUE` if it should continue
        /// </summary>
        /// <param name="isLeftHand"></param>
        /// <param name="weaponItem"></param>
        /// <returns></returns>
        private bool ProceedReloading(bool isLeftHand, IWeaponItem weaponItem, int ammoAmount)
        {
            if (weaponItem == null)
                return false;
            if (!PlayingCharacterEntity.Reload(isLeftHand))
                return false;
            return weaponItem.MaxAmmoEachReload > 0 && ammoAmount - weaponItem.MaxAmmoEachReload > 0;
        }
    }
}
