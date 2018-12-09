using UnityEngine;

namespace MultiplayerARPG
{
    public interface IAttackerEntity
    {
        uint ObjectId { get; }
        GameObject gameObject { get; }
        Transform transform { get; }
    }
}
