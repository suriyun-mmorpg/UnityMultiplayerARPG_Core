namespace MultiplayerARPG
{
    public struct UIBuffRemovalAmountData
    {
        public BuffRemoval removal;
        public float amount;
        public UIBuffRemovalAmountData(BuffRemoval removal, float amount)
        {
            this.removal = removal;
            this.amount = amount;
        }
    }
}
