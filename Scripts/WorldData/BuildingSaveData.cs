using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BuildingSaveData : IBuildingSaveData
{
    public string id;
    public string parentId;
    public int dataId;
    public int currentHp;
    public Vector3 position;
    public Quaternion rotation;
    public string creatorId;
    public string creatorName;

    public string Id
    {
        get { return id; }
        set { id = value; }
    }

    public string ParentId
    {
        get { return parentId; }
        set { parentId = value; }
    }

    public int DataId
    {
        get { return dataId; }
        set { dataId = value; }
    }

    public int CurrentHp
    {
        get { return currentHp; }
        set { currentHp = value; }
    }

    public Vector3 Position
    {
        get { return position; }
        set { position = value; }
    }

    public Quaternion Rotation
    {
        get { return rotation; }
        set { rotation = value; }
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
