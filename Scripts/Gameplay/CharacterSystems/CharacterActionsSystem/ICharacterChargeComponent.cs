namespace MultiplayerARPG
{
    public interface ICharacterChargeComponent
    {
        bool IsCharging { get; }
        float MoveSpeedRateWhileCharging { get; }

        void ClearChargeStates();
        bool StartCharge(bool isLeftHand);
        bool StopCharge();
    }
}
