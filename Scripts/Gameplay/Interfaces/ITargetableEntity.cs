using UnityEngine;

namespace MultiplayerARPG
{
    public interface ITargetableEntity
    {
        Transform transform { get; }
        GameObject gameObject { get; }
        bool ShouldSetAsTargetImmediately();
    }
}
