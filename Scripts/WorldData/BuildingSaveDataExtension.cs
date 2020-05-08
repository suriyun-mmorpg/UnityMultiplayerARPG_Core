using LiteNetLib.Utils;

public static partial class BuildingSaveDataExtension
{
    private static System.Type classType;
    public static System.Type ClassType
    {
        get
        {
            if (classType == null)
                classType = typeof(BuildingSaveDataExtension);
            return classType;
        }
    }

    public static T CloneTo<T>(this IBuildingSaveData from, T to) where T : IBuildingSaveData
    {
        to.Id = from.Id;
        to.ParentId = from.ParentId;
        to.DataId = from.DataId;
        to.CurrentHp = from.CurrentHp;
        to.Position = from.Position;
        to.Rotation = from.Rotation;
        to.IsLocked = from.IsLocked;
        to.LockPassword = from.LockPassword;
        to.CreatorId = from.CreatorId;
        to.CreatorName = from.CreatorName;
        to.ExtraData = from.ExtraData;
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "CloneTo", from, to);
        return to;
    }

    public static void SerializeBuildingSaveData<T>(this T buildingSaveData, NetDataWriter writer) where T : IBuildingSaveData
    {
        writer.Put(buildingSaveData.Id);
        writer.Put(buildingSaveData.ParentId);
        writer.Put(buildingSaveData.DataId);
        writer.Put(buildingSaveData.CurrentHp);
        writer.Put(buildingSaveData.IsLocked);
        writer.Put(buildingSaveData.LockPassword);
        writer.PutVector3(buildingSaveData.Position);
        writer.PutQuaternion(buildingSaveData.Rotation);
        writer.Put(buildingSaveData.CreatorId);
        writer.Put(buildingSaveData.CreatorName);
        writer.Put(buildingSaveData.ExtraData);
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "SerializeBuildingSaveData", buildingSaveData, writer);
    }

    public static BuildingSaveData DeserializeBuildingSaveData(NetDataReader reader)
    {
        BuildingSaveData result = new BuildingSaveData();
        result.Id = reader.GetString();
        result.ParentId = reader.GetString();
        result.DataId = reader.GetInt();
        result.CurrentHp = reader.GetInt();
        result.IsLocked = reader.GetBool();
        result.LockPassword = reader.GetString();
        result.Position = reader.GetVector3();
        result.Rotation = reader.GetQuaternion();
        result.CreatorId = reader.GetString();
        result.CreatorName = reader.GetString();
        result.ExtraData = reader.GetString();
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "DeserializeBuildingSaveData", result, reader);
        return result;
    }

    public static T DeserializeBuildingSaveData<T>(this T buildingSaveData, NetDataReader reader) where T : IBuildingSaveData
    {
        DeserializeBuildingSaveData(reader).CloneTo(buildingSaveData);
        return buildingSaveData;
    }
}
