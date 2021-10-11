using UnityEngine;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct BuildingSaveData : IBuildingSaveData, INetSerializable
    {
        public string Id { get; set; }

        public string ParentId { get; set; }

        public int EntityId { get; set; }

        public int CurrentHp { get; set; }

        public float RemainsLifeTime { get; set; }

        public bool IsLocked { get; set; }

        public string LockPassword { get; set; }

        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }

        public string CreatorId { get; set; }

        public string CreatorName { get; set; }

        public string ExtraData { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            reader.DeserializeBuildingSaveData(ref this);
        }

        public void Serialize(NetDataWriter writer)
        {
            this.SerializeBuildingSaveData(writer);
        }
    }
}
