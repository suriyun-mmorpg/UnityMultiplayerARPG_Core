using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class CharacterHotkeySerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        CharacterHotkey data = (CharacterHotkey)obj;
        info.AddValue("hotkeyId", data.hotkeyId);
        info.AddValue("type", (byte)data.type);
        info.AddValue("dataId", data.dataId);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        CharacterHotkey data = (CharacterHotkey)obj;
        data.hotkeyId = info.GetString("hotkeyId");
        data.type = (HotkeyType)info.GetByte("type");
        // Backward compatible
        var stringId = string.Empty;
        try { stringId = info.GetString("dataId"); }
        catch { }
        if (!string.IsNullOrEmpty(stringId))
            data.dataId = BaseGameData.GenerateHashId(stringId);
        else
            data.dataId = info.GetInt32("dataId");
        obj = data;
        return obj;
    }
}
