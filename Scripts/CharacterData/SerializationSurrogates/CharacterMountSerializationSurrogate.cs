using System.Runtime.Serialization;

namespace MultiplayerARPG
{
    public class CharacterMountSerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(
            object obj,
            SerializationInfo info,
            StreamingContext context)
        {
            CharacterMount data = (CharacterMount)obj;
            info.AddValue("type", (byte)data.type);
            info.AddValue("dataId", data.dataId);
            info.AddValue("level", data.level);
            info.AddValue("exp", data.exp);
        }

        public object SetObjectData(
            object obj,
            SerializationInfo info,
            StreamingContext context,
            ISurrogateSelector selector)
        {
            CharacterMount data = (CharacterMount)obj;
            data.type = (MountType)info.GetByte("type");
            data.dataId = info.GetInt32("dataId");
            data.level = info.GetInt32("level");
            data.exp = info.GetInt32("exp");
            obj = data;
            return obj;
        }
    }
}
