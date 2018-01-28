using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

[System.Serializable]
public struct CharacterAttributeLevel
{
    // Use attributeId as primary key
    public string attributeId;
    public int amount;
    
    public CharacterAttribute Attribute
    {
        get { return GameInstance.CharacterAttributes.ContainsKey(attributeId) ? GameInstance.CharacterAttributes[attributeId] : null; }
    }
}

public class SyncListCharacterAttributeLevel : SyncListStruct<CharacterAttributeLevel> { }
