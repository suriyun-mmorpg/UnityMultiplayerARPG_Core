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
        info.AddValue("dataId", data.dataId);
        info.AddValue("currentHp", data.currentHp);
        info.AddValue("position", data.position);
        info.AddValue("rotation", data.rotation);
        info.AddValue("creatorId", data.creatorId);
        info.AddValue("creatorName", data.creatorName);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        BuildingSaveData data = (BuildingSaveData)obj;
        data.id = info.GetString("id");
        data.parentId = info.GetString("parentId");
        data.dataId = info.GetInt32("dataId");
        data.currentHp = info.GetInt32("currentHp");
        data.position = (Vector3)info.GetValue("position", typeof(Vector3));
        data.rotation = (Quaternion)info.GetValue("rotation", typeof(Quaternion));
        data.creatorId = info.GetString("creatorId");
        data.creatorName = info.GetString("creatorName");
        obj = data;
        return obj;
    }
}
