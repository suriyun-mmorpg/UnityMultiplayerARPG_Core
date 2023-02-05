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
            return base.CanDoActions() && Dealing.DealingState == DealingState.None && !IsWarping;
        }

        public override bool CanEquipItem()
        {
            if (IsWarping)
                return false;
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
            if (IsWarping)
                return false;
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
            if (IsWarping)
                return false;
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
            if (IsWarping)
                return false;
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
            if (IsWarping)
                return false;
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
            if (IsWarping)
                return false;
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
            if (IsWarping)
                return false;
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
            if (IsWarping)
                return false;
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
            if (IsWarping)
                return false;
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
            if (IsWarping)
                return false;
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
            if (IsWarping)
                return false;
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
            if (IsWarping)
                return false;
            if (IsDealing)
                return false;
            return base.CanUseItem();
        }
    }
}
