namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public bool CallCmdPickup(uint objectId)
        {
            if (!CanDoActions())
                return false;
            RPC(CmdPickup, objectId);
            CallAllPlayPickupAnimation();
            return true;
        }

        public bool CallCmdPickupItemFromContainer(uint objectId, int itemsContainerIndex, int amount)
        {
            if (!CanDoActions())
                return false;
            RPC(CmdPickupItemFromContainer, objectId, itemsContainerIndex, amount);
            CallAllPlayPickupAnimation();
            return true;
        }

        public bool CallCmdPickupAllItemsFromContainer(uint objectId)
        {
            if (!CanDoActions())
                return false;
            RPC(CmdPickupAllItemsFromContainer, objectId);
            CallAllPlayPickupAnimation();
            return true;
        }

        public bool CallCmdPickupNearbyItems()
        {
            if (!CanDoActions())
                return false;
            RPC(CmdPickupNearbyItems);
            CallAllPlayPickupAnimation();
            return true;
        }

        public bool CallCmdDropItem(int nonEquipIndex, int amount)
        {
            if (amount <= 0 || !CanDoActions() || nonEquipIndex >= NonEquipItems.Count)
                return false;
            RPC(CmdDropItem, nonEquipIndex, amount);
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

        public bool CallCmdUnSummon(uint objectId)
        {
            RPC(CmdUnSummon, objectId);
            return true;
        }
    }
}
