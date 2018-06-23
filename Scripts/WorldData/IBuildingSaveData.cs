using UnityEngine;

public interface IBuildingSaveData
{
    string Id { get; set; }
    string ParentId { get; set; }
    int DataId { get; set; }
    int CurrentHp { get; set; }
    Vector3 Position { get; set; }
    Quaternion Rotation { get; set; }
    string CreatorId { get; set; }
    string CreatorName { get; set; }
}
