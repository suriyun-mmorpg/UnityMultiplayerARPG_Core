namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public bool IsUpdatingStorage { get; set; } = false;

        public bool IsDealing
        {
            get { return Dealing?.DealingState != DealingState.None; }
        }

        public override bool CanDoActions()
        {
            return base.CanDoActions() && Dealing.DealingState == DealingState.None;
        }

        public override bool CanEquipItem()
        {
            if (IsUpdatingStorage)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanUnEquipItem()
        {
            if (IsUpdatingStorage)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanPickUpItem()
        {
            if (IsUpdatingStorage)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanDropItem()
        {
            if (IsUpdatingStorage)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanRepairItem()
        {
            if (IsUpdatingStorage)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanRefineItem()
        {
            if (IsUpdatingStorage)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanEnhanceSocketItem()
        {
            if (IsUpdatingStorage)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanRemoveEnhancerFromItem()
        {
            if (IsUpdatingStorage)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanDismentleItem()
        {
            if (IsUpdatingStorage)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanSellItem()
        {
            if (IsUpdatingStorage)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanMoveItem()
        {
            if (IsUpdatingStorage)
                return false;
            if (IsDealing)
                return false;
            if (!CanDoActions())
                return false;
            return true;
        }

        public override bool CanUseItem()
        {
            if (IsDealing)
                return false;
            return base.CanUseItem();
        }
    }
}
