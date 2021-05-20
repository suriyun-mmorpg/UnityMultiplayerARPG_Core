using UnityEngine;

namespace MultiplayerARPG
{
    public interface ICraftingQueueSource
    {
        SyncListCraftingQueueItem QueueItems { get; }
        int MaxQueueSize { get; }
        bool CanCraft { get; }
        float TimeCounter { get; set; }
        int SourceId { get; }
        uint ObjectId { get; }
        Vector3 Position { get; }
    }
}
