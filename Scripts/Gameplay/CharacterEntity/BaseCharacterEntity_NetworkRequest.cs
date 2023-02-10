namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public bool CallServerPickupItem(uint objectId)
        {
            if (!CanDoActions())
                return false;
            RPC(ServerPickupItem, objectId);
            CallAllPlayPickupAnimation();
            return true;
        }

        public bool CallServerPickupItemFromContainer(uint objectId, int itemsContainerIndex, int amount)
        {
            if (!CanDoActions())
                return false;
            RPC(ServerPickupItemFromContainer, objectId, itemsContainerIndex, amount);
            CallAllPlayPickupAnimation();
            return true;
        }

        public bool CallServerPickupAllItemsFromContainer(uint objectId)
        {
            if (!CanDoActions())
                return false;
            RPC(ServerPickupAllItemsFromContainer, objectId);
            CallAllPlayPickupAnimation();
            return true;
        }

        public bool CallServerPickupNearbyItems()
        {
            if (!CanDoActions())
                return false;
            RPC(ServerPickupNearbyItems);
            CallAllPlayPickupAnimation();
            return true;
        }

        public bool CallServerDropItem(int nonEquipIndex, int amount)
        {
            if (amount <= 0 || !CanDoActions() || nonEquipIndex >= NonEquipItems.Count)
                return false;
            RPC(ServerDropItem, nonEquipIndex, amount);
            return true;
        }

        public bool CallAllOnDead()
        {
            RPC(AllOnDead);
            return true;
        }

        public bool CallAllOnRespawn()
        {
            RPC(AllOnRespawn);
            return true;
        }

        public bool CallAllOnLevelUp()
        {
            RPC(AllOnLevelUp);
            return true;
        }

        public bool CallServerUnSummon(uint objectId)
        {
            RPC(ServerUnSummon, objectId);
            return true;
        }
    }
}
