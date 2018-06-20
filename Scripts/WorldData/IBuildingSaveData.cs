using UnityEngine;

public interface IBuildingSaveData
{
    int DataId { get; set; }
    Vector3 Position { get; set; }
    Quaternion Rotation { get; set; }
    string CreatorId { get; set; }
    string CreatorName { get; set; }
}
