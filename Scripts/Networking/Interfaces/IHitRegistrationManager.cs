using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IHitRegistrationManager
    {
        void PrepareHitRegValidatation(BaseCharacterEntity attacker, int randomSeed, float[] triggerDurations, byte fireSpread, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel);
        void Register(BaseCharacterEntity attacker, HitRegisterMessage message);
        void PrepareToRegister(DamageInfo damageInfo, int randomSeed, BaseCharacterEntity attacker, Vector3 damagePosition, Vector3 damageDirection, List<HitData> hitDataCollection);
        void SendHitRegToServer();
        int GetApplySeed(int randomSeed, int triggerIndex);
    }
}
