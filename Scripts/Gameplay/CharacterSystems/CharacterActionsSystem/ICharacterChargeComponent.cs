namespace MultiplayerARPG
{
    public interface ICharacterChargeComponent
    {
        bool IsCharging { get; }
        bool WillDoActionWhenStopCharging { get; }
        float MoveSpeedRateWhileCharging { get; }
        MovementRestriction MovementRestrictionWhileCharging { get; }

        void ClearChargeStates();
        void StartCharge(bool isLeftHand);
        void StopCharge();
    }
}
