using UnityEngine;

namespace MultiplayerARPG
{
    public interface IDamageableEntity : IGameEntity
    {
        bool IsImmune { get; }
        int CurrentHp { get; set; }
        Transform OpponentAimTransform { get; }
        bool IsInSafeArea { get; set; }
        bool CanReceiveDamageFrom(EntityInfo instigator);
    }
}
