using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class WorldSaveDataSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        WorldSaveData data = (WorldSaveData)obj;
        info.AddListValue("buildings", data.buildings);
        info.AddListValue("storageItems", data.storageItems);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        WorldSaveData data = (WorldSaveData)obj;
        data.buildings = new List<BuildingSaveData>(info.GetListValue<BuildingSaveData>("buildings"));
        data.storageItems = new List<StorageCharacterItem>(info.GetListValue<StorageCharacterItem>("storageItems"));
        obj = data;
        return obj;
    }
}
