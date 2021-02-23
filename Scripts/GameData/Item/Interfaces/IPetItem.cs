namespace MultiplayerARPG
{
    public partial interface IPetItem : IUsableItem
    {
        /// <summary>
        /// Which monster character entity which will be spawned when use this item (it can be level-up and respawn after dead)
        /// </summary>
        BaseMonsterCharacterEntity PetEntity { get; }
    }
}
