using MultiplayerARPG;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class BuildingSaveDataSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        BuildingSaveData data = (BuildingSaveData)obj;
        info.AddValue("id", data.id);
        info.AddValue("parentId", data.parentId);
        info.AddValue("entityId", data.entityId);
        info.AddValue("currentHp", data.currentHp);
        info.AddValue("position", data.position);
        info.AddValue("rotation", data.rotation);
        info.AddValue("isLocked", data.isLocked);
        info.AddValue("lockPassword", data.lockPassword);
        info.AddValue("creatorId", data.creatorId);
        info.AddValue("creatorName", data.creatorName);
        info.AddValue("extraData", data.extraData);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        BuildingSaveData data = (BuildingSaveData)obj;
        data.id = info.GetString("id");
        data.parentId = info.GetString("parentId");
        data.entityId = info.GetInt32("entityId");
        data.currentHp = info.GetInt32("currentHp");
        data.position = (Vector3)info.GetValue("position", typeof(Vector3));
        data.rotation = (Quaternion)info.GetValue("rotation", typeof(Quaternion));
        data.creatorId = info.GetString("creatorId");
        data.creatorName = info.GetString("creatorName");
        // TODO: Backward compatible, this will be removed in future version
        try
        {
            data.isLocked = info.GetBoolean("isLocked");
        }
        catch { }
        try
        {
            data.lockPassword = info.GetString("lockPassword");
        }
        catch { }
        try
        {
            data.extraData = info.GetString("extraData");
        }
        catch { }
        try
        {
            int dataId = info.GetInt32("dataId");
            foreach (BuildingEntity prefab in GameInstance.BuildingEntities.Values)
            {
                if (dataId == prefab.name.GenerateHashId())
                {
                    data.entityId = prefab.EntityId;
                    break;
                }
            }
        }
        catch { }
        obj = data;
        return obj;
    }
}
