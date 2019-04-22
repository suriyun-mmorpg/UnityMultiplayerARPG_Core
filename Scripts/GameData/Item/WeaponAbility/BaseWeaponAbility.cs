using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseWeaponAbility : ScriptableObject
    {
        protected ShooterPlayerCharacterController playerCharacterController;
        protected Camera controllerCamera;

        public virtual void Setup(ShooterPlayerCharacterController controller)
        {
            playerCharacterController = controller;
            controllerCamera = controller.CacheGameplayCameraControls.CacheCamera;
        }

        // TODO: May add more abstract functions later
        public abstract void ForceDeactivated();
        public abstract WeaponAbilityState UpdateActivation(WeaponAbilityState state, float deltaTime);
    }
}
