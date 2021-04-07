using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IDamageableEntity : IGameEntity
    {
        int CurrentHp { get; set; }
        Transform OpponentAimTransform { get; }
        bool CanReceiveDamageFrom(EntityInfo instigator);
    }
}
