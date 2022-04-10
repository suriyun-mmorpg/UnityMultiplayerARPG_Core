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
            writer.Put(AimPosition);
            writer.PutList(HitDataCollection);
        }

        public void Deserialize(NetDataReader reader)
        {
            AimPosition = reader.Get<AimPosition>();
            HitDataCollection = reader.GetList<HitData>();
        }
    }
}
