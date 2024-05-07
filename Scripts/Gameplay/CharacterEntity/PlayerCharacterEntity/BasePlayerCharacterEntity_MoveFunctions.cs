namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        protected override bool CanMove_Implementation()
        {
            if (Vending.Data.isStarted)
                return false;
            return base.CanMove_Implementation();
        }

        protected override bool CanSprint_Implementation()
        {
            if (Vending.Data.isStarted)
                return false;
            return base.CanSprint_Implementation();
        }

        protected override bool CanWalk_Implementation()
        {
            if (Vending.Data.isStarted)
                return false;
            return base.CanWalk_Implementation();
        }

        protected override bool CanCrouch_Implementation()
        {
            if (Vending.Data.isStarted)
                return false;
            return base.CanCrouch_Implementation();
        }

        protected override bool CanCrawl_Implementation()
        {
            if (Vending.Data.isStarted)
                return false;
            return base.CanCrawl_Implementation();
        }

        protected override bool CanJump_Implementation()
        {
            if (Vending.Data.isStarted)
                return false;
            return base.CanJump_Implementation();
        }

        protected override bool CanDash_Implementation()
        {
            if (Vending.Data.isStarted)
                return false;
            return base.CanDash_Implementation();
        }

        protected override bool CanTurn_Implementation()
        {
            if (Vending.Data.isStarted)
                return false;
            return base.CanTurn_Implementation();
        }
    }
}