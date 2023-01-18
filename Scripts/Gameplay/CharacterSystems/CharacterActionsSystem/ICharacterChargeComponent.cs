using LiteNetLib.Utils;

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
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteClientStartChargeState(NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteServerStartChargeState(NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteClientStopChargeState(NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteServerStopChargeState(NetDataWriter writer);
        void ReadClientStartChargeStateAtServer(NetDataReader reader);
        void ReadServerStartChargeStateAtClient(NetDataReader reader);
        void ReadClientStopChargeStateAtServer(NetDataReader reader);
        void ReadServerStopChargeStateAtClient(NetDataReader reader);
    }
}
