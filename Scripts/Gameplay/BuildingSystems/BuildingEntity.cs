using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

public class BuildingEntity : RpgNetworkEntity, IBuildingSaveData
{
    [Header("Save Data")]
    public SyncFieldInt dataId = new SyncFieldInt();
    public SyncFieldString creatorId = new SyncFieldString();
    public SyncFieldString creatorName = new SyncFieldString();
    private BuildingObject buildingObject;

    public int DataId
    {
        get { return dataId; }
        set { dataId.Value = value; }
    }

    public Vector3 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public Quaternion Rotation
    {
        get { return transform.rotation; }
        set { transform.rotation = value; }
    }

    public string CreatorId
    {
        get { return creatorId; }
        set { creatorId.Value = value; }
    }

    public string CreatorName
    {
        get { return creatorName; }
        set { creatorName.Value = value; }
    }

    public override void OnSetup()
    {
        dataId.onChange += OnDataIdChange;
    }

    private void OnDestroy()
    {
        dataId.onChange -= OnDataIdChange;
    }

    private void OnDataIdChange(int dataId)
    {
        // Instantiate object
        BuildingObject buildingObjectPrefab;
        if (GameInstance.BuildingObjects.TryGetValue(dataId, out buildingObjectPrefab))
        {
            if (buildingObject != null)
                Destroy(buildingObject.gameObject);
            buildingObject = Instantiate(buildingObjectPrefab);
            buildingObject.CacheTransform.parent = CacheTransform;
            buildingObject.CacheTransform.localPosition = Vector3.zero;
            buildingObject.CacheTransform.localRotation = Quaternion.identity;
            buildingObject.CacheTransform.localScale = Vector3.one;
        }
    }
}
