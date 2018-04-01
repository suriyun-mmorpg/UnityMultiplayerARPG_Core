using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibHighLevel;
using LiteNetLib;

public class ItemDropEntity : RpgNetworkEntity
{
    public CharacterItem dropData;
    public Transform modelContainer;
    public SyncFieldString itemId = new SyncFieldString();

    public Transform CacheModelContainer
    {
        get
        {
            if (modelContainer == null)
                modelContainer = GetComponent<Transform>();
            return modelContainer;
        }
    }

    private void Awake()
    {
        var gameInstance = GameInstance.Singleton;
        gameObject.tag = gameInstance.itemDropTag;
        gameObject.layer = gameInstance.itemDropLayer;
    }

    private void Start()
    {
        if (IsServer)
        {
            var id = dropData.itemId;
            if (!GameInstance.Items.ContainsKey(id))
                NetworkDestroy();
            itemId.Value = id;
        }
    }

    public override void OnSetup()
    {
        base.OnSetup();
        itemId.sendOptions = SendOptions.ReliableOrdered;
        itemId.forOwnerOnly = false;
        itemId.onChange += OnItemIdChange;
    }

    protected void OnItemIdChange(string itemId)
    {
        Item item;
        if (GameInstance.Items.TryGetValue(itemId, out item) && item.dropModel != null)
        {
            var model = Instantiate(item.dropModel, CacheModelContainer);
            model.gameObject.SetLayerRecursively(GameInstance.Singleton.itemDropLayer, true);
            model.gameObject.SetActive(true);
            model.transform.localPosition = Vector3.zero;
        }
    }

    private void OnDestroy()
    {
        itemId.onChange -= OnItemIdChange;
    }
}
