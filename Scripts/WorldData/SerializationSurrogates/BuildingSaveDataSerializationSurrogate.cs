using System.Runtime.Serialization;
using UnityEngine;

namespace MultiplayerARPG
{
    public class BuildingSaveDataSerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(
            object obj,
            SerializationInfo info,
            StreamingContext context)
        {
            BuildingSaveData data = (BuildingSaveData)obj;
            info.AddValue("id", data.Id);
            info.AddValue("parentId", data.ParentId);
            info.AddValue("entityId", data.EntityId);
            info.AddValue("currentHp", data.CurrentHp);
            info.AddValue("remainsLifeTime", data.RemainsLifeTime);
            info.AddValue("position", new Vector3(data.PositionX, data.PositionY, data.PositionZ));
            info.AddValue("rotation", Quaternion.Euler(data.RotationX, data.RotationY, data.RotationZ));
            info.AddValue("isLocked", data.IsLocked);
            info.AddValue("lockPassword", data.LockPassword);
            info.AddValue("creatorId", data.CreatorId);
            info.AddValue("creatorName", data.CreatorName);
            info.AddValue("extraData", data.ExtraData);
        }

        public object SetObjectData(
            object obj,
            SerializationInfo info,
            StreamingContext context,
            ISurrogateSelector selector)
        {
            BuildingSaveData data = (BuildingSaveData)obj;
            data.Id = info.GetString("id");
            data.ParentId = info.GetString("parentId");
            data.EntityId = info.GetInt32("entityId");
            data.CurrentHp = info.GetInt32("currentHp");
            data.RemainsLifeTime = info.GetSingle("remainsLifeTime");
            Vector3 position = (Vector3)info.GetValue("position", typeof(Vector3));
            data.PositionX = position.x;
            data.PositionY = position.y;
            data.PositionZ = position.z;
            Quaternion rotation = (Quaternion)info.GetValue("rotation", typeof(Quaternion));
            data.RotationX = rotation.eulerAngles.x;
            data.RotationY = rotation.eulerAngles.y;
            data.RotationZ = rotation.eulerAngles.z;
            data.IsLocked = info.GetBoolean("isLocked");
            data.LockPassword = info.GetString("lockPassword");
            data.CreatorId = info.GetString("creatorId");
            data.CreatorName = info.GetString("creatorName");
            data.ExtraData = info.GetString("extraData");
            obj = data;
            return obj;
        }
    }
}
