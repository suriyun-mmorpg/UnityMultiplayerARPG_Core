using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestDismantleItemsMessage : INetSerializable
    {
        public short[] selectedIndexes;

        public void Deserialize(NetDataReader reader)
        {
            selectedIndexes = reader.GetShortArray();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutArray(selectedIndexes);
        }
    }
}
