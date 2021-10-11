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
            info.AddValue("position", data.Position);
            info.AddValue("rotation", data.Rotation);
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
            data.CurrentHp = info.GetInt32("currentHp");
            data.Position = (Vector3)info.GetValue("position", typeof(Vector3));
            data.Rotation = (Quaternion)info.GetValue("rotation", typeof(Quaternion));
            data.CreatorId = info.GetString("creatorId");
            data.CreatorName = info.GetString("creatorName");
            // TODO: Backward compatible, this will be removed in future version
            try
            {
                data.RemainsLifeTime = info.GetSingle("remainsLifeTime");
            }
            catch { }
            try
            {
                data.IsLocked = info.GetBoolean("isLocked");
            }
            catch { }
            try
            {
                data.LockPassword = info.GetString("lockPassword");
            }
            catch { }
            try
            {
                data.ExtraData = info.GetString("extraData");
            }
            catch { }
            try
            {
                data.EntityId = info.GetInt32("entityId");
            }
            catch { }
            try
            {
                int dataId = info.GetInt32("dataId");
                foreach (BuildingEntity prefab in GameInstance.BuildingEntities.Values)
                {
                    if (dataId == prefab.name.GenerateHashId())
                    {
                        data.EntityId = prefab.EntityId;
                        break;
                    }
                }
            }
            catch { }
            obj = data;
            return obj;
        }
    }
}
