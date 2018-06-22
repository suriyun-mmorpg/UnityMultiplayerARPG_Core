using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class CharacterBuffSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        CharacterBuff data = (CharacterBuff)obj;
        info.AddValue("id", data.id);
        info.AddValue("characterId", data.characterId);
        info.AddValue("type", (byte)data.type);
        info.AddValue("dataId", data.dataId);
        info.AddValue("level", data.level);
        info.AddValue("buffRemainsDuration", data.buffRemainsDuration);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        CharacterBuff data = (CharacterBuff)obj;
        data.id = info.GetString("id");
        data.characterId = info.GetString("characterId");
        data.type = (BuffType)info.GetByte("type");
        // Backward compatible
        var stringId = string.Empty;
        try { stringId = info.GetString("dataId"); }
        catch { }
        if (!string.IsNullOrEmpty(stringId) && !int.TryParse(stringId, out data.dataId))
            data.dataId = stringId.GenerateHashId();
        else
            data.dataId = info.GetInt32("dataId");
        data.level = info.GetInt16("level");
        data.buffRemainsDuration = info.GetSingle("buffRemainsDuration");
        obj = data;
        return obj;
    }
}
