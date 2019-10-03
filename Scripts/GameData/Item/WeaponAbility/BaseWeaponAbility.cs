using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseWeaponAbility : ScriptableObject
    {
        protected BasePlayerCharacterController controller;
        protected CharacterItem weapon;

        public virtual void Setup(BasePlayerCharacterController controller, CharacterItem weapon)
        {
            this.controller = controller;
            this.weapon = weapon;
        }

        // TODO: May add more abstract functions later
        public abstract void Desetup();
        public abstract bool IsTurnToTargetWhileActivated();
        public abstract void OnPreActivate();
        public abstract WeaponAbilityState UpdateActivation(WeaponAbilityState state, float deltaTime);
        public abstract void OnPreDeactivate();
    }
}
