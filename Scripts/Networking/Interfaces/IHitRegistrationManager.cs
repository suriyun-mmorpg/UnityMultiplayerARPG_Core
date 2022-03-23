using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IHitRegistrationManager
    {
        void Validate(DamageInfo damageInfo, BaseCharacterEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed);
        void Register(BaseCharacterEntity attacker, HitRegisterMessage message);
        void PrepareToRegister(DamageInfo damageInfo, int randomSeed, BaseCharacterEntity attacker, AimPosition aimPosition, uint hitObjectId, byte hitBoxIndex, Vector3 hitPoint);
    }
}
