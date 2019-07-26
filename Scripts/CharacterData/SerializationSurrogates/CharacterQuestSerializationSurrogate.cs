using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class CharacterQuestSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {
        CharacterQuest data = (CharacterQuest)obj;
        info.AddValue("dataId", data.dataId);
        info.AddValue("isComplete", data.isComplete);
        info.AddValue("killedMonsters", data.killedMonsters);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {
        CharacterQuest data = (CharacterQuest)obj;
        data.dataId = info.GetInt32("dataId");
        data.isComplete = info.GetBoolean("isComplete");
        data.killedMonsters = (Dictionary<int, int>)info.GetValue("killedMonsters", typeof(Dictionary<int, int>));
        obj = data;
        return obj;
    }
}
