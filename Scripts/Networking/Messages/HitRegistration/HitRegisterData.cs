using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct HitRegisterData : INetSerializable
    {
        public AimPosition AimPosition { get; set; }
        public List<HitData> HitDataCollection { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            AimPosition.Serialize(writer);
            writer.PutList(HitDataCollection);
        }

        public void Deserialize(NetDataReader reader)
        {
            AimPosition.Deserialize(reader);
            HitDataCollection = reader.GetList<HitData>();
        }
    }
}
