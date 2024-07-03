using UnityEngine;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public abstract class BaseCustomDamageInfo : ScriptableObject, IDamageInfo
    {
        public abstract void LaunchDamageEntity(
            BaseCharacterEntity attacker,
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Vector3 fireStagger,
            List<Dictionary<DamageElement, MinMaxFloat>> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            AimPosition aimPosition);
        public abstract Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand);
        public abstract float GetDistance();
        public abstract float GetFov();
        public abstract bool IsHitValid(HitValidateData hitValidateData, HitRegisterData hitData, DamageableHitBox hitBox);
        public virtual bool IsHeadshotInstantDeath()
        {
            return false;
        }

        public virtual void PrepareRelatesData()
        {

        }
    }
}
