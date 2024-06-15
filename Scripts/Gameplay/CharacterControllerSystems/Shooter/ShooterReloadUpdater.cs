using UnityEngine;

namespace MultiplayerARPG
{
    public class ShooterReloadUpdater : MonoBehaviour
    {
        public ShooterPlayerCharacterController Controller { get; set; }
        public BasePlayerCharacterEntity PlayingCharacterEntity => Controller.PlayingCharacterEntity;
        public bool IsReloading { get; set; }

        public virtual void Reload()
        {
            if (IsReloading)
                return;
            IsReloading = true;
            if (Controller.WeaponAbility != null &&
                Controller.WeaponAbility.ShouldDeactivateOnReload)
            {
                Controller.WeaponAbility.ForceDeactivated();
                Controller.WeaponAbilityState = WeaponAbilityState.Deactivated;
            }
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
            bool allReload = true;
            // Reload right-hand weapon
            if (!PlayingCharacterEntity.EquipWeapons.rightHand.IsAmmoFull() &&
                PlayingCharacterEntity.EquipWeapons.rightHand.HasAmmoToReload(PlayingCharacterEntity))
            {
                if (!PlayingCharacterEntity.Reload(false))
                    allReload = false;
            }
            // Reload left-hand weapon
            if (!PlayingCharacterEntity.EquipWeapons.leftHand.IsAmmoFull() &&
                PlayingCharacterEntity.EquipWeapons.leftHand.HasAmmoToReload(PlayingCharacterEntity))
            {
                if (!PlayingCharacterEntity.Reload(true))
                    allReload = false;
            }
            // All reloaded
            if (allReload)
                IsReloading = false;
        }
    }
}
