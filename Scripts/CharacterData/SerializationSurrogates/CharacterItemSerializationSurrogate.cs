using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class CharacterItemSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        CharacterItem data = (CharacterItem)obj;
        info.AddValue("id", data.id);
        info.AddValue("dataId", data.dataId);
        info.AddValue("level", data.level);
        info.AddValue("amount", data.amount);
        info.AddValue("equipSlotIndex", data.equipSlotIndex);
        info.AddValue("durability", data.durability);
        info.AddValue("exp", data.exp);
        info.AddValue("lockRemainsDuration", data.lockRemainsDuration);
        info.AddValue("ammo", data.ammo);
        info.AddValue("sockets", data.sockets);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        CharacterItem data = (CharacterItem)obj;
        data.dataId = info.GetInt32("dataId");
        data.level = info.GetInt16("level");
        data.amount = info.GetInt16("amount");
        data.durability = info.GetSingle("durability");
        data.exp = info.GetInt32("exp");
        data.lockRemainsDuration = info.GetSingle("lockRemainsDuration");
        // TODO: Backward compatible, this will be removed in future version
        try
        {
            data.id = info.GetString("id");
        }
        catch
        {
            data.id = GenericUtils.GetUniqueId();
        }
        try
        {
            data.ammo = info.GetInt16("ammo");
        }
        catch { }
        try
        {
            data.sockets = (List<int>)info.GetValue("sockets", typeof(List<int>));
        }
        catch { }
        try
        {
            data.equipSlotIndex = info.GetByte("equipSlotIndex");
        }
        catch { }
        if (string.IsNullOrEmpty(data.id))
            data.id = GenericUtils.GetUniqueId();
        obj = data;
        return obj;
    }
}
