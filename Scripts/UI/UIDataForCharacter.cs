using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIDataForCharacter<T> : UISelectionEntry<T>
{
    public ICharacterData character { get; protected set; }
    public int indexOfData { get; protected set; }

    public void Setup(T data, ICharacterData character, int indexOfData)
    {
        this.character = character;
        this.indexOfData = indexOfData;
        Data = data;
    }

    public bool IsOwningCharacter()
    {
        return character != null && character is PlayerCharacterEntity && (PlayerCharacterEntity)character == BasePlayerCharacterController.OwningCharacter;
    }
}
