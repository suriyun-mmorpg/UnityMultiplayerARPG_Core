using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class CharacterSkillSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        CharacterSkill data = (CharacterSkill)obj;
        info.AddValue("dataId", data.dataId);
        info.AddValue("level", data.level);
        info.AddValue("coolDownRemainsDuration", data.coolDownRemainsDuration);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        CharacterSkill data = (CharacterSkill)obj;
        // Backward compatible
        var stringId = string.Empty;
        try { stringId = info.GetString("skillId"); }
        catch { }
        if (!string.IsNullOrEmpty(stringId))
            data.dataId = BaseGameData.GenerateHashId(stringId);
        else
            data.dataId = info.GetInt32("dataId");
        data.level = info.GetInt32("level");
        data.coolDownRemainsDuration = info.GetSingle("coolDownRemainsDuration");
        obj = data;
        return obj;
    }
}
