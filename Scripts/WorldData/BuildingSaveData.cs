using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib.Utils;

[System.Serializable]
public partial struct BuildingSaveData : IBuildingSaveData, INetSerializable
{
    public string id;
    public string parentId;
    public int entityId;
    public int currentHp;
    public float remainsLifeTime;
    public bool isLocked;
    public string lockPassword;
    public Vector3 position;
    public Quaternion rotation;
    public string creatorId;
    public string creatorName;
    public string extraData;

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

    public int EntityId
    {
        get { return entityId; }
        set { entityId = value; }
    }

    public int CurrentHp
    {
        get { return currentHp; }
        set { currentHp = value; }
    }

    public float RemainsLifeTime
    {
        get { return remainsLifeTime; }
        set { remainsLifeTime = value; }
    }

    public bool IsLocked
    {
        get { return isLocked; }
        set { isLocked = value; }
    }

    public string LockPassword
    {
        get { return lockPassword; }
        set { lockPassword = value; }
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

    public string ExtraData
    {
        get { return extraData; }
        set { extraData = value; }
    }

    public void Deserialize(NetDataReader reader)
    {
        reader.DeserializeBuildingSaveData(ref this);
    }

    public void Serialize(NetDataWriter writer)
    {
        this.SerializeBuildingSaveData(writer);
    }
}
