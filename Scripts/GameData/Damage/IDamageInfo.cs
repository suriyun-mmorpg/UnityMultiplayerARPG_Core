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
            Vector3 aimPosition,
            Vector3 stagger,
            out HashSet<DamageHitObjectInfo> hitObjectIds);
        Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand);
        Transform GetDamageEffectTransform(BaseCharacterEntity attacker, bool isLeftHand);
        float GetDistance();
        float GetFov();
    }
}
