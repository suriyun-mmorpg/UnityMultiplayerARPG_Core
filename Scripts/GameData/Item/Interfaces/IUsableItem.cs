namespace MultiplayerARPG
{
    public partial interface IUsableItem : IItem
    {
        void UseItem(BaseCharacterEntity characterEntity, short itemIndex, CharacterItem characterItem);
    }
}
