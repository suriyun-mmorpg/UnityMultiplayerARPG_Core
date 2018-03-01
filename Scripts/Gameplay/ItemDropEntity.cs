using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibHighLevel;

public class ItemDropEntity : RpgNetworkEntity
{
    public CharacterItem dropData;
    public SyncFieldString itemId = new SyncFieldString();

    private void Start()
    {
        if (IsServer)
            itemId.Value = dropData.itemId;
    }

    public override void OnSetup()
    {
        base.OnSetup();
        itemId.onChange += OnItemIdChange;
    }

    protected void OnItemIdChange(string itemId)
    {
        // TODO: Instantiate drop model
    }

    private void OnDestroy()
    {
        itemId.onChange -= OnItemIdChange;
    }
}
