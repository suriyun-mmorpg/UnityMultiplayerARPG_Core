using UnityEngine;

namespace MultiplayerARPG
{
    public interface IDamageableEntity : IGameEntity
    {
        bool IsImmune { get; }
        int CurrentHp { get; set; }
        Transform OpponentAimTransform { get; }
        SafeArea SafeArea { get; set; }
        bool IsInSafeArea { get; }
        bool CanReceiveDamageFrom(EntityInfo instigator);
    }
}
