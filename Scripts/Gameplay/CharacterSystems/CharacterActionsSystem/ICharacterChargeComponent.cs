namespace MultiplayerARPG
{
    public interface ICharacterChargeComponent
    {
        bool IsCharging { get; }
        float MoveSpeedRateWhileCharging { get; }

        void ClearChargeStates();
    }
}
