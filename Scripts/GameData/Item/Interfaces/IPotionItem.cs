namespace MultiplayerARPG
{
    public partial interface IPotionItem : IUsableItem
    {
        Buff Buff { get; }
    }
}
