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
            int triggerIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            int randomSeed,
            AimPosition aimPosition,
            Vector3 stagger,
            System.Action<int, Vector3, Vector3, Quaternion> onOriginPrepared,
            System.Action<int, uint, int> onHit);
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
