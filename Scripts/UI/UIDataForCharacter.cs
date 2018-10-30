namespace MultiplayerARPG
{
    public abstract class UIDataForCharacter<T> : UISelectionEntry<T>
    {
        public ICharacterData character { get; protected set; }
        public int indexOfData { get; protected set; }

        public virtual void Setup(T data, ICharacterData character, int indexOfData)
        {
            this.character = character;
            this.indexOfData = indexOfData;
            Data = data;
        }

        public bool IsOwningCharacter()
        {
            return character != null && character is BasePlayerCharacterEntity && (BasePlayerCharacterEntity)character == BasePlayerCharacterController.OwningCharacter;
        }
    }
}
