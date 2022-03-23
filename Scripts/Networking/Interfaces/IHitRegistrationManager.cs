using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IHitRegistrationManager
    {
        void ValidateHit(DamageInfo damageInfo, byte hitIndex, BaseCharacterEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed);
        void RegisterHit(BaseCharacterEntity attacker, HitRegisterMessage message);
    }
}
