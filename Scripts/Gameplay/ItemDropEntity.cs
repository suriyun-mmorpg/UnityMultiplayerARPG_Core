using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

public class ItemDropEntity : RpgNetworkEntity
{
    public CharacterItem dropData;
    public Transform modelContainer;
    public SyncFieldInt itemDataId = new SyncFieldInt();
    public Item Item
    {
        get
        {
            Item item;
            if (GameInstance.Items.TryGetValue(itemDataId, out item))
                return item;
            return null;
        }
    }
    public override string Title
    {
        get
        {
            var item = Item;
            return item == null ? "Unknow" : item.title;
        }
    }

    public Transform CacheModelContainer
    {
        get
        {
            if (modelContainer == null)
                modelContainer = GetComponent<Transform>();
            return modelContainer;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        var gameInstance = GameInstance.Singleton;
        gameObject.tag = gameInstance.itemDropTag;
        gameObject.layer = gameInstance.itemDropLayer;
    }

    protected override void Start()
    {
        base.Start();
        if (IsServer)
        {
            var id = dropData.dataId;
            if (!GameInstance.Items.ContainsKey(id))
                NetworkDestroy();
            itemDataId.Value = id;
            NetworkDestroy(GameInstance.Singleton.itemAppearDuration);
        }
    }

    public override void OnSetup()
    {
        base.OnSetup();
        itemDataId.sendOptions = SendOptions.ReliableOrdered;
        itemDataId.forOwnerOnly = false;
        itemDataId.onChange += OnItemDataIdChange;
    }

    protected void OnItemDataIdChange(int itemDataId)
    {
        var gameInstance = GameInstance.Singleton;
        Item item;
        if (GameInstance.Items.TryGetValue(itemDataId, out item) && item.dropModel != null)
        {
            var model = Instantiate(item.dropModel, CacheModelContainer);
            model.gameObject.SetLayerRecursively(GameInstance.Singleton.itemDropLayer, true);
            model.gameObject.SetActive(true);
            model.gameObject.layer = gameInstance.itemDropLayer;
            model.RemoveComponentsInChildren<Collider>(false);
            model.transform.localPosition = Vector3.zero;
        }
    }

    private void OnDestroy()
    {
        itemDataId.onChange -= OnItemDataIdChange;
    }

    public static ItemDropEntity DropItem(RpgNetworkEntity dropper, int itemDataId, short level, short amount)
    {
        var gameInstance = GameInstance.Singleton;
        var dropPosition = dropper.CacheTransform.position + new Vector3(Random.Range(-1f, 1f) * gameInstance.dropDistance, 0, Random.Range(-1f, 1f) * gameInstance.dropDistance);
        // Raycast to find hit floor
        Vector3? aboveHitPoint = null;
        Vector3? underHitPoint = null;
        var raycastLayerMask = ~(gameInstance.characterLayer.Mask | gameInstance.itemDropLayer.Mask);
        RaycastHit tempHit;
        if (Physics.Raycast(dropPosition, Vector3.up, out tempHit, 100f, raycastLayerMask))
            aboveHitPoint = tempHit.point;
        if (Physics.Raycast(dropPosition, Vector3.down, out tempHit, 100f, raycastLayerMask))
            underHitPoint = tempHit.point;
        // Set drop position to nearest hit point
        if (aboveHitPoint.HasValue && underHitPoint.HasValue)
        {
            if (Vector3.Distance(dropPosition, aboveHitPoint.Value) < Vector3.Distance(dropPosition, underHitPoint.Value))
                dropPosition = aboveHitPoint.Value;
            else
                dropPosition = underHitPoint.Value;
        }
        else if (aboveHitPoint.HasValue)
            dropPosition = aboveHitPoint.Value;
        else if (underHitPoint.HasValue)
            dropPosition = underHitPoint.Value;
        // Random rotation
        var dropRotation = Vector3.up * Random.Range(0, 360);
        var identity = dropper.Manager.Assets.NetworkSpawn(gameInstance.itemDropEntityPrefab.gameObject, dropPosition, Quaternion.Euler(dropRotation));
        var itemDropEntity = identity.GetComponent<ItemDropEntity>();
        var dropData = CharacterItem.Create(itemDataId, level, amount);
        itemDropEntity.dropData = dropData;
        return itemDropEntity;
    }
}
