using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public partial class PlayerCharacterCraftingComponent : BaseNetworkedGameEntityComponent<BasePlayerCharacterEntity>, ICraftingQueueSource
    {
        [SerializeField]
        private int maxQueueSize = 5;
        public int MaxQueueSize
        {
            get { return maxQueueSize; }
        }

        private SyncListCraftingQueueItem queueItems = new SyncListCraftingQueueItem();

        public SyncListCraftingQueueItem QueueItems
        {
            get { return queueItems; }
        }

        public float CraftingDistance
        {
            get { return 0; }
        }

        public bool CanCraft
        {
            get { return !Entity.IsDead(); }
        }

        public float TimeCounter { get; set; }

        public int SourceId { get { return 0; } }

        public override sealed void OnSetup()
        {
            queueItems.forOwnerOnly = true;
        }

        public override sealed void EntityUpdate()
        {
            if (!IsServer)
                return;
            this.UpdateQueue();
        }
    }
}
