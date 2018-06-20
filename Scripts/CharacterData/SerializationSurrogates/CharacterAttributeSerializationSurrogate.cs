using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class CharacterAttributeSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        CharacterAttribute data = (CharacterAttribute)obj;
        info.AddValue("dataId", data.dataId);
        info.AddValue("amount", data.amount);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        CharacterAttribute data = (CharacterAttribute)obj;
        // Backward compatible
        var stringId = string.Empty;
        try { stringId = info.GetString("attributeId"); }
        catch { }
        if (!string.IsNullOrEmpty(stringId))
            data.dataId = stringId.GenerateHashId();
        else
            data.dataId = info.GetInt32("dataId");
        data.amount = info.GetInt16("amount");
        obj = data;
        return obj;
    }
}
