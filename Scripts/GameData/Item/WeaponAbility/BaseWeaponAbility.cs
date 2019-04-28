using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseWeaponAbility : ScriptableObject
    {
        protected BasePlayerCharacterController playerCharacterController;

        public virtual void Setup(BasePlayerCharacterController controller)
        {
            playerCharacterController = controller;
        }

        // TODO: May add more abstract functions later
        public abstract void ForceDeactivated();
        public abstract WeaponAbilityState UpdateActivation(WeaponAbilityState state, float deltaTime);
    }
}
