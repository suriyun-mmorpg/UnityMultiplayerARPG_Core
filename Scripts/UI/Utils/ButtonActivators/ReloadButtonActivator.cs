using UnityEngine;

namespace MultiplayerARPG
{
    public class ReloadButtonActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private void LateUpdate()
        {
            bool canReload = IsReloadable(GameInstance.PlayingCharacter.EquipWeapons.rightHand) || IsReloadable(GameInstance.PlayingCharacter.EquipWeapons.leftHand);
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
