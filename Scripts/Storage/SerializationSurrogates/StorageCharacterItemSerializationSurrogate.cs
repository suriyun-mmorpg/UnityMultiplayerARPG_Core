using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class StorageCharacterItemSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        StorageCharacterItem data = (StorageCharacterItem)obj;
        info.AddValue("storageType", (byte)data.storageType);
        info.AddValue("storageId", data.storageOwnerId);
        info.AddValue("storageIndex", data.storageIndex);
        info.AddValue("dataId", data.dataId);
        info.AddValue("level", data.level);
        info.AddValue("amount", data.amount);
        info.AddValue("durability", data.durability);
        info.AddValue("exp", data.exp);
        info.AddValue("lockRemainsDuration", data.lockRemainsDuration);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        StorageCharacterItem data = (StorageCharacterItem)obj;
        data.storageType = (StorageType)info.GetByte("storageType");
        data.storageOwnerId = info.GetString("storageId");
        data.storageIndex = info.GetInt32("storageIndex");
        data.dataId = info.GetInt32("dataId");
        data.level = info.GetInt16("level");
        data.amount = info.GetInt16("amount");
        data.durability = info.GetSingle("durability");
        data.exp = info.GetInt32("exp");
        data.lockRemainsDuration = info.GetSingle("lockRemainsDuration");
        obj = data;
        return obj;
    }
}
