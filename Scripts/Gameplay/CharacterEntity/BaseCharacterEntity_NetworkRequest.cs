namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public bool CallCmdPickup(uint objectId)
        {
            if (!CanDoActions())
                return false;
            RPC(CmdPickup, objectId);
            CallRpcPlayPickupAnimation();
            return true;
        }

        public bool CallCmdPickupItemFromContainer(uint objectId, int itemsContainerIndex, int amount)
        {
            if (!CanDoActions())
                return false;
            RPC(CmdPickupItemFromContainer, objectId, itemsContainerIndex, amount);
            CallRpcPlayPickupAnimation();
            return true;
        }

        public bool CallCmdPickupAllItemsFromContainer(uint objectId)
        {
            if (!CanDoActions())
                return false;
            RPC(CmdPickupAllItemsFromContainer, objectId);
            CallRpcPlayPickupAnimation();
            return true;
        }

        public bool CallCmdPickupNearbyItems()
        {
            if (!CanDoActions())
                return false;
            RPC(CmdPickupNearbyItems);
            CallRpcPlayPickupAnimation();
            return true;
        }

        public bool CallCmdDropItem(int nonEquipIndex, int amount)
        {
            if (amount <= 0 || !CanDoActions() || nonEquipIndex >= NonEquipItems.Count)
                return false;
            RPC(CmdDropItem, nonEquipIndex, amount);
            return true;
        }

        public bool CallRpcOnDead()
        {
            RPC(RpcOnDead);
            return true;
        }

        public bool CallRpcOnRespawn()
        {
            RPC(RpcOnRespawn);
            return true;
        }

        public bool CallRpcOnLevelUp()
        {
            RPC(RpcOnLevelUp);
            return true;
        }

        public bool CallCmdUnSummon(uint objectId)
        {
            RPC(CmdUnSummon, objectId);
            return true;
        }
    }
}
