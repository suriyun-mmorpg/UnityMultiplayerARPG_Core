namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public System.Action<int> onUpdateRightWeaponAmmoSim;
        public System.Action<int> onUpdateLeftWeaponAmmoSim;

        protected int _rightWeaponAmmoSim;
        public int RightWeaponAmmoSim
        {
            get
            {
                return _rightWeaponAmmoSim;
            }
            set
            {
                if (value < 0)
                    value = 0;
                if (_rightWeaponAmmoSim != value)
                {
                    _rightWeaponAmmoSim = value;
                    onUpdateRightWeaponAmmoSim?.Invoke(value);
                }
            }
        }
        protected int _leftWeaponAmmoSim;
        public int LeftWeaponAmmoSim
        {
            get
            {
                return _leftWeaponAmmoSim;
            }
            set
            {
                if (value < 0)
                    value = 0;
                if (_leftWeaponAmmoSim != value)
                {
                    _leftWeaponAmmoSim = value;
                    onUpdateLeftWeaponAmmoSim?.Invoke(value);
                }
            }
        }

        public void UpdateAmmoSim()
        {
            if (!EquipWeapons.rightHand.IsEmptySlot() &&
                EquipWeapons.rightHand.GetWeaponItem() != null)
                RightWeaponAmmoSim = EquipWeapons.rightHand.ammo;

            if (!EquipWeapons.leftHand.IsEmptySlot() &&
                EquipWeapons.leftHand.GetWeaponItem() != null)
                LeftWeaponAmmoSim = EquipWeapons.leftHand.ammo;
        }
    }
}
