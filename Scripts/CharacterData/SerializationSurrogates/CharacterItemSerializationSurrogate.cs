using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class CharacterItemSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        CharacterItem data = (CharacterItem)obj;
        info.AddValue("dataId", data.dataId);
        info.AddValue("level", data.level);
        info.AddValue("amount", data.amount);
        info.AddValue("durability", data.durability);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        CharacterItem data = (CharacterItem)obj;
        // Backward compatible
        var stringId = string.Empty;
        try { stringId = info.GetString("itemId"); }
        catch { }
        if (!string.IsNullOrEmpty(stringId))
            data.dataId = BaseGameData.GenerateHashId(stringId);
        else
            data.dataId = info.GetInt32("dataId");
        data.level = info.GetInt16("level");
        data.amount = info.GetInt16("amount");
        // Backward compatible
        var durability = 0f;
        try { durability = info.GetSingle("durability"); }
        catch { }
        data.durability = durability;
        obj = data;
        return obj;
    }
}
