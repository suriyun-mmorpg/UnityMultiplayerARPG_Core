using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class AreaSkillControls
    {
        public static PlayerCharacterController Controller { get { return BasePlayerCharacterController.Singleton as PlayerCharacterController; } }
        public static ShooterPlayerCharacterController ShooterController { get { return BasePlayerCharacterController.Singleton as ShooterPlayerCharacterController; } }
        public static bool IsMobile { get { return InputManager.useMobileInputOnNonMobile && !Application.isMobilePlatform; } }

        public static Vector3? UpdateAimControls(Vector3 aimAxes, BaseSkill skill, short skillLevel, GameObject targetObject)
        {
            if (IsMobile)
                return UpdateAimControls_Mobile(aimAxes, skill, skillLevel, targetObject);
            // PC controls
            return null;
        }

        private static Vector3? UpdateAimControls_Mobile(Vector3 aimAxes, BaseSkill skill, short skillLevel, GameObject targetObject)
        {
            return null;
        }

        public static Vector3? UpdateAimControls_Shooter(Vector3 aimAxes, BaseSkill skill, short skillLevel, GameObject targetObject)
        {
            if (IsMobile)
                return UpdateAimControls_ShooterMobile(aimAxes, skill, skillLevel, targetObject);
            // PC controls
            return null;
        }

        public static Vector3? UpdateAimControls_ShooterMobile(Vector3 aimAxes, BaseSkill skill, short skillLevel, GameObject targetObject)
        {
            Vector3.ClampMagnitude(skill.GetAttackDistance(ShooterController.PlayerCharacterEntity, skillLevel, false))
            return null;
        }
    }
}
