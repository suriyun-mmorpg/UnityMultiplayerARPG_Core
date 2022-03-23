using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IHitRegistrationManager
    {
        void PrepareHitRegistration(DamageInfo damageInfo, BaseCharacterEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed);
        void PerformHitRegistration(BaseCharacterEntity attacker, Vector3 origin, Vector3 direction, DamageableEntity target, byte hitBoxIndex, Vector3 hitPoint, int randomSeed);
    }
}
