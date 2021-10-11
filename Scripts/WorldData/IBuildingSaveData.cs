using UnityEngine;

public partial interface IBuildingSaveData
{
    string Id { get; set; }
    string ParentId { get; set; }
    int EntityId { get; set; }
    int CurrentHp { get; set; }
    float RemainsLifeTime { get; set; }
    bool IsLocked { get; set; }
    string LockPassword { get; set; }
    Vector3 Position { get; set; }
    Quaternion Rotation { get; set; }
    string CreatorId { get; set; }
    string CreatorName { get; set; }
    string ExtraData { get; set; }
}
