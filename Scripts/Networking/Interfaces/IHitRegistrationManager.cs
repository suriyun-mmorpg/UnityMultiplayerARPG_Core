using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IHitRegistrationManager
    {
        bool WillProceedHitRegByClient<T>(T damageEntity, EntityInfo attackerInfo) where T : BaseDamageEntity;
        void PrepareHitRegValidatation(BaseGameEntity attacker, int randomSeed, float[] triggerDurations, byte fireSpread, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel);
        void IncreasePreparedDamageAmounts(BaseGameEntity attacker, int randomSeed, Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts);
        void PrepareHitRegOrigin(BaseGameEntity attacker, int randomSeed, byte triggerIndex, byte spreadIndex, Vector3 position, Vector3 direction);
        void PrepareToRegister(int randomSeed, byte triggerIndex, byte spreadIndex, uint objectId, byte hitBoxIndex, Vector3 hitPoint);
        void SendHitRegToServer();
        void Register(BaseGameEntity attacker, HitRegisterMessage message);
        void ClearData();
    }
}
