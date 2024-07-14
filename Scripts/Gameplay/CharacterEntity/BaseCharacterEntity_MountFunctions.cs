namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public override void SetPassengingVehicle(byte seatIndex, IVehicleEntity vehicleEntity)
        {
            base.SetPassengingVehicle(seatIndex, vehicleEntity);
            _isRecaching = true;
        }

        public override bool CanEnterVehicle(IVehicleEntity vehicleEntity, byte seatIndex, out UITextKeys gameMessage)
        {
            if (!base.CanEnterVehicle(vehicleEntity, seatIndex, out gameMessage))
                return false;

            if (!IsGameEntityInDistance(vehicleEntity))
            {
                gameMessage = UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR;
                return false;
            }

            return true;
        }
    }
}