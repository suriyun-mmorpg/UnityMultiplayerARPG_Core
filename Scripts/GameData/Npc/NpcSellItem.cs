namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct NpcSellItem
    {
        /// <summary>
        /// Selling item
        /// </summary>
        public BaseItem item;
        /// <summary>
        /// Require gold to buy item
        /// </summary>
        public int sellPrice;
    }
}