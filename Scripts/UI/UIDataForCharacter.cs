namespace MultiplayerARPG
{
    public abstract class UIDataForCharacter<T> : UISelectionEntry<T>
    {
        public ICharacterData Character { get; protected set; }
        public int IndexOfData { get; protected set; }

        public virtual void Setup(T data, ICharacterData character, int indexOfData)
        {
            Character = character;
            IndexOfData = indexOfData;
            Data = data;
        }

        public bool IsOwningCharacter()
        {
            return Character == GameInstance.PlayingCharacter;
        }
    }
}
