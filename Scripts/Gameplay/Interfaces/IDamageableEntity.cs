using System.Collections.Generic;

namespace MultiplayerARPG
{
    public interface IDamageableEntity : IGameEntity
    {
        int CurrentHp { get; set; }
        bool IsDead();
        void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff);
        bool CanReceiveDamageFrom(IAttackerEntity attacker);
        void PlayHitEffects(IEnumerable<DamageElement> allDamageElements, Skill skill);
    }
}
