using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IDamageableEntity
    {
        uint ObjectId { get; }
        int CurrentHp { get; set; }
        GameObject gameObject { get; }
        Transform transform { get; }
        bool IsDead();
        void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId);
        bool CanReceiveDamageFrom(IAttackerEntity attacker);
        BaseGameEntity Entity { get; }
    }
}
