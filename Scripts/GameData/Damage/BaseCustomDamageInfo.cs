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
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            int randomSeed,
            AimPosition aimPosition,
            Vector3 stagger,
            out Dictionary<uint, int> hitBoxes);
        public abstract Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand);
        public abstract float GetDistance();
        public abstract float GetFov();

        public virtual void PrepareRelatesData()
        {

        }
    }
}
