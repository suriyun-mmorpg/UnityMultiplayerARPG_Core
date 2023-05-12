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
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            AimPosition aimPosition,
            DamageOriginPreparedDelegate onOriginPrepared,
            DamageHitDelegate onHit);
        public abstract Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand);
        public abstract float GetDistance();
        public abstract float GetFov();
        public abstract bool IsHitReachedMax(int alreadyHitCount);

        public virtual void PrepareRelatesData()
        {

        }

        public virtual bool ValidatedByHitRegistrationManager()
        {
            return false;
        }
    }
}
