namespace MultiplayerARPG
{
    public partial interface IUsableItem : IItem, ICustomAimController
    {
        void UseItem(BaseCharacterEntity characterEntity, short itemIndex, CharacterItem characterItem);
    }
}
