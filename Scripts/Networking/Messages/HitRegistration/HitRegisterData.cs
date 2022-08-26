using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct HitRegisterData : INetSerializable
    {
        public Vector3 Position { get; set; }
        public DirectionVector3 Direction { get; set; }
        public List<HitData> HitDataCollection { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutVector3(Position);
            writer.Put(Direction);
            writer.PutList(HitDataCollection);
        }

        public void Deserialize(NetDataReader reader)
        {
            Position = reader.GetVector3();
            Direction = reader.Get<DirectionVector3>();
            HitDataCollection = reader.GetList<HitData>();
        }
    }
}
