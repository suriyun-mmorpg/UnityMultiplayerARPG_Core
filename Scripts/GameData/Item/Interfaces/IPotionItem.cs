namespace MultiplayerARPG
{
    public partial interface IPotionItem : IUsableItem
    {
        /// <summary>
        /// Buff which will be applied when use this item
        /// </summary>
        Buff Buff { get; }
    }
}
