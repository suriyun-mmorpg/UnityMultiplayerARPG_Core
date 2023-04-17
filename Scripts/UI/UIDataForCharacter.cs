namespace MultiplayerARPG
{
    public abstract class UIDataForCharacter<T> : UISelectionEntry<T>
    {
        private ICharacterData _character;
        public ICharacterData Character
        {
            get
            {
                if (_character != null)
                    return _character;
                return GameInstance.PlayingCharacter;
            }
            protected set
            {
                _character = value;
            }
        }
        public int IndexOfData { get; protected set; }

        public virtual void Setup(T data, ICharacterData character, int indexOfData)
        {
            Character = character;
            IndexOfData = indexOfData;
            Data = data;
        }

        public bool IsOwningCharacter()
        {
            return Character != null && GameInstance.PlayingCharacter != null && Character.Id == GameInstance.PlayingCharacter.Id;
        }
    }
}
