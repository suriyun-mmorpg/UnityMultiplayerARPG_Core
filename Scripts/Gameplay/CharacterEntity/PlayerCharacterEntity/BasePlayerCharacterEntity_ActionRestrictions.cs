namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public bool IsUpdatingItems { get; set; } = false;

        public bool IsDealing
        {
            get { return Dealing.DealingState != DealingState.None; }
        }

        public bool IsVendingStarted
        {
            get { return Vending.Data.isStarted; }
        }

        public override bool CanDoActions()
        {
            return base.CanDoActions() && Dealing.DealingState == DealingState.None && !Vending.Data.isStarted && !IsWarping;
        }

        public override bool CanEquipItem()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanUnEquipItem()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanPickup()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (this.IsDead())
                return false;
            return true;
        }

        public override bool CanDropItem()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (this.IsDead())
                return false;
            return true;
        }

        public override bool CanRepairItem()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanRefineItem()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanEnhanceSocketItem()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanRemoveEnhancerFromItem()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanDismantleItem()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanSellItem()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanMoveItem()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanUseItem()
        {
            if (IsWarping)
                return false;
            if (IsDealing)
                return false;
            if (IsVendingStarted)
                return false;
            return base.CanUseItem();
        }

        public override bool CanChangeAmmoItem()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanRemoveAmmoFromItem()
        {
            if (IsWarping)
                return false;
            if (IsUpdatingItems)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }
    }
}
