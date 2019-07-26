using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class CharacterBuffSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        CharacterBuff data = (CharacterBuff)obj;
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
        data.type = (BuffType)info.GetByte("type");
        data.dataId = info.GetInt32("dataId");
        data.level = info.GetInt16("level");
        data.buffRemainsDuration = info.GetSingle("buffRemainsDuration");
        obj = data;
        return obj;
    }
}
