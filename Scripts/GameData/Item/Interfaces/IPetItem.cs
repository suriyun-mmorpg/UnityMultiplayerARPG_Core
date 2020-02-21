namespace MultiplayerARPG
{
    public partial interface IPetItem : IUsableItem
    {
        BaseMonsterCharacterEntity PetEntity { get; }
    }
}
