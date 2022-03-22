using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IDamageInfo
    {
        void LaunchDamageEntity(
            BaseCharacterEntity attacker,
            bool isLeftHand,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            short skillLevel,
            int randomSeed,
            AimPosition aimPosition,
            Vector3 stagger,
            out HashSet<DamageHitObjectInfo> damageHitObjectInfos);
        Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand);
        float GetDistance();
        float GetFov();
    }
}
