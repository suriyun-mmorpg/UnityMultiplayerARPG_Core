using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IDamageableEntity : IGameEntity
    {
        int CurrentHp { get; set; }
        Transform OpponentAimTransform { get; }
        bool IsDead();
        void ReceiveDamage(IGameEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel);
        bool CanReceiveDamageFrom(IGameEntity attacker);
        void PlayHitEffects(IEnumerable<DamageElement> damageElements, BaseSkill skill);
    }
}
