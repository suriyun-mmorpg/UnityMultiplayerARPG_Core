namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public virtual bool CanEquipItem
        {
            get { return true; }
        }

        public virtual bool CanUnEquipItem
        {
            get { return true; }
        }

        public virtual bool CanPickUpItem
        {
            get { return true; }
        }

        public virtual bool CanDropItem
        {
            get { return true; }
        }

        public virtual bool CanRepairItem
        {
            get { return true; }
        }

        public virtual bool CanRefineItem
        {
            get { return true; }
        }

        public virtual bool CanEnhanceSocketItem
        {
            get { return true; }
        }

        public virtual bool CanRemoveEnhancerFromItem
        {
            get { return true; }
        }

        public virtual bool CanDismentleItem
        {
            get { return true; }
        }

        public virtual bool CanSellItem
        {
            get { return true; }
        }

        public virtual bool CanMoveItem
        {
            get { return true; }
        }
    }
}
