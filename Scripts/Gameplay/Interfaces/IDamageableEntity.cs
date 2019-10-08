using System.Collections.Generic;

namespace MultiplayerARPG
{
    public interface IDamageableEntity : IGameEntity
    {
        int CurrentHp { get; set; }
        bool IsDead();
        void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel);
        bool CanReceiveDamageFrom(IAttackerEntity attacker);
        void PlayHitEffects(IEnumerable<DamageElement> damageElements, BaseSkill skill);
    }
}
