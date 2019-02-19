using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class CharacterSkillUsageSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        CharacterSkillUsage data = (CharacterSkillUsage)obj;
        info.AddValue("type", (byte)data.type);
        info.AddValue("dataId", data.dataId);
        info.AddValue("coolDownRemainsDuration", data.coolDownRemainsDuration);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        CharacterSkillUsage data = (CharacterSkillUsage)obj;
        data.type = (SkillUsageType)info.GetByte("type");
        data.dataId = info.GetInt32("dataId");
        data.coolDownRemainsDuration = info.GetSingle("coolDownRemainsDuration");
        obj = data;
        return obj;
    }
}
