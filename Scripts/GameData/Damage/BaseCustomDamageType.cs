using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCustomDamageType : ScriptableObject
    {
        public abstract bool UseCustomControls();
        public abstract void StartCustomControls(BasePlayerCharacterController controller, Skill causingSkill, short skillLevel);
        public abstract void UpdateCustomControls();
        public abstract void StopCustomControls();
        public abstract float GetDistance();
        public abstract float GetFov();
        public abstract void LaunchDamageEntity(
            bool isLeftHand,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            CharacterBuff debuff,
            Skill skill,
            Vector3 aimPosition,
            Vector3 stagger);
    }
}
