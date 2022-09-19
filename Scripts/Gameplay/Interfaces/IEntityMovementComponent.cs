using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public interface IEntityMovementComponent : IEntityMovement, IGameEntityComponent
    {
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="shouldSendReliably"></param>
        /// <returns></returns>
        bool WriteClientState(NetDataWriter writer, out bool shouldSendReliably);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="shouldSendReliably"></param>
        /// <returns></returns>
        bool WriteServerState(NetDataWriter writer, out bool shouldSendReliably);
        void ReadClientStateAtServer(NetDataReader reader);
        void ReadServerStateAtClient(NetDataReader reader);
    }
}
