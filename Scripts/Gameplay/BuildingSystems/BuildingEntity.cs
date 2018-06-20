using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingEntity : RpgNetworkEntity, IBuildingSaveData
{
    [Header("Building Data")]
    [Tooltip("Type of building you can set it as Foundation, Wall, Door anything as you wish")]
    public string buildingType;

    [Header("Character Data")]
    public string creatorId;
    public string creatorName;

    public string Id { get { return Identity.AssetId; } }

    public int DataId
    {
        get { return Id.GenerateHashId(); }
        set { }
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
        set { creatorId = value; }
    }

    public string CreatorName
    {
        get { return creatorName; }
        set { creatorName = value; }
    }
}
