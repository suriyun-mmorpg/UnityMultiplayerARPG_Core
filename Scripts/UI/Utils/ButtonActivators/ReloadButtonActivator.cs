using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ReloadButtonActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private bool canReload;

        private void LateUpdate()
        {
            canReload = IsReloadable(BasePlayerCharacterController.OwningCharacter.EquipWeapons.rightHand) ||
                IsReloadable(BasePlayerCharacterController.OwningCharacter.EquipWeapons.leftHand);

            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(canReload);
            }
        }

        private bool IsReloadable(CharacterItem characterItem)
        {
            IWeaponItem weaponItem = characterItem.GetWeaponItem();
            return weaponItem != null && weaponItem.WeaponType.RequireAmmoType != null;
        }
    }
}
