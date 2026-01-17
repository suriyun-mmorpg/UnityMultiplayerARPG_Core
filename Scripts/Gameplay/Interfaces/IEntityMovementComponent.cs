using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IEntityMovementComponent : IEntityMovement
    {
        bool enabled { get; set; }
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writeTick"></param>
        /// <param name="writer"></param>
        /// <param name="shouldSendReliably"></param>
        /// <returns></returns>
        bool WriteClientState(uint writeTick, NetDataWriter writer, out bool shouldSendReliably);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writeTick"></param>
        /// <param name="writer"></param>
        /// <param name="shouldSendReliably"></param>
        /// <returns></returns>
        bool WriteServerState(uint writeTick, NetDataWriter writer, out bool shouldSendReliably);
        void ReadClientStateAtServer(uint peerTick, NetDataReader reader);
        void ReadServerStateAtClient(uint peerTick, NetDataReader reader);
        Bounds GetMovementBounds();
    }
}
