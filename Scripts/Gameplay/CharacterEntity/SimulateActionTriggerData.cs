using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct SimulateActionTriggerData : INetSerializable
    {
        public int simulateSeed;
        public byte triggerIndex;
        public AimPosition aimPosition;

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(simulateSeed);
            writer.Put(triggerIndex);
            writer.Put(aimPosition);
        }

        public void Deserialize(NetDataReader reader)
        {
            simulateSeed = reader.GetPackedInt();
            triggerIndex = reader.GetByte();
            aimPosition = reader.Get<AimPosition>();
        }
    }
}
