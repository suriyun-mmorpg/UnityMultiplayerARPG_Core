using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class CharacterSummonSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        CharacterSummon data = (CharacterSummon)obj;
        info.AddValue("type", (byte)data.type);
        info.AddValue("dataId", data.dataId);
        info.AddValue("summonRemainsDuration", data.summonRemainsDuration);
        info.AddValue("level", data.Level);
        info.AddValue("exp", data.Exp);
        info.AddValue("currentHp", data.CurrentHp);
        info.AddValue("currentMp", data.CurrentMp);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        CharacterSummon data = (CharacterSummon)obj;
        data.type = (SummonType)info.GetByte("type");
        data.dataId = info.GetInt32("dataId");
        data.summonRemainsDuration = info.GetSingle("summonRemainsDuration");
        data.level = info.GetInt16("level");
        data.exp = info.GetInt32("exp");
        data.currentHp = info.GetInt32("currentHp");
        data.currentMp = info.GetInt32("currentMp");
        obj = data;
        return obj;
    }
}
