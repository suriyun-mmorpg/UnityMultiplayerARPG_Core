using UnityEngine;

namespace MultiplayerARPG
{
    public interface IGameEntity
    {
        uint ObjectId { get; }
        GameObject gameObject { get; }
        Transform transform { get; }
        BaseGameEntity Entity { get; }
    }
}
