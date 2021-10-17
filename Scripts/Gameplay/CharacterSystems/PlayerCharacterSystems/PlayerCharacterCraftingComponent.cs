using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class PlayerCharacterCraftingComponent : BaseNetworkedGameEntityComponent<BasePlayerCharacterEntity>, ICraftingQueueSource
    {
        [SerializeField]
        private int maxQueueSize = 5;
        private SyncListCraftingQueueItem queueItems = new SyncListCraftingQueueItem();

        public SyncListCraftingQueueItem QueueItems
        {
            get { return queueItems; }
        }

        public int MaxQueueSize
        {
            get { return maxQueueSize; }
        }

        public bool CanCraft
        {
            get { return !Entity.IsDead(); }
        }

        public float TimeCounter { get; set; }

        public int SourceId { get { return 0; } }

        public override sealed void OnSetup()
        {
            base.OnSetup();
            queueItems.forOwnerOnly = true;
        }

        public override sealed void EntityUpdate()
        {
            base.EntityUpdate();
            if (IsServer)
                this.UpdateQueue(0f, CacheTransform.position);
        }
    }
}
