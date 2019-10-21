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
        to.CreatorId = from.CreatorId;
        to.CreatorName = from.CreatorName;
        DevExtUtils.InvokeStaticDevExtMethods(ClassType, "CloneTo", from, to);
        return to;
    }
}
