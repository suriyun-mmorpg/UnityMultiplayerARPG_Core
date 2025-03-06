using UnityEngine;

namespace MultiplayerARPG
{
    public class ShooterReloadUpdater : MonoBehaviour
    {
        public enum EInterruptByAttackingState
        {
            None,
            ConfirmingToAttackR,
            ConfirmingToAttackL,
            AttackingR,
            AttackingL,
        }
        public ShooterPlayerCharacterController Controller { get; set; }
        public BasePlayerCharacterEntity PlayingCharacterEntity => Controller.PlayingCharacterEntity;
        public bool IsReloading { get; protected set; }
        public EInterruptByAttackingState InterruptByAttackingState { get; protected set; } = EInterruptByAttackingState.None;

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

        public virtual void InterruptByAttacking(bool isLeftHand)
        {
            if (isLeftHand)
                InterruptByAttackingState = EInterruptByAttackingState.ConfirmingToAttackL;
            else
                InterruptByAttackingState = EInterruptByAttackingState.ConfirmingToAttackR;
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
            bool isAttackLeftHand;
            switch (InterruptByAttackingState)
            {
                case EInterruptByAttackingState.AttackingR:
                    InterruptByAttackingState = EInterruptByAttackingState.None;
                    isAttackLeftHand = false;
                    PlayingCharacterEntity.Attack(ref isAttackLeftHand);
                    return;
                case EInterruptByAttackingState.AttackingL:
                    InterruptByAttackingState = EInterruptByAttackingState.None;
                    isAttackLeftHand = true;
                    PlayingCharacterEntity.Attack(ref isAttackLeftHand);
                    return;
            }
            bool continueReloadingR = false;
            bool continueReloadingL = false;
            // Reload right-hand weapon
            if (!PlayingCharacterEntity.EquipWeapons.rightHand.IsAmmoFull(PlayingCharacterEntity) &&
                PlayingCharacterEntity.EquipWeapons.rightHand.HasAmmoToReload(PlayingCharacterEntity, out int reloadingAmmoDataIdR, out int ammoAmountR) &&
                (!_reloadedDataIdR.HasValue || _reloadedDataIdR.Value == reloadingAmmoDataIdR))
            {
                _reloadedDataIdR = reloadingAmmoDataIdR;
                continueReloadingR = ProceedReloading(false, PlayingCharacterEntity.EquipWeapons.GetRightHandWeaponItem(), ammoAmountR);
            }
            // Reload left-hand weapon
            if (!PlayingCharacterEntity.EquipWeapons.leftHand.IsAmmoFull(PlayingCharacterEntity) &&
                PlayingCharacterEntity.EquipWeapons.leftHand.HasAmmoToReload(PlayingCharacterEntity, out int reloadingAmmoDataIdL, out int ammoAmountL) &&
                (!_reloadedDataIdL.HasValue || _reloadedDataIdL.Value == reloadingAmmoDataIdL))
            {
                _reloadedDataIdL = reloadingAmmoDataIdL;
                continueReloadingL = ProceedReloading(true, PlayingCharacterEntity.EquipWeapons.GetLeftHandWeaponItem(), ammoAmountL);
            }
            if (!continueReloadingR && !continueReloadingL)
            {
                // Not continue reloading
                IsReloading = false;
                InterruptByAttackingState = EInterruptByAttackingState.None;
            }
            else
            {
                // Continue reloading, let's check if it is being interrupted by attacking inputs or not, if interrupted then attack later
                switch (InterruptByAttackingState)
                {
                    case EInterruptByAttackingState.ConfirmingToAttackR:
                        InterruptByAttackingState = EInterruptByAttackingState.AttackingR;
                        break;
                    case EInterruptByAttackingState.ConfirmingToAttackL:
                        InterruptByAttackingState = EInterruptByAttackingState.AttackingL;
                        break;
                }
            }
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
            return weaponItem.MaxAmmoEachReload > 0 && ammoAmount - weaponItem.MaxAmmoEachReload >= 0;
        }
    }
}
