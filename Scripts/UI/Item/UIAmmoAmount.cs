using UnityEngine;

namespace MultiplayerARPG
{
    public class UIAmmoAmount : UIBase
    {
        [Header("Right Hand Ammo Amount")]
        public GameObject uiRightHandAmmoRoot;
        public TextWrapper uiTextRightHandCurrentAmmo;
        public TextWrapper uiTextRightHandReserveAmmo;
        public TextWrapper uiTextRightHandSumAmmo;
        public GameObject[] rightHandRequireAmmoSymbols;
        public GameObject[] rightHandNoRequireAmmoSymbols;
        public UIGageValue gageRightHandAmmo;
        [Header("Left Hand Ammo Amount")]
        public GameObject uiLeftHandAmmoRoot;
        public TextWrapper uiTextLeftHandCurrentAmmo;
        public TextWrapper uiTextLeftHandReserveAmmo;
        public TextWrapper uiTextLeftHandSumAmmo;
        public GameObject[] leftHandRequireAmmoSymbols;
        public GameObject[] leftHandNoRequireAmmoSymbols;
        public UIGageValue gageLeftHandAmmo;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            uiRightHandAmmoRoot = null;
            uiTextRightHandCurrentAmmo = null;
            uiTextRightHandReserveAmmo = null;
            uiTextRightHandSumAmmo = null;
            rightHandRequireAmmoSymbols.Nulling();
            rightHandNoRequireAmmoSymbols.Nulling();
            gageRightHandAmmo = null;
            uiLeftHandAmmoRoot = null;
            uiTextLeftHandCurrentAmmo = null;
            uiTextLeftHandReserveAmmo = null;
            uiTextLeftHandSumAmmo = null;
            leftHandRequireAmmoSymbols.Nulling();
            leftHandNoRequireAmmoSymbols.Nulling();
            gageLeftHandAmmo = null;
        }

        protected virtual void OnEnable()
        {
            UpdateOwningCharacterData();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onRecached += UpdateOwningCharacterData;
        }

        protected virtual void OnDisable()
        {
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onRecached -= UpdateOwningCharacterData;
        }

        public void UpdateOwningCharacterData()
        {
            if (!GameInstance.PlayingCharacterEntity) return;
            UpdateData();
        }

        public void UpdateData()
        {
            UpdateUI(uiRightHandAmmoRoot, uiTextRightHandCurrentAmmo, uiTextRightHandReserveAmmo, uiTextRightHandSumAmmo, rightHandRequireAmmoSymbols, rightHandNoRequireAmmoSymbols, gageRightHandAmmo, GameInstance.PlayingCharacterEntity.EquipWeapons.rightHand);
            UpdateUI(uiLeftHandAmmoRoot, uiTextLeftHandCurrentAmmo, uiTextLeftHandReserveAmmo, uiTextLeftHandSumAmmo, leftHandRequireAmmoSymbols, leftHandNoRequireAmmoSymbols, gageLeftHandAmmo, GameInstance.PlayingCharacterEntity.EquipWeapons.leftHand);
        }

        protected virtual void UpdateUI(GameObject root, TextWrapper textCurrentAmmo, TextWrapper textReserveAmmo, TextWrapper textSumAmmo, GameObject[] requireAmmoSymbols, GameObject[] noRequireAmmoSymbols, UIGageValue gageAmmo, CharacterItem characterItem)
        {
            IWeaponItem weaponItem = characterItem.GetWeaponItem();
            bool isActive = weaponItem != null && weaponItem.WeaponType.AmmoType != null;
            if (root != null)
                root.SetActive(isActive);

            int currentAmmo = characterItem.ammo;
            int reserveAmmo = 0;
            if (GameInstance.PlayingCharacterEntity && isActive)
                reserveAmmo = GameInstance.PlayingCharacterEntity.CountAllAmmos(weaponItem.WeaponType.AmmoType);

            if (textCurrentAmmo != null)
            {
                textCurrentAmmo.SetGameObjectActive(isActive && weaponItem.AmmoCapacity > 0);
                if (isActive)
                    textCurrentAmmo.text = currentAmmo.ToString("N0");
            }

            if (textReserveAmmo != null)
            {
                textReserveAmmo.SetGameObjectActive(isActive);
                if (isActive)
                    textReserveAmmo.text = reserveAmmo.ToString("N0");
            }

            if (textSumAmmo != null)
            {
                textSumAmmo.SetGameObjectActive(isActive);
                textSumAmmo.text = (currentAmmo + reserveAmmo).ToString("N0");
            }

            if (requireAmmoSymbols != null)
            {
                foreach (GameObject symbol in requireAmmoSymbols)
                {
                    symbol.SetActive(isActive);
                }
            }

            if (noRequireAmmoSymbols != null)
            {
                foreach (GameObject symbol in noRequireAmmoSymbols)
                {
                    symbol.SetActive(!isActive);
                }
            }

            if (gageAmmo != null)
            {
                gageAmmo.SetVisible(isActive && weaponItem.AmmoCapacity > 1);
                if (isActive)
                    gageAmmo.Update(currentAmmo, weaponItem.AmmoCapacity);
            }
        }
    }
}
