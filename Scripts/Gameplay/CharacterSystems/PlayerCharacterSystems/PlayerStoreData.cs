using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct PlayerStoreData : INetSerializable
    {
        public bool isOpen;
        public string title;

        public void Deserialize(NetDataReader reader)
        {
            isOpen = reader.GetBool();
            if (isOpen)
                title = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(isOpen);
            if (isOpen)
                writer.Put(title);
        }
    }

    [System.Serializable]
    public class SyncFieldPlayerStoreData : LiteNetLibSyncField<PlayerStoreData>
    {
    }
}