namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public bool IsUpdatingStorage { get; set; } = false;

        public bool IsDealing
        {
            get { return Dealing?.DealingState != DealingState.None; }
        }

        public override bool CanEquipItem
        {
            get
            {
                if (IsUpdatingStorage)
                    return false;
                if (IsDealing)
                    return false;
                if (!CanDoActions())
                    return false;
                return true;
            }
        }

        public override bool CanUnEquipItem
        {
            get
            {
                if (IsUpdatingStorage)
                    return false;
                if (IsDealing)
                    return false;
                if (!CanDoActions())
                    return false;
                return true;
            }
        }

        public override bool CanPickUpItem
        {
            get
            {
                if (IsUpdatingStorage)
                    return false;
                if (IsDealing)
                    return false;
                if (!CanDoActions())
                    return false;
                return true;
            }
        }

        public override bool CanDropItem
        {
            get
            {
                if (IsUpdatingStorage)
                    return false;
                if (IsDealing)
                    return false;
                if (!CanDoActions())
                    return false;
                return true;
            }
        }

        public override bool CanRepairItem
        {
            get
            {
                if (IsUpdatingStorage)
                    return false;
                if (IsDealing)
                    return false;
                if (!CanDoActions())
                    return false;
                return true;
            }
        }

        public override bool CanRefineItem
        {
            get
            {
                if (IsUpdatingStorage)
                    return false;
                if (IsDealing)
                    return false;
                if (!CanDoActions())
                    return false;
                return true;
            }
        }

        public override bool CanEnhanceSocketItem
        {
            get
            {
                if (IsUpdatingStorage)
                    return false;
                if (IsDealing)
                    return false;
                if (!CanDoActions())
                    return false;
                return true;
            }
        }

        public override bool CanRemoveEnhancerFromItem
        {
            get
            {
                if (IsUpdatingStorage)
                    return false;
                if (IsDealing)
                    return false;
                if (!CanDoActions())
                    return false;
                return true;
            }
        }

        public override bool CanDismentleItem
        {
            get
            {
                if (IsUpdatingStorage)
                    return false;
                if (IsDealing)
                    return false;
                if (!CanDoActions())
                    return false;
                return true;
            }
        }

        public override bool CanSellItem
        {
            get
            {
                if (IsUpdatingStorage)
                    return false;
                if (IsDealing)
                    return false;
                if (!CanDoActions())
                    return false;
                return true;
            }
        }

        public override bool CanMoveItem
        {
            get
            {
                if (IsUpdatingStorage)
                    return false;
                if (IsDealing)
                    return false;
                if (!CanDoActions())
                    return false;
                return true;
            }
        }
    }
}
