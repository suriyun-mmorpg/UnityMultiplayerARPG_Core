using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class CharacterSummonSerialization : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        CharacterSummon data = (CharacterSummon)obj;
        info.AddValue("type", (byte)data.type);
        info.AddValue("dataId", data.dataId);
        info.AddValue("level", data.level);
        info.AddValue("exp", data.exp);
        info.AddValue("currentHp", data.currentHp);
        info.AddValue("currentMp", data.currentMp);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        CharacterSummon data = (CharacterSummon)obj;
        data.type = (SummonType)info.GetByte("type");
        data.dataId = info.GetInt32("dataId");
        data.level = info.GetInt16("level");
        data.exp = info.GetInt32("exp");
        data.currentHp = info.GetInt32("currentHp");
        data.currentMp = info.GetInt32("currentMp");
        obj = data;
        return obj;
    }
}
