using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCustomActiveSkillType : ScriptableObject
    {
        public abstract bool HasCustomAimControls();
        public abstract Vector3? UpdateAimControls(BaseSkill causingSkill, short causingSkillLevel);
        public abstract void LaunchDamageEntity(
            BaseCharacterEntity baseCharacterEntity,
            bool isLeftHand,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            CharacterBuff debuff,
            BaseSkill skill,
            short skillLevel,
            Vector3 aimPosition,
            Vector3 stagger);
    }
}
