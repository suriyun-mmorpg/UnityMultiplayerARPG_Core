using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIAmmoAmount : UIBase
    {
        public ICharacterData character { get; protected set; }

        [Header("Right Hand Ammo Amount")]
        public GameObject uiRightHandAmmoRoot;
        public TextWrapper uiTextRightHandCurrentAmmo;
        public TextWrapper uiTextRightHandReserveAmmo;
        [Header("Left Hand Ammo Amount")]
        public GameObject uiLeftHandAmmoRoot;
        public TextWrapper uiTextLeftHandCurrentAmmo;
        public TextWrapper uiTextLeftHandReserveAmmo;

        public void UpdateData(ICharacterData character)
        {
            this.character = character;
            UpdateUI(uiRightHandAmmoRoot, uiTextRightHandCurrentAmmo, uiTextRightHandReserveAmmo, character.EquipWeapons.rightHand);
            UpdateUI(uiLeftHandAmmoRoot, uiTextLeftHandCurrentAmmo, uiTextLeftHandReserveAmmo, character.EquipWeapons.leftHand);
        }

        private void UpdateUI(GameObject root, TextWrapper textCurrentAmmo, TextWrapper textReserveAmmo, CharacterItem characterItem)
        {
            Item weaponItem = characterItem.GetWeaponItem();
            bool isActive = weaponItem != null && weaponItem.WeaponType.requireAmmoType != null;
            if (root != null)
                root.SetActive(isActive);

            if (textCurrentAmmo != null)
            {
                textCurrentAmmo.gameObject.SetActive(isActive && weaponItem.ammoCapacity > 0);
                if (isActive)
                    textCurrentAmmo.text = characterItem.ammo.ToString("N0");
            }

            if (textReserveAmmo != null)
            {
                textReserveAmmo.gameObject.SetActive(isActive);
                if (isActive)
                {
                    int ammoAmount = character != null && weaponItem.WeaponType.requireAmmoType != null ? character.CountAmmos(weaponItem.WeaponType.requireAmmoType) : 0;
                    textReserveAmmo.text = ammoAmount.ToString("N0");
                }
            }
        }
    }
}
