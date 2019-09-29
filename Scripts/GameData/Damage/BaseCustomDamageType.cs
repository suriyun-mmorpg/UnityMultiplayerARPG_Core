using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCustomDamageType : ScriptableObject
    {
        public abstract bool HasCustomAimControls();
        public abstract Vector3? UpdateAimControls(Skill causingSkill, short causingSkillLevel);
        public abstract float GetDistance();
        public abstract float GetFov();
        public abstract void LaunchDamageEntity(
            BaseCharacterEntity baseCharacterEntity,
            bool isLeftHand,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            CharacterBuff debuff,
            Skill skill,
            short skillLevel,
            Vector3 aimPosition,
            Vector3 stagger);
    }
}
