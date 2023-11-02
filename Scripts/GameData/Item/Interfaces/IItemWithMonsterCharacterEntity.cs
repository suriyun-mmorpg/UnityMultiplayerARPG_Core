namespace MultiplayerARPG
{
    public interface IItemWithMonsterCharacterEntity : IItem
    {
        /// <summary>
        /// Monster entity for this item
        /// </summary>
        BaseMonsterCharacterEntity MonsterCharacterEntity { get; }
    }
}