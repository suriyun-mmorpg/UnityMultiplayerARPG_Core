namespace MultiplayerARPG
{
    public partial interface IUsableItem : IItem, ICustomAimController
    {
        /// <summary>
        /// Cooldown duration before it is able to use again
        /// </summary>
        float UseItemCooldown { get; }
        void UseItem(BaseCharacterEntity characterEntity, short itemIndex, CharacterItem characterItem);
    }
}
