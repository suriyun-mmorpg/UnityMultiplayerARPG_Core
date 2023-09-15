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
        /// <param name="writeTimestamp"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteClientStartChargeState(long writeTimestamp, NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writeTimestamp"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteServerStartChargeState(long writeTimestamp, NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writeTimestamp"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteClientStopChargeState(long writeTimestamp, NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writeTimestamp"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteServerStopChargeState(long writeTimestamp, NetDataWriter writer);
        void ReadClientStartChargeStateAtServer(long peerTimestamp, NetDataReader reader);
        void ReadServerStartChargeStateAtClient(long peerTimestamp, NetDataReader reader);
        void ReadClientStopChargeStateAtServer(long peerTimestamp, NetDataReader reader);
        void ReadServerStopChargeStateAtClient(long peerTimestamp, NetDataReader reader);
    }
}
