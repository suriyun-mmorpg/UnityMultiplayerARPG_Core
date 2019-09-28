using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCustomDamageType : ScriptableObject
    {
        public abstract bool UseCustomAimControls();
        public abstract void StartAimControls(BasePlayerCharacterController controller, Skill causingSkill, short skillLevel);
        public abstract void UpdateAimControls();
        public abstract void StopAimControls();
        public abstract float GetDistance();
        public abstract float GetFov();
        public abstract void LaunchDamageEntity(
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
