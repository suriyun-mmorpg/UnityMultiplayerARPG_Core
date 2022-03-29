using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IHitRegistrationManager
    {
        void Validate(DamageInfo damageInfo, int randomSeed, byte fireSpread, BaseCharacterEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel);
        void Register(BaseCharacterEntity attacker, HitRegisterMessage message);
        void PrepareToRegister(DamageInfo damageInfo, int randomSeed, BaseCharacterEntity attacker, AimPosition aimPosition, List<HitData> hitDataCollection);
    }
}
