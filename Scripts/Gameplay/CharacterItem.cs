using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

[System.Serializable]
public struct CharacterItem
{
    // Use id as primary key
    public string id;
    public string itemId;
    public int level;
    public int amount;
    // TODO: I want to add random item bonus

    public Item Item
    {
        get { return GameInstance.Items.ContainsKey(itemId) ? GameInstance.Items[itemId] : null; }
    }

    public EquipmentItem EquipmentItem
    {
        get { return Item != null ? Item as EquipmentItem : null; }
    }
}

public class SyncListCharacterItem : SyncListStruct<CharacterItem> { }
