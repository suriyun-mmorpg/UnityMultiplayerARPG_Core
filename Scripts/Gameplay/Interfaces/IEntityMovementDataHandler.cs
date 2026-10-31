using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public interface IEntityMovementDataHandler
    {
        uint ObjectId { get; }
        long ConnectionId { get; }
        void ReadClientStateAtServer(long peerTimestamp, NetDataReader reader);
        void ReadServerStateAtClient(long peerTimestamp, NetDataReader reader);
        bool WriteClientState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably);
        bool WriteServerState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably);
    }

    // The one-shot jump, dash, teleport and still-move flags.
    // The server send loop writes each entity once per receiver, so clearing these flags inside that
    // write meant only the first player written in a tick got them. A data handler with such flags
    // implements this: its writes only read them, and the loop consumes them once per tick.
    public interface IEntityMovementServerStateFlags
    {
        /// <summary>
        /// Clears the one-shot flags this tick's server writes carried. Called by the server send loop
        /// once per tick, after its last player, on each handler it wrote; never from a write.
        /// </summary>
        void ConsumeServerStateFlags();
    }

    /// <summary>
    /// The handlers one server send pass wrote. Begin, Record
    /// after every successful write (a handler written to many players is kept once), ConsumeAll after
    /// the last player. A handler written to no one keeps its flags for a later tick, so an owner still
    /// gets its teleport, and so does a flag raised after the pass.
    /// </summary>
    public sealed class EntityMovementServerStateFlagsPass
    {
        private readonly HashSet<IEntityMovementServerStateFlags> _written = new HashSet<IEntityMovementServerStateFlags>();

        public int RecordedCount => _written.Count;

        /// <summary>
        /// Starts a pass. Drops, unconsumed, what a pass that threw before ConsumeAll left behind, so
        /// those flags go out again next tick (at least once, never lost).
        /// </summary>
        public void Begin()
        {
            _written.Clear();
        }

        public void Record(IEntityMovementDataHandler handler)
        {
            if (handler is IEntityMovementServerStateFlags flags)
                _written.Add(flags);
        }

        public void ConsumeAll()
        {
            foreach (IEntityMovementServerStateFlags flags in _written)
                flags.ConsumeServerStateFlags();
            _written.Clear();
        }
    }
}
