namespace MultiplayerARPG
{
    public interface IPatchableData
    {
        string Id { get; }
        int DataId { get; }

        void ClearPatchCaches();
    }
}
