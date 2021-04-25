using UnityEngine;

namespace MultiplayerARPG
{
    public interface IDamageableEntity : IGameEntity
    {
        int CurrentHp { get; set; }
        Transform OpponentAimTransform { get; }
        bool IsInSafeArea { get; set; }
        bool CanReceiveDamageFrom(EntityInfo instigator);
    }
}
